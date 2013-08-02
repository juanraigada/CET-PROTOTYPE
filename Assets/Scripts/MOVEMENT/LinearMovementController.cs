using UnityEngine;
using System.Collections;

public class LinearMovementController : MovementController {

	// Use this for initialization
	void Start () {
		controllerName = "Fixed Speed Movement";
		numberOfOptions = 7;
	}
	
		/*********************
		 * LINEAR MOVE: 
		 * ******************/
	//These variables hold the maximum speed the player can have.
	float maxLinearSpeed = 15;
	//This holds the minimum slope (slope at which movmenet starts to be affected) and the maximum slope (slope at which movement is not possible.
	float minSlopeLinear = 2f;
	float maxSlopeLinear = 15f;
	//This variable tells the system how much speed to reduce at max slope (theres a linear interpolation from min to max).
	float slopeSpeedReduction = 0.9f;
	//And do we add speed if the slope in downwards?
	bool allowSlopeSpeedIncrement = true;
	//Amount of horizontal speed we apply while jumping
	float jumpMoveFactor = 0.5f;
	
	public override void MovementUpdate () {
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
		Vector3[] conformedInputs = MovementManager.GetConformedInput(new Vector2(x,y),transform.position);
		Vector3 velocity = conformedInputs[0]*maxLinearSpeed + conformedInputs[1]*maxLinearSpeed;
		//We cap the speed. more difficult calculaitons here are unnecessary since the angle is always 45 degrees.
		if(velocity.magnitude > maxLinearSpeed){
			velocity.Normalize ();
			velocity *= maxLinearSpeed;
		}
		//Now we Dot the velocity Normalized and the projected down vector on the slope (normalized). That result is the ammount of velocity applied on the direction of the slope (1 if we are moving fully downslope, -1 if we are moving upslope).
		Vector3 normalizedVelocity = velocity;
		normalizedVelocity.Normalize ();
		Vector3 slopeDirection = -Vector3.up - Vector3.Project(-Vector3.up,MovementManager.currentCollisionNormal);
		slopeDirection.Normalize ();
		float ammountOfVelocityTowardsSlope = Vector3.Dot(normalizedVelocity,slopeDirection);
		//That number, multiplied by the current slope factor, gives us the final velocity.
		float slopeFactor = (Vector3.Angle (MovementManager.currentCollisionNormal,Vector3.up) - minSlopeLinear)/(maxSlopeLinear-minSlopeLinear);
		slopeFactor = Mathf.Max (slopeFactor,0);
		slopeFactor = Mathf.Min (slopeFactor,1);
		float finalSlopeFactor = slopeFactor*ammountOfVelocityTowardsSlope;
		//If we dont allow for acceleration, we dont add it if the ammount of velocity on slope in positive.
		if(finalSlopeFactor > 0 && !allowSlopeSpeedIncrement){
			finalSlopeFactor = 0;
		}
		//And if we have gone over the accepted slopes, we cap the velocity in that direction.
		if(finalSlopeFactor < 0 && slopeFactor == 1){
			finalSlopeFactor = -1;
		}
		velocity = velocity + Vector3.Project (velocity,slopeDirection)*finalSlopeFactor*slopeSpeedReduction;
		
		//This compensates for movement outside of the current Normal (can happen on frams where the normal changes OR when the real collision normal is different from the stored one -because we are over MaxSlope)
		velocity = velocity + Vector3.Project( transform.position - velocity,MovementManager.currentCollisionNormal);
		
		//Then we account for diminishing the speed while jumping.
		if(MovementManager.isJumping){
			rigidbody.velocity = velocity*jumpMoveFactor;
		}else{
			rigidbody.velocity = velocity;
		}
	}
	
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		maxLinearSpeed = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/4,MovementManager.controlHeight), maxLinearSpeed, 0.0f, 25.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/4,MovementManager.controlHeight),"Max Fixed speed (on flat ground)= " + maxLinearSpeed);
		minSlopeLinear = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight), minSlopeLinear, 0.0f, 90.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight),"Slope at which we start to apply reductions = " + minSlopeLinear);
		maxSlopeLinear = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight), maxSlopeLinear, 0.0f, 90.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight),"Slope at which we apply max reductions = " + maxSlopeLinear);
		slopeSpeedReduction = 			GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight), slopeSpeedReduction, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight),"Speed Reduction at max slope= " + slopeSpeedReduction);
		allowSlopeSpeedIncrement = 		GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*5,Screen.width/2,MovementManager.controlHeight),allowSlopeSpeedIncrement,"Allow speed increase over max speed if going down?");
		jumpMoveFactor = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight), jumpMoveFactor, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight),"Air move factor = " + jumpMoveFactor);
	
	}
	
}
