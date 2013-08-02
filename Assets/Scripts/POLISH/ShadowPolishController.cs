using UnityEngine;
using System.Collections;

/********************************************************************************
 * 	CREATES A SHADOW BELOW THE BALL 
 *	Actually, the script doesn't create anything. The shadow
 *	is created and assigned in-editor as a blob shadow projector
 *	This script only modifies one variable of the shadow. It is left as an 
 *	exercise to also add controllers to change the farclipplane of the projector.
 * ******************************************************************************/

public class ShadowPolishController : PolishController {
	
	public LayerMask toIgnore;
	
	//Exposed public variable to set the projector (an Unity component used for blob shadows among other things) in the editor.
	public Projector projector;
	
	// Use this for initialization
	void Start () {
		polishName = "Shadow";
		numberOfOptions = 3;
	}
	
		/****************************
		 * SHADOW:	
		 * *************************/
	//Will we draw the shadow?
	bool useShadow = true;
	//Will the shadow change size according to height?
	bool shadowSizeChange = true;
	
	public override void DoPolish(){
		if(useShadow){
			projector.gameObject.SetActive(true);
			if(shadowSizeChange){
				RaycastHit hit = new RaycastHit();
				if (Physics.Raycast (projector.transform.position, projector.transform.position-projector.transform.up,out hit,projector.farClipPlane,toIgnore)) {
					projector.orthoGraphicSize = 0.6f - (hit.point - projector.transform.position).magnitude/projector.farClipPlane*0.6f;
				}else{
					projector.orthoGraphicSize = 0;
				}
			}else{
				projector.orthoGraphicSize = 0.6f;
			}
		}else{
			projector.gameObject.SetActive(false);
		}
	}
	
		/*********************
		 * GUI CONTROLS: 
		 * ******************/
	public override void DrawGUIControls(Rect rect){
		useShadow = 				GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight,Screen.width/2,MovementManager.controlHeight),useShadow,"Use Shadow?");
		shadowSizeChange = 				GUI.Toggle(new Rect(0,MovementManager.controlHeight*3 + MovementManager.controlHeight*2,Screen.width/2,MovementManager.controlHeight),shadowSizeChange,"Will the shadow change size?");
	}
}
