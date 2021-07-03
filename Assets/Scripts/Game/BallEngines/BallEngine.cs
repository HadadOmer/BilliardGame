using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BallEngine : MonoBehaviour
{
   
    [Header("Components")]
    protected Rigidbody rb;
    public OfflineGameEngine gameEngine;

    public BallType ballType;
    bool needToBeStopped;
    // Start is called before the first frame update
    void Start()
    {     

        rb=GetComponent<Rigidbody>();
        StartCoroutine(FreezeYMovement(2));
        gameEngine=GameObject.Find("GameEngine").GetComponent<OfflineGameEngine>();
    }

    // Update is called once per frame
    void Update()
    {      
        if(rb.velocity.magnitude<0.15f&&needToBeStopped)
        {
            rb.velocity=Vector3.zero;
            rb.angularVelocity=Vector3.zero;
            needToBeStopped=false;
        }
    }
    public virtual void OnCollisionEnter(Collision collisionInfo)
    {
        if(collisionInfo.collider.tag=="TableSide"||collisionInfo.collider.tag=="Ball")
        {
            //Gives the ball minimal time to move after collision with ball or table sides
            StartCoroutine(StopBall(1.5f));
        }
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        //Adds a ball to the balls counter in game engine and destroys it
        if(other.tag=="Hole")
        {
            if(ballType!=BallType.white&&
                ballType!=BallType.black&&
                GetComponent<PhotonView>().IsMine)
            {
                gameEngine.AddBallIn(ballType);
                gameEngine.TurnFirstBallIn((int)ballType);
            }
            DestroyThisObject();
        }
    }
    public virtual void DestroyThisObject()
    {
        //If its an offline game destroys this game object locally
        if(GameObject.Find("GameEngine").GetComponent<OnilineGameEngine>()==null)
        {
            Debug.Log("Offline destroy");
            Destroy(this.gameObject);
        }
            
        //Else the client who controls the object destroys it on the network
        else if(GetComponent<PhotonView>().IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
    IEnumerator StopBall(float delay)
    {
        yield return new WaitForSeconds(delay);
        needToBeStopped=true;
    }  
    
    IEnumerator FreezeYMovement(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.constraints=RigidbodyConstraints.FreezePositionY;
    }


}


public enum BallType
{
    none=0,
    white=1,
    black=2,
    full=3,
    half=4
}
