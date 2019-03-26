using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonMaker : Interactive {
    public List<GameObject> balloonPrefabs;
    public List<Color> balloonColors;
    public List<AudioClip> spawnSounds;
    public GameObject particleEffect;
    void Awake() {
        Interaction balloon = new Interaction(this, "Balloon", "MakeBalloon");
        balloon.defaultPriority = 1;
        balloon.hideInManualActions = false;
        balloon.hideInRightClickMenu = true;
        balloon.otherOnPlayerConsent = false;
        balloon.playerOnOtherConsent = false;
        balloon.inertOnPlayerConsent = false;
        interactions.Add(balloon);
    }
    public void MakeBalloon() {
        Controllable controllable = GetComponent<Controllable>();
        Inventory inv = GetComponent<Inventory>();
        if (controllable != null && inv != null) {
            StartCoroutine(BalloonRoutine(controllable, inv));
        }
    }
    IEnumerator BalloonRoutine(Controllable controllable, Inventory inv) {
        controllable.disabled = true;
        yield return new WaitForSeconds(0.5f);
        GameObject balloonObject = SpawnBalloon();
        Pickup balloonPickup = balloonObject.GetComponent<Pickup>();
        if (balloonPickup != null)
            inv.GetItem(balloonPickup);
        yield return new WaitForSeconds(0.2f);
        if (controllable == Controller.Instance.focus) {
            controllable.control = Controllable.ControlType.player;
        } else {
            controllable.control = Controllable.ControlType.AI;
        }
        controllable.disabled = false;
    }
    public GameObject SpawnBalloon() {
        GameObject balloon = GameObject.Instantiate(balloonPrefabs[Random.Range(0, balloonPrefabs.Count)], transform.position, Quaternion.identity);
        SpriteRenderer spriteRenderer = balloon.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteRenderer.color = balloonColors[Random.Range(0, balloonColors.Count)];
        }
        Transform subBalloon = balloon.transform.Find("balloon");
        if (subBalloon) {
            spriteRenderer = subBalloon.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) {
                spriteRenderer.color = balloonColors[Random.Range(0, balloonColors.Count)];
            }
        }
        if (spawnSounds.Count > 0) {
            Toolbox.Instance.AudioSpeaker(spawnSounds[Random.Range(0, spawnSounds.Count)], transform.position);
        }
        if (particleEffect != null) {
            GameObject.Instantiate(particleEffect, transform.position, Quaternion.identity);
        }
        return balloon;
    }
}
