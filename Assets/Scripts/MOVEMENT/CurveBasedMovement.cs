using UnityEngine;
using System.Collections;

public class CurveBasedMovement : MovementController {

	// Use this for initialization
	void Start () {
		controllerName = "Curve-Based";
		numberOfOptions = 10;
	}
	
		/*********************
		* CURVE MOVE: 
		* ******************/
	public AnimationCurve movementCurve;
	float maxSpeedCurve = 30;
	float lengthAcceleration = 2;
	float lengthDeceleration = 0.3f;
	float lengthBrake = 0.45f;
	float currentX = 0;
	float currentY = 0;
	//This holds the minimum slope (slope at which movmenet starts to be affected) and the maximum slope (slope at which movement is not possible.
	float minSlopeCurve = 0f;
	float maxSlopeCurve = 25f;
	//This variable tells the system how much force to apply at max slope (theres a linear interpolation from min to max).
	float slopeForceCurve = 0.25f;
	bool allowSlopeCurveAccelerationIncrement = true;
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
		
		//Now we get the vectors and apply the speed.//The first vector, the Y vector, we get through the camera.
		Vector3 YVector = transform.position - Camera.main.transform.position - Vector3.Project (transform.position - Camera.main.transform.position,MovementManager.currentCollisionNormal);
		YVector.Normalize ();
		//The second one we get through cross product
		Vector3 XVector = Vector3.Cross (MovementManager.currentCollisionNormal,YVector);
		XVector.Normalize ();
		
		//We update our current position in the curve according to wether we are acceleration, decelerating (trying to change direction) or braking.
		float airModifier = 1;
		if(MovementManager.isJumping){
			airModifier = jumpMoveFactor;
		}
		
		float xChange = currentX;
		float yChange = currentY;
		
		
		if((x>0&&currentX>=0)||((x<0&&currentX<=0))){
			currentX += x*Time.deltaTime/lengthAcceleration*airModifier ;
		}else if((x>0&&currentX<=0)||((x<0&&currentX>=0))){
			currentX += x*Time.deltaTime/lengthDeceleration*airModifier;
		}else if(x==0){
			float tobrakeX = Time.deltaTime/lengthBrake*airModifier*Mathf.Sign(currentX);
			if(Mathf.Abs (tobrakeX)>Mathf.Abs (currentX)){
				currentX = 0;
			}else{
				currentX -= tobrakeX;
			}
		}
		if((y>0&&currentY>=0)||((y<0&&currentY<=0))){
			currentY += y*Time.deltaTime/lengthAcceleration*airModifier;
		}else if((y>0&&currentY<=0)||((y<0&&currentY>=0))){
			currentY += y*Time.deltaTime/lengthDeceleration*airModifier;
		}else if(y==0){
			float tobrakeY = Time.deltaTime/lengthBrake*airModifier*Mathf.Sign(currentY);
			if(Mathf.Abs (tobrakeY)>Mathf.Abs (currentY)){
				currentY = 0;
			}else{
				currentY -= tobrakeY;
			}
		}
		
		xChange = currentX - xChange;
		yChange = currentY - xChange;	
		Vector3 slopeDirection = -Vector3.up - Vector3.Project(-Vector3.up,MovementManager.currentCollisionNormal);
		slopeDirection.Normalize ();
		//That number, multiplied by the current slope factor, gives us the final velocity.
		float slopeFactor = (Vector3.Angle (MovementManager.currentCollisionNormal,Vector3.up) - minSlopeCurve)/(maxSlopeCurve-minSlopeCurve);
		slopeFactor = Mathf.Max (slopeFactor,0);
		slopeFactor = Mathf.Min (slopeFactor,1);
		float onSlopeX = Vector3.Dot (XVector,slopeDirection)*slopeForceCurve;
		float onSlopeY = Vector3.Dot (YVector,slopeDirection)*slopeForceCurve;
		//If we dont allow for acceleration, we dont add it if the ammount of velocity on slope in positive.
		if(onSlopeX > 0 && !allowSlopeCurveAccelerationIncrement){
			onSlopeX = 0;
		}
		if(onSlopeY > 0 && !allowSlopeCurveAccelerationIncrement){
			onSlopeY = 0;
		}
		if(onSlopeX < 0  && slopeFactor == 1){
		//And if we have gone over the accepted slopes, we cap the velocity in that direction.
			onSlopeX = -1;
		}if(onSlopeY < 0  && slopeFactor == 1){
		//And if we have gone over the accepted slopes, we cap the velocity in that direction.
			onSlopeY = -1;
		}
		onSlopeX = onSlopeX*Mathf.Sign (currentX);
		onSlopeY = onSlopeX*Mathf.Sign (currentY);
		xChange = xChange*onSlopeX;
		yChange = xChange*onSlopeY;	
		currentX = currentX + xChange;
		currentY = currentY + yChange;
		
		float speedX = movementCurve.Evaluate(Mathf.Abs (currentX))*maxSpeedCurve*Mathf.Sign (currentX);
		float speedY = movementCurve.Evaluate(Mathf.Abs (currentY))*maxSpeedCurve*Mathf.Sign (currentY);
		Vector3 velocity = XVector*speedX+YVector*speedY;
		if(velocity.magnitude > maxSpeedCurve){
			velocity.Normalize ();
			velocity = velocity*maxSpeedCurve;
		} 
		
		rigidbody.velocity = velocity +  Vector3.Project( transform.position - velocity,MovementManager.currentCollisionNormal);
	}
	
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		
		
		maxSpeedCurve = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/4,MovementManager.controlHeight), maxSpeedCurve, 0.0f, 35.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/4,MovementManager.controlHeight),"Max speed to be attainable by accelerating= " + maxSpeedCurve);
		lengthAcceleration = 			GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight), lengthAcceleration, 0.0f, 5.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/4,MovementManager.controlHeight),"Time it takes to accelerate= " + lengthAcceleration);
		lengthDeceleration = 			GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight), lengthDeceleration, 0.0f, 5.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*3,Screen.width/4,MovementManager.controlHeight),"Time it takes to change direction= " + lengthDeceleration);
		lengthBrake = 					GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight), lengthBrake, 0.0f, 5.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*4,Screen.width/4,MovementManager.controlHeight),"Time it takes to brake= " + lengthBrake);
		minSlopeCurve = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*5,Screen.width/4,MovementManager.controlHeight), minSlopeCurve, 0.0f, 90.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*5,Screen.width/4,MovementManager.controlHeight),"Slope at which we start make accelerating and braking faster = " + minSlopeCurve);
		maxSlopeCurve = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight), maxSlopeCurve, 0.0f, 90.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*6,Screen.width/4,MovementManager.controlHeight),"Slope at which we end make accelerating and braking faster  " + maxSlopeCurve);
		
		
		slopeForceCurve = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*7,Screen.width/4,MovementManager.controlHeight), slopeForceCurve, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*7,Screen.width/4,MovementManager.controlHeight),"Maximum change acceleration and deceleration time based on slope= " + slopeForceCurve);
		allowSlopeCurveAccelerationIncrement = 	GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*8,Screen.width/2,MovementManager.controlHeight),allowSlopeCurveAccelerationIncrement,"Allow the ball accelerate faster when going down?");
		jumpMoveFactor = 				GUI.HorizontalSlider (new Rect(Screen.width/4,MovementManager.controlHeight*3 + MovementManager.controlHeight*9,Screen.width/4,MovementManager.controlHeight), jumpMoveFactor, 0.0f, 1.0f);
										GUI.Label (new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*9,Screen.width/4,MovementManager.controlHeight),"Air move factor = " + jumpMoveFactor);
	
	}
}
