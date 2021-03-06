﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Toilet : Container {
    private AudioSource audioSource;
    public AudioClip flushSound;
    private float timeout;
    public float refractoryPeriod;
    override protected void Awake() {
        base.Awake();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        Interaction flushAct = new Interaction(this, "Flush", "Flush");
        interactions.Add(flushAct);
    }
    override protected void PopulateContentActions() {
        base.PopulateContentActions();
        Interaction flushAct = new Interaction(this, "Flush", "Flush");
        interactions.Add(flushAct);
    }
    public void Flush() {
        if (timeout > 0)
            return;
        if (flushSound)
            audioSource.PlayOneShot(flushSound);
        for (int i = 0; i < items.Count; i++) {
            Pickup target = items[i];
            StartCoroutine(spinCycle(target.transform));
            if (Toolbox.Instance.CloneRemover(target.name) == "dollar") {
                // GameManager.Instance.data.achievementStats.dollarsFlushed += 1;
                // GameManager.Instance.CheckAchievements();
                GameManager.Instance.IncrementStat(StatType.dollarsFlushed, 1);
            }
        }
        items = new List<Pickup>();
        timeout = refractoryPeriod;
        for (int i = 0; i < 3; i++) {
            GameObject droplet = Toolbox.Instance.SpawnDroplet(Liquid.LoadLiquid("toilet_water"), 0.5f, gameObject, 0.2f);
            if (i < 2) {
                Collider2D dropletCollider = droplet.GetComponent<Collider2D>();
                foreach (Collider2D playerCollider in InputController.Instance.focus.GetComponentsInChildren<Collider2D>()) {
                    Physics2D.IgnoreCollision(dropletCollider, playerCollider, true);
                }
            }
        }
    }
    public string Flush_desc() {
        if (items.Count > 0) {
            string itemname = Toolbox.Instance.GetName(items[0].gameObject);
            return "Flush " + itemname + " down the toilet";
        } else {
            return "Flush toilet";
        }
    }
    void Update() {
        if (timeout > 0)
            timeout -= Time.deltaTime;
    }
    IEnumerator spinCycle(Transform target) {
        float timer = 0f;
        while (timer < 1f) {
            target.Rotate(Vector3.forward * -90);
            timer += Time.deltaTime;
            yield return null;
        }
        Dump(target.GetComponent<Pickup>());
        ClaimsManager.Instance.WasDestroyed(target.gameObject);
        MySaver.Save();
        MyMarker marker = target.GetComponent<MyMarker>();

        HashSet<Guid> itemTree = new HashSet<Guid>();
        MySaver.RecursivelyAddTree(itemTree, marker.id);

        if (MySaver.objectDataBase.ContainsKey(marker.id)) {
            PersistentObject itemObj = MySaver.objectDataBase[marker.id];
            foreach (KeyValuePair<string, PersistentObject> kvp in itemObj.persistentChildren) {
                MySaver.RecursivelyAddTree(itemTree, kvp.Value.id);
            }
        }

        foreach (Guid idn in itemTree) {
            GameManager.Instance.data.toiletItems.Add(idn);
        }
        // there needs to be a disabled objects cleanup here!
        // if there is a disabled object that belongs to the flushed object but is not in the flushed object
        // it will not be cleaned up properly.
        Destroy(target.gameObject);
    }
}
