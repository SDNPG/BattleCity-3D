using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {

    private bool locked = true;
    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
	}

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.O)) 
        {
            if (locked)
            {
            gameObject.transform.Rotate(new Vector3(1f, 0, 0));
                locked= false;

            }
        }

    }






}
