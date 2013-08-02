using UnityEngine;
using System.Collections;


//This class catches all input calls from the unity input system. It is good practise to use a middle layer of script like this one between the controller inputs and the game logic.

//Please note that this class is mostly useful for digital button input. It's not really necessary to implement catching the analog axis, but it is good practise to encapsulate all input in our own class.

//We will be calling specific buttons that we have previously named in Unity's Project Settings -> Input
public class InputManager : MonoBehaviour {
	//This is the list of all posible inputs in out.
	public enum inputTypes {JUMP,UP,DOWN,LEFT,RIGHT};
	//And this is a list of the names in Unity's input manager. The relationship between these two variables is what maps one to the other.
	string[] trueInputNames = {"Jump","Up","Down","Left","Right"};
	
	//This is a struct that stores the state of each button.
	public struct inputState {
		/*These two variables serve as the layer between:
			inputTypes -> our code 
			trueInputName -> Unity's input system.
		*/
		public int type;
		public string trueInputName;
		/*These variables store:
		 	Status of the button press.
		 	Time it was pressed.
		*/
		public bool isActive;
		public float lastTimePressed;
	}
	
	//And here we store our input states.
	static public inputState[] inputs;
	
	//This is the time a positive input will remain positive once the button is depressed.
	float bufferTime = 0.05f;	
	
	// We initialize each input.
	void Start () {
		inputs = new inputState[trueInputNames.Length];
		for(int i = 0; i < trueInputNames.Length ; i += 1){
			inputState toPut = new inputState();
			toPut.type = i;
			toPut.trueInputName = trueInputNames[i];
			toPut.isActive = false;
			toPut.lastTimePressed = 0;
			inputs[i] = toPut;
		}
	}
	
	// Input code will be in LateUpdate to ensure the player code can access the variables before the input system deletes them. Alternatively, we can use Unity's Project Settings -> Script execution order to force the input to update after the character controller.
	void Update () {
		//Now we check for each button wether it's down
		for(int i = 0 ; i < trueInputNames.Length ; i += 1){
			if(Input.GetButton(inputs[i].trueInputName)){
			//If the button is pressed, set the bool to true AND set the new time stamp
				inputs[i].isActive = true;
				inputs[i].lastTimePressed = 0;
			}else if(inputs[i].isActive){
				inputs[i].lastTimePressed += Time.deltaTime;
			//Else, check if the difference between the time and the timestamp is bigger than the buffer and deactivate it.
				if(inputs[i].lastTimePressed > bufferTime){
					inputs[i].isActive = false;
				}
			}
		}
	}
	
	//TODO: Axis Wrapper
}
