using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;

public class MainMenuHandler : MonoBehaviour
{
    public NetworkManager networkManager;
 
    [Header("Objects")]
    public List<Transform> screens;
    public Camera mainCamera;
    public Text alert;
    public Text roomName;

    [Header("Prefabs")]
    public GameObject roomBoxInfoPrefab;

    

   

    // Start is called before the first frame update
    void Start()
    {
        DisableAllScreens();
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        networkManager.ui = this;

        if (networkManager.connectedToMaster)
            EnableScreen("MainScreen");
    }  


    //Sets active false all the screens in the creens list
    public void DisableAllScreens()
    {
        foreach (Transform screen in screens)
            screen.gameObject.SetActive(false);
    }
    //Sets active true the screen with the declared name
    public void EnableScreen(string name)
    {
        
        foreach (Transform screen in screens)
        {
            if (screen.name == name)
                screen.gameObject.SetActive(true);
            else
                screen.gameObject.SetActive(false);
        }    
    }
    public Transform FindScreen(string name)
    {
        foreach (Transform screen in screens)
        {
            if(screen.name==name)
                return screen;
        }    
        return null;
    }
    public void OpenScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    public void OpenScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void RefreshAvailableRooms(List<RoomInfo> availableRooms)
    {
        GameObject temp;
        for (var i = 0; i < availableRooms.Count; i++)
        {
            Transform JoinRoomScreen=FindScreen("JoinRoomScreen");
            temp=Instantiate(roomBoxInfoPrefab,JoinRoomScreen);
            temp.transform.localPosition=new Vector3(0,300-100*i,0);
            
            temp.GetComponent<RoomInfoBoxHandler>().DefineRoomInfo(networkManager,availableRooms[i].Name);
        } 
                   
    }
   

    //Network functions
    #region 
    public void CreateRoom()
    {
        networkManager.CreateRoom();
    }
    public void LeaveRoom()
    {
        networkManager.LeaveRoom();
    }
    #endregion
}

