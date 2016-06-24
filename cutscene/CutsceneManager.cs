using UnityEngine;
using UnityEngine.UI;
using Easings;

public class CutsceneManager : MonoBehaviour {

    public enum CutsceneType {newDay}
    public CutsceneType type = CutsceneType.newDay;
    private float timer;
    Text tomText;
    Text dayText;
    GameObject canvas;

    private float stopTomFade = 1.5f;
    private float startDayFade = 1.5f;
    private float stopDayFade = 2.5f;
    private float stopTime = 12f;

    void Start(){
        if (type == CutsceneType.newDay){
            canvas = GameObject.Find("Canvas");
            tomText = canvas.transform.Find("tomText").GetComponent<Text>();
            dayText = canvas.transform.Find("dayText").GetComponent<Text>();

            dayText.text = "Day "+GameManager.Instance.data.days.ToString();

            Color blank = new Color(255, 255, 255, 0);
            tomText.color = blank;
            dayText.color = blank;
        }
    }

    void Update(){
        timer += Time.deltaTime;
        if (timer < stopTomFade){
            Color col = tomText.color;
            col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0, 1, stopTomFade);
            tomText.color = col;
        }
        if (timer > startDayFade && timer < stopDayFade){
            Color col = dayText.color;
            col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer - startDayFade, 0, 1, stopDayFade - startDayFade);
            dayText.color = col;
        }
        if (timer >= stopTime || Input.anyKey){
            GameManager.Instance.NewDay();
        }
    }
    
}
