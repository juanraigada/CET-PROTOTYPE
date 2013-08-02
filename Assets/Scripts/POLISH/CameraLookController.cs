using UnityEngine;
using System.Collections;

/********************************************************************************
 * 	CONTROLS THE CAMERA ORIENTATION
 *	Cameras are considered polish, although arguably they affect simulation and control.
 *All this script does is to ensure the camera always points towards the ball.
 * ******************************************************************************/

public class CameraLookController : PolishController {
	
	public Transform ball;
	
	// Use this for initialization
	void Start () {
		polishName = "Camera Look";
		numberOfOptions = 4;
	}
	
		/****************************
		 * CAMERA INTERPOLATION:	
		 * *************************/
	//Will we slowly interpolate the camera to face the ball?
	bool interpolateCamera = true;
	//Variables to hold the current and the desired interpolation strength (the current we used, the desired we switch to when we want soft interpolation. Otherwise we set current to a high value).
	float cameraInterpolationStrength = 16f;
	//Vertical offset (characters are seldom centered in the vertical)
	float cameraVerticalOffSet = 3;
	
	
	public override void DoPolish(){
		if(interpolateCamera){
			//Focus camera on ball. Ideally this code would be in a proper CameraController class. However, for the purposes of this exercise, we are keeping all relevant code in one single file.
			Camera.main.transform.rotation = Quaternion.Slerp (Camera.main.transform.rotation,Quaternion.LookRotation(ball.position + Camera.main.transform.up*cameraVerticalOffSet - Camera.main.transform.position,Camera.main.transform.up),cameraInterpolationStrength*Time.deltaTime);
		}else{
			//Focus camera on ball. 
			Camera.main.transform.rotation = Quaternion.LookRotation(ball.position + Camera.main.transform.up*cameraVerticalOffSet - Camera.main.transform.position,Camera.main.transform.up);
		}
	}
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		interpolateCamera = 			GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/2,MovementManager.controlHeight),interpolateCamera,"Interpolate camera look?");
		cameraInterpolationStrength = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight), cameraInterpolationStrength, 0.0f, 20.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight),"Camera Interpolation Strentgh = " + cameraInterpolationStrength);
		cameraVerticalOffSet = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight), cameraVerticalOffSet, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight),"Vertical Offset = " + cameraVerticalOffSet);
		
	}
}
