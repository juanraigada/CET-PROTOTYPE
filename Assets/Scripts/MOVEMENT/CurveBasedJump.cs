using UnityEngine;
using System.Collections;

public class CurveBasedJump : MovementController {

	// Use this for initialization
	void Start () {
		controllerName = "Curve Based Jump";
		numberOfOptions = 5;
	}
	
	
		/*********************
		 * CURVE BASED JUMP: 
		 * ******************/
	//These variables hold the speed curve for the jump, the duration of the curve, it's magnitude (what speed the curve maps to in the vertical) at the normalized point in time we cutoff to (since we map speed to time, we dont need a speed value but a time value).
	public AnimationCurve jumpSpeedCurve;
	float jumpCurveLength = 1.05f;
	float jumpCurveMagnitude = 40f;
	float jumpCurveCapPoint = 0.35f;
		//This is an auxiliary variable to hold the current time into the jump. A generic timer class would do, but this is more readable.
	float currentJumpCurveTime;
		//Normalized point at which the curve goes from up to down. This means the Apex of the curve in Unity's editor should rest at 0.5 in the X axis.
		//Alternatively we could use two curves to avoid mistakes, but making reading the curve less intuitive. 
	float jumpCurveHighPoint = 0.5f;
	//This selects wether to allow jumping to stop early (allowing for tapping to hop and holding to jump):
	bool canTap = true;
		//Flag Toggle to avoid capping the speed more than once (for optimization, not necessary):
	bool isHolding = false;
	
	public override void MovementUpdate(){
		//First part of the check is to see if we SHOULD be jumping (basically looking for input in we are alreadynot jumping)
		if(!MovementManager.isJumping && InputManager.inputs[(int)InputManager.inputTypes.JUMP].isActive){
			//We initialize our flags and variables.
			MovementManager.isJumping = true;
			isHolding = true;
			//We initialize the curve
			currentJumpCurveTime = 0;
		}
		//And then, if we are jumping, update the physics.
		if(MovementManager.isJumping){
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
	
	
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		
		canTap = 						GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/2,MovementManager.controlHeight),canTap,"Allow Tapping?");
		jumpCurveLength =  				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight), jumpCurveLength, 0.0f, 5);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight),"Curve jump length (in seconds) = " + jumpCurveLength);
		jumpCurveMagnitude =  			GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight), jumpCurveMagnitude, 0.0f, 50);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight),"Curve jump magnitude (max speed)= " + jumpCurveMagnitude);
		jumpCurveCapPoint =  			GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight), jumpCurveCapPoint, 0.0f, 1f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight),"Point in the curve we cap = " + jumpCurveCapPoint);
	}
}
