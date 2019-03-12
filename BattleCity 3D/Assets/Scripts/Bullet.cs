using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    #region 可设置变量
    //炮弹可设置属性变量
    public float Speed;//炮弹飞行速度
    public float Penetration;//穿深
    public float PenetrationAttenuation_A;//炮弹百米穿深衰减系数
    public float PenetrationAttenuation_B;//化学弹爆炸后衰减系数
    public float Caliber;//炮弹口径
    public float CorrectionAngle;//炮弹转正角度
    public float RicochetAngle;//炮弹跳弹角度
    public int SDamage;//炮弹标准平均伤害
    public int EDamage;//炮弹额外伤害

    public enum shelltype
    {
        AP, APCR, HE, PIECE
    }
    public shelltype ShellType;
    public GameObject UI_Control;

    #endregion

    #region 内部计算变量
    //计算的变量
    private bool IsFirst = true;//是否已经发生过判定（穿深浮动只计算一次）
    private bool IsFresh = true;//是否穿透（该计算内部部件伤害了）
    private GameObject Damage_Judge;//定义伤害判定处理单位
    private GameObject Damage_Caculate;//定义伤害计算

    //内部计算相关变量
    private Vector3 Start_Location;//炮弹起点坐标
    private Vector3 Final_Location;//炮弹终点坐标
    private float N_CorrectionAngle;//炮弹实际转正角度
    private Vector3 Ray_startPos;//射线起点
    private Tank P_tank;
    private Property property;
    private Vector3 AttedForward;

    //数据传输相关
    private float Angle;
    private float ArmorValue;
    private float Distance;
    private GameObject attacker;
    private GameObject atted;
    private GameObject attedParent;

    #endregion

    // Use this for initialization
    void Start() {
        Start_Location = gameObject.transform.position;//更新起点位置
        Ray_startPos = gameObject.transform.position;//
    }

    // Update is called once per frame
    void Awake()
    {
        if (!IsFresh)
        {
            Destroy(gameObject, 6f / Speed);
        }
        else Destroy(gameObject, 100f / Speed);
    }

    private void FixedUpdate()
    {
        //自身移动
        transform.transform.position += transform.forward * Speed * Time.fixedDeltaTime;
        
        //碰撞判定
        float length = (transform.position - Ray_startPos).magnitude;//射线的长度
        Vector3 direction = transform.position - Ray_startPos;//方向
        RaycastHit hitinfo;
        bool isCollider = Physics.Raycast(Ray_startPos, direction, out hitinfo, length);
        Ray_startPos = hitinfo.point;
        if (isCollider) 
            {
            //确定被击者
            atted = hitinfo.collider.gameObject;
            property = atted.GetComponent<Property>();//获取被击者的Property脚本


            switch (property.type)
                {
                case Property.TYPE.wall://打到墙的场合
                    if (!IsFresh)//
                        return;
                    float HP = property.HP;
                    if (Penetration >= HP) 
                        {
                        Penetration -= 20;
                        Destroy(atted);
                        }
                    else
                        {
                        property.BeAttacked(Penetration);
                        Destroy(gameObject);
                        }
                        break;

                case Property.TYPE.barriar:
                    if (!IsFresh)
                        return;
                    if (Penetration > 100)
                    {
                        Destroy(atted);
                        Penetration -= 100;
                    }
                    else Destroy(gameObject);
                    break;

                case Property.TYPE.Armor:       
                    if (!IsFresh)
                    {
                        if (IsFirst)                        
                            IsFirst = false;                       
                        else Destroy(gameObject);
                        return;
                    }
                    attedParent = property.parent;//获取被击者本体信息
                    P_tank = attedParent.GetComponent<Tank>();//获取被击者本体的Tank脚本
                    #region 打中装甲模型

                    //敌我判定
                    if (P_tank.camp == attacker.GetComponent<Tank>().camp) 
                        {
                        print("命中友军");
                        }
                    float ArmorValue = property.ArmorValue;

                    //跳弹计算
                    AttedForward = atted.transform.forward;//获取被击面的法向量

                    AttedForward = Quaternion.AngleAxis(property.Angle_X, Vector3.right) * AttedForward;
                    AttedForward = Quaternion.AngleAxis(property.Angle_Y, Vector3.up) * AttedForward;
                    AttedForward = Quaternion.AngleAxis(property.Angle_Z, Vector3.forward) * AttedForward;
                    Angle = 180 - Mathf.Acos(Vector3.Dot(gameObject.transform.forward, AttedForward)) * Mathf.Rad2Deg;//计算入射角

                    if (Angle > RicochetAngle && Caliber < 3 * ArmorValue)   //满足跳弹条件

                    {
                        //gameObject.transform.position = Ray_startPos;
                        //Vector3 newdir = Vector3.Reflect(gameObject.transform.forward, AttedForward);//计算反弹的方向
                        //transform.rotation = Quaternion.LookRotation(-newdir);

                        //Ricochet();
                        break;
                        }

                    if (IsFirst) //穿透力浮动只计算一次
                        Penetration *= Random.Range(0.9f, 1.1f);//进行10%的上下浮动计算

                    //转正效应
                    N_CorrectionAngle = CorrectionAngle;

                    //二倍转正增加机制
                    if (Caliber >= 2 * ArmorValue)
                        N_CorrectionAngle = 1.4f * CorrectionAngle * Caliber / ArmorValue;

                    //等效装甲计算
                    if (Angle < N_CorrectionAngle)
                        N_CorrectionAngle = Angle;//防止出现0-90以外的角度
                    ArmorValue = ArmorValue / Mathf.Cos(Mathf.PI * (Angle - N_CorrectionAngle) / 180);
                    if (Penetration >= ArmorValue)
                    {
                        Penetrate();

                        Debug.Log("击穿");
                        IsFirst = false;
                        IsFresh = false;
                        Destroy(gameObject.GetComponent<MeshRenderer>());
                        Destroy(gameObject, 6f / Speed);

                    }
                    else
                    {
                        Debug.Log("我们未能穿透");

                    }
                    Destroy(gameObject);
                    break;

                #endregion
                case Property.TYPE.crew:
                    attedParent = property.parent;//获取被击者本体信息
                    P_tank = attedParent.GetComponent<Tank>();//获取被击者本体的Tank脚本
                    if (!IsFresh)//判断是否进入伤害计算阶段
                        {
                        P_tank.BeAttacked(EDamage * Random.Range(90, 110) / 100, attacker);
                        Debug.Log(atted.name);


                        }
                        break;

                case Property.TYPE.parts:
                    attedParent = property.parent;//获取被击者本体信息
                    P_tank = attedParent.GetComponent<Tank>();//获取被击者本体的Tank脚本
                    if (!IsFresh) 
                    {
                        P_tank.BeAttacked(EDamage * Random.Range(90, 110) / 100, attacker);
                        Debug.Log(atted.name);

                    }
                    break;

                case Property.TYPE.others:

                    break;
                }
          }
        Ray_startPos = transform.position;
        
    }

    private void Ricochet()
    {
        gameObject.transform.position = Ray_startPos;
        Vector3 newdir = Vector3.Reflect(gameObject.transform.forward, AttedForward);//计算反弹的方向
        transform.rotation = Quaternion.LookRotation(newdir);
        Penetration *= 0.8f;//穿透力降低

    }

    private void Penetrate()
    {
        P_tank.BeAttacked(SDamage * Random.Range(90, 110) / 100, attacker);
        int P = 0;
        GameObject LastPiece;
        switch (ShellType) {
            case shelltype.AP:
                P = 4;
                break;
            case shelltype.APCR:
                P = 3;
                break;
        }
        for (int i = 0; i < P; i++)
        {
            LastPiece = Instantiate(gameObject,Ray_startPos,transform.rotation);
            Bullet lastpiece = LastPiece.GetComponent<Bullet>();
            lastpiece.IsFresh = false;
            lastpiece.Speed = 6f;
            Destroy(LastPiece.GetComponent<MeshRenderer>());
            LastPiece.transform.Rotate(new Vector3(Random.Range(0f,45f), Random.Range(0f, 45f), 0));

        }

    }

    public void M_Attacker(GameObject A)
    {
        attacker = A;
    }
   

}
