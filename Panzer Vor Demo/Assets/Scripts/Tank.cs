using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour {

    public Transform Turret;//定义炮塔位置
    public Transform Gun;//定义炮管位置
    public Transform ammunition_AP;//定义弹种1位置
    public Transform ammunition_APCR;//定义弹种2位置
    public Transform ammunition_HE;//定义弹种3位置
    public Texture2D tanksight;//坦克准心素材
    private Transform ammunition;//膛内炮弹
    public Transform camera_Target;//摄像机目标
    private Transform LastBullet;//最近一发炮弹
    public GameObject UI_Manager;//UI信息框

    public static float steer = 40;//车身转速
    public float speed = 6f;//坦克速度
    public float turretRotSpeed = 1f;//炮塔转速
    public float maxRoll = 10f;//炮管最大俯角
    public float minRoll = -4f;//炮管最大仰角

    public float reloadingtimes = 5;//装填时间
    public float reloadingspeed = 1;//装填速度
    private float reloadingprogress;//装填进度

    private float gear = 0;//发动机加力
    private float left_stick = 0;//左转向操纵杆
    private float right_stick = 0;//右转向操纵杆

    private bool inbore;//炮弹是否完成装填
    public static bool outgame = true;//游戏是否暂停
    private bool lockedSights = false;//视角锁定

    public AudioSource TankAudioSource;//音源
    public AudioClip shoot;//开炮音效
    public AudioClip reloadingFinish;//装填完毕音效
    public AudioClip hurtAlly;//打中友军
    public AudioClip ricochet;//跳弹
    public AudioClip noPenetrate;//我们未能穿透他们的装甲
    public AudioClip penetrate;//打得好

    public GameObject PlayerTank;//定义玩家控制的坦克
    public float turretRotTarget = 0;//炮塔朝向目标角度
    public float turretRollTarget = 0;//炮管朝向目标角度

    //UI信息相关
    public static float Caliber = 76;
    public static float Penetrate;
    public static float Angle;
    public static float CorrectionAngle;
    public static float Armor;
    public static float Distance;

    public static float F_Penetrate;
    public static float F_Angle;
    public static float F_CorrectionAngle;
    public static float F_Armor;
    public static float F_Distance;
    public static bool F_result = false;


    public CtrlType ctrlType = CtrlType.player;//默认为玩家
    public enum CtrlType//定义所属
    {
        none, player, computer, Ally
    }



    void Start() {
        //初始化发射音源
        TankAudioSource = gameObject.AddComponent<AudioSource>();
        TankAudioSource.spatialBlend = 1;
        ammunition = ammunition_AP;//上AP
    }

    private void FixedUpdate()
    {
        if (ctrlType != CtrlType.player) return;
        if (outgame) return;

        //输出刷新
        Move();//移动函数
        TurretRotation();//方向机函数
        TurretRoll();//高低机函数
        TargetSignPos();//视角位置坐标
        reloading();//装填函数

    }

    void Update()
    {
        UIInputManager();//菜单栏控制输入采集
        if (outgame) return;
        //游戏操作输入采集
        if (ctrlType != CtrlType.player) return;
        GameInputManager();
    }
    //UI
    private void OnGUI()
    {
        if (outgame) return;
        if (ctrlType != CtrlType.player) return;
        DrawSight();//瞄准圈
        //DrawHp();//生命条
    }

    /// <summary>
    /////////////////////////// 次级方法 
    /// </summary>
    //UI输入采集
    private void UIInputManager()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            outgame = false;
        

    }
    //游戏输入采集
    private void GameInputManager()
    {
        //非玩家控制的场合则跳过后面
        if (ctrlType != CtrlType.player)
            return;
        //开火
        if (Input.GetMouseButtonDown(0))
            if (inbore)
                Shoot();
            else print("装填中");
        //切换弹种
        

        if (Input.GetKeyDown(KeyCode.Alpha1))
            ChangeAmmunition(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            ChangeAmmunition(2);
        //if (Input.GetKeyDown(KeyCode.Alpha3))
            //ChangeAmmunition(3);

        //播放死亡动画
        if (Input.GetKeyDown(KeyCode.Alpha0))
            Die();

        if (Input.GetKeyDown(KeyCode.E))
            Audio(hurtAlly);

        //锁定视角
        if (Input.GetMouseButtonDown(1))       
            lockedSights = true;
        if (Input.GetMouseButtonUp(1))
            lockedSights = false;


        //前进后退
        gear = Input.GetAxis("Vertical");
        //左右转向
        if (Input.GetKey(KeyCode.A))
        {
            left_stick = 1;
            right_stick = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            left_stick = -1;
            right_stick = 1;
        }
        else
        {
            //归零
            left_stick = 0;
            right_stick = 0;
        }
        turretRollTarget = camera_Target.eulerAngles.x;//高低机目标角度
        turretRotTarget = camera_Target.eulerAngles.y;//方向机目标角度
    }
    //移动函数
    private void Move()
    {
        //前进后退
        transform.transform.position += gear * transform.forward * speed * Time.fixedDeltaTime;
        //旋转
        if (gear >= 0)
        {
            transform.Rotate(0, right_stick * steer * Time.fixedDeltaTime, 0);
        }
        //后退反向
        else if (gear < 0)
        {
            transform.Rotate(0, left_stick * steer * Time.fixedDeltaTime, 0);
        }
    }
    //方向机函数
    private void TurretRotation()
    {
        if (lockedSights)
            return;
        if (Camera.main == null) return;
        if (Turret == null) return;
        //判定旋转方向并旋转
        float angle = Turret.eulerAngles.y - turretRotTarget;
        if (angle < 0) angle += 360;
        //取最近的方向旋转
        if (angle > turretRotSpeed && angle < 180)
            Turret.Rotate(0f, -turretRotSpeed, 0f);
        else if (angle > 180 && angle < 360 - turretRotSpeed)
            Turret.Rotate(0f, turretRotSpeed, 0f);
    }
    //高低机函数
    private void TurretRoll()
    {
        if (lockedSights)
            return;
        if (Camera.main == null) return;
        if (Turret == null) return;
        //获取角度
        Vector3 worldEuler = Gun.eulerAngles;
        Vector3 localEuler = Gun.localEulerAngles;
        //世界坐标计算
        worldEuler.x = turretRollTarget;
        Gun.eulerAngles = worldEuler;
        //本地坐标计算
        Vector3 euler = Gun.localEulerAngles;
        if (euler.x > 180)
            euler.x -= 360;
        //范围限制
        if (euler.x > maxRoll)
            euler.x = maxRoll;
        if (euler.x < minRoll)
            euler.x = minRoll;
        //更新变化
        Gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);

    }
    //瞄准位置获取
    private void TargetSignPos()
    {
        //定义碰撞信息和碰撞点
        Vector3 hitPiont = Vector3.zero;
        RaycastHit raycastHit;
        //屏幕中心位置
        Vector3 centerVec = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = Camera.main.ScreenPointToRay(centerVec);
        //射线检测，获取hitPiont
        if (Physics.Raycast(ray, out raycastHit, 400.0f))
            hitPiont = raycastHit.point;//存在物体，返回那个坐标      
        else hitPiont = ray.GetPoint(400);//不存在则返回400米处位置的坐标
        //计算目标角度
        Vector3 dir = hitPiont - Turret.position;
        Quaternion angle = Quaternion.LookRotation(dir);
        turretRotTarget = angle.eulerAngles.y;
        turretRollTarget = angle.eulerAngles.x;
    }
    //准心绘制函数
    private void DrawSight()
    {
        //计算实际射击位置
        Vector3 explodePoint = CalExplodePoint();
        //获取“坦克准心”的屏幕坐标
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(explodePoint);

        //绘制坦克准心
        Rect tankRect = new Rect(screenPoint.x - tanksight.width / 2, Screen.height - screenPoint.y - tanksight.height / 2, tanksight.width, tanksight.height);
        GUI.DrawTexture(tankRect, tanksight);

        //绘制中心准心
        //Rect centerRect = new Rect(Screen.width / 2 - centerSight.width / 2, Screen.height / 2 - centerSight.height / 2, centerSight.width, centerSight.height);
        //GUI.DrawTexture(centerRect, centerSight);
    }
    //预瞄位置计算
    private Vector3 CalExplodePoint()
    {
        //碰撞信息和碰撞点
        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;
        //沿炮管方向的射线
        Vector3 pos = Gun.position + Gun.forward * 5;
        Ray ray = new Ray(pos, Gun.forward);
        //射线检测
        if (Physics.Raycast(ray, out hit, 400.0f))
        {
            hitPoint = hit.point;
        }
        else hitPoint = ray.GetPoint(400);
        return hitPoint;

    }
    //装弹函数
    private void reloading()
    {
        if (inbore) return;//已经上膛则返回

        if (reloadingprogress >= reloadingtimes)//刚上好弹
        {
            Audio(reloadingFinish);//播放音效
            inbore = true;
            reloadingprogress = 0;//进度归零

        }
        reloadingprogress += reloadingspeed * Time.fixedDeltaTime;//增加上弹进度


    }
    //被击函数
    public void BeAttacked(float F_Dmage, GameObject attackTank)
    {

    }
    //弹种切换函数
    private void ChangeAmmunition(int T_shellType)
    {
        if (T_shellType == 1)
            ammunition = ammunition_AP;
        else if (T_shellType == 2)
            ammunition = ammunition_APCR;
        else if (T_shellType == 3)
            ammunition = ammunition_HE;


    }
    /// <summary>
    /////////////////////////// 三级方法
    /// </summary>
    //开火
    private void Shoot()
    {
        Vector3 pos = Gun.position + Gun.forward * 0.5f;//确定炮弹位置
        LastBullet = Instantiate(ammunition, pos, Gun.rotation);//生成炮弹
        Audio(shoot);//播放音效
        BulletProperty();//炮弹信息传递
        inbore = false;


    }
    //死亡效果
    private void Die()
    {
        
    }

    //音效函数
    private void Audio(AudioClip STR)
    {
        TankAudioSource.PlayOneShot(STR);
    }
    //炮弹信息传递
    private void BulletProperty()
    {
        Vector3 B_POS = gameObject.transform.position;
        LastBullet.SendMessage("Master", PlayerTank);
        LastBullet.SendMessage("Location", B_POS);

        
    }

    /// <summary>
    /////////////////////////// 其它
    /// </summary>
    
    //我们打的是自己人
    private void Sound_hurtAlly()
    {
        Audio(hurtAlly);
    }
    //跳弹
    private void Sound_ricochet()
    {        
        UI_Manager.SendMessage("Output_ricochet");
        Audio(ricochet);
    }
    //我们未能穿透他们的装甲
    private void Sound_noPenetrate()
    {       
        UI_Manager.SendMessage("Output_nopenetrate");
        Audio(noPenetrate);
    }
    //打得好
    private void Sound_penetrate()
    {      
        UI_Manager.SendMessage("Output_penetrate");
        Audio(penetrate);
    }

    ///UI信息接收
    private void UI_penetrate(float a)
    {
        Penetrate = a;
    }
    private void UI_angle(float a)
    {
        Angle = a;
    }
    private void UI_correctionangle(float a)
    {
        CorrectionAngle = a;
    }
    private void UI_armor(float a)
    {
        Armor = a;
    }
    private void UI_distance(float a)
    {
        Distance = a;
}

    private void FUI_penetrate(float a)
    {
        F_Penetrate = a;
    }
    private void FUI_angle(float a)
    {
        F_Angle = a;
    }
    private void FUI_correctionangle(float a)
    {
        F_CorrectionAngle = a;
    }
    private void FUI_armor(float a)
    {
        F_Armor = a;
    }
    private void FUI_distance(float a)
    {
        F_Distance = a;
    }







}
