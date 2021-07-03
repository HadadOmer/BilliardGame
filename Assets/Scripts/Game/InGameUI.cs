using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class InGameUI : MonoBehaviour
{
    [Header("Objects")]
    public Text turn;
    public Text alert;
    public Text instructions;
    public NetworkManager networkManager;
    public PhotonView PV;

    public void ExitGame()
    {
        PV.RPC("OtherClientExit",RpcTarget.Others);

        networkManager=GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        networkManager.LeaveRoom();        
    }
    [PunRPC]
    public void OtherClientExit()
    {
        //Make other player leave
        networkManager=GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        StartCoroutine(networkManager.ForceExit(2));
    }

    
}
