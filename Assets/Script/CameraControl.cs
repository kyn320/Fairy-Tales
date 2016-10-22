using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{

    public Transform target;
    Transform tr;

    public Vector3 basic, margin;
    public float speed;

    Vector3 newPos;
    PlayerControl players;

    // Use this for initialization
    void Start()
    {
        tr = GetComponent<Transform>();
        players = target.GetComponent<PlayerControl>();
    }

    void FixedUpdate()
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
