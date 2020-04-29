using UnityEngine;
using System.Collections.Generic;

public class CutsceneMoonLanding : Cutscene {
    private float timer;
    public Dictionary<Collider2D, PhysicsMaterial2D> materials;
    Controllable playerControllable;
    AdvancedAnimation playerAnimation;
    HeadAnimation playerHeadAnimation;
    Collider2D playerCollider;
    Inventory playerInv;
    Pickup initHolding;
    Speech playerSpeech;
    GameObject landingString;
    public override void Configure() {
        materials = new Dictionary<Collider2D, PhysicsMaterial2D>();
        Transform spawnPoint = GameObject.Find("cannonEntryPoint").transform;
        GameObject player = GameManager.Instance.playerObject;
        configured = true;
        player.transform.localScale = new Vector3(-1f, 1f, 1f);
        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;
        playerControllable = GameManager.Instance.playerObject.GetComponent<Controllable>();
        playerAnimation = GameManager.Instance.playerObject.GetComponent<AdvancedAnimation>();
        playerHeadAnimation = GameManager.Instance.playerObject.GetComponent<HeadAnimation>();
        playerSpeech = GameManager.Instance.playerObject.GetComponent<Speech>();
        playerCollider = GameManager.Instance.playerObject.GetComponent<Collider2D>();

        playerInv = GameManager.Instance.playerObject.GetComponent<Inventory>();
        if (playerInv != null) {
            initHolding = playerInv.holding;
        }
        if (playerControllable != null) {
            playerControllable.enabled = false;
        }
        if (playerAnimation != null) {
            playerAnimation.enabled = false;
        }
        if (playerHeadAnimation != null) {
            playerHeadAnimation.enabled = false;
        }
        if (playerSpeech != null) {
            playerSpeech.enabled = false;
        }
        if (playerCollider != null) {
            playerCollider.enabled = false;
        }
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        body.gravityScale = GameManager.Instance.gravity;
        body.drag = 0f;
        body.AddForce(8000f * spawnPoint.up, ForceMode2D.Force);
        PhysicsMaterial2D moonMaterial = Resources.Load("moonlanding") as PhysicsMaterial2D;
        foreach (Collider2D collider in player.GetComponentsInChildren<Collider2D>()) {
            materials[collider] = collider.sharedMaterial;
            collider.sharedMaterial = moonMaterial;
        }
        AudioClip charlierAugh = Resources.Load("sounds/auugh") as AudioClip;
        Toolbox.Instance.AudioSpeaker(charlierAugh, new Vector3(0.38f, -0.65f, 0));

        // landingStrip = GameObject.Find("landingStrip");
        // landingStrip.GetComponent<BoxCollider2D>().enabled = true;
        // landingStrip.GetComponentInChildren<ParticleSystem>().Start();

        landingString = GameObject.Instantiate(Resources.Load("cutscene/landingStrip")) as GameObject;
        landingString.transform.position = new Vector3(0.08f, -0.88f, 1);
    }
    public override void Update() {
        if (timer == 0) {
            UINew.Instance.RefreshUI();
        }
        timer += Time.deltaTime;
        // check to reenable collider
        if (GameManager.Instance.playerObject.transform.position.y < -0.533 && playerCollider != null && !playerCollider.enabled) {
            playerCollider.enabled = true;
        }
        if (timer > 3f) {
            complete = true;
        }
    }
    public override void CleanUp() {
        UINew.Instance.RefreshUI(active: true);
        if (playerControllable != null) {
            playerControllable.enabled = true;
        }
        if (playerAnimation != null) {
            playerAnimation.enabled = true;
        }
        if (playerHeadAnimation != null) {
            playerHeadAnimation.enabled = true;
        }
        if (playerSpeech != null) {
            playerSpeech.enabled = true;
        }
        Rigidbody2D body = GameManager.Instance.playerObject.GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        foreach (KeyValuePair<Collider2D, PhysicsMaterial2D> kvp in materials) {
            kvp.Key.sharedMaterial = kvp.Value;
        }
        Hurtable playerHurable = GameManager.Instance.playerObject.GetComponent<Hurtable>();
        if (playerHurable) {
            playerHurable.KnockDown();
        }
        GameObject.Destroy(landingString);

        if (playerInv != null && initHolding != null) {
            // initHolding = playerInv.holding;
            playerInv.GetItem(initHolding);
        }

        if (!GameManager.Instance.data.visitedMoon) {
            GameManager.Instance.data.visitedMoon = true;
            GameManager.Instance.ShowDiaryEntry("moon");
        }
    }
}