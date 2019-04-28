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
    public bool leader;
    public AnchoriteDance danceLeader;
    public List<AnchoriteDance> pupils;

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
    public AudioSource audioSource;
    public AudioClip[] hopSound;
    public AudioClip[] poseSound;
    public int poseIndex;

    private float poseInterval;
    public float hopInterval;
    float timer;
    Vector3 initPos;
    float hopTimer;
    float offset;
    public bool dance;
    private Transform initFootpoint;
    void Start() {
        dance = true;
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        poseInterval = hopInterval * 8.5f;

        initPos = transform.position;

        headSprites = headAnimation.sprites;

        spriteRenderer.sprite = spritesheet[0];
        headRenderer.sprite = headSprites[3];

        if (leader) {
            foreach (AnchoriteDance dancer in GameObject.FindObjectsOfType<AnchoriteDance>()) {
                if (dancer != this) {
                    pupils.Add(dancer);
                }
            }
        }

        foreach (AnchoriteDance pupil in pupils) {
            if (pupil != null) {
                pupil.pupil = true;
                pupil.danceLeader = this;
            }
        }
        offset = 0;
        // offset = Random.Range(0f, 0.005f);
        timer = offset;
        hopTimer = offset;

        poseIndex = Random.Range(0, poseSound.Length - 1);

        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandeHitStun);
    }
    public void HandeHitStun(MessageHitstun message) {
        EndDance();
    }
    public void StartDance(bool send = true) {
        dance = true;
        timer = 0;
        hopTimer = 0;
        shadow.SetActive(true);
        if (send) {
            foreach (AnchoriteDance pupil in pupils) {
                if (pupil != null) {
                    pupil.StartDance(send: false);
                }
            }
            if (danceLeader != null) {
                danceLeader.StartDance();
            }
        }
    }
    public void StopDance(bool send = true) {
        dance = false;
        shadow.SetActive(false);
        if (send) {
            foreach (AnchoriteDance pupil in pupils) {
                if (pupil != null) {
                    pupil.StopDance(send: false);
                }
            }
            if (danceLeader != null) {
                danceLeader.StopDance();
            }
        }
    }
    public void EndDance(bool send = true) {

        StopDance();
        decisionMaker.enabled = true;
        controllable.enabled = true;
        headAnimation.enabled = true;
        advancedAnimation.enabled = true;
        animator.enabled = true;
        headAnimator.enabled = true;
        if (send) {
            foreach (AnchoriteDance pupil in pupils) {
                if (pupil != null) {
                    pupil.EndDance(send: false);
                }
            }
            if (danceLeader != null) {
                danceLeader.EndDance();
            }
        }
        Destroy(this);
    }

    void Update() {
        float y = Hop(hopTimer, initPos.y, 0.025f, hopInterval);
        if (hopTimer != -1) {
            hopTimer += Time.deltaTime;
        } else {
            y = initPos.y;
        }
        if (timer != -1)
            timer += Time.deltaTime;

        // float y = Hop(hopTimer, initPos.y, 0.05f, hopInterval);
        Vector3 position = new Vector3(initPos.x, y, initPos.z);
        transform.position = position;

        if (timer > poseInterval + offset) {
            timer = offset;
            if (!pupil) {
                if (dance) {
                    Pose();
                } else {
                    Pose(reset: true);
                }
            }
            if (poseSound.Length > 0) {
                audioSource.pitch = 1;
                audioSource.PlayOneShot(poseSound[poseIndex]);
                poseIndex += 1;
                if (poseIndex > poseSound.Length - 1) {
                    poseIndex = 0;
                }
            }
            if (!dance) {
                timer = -1;
            }
        }
        if (hopTimer > hopInterval) {
            hopTimer = offset;
            audioSource.pitch = 1;
            if (hopSound.Length > 0)
                audioSource.PlayOneShot(hopSound[Random.Range(0, hopSound.Length)]);
            if (!dance)
                hopTimer = -1;
        }
    }

    void Pose(bool reset = false) {
        int spriteIndex = Random.Range(0, spritesheet.Length);
        int headIndex = Random.Range(0, headSprites.Length);
        if (reset) {
            spriteIndex = 0;
            headIndex = 2;
        }


        spriteRenderer.sprite = spritesheet[spriteIndex];
        headRenderer.sprite = headSprites[headIndex];

        spriteIndex = Random.Range(0, spritesheet.Length);
        headIndex = Random.Range(0, headSprites.Length);
        if (reset) {
            spriteIndex = 0;
            headIndex = 2;
        }

        foreach (AnchoriteDance pupil in pupils) {
            if (pupil != null) {
                pupil.spriteRenderer.sprite = pupil.spritesheet[spriteIndex];
                pupil.headRenderer.sprite = pupil.headSprites[headIndex];
            }
        }
    }

    float Hop(float timer, float init, float delta, float interval) {
        return init + delta * Mathf.Sin((timer / interval) * Mathf.PI);
    }
    // load pupils
}
