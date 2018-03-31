using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_targetPos : MonoBehaviour {

    private GameObject Target;

    // Use this for initialization

    void Start () {
        Target = GameObject.Find("S_CameraTarget");
	}
	
	// Update is called once per frame
	void LateUpdate () {
        gameObject.transform.LookAt(Target.transform);
	}
}
