using UnityEngine;
using System.Collections;

/**************************************************************************************************************************************
 * JUMP CLASS - Normally you would want all movemenet behavior into a single compact class. 
 * However, the fact that this is a modular control exercise (with a lot of extra code for different jump types),
 * makes it more practical to divide every control (jump/movement...) into it's own class for readability.
 * ***********************************************************************************************************************************/

public class PlayerJumpController : MonoBehaviour {
	
	
	//The only variable we really need in every case is a state control variable. Are we in the air jumping?
	public bool isJumping = true;
	
	//These two functions are here for coherence and readability. We will deactivate our jumping state within our collision code, and we want that to call functions in the right sections of the code.
	//In a real application these wouldn't be used.
	void DeactivateJumping(){
		isJumping = false;
	}
	
	void ActivateJumping(){
		isJumping = true;
	}
	
	//This are hooks for Unity GUI Windows
	Rect jumpsimulationWindowRect;
	Rect jumpPolishWindowRect;

	public bool showWindows = true;
	
	/**************************************************************************************************************************************
	 * MAIN LOOP - It is good practise to always keep it clean and simple. 
	 * ***********************************************************************************************************************************/
	
	// Use this for initialization
	void Start () {
		//We just need to initialize the windows
		float controlHeight = 20;
		jumpsimulationWindowRect = new Rect(0,0,Screen.width/2,controlHeight*11);
		jumpPolishWindowRect = new Rect(0,Screen.height/4,Screen.width/2,controlHeight*23);
	}
	
	// Logic code always goes into FixedUpdate
	void FixedUpdate () {		
		CheckJumpSimulation();
	}
	
	//Visual code always goes into Update or LateUpdate
	void Update(){
		CheckJumpPolish();
	}
	
	//OnGUI is called every draw frame and then some (GUI events call it). All GUI functions need to go into onGUI.
	void OnGUI(){
		if(showWindows){
			//We just update the windows Rects.
			jumpsimulationWindowRect = GUI.Window (0, jumpsimulationWindowRect, DoJumpSimulationWindow, "JUMP SIMULATION AND CONTROLS");
			jumpPolishWindowRect = GUI.Window (1, jumpPolishWindowRect, DoJumpPolishWindow, "JUMP POLISH");
		}
	}
	
	
	/**********************************************************************************************************************************
	 * SIMULATION: This part of the code ONLY deals with the simulation aspects of jumping. (modifies the ball speed in the vertical)	
	 * and adds special simulation behavior like bouncing.
	 * *******************************************************************************************************************************/
	
	//These variables set wether to use a curve based velocity or an acceleration based one:
	bool useCurveJump = true;
	//This selects wether to allow jumping to stop early (allowing for tapping to hop and holding to jump):
	bool canTap = true;
		//Flag Toggle to avoid capping the speed more than once (for optimization, not necessary):
	bool isHolding = false;
	
	
	
	//This function checks for jump input and, if jumping, applies the movement logic.
	void CheckJumpSimulation(){
		if(useCurveJump){
			CheckCurveJump();
		}else{
			CheckAccelerationJump();
		}
	}
	
	
		/*********************
		 * COLLISIONS: 
		 * ******************/
	//This belongs to the simulation part of the code. Although at this point we are letting unity handle all the logical behavior but for switching off the isJumping state (which we do when we collide with the gorund).
	//This stores the normal at the ground. We use it only for visual representation, but in a more advanced project with horizontal movement, we will need it also for simulation.
	Vector3 currentCollisionNormal;
	
	//This is the max slope at which we get out of the jump. For values higher than this, we should slide down (keep falling)
	float maxSlope = 60;
	
