using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Easings;

public class StomachContentsIndicator : MonoBehaviour {
    public enum State { none, easeIn, easeOut }
    public State state;
    public Image icon;
    public System.Guid itemId;
    public RectTransform rectTransform;
    // public Coroutine 
    public float easeInTime = 0.1f;
    public float easeOutTIme = 0.1f;
    public Coroutine easeInCoroutine;
    public Coroutine easeOutCoroutine;
    public void Configure(GameObject item, System.Guid id) {
        // Debug.Log($"configure stomach indicator for id {id}");
        this.itemId = id;
        this.rectTransform = GetComponent<RectTransform>();
        SpriteRenderer spriteRenderer = item.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            icon.sprite = spriteRenderer.sprite;
        } else {
            Debug.LogWarning("eaten object with no sprite renderer");
        }

        MonoLiquid monoLiquid = item.GetComponent<MonoLiquid>();
        if (monoLiquid != null) {
            if (monoLiquid.liquid != null) {
                icon.color = monoLiquid.liquid.color;
            }
        }
        Appear();
    }
    void OnDisable() {
        if (easeInCoroutine != null) {
            StopCoroutine(easeInCoroutine);
        }
        if (easeOutCoroutine != null) {
            StopCoroutine(easeOutCoroutine);
        }
    }
    void OnEnable() {
        if (easeInCoroutine != null) {
            StopCoroutine(easeInCoroutine);
        }
        if (easeOutCoroutine != null) {
            StopCoroutine(easeOutCoroutine);
        }
        if (state == State.easeIn) {
            easeInCoroutine = StartCoroutine(EaseIn());
        } else if (state == State.easeOut) {
            easeOutCoroutine = StartCoroutine(EaseOut());
        }
    }
    public void Remove() {
        state = State.easeOut;
        if (gameObject.activeInHierarchy) {
            if (easeInCoroutine != null) {
                StopCoroutine(easeInCoroutine);
            }
            easeOutCoroutine = StartCoroutine(EaseOut());
        }
    }
    public void Appear() {
        state = State.easeIn;
        if (gameObject.activeInHierarchy) {
            if (easeOutCoroutine != null) {
                StopCoroutine(easeOutCoroutine);
            }
            easeInCoroutine = StartCoroutine(EaseIn());
        }
    }
    public IEnumerator EaseIn() {
        float timer = 0f;
        while (timer < easeInTime) {
            timer += Time.deltaTime;
            float scale = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 0, 50f, easeInTime);
            rectTransform.sizeDelta = new Vector2(60f, scale);
            icon.rectTransform.localScale = new Vector2(1f, scale / 60f);
            yield return null;
        }
        yield return null;
    }
    public IEnumerator EaseOut() {
        float timer = 0f;
        while (timer < easeInTime) {
            timer += Time.deltaTime;
            float scale = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 50f, -50f, easeInTime);
            rectTransform.sizeDelta = new Vector2(60f, scale);
            icon.rectTransform.localScale = new Vector2(1f, scale / 60f);
            yield return null;
        }
        Destroy(gameObject);
        yield return null;
    }
}
