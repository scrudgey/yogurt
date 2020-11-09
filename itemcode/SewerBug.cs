using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SewerBug : MonoBehaviour {
    public enum State { run, stop };
    public State state;
    public Rigidbody2D body;
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public Vector2 _velocity;
    public float runTime;
    public float dTheta;
    private int spriteIndex;
    private float spriteTimer;
    public Vector2 velocity {
        get { return _velocity; }
        set {
            _velocity = value;
            body.velocity = value;
        }
    }
    void Start() {
        Vector3 initialV = Random.insideUnitCircle;
        initialV.z = 0f;
        velocity = initialV;
        state = State.run;
        runTime = Random.Range(3f, 5f);
        dTheta = 0.02f;
    }
    void StartRun() {
        state = State.run;
        runTime = Random.Range(3f, 5f);

        Vector3 initialV = Random.insideUnitCircle;
        initialV.z = 0f;
        velocity = initialV;
    }

    public void OnTriggerEnter2D(Collider2D other) {
        velocity *= -1f;
        velocity = Toolbox.Instance.RotateZ(velocity, 90 * Mathf.PI / 360f);
        // dTheta *= -1f;
        if (state == State.stop) {
            StartRun();
        }
    }

    void FixedUpdate() {
        if (runTime > 0) {
            runTime -= Time.deltaTime;
        }
        if (state == State.run) {
            velocity = Toolbox.Instance.RotateZ(velocity, dTheta);
            if (runTime < 0) {
                state = State.stop;
                runTime = Random.Range(3f, 5f);
            }

            spriteTimer -= Time.deltaTime;
            if (spriteTimer < 0) {
                spriteIndex += 1;
                spriteTimer = 0.05f;
                if (spriteIndex > 1) {
                    spriteIndex = 0;
                }
                spriteRenderer.sprite = sprites[spriteIndex];
            }
        } else if (state == State.stop) {
            velocity = Vector3.zero;
            if (runTime < 0) {
                StartRun();
            }
        }
    }
}
