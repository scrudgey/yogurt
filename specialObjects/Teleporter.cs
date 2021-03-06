﻿using UnityEngine;

public class Teleporter : Interactive {
    public ParticleSystem startEffect;
    public float timer;
    public float teleportDelay;
    public string destination;
    public AudioClip teleportStartSound;
    void Start() {
        Interaction teleport = new Interaction(this, "Teleport", "Teleport");
        interactions.Add(teleport);
    }
    public void Teleport() {
        GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.teleport);
        TeleportMenu menu = menuObject.GetComponent<TeleportMenu>();
        menu.PopulateSceneList();
        menu.teleporter = this;
    }
    public void DoTeleport(string toSceneName) {
        timer += teleportDelay;
        startEffect.Play();
        GameManager.Instance.PlayPublicSound(teleportStartSound);
        destination = toSceneName;
    }
    void Update() {
        if (timer > 0) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                startEffect.Stop();
                InputController.Instance.suspendInput = false;
                GameManager.Instance.LeaveScene(destination, 420);
            }
        }
    }
}
