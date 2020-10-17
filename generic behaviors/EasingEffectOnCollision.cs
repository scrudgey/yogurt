using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class EasingEffectOnCollision : MonoBehaviour {
    public RectTransform rect;
    public Collider2D myCollider;
    public AudioClip[] bounceSounds;
    public AudioSource audioSource;
    public float effectTime = 1f;
    public int maxTimes = 10;
    public int times;
    public float lifetime;
    void Start() {
        rect = GetComponent<RectTransform>();
        myCollider = GetComponent<Collider2D>();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    void Update() {
        lifetime += Time.deltaTime;
    }
    void OnCollisionEnter2D(Collision2D coll) {
        if (lifetime < 1f)
            return;
        times += 1;
        StartCoroutine(DoEffect(destroyOnFinish: times > maxTimes));
        // audioSource.PlayOneShot(bounceSounds[Random.Range(0, bounceSounds.Length)]);
        GameManager.Instance.PlayPublicSound(bounceSounds[Random.Range(0, bounceSounds.Length)]);
        // audioSource.PlayOneShot(bounceSounds[Random.Range(0, bounceSounds.Length)]);
        // StartCoroutine(WaitAndReenableCollider());
    }

    IEnumerator DoEffect(bool destroyOnFinish = false) {
        float timer = 0f;
        while (timer < effectTime) {
            timer += Time.deltaTime;
            float scale = (float)PennerDoubleAnimation.ElasticEaseOut(timer, 0.5f, 0.5f, effectTime);
            rect.localScale = scale * Vector3.one;
            yield return null;
        }
        if (destroyOnFinish) {
            rect.localScale = Vector3.one;
            Destroy(this);
        }
        yield return null;
    }
    IEnumerator WaitAndReenableCollider() {
        yield return new WaitForSeconds(0.1f);
        myCollider.enabled = false;
        // yield return new WaitForSeconds(effectTime);
        yield return new WaitForSeconds(0.1f);
        myCollider.enabled = true;
        yield return null;
    }
}
