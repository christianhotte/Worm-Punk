using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstGunScript : PlayerEquipment
{
    public ConfigurableJoint breakJoint;
    private PlayerInput input;

    public bool Ejecting = false;
    [SerializeField, Range(0, 90), Tooltip("Angle at which barrels will rest when breach is open")] private float breakAngle;

    private protected override void Awake()
    {
        input = GetComponentInParent<PlayerInput>();
        base.Awake();
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
    }   
    public IEnumerator WaitandClose()
    {
        yield return new WaitForSeconds(5f);
        Debug.Log("closing");
        CloseBreach();

    }
}
