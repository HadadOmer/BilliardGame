using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WhiteBalll : BallEngine
{  
    public PhotonView pv;
    protected override void OnTriggerEnter(Collider other)
    {
       //resets the white ball 
       if(other.tag=="Hole")
       {
           Debug.Log("Reset White");
           base.DestroyThisObject();
       }
    }   
    public override void OnCollisionEnter(Collision collisionInfo)
    {
        base.OnCollisionEnter(collisionInfo);
        if(collisionInfo.collider.tag=="Ball")
        {
            BallType ballType=collisionInfo.collider.GetComponent<BallEngine>().ballType;
            gameEngine.TurnFirstBallTouched((int)ballType);
        }
    }
}
