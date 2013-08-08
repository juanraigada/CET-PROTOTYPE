using UnityEngine;
using System.Collections;

/********************************************************************************
 * 	ANIMATIONS 
 *	Animations, even this simple, can totally change how a character feels.
 * ******************************************************************************/

public class AnimationPolishController : PolishController {

	//variable that points to the visual representation of the ball.
	public Transform ball;
	public Transform face;
	
	// Use this for initialization
	void Start () {
		polishName = "Animation";
		numberOfOptions = 12;
	}
	
		/****************************
		 * ANIMATION:	
		 * *************************/
	//These deals with the squash and stretching, by setting magnitudes.
	bool showEyes = true;
	bool rotationBasedOnInput = true;
	float rotationSpeed = 5f;
	bool blink = true;
	//Average time between blinks
	float blinkRandomPeriod = 1f;
	//Time the blink takes
	float blinkDuration = 0.2f;
	float currentBlinkDuration = 0;
	//Curves for the hopping
	bool hop = true;
	public AnimationCurve verticalHopCurve;
	public AnimationCurve horizontalHopCurve;
	public AnimationCurve scaleHopCurve;
	float speedToCurveFactor = 0.075f;
	float hopTime = 0.2f;
	float hopHeight = 0.85f;
	float scaleFactor = 1.8f;
	//Auxiliary
	float currentHoptime = 0;
	float blinkCurrentTime = 0;
	float blinkTime = 0;
	
	//This is going to be used by the sound and particle controllers to check whether its time to play
	public bool playHopSound = false;
	public bool playHopParticle = false;
	
	Vector3 previouslook;
	
	
	public override void DoPolish(){
		if(showEyes){
			face.gameObject.SetActive(true);
		}else{
			face.gameObject.SetActive(false);
		}
		
		//Rotation
		Vector3 desiredVector = (rigidbody.velocity - Vector3.Project (rigidbody.velocity,MovementManager.currentCollisionNormal))/5;
		if(rotationBasedOnInput){
			float x = 0;
			if(InputManager.inputs[(int)InputManager.inputTypes.LEFT].isActive){
				 x = -1;
			}
			if(InputManager.inputs[(int)InputManager.inputTypes.RIGHT].isActive){
				x = 1;
			}
			float y = 0;
			if(InputManager.inputs[(int)InputManager.inputTypes.DOWN].isActive){
				 y = -1;
			}
			if(InputManager.inputs[(int)InputManager.inputTypes.UP].isActive){
				y = 1;
			}
			Vector3[] v = MovementManager.GetConformedInput(new Vector2(x,y),transform.position);
			desiredVector = v[0] + v[1];
		}
		if(desiredVector.magnitude < 0.5){
			desiredVector = previouslook;
		}
		
		ball.rotation = Quaternion.Slerp (ball.rotation,Quaternion.LookRotation(desiredVector,MovementManager.currentCollisionNormal),rotationSpeed*Time.deltaTime);
		previouslook = ball.forward;
		
		//Blink
		if(blink){
			blinkCurrentTime = blinkCurrentTime + Time.deltaTime;
			if(blinkCurrentTime > blinkTime){
				blinkTime = Random.Range(blinkRandomPeriod*0.5f,blinkRandomPeriod*1.5f);
				blinkCurrentTime = 0;
				currentBlinkDuration = 0;
			}
			currentBlinkDuration = currentBlinkDuration + Time.deltaTime;
			float amm = 1-Mathf.Sin (Mathf.PI*Mathf.Min (currentBlinkDuration/blinkDuration,1));
			face.localScale = new Vector3(1,amm,1);
		}else{
			face.localScale = new Vector3(1,1,1);
		}
		
		
		//Movement hops
		//NOTE: this code interacts with the squash and strech code, since we are starting to do very similare things in different classes...
		//This was to demonstrate the jump animation polish without having to introduce the move animation polish.
		//However, in a real implementation that code has to go together.
		if(hop&&!MovementManager.isJumping&&((SquahAndStrechPolishController)gameObject.GetComponent("SquahAndStrechPolishController")).IsSquashFinished()){
			//How much space am I going through at this speed for a full hop
			float distance = (rigidbody.velocity - Vector3.Project(rigidbody.velocity,MovementManager.currentCollisionNormal)).magnitude*speedToCurveFactor*hopTime; 
			//Where am I on the hop
			if(rigidbody.velocity.magnitude > 2 || currentHoptime > 0){
				currentHoptime = currentHoptime + Time.deltaTime;
			}
			float am = currentHoptime/hopTime;  
			ball.transform.localScale = new Vector3(1,1-((0.5f-scaleHopCurve.Evaluate(am))/2)*scaleFactor,1);
			ball.transform.localPosition = -ball.transform.forward*horizontalHopCurve.Evaluate(am)*distance + ball.transform.up*hopHeight*verticalHopCurve.Evaluate(am) -ball.transform.up*(1-ball.transform.localScale.y)/2;
			
			if(currentHoptime > hopTime){
				currentHoptime = 0;
			}
			//For hopping we need to do it in the "going up" phase. Note that we are using a number under 0.5 so that theres no delay.
			if(currentHoptime > hopTime*0.35 && currentHoptime - Time.deltaTime< hopTime*0.35){
				playHopSound = true;
				//Here we set the hooks for sound and particles...
				playHopParticle = true;
			}
		}else{
			currentHoptime = 0;
		}
	}
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		showEyes = 						GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/2,MovementManager.controlHeight),showEyes,"Show Eyes?");
		rotationBasedOnInput = 			GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/2,MovementManager.controlHeight),rotationBasedOnInput,"Is rotation based on input (as possed to on velocity)?");
		rotationSpeed = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight), rotationSpeed, 0.0f, 30.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight),"Rotation Speed = " + rotationSpeed);
		
		blink = 						GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/2,MovementManager.controlHeight),blink,"Blink?");
		blinkRandomPeriod = 			GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*5,Screen.width/4,MovementManager.controlHeight), blinkRandomPeriod, 0.0f, 5.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*5,Screen.width/4,MovementManager.controlHeight),"Average time between blinks = " + blinkRandomPeriod);
		blinkDuration = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight), blinkDuration, 0.0f, 2.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight),"Duration of each blink = " + blinkDuration);
		
		
		hop = 							GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*7,Screen.width/2,MovementManager.controlHeight),hop,"Hop?");
		speedToCurveFactor = 			GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*8,Screen.width/4,MovementManager.controlHeight), speedToCurveFactor, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*8,Screen.width/4,MovementManager.controlHeight),"Factor of hop speed versus real speed = " + speedToCurveFactor);
		hopTime = 						GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*9,Screen.width/4,MovementManager.controlHeight), hopTime, 0.0f, 2.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*9,Screen.width/4,MovementManager.controlHeight),"Time each hop takes = " + hopTime);
		hopHeight = 						GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*10,Screen.width/4,MovementManager.controlHeight), hopHeight, 0.0f, 2.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*10,Screen.width/4,MovementManager.controlHeight),"Height of each hop (checked against curve) = " + hopHeight);
		scaleFactor = 						GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*11,Screen.width/4,MovementManager.controlHeight), scaleFactor, 0.0f, 2.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*11,Screen.width/4,MovementManager.controlHeight),"Squash and strech of each hop (checked against curve) = " + scaleFactor);
		
	}
	
}
