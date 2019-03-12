using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    public Tank tank;

    public enum Status
    {
        Patrol, Attack
    }
    private Status status = Status.Patrol;

    private GameObject target;
    private float sightDistance = 30;
    private float lastSearchTargetTime = 0;
    private float searchTargetInterval = 3;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (tank.ctrltype != Tank.CtrlType.computer) return;

        if (status == Status.Patrol) PatrolUpdate();

        else if (status == Status.Attack) AttackUpdate();

        TargetUpdate();

    }

    public void ChangeStatus(Status status)//在巡逻和战斗状态中切换
    {
        if (status == Status.Patrol)
        {
            if (status == Status.Patrol)
                PatrolStart();

            else if (status == Status.Attack)
            
                AttackStart();
            

        }
    }

    void PatrolStart()
    {

    }

    void AttackStart()
    {

    }

    void PatrolUpdate()
    {

    }

    void AttackUpdate()
    {

    }

    void TargetUpdate()//找寻目标
    {
        float interval = Time.time - lastSearchTargetTime;
        if (interval < searchTargetInterval) return;

        lastSearchTargetTime = Time.time;

        if (target != null)
            HasTarget();
        else
            NoTarget();
        
    }

    void HasTarget()//存在目标的场合
    {
        Tank targetTank = target.GetComponent<Tank>();
        Vector3 pos = transform.position;
        Vector3 targetPos = target.transform.position;

        if (targetTank.ctrltype == Tank.CtrlType.none)
        {
            target = null;
        }
        else if (Vector3.Distance(pos, targetPos) > sightDistance) 
        {
            target = null;
        }
    }

    void NoTarget()//没有目标的场合
    {
        //float minHp=float.MaxValue;
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < targets.Length; i++)
        {
            Tank tank = targets[i].GetComponent<Tank>();
            if (tank == null) continue;
            if (targets[i] == gameObject) continue;
            if (tank.camp != gameObject.GetComponent<Tank>().camp) continue;
            

            

            Vector3 pos = transform.position;
            Vector3 targetPos = targets[i].transform.position;
            if (Vector3.Distance(pos, targetPos) > sightDistance) continue;
            //if (minHp > tank.hp) 
            target = tank.gameObject;
            Debug.Log("发现敌人");

        }
    }

    public void OnAttacked(GameObject attackTank)//被攻击仇恨设置
    {
        target = attackTank;
    }

    public Vector3 GetTurretTarget()
    {
        if (target==null)//无目标场合
        {
            float y = transform.eulerAngles.y;
            Vector3 rot = new Vector3(0, y, 0);
            return rot;
        }

        else
        {
            Vector3 pos = transform.position;
            Vector3 targetPos = target.transform.position;
            Vector3 vec = targetPos - pos;
            return Quaternion.LookRotation(vec).eulerAngles;
        }
    }

    public bool IsShoot()
    {
        if (target == null) return false;

        float traversing = tank.traversing.eulerAngles.y;
        float angle = traversing - GetTurretTarget().y;
        if (angle < 0) angle += 360;
        if (angle < 10 || angle > 350) return true;

        else return false;
    }


}
