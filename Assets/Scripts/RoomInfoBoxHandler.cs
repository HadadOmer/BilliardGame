using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoBoxHandler : MonoBehaviour
{
    [Header("Objects")]
    public Text roomNameText;
    public NetworkManager networkManager;
    [Header("Values")]
    public string roomName;
   

    public void DefineRoomInfo(NetworkManager networkManager,string roomName)
    {
        //Intializes the room box values
        this.roomName=roomName;
        this.networkManager=networkManager;
        roomNameText.text=$"Room Name:{this.roomName}";
    }

    public void JoinRoomClick()
    {
        //Joins the room if the network manager exists
        if(networkManager!=null)
            networkManager.JoinRoom(roomName);
        else
            Debug.LogError("Network manager isn't referenced");
    }

}
