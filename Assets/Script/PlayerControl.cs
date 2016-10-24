using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public enum playerstate
    {
        idle,
        run,
        jump,
        fly
    };

    //유저정보
    public playerstate state, oldState;
    public TextMesh NickName;
    Vector3 oldPos;


    //땅에 닫고 있는가?
    public bool checkGround = true, groundDelays;
    public bool grounded;
    public float groundLength, GckDelay;
    public Transform groundCk;
    public LayerMask groundLayer;

    //점프 관련
    public int jumpCnt;
    public bool jumped;
    public float jumpPower;

    //이동 속도
    public float basicSpeed, moveSpeed;

    //조작 가능한가?
    public bool inputed, nuckbacked, dashed, boostered, isPlayer;
    public int dir; //왼 4 오른6

    //효과음 리스트
    public AudioClip[] audioclips;

    //조작 빈도
    public float h, v;

    //대쉬 , 부스터
    public float dashNum, boosterNum;
    public bool dashUPed;
    public GameObject boosterLine;


    //게이지 관련
    public Slider dashSlider, boosterSlider;


    //캐싱 컴포넌트
    Rigidbody2D ri;
    Transform tr;
    Animator ani;
    AudioSource aud;
    SpriteRenderer spr;

    // Use this for initialization
    void Start()
    {
        //컴포넌트를 캐싱 합니다.
        ri = GetComponent<Rigidbody2D>();
        tr = GetComponent<Transform>();
        ani = tr.GetChild(0).GetComponent<Animator>();
        aud = GetComponent<AudioSource>();
        spr = GetComponent<SpriteRenderer>();

        DashManager(0);
        BoosterManager(0);
    }

    // Update is called once per frame
    void Update()
    {
        // 조작 가능인 경우 조작 빈도를 대입
        if (inputed && isPlayer)
        {
            InputMoveMent();

            if (Input.GetKeyDown(KeyCode.LeftControl) && boosterNum >= 100)
            {
                StartCoroutine("BoosterOn");
            }

            if (Input.GetKeyDown(KeyCode.Space) && grounded && !jumped && state != playerstate.fly)
            {
                Jump();
                jumped = true;
                ServerManager.Instance.WriteLine(string.Format("Ani:{0}", 2));
            }
            else if (Input.GetKeyDown(KeyCode.Space) && !grounded && jumped && state != playerstate.fly)
            {
                Jump();
                jumped = false;
                ServerManager.Instance.WriteLine(string.Format("Ani:{0}", 3));
            }
            if (Input.GetKey(KeyCode.Z) && dashNum > 0 && !boostered && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f)
            {
                if (dashUPed)
                {
                    StopCoroutine("DashUp");
                    dashUPed = false;
                }
                Dash();
            }
            else if (dashed)
            {
                moveSpeed = basicSpeed;
                dashed = false;
                if (!dashUPed)
                {
                    StartCoroutine("DashUp");
                }
            }
        }


    }

    void FixedUpdate()
    {
        if (inputed && isPlayer)
        {
            Move();
        }
        if (checkGround)
            IsGround();
    }

    public void Moving(Vector3 newPos, int dir)
    {
        if (oldPos != newPos)
        {
            transform.position = Vector3.Lerp(oldPos, newPos, 10f);
            oldPos = transform.position;
            switch (dir)
            {
                case 4:
                    tr.localRotation = Quaternion.Euler(0, 180, 0);
                    dir = 4; break;
                case 6:
                    tr.localRotation = Quaternion.Euler(0, 0, 0);
                    dir = 6; break;
                default: break;
            }
        }
    }

    public void Aning(int a)
    {
        // 0 : idle, 1 : run , 2 :  jump , 3 : jump2 , 4 : fly , 5 :  nuckback
        switch (a)
        {
            case 0: ani.SetFloat("Run", 0); break;
            case 1: ani.SetFloat("Run", 1); break;
            case 2: ani.SetInteger("Jump", 1); break;
            case 3: ani.SetInteger("Jump", 2); break;
            case 4: ani.SetBool("Fly", true); break;
            case 5: ani.SetBool("NuckBack", true); break;
            default: break;
        }
    }

    void InputMoveMent()
    {

        if (oldPos != transform.position)
        {
            ServerManager.Instance.WriteLine(string.Format("MOVE:{0}:{1}:{2}", transform.position.x, transform.position.y,dir));
            oldPos = transform.position;
        }

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f)
        {
            if (Input.GetAxis("Horizontal") > 0.2f)
            {
                tr.localRotation = Quaternion.Euler(0, 0, 0);
                dir = 6;
            }
            else if (Input.GetAxis("Horizontal") < -0.2f)
            {
                tr.localRotation = Quaternion.Euler(0, 180, 0);
                dir = 4;
            }
            h = Input.GetAxis("Horizontal");
            state = playerstate.run;
            if (oldState != state)
            {
                oldState = state;
                ServerManager.Instance.WriteLine(string.Format("Ani:{0}", 1));
            }
        }
        else if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f)
        {
            v = Input.GetAxis("Vertical");
            state = playerstate.run;
        }
        else
        {

            h = 0;
            v = 0;
            if (state != playerstate.fly)
            {
                state = playerstate.idle;
                if (oldState != state)
                {
                    oldState = state;
                    ServerManager.Instance.WriteLine(string.Format("Ani:{0}", 0));
                }
            }
        }
        ani.SetFloat("Run", Mathf.Abs(Input.GetAxis("Horizontal")));
    }

    void Move()
    {
        oldPos = transform.position;
        tr.localPosition += (Vector3)(new Vector2(h, 0) * Time.deltaTime * moveSpeed);
    }

    void Dash()
    {
        dashed = true;
        moveSpeed = basicSpeed * 1.5f;
        DashManager(-1);
    }

    void Jump()
    {
        state = playerstate.jump;
        ++jumpCnt;

        ani.SetInteger("Jump", jumpCnt);


        if (groundDelays)
            StopCoroutine("groundCkDelay");

        StartCoroutine("groundCkDelay");


        grounded = false;
        ri.velocity = new Vector2(0, jumpPower);

    }

    public void DashManager(float n)
    {
        dashNum += n;
        dashNum = Mathf.Clamp(dashNum, 0, 100);
        dashSlider.value = dashNum;
    }

    public void BoosterManager(float b)
    {
        boosterNum += b;
        boosterNum = Mathf.Clamp(boosterNum, 0, 100);
        boosterSlider.value = boosterNum;
    }

    IEnumerator DashUp()
    {
        dashUPed = true;
        while (dashNum < 100 & !dashed)
        {
            yield return new WaitForSeconds(0.1f);
            DashManager(1);
        }
    }

    IEnumerator BoosterOn()
    {
        Physics2D.IgnoreLayerCollision(9, 11, true);
        boostered = true;
        boosterLine.SetActive(true);
        moveSpeed = basicSpeed * 2.5f;
        for (int i = 0; i < 100; i++)
        {
            BoosterManager(-1);
            yield return new WaitForSeconds(0.1f);
        }
        boostered = false;
        boosterLine.SetActive(false);
        moveSpeed = basicSpeed;
        Physics2D.IgnoreLayerCollision(9, 11, false);

    }

    void IsGround()
    {
        grounded = Physics2D.OverlapCircle(groundCk.position, groundLength, groundLayer);
        if (grounded && ((jumpCnt >= 0) || state == playerstate.fly))
        {
            if (nuckbacked)
            {
                inputed = true;
                nuckbacked = false;
                Physics2D.IgnoreLayerCollision(9, 11, false);
                ani.SetBool("NuckBack", false);
            }
            jumped = false;
            jumpCnt = 0;
            ani.SetInteger("Jump", jumpCnt);
            state = playerstate.idle;
            ani.SetBool("Fly", false);
        }
        else if (grounded == false && ri.velocity.y < -8f)
        {
            state = playerstate.fly;
            if (oldState != state)
            {
                oldState = state;
                ani.SetBool("Fly", true);
                ServerManager.Instance.WriteLine(string.Format("Ani:{0}", 4));
            }
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCk.position, groundLength);
    }

    IEnumerator groundCkDelay()
    {
        checkGround = false;
        groundDelays = true;
        yield return new WaitForSeconds(GckDelay);
        checkGround = true;
        groundDelays = false;
    }

    public void GravityScale(float g)
    {
        ri.gravityScale = g;
    }

    public void GravityScale(float g, float time)
    {
        ri.gravityScale = g;
        StartCoroutine("GravityDelay", time);
    }

    IEnumerator GravityDelay(float time)
    {
        yield return new WaitForSeconds(time);
        ri.gravityScale = 1;
    }

    public void VelocityScale(Vector2 v)
    {
        ri.velocity = v;
    }

    public void NuckBack(float dist)
    {
        ani.SetBool("NuckBack", true);
        ServerManager.Instance.WriteLine(string.Format("Ani:{0}", 5));
        nuckbacked = true;
        inputed = false;
        BoosterManager(10f);
        switch (dir)
        {
            case 4:
                ri.velocity = new Vector2(dist, jumpPower);
                break;
            case 6:
                ri.velocity = new Vector2(-dist, jumpPower);
                break;
            default: break;
        }
        Physics2D.IgnoreLayerCollision(9, 11, true);

    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Enemy2") && !boostered)
        {
            NuckBack(3f);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Enemy") && col.gameObject.GetComponent<CardWarrior>().down && !boostered)
        {
            NuckBack(3f);
        }
    }



}
