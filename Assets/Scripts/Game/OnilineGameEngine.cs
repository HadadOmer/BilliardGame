using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OnilineGameEngine : OfflineGameEngine
{
    PhotonView PV;

    [Header("Online game state")]
    public bool turn;

    // Start is called before the first frame update
    public override void Start()
    {
        //Checks if the client is connected to the network and if not makes it an offline game
        if(!PhotonNetwork.IsConnected)
        {
            OfflineGameEngine offlineGameEngine= gameObject.AddComponent<OfflineGameEngine>();
            EqualValues(offlineGameEngine,this);
            Destroy(this);
        }
        else
        {
            base.Start();

            turn=PhotonNetwork.IsMasterClient;
            //Only the client who strikes first has the openning strike
            isOpenningStrike=turn;
            if(!turn)
                 stick.position=new Vector3(0,50,0);
            PV=gameObject.GetComponent<PhotonView>();
            ui.alert.text=turn?"Your turn":"Opponnets turn";
        }
       
    }
    //Makes the values between the two objects equal
    public void EqualValues(OfflineGameEngine offline,OnilineGameEngine online)
    {
        //Equals all the presett values
        offline.whiteBall=online.whiteBall;
        offline.stick=online.stick;
        offline.balls=online.balls;
        offline.WhiteBalllPrefab=online.WhiteBalllPrefab;
        offline.white=online.white;
        offline.shotForceMult=75f;
    }

    // Update is called once per frame
    public override void Update()
    {
        if(turn)
        {
            base.Update();
        }
    }
//Turn functions
#region 
    public override void NextTurn()
    {
        //Moves to next turn on all clients
        PV.RPC("ServerNextTurn",RpcTarget.AllBuffered);      
    }
    [PunRPC]
    public void ServerNextTurn()
    {
        //Switches the turn based on the billiard rules
        if(whiteBall==null||freeBall||firstBallIn==BallType.none||
        (fullOrHalf!=BallType.none&&(firstBallIn!=fullOrHalf||firstBallTouched!=fullOrHalf)))
            turn=!turn;       
        
        base.NextTurn();
    }
    public override void DisplayTurnMessege()
    {
        ui.alert.text=turn?"Your turn":"Opponnets turn";       
    }
    public override void TurnBlackIn()
    {
        if(turn)
        {
            //Determines who won
            bool masterWon;
            if(!freeBall&&blackEnteredHole==blackPickedHole&&
                (fullOrHalf==BallType.full&&fullBallsIn==7)||
                (fullOrHalf==BallType.half&&halfBallsIn==7))
            {
                masterWon=PhotonNetwork.IsMasterClient;
            }
            else
            masterWon=!PhotonNetwork.IsMasterClient;
        
        PV.RPC("GameEnded",RpcTarget.All,masterWon);    
        }
    }
    [PunRPC]
    public void GameEnded(bool masterWon)
    {
        //Dispalys the relevant messege for every client
        ui.alert.text=masterWon==PhotonNetwork.IsMasterClient?"You won":"You lost";
    }

#endregion

//Turn values functions
#region 
    public override void AddBallIn(BallType ballType)
    {
        Debug.Log((int)ballType);
        PV.RPC("ServerAddBallIn", RpcTarget.All, (int)ballType);
    }
    [PunRPC]
    public void ServerAddBallIn(int ballType)
    {
        base.AddBallIn((BallType)ballType);
    }
    public override void TurnFirstBallIn(int ballType)
    {
        PV.RPC("ServerTurnFirstBallIn",RpcTarget.All,ballType);        
    }
    [PunRPC]
    public void ServerTurnFirstBallIn(int ballType)
    {
        base.TurnFirstBallIn(ballType);
    }
    public override void TurnFirstBallTouched(int ballType)
    {
        PV.RPC("ServerTurnFirstBallTouched",RpcTarget.All,ballType);
    }
    [PunRPC]
    public void ServerTurnFirstBallTouched(int ballType)
    {
        base.TurnFirstBallTouched(ballType);
    }

    public override void WhiteBallIn()
    {
        PV.RPC("ServerWhiteBallIn",RpcTarget.All);
    }
    [PunRPC]
    public void ServerWhiteBallIn()
    {
        base.WhiteBallIn();
    }
    public override void BlackBallIn(string holeName)
    {
        PV.RPC("ServerBlackBallIn",RpcTarget.All,holeName);
    }
    [PunRPC]
    public void ServerBlackBallIn(string holeName)
    {
        base.BlackBallIn(holeName);
    }
#endregion
    
//Player Actions
#region 
    public override void PositionBall()
    {    
        //Makes sure there is only one ball   
        if(whiteBall!=null)
            PhotonNetwork.Destroy(whiteBall.GetComponent<PhotonView>());
        float x=Input.mousePosition.x,y=Input.mousePosition.y,z=Input.mousePosition.z;
        Ray ray=Camera.main.ScreenPointToRay(new Vector3(x,y,z));
        RaycastHit hit;
        Vector3 clickPoint;
        if(Physics.Raycast(ray,out hit)&&hit.collider.tag=="TableSurface")
        {
            clickPoint=hit.point+new Vector3(0,0.15f,0);
            whiteBall=PhotonNetwork.Instantiate("Prefabs/Balls/WhiteBall",clickPoint,Quaternion.identity).transform;
            freeBall=false;
            PV.RPC("ServerWhiteBallPlaced",RpcTarget.Others,whiteBall.name);
            ui.instructions.text="";
        }
           
    }
    [PunRPC]
    public void ServerWhiteBallPlaced(string whiteBallName)
    {
        whiteBall=GameObject.Find(whiteBallName).transform;
        freeBall=false;
    }
    
    public override void PickHole()
    {
        base.PickHole();
        PV.RPC("ServerPickHole",RpcTarget.Others,blackPickedHole);
    }
    [PunRPC]
    public void ServerPickHole(string holeName)
    {
        blackPickedHole=holeName;
    }
#endregion
}
