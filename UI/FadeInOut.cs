using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Easings;
using System;

public class FadeInOut : MonoBehaviour {
    class FaderModule {
        public bool initialized = false;
        public bool complete;
        protected Action callback;
        protected Image image;
        protected Text text;
        protected Outline outline;
        protected float timer;
        public Color targetColor;
        public virtual void Update() {
            if (!initialized) {
                initialized = true;
                timer = 0f;
            } else {
                timer += Time.unscaledDeltaTime;
            }
        }
        public FaderModule(Action callback, Image image, Text text, Color targetColor) {
            this.callback = callback;
            this.image = image;
            this.timer = 0;
            this.text = text;
            this.targetColor = targetColor;
            if (text != null)
                outline = text.gameObject.GetComponent<Outline>();

        }
        public virtual void End() {
            complete = true;
        }
        protected void SetColor(float alpha) {
            Color newColor = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            Color blackColor = new Color(0f, 0f, 0f, alpha); // a hack
            if (text != null) {
                text.color = newColor;
            }
            if (outline != null) {
                outline.effectColor = blackColor;
            }
            if (image != null) {
                image.color = newColor;
            }
        }
    }
    class FadeInModule : FaderModule {
        int frames;
        float fadeInTime;
        public FadeInModule(Action callback, Image image, Text text, Color targetColor, float fadeInTime) : base(callback, image, text, targetColor) {
            this.fadeInTime = fadeInTime;
            // image.color = Color.black;
            SetColor(1f);
        }
        public override void Update() {
            base.Update();
            // Debug.Log(timer);
            if (frames < 2) {
                timer = 0f;
                frames += 1;
                return;
            }
            float alpha = (float)PennerDoubleAnimation.CircEaseIn(timer, 1f, -1, fadeInTime);
            // Color color = new Color(0, 0, 0, alpha);
            SetColor(alpha);
            if (timer > fadeInTime) {
                End();
            }
        }
        public override void End() {
            base.End();
            SetColor(0);
            if (callback != null)
                callback();
        }
    }
    class FadeOutModule : FaderModule {
        float fadeOutTime;
        public FadeOutModule(Action callback, Image image, Text text, Color targetColor, float fadeOutTime) : base(callback, image, text, targetColor) {
            this.fadeOutTime = fadeOutTime;
        }
        public override void Update() {
            base.Update();
            float alpha = (float)PennerDoubleAnimation.CircEaseOut(timer, 0, 1, fadeOutTime);
            SetColor(alpha);
            if (timer > fadeOutTime) {
                End();
            }
        }
        public override void End() {
            base.End();
            SetColor(1f);
            if (callback != null)
                callback();
        }
    }
    class FadePingPong : FaderModule {
        public float minAlpha;
        public float maxAlpha;
        public float period;
        public FadePingPong(Action callback, Image image, Text text, Color targetColor, float minAlpha, float maxAlpha, float period) : base(callback, image, text, targetColor) {
            this.minAlpha = minAlpha;
            this.maxAlpha = maxAlpha;
            this.period = period;
        }
        public override void Update() {
            base.Update();
            float alpha = minAlpha + (maxAlpha - minAlpha) * (Mathf.Sin(timer * 6.28f / period) + 1) / 2;
            SetColor(alpha);
        }
    }
    Stack<FaderModule> modules = new Stack<FaderModule>();
    public Image image;
    public Text text;
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;
    public void FadeIn(Action callback, Color targetColor) {
        // Debug.Log($"{gameObject} start fade in");
        modules.Push(new FadeInModule(callback, image, text, targetColor, fadeInTime));
    }
    public void FadeOut(Action callback, Color targetColor, float fadeTime = 0.5f) {
        // Debug.Log("{gameObject} start fade out");
        modules.Push(new FadeOutModule(callback, image, text, targetColor, fadeTime));
    }
    public void PingPong(float minAlpha, float maxAlpha, float period, Color targetColor) {
        modules.Push(new FadePingPong(null, image, text, targetColor, minAlpha, maxAlpha, period));
    }
    public void Black() {
        if (image != null)
            image.color = Color.black;
        if (text != null)
            text.color = Color.clear;
    }
    public void White() {
        if (image != null)
            image.color = Color.white;
        if (text != null)
            text.color = Color.white;
    }
    public void Clear() {
        if (image != null)
            image.color = Color.clear;
        if (text != null)
            text.color = Color.clear;
    }
    public void Update() {
        if (modules.Count == 0) {
            return;
        } else {
            FaderModule module = modules.Pop();
            module.Update();
            if (!module.complete) {
                modules.Push(module);
            }
        }
    }
    public void ClearAllModules() {

        // may not want to enable this code:
        // it makes sense to clean up modules but this leads to
        // an infinite new day cutscene loop for some reason.

        // while (modules.Count > 0) {
        //     FaderModule module = modules.Pop();
        //     if (!module.complete)
        //         module.End();
        // }

        modules = new Stack<FaderModule>();
    }
}
