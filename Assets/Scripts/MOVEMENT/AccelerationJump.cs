using UnityEngine;
using System.Collections;

public class AccelerationJump : MovementController {
	
	
	
	// Use this for initialization
	void Start () {
		controllerName = "Acceleration Based Jump";
		numberOfOptions = 5;
	}
	
	
		/****************************
		 * ACCELERATION BASED JUMP:	
		 * *************************/
	//These variables controls the vertical jump negative acceleration, initial speed and speed cutoff (the speed we cap at at input release):
	float jumpAccelerationGravity = 25;
	float jumpAccelerationSpeed = 15;
	float jumpAccelerationSpeedCutoff = 7.5f;
	
	float currentVerticalSpeed = 0;
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
			//Set initial speed
			currentVerticalSpeed = jumpAccelerationSpeed;
		}
		//And then, if we are jumping, update the physics.
		if(MovementManager.isJumping){
			if(canTap&&isHolding&&!InputManager.inputs[(int)InputManager.inputTypes.JUMP].isActive){
				isHolding = false;
				//Check upwards velocity
				if(currentVerticalSpeed > 0 && currentVerticalSpeed > jumpAccelerationSpeedCutoff){
					currentVerticalSpeed = jumpAccelerationSpeedCutoff;
				}
			}
			currentVerticalSpeed = currentVerticalSpeed - jumpAccelerationGravity*Time.deltaTime;
			rigidbody.velocity = new Vector3(rigidbody.velocity.x,currentVerticalSpeed,rigidbody.velocity.z);
		}
		
	}
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		
		canTap = 						GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/2,MovementManager.controlHeight),canTap,"Allow Tapping?");
		jumpAccelerationGravity = 		GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight), jumpAccelerationGravity, 0.0f, 90.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight),"Acceleration Gravity = " + jumpAccelerationGravity);
		jumpAccelerationSpeed =  		GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight), jumpAccelerationSpeed, 0.0f, 30.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight),"Acceleration Initial Speed = " + jumpAccelerationSpeed);
		jumpAccelerationSpeedCutoff =  	GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight), jumpAccelerationSpeedCutoff, 0.0f, jumpAccelerationSpeed);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight),"Acceleration Speed Cutoff = " + jumpAccelerationSpeedCutoff);
	
	}
}
