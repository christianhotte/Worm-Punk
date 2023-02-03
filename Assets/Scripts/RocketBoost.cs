using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBoost : PlayerEquipment
{
    public Rigidbody player;
    public Transform rocketTip;
    public float rocketPower=20;
    bool rocketin = false;
    // Start is called before the first frame update
    private protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    private protected override void Update()
    {
        if (rocketin)
        {
            player.velocity = (rocketTip.forward * rocketPower);
        }
        base.Update();
    }
    public void RocketStart()
    {
        rocketin = true;
    }
    public void RocketStop()
    {
        rocketin = false;
    }
}
