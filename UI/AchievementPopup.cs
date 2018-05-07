using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Easings;
public class AchievementPopup : MonoBehaviour {
    public struct CollectedInfo {
        public string name;
        public Sprite sprite;
        public CollectedInfo(GameObject obj) {
            name = Toolbox.Instance.GetName(obj);
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer) {
                sprite = renderer.sprite;
            } else {
                sprite = null;
            }
        }
    }
    public Text titleText;
    public Text bodyText;
    public Image image;
    public AudioClip collectedSound;
    private AudioSource audioSource;

    public void CollectionPopup(GameObject obj) {
        CollectedInfo info = new CollectedInfo(obj);
        CollectionPopup(info);
    }
    public void CollectionPopup(CollectedInfo info) {
        titleText = transform.Find("Panel/TextPanel/Title").GetComponent<Text>();
        bodyText = transform.Find("Panel/TextPanel/Body").GetComponent<Text>();
        image = transform.Find("Panel/icon").GetComponent<Image>();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        audioSource.volume = 0.25f;
        audioSource.spatialBlend = 0;
        titleText.text = "Collected";
        bodyText.text = info.name;
        image.sprite = info.sprite;
        audioSource.PlayOneShot(collectedSound);
        StartCoroutine(Display());
    }
    public void Achievement(Achievement achieve) {
        titleText = transform.Find("Panel/TextPanel/Title").GetComponent<Text>();
        bodyText = transform.Find("Panel/TextPanel/Body").GetComponent<Text>();
        image = transform.Find("Panel/icon").GetComponent<Image>();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        audioSource.volume = 0.25f;
        audioSource.spatialBlend = 0;
        titleText.text = "Achievement Unlocked!";
        bodyText.text = achieve.title;
        Sprite icon = Resources.Load<Sprite>("achievements/icons/"+achieve.icon) as Sprite;
        image.sprite = icon;
        audioSource.PlayOneShot(collectedSound);
        StartCoroutine(Display());
    }
    IEnumerator Display() {
        RectTransform rectTransform = transform.Find("Panel").GetComponent<RectTransform>();
        Vector3 tempPos = rectTransform.anchoredPosition;
        Canvas canvas = GetComponent<Canvas>();
        canvas.sortingOrder = 500;
        float intime = 0.75f;
        float outtime = 0.75f;
        float hangtime = 1.55f;
        float t = 0f;
        float y0 = -100f;
        while (t < intime) {
            t += Time.deltaTime;
            tempPos.y = (float)PennerDoubleAnimation.ExpoEaseOut(t, y0, 100f, intime);
            rectTransform.anchoredPosition = tempPos;
            yield return null;
        }
        yield return new WaitForSeconds(hangtime);
        t = 0f;
        y0 = tempPos.y;
        while (t < outtime) {
            t += Time.deltaTime;
            tempPos.y = (float)PennerDoubleAnimation.ExpoEaseIn(t, y0, -100f, intime);
            rectTransform.anchoredPosition = tempPos;
            yield return null;
        }
        Destroy(gameObject);
        UINew.Instance.achievementPopupInProgress = false;
        yield return null;
    }
}
