using UnityEngine;
using System.Collections;


/********************************************************************************
 * 	SQUASHES AND STRECTCHES THE BALL ON JUMP 
 *	Animations, even this simple, can totally change how a character feels.
 * ******************************************************************************/

public class SquahAndStrechPolishController : PolishController {

	//variable that points to the visual representation of the ball.
	public Transform ball;
	
	// Use this for initialization
	void Start () {
		polishName = "Squash and Stretch";
		numberOfOptions = 7;
	}
	
		/****************************
		 * SQUASH AND STRETCH:	
		 * *************************/
	//These deals with the squash and stretching, by setting magnitudes.
	bool squashAndStrech = true;
	float squashMagnitude = 0.75f;
	float stretchMagnitude = 0.75f;
	float maxSpeedStretch = 10;
	float maxSpeedSquash = 10;
		//This stores the Squash time in case of collision at max speed.
	float squashMaxtime = 0.4f;
	float squashCurrentTime = 0f;
	float jumpCollisionSpeed = 0f;
	
	public override void DoPolish(){
		if(squashAndStrech){
			Vector3 wantedScale = new Vector3(1,1,1);
			if(MovementManager.isJumping){
			//Then, if jumping, we simply stretch the ball according to the current speed.
				float currentSpeed = Vector3.Project (rigidbody.velocity,transform.up).magnitude;
				jumpCollisionSpeed = currentSpeed;
				float ammount = Mathf.Min (currentSpeed/maxSpeedStretch,1);
				wantedScale = new Vector3(1-ammount*stretchMagnitude,1+ammount*stretchMagnitude,1-ammount*stretchMagnitude);
				squashCurrentTime = 0;
			}else{
				squashCurrentTime += Time.deltaTime;
				//Now we set a max time for this squash
				float squashMax = Mathf.Min (jumpCollisionSpeed/maxSpeedSquash,1);
				float squashTimeAmount = Mathf.Min (squashCurrentTime/squashMaxtime*squashMax,1);
				float squashAmount = squashMax*Mathf.Sin (Mathf.PI*squashTimeAmount)*squashMagnitude;
				wantedScale = new Vector3(1+squashAmount,1-squashAmount,1+squashAmount);
			}
			ball.localScale = Vector3.Lerp (ball.localScale,wantedScale,10f*Time.deltaTime);
			ball.localPosition = new Vector3(0,-(1-ball.localScale.y)/2,0);
			ball.rotation = Quaternion.LookRotation(ball.forward,MovementManager.currentCollisionNormal);
		}
		
	}
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		squashAndStrech = 				GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/2,MovementManager.controlHeight),squashAndStrech,"Squash and Stretch?");
		squashMagnitude = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight), squashMagnitude, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight),"Percentage of the ball to Squash = " + squashMagnitude);
		stretchMagnitude = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight), stretchMagnitude, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight),"Percentage of the ball to Stretch = " + stretchMagnitude);
		maxSpeedStretch = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight), maxSpeedStretch, 0.0f, 30.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight),"Speed at which stretch maxes out = " + maxSpeedStretch);
		maxSpeedSquash = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*5,Screen.width/4,MovementManager.controlHeight), maxSpeedSquash, 0.0f, 30.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*5,Screen.width/4,MovementManager.controlHeight),"Speed at which squash maxes out = " + maxSpeedSquash);
		squashMaxtime = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight), squashMaxtime, 0.0f, 2.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight),"Time the squash lasts = " + squashMaxtime);
	}
}
