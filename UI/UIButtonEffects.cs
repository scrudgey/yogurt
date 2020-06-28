using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonEffects : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler {
    public GameObject prefabSet;

    public List<Button> buttons = new List<Button>();
    public List<InputField> inputs = new List<InputField>();
    public AudioClip startSound;
    public AudioClip stopSound;
    public AudioClip clickSound;
    public AudioClip mouseOverSound;
    public AudioClip inputChangedSound;
    void Start() {
        Configure();
    }
    // void Awake() {
    //     Play(startSound);
    // }
    void OnEnable() {
        Play(startSound);
    }

    public void Configure() {
        if (prefabSet != null)
            AdoptPrefabSounds(prefabSet);
        // Play(startSound);
        foreach (Button button in buttons) {
            button.onClick.AddListener(PlayClickSound);
            UIButtonEffects bfx = button.gameObject.AddComponent<UIButtonEffects>();
            bfx.mouseOverSound = mouseOverSound;
        }
        foreach (InputField infield in inputs) {
            infield.onValueChanged.AddListener(delegate { PlayInputChangedSound(); });
        }
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
    }
    //     public void OnUpdateSelected(BaseEventData eventData) {
    // Play(inputChangedSound);
    //     }
    void OnDestroy() {
        Play(stopSound);
    }

    public void Play(AudioClip clip) {
        if (clip != null) {
            GameManager.Instance.PlayPublicSound(clip);
        }
    }
}
