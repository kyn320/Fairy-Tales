using UnityEngine;
using System.Collections;

public class GravityControl : MonoBehaviour {

    public Vector2 change;
    public float g = 1,time = 1;

    void OnTriggerEnter2D(Collider2D col)
    {
        col.gameObject.GetComponent<PlayerControl>().VelocityScale(change);
        col.gameObject.GetComponent<PlayerControl>().GravityScale(g,time);
    }
}
