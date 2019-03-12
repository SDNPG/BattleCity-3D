using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Property : MonoBehaviour {

    //类型分类
    public enum TYPE
    {
        wall,barriar,Armor,parts,crew,others//可破坏的墙、不可破坏的墙、装甲、部件、成员、坦克
    }
    public TYPE type;
    public GameObject parent;

    #region 用作Armor的场合
    //基本属性
    public bool LastArmor = true;//是否为最内层装甲
    public float ArmorValue;//装甲数值

    //伤害模型角度修正
    public float Angle_X;//X轴方向补正
    public float Angle_Y;//Y轴方向补正
    public float Angle_Z;//Z轴方向补正
    #endregion

    #region 用作wall和barriar的场合
    public float Speed = 5f;
    public bool isDrop = false;
    #endregion

    #region 用作parts和crew的场合
    public float HP;//模块血量

    #endregion
    public int camp;

    // Use this for initialization
    void Start ()
    {
        if (type == TYPE.Armor || type == TYPE.crew || type == TYPE.parts)
            camp = parent.GetComponent<Tank>().camp;       
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (type==TYPE.wall)
        {
            //transform.transform.position -= Speed * transform.up * Time.fixedDeltaTime;
        }
        
    }

    public void BeAttacked(float Num)
    {
        HP -= Num;
        if (type == TYPE.crew || type == TYPE.parts)
        {

        }

    }
}
