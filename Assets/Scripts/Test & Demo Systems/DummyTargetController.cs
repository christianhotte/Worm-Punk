using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls objects used to test projectile hit detection and effects.
/// </summary>
public class DummyTargetController : MonoBehaviour, IShootable
{
    //Objects & Components:
    private AudioSource audioSource; //Audio source component this object uses to play sounds from

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        if (!TryGetComponent(out audioSource)) audioSource = gameObject.AddComponent<AudioSource>(); //Make sure target has an audio source component
    }

    //FUNCTIONALITY METHODS:
    public void IsHit(Projectile projectile)
    {
        audioSource.PlayOneShot((AudioClip)Resources.Load("Default_Hurt_Sound")); //Play hurt sound
        print("Target hit!");                                                     //Indicate that target was hit
    }
}
