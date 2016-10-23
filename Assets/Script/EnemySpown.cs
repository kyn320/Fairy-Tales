using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpown : MonoBehaviour
{

    public GameObject enemy;
    public Transform Pos;
    public List<GameObject> pool;
    public float time;


    void Start()
    {
        StartCoroutine("spown", time);
    }

    IEnumerator spown(float time)
    {
        while (true) {
            if (pool.Count == 0)
            {
                GameObject g = Instantiate(enemy, Pos.position, Quaternion.Euler(0, 180f, 0)) as GameObject;
                pool.Add(g);
            }
            else {
                print("asd");
                for (int i = 0; i < pool.Count; i++)
                {
                    if (pool[i].activeSelf == false)
                    {
                        pool[i].transform.position = Pos.position;
                        pool[i].SetActive(true);
                        break;
                    }
                    else if ((i + 1) == pool.Count)
                    {
                        GameObject g = Instantiate(enemy, Pos.position, Quaternion.Euler(0,180f,0)) as GameObject;
                        pool.Add(g);
                        break;
                    }
                }

            }

            yield return new WaitForSeconds(time+Random.Range(0f,1f));
        }
    }
}

