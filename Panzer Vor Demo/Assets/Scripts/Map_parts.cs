using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map_parts : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.P))
            Destroy(gameObject);

	}

    void Die()
    {
        Destroy(gameObject);
    }
}
