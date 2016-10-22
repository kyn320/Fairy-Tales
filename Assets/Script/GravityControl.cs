using UnityEngine;
using System.Collections;

public class GravityControl : MonoBehaviour {

    public Vector2 change;

    void OnTriggerEnter2D(Collider2D col)
    {
        print("asd");
        col.gameObject.GetComponent<PlayerControl>().VelocityScale(change);
        
    }
}
