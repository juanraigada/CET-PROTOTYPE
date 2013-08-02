using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	
	public Transform target;
	
	float interpolationStrength = 10;
	float targetVerticalOffSet = 3;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.rotation = Quaternion.Slerp (transform.rotation,Quaternion.LookRotation(target.position + target.up*targetVerticalOffSet - transform.position,transform.up),interpolationStrength*Time.deltaTime);
	}
}
