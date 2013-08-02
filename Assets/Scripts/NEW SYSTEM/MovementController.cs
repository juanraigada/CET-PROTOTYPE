using UnityEngine;
using System.Collections;

public class MovementController : MonoBehaviour {
	
	public string controllerName;
	public int numberOfOptions;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public virtual void MovementUpdate () {
	
	}
	
	public virtual void DrawGUIControls(Rect rect){}
}
