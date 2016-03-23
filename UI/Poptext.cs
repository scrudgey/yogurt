using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Poptext : MonoBehaviour {
    public string description = "test";
    private HueShiftText hueShifter;
    public float initValue;
    public float finalValue;
    private Text descriptionText;
    private Text valueText;
    private float lifetime;
    private GameObject dock;
    
    public bool colorFX = false;
    
    private AudioSource audioSource;
    public AudioClip incrementSound;
	void Start () {
	   descriptionText = transform.Find("dock/Text").GetComponent<Text>();
       valueText = transform.Find("dock/value").GetComponent<Text>();
       hueShifter = transform.Find("dock/value").GetComponent<HueShiftText>();
       dock = transform.Find("dock").gameObject;
       audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
       
       hueShifter.enabled = false;
       hueShifter.speedConst = 0.3f;
       
       if (descriptionText.text != description){
           descriptionText.text = description + ":";
       }
       valueText.text = initValue.ToString();
       
       StartCoroutine(Display());
	}
    
    IEnumerator Display (){
        RectTransform rectTransform = dock.GetComponent<RectTransform>();
        Vector3 tempPos = rectTransform.anchoredPosition;
        float intime = 1f;
        float outtime = 1f;
        float hangtime = 0.55f;
        
        float t = 0f;
        float y0 = tempPos.y;
        while (t < intime){
            t += Time.deltaTime;
            tempPos.y = easing(t, y0, 70.0f, intime);
            rectTransform.anchoredPosition = tempPos;
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);
        
        valueText.text = finalValue.ToString();
        audioSource.PlayOneShot(incrementSound);
        if (colorFX){
            valueText.color = Color.red;
            hueShifter.enabled = true;
        }
        
        yield return new WaitForSeconds(hangtime);
        
        hueShifter.enabled = false;
        valueText.color = Color.white;
        
        t = 0f;
        y0 = tempPos.y;
        while (t < outtime){
            t += Time.deltaTime;
            tempPos.y = easing(t, 35, -70.0f, outtime);
            rectTransform.anchoredPosition = tempPos;
            yield return null;
        } 
        
        yield return null;
    }
    float easing(float t, float b, float c, float d){
        t /= d/2;
        if (t < 1){
            return c/2 * t * t + b;
        } else {
            t -= 1;
            return -c/2 * (t*(t-2) - 1) +b;
        }
    }
}
