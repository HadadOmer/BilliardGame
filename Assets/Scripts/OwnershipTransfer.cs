using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OwnershipTransfer : MonoBehaviour
{
    PhotonView PV;
    OnilineGameEngine gameEngine;
    // Start is called before the first frame update
    void Start()
    {
        PV=GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gameEngine==null)
        {
            gameEngine=GameObject.Find("GameEngine").GetComponent<OnilineGameEngine>();
            if(gameEngine==null)
            {
                Destroy(this);
                return;
            }
        }
    
        if(!PV.IsMine&&gameEngine.turn)
            PV.RequestOwnership();
    }
}
