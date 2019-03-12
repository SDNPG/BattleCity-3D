using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Main : MonoBehaviour {


    public static GameObject Target_Nomal;
    public static GameObject Target_Snip;
    public GameObject Target;
    public static bool snipSights = false;//是否为狙击视角
    public static bool lockedSights = false;//是否锁定视角

    private float Distance = 15;//摄像机到目标物体的距离

    public static float rot = 0;//横向角度
    public static float roll = 30f * Mathf.PI * 2 / 360;//纵向角度
    public static float Toll = 30f * Mathf.PI * 2 / 360;
    private float rotspeed = 0.2f;//横向速度
    private float rollspeed = 0.2f;//纵向速度

    private float maxDistance = 22f;//视角缩放最远距离
    private float minDistance = 5f;//视角缩放最近距离
    private float zoomSpeed = 0.2f;//缩放速度
    private float maxRoll = 70f * Mathf.PI * 2 / 360;//纵向角度范围
    private float minRoll = -10f * Mathf.PI * 2 / 360;//纵向角度范围    
    private float maxToll = 70f * Mathf.PI * 2 / 360;//纵向角度范围
    private float minToll = -20f * Mathf.PI * 2 / 360;//纵向角度范围

    private bool ChangeS = true;



    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetAxisRaw("SnipSights") == 1) 
        {
            
        }
	}
    private void LateUpdate()
    {
        if (Target_Nomal == null)       
            return;


        Follow();
        


    }



    private void Follow()
    {
        //缩放镜头改变摄像机与目标物体的距离
        if (Input.GetAxis("Mouse ScrollWheel") > 0) if (Distance > minDistance) Distance -= zoomSpeed;
        if (Input.GetAxis("Mouse ScrollWheel") < 0) if (Distance < maxDistance) Distance += zoomSpeed;

        //获得鼠标在XY上的偏移量增减
        float w = Input.GetAxis("Mouse X") * rotspeed;
        float v = Input.GetAxis("Mouse Y") * rollspeed;
        //输入反转
        if (snipSights)
        {
            rot -= w / 10f;
            roll += v / 10f;
            Toll += v / 10f;
        }
        else
        {
            rot -= w;
            roll -= v;
            Toll += v;
        }
        //不超过最大范围
        if (roll > maxRoll) roll = maxRoll;
        if (roll < minRoll) roll = minRoll;
        if (Toll > maxToll) Toll = maxToll;
        if (Toll < minToll) Toll = minToll;


        //获得目标的位置
        Vector3 targetpos = Target_Nomal.transform.position;

        //计算摄像机相对目标的位置
        float d = Distance * Mathf.Cos(roll);
        float height = Distance * Mathf.Sin(roll);
        Target.transform.position = new Vector3(targetpos.x - d * Mathf.Cos(rot), targetpos.y + Distance * Mathf.Sin(Toll), targetpos.z - d * Mathf.Sin(rot));


        //更新相机位置        
        if (snipSights)
        {
            if (!ChangeS) ChangeSights();
            Camera.main.transform.position = Target_Snip.transform.position;
            Camera.main.transform.LookAt(Target.transform);
        }
        else
        {
            if (ChangeS) ChangeSights();
            Camera.main.transform.position = new Vector3(targetpos.x + d * Mathf.Cos(rot), targetpos.y + height, targetpos.z + d * Mathf.Sin(rot));
            Camera.main.transform.LookAt(Target_Nomal.transform);
        }
    }

    void ChangeSights()
    {
        if (!ChangeS)
        {
            ChangeS = true;

        }
        else
        {
            ChangeS = false;

        }

    }

}
