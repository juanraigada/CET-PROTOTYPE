using UnityEngine;
using System.Collections;

/********************************************************************************
 * 	CONTROLS THE USE OF PARTICLES
 *	Particles are an extremely useful and effective form of feedback.
 *	Since particles very seldomly interact in a meaningful way, they tend to be just polish.
 * ******************************************************************************/
public class ParticlePolishController : PolishController {
	
		//variable that points to the visual representation of the ball.
	public Transform ball;
	
	// Use this for initialization
	void Start () {
		polishName = "Particles";
		numberOfOptions = 3;
	}
	
		/****************************
		 * PARTICLES:	
		 * *************************/
	//Ground hit particles. To create and assign in the editor.
	public ParticleSystem groundHitparticles;
	public ParticleSystem jumpParticles;
	//These variables tell the ball wheter to play particles or not.
	bool polishJumpWithParticle = true;
	bool polishLandWithParticle = true;
	//Auxiliary flag to detect a collision without having to go into the collision functions, to keep this part of the code self-reliable.
	//Note tghe order we call these functions in our main Polish function so that this variable behaves consistently.
	bool wasJumpingForParticles = false;
	
	public override void DoPolish(){
		if(polishLandWithParticle){
			PlayGroundHitParticles();
		}
		if(polishJumpWithParticle){
			PlayJumpParticles();
		}
		if(!MovementManager.isJumping){
			wasJumpingForParticles = false;
		}else{
			wasJumpingForParticles = true;
		}
	}
	
	//Play the particles when colliding with the ground. We could call this from collision detection as you probably would want in a real project. However, we use flag for code independency.
	void PlayGroundHitParticles(){
			if(!MovementManager.isJumping && wasJumpingForParticles){
				groundHitparticles.transform.position = transform.position - MovementManager.currentCollisionNormal/2;
				groundHitparticles.transform.rotation = Quaternion.LookRotation (MovementManager.currentCollisionNormal,transform.forward);
				groundHitparticles.Play ();
			}
	}
	
	//Play the jump particles. We should call this funciton at input detection, but, as for the previous one, we are using flags so we can maintain independency. Don't do this in a real project.
	void PlayJumpParticles(){
			if(MovementManager.isJumping&&!wasJumpingForParticles){
				jumpParticles.transform.position = transform.position - MovementManager.currentCollisionNormal/2;
				jumpParticles.transform.rotation = ball.rotation;
				jumpParticles.Play ();
			}
	}
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		polishLandWithParticle = 			GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/2,MovementManager.controlHeight),polishLandWithParticle,"Play Particles on Land?");
		polishJumpWithParticle = 			GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/2,MovementManager.controlHeight),polishJumpWithParticle,"Play Particles on Jump?");
	}
}
