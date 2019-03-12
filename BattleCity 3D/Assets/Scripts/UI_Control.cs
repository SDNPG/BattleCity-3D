using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Control : MonoBehaviour {

    public Text ShellType;//弹种UI


    // Use this for initialization
    void Start () {
        A_ShellType(0);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// 触发型函数
    /// </summary>
    public void A_ShellType(int A)
    {
        switch (A)
        {
            case 0:
                ShellType.GetComponent<Text>().text = "弹种：AP";
                break;
            case 1:
                ShellType.GetComponent<Text>().text = "弹种：APCR";
                break;
            case 2:
                ShellType.GetComponent<Text>().text = "弹种：HE";
                break;
        }
    }



}
