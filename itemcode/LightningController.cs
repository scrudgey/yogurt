using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class LightningController : MonoBehaviour {
    public List<SpriteRenderer> lightnings;
    public SpriteRenderer clouds;
    public SpriteRenderer sky;
    public Color mountainInitialColor;
    public Color mountainWhitestColor;
    public float timer;
    public float interval;
    public float intervalMin;
    public float intervalMax;
    public float cloudFlashMin;
    public float cloudFlashMax;
    // public float cloudFlashDuration;
    public AudioSource audioSource;
    public List<AudioClip> strikeSounds;
    public void Start() {
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        audioSource.minDistance = 3f;
        audioSource.maxDistance = 5.42f;
        audioSource.spatialBlend = 0;
        foreach (SpriteRenderer lightning in lightnings) {
            lightning.enabled = false;
        }
    }
    public void Update() {
        timer += Time.deltaTime;
        if (timer > interval) {
            timer = 0f;
            interval = Random.Range(intervalMin, intervalMax);
            Strike();
        }
    }
    public void Strike() {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(strikeSounds[Random.Range(0, strikeSounds.Count)]);
        StartCoroutine(StrikeLightning(lightnings[Random.Range(0, lightnings.Count)]));
        StartCoroutine(FlashClouds());
    }
    IEnumerator StrikeLightning(SpriteRenderer lightning) {
        lightning.enabled = true;
        sky.color = Color.white;
        yield return new WaitForEndOfFrame();
        sky.color = Color.black;
        yield return new WaitForSeconds(0.2f);
        lightning.enabled = false;
        yield return null;
    }
    IEnumerator FlashClouds() {
        float timer = 0f;
        float cloudFlashDuration = Random.Range(cloudFlashMin, cloudFlashMax);
        while (timer < cloudFlashDuration) {
            timer += Time.deltaTime;
            timer = Mathf.Min(timer, cloudFlashDuration);
            float colorValue = (float)PennerDoubleAnimation.BounceEaseIn(timer, mountainInitialColor.r, mountainWhitestColor.r - mountainInitialColor.r, cloudFlashDuration);
            Color color = new Color(colorValue, colorValue, colorValue, 1);
            clouds.color = color;
            yield return null;
        }
        timer = 0;
        while (timer < cloudFlashDuration) {
            timer += Time.deltaTime;
            timer = Mathf.Min(timer, cloudFlashDuration);
            float colorValue = (float)PennerDoubleAnimation.BounceEaseOut(timer, mountainWhitestColor.r, mountainInitialColor.r - mountainWhitestColor.r, cloudFlashDuration);
            Color color = new Color(colorValue, colorValue, colorValue, 1);
            clouds.color = color;
            yield return null;
        }

    }
}
