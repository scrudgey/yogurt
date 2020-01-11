using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : MonoBehaviour {
    enum State { slew, fire }
    State state;
    public GameObject dart;
    public Transform firePoint;
    public AudioClip fireSound;
    public AudioSource source;
    public float timer;
    public float slewTime;
    public int volleyNumber;
    public int volleyCounter;
    // public float fireAngle = 224f;
    void Start() {
        source = Toolbox.Instance.SetUpAudioSource(gameObject);
        timer = Random.Range(0, 4.5f);
    }
    void Update() {
        timer += Time.deltaTime;
        switch (state) {
            default:
            case State.slew:
                if (timer > slewTime) {
                    state = State.fire;
                    timer = 0;
                }
                break;
            case State.fire:
                if (timer > 0.1f) {
                    timer = 0;
                    GameObject dartObj = Instantiate(dart, firePoint.position, Quaternion.LookRotation(Vector2.left, Vector3.up));
                    Rigidbody2D dartBody = dartObj.GetComponent<Rigidbody2D>();
                    dartBody.velocity = 4f * Toolbox.Instance.RandomVector(firePoint.up, 10f);
                    Toolbox.Instance.AudioSpeaker(fireSound, firePoint.position);
                    volleyCounter += 1;
                    if (volleyCounter > volleyNumber) {
                        volleyCounter = 0;
                        state = State.slew;
                        volleyNumber = Random.Range(3, 13);
                        slewTime = Random.Range(4, 6);
                    }
                }
                break;
        }

    }
}
