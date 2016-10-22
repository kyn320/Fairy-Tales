using UnityEngine;
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

    public playerstate state;

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
    public float moveSpeed;

    //조작 가능한가?
    public bool inputed;

    //효과음 리스트
    public AudioClip[] audioclips;

    //조작 빈도
    public float h, v;


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
    }

    // Update is called once per frame
    void Update()
    {
        // 조작 가능인 경우 조작 빈도를 대입
        if (inputed)
        {
            InputMoveMent();
            if (Input.GetKeyDown(KeyCode.Space) && grounded && !jumped)
            {
                Jump();
                jumped = true;
            }
            else if (Input.GetKeyDown(KeyCode.Space) && !grounded && jumped)
            {
                Jump();
                jumped = false;
            }
        }


    }

    void FixedUpdate()
    {
        if (inputed)
        {
            Move();
        }
        if (checkGround)
            IsGround();
    }

    void InputMoveMent()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f)
        {
            if (Input.GetAxis("Horizontal") > 0.2f)
            {
                tr.localRotation = Quaternion.Euler(0,0,0);
            }
            else if(Input.GetAxis("Horizontal") < -0.2f)
            {
                tr.localRotation = Quaternion.Euler(0, 180, 0);
            }
            h = Input.GetAxis("Horizontal");
            state = playerstate.run;
        }
        else if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f)
        {
            v = Input.GetAxis("Vertical");
            state = playerstate.run;
        }
        else {
            
            h = 0;
            v = 0;
        }
        ani.SetFloat("Run", Mathf.Abs(Input.GetAxis("Horizontal")));
    }

    void Move()
    {
        tr.localPosition += (Vector3)(new Vector2(h,0) * Time.deltaTime * moveSpeed);
    }

    void Jump()
    {
        state = playerstate.jump;
        ++jumpCnt;

        ani.SetInteger("Jump",jumpCnt);   


        if (groundDelays)
            StopCoroutine("groundCkDelay");

        StartCoroutine("groundCkDelay");
        
        
        grounded = false;
        ri.velocity = new Vector2(0,jumpPower);
    }

    void IsGround()
    {
        grounded = Physics2D.OverlapCircle(groundCk.position, groundLength, groundLayer);
        if (grounded && (jumpCnt > 0))
        {
            jumped = false;
            jumpCnt = 0;
            ani.SetInteger("Jump", jumpCnt);
            state = playerstate.idle;
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

    public void GravityScale(float g) {
        ri.gravityScale = g;
    }

    public void GravityScale(float g,float time)
    {
        ri.gravityScale = g;
        StartCoroutine("GravityDelay",time);
    }

    IEnumerator GravityDelay(float time) { 
        yield return new WaitForSeconds(time);
        ri.gravityScale = 1;
    }

    public void VelocityScale(Vector2 v)
    {
        ri.velocity = v;
    }





}
