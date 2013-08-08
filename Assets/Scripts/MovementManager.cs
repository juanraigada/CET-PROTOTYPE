using UnityEngine;
using System.Collections;

public class MovementManager : MonoBehaviour {
	
	
	//Main variable for state change. Since this will affect both simulation and polish, we place it here.
	public static bool isJumping = true;
	
	//LayerMask for raycasts. We will do several raycasts within our polish and collision code and we need a LayerMask to see which layer to ignore for those raycasts (these will be done only against the floor).
	//LayerMasks are best assigned in the editor.
	public LayerMask toIgnore;
	
	//Different Types of horizontal Moves
	public MovementController[] movementTypes;
	public MovementController[] jumpTypes;
	public PolishController[] polishTypes;
	int currentMovementType = 0;
	int currentJumpType = 0;
	int currentPolishType = 0;
	
	//This are hooks for Unity GUI Windows
	Rect moveSimulationWindowRect;
	Rect polishSimulationWindowRect;
	string[] horizontalMovementStrings;
	string[] jumpMovementStrings;
	string[] polishMovementStrings;
	
	// Use this for initialization
	void Start () {
		horizontalMovementStrings = new string[movementTypes.Length];
		for(int i = 0; i < movementTypes.Length; i +=1){
			horizontalMovementStrings[i] = movementTypes[i].controllerName;
		}
		jumpMovementStrings = new string[jumpTypes.Length];
		for(int i = 0; i < jumpTypes.Length; i +=1){
			jumpMovementStrings[i] = jumpTypes[i].controllerName;
		}
		polishMovementStrings = new string[polishTypes.Length];
		for(int i = 0; i < polishTypes.Length; i +=1){
			polishMovementStrings[i] = polishTypes[i].polishName;
		}
		moveSimulationWindowRect = new Rect(0,0,Screen.width/2,controlHeight*5 + movementTypes[currentMovementType].numberOfOptions);
		polishSimulationWindowRect = new Rect(Screen.width/2,0,Screen.width/2,controlHeight*5 + movementTypes[currentMovementType].numberOfOptions);
	}
	
	/**********************************************************************************************************************************
	 * SIMULATION: This part of the code ONLY deals with the simulation aspects of moving. (modifies the ball speed in the horizontal)	
	 * *******************************************************************************************************************************/
	
		/*********************
		 * INPUT:  
		 * ******************/
	//This is an auxiliary input function that translates an input Vector2 to a pair of camera relative, normal-conformed Vector3.
	//For 3D directional input it is always neccesary to have a function like this one.
	public static Vector3[] GetConformedInput(Vector2 input,Vector3 pos){
		//First, get the vector between the camera and the object. We project this in the plane of our normal and normalize it to obtain our Y Vector.
		Vector3 yVector = (pos - Camera.main.transform.position) - Vector3.Project (pos - Camera.main.transform.position,currentCollisionNormal);
		yVector.Normalize ();
		//Then we cross that with our normal to get the right vector.
		Vector3 xVector = Vector3.Cross (currentCollisionNormal,yVector);
		xVector.Normalize ();
		return new Vector3[] {xVector*input.x,yVector*input.y};
	}
	
		/*********************
		 * COLLISIONS: 
		 * ******************/
	//This belongs to the simulation part of the code. since we are moving on slopes, we will use the normal information for our simulation (as opossed to jumping, were we did use it only for polish).
	//This stores the normal at the ground. We use it only for visual representation, but in a more advanced project with horizontal movement, we will need it also for simulation.
	public static Vector3 currentCollisionNormal;
	
	//This is the max slope at which we can move. Over this we will not change the normal.
	//note that this is different than the movemenet related limits, movement speed can be limited by lower slopes, but we need a maximum to avoid the ball sticking to walls shall the standard limits be too low.
	static float maxSlope = 60;
	
