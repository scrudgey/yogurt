using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Easings;
using System;

public class FadeInOut : MonoBehaviour {
    public enum State { none, fadeIn, fadeOut };
    public State state;
    public Image image;
    public float timer;
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;
    public Action callback;
    public void FadeIn(Action callback) {
        if (state != State.fadeIn) {
            this.callback = callback;
            state = State.fadeIn;
            timer = 0;
        }
    }
    public void FadeOut(Action callback) {
        if (state != State.fadeOut) {
            this.callback = callback;
            state = State.fadeOut;
            timer = 0;
        }
    }
    public void Update() {
        if (state == State.none)
            return;
        timer += Time.unscaledDeltaTime;
        // Debug.Log(timer);
        switch (state) {
            case State.fadeIn:
                fadeInUpdate();
                break;
            case State.fadeOut:
                fadeOutUpdate();
                break;
            default:
                break;
        }
    }
    public void fadeInUpdate() {
        float alpha = (float)PennerDoubleAnimation.CircEaseIn(timer, 1, -1, fadeInTime);
        Color color = new Color(0, 0, 0, alpha);
        image.color = color;
        if (timer > fadeInTime) {
            state = State.none;
            if (callback != null)
                callback();
            callback = null;
        }
    }
    public void fadeOutUpdate() {
        float alpha = (float)PennerDoubleAnimation.CircEaseOut(timer, 0, 1, fadeOutTime);
        Color color = new Color(0, 0, 0, alpha);
        image.color = color;
        if (timer > fadeOutTime) {
            state = State.none;
            if (callback != null)
                callback();
            callback = null;
        }
    }
}
