using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonMaker : Interactive {
    public List<GameObject> balloonPrefabs;
    void Start() {
        Interaction balloon = new Interaction(this, "Balloon", "MakeBalloon");
        balloon.defaultPriority = 10;
        balloon.hideInManualActions = false;
        interactions.Add(balloon);
    }
    public void MakeBalloon() {
        GameObject balloon = GameObject.Instantiate(balloonPrefabs[Random.Range(0, balloonPrefabs.Count)], transform.position, Quaternion.identity);
    }
}
