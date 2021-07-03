using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackBall : BallEngine
{
    protected override void OnTriggerEnter(Collider other)
    {
        if(other.tag=="Hole")
        {
            gameEngine.BlackBallIn(other.name);
            DestroyThisObject();
        }
    }
    
}
