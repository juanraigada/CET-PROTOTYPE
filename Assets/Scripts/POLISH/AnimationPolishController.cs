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
		numberOfOptions = 7;
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
	float speedToCurveFactor = 0.25f;
	float hopTime = 0.35f;
	float hopHeight = 0.25f;
	float scaleFactor = 0.9f;
	float distanceFactor = 0.05f;
	//Auxiliary
	float currentHoptime = 0;
	float blinkCurrentTime = 0;
	float blinkTime = 0;
	
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
		
		
		//Movemenet hops
		
		//NOTE: this code interacts with the squash and strech code, since we are starting to do very similare things in different classes... N
		//This was to demosntrate the jump animation polish without having to introduce the move animation polish.
		//However, in a real implementation that code has to go together.
		if(hop&&!MovementManager.isJumping&&((SquahAndStrechPolishController)gameObject.GetComponent("SquahAndStrechPolishController")).IsSquashFinished()){
			//How much space am I going through at this speed for a full hop
			float distance = (rigidbody.velocity - Vector3.Project(rigidbody.velocity,MovementManager.currentCollisionNormal)).magnitude*speedToCurveFactor*hopTime;
			//Where am I on the hop
			if(rigidbody.velocity.magnitude > 5 || currentHoptime > 0){
				currentHoptime = currentHoptime + Time.deltaTime*distanceFactor/Mathf.Min (distance,distanceFactor);
			}
			float am = currentHoptime/hopTime;  
			ball.transform.localScale = new Vector3(1,1-((0.5f-scaleHopCurve.Evaluate(am))/2)*scaleFactor,1);
			Debug.Log (am + " " + scaleHopCurve.Evaluate(am));
			ball.transform.localPosition = -ball.transform.forward*horizontalHopCurve.Evaluate(am)*distance + ball.transform.up*hopHeight*verticalHopCurve.Evaluate(am) -ball.transform.up*(1-ball.transform.localScale.y)/2;
			
			if(currentHoptime > hopTime){
				currentHoptime = 0;
			}
		}else{
			currentHoptime = 0;
		}
	}
	
}
