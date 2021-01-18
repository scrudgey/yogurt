using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Easings;

public class UIButtonEffects : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {
    public enum MouseEffect { none, bubbleUp }
    public GameObject prefabSet;

    public List<Button> buttons = new List<Button>();
    public List<InputField> inputs = new List<InputField>();
    public AudioClip startSound;
    public AudioClip stopSound;
    public AudioClip clickSound;
    public AudioClip mouseOverSound;
    public AudioClip inputChangedSound;
    public AudioSource audioSource;
    public MouseEffect mouseOverEffect;
    public RectTransform rect;
    public float mouseOverEffectTime = 0.5f;
    public float mouseOverEffectMagnitude = 0.5f;
    private bool configured = false;
    void Start() {
        Configure();
    }
    void OnEnable() {
        Configure();
        Play(startSound);
    }

    public void Configure() {
        if (configured)
            return;
        configured = true;
        audioSource = Toolbox.Instance.SetUpGlobalAudioSource(gameObject);
        if (prefabSet != null)
            AdoptPrefabSounds(prefabSet);
        // Play(startSound);
        foreach (Button button in buttons) {
            if (button == null)
                continue;
            button.onClick.AddListener(PlayClickSound);
            // UIButtonEffects bfx = button.gameObject.AddComponent<UIButtonEffects>();
            UIButtonEffects bfx = Toolbox.GetOrCreateComponent<UIButtonEffects>(button.gameObject);
            bfx.mouseOverSound = mouseOverSound;
        }
        foreach (InputField infield in inputs) {
            infield.onValueChanged.AddListener(delegate { PlayInputChangedSound(); });
        }
        rect = GetComponent<RectTransform>();
    }
    void AdoptPrefabSounds(GameObject prefab) {
        UIButtonEffects effects = prefab.GetComponent<UIButtonEffects>();
        if (startSound == null) {
            startSound = effects.startSound;
        }
        if (stopSound == null) {
            stopSound = effects.stopSound;
        }
        if (clickSound == null) {
            clickSound = effects.clickSound;
        }
        if (mouseOverSound == null) {
            mouseOverSound = effects.mouseOverSound;
        }
    }
    void PlayInputChangedSound() {
        Play(inputChangedSound);
    }
    public void OnPointerDown(PointerEventData eventData) {
        Play(clickSound);
    }
    void PlayClickSound() {
        Play(clickSound);
    }
    public void OnPointerEnter(PointerEventData eventData) {
        Play(mouseOverSound);
        switch (mouseOverEffect) {
            case MouseEffect.bubbleUp:
                StopAllCoroutines();
                StartCoroutine(BubbleIn());
                break;
            default:
                break;
        }
    }
    public void OnPointerExit(PointerEventData eventData) {
        switch (mouseOverEffect) {
            case MouseEffect.bubbleUp:
                StopAllCoroutines();
                StartCoroutine(BubbleOut());
                break;
            default:
                break;
        }
    }
    //     public void OnUpdateSelected(BaseEventData eventData) {
    // Play(inputChangedSound);
    //     }
    void OnDestroy() {
        Play(stopSound);
    }

    public void Play(AudioClip clip) {
        if (clip != null) {
            // GameManager.Instance.PlayPublicSound(clip);
            audioSource.PlayOneShot(clip);
        }
    }
    IEnumerator BubbleIn() {
        // must wait for end of this AND next frame for the new resolution to be applied
        float timer = 0f;
        float initScale = rect.localScale.x;

        float mag = Mathf.Min((1 + mouseOverEffectMagnitude) - initScale, mouseOverEffectMagnitude);
        while (timer < mouseOverEffectTime) {
            timer += Time.unscaledDeltaTime;
            float scale = (float)PennerDoubleAnimation.QuartEaseOut(timer, initScale, mag, mouseOverEffectTime);
            rect.localScale = scale * Vector3.one;
            yield return null;
        }
        // rect.localScale = 1.5f * Vector3.one;
        yield return null;
    }
    IEnumerator BubbleOut() {
        // must wait for end of this AND next frame for the new resolution to be applied
        // yield return new WaitForEndOfFrame();
        // yield return new WaitForEndOfFrame();
        float timer = 0f;
        float initScale = rect.localScale.x;

        float mag = Mathf.Max(1f - initScale, -1f * mouseOverEffectMagnitude);
        while (timer < mouseOverEffectTime) {
            timer += Time.unscaledDeltaTime;
            float scale = (float)PennerDoubleAnimation.QuartEaseOut(timer, initScale, mag, mouseOverEffectTime);
            rect.localScale = scale * Vector3.one;
            yield return null;
        }
        rect.localScale = Vector3.one;
        yield return null;
    }
}
