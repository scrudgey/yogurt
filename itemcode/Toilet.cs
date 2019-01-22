using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Toilet : Container {
    private AudioSource audioSource;
    public AudioClip flushSound;
    private float timeout;
    public float refractoryPeriod;
    protected override void Awake() {
        base.Awake();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
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
            if (i < 2){
                Collider2D dropletCollider = droplet.GetComponent<Collider2D>();
                foreach(Collider2D playerCollider in Controller.Instance.focus.GetComponentsInChildren<Collider2D>()){
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
        Destroy(target.gameObject);
    }
}
