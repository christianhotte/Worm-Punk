using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    //Objects & Components:
    private Material mat; //Material in explosion mesh renderer

    //Settings:
    [Header("Visual Settings:")]
    [SerializeField, Tooltip("How long the explosion lasts.")]                              private float duration;
    [SerializeField, Tooltip("Largest scale explosion object reaches.")]                    private float maxScale;
    [SerializeField, Tooltip("Curve describing mesh opacity throughout lifetime.")]         private AnimationCurve transparencyCurve;
    [SerializeField, Tooltip("Curve describing progression of scale throughout lifetime.")] private AnimationCurve scaleCurve;
    [Header("Impact Settings:")]
    [SerializeField, Tooltip("Maximum amount of force explosion can launch players with (scales depending on how close player is to explosion center).")] private float maxLaunchForce;
    [SerializeField, Tooltip("How close a player must be to explosion for it to damage them.")]                                                           private float damageRadius;

    //Runtime Variables:
    internal int originPlayerID;                                        //ID number of the player who caused this explosion, inherited from projectile which caused it
    private float timeAlive;                                            //How long the explosion has existed for
    private List<NetworkPlayer> hitPlayers = new List<NetworkPlayer>(); //List of players who have already been caught in this explosion (used to prevent recurring effects)

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        mat = GetComponent<MeshRenderer>().material; //Material which will be getting more and more transparent throughout explosion
    }
    private void Update()
    {
        //Time update:
        timeAlive += Time.deltaTime;                   //Update lifetime
        if (timeAlive > duration) Destroy(gameObject); //Destroy explosion prefab once its time has expired
        float timeInterpolant = timeAlive / duration;  //Get interpolant to apply to progression curves

        //Update color:
        Color newColor = mat.color;                               //Initialize color value to change
        newColor.a = transparencyCurve.Evaluate(timeInterpolant); //Adjust new alpha color based on evaluation of curve
        mat.color = newColor;                                     //Set new material color

        //Update scale:
        float newScale = scaleCurve.Evaluate(timeInterpolant) * maxScale; //Get new scale value based on evaluation of curve
        transform.localScale = Vector3.one * newScale;                    //Apply new scale to transform

        //Check for player hits:
        Collider[] overlapColliders = Physics.OverlapSphere(transform.position, newScale / 2, LayerMask.NameToLayer("Player"), QueryTriggerInteraction.Ignore); //Check for overlapping player colliders
        foreach (Collider collider in overlapColliders) //Iterate through list of colliders hit by explosion
        {
            NetworkPlayer player = collider.GetComponentInParent<NetworkPlayer>(); //Try to get network player from collider
            if (hitPlayers.Contains(player)) continue;                             //Skip if player has already been hit by this explosion
            if (player != null) //Explosion has hit a player
            {
                //Initialization:
                if (player.photonView.ViewID == originPlayerID) continue;                            //Do not allow explosion to damage the player who created it
                float distance = Vector3.Distance(transform.position, collider.transform.position);  //Get distance between epicenter and player
                Vector3 launchForce = (collider.transform.position - transform.position).normalized; //Initialize generated launch force as normalized direction from explosion to player
                launchForce *= (distance / (newScale / 2)) * maxLaunchForce;                         //Modify launch force based on how close player is to epicenter of explosion
                
                //Send signals:
                player.photonView.RPC("RPC_Launch", Photon.Pun.RpcTarget.All, launchForce);                  //Launch player using calculated force
                if (distance <= damageRadius) player.photonView.RPC("RPC_Hit", Photon.Pun.RpcTarget.All, 1); //Damage player if inside radius

                //Cleanup:
                hitPlayers.Add(player); //Add player to make sure it doesn't get hit again
            }
        }
    }
}
