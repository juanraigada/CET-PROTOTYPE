using UnityEngine;
using System.Collections;

public class PolishController : MonoBehaviour {

	public string polishName;
	public int numberOfOptions;
	
	public virtual void DoPolish(){}
	
	public virtual void DrawGUIControls(Rect rect){}
}
