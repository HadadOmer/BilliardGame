using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineGameEngine : MonoBehaviour
{
    [Header("Objects")]
    public Camera mainCamera;
    public Transform whiteBall;
    public Transform stick;
    public Transform balls;
    public InGameUI ui;
    public Transform ballCamera;

    [Header("Prefabs")]
    public GameObject WhiteBalllPrefab;
    public Material white;

    [Header("BallsIn")]
    public float fullBallsIn;
    public float halfBallsIn;

    [Header("Values")]
    public float shotForceMult;

    [Header("GameState")]
    public bool aiming;
   
    public bool playing;//This is true when the player can play and false when the balls are still moving
    public bool gameOpen;//This state is before any ball was in

    [Header("Turn Values")]
    public int turnNumber;
    public BallType fullOrHalf ;//Whos turn is it,full or half

    public BallType firstBallIn;//The first ball which entered a hole in a turn
    public BallType firstBallTouched;//The first ball the white ball touched in a turn
    public bool isOpenningStrike;//Is the strike is a restricted strike

    public bool freeBall;//Is this turn have a free ball
    public bool blackIn;//Is the black ball entered a hole this turn
    public string blackEnteredHole;//The hole the black ball entered
    public string blackPickedHole;//The hole that selected to enter the black ball

    // Start is called before the first frame update
    public virtual void Start()
    {
        ui=GameObject.Find("Canvas").GetComponent<InGameUI>();
        ballCamera=GameObject.Find("BallCamera").transform;
        //Game state
        aiming=false;
        isOpenningStrike=true;
        playing=true;
        gameOpen=true;
        
        Debug.Log("In");
        //Turn Values
        turnNumber=1;
        fullOrHalf=BallType.none;
        firstBallIn=BallType.none;
        firstBallTouched=BallType.none;
        blackPickedHole="";

        ReferenceGameEngineInBalls();

    }
    //Refrences the this script in all the ball engines
    public void ReferenceGameEngineInBalls()
    {
        whiteBall.GetComponent<BallEngine>().gameEngine=this;
        foreach(Transform ball in balls)
            ball.GetComponent<BallEngine>().gameEngine=this;
    }
    // Update is called once per frame
    public virtual void Update()
    {
        if(playing)
            Playing();      
    }
    public virtual void Playing()
    {
        DisplayRelevantInstruction();
        //Positions the ball when there is a free ball
        if(Input.GetMouseButton(0)&&(freeBall||whiteBall==null))         
            PositionBall();
        //Picks the hole to enter the black when all of the balls are entered
        else if(Input.GetMouseButton(0)&&blackPickedHole==""
              &&((fullOrHalf==BallType.full&&fullBallsIn==7)
               ||(fullOrHalf==BallType.half&&halfBallsIn==7)))            
            PickHole();
        else if(Input.GetMouseButton(0)&&!aiming)
            PositionStick();
        else if(Input.GetMouseButton(0)&&aiming)
        {
            isOpenningStrike=false;
            StartCoroutine(Shoot());
        }

        else if(Input.GetKeyDown(KeyCode.Space)&&stick.position.y>0&&whiteBall !=null)
        {
            aiming=!aiming;
            if(aiming)
                StartCoroutine(Aim());
        }
    }

    public void DisplayRelevantInstruction()
    {
        if((freeBall||whiteBall==null))
            ui.instructions.text="Place the ball";
        else if(blackPickedHole==""
              &&((fullOrHalf==BallType.full&&fullBallsIn==7)
               ||(fullOrHalf==BallType.half&&halfBallsIn==7)))
            ui.instructions.text="Choose a hole";
    }
//Turn functions
#region    
    public virtual void NextTurn()
    {
        //Gives free ball if the white ball was destroyed
        if(whiteBall==null)
            freeBall=true;
        
        if(blackIn)
        {
            TurnBlackIn();
            return;
        }      
        //This moves the turn when the game is still open
        else if(gameOpen)
           TurnGameOpen();
        //This Moves the turn when the type of balls every player need to insert is already determined
        else
            TurnGameNotOpen();
        
        DisplayTurnMessege();
        
        ResetTurnValues();
        
    }
    public virtual void DisplayTurnMessege()
    {
        if(fullOrHalf==BallType.none)
            ui.turn.text="Game still open";
        else
            ui.turn.text=fullOrHalf==BallType.full?"Full turn":"Half Turn"; 
    }
    public void ResetTurnValues()
    {
        firstBallIn=BallType.none;
        firstBallTouched=BallType.none;
        turnNumber++;
        playing=true;
        blackPickedHole="";
        ChangeBallCameraPos(); 
    }
    public void ChangeBallCameraPos()
    {
        if(fullOrHalf==BallType.full)
            ballCamera.localPosition=new Vector3(0,0,0);
        else if(fullOrHalf==BallType.half)
            ballCamera.localPosition=new Vector3(5,0,0);
    }
    public virtual void TurnBlackIn()
    {
            //The game is won all of the player entered all of his balls,didnt enter the white 
            //and he entered the black to the hole he picked
            if(!freeBall&&blackEnteredHole==blackPickedHole&&
            (fullOrHalf==BallType.full&&fullBallsIn==7)||
            (fullOrHalf==BallType.half&&halfBallsIn==7))
            {
                ui.alert.text=fullOrHalf+" won";
            }
            else
            {
                ui.alert.text=fullOrHalf+" lost";
            }
            //Pauses the game at end
            playing=false;
    }
    public void TurnGameOpen()
    {        
           //The game can only only be 'closed' in the second hit
            if(turnNumber>1)
            {
                if(firstBallIn==BallType.full)
                    fullOrHalf=BallType.full;

                if(firstBallIn==BallType.half)
                    fullOrHalf=BallType.half;
                
                gameOpen=fullOrHalf==BallType.none;               
            }

            //Switches the turn if there is a free ball and the full or half is determined in this turn
            if(freeBall&&fullOrHalf!=BallType.none)
                fullOrHalf=fullOrHalf==BallType.full?fullOrHalf=BallType.half:fullOrHalf=BallType.full;

            //Free ball if the white ball didnt touch any other balls than black
            if((firstBallTouched==BallType.none||firstBallTouched==BallType.black)
            &&(!freeBall&&whiteBall.gameObject!=null))
            {
                whiteBall.GetComponent<BallEngine>().DestroyThisObject();
                freeBall=true;
            }
    }
    
    public void TurnGameNotOpen()
    {
        //If a player doesnt hit his own ball type first its a free ball
            if(!freeBall&&firstBallTouched!=fullOrHalf)
            {
                whiteBall.GetComponent<BallEngine>().DestroyThisObject();
                freeBall=true;
            }
            if(firstBallIn!=fullOrHalf||freeBall)
            {
                //Changes the next turn ball type
                fullOrHalf=fullOrHalf==BallType.full?BallType.half:BallType.full;              
            }
    }
    //A turn ends when all the balls are stopped
    public virtual IEnumerator WaitForTurnToEnd()
    {
        yield return new WaitForSeconds(2);
        
        while(!CheckAllBallsStopped())
            yield return null;
        yield return new WaitForSeconds(0.5f);
        NextTurn();            
    }

    public bool CheckAllBallsStopped()
    {
        Rigidbody rigidbody;
        foreach(Transform ball in balls)
        {
            rigidbody=ball.GetComponent<Rigidbody>();
            if(rigidbody!=null&&rigidbody.velocity.magnitude>0.02f)
                return false;
        }
        if(whiteBall!=null&&
        whiteBall.GetComponent<Rigidbody>().velocity.magnitude>0.02f)
            return false;
        return true;
    }   
#endregion 
    
//Turn values functions 
#region  
    //Adds 1 to the fitting ball type counter
    public virtual void AddBallIn(BallType ballType)
    {
        if(ballType==BallType.full)
            fullBallsIn++;
        if(ballType==BallType.half)
            halfBallsIn++;
    }
    public virtual void WhiteBallIn()
    {
        freeBall=true;
    }
    public virtual void BlackBallIn(string holeName)
    {
        blackIn=true;
        blackEnteredHole=holeName;
    }
    public virtual void TurnFirstBallIn(int ballType)
    {
        if(firstBallIn==BallType.none)
            firstBallIn=(BallType)ballType;
    }
    public virtual void TurnFirstBallTouched(int ballType)
    {
        
        if(firstBallTouched==BallType.none)
            firstBallTouched=(BallType)ballType;
    }
#endregion 
   
//Player Actions
#region 
    public virtual void PositionBall()
    {
        Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 clickPoint;
        if(Physics.Raycast(ray,out hit)&&hit.collider.tag=="TableSurface")
        {
            clickPoint=hit.point+new Vector3(0,0.15f,0);
            whiteBall=Instantiate(WhiteBalllPrefab,clickPoint,Quaternion.identity).transform;
            freeBall=false;
            ui.instructions.text="";
        }
    }
    public virtual void PickHole()
    {
        Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit)&&hit.collider.tag=="Hole")
        {
            blackPickedHole=hit.collider.name;
            ui.instructions.text="";
        }
    }
    public virtual void PositionStick()
    {
        
        Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 clickPoint;
       
        //Finds the click point on the table 
        if(Physics.Raycast(ray,out hit)&&(hit.collider.tag=="TableSurface"
         ||hit.collider.tag=="TableSide"||hit.collider.tag=="Hole"))
        {
            clickPoint=new Vector3(hit.point.x,0.15f,hit.point.z);
        }
        //Finishes the function if the click isnt on the table surface
        else
        {
            return;
        }


        //Fires a ray from the mouse click position to the white ball
        Vector3 dir=whiteBall.position-clickPoint;
        dir=new Vector3(dir.x,0,dir.z);
        ray=new Ray(clickPoint,dir);
        Physics.Raycast(ray,out hit,20,1<<8);
        
        //Check if the stick has a clean sight of the white ball
        if(hit.collider!=null &&hit.collider.GetComponent<BallEngine>().ballType==BallType.white)
        {
            Vector3 whiteHitPoint=hit.point;
            //Postions the stick in it's position based on the mouse click
            stick.position=whiteHitPoint;

            //Limits the position of the stick in an openning strike
            if(isOpenningStrike)
            {
                float xPos=stick.position.x;
                xPos=Mathf.Clamp(xPos,-7,whiteBall.position.x);
                stick.position=(new Vector3(xPos,stick.position.y,stick.position.z));
            }

            stick.LookAt(whiteBall);
            float yAngle=stick.eulerAngles.y;

            //Limits the angle if the strike is an openning strike
            if(isOpenningStrike)
                yAngle=Mathf.Clamp(yAngle,1,179);
            stick.eulerAngles=new Vector3(3,yAngle,0);   

            DrawAimRay();
        }     
    }
    public void DrawAimRay()
    {   
        LineRenderer line;
        if(whiteBall.GetComponent<LineRenderer>()==null)
            whiteBall.gameObject.AddComponent<LineRenderer>();
        line=whiteBall.GetComponent<LineRenderer>();

        //Line renderer look
        line.SetWidth(0.05f,0.05f);
        line.material=white;

        //Sets the postion of the line dots
        line.SetPosition(0,whiteBall.transform.position);
        Vector3 dir=(whiteBall.position-stick.position).normalized;
        dir=new Vector3(dir.x,0,dir.z);
        line.SetPosition(1,whiteBall.position+dir*5);
        

    }

    public virtual IEnumerator Aim()
    {
        aiming=true;
        float distance;
        bool back=false;
        while(aiming)
        {
            yield return null;
            stick.Translate(0,0,back?-1*Time.deltaTime:1*Time.deltaTime);
            distance=(whiteBall.position-stick.position).magnitude;

            if(distance<0.15f||distance>2f)
            {      
                back=!back;
                //Makes sure the stick gets in range after the direction switch
                while(distance<0.15f||distance>2f)
                {
                    yield return null;
                    stick.Translate(0,0,back?-1*Time.deltaTime:1*Time.deltaTime);
                    distance=(whiteBall.position-stick.position).magnitude;
                }              
            }
        }       
        
    }

    public virtual IEnumerator Shoot()
    {
        playing=false;
        aiming=false;
        
        float distance;
        Vector3 force,dir;

        //Calculates the force before starting the stick move
        distance=(whiteBall.position-stick.position).magnitude;
        dir=(whiteBall.position-stick.position).normalized;
        force= dir*distance*shotForceMult;
        while(distance>0.15f)
        {
            yield return null;
            stick.Translate(0,0,3*Time.deltaTime);
            distance=(whiteBall.position-stick.position).magnitude;
        }
       
        whiteBall.GetComponent<Rigidbody>().AddForce(force);
         //Resets the stick position
        stick.position=new Vector3(0,50,0);
        //Destroys the white's ball aim ray
        Destroy(whiteBall.GetComponent<LineRenderer>());

        StartCoroutine(WaitForTurnToEnd());
    }
#endregion


}