	Vector3 GetGroundNormal(Vector3 point){
		//Here we get the normal at the collision.
		//The Collision structure Unity gives the collision functions is useful, but the normal it gives is modified by tghe relative speed -and it's behavior is a black box without access to the code-.
		//Therefore we need an extra raycast to get the true surface normal at the point.
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast (transform.position, point,out hit,5f,toIgnore)) {
			return hit.normal;
		}
		return Vector3.up;
	}
	
	bool isOnGround(){
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast (transform.position,  - currentCollisionNormal,out hit,0.5f,toIgnore)) {
			return true;
		}
		else return false;
	}
	
	//Hooks for Unity's collision functions. 
	//We use two collision functions to avoid unwanted side-effects on edge cases, although most likely behaviotr will be 100% similar without the OnStay callnback.
	void OnCollisionEnter (Collision coll){
		for(int i = 0 ; i < coll.contacts.Length; i+=1){
			Vector3 differentNormal = GetGroundNormal(coll.contacts[i].point);;
			
			if(Vector3.Angle (differentNormal,Vector3.up) <= maxSlope ){  
				currentCollisionNormal = differentNormal;
				isJumping = false;
			}
		}
	}
	
	void OnCollisionStay (Collision coll){
		for(int i = 0 ; i < coll.contacts.Length; i+=1){
			Vector3 differentNormal = GetGroundNormal(coll.contacts[i].point);;
			
			if(Vector3.Angle (differentNormal,Vector3.up) <= maxSlope){  
				currentCollisionNormal = differentNormal;
				isJumping = false;
			}
		}
	}
	
	void OnCollisionExit(){
		currentCollisionNormal = Vector3.up;
	}
	
	//IMPORTANT: We are missing a function to trigger the fall part of a jump when out of collision with the ground. We can use a OnCollisionExit for that, but for this demo, its not necessary. We will implement that in our complete movement prototype.

		/*********************
		 * SIMULATION UPDATE: 
		 * ******************/
	//Logic code (related to the simulation) always goes on Fixedupdate
	void FixedUpdate () {
		if(isJumping){
			currentCollisionNormal = Vector3.up;
		}
		if(!isOnGround()){
			isJumping = true;
		}else{
			isJumping = false;
		}
		
		for(int i = 0; i < movementTypes.Length; i +=1){
			if(i==currentMovementType){
				movementTypes[i].enabled = true;
			}else{
				movementTypes[i].enabled = false;
			}
		}
		for(int z = 0; z < jumpTypes.Length; z +=1){
			if(z==currentJumpType){
				jumpTypes[z].enabled = true;
			}else{
				jumpTypes[z].enabled = false;
			}
		}
		movementTypes[currentMovementType].MovementUpdate();
		jumpTypes[currentJumpType].MovementUpdate();
		
	}
	
	void LateUpdate(){
	}
	
	
		/*********************
		 * POLISH UPDATE: 
		 * ******************/
	//Code that is only visual goes on Update, for:
		//1- fluidity in motion
		//2-Performance in case rendering code is more taxing than logic code (since its unnecesary to do rendering logic if nothing is being draw).
	void Update(){		
		for(int i = 0; i < polishTypes.Length; i +=1){
			polishTypes[i].DoPolish ();
		}
	}
	
	
	
		/*********************
		 * GUI UPDATE UPDATE: 
		 * ******************/
	public static int controlHeight = 20;
	int showJump = 0;
	
	//OnGUI is called every draw frame and then some (GUI events call it). All GUI functions need to go into onGUI.
	void OnGUI(){
		moveSimulationWindowRect.height =controlHeight*3 + controlHeight*movementTypes[currentMovementType].numberOfOptions;
		moveSimulationWindowRect = GUI.Window (2, moveSimulationWindowRect, DoMoveSimulationWindow, "MOVE SIMULATION CONTROLS");
		
		
		polishSimulationWindowRect.height =controlHeight*3 + controlHeight*polishTypes[currentPolishType].numberOfOptions;
		polishSimulationWindowRect = GUI.Window (1, polishSimulationWindowRect, DoPolishSimulationWindow, "POLISH CONTROLS");
	}
	
	void DoMoveSimulationWindow (int windowID ) {
		string[] st = {"MOVEMENT", "JUMP"};
		showJump = 								GUI.Toolbar (new Rect(0,controlHeight,Screen.width/2,controlHeight), showJump, st);
		if(showJump == 0){
			currentMovementType = 				GUI.Toolbar (new Rect(0,controlHeight*2,Screen.width/2,controlHeight), currentMovementType, horizontalMovementStrings);
			movementTypes[currentMovementType].DrawGUIControls(moveSimulationWindowRect);
		}else{
			currentJumpType = 				GUI.Toolbar (new Rect(0,controlHeight*2,Screen.width/2,controlHeight), currentJumpType, jumpMovementStrings);
			jumpTypes[currentJumpType].DrawGUIControls(moveSimulationWindowRect);
			
		}
		GUI.DragWindow ();
	}
	
	void DoPolishSimulationWindow(int windowID){
		currentPolishType = 				GUI.Toolbar (new Rect(0,controlHeight*2,Screen.width/2,controlHeight), currentPolishType, polishMovementStrings);
		polishTypes[currentPolishType].DrawGUIControls(polishSimulationWindowRect);
		GUI.DragWindow ();
	}
}
