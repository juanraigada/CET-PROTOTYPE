using UnityEngine;
using System.Collections;

/********************************************************************************
 * 	CONTROLS THE USE OF SOUND EFFECTS
 *	Sound effects are visceral and greatly enhance a game feel. 
 *However, they can be used as crutches sometimes. If you can make your game work 
 *without sound, it will be the better, so sometimes it's counter productive to 
 *implement sound too early...
 * ******************************************************************************/
public class SoundPolishController : PolishController {

	// Use this for initialization
	void Start () {
		polishName = "Sound";
		numberOfOptions = 3;
	}
	
		/****************************
		 * SOUND:	
		 * *************************/
	//Play sound on jump?
	bool playJumpSound = true;
	// play Sound on Land?
	bool playLandSound = true;
	//Auxiliary flag to detect a collision without having to go into the collision functions, to keep this part of the code self-reliable.
	//Note tghe order we call these functions in our main Polish function so that this variable behaves consistently.
	bool wasJumpingForSound = false;
	
	//AudioSurce to play the saounds with. To assign in editor.
	public AudioSource audioSource;
	//Clips to play
	public AudioClip jumpSound;
	public AudioClip landSound;
	public float jumpSoundVolume;
	public float landSoundVolume;
	
	public override void DoPolish(){
		if(playJumpSound){
			PlayJumpSound();
		}
		if(playLandSound){
			PlayLandSound();
		}
		if(!MovementManager.isJumping){
			wasJumpingForSound = false;
		}else{
			wasJumpingForSound = true;
		}
	}
	
	void PlayJumpSound(){
		if(MovementManager.isJumping&&!wasJumpingForSound){
			audioSource.clip = jumpSound;
			audioSource.volume = jumpSoundVolume;
			audioSource.Play ();
		}
	}
	
	void PlayLandSound(){
		if(!MovementManager.isJumping && wasJumpingForSound){
			audioSource.clip = landSound;
			audioSource.volume = landSoundVolume;
			audioSource.Play ();
		}
	}
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		playLandSound = 			GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/2,MovementManager.controlHeight),playLandSound,"Play Sound on Land?");
		playJumpSound = 			GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/2,MovementManager.controlHeight),playJumpSound,"Play Sound on Jump?");
	}
}
