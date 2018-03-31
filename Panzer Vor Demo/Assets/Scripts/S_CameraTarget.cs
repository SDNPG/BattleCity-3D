using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_CameraTarget : MonoBehaviour
{

    private float roll = 30f * Mathf.PI * 2 / 360;
    private float rot = 0;
    private float Camera_distance = 15;
    private float rotSpeed = 0.2f;
    private float rollSpeed = 0.2f;
    private float maxRoll = 4f * Mathf.PI * 2 / 360;//纵向角度范围
    private float minRoll = -10f * Mathf.PI * 2 / 360;//纵向角度范围

    public GameObject S_pos;//定义目标

    // Use this for initialization

    void Start()
    {

    }


    // Update is called once per frame
    void LateUpdate()
    {
        if (Tank.outgame)
            return;
        Rotate();
        Follow();



    }
    private void Rotate()
    {
        float w = Input.GetAxis("Mouse X") * rotSpeed;
        float v = Input.GetAxis("Mouse Y") * rollSpeed;
        rot -= w;
        roll += v;
        if (roll > maxRoll)
            roll = maxRoll;
        if (roll < minRoll)
            roll = minRoll;


    }

    private void Follow()
    {
        Vector3 targetpos = S_pos.transform.position;
        Vector3 Pos;

        float d = Camera_distance * Mathf.Cos(roll);
        float height = Camera_distance * Mathf.Sin(roll);
        Pos.x = targetpos.x - d * Mathf.Cos(rot);
        Pos.z = targetpos.z - d * Mathf.Sin(rot);
        Pos.y = targetpos.y + height;
        gameObject.transform.position = Pos;
    }

}
