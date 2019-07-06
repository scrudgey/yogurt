using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionSeller : Interactive {
    public Speech speech;
    public Sprite[] leftWave;
    public Sprite[] rightWave;
    public Sprite[] bothWave;
    public AudioClip ingredientSound;
    public AudioSource audioSource;
    public GameObject leftPoint;
    public GameObject rightPoint;
    public SpriteRenderer leftSprite;
    public SpriteRenderer rightSprite;
    void Start() {
        speech = GetComponent<Speech>();
        Interaction giveAct = new Interaction(this, "analyze", "Analyze");
        interactions.Add(giveAct);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public void Analyze(Pickup pickup) {
        CutsceneImp cutscene = new CutsceneImp();
        cutscene.Configure(pickup.gameObject);
        CutsceneManager.Instance.InitializeCutscene(cutscene);
    }
    public string Analyze_desc(Pickup pickup) {
        return "Analyze " + Toolbox.Instance.GetName(pickup.gameObject) + "...";
    }
    public void PlayIngredientSound() {
        audioSource.PlayOneShot(ingredientSound);
    }
}
