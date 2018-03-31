using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelMgr : MonoBehaviour {

    public Text Armor;

    public GameObject UI_help;
    public GameObject O_UI;
    public GameObject target1;
    public GameObject target2;
    public GameObject target3;

    public static bool isdouble = false;
    public static bool isricochet = false;

    public void OnPanel1BtnClick()
    {
        SetArmor(25, "25mm");
        ClosePanel();

    }

    public void OnPanel2BtnClick()
    {
        SetArmor(35, "35mm");
        ClosePanel();

    }

    public void OnPanel3BtnClick()
    {
        SetArmor(45, "45mm");
        ClosePanel();

    }

    public void OnPanel4BtnClick()
    {
        SetArmor(100, "100mm");
        ClosePanel();

    }

    public void OnPanel5BtnClick()
    {
        if (isdouble)
        {
            target2.SetActive(false);
            O_UI.gameObject.SetActive(false);
            isdouble = false;
        }
        else if (!isdouble)
        {
            target2.SetActive(true);
            O_UI.gameObject.SetActive(true);
            isdouble = true;

        }
        ClosePanel();

    }

    public void OnPanel6BtnClick()
    {
        if (isricochet)
        {
            target3.SetActive(false);
            isricochet = false;
        }
        else if (!isricochet)
        {
            target3.SetActive(true);
            isricochet = true;
        }
        ClosePanel();

    }

    public void OnPanelHelpBtnClick()
    {
        UI_help.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
    public void OnPanelExitBtnClick()
    {
        Application.Quit();
    }

    //设置装甲厚度
    private void SetArmor(int a,string b)
    {
        target1.gameObject.GetComponent<Property>().ArmorValue = a;
        Armor.GetComponent<Text>().text = b;
    }

    //关闭窗口
    private void ClosePanel()
    {
        gameObject.SetActive(false);
        Tank.outgame = false;
        Cursor.visible = false;
    }

}