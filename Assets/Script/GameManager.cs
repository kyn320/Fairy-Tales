using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public Text timer;

    float time;

    public bool goaled;


    void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

	// Use this for initialization
	void Start () {
        ServerManager.Instance.StartConnect();
	}
	
	// Update is called once per frame
	void Update () {
        
        if(!goaled)
        DrawTimer();

    }

    void DrawTimer() {
        time += Time.deltaTime;
        float minutes = time / 60;
        float seconds = time % 60;
        float fraction = time * 1000;
        fraction = fraction % 1000;
        timer.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);

    }


}
