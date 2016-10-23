using UnityEngine;
using System.Collections;

public class TwinEnemy : MonoBehaviour
{

    Transform tr;

    public float basicSpeed = 1f, moveSpeed = 1f, activeTime = 8f;


    void Start()
    {
        tr = GetComponent<Transform>();
    }

    void OnEnable()
    {
        Invoke("ActiveSet", activeTime);
        moveSpeed = basicSpeed + Random.Range(0, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        tr.localPosition += (Vector3)(new Vector2(-1, 0) * Time.deltaTime * moveSpeed);
    }

    void ActiveSet()
    {
        gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer.Equals("Player"))
        {

        }
    }

}
