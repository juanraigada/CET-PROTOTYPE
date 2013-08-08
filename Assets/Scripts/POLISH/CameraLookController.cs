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
		numberOfOptions = 9;
	}
	
		/****************************
		 * CAMERA INTERPOLATION:	
		 * *************************/
	//Will we slowly interpolate the camera to face the ball?
	bool interpolateCamera = true;
	//Variables to hold the current and the desired interpolation strength (the current we used, the desired we switch to when we want soft interpolation. Otherwise we set current to a high value).
	float cameraInterpolationStrength = 16f;
	//Vertical offset (characters are seldom centered in the vertical)
	float cameraVerticalOffSet = 1;
	float cameraHorizontalOffsetPerSpeed = 0.2f;
	
	
	bool follow = true;
	float distanceToPlayer = 12;
	float height = 3;
	float cameraMovementStrength = 2f;
	public LayerMask cameraCollisionLayers;
	float distanceToWall = 0.2f;
	
	Vector3 vectorToPlayer;
	
	public override void DoPolish(){
		vectorToPlayer = Camera.main.transform.position - transform.position - Vector3.Project (Camera.main.transform.position - transform.position,Vector3.up);
		vectorToPlayer.Normalize ();
		
		Vector3 cameraRelativeVelocity = Vector3.Project (rigidbody.velocity  ,Camera.main.transform.right);
		
		if(interpolateCamera){
			//Focus camera on ball. Ideally this code would be in a proper CameraController class. However, for the purposes of this exercise, we are keeping all relevant code in one single file.
			Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation,Quaternion.LookRotation(transform.position + cameraRelativeVelocity*cameraHorizontalOffsetPerSpeed + Camera.main.transform.up*cameraVerticalOffSet - Camera.main.transform.position,Vector3.up),cameraInterpolationStrength*Time.deltaTime);
		}else{
			//Focus camera on ball. 
			Camera.main.transform.rotation = Quaternion.LookRotation(transform.position + cameraRelativeVelocity*cameraHorizontalOffsetPerSpeed+ Camera.main.transform.up*cameraVerticalOffSet - Camera.main.transform.position,Camera.main.transform.up);
		}
		
		if(follow){
			Vector3 desiredPosition = transform.position + vectorToPlayer*distanceToPlayer + Vector3.up*height;
			
			Vector3 dir = desiredPosition - transform.position;
			float distanceToCamera = dir.magnitude;
			dir.Normalize ();
			
			RaycastHit hit = new RaycastHit();
			if (Physics.Raycast (transform.position, dir,out hit,distanceToCamera + distanceToWall,cameraCollisionLayers )) {
				Vector3 point = hit.point - dir*distanceToWall;
				desiredPosition = point + Vector3.up*(1 - (point - transform.position).magnitude/distanceToPlayer)*10;
			}
			Camera.main.transform.position =Vector3.Lerp (Camera.main.transform.position ,desiredPosition,cameraMovementStrength*Time.deltaTime);
		}
	}
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		interpolateCamera = 			GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/2,MovementManager.controlHeight),interpolateCamera,"Interpolate camera look?");
		cameraInterpolationStrength = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight), cameraInterpolationStrength, 0.0f, 20.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight),"Camera Interpolation Strentgh = " + cameraInterpolationStrength);
		cameraVerticalOffSet = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight), cameraVerticalOffSet, 0.0f, 2.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight),"Vertical Offset = " + cameraVerticalOffSet);
		cameraHorizontalOffsetPerSpeed = GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight), cameraHorizontalOffsetPerSpeed, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight),"Horizontal Offset Relative to Speed = " + cameraHorizontalOffsetPerSpeed);
		follow = 			GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*5,Screen.width/2,MovementManager.controlHeight),follow,"CameraFollow Ball?");
		distanceToPlayer = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight), distanceToPlayer, 0.0f, 20.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight),"Distance to the ball = " + distanceToPlayer);
		height = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*7,Screen.width/4,MovementManager.controlHeight), height, 0.0f, 10.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*7,Screen.width/4,MovementManager.controlHeight),"Height = " + height);
		cameraMovementStrength = GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*8,Screen.width/4,MovementManager.controlHeight), cameraMovementStrength, 0.0f, 4.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*8,Screen.width/4,MovementManager.controlHeight),"Camera follow strength = " + cameraMovementStrength);
		
		
	}
}
