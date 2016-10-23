using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{

    public static CameraControl instance;

    public Transform target;
    Transform tr;

    public Vector3 basic, margin;
    public float speed;

    Vector3 newPos;
    PlayerControl players;

    void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    // Use this for initialization
    void Start()
    {
        tr = GetComponent<Transform>();
    }

    public void SetPlayer(PlayerControl p) {
        players = p;
        target = p.gameObject.transform;
    }

    void FixedUpdate()
    {
        if (players != null)
        {
            if (Mathf.Abs(players.h) > 0.2f)
            {
                margin.x = players.h * 2.3f;
            }
            else
            {
                margin = basic;
            }


            newPos = target.position + margin;
            tr.position = Vector3.Lerp(tr.position, newPos, speed * Time.deltaTime);
        }
    }


}