	void GetGroundNormal(Vector3 point){
		//Here we get the normal at the collision.
		//The Collision structure Unity gives the collision functions is useful, but the normal it gives is modified by tghe relative speed -and it's behavior is a black box without access to the code-.
		//Therefore we need an extra raycast to get the true surface normal at the point.
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast (transform.position, point,out hit,5,toIgnore)) {
			currentCollisionNormal = hit.normal;
		}
	}
	
	//Hooks for Unity's collision functions. 
	//We use two collision functions to avoid unwanted side-effects on edge cases, although most likely behaviotr will be 100% similar without the OnStay callnback.
	void OnCollisionEnter (Collision coll){
		//This is the only part of the code that is not yet used for control and simulation (it would be if apart from jumping we would have horizontal displacement).
		//It is used in the polish section of the code, though.
		GetGroundNormal(coll.contacts[0].point);
		if(Vector3.Angle (currentCollisionNormal,Vector3.up) < maxSlope){
			//Note that we are not checking for collision direction. In a real project with more complex level geometry we would have to.
			DeactivateJumping();
		}else{
			currentCollisionNormal = Vector3.up;
		}
	}
	
	void OnCollisionStay (Collision coll){
		//This is the only part of the code that is not yet used for control and simulation (it would be if apart from jumping we would have horizontal displacement).
		//It is used in the polish section of the code, though.
		GetGroundNormal(coll.contacts[0].point);
		if(Vector3.Angle (currentCollisionNormal,Vector3.up) < maxSlope){
			//Note that we are not checking for collision direction. In a real project with more complex level geometry we would have to.
			DeactivateJumping();
		}else{
			currentCollisionNormal = Vector3.up;
		}
	}
	void OnCollisionExit(){
		currentCollisionNormal = Vector3.up;
	}
	
	
		/*********************
		 * CURVE BASED JUMP: 
		 * ******************/
	//These variables hold the speed curve for the jump, the duration of the curve, it's magnitude (what speed the curve maps to in the vertical) at the normalized point in time we cutoff to (since we map speed to time, we dont need a speed value but a time value).
	public AnimationCurve jumpSpeedCurve;
	float jumpCurveLength = 1.5f;
	float jumpCurveMagnitude = 30f;
	float jumpCurveCapPoint = 0.3f;
		//This is an auxiliary variable to hold the current time into the jump. A generic timer class would do, but this is more readable.
	float currentJumpCurveTime;
		//Normalized point at which the curve goes from up to down. This means the Apex of the curve in Unity's editor should rest at 0.5 in the X axis.
		//Alternatively we could use two curves to avoid mistakes, but making reading the curve less intuitive. 
	float jumpCurveHighPoint = 0.5f;
	
	void CheckCurveJump(){
		//First part of the check is to see if we SHOULD be jumping (basically looking for input in we are alreadynot jumping)
		if(!isJumping && InputManager.inputs[(int)InputManager.inputTypes.JUMP].isActive){
			//We initialize our flags and variables.
			isJumping = true;
			isHolding = true;
			//We initialize the curve
			currentJumpCurveTime = 0;
		}
		//And then, if we are jumping, update the physics.
		if(isJumping){
			//Update the time to see position on the curve
			currentJumpCurveTime += Time.deltaTime;
			currentJumpCurveTime = Mathf.Min (currentJumpCurveTime,jumpCurveLength);
			//Capping it if needed.
			float currentPoint = currentJumpCurveTime/jumpCurveLength;
			if(canTap&&isHolding&&!InputManager.inputs[(int)InputManager.inputTypes.JUMP].isActive){
				isHolding = false;
				if(currentPoint < jumpCurveCapPoint){
					currentPoint = jumpCurveCapPoint;
					currentJumpCurveTime = currentPoint*jumpCurveLength;
				}
			}
			
			float verticalSpeed = 1-jumpSpeedCurve.Evaluate(currentPoint);
			//Make the speed negative or positive depending on wether we are in the up or down part of the curve.
			if(currentJumpCurveTime > jumpCurveLength*jumpCurveHighPoint){
				verticalSpeed = -verticalSpeed;
			}
			rigidbody.velocity = new Vector3(rigidbody.velocity.x,verticalSpeed*jumpCurveMagnitude,rigidbody.velocity.z);
		}
	}
	
		/****************************
		 * ACCELERATION BASED JUMP:	
		 * *************************/
	//These variables controls the vertical jump negative acceleration, initial speed and speed cutoff (the speed we cap at at input release):
	float jumpAccelerationGravity = 25;
	float jumpAccelerationSpeed = 15;
	float jumpAccelerationSpeedCutoff = 7.5f;
	
	void CheckAccelerationJump(){
		//First part of the check is to see if we SHOULD be jumping (basically looking for input in we are alreadynot jumping)
		if(!isJumping && InputManager.inputs[(int)InputManager.inputTypes.JUMP].isActive){	
			//We initialize our flags and variables.
			isJumping = true;
			isHolding = true;
			//Set initial speed
			rigidbody.velocity = rigidbody.velocity + transform.up*jumpAccelerationSpeed;
		}
		//And then, if we are jumping, update the physics.
		if(isJumping){
			if(canTap&&isHolding&&!InputManager.inputs[(int)InputManager.inputTypes.JUMP].isActive){
				isHolding = false;
				//Check upwards velocity
				Vector3 vel = Vector3.Project(rigidbody.velocity,transform.up);
				if(Vector3.Dot (vel,transform.up) > 0 && vel.magnitude > jumpAccelerationSpeedCutoff){
					rigidbody.velocity = transform.up*jumpAccelerationSpeedCutoff;
				}
			}
			rigidbody.velocity = rigidbody.velocity - transform.up*jumpAccelerationGravity*Time.deltaTime;
		}
		
	}
	
	/**********************************************************************************************************************************
	 * POLISH: This part of the code ONLY deals with the visual representation of the ball as it jumps. 
	 * The behavior will always be the same no matter what we do here.
	 * *******************************************************************************************************************************/
	
	//This stores the visual ball (as oppossed to its physical representation in the collider). Because we want to dettach them, it's better to keep them separate.
	//I'm putting this under the Polish code since it's unrelated to how the ball behaves.
	public Transform ball;
	
	//LayerMask for raycasts. We will do several raycasts within our polish code and we need a LayerMask to see which layer to ignore for those raycasts (these will be done only against the floor).
	//LayerMasks are best assigned in the editor.
	public LayerMask toIgnore;
	
	void CheckJumpPolish(){
		DoCameraZoom();
		DoCameraShake();
		PlayJumpParticles();
		PlayGroundHitParticles();
		PlayJumpSound();
		PlayLandSound();
	}
	
	
	
	
	
		/****************************
		 * CAMERA ZOOM:	
		 * *************************/
	//Will we zoom out when the ball is jumping?
	bool zoomOutInJump = true;
	//Ranges of the camera field of view we will interpolate between.
	float zoomOutDefaultRange = 40;
	float zoomRangeChange = 5;
	//Time it takes to perform the interpolation
	float zoomOuttime = 0.15f;
	//Counter of current time into the interpolation.
	float currentzoomOutTime = 0f;
	
	void DoCameraZoom(){
		if(zoomOutInJump){
			if(isJumping){
				currentzoomOutTime += Time.deltaTime;
			}else{
				currentzoomOutTime -= Time.deltaTime;
			}
			currentzoomOutTime = Mathf.Max (currentzoomOutTime,0);
			currentzoomOutTime = Mathf.Min (currentzoomOutTime,zoomOuttime);
		}else{
			currentzoomOutTime = 0;
		}
		float ammountOfZoom = zoomRangeChange*Mathf.Sin (Mathf.PI/2*currentzoomOutTime/zoomOuttime);
		Camera.main.fieldOfView = zoomOutDefaultRange + ammountOfZoom;
	}
	
		/****************************
		 * PARTICLES:	
		 * *************************/
	//Ground hit particles. To create and assign in the editor.
	public ParticleSystem groundHitparticles;
	public ParticleSystem jumpParticles;
	//These variables tell the ball wheter to play particles or not.
	bool polishJumpWithParticle = true;
	bool polishLandWithParticle = true;
	//Auxiliary flag to detect a collision without having to go into the collision functions, to keep this part of the code self-reliable.
	//Note tghe order we call these functions in our main Polish function so that this variable behaves consistently.
	bool wasJumpingForParticles = false;
	
	//Play the particles when colliding with the ground. We could call this from collision detection as you probably would want in a real project. However, we use flag for code independency.
	void PlayGroundHitParticles(){
		if(polishLandWithParticle){
			if(!isJumping && wasJumpingForParticles){
				groundHitparticles.transform.position = transform.position - currentCollisionNormal/2;
				groundHitparticles.transform.rotation = Quaternion.LookRotation (currentCollisionNormal,transform.forward);
				groundHitparticles.Play ();
			}
		}
		if(!isJumping){
			wasJumpingForParticles = false;
		}else{
			wasJumpingForParticles = true;
		}
	}
	
	//Play the jump particles. We should call this funciton at input detection, but, as for the previous one, we are using flags so we can maintain independency. Don't do this in a real project.
	void PlayJumpParticles(){
		if(polishJumpWithParticle){
			if(isJumping&&!wasJumpingForParticles){
				jumpParticles.transform.position = transform.position - currentCollisionNormal/2;
				jumpParticles.transform.rotation = ball.rotation;
				jumpParticles.Play ();
			}
		}
	}
	
		/****************************
		 * SOUND:	
		 * *************************/
	//Play sound on jump?
	bool playJumpSound = true;
	
	// play Sound on Land?
	bool playLandSound = true;
	//Auxiliary flag to detect a collision without having to go into the collision functions, to keep this part of the code self-reliable.
	//Note tghe order we call these functions in our main Polish function so that this variable behaves consistently.
	bool wasJumpingForSound = false;
	
	//AudioSurce to play the saounds with. To assign in editor.
	public AudioSource audioSource;
	//Clips to play
	public AudioClip jumpSound;
	public AudioClip landSound;
	public float jumpSoundVolume;
	public float landSoundVolume;
	
	void PlayJumpSound(){
		if(playJumpSound){
			if(isJumping&&!wasJumpingForSound){
				audioSource.clip = jumpSound;
				audioSource.volume = jumpSoundVolume;
				audioSource.Play ();
			}
		}
	}
	
	void PlayLandSound(){
		if(playLandSound){
			if(!isJumping && wasJumpingForSound){
				audioSource.clip = landSound;
				audioSource.volume = landSoundVolume;
				audioSource.Play ();
			}
		}
		if(!isJumping){
			wasJumpingForSound = false;
		}else{
			wasJumpingForSound = true;
		}
	}
	
	/**********************************************************************************************************************************
	 * CONTROLS: Draws the controls for the system. Good to take a look at to see a glimpse of Unity's GUI system.
	 * *******************************************************************************************************************************/
	
	void DoJumpPolishWindow(int windowID){
		
		
		GUI.DragWindow ();
	}
	
	void DoJumpSimulationWindow (int windowID ) {
		float controlHeight = 20;
		Vector2 scrollViewVector = Vector2.zero;
		useCurveJump = 					GUI.Toggle(new Rect(0,controlHeight,Screen.width/2,controlHeight),useCurveJump,"Use Curve Based Jump?");
		canTap = 						GUI.Toggle(new Rect(0,controlHeight*2,Screen.width/2,controlHeight),canTap,"Allow Tapping?");
		//Draw acceleration based jump controls.
										GUI.Label (new Rect(0,controlHeight*3,Screen.width/2,controlHeight),"CONTROLS FOR ACCELERATION (GRAVITY) CONTROLLED JUMP");
		jumpAccelerationGravity = 		GUI.HorizontalSlider (new Rect(Screen.width/4,controlHeight*4,Screen.width/4,controlHeight), jumpAccelerationGravity, 0.0f, 50.0f);
										GUI.Label (new Rect(0,controlHeight*4,Screen.width/4,controlHeight),"Acceleration Gravity = " + jumpAccelerationGravity);
		jumpAccelerationSpeed =  		GUI.HorizontalSlider (new Rect(Screen.width/4,controlHeight*5,Screen.width/4,controlHeight), jumpAccelerationSpeed, 0.0f, 30.0f);
										GUI.Label (new Rect(0,controlHeight*5,Screen.width/4,controlHeight),"Acceleration Initial Speed = " + jumpAccelerationSpeed);
		jumpAccelerationSpeedCutoff =  	GUI.HorizontalSlider (new Rect(Screen.width/4,controlHeight*6,Screen.width/4,controlHeight), jumpAccelerationSpeedCutoff, 0.0f, jumpAccelerationSpeed);
										GUI.Label (new Rect(0,controlHeight*6,Screen.width/4,controlHeight),"Acceleration Speed Cutoff = " + jumpAccelerationSpeedCutoff);
		
		//Draw curve based controls.
										GUI.Label (new Rect(0,controlHeight*7,Screen.width/2,controlHeight),"CONTROLS FOR CURVE CONTROLLED JUMP");
		jumpCurveLength =  	GUI.HorizontalSlider (new Rect(Screen.width/4,controlHeight*8,Screen.width/4,controlHeight), jumpCurveLength, 0.0f, 5);
										GUI.Label (new Rect(0,controlHeight*8,Screen.width/4,controlHeight),"Curve jump length (in seconds) = " + jumpCurveLength);
		jumpCurveMagnitude =  	GUI.HorizontalSlider (new Rect(Screen.width/4,controlHeight*9,Screen.width/4,controlHeight), jumpCurveMagnitude, 0.0f, 50);
										GUI.Label (new Rect(0,controlHeight*9,Screen.width/4,controlHeight),"Curve jump magnitude (max speed)= " + jumpCurveMagnitude);
		jumpCurveCapPoint =  	GUI.HorizontalSlider (new Rect(Screen.width/4,controlHeight*10,Screen.width/4,controlHeight), jumpCurveCapPoint, 0.0f, 1f);
										GUI.Label (new Rect(0,controlHeight*10,Screen.width/4,controlHeight),"Point in the curve we cap = " + jumpCurveCapPoint);
		
		GUI.DragWindow ();
	}
}
