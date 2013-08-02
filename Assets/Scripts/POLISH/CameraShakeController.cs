using UnityEngine;
using System.Collections;

/********************************************************************************
 * 	CONTROLS THE CAMERA SHAKE
 *	Cameras shake add weight and physicality to the interactions 
 *	and are a very easy effect to implement.
 * ******************************************************************************/

public class CameraShakeController : PolishController {

	
	// Use this for initialization
	void Start () {
		polishName = "Camera Shake";
		numberOfOptions = 5;
	}
	
	
	
		/****************************
		 * CAMERA SHAKE:	
		 * *************************/
	
	//Will we shake the camera?
	bool shakeCamera = true;
	//Strength of the shake. First value is how out from the center position we want to go, second value is how hard the interpolation to that position is.
	float shakeOutStrength = 0.85f;
	float shakeRecoverStrength = 25f;
	//Time the shake goes for.
	float shakeTime = 0.25f;
	
	//AUXILIARY VARIABLES
	//Vector were we store the camera original posititon.
	Vector3 cameraOriginalPosition = Vector3.zero;
	//Flag, are we shaking the camera?
	bool isShaking = false;
	//Auxiliary flag to detect a collision without having to go into the collision functions, to keep this part of the code self-reliable.
	bool wasJumping = false;
	//Counter for the current Time into the shaking.
	float currentShaketime = 0f;
	
	public override void DoPolish(){
		//Check if we need to initialize the shake.
		if(cameraOriginalPosition == Vector3.zero){
			cameraOriginalPosition = Camera.main.transform.position;
		}
		if(shakeCamera){
			if(!MovementManager.isJumping && wasJumping){
				currentShaketime = 0;
				isShaking = true;
			}
		}
		//If the shake is initialized, execute it.
		if(isShaking&&!MovementManager.isJumping){
			currentShaketime += Time.deltaTime;
			if(currentShaketime > shakeTime){
				isShaking = false;
			}
			Vector3 randomVector = Random.insideUnitSphere*shakeOutStrength;
			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position,Camera.main.transform.position+randomVector,shakeRecoverStrength*Time.deltaTime);
			
		}else{
			Camera.main.transform.position = cameraOriginalPosition;
		}
		
		//Here we set the flag as tio wether the character was jumping in the previous frame.
		if(!MovementManager.isJumping){
			wasJumping = false;
		}else{
			wasJumping = true;
		}
	}
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		shakeCamera = 					GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/2,MovementManager.controlHeight),shakeCamera,"Shake the camera?");
		shakeOutStrength = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight), shakeOutStrength, 0.0f, 2.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight),"Shake out vector size = " + shakeOutStrength);
		shakeRecoverStrength = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight), shakeRecoverStrength, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight),"Strength of movement interpolation to shake position = " + shakeRecoverStrength);
		shakeTime = 					GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight), shakeTime, 0.0f, 2.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight),"Time the shake lasts = " + shakeTime);
		
	}
}
