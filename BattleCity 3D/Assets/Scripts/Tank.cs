using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour {

    //变量定义
    #region 可设置的属性变量
    //基础
    public int hp;//生命值

    //机动相关
    public float steer;//车身转速
    public float f_speed;//前进极速
    public float r_speed;//倒车极速
    public float acceleration;//坦克加速度
    public float deceleration;//坦克减速度
    public float b_speed;//刹车加速度

    //火力相关
    public float turretRotSpeed;//炮塔转速
    public float maxRoll;//炮管最大俯角
    public float minRoll;//炮管最大仰角
    public float fullReloadingtime;//大装填时间
    //public float ClipReloadingtime;//弹夹炮弹装填时间
    //public int Clip;//弹夹炮弹数量

    //预制体链接
    public Transform traversing;//方向机中心位置
    public Transform elevating;//高低机中心位置
    public Transform[] ammunition;//弹药

    //音效相关
    public AudioClip s_Fire;//开炮音效
    public AudioClip s_ReloadingFinish;//装弹音效
    public AudioClip s_Engine;//引擎音效
    public AudioClip s_Traversing;//方向机音效
    public AudioClip s_EngineEnd;//停车引擎音效

    //信息传递目标
    public GameObject ui_Manager;//UI信息处理

    //外在目标
    public Transform camera_Target;//摄像机目标
    public Transform SnipSights;//狙击视角目标
    
    //控制者所属
    public CtrlType ctrltype;
    public enum CtrlType
    {
        player,computer,none
    }

    //阵营所属
    public int camp = 0;

    public Texture2D UI_sights;//坦克准星

    #endregion

    #region 内部计算用变量
    private AudioSource engineAudio;//发动机音源（引擎声）
    private AudioSource fireAudio;//火炮音源（开炮，装填完毕）
    private AudioSource pronunciation;//车长语音音源
    private AudioSource traversingWork;//方向机音源
    private AudioSource othersAudio;//其他音源

    //机动相关
    private float n_Speed = 0;//当前速度
    private float stateAction=0;//当前行动状态
    private float traversingTarget;//方向机朝向目标角度
    private float elevatingTarget;//高低机朝向目标角度
    private float L_joystick = 0;//左操纵杆
    private float R_joystick = 0;//右操纵杆
    private float gear = 0;//发动机加力

    //动画相关
    private Transform L_tracks;//左侧履带
    private Transform R_tracks;//右侧履带
    private Transform L_wheels;//左侧轮子
    private Transform R_wheels;//右侧轮子
    private MeshRenderer LT;
    private MeshRenderer RT;
    private float L_offset;//左侧履带偏移量
    private float R_offset;//右侧履带偏移量

    private Rigidbody mmRigidbody;

    //火力相关
    private int n_Clip;//当前剩余炮弹
    private Transform ammunitionInBore;//膛内炮弹
    private Transform ammunitionNext;//下一发炮弹
    private int ammunitionNum = 0;//弹药序号
    private float reloadingProgress = 0;//装填进度
    private bool inbore = false;//是否完成装填
    private bool fullClip = false;//是否满弹夹
    private Transform lastBullet;//最近一发炮弹

    //输入限制
    private bool SG_F = false;
    private bool SG_CA = false;
    private bool SG_RL = false;
    private bool SG_SS = false;
    private AI ai;
    private UI_Control ui_Contorl;


    //音效相关
    private bool T_traversing;//方向机是否在工作

    #endregion

    #region 静态变量


    #endregion

    // Use this for initialization
    void Start () {
        //初始化音源部分
        {
            engineAudio = gameObject.AddComponent<AudioSource>();
            engineAudio.playOnAwake = false;
            engineAudio.clip = s_Engine;
            engineAudio.spatialBlend = 0.3f;
            engineAudio.loop = true;

            fireAudio = gameObject.AddComponent<AudioSource>();
            fireAudio.playOnAwake = false;
            fireAudio.spatialBlend = 1;

            traversingWork = gameObject.AddComponent<AudioSource>();
            traversingWork.playOnAwake = false;
            traversingWork.clip = s_Traversing;
            traversingWork.spatialBlend = 1;
            traversingWork.loop = true;

            othersAudio = gameObject.AddComponent<AudioSource>();
            othersAudio.playOnAwake = false;
            othersAudio.spatialBlend = 1;
        }
        if (ctrltype==CtrlType.player)//是玩家控制的场合
        {
            Camera_Main.Target_Nomal = camera_Target.gameObject;//设置为摄像机目标
            Camera_Main.Target_Snip = SnipSights.gameObject;
            gameObject.AddComponent<AudioListener>();//添加声音接收器
        }
        mmRigidbody = gameObject.GetComponent<Rigidbody>();

        //初始化火力部分
        reloadingProgress = fullReloadingtime;
        ammunitionNext = ammunition[ammunitionNum];
        ammunitionInBore = ammunitionNext;
        //n_Clip = Clip;
        inbore = false;
        //fullClip = false;

        //初始化机动部分
        L_tracks = gameObject.transform.Find("L_Track");
        R_tracks = gameObject.transform.Find("R_Track");
        L_wheels = gameObject.transform.Find("L_Wheels");
        R_wheels = gameObject.transform.Find("R_Wheels");

        LT = L_tracks.gameObject.GetComponent<MeshRenderer>();
        RT = R_tracks.gameObject.GetComponent<MeshRenderer>();

        //初始化AI部分
        ai = gameObject.AddComponent<AI>();
        ai.tank = this;

        //初始化UI部分
        ui_Contorl = ui_Manager.GetComponent<UI_Control>();
    }

    // Update is called once per frame
    void Update () {
        if (ctrltype == CtrlType.player)
        {
            Tank_Input();//输入采集
            TargetSignPos();//瞄准目标
        }
        if (ctrltype==CtrlType.computer)
        {
            ComputerCtrl();//AI控制
        }
        if (ctrltype==CtrlType.none)
        {
            NoneCtrl();
        }

    }
    
    //物理相关
    private void FixedUpdate()
    {

        //条件筛选

        if (System_Control.GamePause) return;
        //机动部分
        Engine();//发动机函数
        Move();//移动函数
        //火力部分
        Reloading();//炮弹装填函数
        Elevating();//高低机函数
        Traversing();//方向机函数
        Sound();//常驻音效
        if (ctrltype == CtrlType.player)
            ANM_Move();//轮子和履带动画
        else if (Vector3.Distance(Camera_Main.Target_Nomal.transform.position, gameObject.transform.position) < 21 && ctrltype == CtrlType.computer || Camera_Main.Target_Nomal == null)//其他车辆距离一定范围后就不播放动画以提升性能
            ANM_Move();
            
    }

    //UI
    private void OnGUI()
    {
        //if (outgame) return;
        if (ctrltype != CtrlType.player) return;
        DrawSight();//瞄准圈
        //DrawHp();//生命条
    }

    #region 常驻函数    
    private void Tank_Input()//输入采集函数
    {
        //防多次触发变量重置
        if (Input.GetAxis("ChangeAmmunition") == 0)
            SG_CA = false;
        if (Input.GetAxis("Fire") == 0)
            SG_F = false;
        if (Input.GetAxis("ReLoading") == 0)
            SG_RL = false;
        if (Input.GetAxis("SnipSights") == 0)         
            SG_SS = false;
        
        //开炮
        if (Input.GetAxis("Fire") > 0)
            if (!SG_F)
            {
                A_Fire();
                SG_F = true;
            }

        //切换弹种        
        if (Input.GetAxis("ChangeAmmunition") > 0)
            if (!SG_CA)
            {
                ChangeAmmunition();
                SG_CA = true;
            }

        //重新装填弹药
        if (Input.GetAxis("ReLoading") > 0)
            if (!SG_RL)
                if (ammunitionInBore != ammunitionNext || fullClip == false)  //当前炮弹与待装填炮弹不同时或弹夹不满时
                {
                    inbore = false;
                    //reloadingProgress = fullReloadingtime;
                    ammunitionInBore = ammunitionNext;
                    fullClip = true;
                    SG_RL = true;
                }

        //开镜瞄准
        if (Input.GetAxis("SnipSights") > 0)
            if (!SG_SS)
            {
                if (Camera_Main.snipSights)
                    Camera_Main.snipSights = false;
                else Camera_Main.snipSights = true;
                SG_SS = true;
            }

        if (Input.GetAxis("Lock") > 0) 
        {

        }

        //前进后退
        gear = Input.GetAxis("throttle");
        L_joystick = Input.GetAxis("L_joystick");
        R_joystick = Input.GetAxis("R_joystick");

        elevatingTarget = Camera.main.transform.eulerAngles.x;//高低机目标角度
        traversingTarget = Camera.main.transform.eulerAngles.y;//方向机目标角度
        


    }

    private void Engine()//发动机函数
    {
        if (stateAction == 0 && gear == 0)//无操作直接返回
        {
            gameObject.transform.position = gameObject.transform.position;
            
            return;
        }
        if (gear == 0) //无操作时        
            if (n_Speed < 1f)//减速停车停稳
            {
                n_Speed = 0f;
                stateAction = gear;
                return;
            }
            else
            {
                n_Speed -= deceleration * Time.fixedDeltaTime;//自然减速
                return;
            }
        if (gear != stateAction) //与当前状态不同
        {
            if (n_Speed == 0f) stateAction = gear;//静止起步            

            else if (n_Speed < 1f)//刹车刹稳
            {
                n_Speed = 0f;
                stateAction = gear;
                return;
            }
            else
            {
                n_Speed -= b_speed * Time.fixedDeltaTime;//以刹车速度停止
                return;
            }
        }
        if (gear > 0) //根据加力方向限制最大速度
        {
            if (n_Speed < f_speed) n_Speed += acceleration * Time.fixedDeltaTime;//前进极速限制
        }
        else if (gear < 0)
        {
            if (n_Speed < r_speed) n_Speed += acceleration * Time.fixedDeltaTime;//倒车极速限制
        }
    }

    private void Move()//移动函数
    {
        //前进后退
        mmRigidbody.velocity =stateAction * transform.forward * n_Speed * Time.fixedDeltaTime;
        //mmRigidbody.MovePosition(mmTransform.position + stateAction * transform.forward * n_Speed * Time.fixedDeltaTime);
        //transform.transform.position += stateAction * transform.forward * n_Speed * Time.fixedDeltaTime;
        //转向
        if (gear >= 0)
        {
            transform.Rotate(0, R_joystick * steer * Time.fixedDeltaTime, 0);
        }
        //后退反向
        else if (gear < 0)
        {
            transform.Rotate(0, L_joystick * steer * Time.fixedDeltaTime, 0);
        }
    }

    private void Elevating()//高低机函数
    {
        if (Input.GetAxis("Lock") > 0) //视角锁定则返回
            return;

        //获取角度
        Vector3 worldEuler = elevating.eulerAngles;
        Vector3 localEuler = elevating.localEulerAngles;
        //计算世界坐标
        worldEuler.x = elevatingTarget;
        elevating.eulerAngles = worldEuler;
        //计算本地坐标
        Vector3 euler = elevating.localEulerAngles;
        if (euler.x > 180)
            euler.x -= 360;
        //限制范围
        if (euler.x > maxRoll)
            euler.x = maxRoll;
        if (euler.x < minRoll)
            euler.x = minRoll;
        //更新变化
        elevating.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);
    }

    private void Traversing()//方向机函数
    {
        if (Input.GetAxis("Lock") > 0) //视角锁定则返回
            return;

        float angle = traversing.eulerAngles.y - traversingTarget;
        if (Mathf.Abs(angle) < 1f) 
            T_traversing = false;
        else    
            T_traversing = true;


        if (angle < 0) angle += 360;
        if (angle > turretRotSpeed && angle < 180)
            traversing.Rotate(0f, -turretRotSpeed, 0f);
        else if (angle > 180 && angle < 360 - turretRotSpeed)
            traversing.Rotate(0f, turretRotSpeed, 0f);
    }

    private void Reloading()//自动装弹函数
    {
        if (inbore) return;//已经上好弹则返回

        if (reloadingProgress <= 0) //装弹倒计时结束
        {
            inbore = true;
            //if (n_Clip != 1)
            {
                //reloadingProgress = ClipReloadingtime;
            }
            //else            
                reloadingProgress = fullReloadingtime;
                //ammunitionInBore = ammunitionNext;
            A_Audio(fireAudio, s_ReloadingFinish);
                //n_Clip = Clip;
                //fullClip = true;
            return;
        }
        else reloadingProgress -= Time.fixedDeltaTime;
    }

    private void ANM_Move()//轮子和履带动画函数
    {
        //遍历两侧轮子，根据发动机和操纵杆的关系分别旋转，同理计算履带贴图的偏移量
        if (stateAction >= 0)
        {
            foreach (Transform wheel in L_wheels)
                wheel.transform.Rotate(new Vector3((stateAction * n_Speed - L_joystick / 2f * f_speed) * Time.fixedDeltaTime, 0, 0));
            foreach (Transform wheel in R_wheels)
                wheel.transform.Rotate(new Vector3((stateAction * n_Speed - R_joystick / 2f * f_speed) * Time.fixedDeltaTime, 0, 0));
            if (L_offset >= 15f)
                L_offset = 0f;
            else L_offset += (stateAction * n_Speed - L_joystick / 2f * f_speed) / 10000f;
            if (R_offset >= 15f)
                R_offset = 0f;
            else R_offset += (stateAction * n_Speed - R_joystick / 2f * f_speed) / 10000f;
        }
        else if (stateAction < 0) //因移动函数中后退时的转弯的操作被对调了
        {
            foreach (Transform wheel in L_wheels)
                wheel.transform.Rotate(new Vector3((stateAction * n_Speed - R_joystick / 2f * f_speed) * Time.fixedDeltaTime, 0, 0));
            foreach (Transform wheel in R_wheels)
                wheel.transform.Rotate(new Vector3((stateAction * n_Speed - L_joystick / 2f * f_speed) * Time.fixedDeltaTime, 0, 0));
            if (L_offset >= 15f)
                L_offset = 0f;
            else L_offset += (stateAction * n_Speed - R_joystick / 2f * f_speed) / 10000f;
            if (R_offset >= 15f)
                R_offset = 0f;
            else R_offset += (stateAction * n_Speed - L_joystick / 2f * f_speed) / 10000f;
        }
        //将贴图位移实现
        LT.material.mainTextureOffset = new Vector2(0, L_offset);
        RT.material.mainTextureOffset = new Vector2(0, R_offset);
    }

    private void Sound()//常驻音效
    {
        if (Input.GetAxis("throttle") != 0 && !engineAudio.isPlaying)
        {
            ;
            engineAudio.Play();
        }
        else if (L_joystick != 0 && !engineAudio.isPlaying)
        {
            engineAudio.Play();
        }
        else if (Input.GetAxis("throttle") == 0 && engineAudio.isPlaying && L_joystick == 0)  
        {
            engineAudio.Pause();
            A_Audio(othersAudio, s_EngineEnd);
        }

        if (T_traversing && !traversingWork.isPlaying)
            traversingWork.Play();
        else if (!T_traversing && traversingWork.isPlaying)        
            traversingWork.Pause();
        

    }

    private void TargetSignPos()//获取瞄准位置
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
        Vector3 dir = hitPiont - traversing.position;
        Quaternion angle = Quaternion.LookRotation(dir);
        traversingTarget = angle.eulerAngles.y;
        elevatingTarget = angle.eulerAngles.x;
    }

    private void DrawSight()//准心绘制函数
    {
        //计算实际射击位置
        Vector3 explodePoint = CalExplodePoint();
        //获取“坦克准心”的屏幕坐标
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(explodePoint);
        //绘制坦克准心
        Rect tankRect = new Rect(screenPoint.x - UI_sights.width / 2, Screen.height - screenPoint.y - UI_sights.height / 2, UI_sights.width, UI_sights.height);
        GUI.DrawTexture(tankRect, UI_sights);

    }
    //预瞄位置计算
    private Vector3 CalExplodePoint()
    {
        //碰撞信息和碰撞点
        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;
        //沿炮管方向的射线
        Vector3 pos = elevating.position + elevating.forward * 5;
        Ray ray = new Ray(pos, elevating.forward);
        //射线检测
        if (Physics.Raycast(ray, out hit, 400.0f))
        {
            hitPoint = hit.point;
        }
        else hitPoint = ray.GetPoint(400);
        return hitPoint;

    }
    #endregion



    #region 触发函数
    private void A_Fire()//开炮
    {
        if (inbore)
        {
            inbore = false;
            //A_Audio(s_Fire,1);
            Vector3 Pos = elevating.transform.position + elevating.forward * 0.5f;
            lastBullet = Instantiate(ammunitionInBore, Pos, elevating.rotation);
            lastBullet.SendMessage("M_Attacker", gameObject);
            A_Audio(fireAudio, s_Fire);
            //n_Clip -= 1;
            //fullClip = false;

        }
        else
        {
            ;//未装填完毕
        }        
    }
    private void ChangeAmmunition()//切换炮弹动作
    {
        if (ammunitionNum == ammunition.Length - 1) //序号到达上限置零       
            ammunitionNum = 0;        
        else
            ammunitionNum += 1;
        //ammunitionNext = ammunition[ammunitionNum];    
        ammunitionInBore = ammunition[ammunitionNum];
        ui_Contorl.A_ShellType(ammunitionNum);
    }

    private void A_Audio(AudioSource A, AudioClip B)
    {

        A.PlayOneShot(B);

            //case 0:
            //    A.Stop();
           //     break;

          //  case 2:
            //    A.loop = true;
          //      A.clip = B;
           //     A.Play();
           //     break;

        
    }

    public void BeAttacked(int A, GameObject Attacker)
    {
        hp -= A;
        Debug.Log(hp);
        if (hp <= 0) //被击毁
        {
            Destroy(gameObject);
        }

        if (ai != null) 
        {
            ai.OnAttacked(Attacker);
        }
    }

    public void ComputerCtrl()
    {

        Vector3 rot = ai.GetTurretTarget();
        elevatingTarget = rot.y;
        traversingTarget = rot.x;
        if (ai.IsShoot()) A_Fire();
        

        
    }

    public void NoneCtrl()
    {

        gear = 0;
        steer = 0;
        

        
    }
    #endregion




}
