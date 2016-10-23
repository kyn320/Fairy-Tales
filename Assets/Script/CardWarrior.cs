using UnityEngine;
using System.Collections;

public class CardWarrior : MonoBehaviour {

    public bool down = false;
    public bool stand;
    Animator ani;
	// Use this for initialization
	void Start () {
        ani = GetComponent<Animator>();
        if(!stand)
        StartCoroutine("loopAni");
	}

    IEnumerator loopAni()
    {
        while (true)
        {
            ani.SetBool("Looping", true);
            down = true;
            yield return new WaitForSeconds(2f+Random.Range(1f,5f));
            ani.SetBool("Looping", false);
            down = false;
            yield return new WaitForSeconds(2f + Random.Range(1f, 5f));
        }
    }


	
	
}
