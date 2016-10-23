using UnityEngine;
using System.Collections;

public class GoalLine : MonoBehaviour {


    void OnTriggerEnter2D(Collider2D col) {
        GameManager.instance.goaled = true;
        

    }
}
