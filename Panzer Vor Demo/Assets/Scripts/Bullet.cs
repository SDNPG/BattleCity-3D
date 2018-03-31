using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    //炮弹自有属性
    public float Speed;//炮弹飞行速度
    public float Penetration;//穿深
    public float PenetrationAttenuation;//炮弹衰减系数
    public float Caliber;//口径
    public float CorrectionAngle=0f;//转正角度
    public bool IsFirst = true;//是否未发生判定
    public GameObject Empty;//空物体
    public GameObject UI_Manager;//UI信息接收
    public float RicochetAngle;//跳弹角度

    //内部计算相关
    private float angle;
    private float ArmorValue;
    private float distance;

    //定义所属
    public int ShellType;//定义弹种：1AP/2APCR/3HE/4HEAT/5HESH


    //炮弹传输数据
    private GameObject master;//炮弹所有者
    private GameObject atted;//被击者
    private Vector3 Start_Location;//炮弹起点
    private Vector3 Final_Location;//炮弹终点


    // Use this for initialization
    void Awake() {
        Destroy(gameObject, 200f/Speed);
    }
    

    
    // Update is called once per frame
    void Update()
    {
        if (Tank.outgame)
            return;
        transform.position += transform.forward * Speed * Time.deltaTime;//向前飞行

    }
    //碰撞器
   // private void OnCollisionEnter(Collision collision)
    
       // print("oncollisionenter");
    
    private void OnTriggerEnter(Collider collision)
    {
        atted = collision.gameObject;
        //if (collision.gameObject.GetComponentInParent<Tank>().ctrlType == Tank.CtrlType.player)
        //return;
        Final_Location = gameObject.transform.position;//获取终点位置
        distance = Vector3.Distance(Start_Location, Final_Location);//计算炮弹飞行距离
        Penetration -= distance * PenetrationAttenuation/100;//穿深递减
        Start_Location = Final_Location;//更新计算穿深衰减的起点



        switch (collision.tag)
        {
            //可摧毁的墙壁
            case "wall":

                Penetration -= 20;
                Debug.Log(Penetration);
                if (Penetration > 0)
                {
                    atted.SendMessage("Die");
                }
                if (Penetration < 0)
                    Destroy(gameObject);
                if (Penetration == 0)
                {
                    Destroy(gameObject);
                    atted.SendMessage("Die");

                }
                break;
            //无法摧毁的墙壁
            case "barriar":
                Destroy(gameObject);

                Debug.Log("barriar");
                break;
            //装甲模型“横向”
            case "C_armor":
                print(atted);
                //敌我判定
                if (atted.gameObject.GetComponentInParent<Tank>().ctrlType == Tank.CtrlType.Ally)                
                    master.SendMessage("Sound_hurtAlly");//痛击友军                

                //入射角计算
                //GameObject a = Instantiate(Empty, gameObject.transform.position, gameObject.transform.rotation);
                //GameObject b = Instantiate(Empty, gameObject.transform.position, gameObject.transform.rotation);
                //GameObject c = Instantiate(Empty, gameObject.transform.position, gameObject.transform.rotation);

               // a.transform.SetParent(collision.gameObject.transform);
               // a.transform.localPosition = new Vector3(1, 0, 0);
               // b.transform.SetParent(collision.gameObject.transform);
              //  b.transform.localPosition = new Vector3(0, 0, 0);
              //  c.transform.SetParent(collision.gameObject.transform);
              //  c.transform.localPosition = new Vector3(0, 0, 1);


                angle = Mathf.Acos(Vector3.Dot(gameObject.transform.forward, collision.transform.forward)) * Mathf.Rad2Deg;
                if (angle > 90)
                    angle = 180 - angle;

                //获取被击装甲的绝对厚度
                ArmorValue = atted.gameObject.GetComponent<Property>().ArmorValue;

                //跳弹判定
                if (IsFirst)//间隙装甲内不会跳弹
                    if (Caliber < 3 * ArmorValue)//三倍碾压机制
                        if (angle < RicochetAngle) 
                        {
                            Ricochet();//跳弹
                            IsFirst = false;
                            break;
                        }

                //穿深浮动计算
                if (IsFirst)//间隙装甲内不会再次计算浮动
                    Penetration = Random.Range(0.75f, 1.25f) * Penetration;//进行25&上下的浮动计算


                //转正效应
                CorrectionAngle = 0f;
                if (ShellType == 1)
                    CorrectionAngle = 5f;
                if (ShellType == 2)
                    CorrectionAngle = 2f;

                //二倍转正增加机制
                if (Caliber >= 2 * ArmorValue)
                {
                    CorrectionAngle = 1.4f * CorrectionAngle * Caliber / ArmorValue;
                }

                //等效装甲计算
                if (angle > 90 - CorrectionAngle)
                    CorrectionAngle = 90 - angle;
                ArmorValue = ArmorValue / Mathf.Sin(Mathf.PI * (angle + CorrectionAngle) / 180);

                //击穿判定
                if (Penetration >= ArmorValue)//大于的场合
                {
                    //区分是否为间隙装甲
                    if (atted.gameObject.GetComponent<Property>().LastArmor)//最后一层的场合
                    {
                        L_OUTPUT();//发送第二层的信息
                        Penetrate();//有效击穿
                    }
                    if (!atted.gameObject.GetComponent<Property>().LastArmor)
                    {
                        F_OUTPUT();//发送第一层的信息
                        Penetration -= ArmorValue;//扣除击穿第一层的等效装甲厚度
                        IsFirst = false;
                        Tank.F_result = true;
                    }
                
                }
                //我们未能穿透他们的装甲
                if (Penetration < ArmorValue)
                {
                    if (atted.gameObject.GetComponent<Property>().LastArmor)//第二层的场合
                        L_OUTPUT();//发给第二层UI
                    if (!atted.gameObject.GetComponent<Property>().LastArmor)
                        F_OUTPUT();//发给第一层UI
                    if (atted.gameObject.GetComponentInParent<Tank>().ctrlType != Tank.CtrlType.Ally)//是队友就不发声
                        master.SendMessage("Sound_noPenetrate");
                }

                if (atted.gameObject.GetComponent<Property>().LastArmor)
                    Destroy(gameObject);


                break;


            //装甲模型“纵向”
            case "T_armor":
                break;
            case "bullet":

                break;
        }



    }

    //跳弹
    private void Ricochet()
    {
        Penetration = 0.8f * Penetration;//因跳弹而造成的穿深衰减

        //if (atted.transform.forward - gameObject.transform.forward > 0)
        

        

        gameObject.transform.Rotate(0f, 2 * angle, 0f);//改变炮弹飞行方向
        if (atted.gameObject.GetComponent<Property>().LastArmor)//第二层的场合
            L_OUTPUT();
        if (!atted.gameObject.GetComponent<Property>().LastArmor)//第一层
            F_OUTPUT();
        if (atted.GetComponentInParent<Tank>().ctrlType != Tank.CtrlType.Ally)
            if (IsFirst)//第一次判定才能跳弹
                master.SendMessage("Sound_ricochet");
            
        
    }

    //击穿处理
    private void Penetrate()
    {
        if (atted.GetComponentInParent<Tank>().ctrlType != Tank.CtrlType.Ally)//非友军的场合
                master.SendMessage("Sound_penetrate");
    }

    //综合信息发送
    private void L_OUTPUT()
    {
        master.SendMessage("UI_penetrate", Penetration);
        master.SendMessage("UI_angle", 90 - angle);
        master.SendMessage("UI_correctionangle", CorrectionAngle);
        master.SendMessage("UI_armor", ArmorValue);
        master.SendMessage("UI_distance", distance);
    }
    private void F_OUTPUT()
    {
        master.SendMessage("FUI_penetrate", Penetration);
        master.SendMessage("FUI_angle", 90 - angle);
        master.SendMessage("FUI_correctionangle", CorrectionAngle);
        master.SendMessage("FUI_armor", ArmorValue);
        master.SendMessage("FUI_distance", distance);
    }

    /// <summary>
    /// //////////////////信息接收
    /// </summary>

    //接收发射者信息
    private void Master(GameObject Fgameobject)
    {
        master = Fgameobject;
    }
    //接收炮弹起始位置信息
    private void Location(Vector3 Fgameobeject)
    {
        Start_Location = Fgameobeject;
    }
    
}
