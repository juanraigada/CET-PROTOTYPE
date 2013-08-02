using UnityEngine;
using System.Collections;

public class ForceMovementController : MovementController {

	// Use this for initialization
	void Start () {
		controllerName = "Force-Based Speed Movement";
		numberOfOptions = 9;
	}
	
		/*********************
		 * FORCE MOVE: 
		 * ******************/
	//These variables hold the maximum speed the player can have.
	float maxForceSpeed = 15;
	float forceAcceleration = 30f;
	float forceDrag = 15f;
	//This holds the minimum slope (slope at which movmenet starts to be affected) and the maximum slope (slope at which movement is not possible.
	float minSlopeForce = 0f;
	float maxSlopeForce = 25f;
	//This variable tells the system how much force to apply at max slope (theres a linear interpolation from min to max).
	float slopeForceAcceleration = 25f;
	//And do we add speed if the slope in downwards?
	bool allowSlopeMaxForceIncrement = false;
	//Amount of horizontal speed we apply while jumping
	float jumpMoveFactor = 0.5f;
	
	public override void MovementUpdate(){
		//Get the inputs.
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
		if(x!=0 && y!=0){
			x = x*Mathf.Sin(Mathf.PI/4);
			y = y*Mathf.Sin(Mathf.PI/4);
		}
		//Then we account for diminishing the speed while jumping.
		if(MovementManager.isJumping){
			x = x*jumpMoveFactor;
			y = y*jumpMoveFactor;
		}
		//Get the speed.
		//The first vector, the Y vector, we get through the camera.
		Vector3[] conformedInputs = MovementManager.GetConformedInput(new Vector2(1,1),transform.position);
		Vector3 YVector = conformedInputs[1];
		Vector3 XVector = conformedInputs[0];
		//Now we get the velocituy components on those vectors
		Vector3 velocityVector = (rigidbody.velocity - Vector3.Project(rigidbody.velocity,MovementManager.currentCollisionNormal));
		Vector3 verticalVelocityVector = Vector3.Project(rigidbody.velocity,MovementManager.currentCollisionNormal);
		velocityVector = velocityVector - verticalVelocityVector;
		Vector3 XVectorSpeed = Vector3.Project (velocityVector,XVector);
		Vector3 YVectorSpeed = Vector3.Project (velocityVector,YVector);
		//Now we add the speed.
		if(x!=0){
			XVectorSpeed = XVectorSpeed + XVector*Time.deltaTime*forceAcceleration*x;
		}
		if(y!=0){
			YVectorSpeed = YVectorSpeed + YVector*Time.deltaTime*forceAcceleration*y;
		}
		if(x==0&&y==0&&!MovementManager.isJumping){
			if(XVectorSpeed.magnitude !=0){
				XVectorSpeed = XVectorSpeed -  XVectorSpeed/XVectorSpeed.magnitude*Time.deltaTime*forceDrag;
			}
			if(YVectorSpeed.magnitude !=0){
				YVectorSpeed = YVectorSpeed - YVectorSpeed/YVectorSpeed.magnitude*Time.deltaTime*forceDrag;
			}
		}
		
		//We modify this with the slope.
		Vector3 velocity = XVectorSpeed + YVectorSpeed;
		Vector3 slopeDirection = -Vector3.up - Vector3.Project(-Vector3.up,MovementManager.currentCollisionNormal);
		slopeDirection.Normalize ();
		//That number, multiplied by the current slope factor, gives us the final velocity.
		float slopeFactor = (Vector3.Angle (MovementManager.currentCollisionNormal,Vector3.up) - minSlopeForce)/(maxSlopeForce-minSlopeForce);
		slopeFactor = Mathf.Max (slopeFactor,0);
		slopeFactor = Mathf.Min (slopeFactor,1);
		slopeDirection = slopeDirection*slopeFactor*slopeForceAcceleration*Time.deltaTime;
		float velocityOnSlope = Vector3.Dot (velocity,slopeDirection);
		//If we dont allow for acceleration, we dont add it if the ammount of velocity on slope in positive.
		if(velocityOnSlope > 0 && !allowSlopeMaxForceIncrement){
			slopeDirection = Vector3.zero;
		}
		if(velocityOnSlope < 0  && slopeFactor == 1){
		//And if we have gone over the accepted slopes, we cap the velocity in that direction.
			slopeDirection.Normalize ();
			slopeDirection = -Vector3.Project (velocity,slopeDirection);
		}
		if(velocity.magnitude > maxForceSpeed){
			velocity.Normalize ();
			velocity *= maxForceSpeed;
		}
		if(!MovementManager.isJumping){
			verticalVelocityVector = Vector3.zero;
		}
		velocity =  verticalVelocityVector + velocity + slopeDirection;
		//This compensates for movement outside of the current Normal (can happen on frams where the normal changes OR when the real collision normal is different from the stored one -because we are over MaxSlope)
		velocity = velocity - MovementManager.currentCollisionNormal;
		rigidbody.velocity = velocity;
		
	}
	
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		maxForceSpeed = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/4,MovementManager.controlHeight), maxForceSpeed, 0.0f, 35.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/4,MovementManager.controlHeight),"Max speed to be attainable by accelerating= " + maxForceSpeed);
		forceAcceleration = 			GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight), forceAcceleration, 0.0f, 50.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight),"Accelerating (and decelerating) force= " + forceAcceleration);
		forceDrag = 					GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight), forceDrag, 0.0f, 50.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight),"Drag (for auto stopping)= " + forceDrag);
		minSlopeForce = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight), minSlopeForce, 0.0f, 90.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight),"Slope at which we start to apply extra force = " + minSlopeForce);
		maxSlopeForce = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*5,Screen.width/4,MovementManager.controlHeight), maxSlopeForce, 0.0f, 90.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*5,Screen.width/4,MovementManager.controlHeight),"Slope at which we apply max extra force = " + maxSlopeForce);
		slopeForceAcceleration = 		GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight), slopeForceAcceleration, 0.0f, 50.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight),"Force applied at max slope= " + slopeForceAcceleration);
		allowSlopeMaxForceIncrement = 	GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*7,Screen.width/2,MovementManager.controlHeight),allowSlopeMaxForceIncrement,"Allow the ball to go down?");
		jumpMoveFactor = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*8,Screen.width/4,MovementManager.controlHeight), jumpMoveFactor, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*8,Screen.width/4,MovementManager.controlHeight),"Air move factor = " + jumpMoveFactor);
	
	}
}
