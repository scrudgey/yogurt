using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Easings;
using UnityEngine.InputSystem;

public class CutsceneNewDay : Cutscene {
    private float timer;
    Text tomText;
    Text dayText;
    Text skipText;
    GameObject canvas;
    private float stopTomFade = 1.5f;
    private float startDayFade = 1.5f;
    private float stopDayFade = 2.5f;
    private float stopTime = 12f;

    private float startSkipFade = 2.5f;
    private float stopSkipFade = 3.5f;
    public override void Configure() {
        Debug.Log("starting new day cutscene");
        configured = true;
        canvas = GameObject.Find("Canvas");
        tomText = canvas.transform.Find("tomText").GetComponent<Text>();
        dayText = canvas.transform.Find("dayText").GetComponent<Text>();
        tomText.color = Color.clear;
        dayText.color = Color.clear;
        skipText = canvas.transform.Find("skiptext").GetComponent<Text>();

        tomText.text = GameManager.Instance.saveGameName + "'s house";
        dayText.text = "Day " + GameManager.Instance.data.days.ToString();

        Color blank = new Color(255, 255, 255, 0);
        tomText.color = blank;
        dayText.color = blank;
        skipText.color = blank;
    }
    public override void Update() {
        timer += Time.deltaTime;
        if (timer < stopTomFade) {
            Color col = tomText.color;
            col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0, 1, stopTomFade);
            tomText.color = col;
        }
        if (timer > startDayFade && timer < stopDayFade) {
            Color col = dayText.color;
            col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer - startDayFade, 0, 1, stopDayFade - startDayFade);
            dayText.color = col;
        }
        if (timer > startSkipFade && timer < stopSkipFade) {
            Color col = skipText.color;
            col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer - startSkipFade, 0, 1, stopSkipFade - startSkipFade);
            skipText.color = col;
        }
        if (timer >= stopTime || (timer > startDayFade && Keyboard.current.anyKey.isPressed)) {
            InputController.Instance.ResetInput();
            complete = true;
            GameManager.Instance.NewDay();
        }
    }
}