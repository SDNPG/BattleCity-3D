using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_instruction : MonoBehaviour {


	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0) | Input.GetKeyDown(KeyCode.Escape)) 
        {
            ClosePanel();
        }
	}
    void ClosePanel()
    {
        gameObject.SetActive(false);
        Tank.outgame = false;
        Cursor.visible = false;
    }
}
