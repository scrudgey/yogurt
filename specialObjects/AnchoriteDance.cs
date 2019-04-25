using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class AnchoriteDance : MonoBehaviour {
    Transform cachedTransform;
    public new Transform transform {
        get {
            if (cachedTransform == null) {
                cachedTransform = gameObject.GetComponent<Transform>();
            }
            return cachedTransform;
        }
    }
    public bool pupil;
    public AnchoriteDance[] pupils;

    public Sprite[] spritesheet;
    public Sprite[] headSprites;
    public DecisionMaker decisionMaker;
    public Controllable controllable;
    public HeadAnimation headAnimation;
    public AdvancedAnimation advancedAnimation;
    public Animation animator;
    public Animation headAnimator;

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer headRenderer;
    public GameObject shadow;

    public float poseInterval;
    public float hopInterval;
    float timer;
    Vector3 initPos;
    float hopTimer;
    float offset;
    void Start() {
        poseInterval = hopInterval * 9;

        initPos = transform.position;

        headSprites = headAnimation.sprites;

        spriteRenderer.sprite = spritesheet[0];
        headRenderer.sprite = headSprites[3];

        foreach (AnchoriteDance pupil in pupils) {
            if (pupil != null)
                pupil.pupil = true;
        }
        offset = 0;
        // offset = Random.Range(0f, 0.005f);
        timer = offset;
        hopTimer = offset;
    }

    void Update() {
        timer += Time.deltaTime;
        hopTimer += Time.deltaTime;

        float y = Hop(hopTimer, initPos.y, 0.05f, hopInterval);
        Vector3 position = new Vector3(initPos.x, y, initPos.z);
        transform.position = position;

        if (timer > poseInterval + offset) {
            timer = offset;
            if (!pupil)
                Pose();
        }
        if (hopTimer > hopInterval) {
            hopTimer = offset;
        }
    }

    void Pose() {
        int spriteIndex = Random.Range(0, spritesheet.Length);
        int headIndex = Random.Range(0, headSprites.Length);

        spriteRenderer.sprite = spritesheet[spriteIndex];
        headRenderer.sprite = headSprites[headIndex];

        spriteIndex = Random.Range(0, spritesheet.Length);
        headIndex = Random.Range(0, headSprites.Length);

        foreach (AnchoriteDance pupil in pupils) {
            pupil.spriteRenderer.sprite = pupil.spritesheet[spriteIndex];
            pupil.headRenderer.sprite = pupil.headSprites[headIndex];
        }
    }

    float Hop(float timer, float init, float delta, float interval) {
        return init + delta * Mathf.Sin((timer / interval) * Mathf.PI);
    }

    void Reset() {

    }
}
