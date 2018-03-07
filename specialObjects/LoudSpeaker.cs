using UnityEngine;

public class LoudSpeaker : MonoBehaviour {
    public Speech speech;
    public string line;
    public float interval;
    private float timer;
    void Start() {
        speech = GetComponent<Speech>();
        timer = 2f * interval;
    }
    void Update() {
        timer += Time.deltaTime;
        if (timer > interval) {
            timer = 0f;
            speech.Say(line);
        }
    }
}
