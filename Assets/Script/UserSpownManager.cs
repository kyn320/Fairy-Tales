using UnityEngine;
using System.Collections;

public class UserSpownManager : MonoBehaviour
{

    public Transform spownPos;


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            col.gameObject.transform.position = spownPos.position;
            col.gameObject.GetComponent<PlayerControl>().BoosterManager(20f);
        }
    }
}
