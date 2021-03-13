using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineDance : Routine {
        public static float GlobalHopTime;
        public static float GlobalPoseTime;
        Transform cachedTransform;
        public new Transform transform {
            get {
                if (cachedTransform == null) {
                    cachedTransform = gameObject.GetComponent<Transform>();
                }
                return cachedTransform;
            }
        }
        // public bool pupil;
        // public bool leader;
        // public AnchoriteDance danceLeader;
        // public List<RoutineDance> pupils;
        public Sprite[] spritesheet; // get from resources
        public Sprite[] headSprites;
        // public DecisionMaker decisionMaker;
        // public Controllable controllable;
        // public Controller controller;
        public HeadAnimation headAnimation;
        public AdvancedAnimation advancedAnimation;
        public Animation animator;
        public Animation headAnimator;

        public SpriteRenderer spriteRenderer;
        public SpriteRenderer headRenderer;
        public GameObject shadow;
        public AudioSource audioSource;
        public AudioClip[] hopSound = new AudioClip[] { };
        public AudioClip[] poseSound = new AudioClip[] { };
        public int poseIndex;

        private float poseInterval;
        public float hopInterval = 0.3f;
        // float timer;
        Vector3 initPos;
        // float hopTimer;
        // float offset;
        // public bool inControl;
        public bool configured;
        public bool dance;
        private Transform initFootpoint;
        private Personality personality;

        public RoutineDance(GameObject g, Controller c, Personality p) : base(g, c) {
            c.lostControlDelegate += LeaveControl;
            c.gainedControlDelegate += AssumeControl;
            configured = false;

            routineThought = "Dancing!";
            this.personality = p;

            poseInterval = hopInterval * 8.5f;
            initPos = transform.position;
            RoutineDance.GlobalHopTime = 0;
            RoutineDance.GlobalPoseTime = 0;


            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
            spriteRenderer = g.GetComponent<SpriteRenderer>();
            // animator
            animator = g.GetComponent<Animation>();
            // headAnimator
            headAnimator = g.transform.Find("head").GetComponent<Animation>();

            // headrenderer
            headRenderer = g.transform.Find("head").GetComponent<SpriteRenderer>();


            headAnimation = g.GetComponentInChildren<HeadAnimation>();
            advancedAnimation = g.GetComponent<AdvancedAnimation>();

            // shadow

            // spritesheet
            // spritesheet = Resources.LoadAll<Sprite>("spritesheets/anchorite_dance 2") as Sprite[];
            spritesheet = Toolbox.MemoizedSkinTone("anchorite_dance", SkinColor.dark);
            // headSprites = headAnimation.sprites;
            headSprites = Toolbox.MemoizedSkinTone("anchorite_head", SkinColor.dark);

            if (personality.dancer == Personality.Dancer.leader) {
                // hopsound
                hopSound = Resources.LoadAll<AudioClip>("sounds/toms") as AudioClip[];
                // posesound
                poseSound = Resources.LoadAll<AudioClip>("sounds/chant") as AudioClip[];
            }
            headAnimation.Awake();
            headAnimation.LoadSprites();
            spriteRenderer.sprite = spritesheet[0];
            headRenderer.sprite = headSprites[3];
            poseIndex = UnityEngine.Random.Range(0, spritesheet.Length - 1);
        }

        public void AssumeControl() {
            if (configured)
                return;
            configured = true;

            headAnimation.enabled = false;
            advancedAnimation.enabled = false;
            animator.enabled = false;
            headAnimator.enabled = false;
        }
        public override void ExitPriority() {
            LeaveControl();
        }
        public void LeaveControl() {
            // if (!configured)
            //     return;
            // configured = false;
            headAnimation.enabled = true;
            advancedAnimation.enabled = true;
            animator.enabled = true;
            headAnimator.enabled = true;
        }

        protected override status DoUpdate() {

            // check that controller is active 
            if (!control.Authenticate()) {
                return status.neutral;
            }

            AssumeControl();

            float y = Hop(RoutineDance.GlobalHopTime, initPos.y, 0.025f, hopInterval);

            if (RoutineDance.GlobalPoseTime > poseInterval) {
                if (personality.dancer == Personality.Dancer.leader) {
                    RoutineDance.GlobalPoseTime = 0;
                }
            }

            if (personality.dancer == Personality.Dancer.leader) {
                if (RoutineDance.GlobalHopTime != -1) {
                    RoutineDance.GlobalHopTime += Time.deltaTime;
                }
                if (RoutineDance.GlobalPoseTime != -1)
                    RoutineDance.GlobalPoseTime += Time.deltaTime;
            }

            Vector3 position = new Vector3(initPos.x, y, initPos.z);
            transform.position = position;

            if (RoutineDance.GlobalPoseTime > poseInterval) {
                Pose();

                if (poseSound.Length > 0) {
                    audioSource.pitch = 1;
                    audioSource.PlayOneShot(poseSound[poseIndex]);
                    poseIndex += 1;
                    if (poseIndex > poseSound.Length - 1) {
                        poseIndex = 0;
                    }
                }
            }
            if (RoutineDance.GlobalHopTime > hopInterval) {
                if (personality.dancer == Personality.Dancer.leader) {
                    RoutineDance.GlobalHopTime = 0;
                }
                audioSource.pitch = 1;
                if (hopSound.Length > 0)
                    audioSource.PlayOneShot(hopSound[UnityEngine.Random.Range(0, hopSound.Length)]);
            }
            return status.neutral;
        }

        void Pose() {
            int spriteIndex = 0;
            int headIndex = 0;
            // hash time to get the index.
            if (personality.dancer == Personality.Dancer.leader) {
                spriteIndex = ((int)UnityEngine.Time.realtimeSinceStartup + 1) % spritesheet.Length;
                headIndex = ((int)UnityEngine.Time.realtimeSinceStartup + 1) % headSprites.Length;
            } else {
                spriteIndex = (int)UnityEngine.Time.realtimeSinceStartup % spritesheet.Length;
                headIndex = (int)UnityEngine.Time.realtimeSinceStartup % headSprites.Length;
            }

            spriteRenderer.sprite = spritesheet[spriteIndex];
            headRenderer.sprite = headSprites[headIndex];
        }
        float Hop(float timer, float init, float delta, float interval) {
            return init + delta * Mathf.Sin((timer / interval) * Mathf.PI);
        }
    }
}