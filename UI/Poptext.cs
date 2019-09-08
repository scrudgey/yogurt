using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Poptext : MonoBehaviour {
    public List<string> description = new List<string>();
    private HueShiftText hueShifter;
    public List<float> initValueList = new List<float>();
    public List<float> finalValueList = new List<float>();

    private Text descriptionText;
    private Text valueText;
    private float lifetime;
    private GameObject dock;
    public bool colorFX = false;
    private AudioSource audioSource;
    public AudioClip incrementSound;
    public AudioClip decrementSound;
    public Commercial commercial;
    public void Start() {
        descriptionText = transform.Find("dock/Text").GetComponent<Text>();
        valueText = transform.Find("dock/value").GetComponent<Text>();
        hueShifter = transform.Find("dock/value").GetComponent<HueShiftText>();
        dock = transform.Find("dock").gameObject;
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        audioSource.volume = 0.25f;
        audioSource.spatialBlend = 0;
        hueShifter.enabled = false;
        hueShifter.speedConst = 0.3f;
        if (description.Count > 0) {
            descriptionText.text = description[0] + ":";
            valueText.text = initValueList[0].ToString();
        }
        StartCoroutine(Display());
    }
    IEnumerator Display() {
        RectTransform rectTransform = dock.GetComponent<RectTransform>();
        Vector3 tempPos = rectTransform.anchoredPosition;
        float intime = 1f;
        float outtime = 1f;
        float hangtime = 0.55f;

        float t = 0f;
        float y0 = tempPos.y;
        while (t < intime) {
            t += Time.unscaledDeltaTime;
            tempPos.y = easing(t, y0, 70.0f, intime);
            rectTransform.anchoredPosition = tempPos;
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);

        while (finalValueList.Count > 0) {
            descriptionText.text = description[0] + ":";
            description.Remove(description[0]);

            float finalValue = finalValueList[0];
            float initValue = initValueList[0];
            finalValueList.RemoveAt(0);
            initValueList.RemoveAt(0);
            if (description.Count > 0) {
                hangtime = 1.2f;
            }

            valueText.text = finalValue.ToString();
            if (finalValue > initValue) {
                audioSource.PlayOneShot(incrementSound);
            } else {
                audioSource.PlayOneShot(decrementSound);
            }
            if (colorFX) {
                valueText.color = Color.red;
                hueShifter.enabled = true;
            }
            if (commercial != null) {
                // UI check if commercial is complete      
                UINew.Instance.UpdateRecordButtons(commercial);
            }

            yield return new WaitForSeconds(hangtime);
        }

        hueShifter.enabled = false;
        valueText.color = Color.white;

        t = 0f;
        y0 = tempPos.y;
        while (t < outtime) {
            t += Time.unscaledDeltaTime;
            tempPos.y = easing(t, 35, -70.0f, outtime);
            rectTransform.anchoredPosition = tempPos;
            yield return null;
        }
        if (finalValueList.Count > 0) {
            yield return StartCoroutine(Display());
        } else {
            Destroy(gameObject);
            yield return null;
        }
    }
    float easing(float t, float b, float c, float d) {
        t /= d / 2;
        if (t < 1) {
            return c / 2 * t * t + b;
        } else {
            t -= 1;
            return -c / 2 * (t * (t - 2) - 1) + b;
        }
    }
}
