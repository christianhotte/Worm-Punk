using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstGunScript : PlayerEquipment
{
    public ConfigurableJoint breakJoint;
    private PlayerInput input;
    public GameObject projectile;
    public bool Ejecting = false;
    public int Barrels = 2, shotsLeft,pellets=30;
    public float maxSpreadAngle=10,projectileSpeed=5;
    [SerializeField, Range(0, 90), Tooltip("Angle at which barrels will rest when breach is open")] private float breakAngle;
    public Transform BarreTran;

    private protected override void Awake()
    {
        input = GetComponentInParent<PlayerInput>();
        base.Awake();
        shotsLeft = Barrels;
    }
    // Start is called before the first frame update
    void Start()
    {
        //Eject();
        //StartCoroutine(WaitandClose());
        SoftJointLimit angleCap = new SoftJointLimit();
        angleCap = breakJoint.highAngularXLimit;
        Debug.Log(angleCap.limit);
    }

    // Update is called once per frame
    private protected override void Update()
    {
        base.Update();
    }
    public void Eject()
    {
        Ejecting = true;
        Debug.Log("ejecting");
        breakJoint.angularXMotion = ConfigurableJointMotion.Limited;
        breakJoint.targetRotation = Quaternion.Euler(Vector3.right * breakAngle);
    }
    public void CloseBreach()
    {
        if (!Ejecting) return;
        breakJoint.targetRotation = Quaternion.Euler(Vector3.zero);
        //float num = breakJoint.co
        breakJoint.angularXMotion = ConfigurableJointMotion.Locked; 
        Ejecting = false;
        shotsLeft = Barrels;
    }   

    public IEnumerator WaitandClose()
    {
        yield return new WaitForSeconds(5f);
        Debug.Log("closing");
        CloseBreach();

    }
    public void shootLeft()
    {
        Debug.Log("Tryingshot");
        Fire(true, BarreTran);
    }
    public void shootRight()
    {
        Debug.Log("Tryingshot");
        Fire(false, BarreTran);
    }
    public void Fire(bool left,Transform barrelpos)
    {
        Vector3 SpawnPoint = barrelpos.localEulerAngles;
        List<Projectile> projectiles = new List<Projectile>();
        if (shotsLeft > 0&&left)
        {
            for(int i=0; i < pellets; i++)
            {
                Projectile newProjectile = Instantiate(projectile).GetComponent<Projectile>();
                projectiles.Add(newProjectile);
                Vector3 exitAngles = Random.insideUnitCircle * maxSpreadAngle;
                barrelpos.localEulerAngles = new Vector3(SpawnPoint.x + exitAngles.x, SpawnPoint.y + exitAngles.y, SpawnPoint.z + exitAngles.z);
           

                Vector3 projVel = barrelpos.forward * projectileSpeed;


                newProjectile.transform.position = barrelpos.transform.position;
                //newProjectile.transform.rotation = Quaternion.LookRotation(projVel);
                newProjectile.transform.forward = barrelpos.forward;
            }
            shotsLeft--;
            Debug.Log("LeftShot");
                
        }
        else if (shotsLeft>0 && !left) {
            for (int i = 0; i < pellets; i++)
            {
                Projectile newProjectile = Instantiate(projectile).GetComponent<Projectile>();
                projectiles.Add(newProjectile);
            }
            shotsLeft--;
            Debug.Log("RightShot");
        }
    }
}
