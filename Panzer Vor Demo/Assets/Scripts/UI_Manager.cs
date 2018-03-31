using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour {

    public Text ShellTypeValue;//弹种UI
    public Text CaliberValue;//口径UI
    public Text S_Penetration;//平均穿深

    public Text PenetrationValue;//穿深UI
    public Text AngleValue;//入射角UI
    public Text CorretionAngleValue;//转正角度UI
    public Text ArmorValue;//等效装甲UI
    public Text DistanceValue;//飞行距离UI
    public Text ReturnValue;//结果UI

    public GameObject mainmeun;//主菜单

    public Text F_PenetrationValue;//穿深UI
    public Text F_AngleValue;//入射角UI
    public Text F_CorretionAngleValue;//转正角度UI
    public Text F_ArmorValue;//等效装甲UI
    public Text F_DistanceValue;//飞行距离UI
    public Text F_ReturnValue;//结果UI

    // Use this for initialization
    void Start() {
        

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShellTypeValue.GetComponent<Text>().text = "AP";
            S_Penetration.GetComponent<Text>().text = "86";
            CaliberValue.GetComponent<Text>().text = Tank.Caliber.ToString();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShellTypeValue.GetComponent<Text>().text = "APCR";
            S_Penetration.GetComponent<Text>().text = "102";
            CaliberValue.GetComponent<Text>().text = Tank.Caliber.ToString();
        }
        // (Input.GetKeyDown(KeyCode.Alpha3))
        {
            //ShellTypeValue.GetComponent<Text>().text = "HE";
            //S_Penetration.GetComponent<Text>().text = "38";
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            if (!Tank.outgame)        
            {
                mainmeun.gameObject.SetActive(true);
                Tank.outgame = true;
                Cursor.visible = true;
            }
        
                

        
    }


    private void Output_ricochet()//跳弹
    {
        if (PanelMgr.isdouble)//间隙装甲的场合
        {
            F_PenetrationValue.GetComponent<Text>().text = Tank.F_Penetrate.ToString();
            F_AngleValue.GetComponent<Text>().text = Tank.F_Angle.ToString();
            F_CorretionAngleValue.GetComponent<Text>().text = "/////";
            F_ArmorValue.GetComponent<Text>().text = "/////";
            F_DistanceValue.GetComponent<Text>().text = Tank.F_Distance.ToString();
            F_ReturnValue.GetComponent<Text>().text = "跳弹";
            
        }

        PenetrationValue.GetComponent<Text>().text = Tank.Penetrate.ToString();
        AngleValue.GetComponent<Text>().text = Tank.Angle.ToString();
        CorretionAngleValue.GetComponent<Text>().text = "/////";
        ArmorValue.GetComponent<Text>().text = "/////";
        DistanceValue.GetComponent<Text>().text = Tank.Distance.ToString();
        if (PanelMgr.isdouble)
            ReturnValue.GetComponent<Text>().text = "///////";
        if (!PanelMgr.isdouble)
            ReturnValue.GetComponent<Text>().text = "跳弹";

    }
    private void Output_penetrate()//击穿
    {
        if (PanelMgr.isdouble)//间隙装甲
        {
            F_PenetrationValue.GetComponent<Text>().text = Tank.F_Penetrate.ToString();
            F_AngleValue.GetComponent<Text>().text = Tank.F_Angle.ToString();
            F_CorretionAngleValue.GetComponent<Text>().text = Tank.F_CorrectionAngle.ToString();
            F_ArmorValue.GetComponent<Text>().text = Tank.F_Armor.ToString();
            F_DistanceValue.GetComponent<Text>().text = Tank.F_Distance.ToString();
            if (Tank.F_result)
                F_ReturnValue.GetComponent<Text>().text = "击穿";
            if (!Tank.F_result)
                F_ReturnValue.GetComponent<Text>().text = "未能击穿";
        }

        PenetrationValue.GetComponent<Text>().text = Tank.Penetrate.ToString();
        AngleValue.GetComponent<Text>().text = Tank.Angle.ToString();
        CorretionAngleValue.GetComponent<Text>().text = Tank.CorrectionAngle.ToString();
        ArmorValue.GetComponent<Text>().text = Tank.Armor.ToString();
        DistanceValue.GetComponent<Text>().text = Tank.Distance.ToString();
        ReturnValue.GetComponent<Text>().text = "击穿";


    }
    private void Output_nopenetrate()//未击穿
    {
        if (PanelMgr.isdouble)//间隙装甲
        {
            F_PenetrationValue.GetComponent<Text>().text = Tank.F_Penetrate.ToString();
            F_AngleValue.GetComponent<Text>().text = Tank.F_Angle.ToString();
            F_CorretionAngleValue.GetComponent<Text>().text = Tank.F_CorrectionAngle.ToString();
            F_ArmorValue.GetComponent<Text>().text = Tank.F_Armor.ToString();
            F_DistanceValue.GetComponent<Text>().text = Tank.F_Distance.ToString();
            if (Tank.F_result)
                F_ReturnValue.GetComponent<Text>().text = "击穿";
            if (!Tank.F_result)
                F_ReturnValue.GetComponent<Text>().text = "未能击穿";

        }

        PenetrationValue.GetComponent<Text>().text = Tank.Penetrate.ToString();
        AngleValue.GetComponent<Text>().text = Tank.Angle.ToString();
        CorretionAngleValue.GetComponent<Text>().text = Tank.CorrectionAngle.ToString();
        ArmorValue.GetComponent<Text>().text = Tank.Armor.ToString();
        DistanceValue.GetComponent<Text>().text = Tank.Distance.ToString();
        ReturnValue.GetComponent<Text>().text = "未能击穿";

    }

    private void UIChange(string a,string b,string c,string d,string e,string f)
    {

    }


}
