using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Follow : MonoBehaviour {

    private float Camera_distance = 15;
    //private float Target_distance = 15;
    private float Air_distance = 15;

    private float rot = 0;
    private float roll= 30f * Mathf.PI * 2 / 360;

    //private float zoomSpeed = 0.2f;
    private float rotSpeed = 0.2f;
    private float rollSpeed = 0.2f;

    public float tp_maxDistance = 22f;//第三人称视角缩放最远距离
    public float tp_minDistance = 5f;//第三人称视角缩放最近距离
    public float air_maxDistance = 35f;//鸟瞰缩放最近距离
    public float air_minDistance = 22f;//鸟瞰缩放最近距离
    private float maxRoll = 70f * Mathf.PI * 2 / 360;//纵向角度范围
    private float minRoll = -1f * Mathf.PI * 2 / 360;//纵向角度范围


    public GameObject N_target;//定义目标
    public GameObject S_target;//定义目标
    public GameObject S_pos;//定义目标
    public GameObject Gun;

    public bool Airview = false;//定义是否为鸟瞰视角
    public static bool Snipeview = false;//定义是否为狙击视角



    void Start ()
    {
        //N_target = GameObject.Find("N_CameraTarget");
        //S_pos = GameObject.Find("S_CameraPos");
        //S_target = GameObject.Find("S_CameraTarget");
    }

    // Update is called once per frame

    private void LateUpdate()
    {
        if (Tank.outgame)
            return;

        
        Rotate();

        if (Airview)
            air_Follow();

        else if (Snipeview)
            snipe_Follow();

        else tp_Follow();

    }
    private void Update()
    {
        if (Tank.outgame)
            return;
        if (Input.GetKeyDown(KeyCode.LeftShift))//进入狙击视角
            if (Snipeview)
                Snipeview = false;
            else Snipeview = true;
        if (Input.GetKeyDown(KeyCode.Q))//进入鸟瞰视角
            if (Airview)
            {
                Airview = false;
                Snipeview = false;
            }
            else Airview = true;
    }
    //环视函数
    private void Rotate()
    {
        float w = Input.GetAxis("Mouse X") * rotSpeed;
        float v = Input.GetAxis("Mouse Y") * rollSpeed;
        rot -= w;
        roll -= v;
        if (roll > maxRoll)
            roll = maxRoll;
        if (roll < minRoll)
            roll = minRoll;


    }

    //第三视角
    private void tp_Follow()
    {
        Vector3 targetpos = S_pos.transform.position;//参照物坐标
        Vector3 Pos;
        //计算与参照物的相对位置
        float d = Camera_distance * Mathf.Cos(roll);
        float height = Camera_distance * Mathf.Sin(roll);
        //实现
        Pos.x = targetpos.x + d * Mathf.Cos(rot);
        Pos.z = targetpos.z + d * Mathf.Sin(rot);
        Pos.y = targetpos.y + height;
        Camera.main.transform.position = Pos;
        Camera.main.transform.LookAt(N_target.transform);

    }
    //鸟瞰视角
    private void air_Follow()
    {
        Vector3 targetpos = N_target.transform.position;//参照物坐标
        Vector3 Pos;
        //实现
        Pos.x = targetpos.x;
        Pos.z = targetpos.z;
        Pos.y = targetpos.y + Air_distance;
        Camera.main.transform.position = Pos;
        Camera.main.transform.LookAt(N_target.transform);

    }
    //狙击视角
    private void snipe_Follow()
    {
        //相机位置
        Vector3 targetPos = S_pos.transform.position;
        //计算相机的坐标
        Camera.main.transform.position = targetPos;
        //Camera.main.transform.rotation = Gun.transform.rotation;
        Camera.main.transform.LookAt(S_target.transform);

    }
}
