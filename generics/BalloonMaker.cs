﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonMaker : Interactive {
    public List<GameObject> balloonPrefabs;
    public List<Color> balloonColors;
    public List<AudioClip> spawnSounds;
    private AudioSource audioSource;
    public GameObject particleEffect;
    void Awake() {
        Interaction balloon = new Interaction(this, "Balloon", "MakeBalloon");
        balloon.defaultPriority = 10;
        balloon.hideInManualActions = false;
        interactions.Add(balloon);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public void MakeBalloon() {
        Controllable controllable = GetComponent<Controllable>();
        Inventory inv = GetComponent<Inventory>();
        if (controllable != null && inv != null){
            StartCoroutine(BalloonRoutine(controllable, inv));
        }
    }
    IEnumerator BalloonRoutine(Controllable controllable, Inventory inv){
        Controllable.ControlType oldControlType = controllable.control;
        controllable.SetControl(Controllable.ControlType.none);
        yield return new WaitForSeconds(0.5f);
        GameObject balloonObject = SpawnBalloon();
        Pickup balloonPickup = balloonObject.GetComponent<Pickup>();
        if (balloonPickup != null)
            inv.GetItem(balloonPickup);
        yield return new WaitForSeconds(0.2f);
        controllable.SetControl(oldControlType);
    }
    public GameObject SpawnBalloon(){
        GameObject balloon = GameObject.Instantiate(balloonPrefabs[Random.Range(0, balloonPrefabs.Count)], transform.position, Quaternion.identity);
        SpriteRenderer spriteRenderer = balloon.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null){
            spriteRenderer.color = balloonColors[Random.Range(0, balloonColors.Count)];
        }
        Transform subBalloon = balloon.transform.Find("balloon");
        if (subBalloon){
            spriteRenderer = subBalloon.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null){
                spriteRenderer.color = balloonColors[Random.Range(0, balloonColors.Count)];
            }
        }
        if (spawnSounds.Count > 0){
            audioSource.PlayOneShot(spawnSounds[Random.Range(0, spawnSounds.Count)]);
        }
        if (particleEffect != null){
            GameObject.Instantiate(particleEffect, transform.position, Quaternion.identity);
        }
        return balloon;
    }
}