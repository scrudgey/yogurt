using UnityEngine;
using System.Collections.Generic;

public class DamageZone : MonoBehaviour {
    public damageType type;
    public float amount;
    public bool suppressImpactSounds;
    public MessageDamage message;
    private Dictionary<Collider2D, GameObject> roots = new Dictionary<Collider2D, GameObject>();
    public AudioClip damageSound;
    public AudioSource audioSource;
    public HashSet<GameObject> damageQueue = new HashSet<GameObject>();
    void Start() {
        message = new MessageDamage(amount, type);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        audioSource.clip = damageSound;
        message.responsibleParty = gameObject;
        message.impersonal = true;
        message.suppressImpactSound = suppressImpactSounds;
    }
    void OnTriggerStay2D(Collider2D other) {
        if (enabled == false)
            return;
        if (InputController.forbiddenTags.Contains(other.tag))
            return;

        // TODO: cache needs invalidation
        // if (!roots.ContainsKey(other)) {
        //     roots[other] = InputController.Instance.GetBaseInteractive(other.transform);
        // }

        // GameObject root = roots[other];
        if (damageQueue.Contains(other.gameObject))
            return;

        damageQueue.Add(other.gameObject);
    }
    void FixedUpdate() {
        HashSet<GameObject> roots = new HashSet<GameObject>();
        foreach (GameObject obj in damageQueue) {
            if (obj == null) continue;
            roots.Add(InputController.Instance.GetBaseInteractive(obj.transform));
        }
        foreach (GameObject obj in roots) {
            if (obj == null)
                continue;
            message.amount = amount * Time.fixedDeltaTime;
            Toolbox.Instance.SendMessage(obj, this, message, sendUpwards: false);
            // Debug.Log($"sending damage to {obj} {message.amount}");
        }
        damageQueue = new HashSet<GameObject>();
    }

    void OnTriggerExit2D() {
    }
    public void ImpactReceived(ImpactResult result) {
        if (Damageable.DamageResults.Contains(result)) {
            if (!audioSource.isPlaying && audioSource.clip != null) {
                audioSource.Play();
            }
        }
    }
}
