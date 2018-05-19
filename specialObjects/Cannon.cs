using UnityEngine;

public class Cannon : Interactive {
    public ParticleSystem shootEffect;
    public AudioSource audioSource;
    public AudioClip shootSound;
    void Start() {
        Interaction shoot = new Interaction(this, "Shoot", "StartCannon");
        interactions.Add(shoot);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public void Shoot() {
        shootEffect.Play();
        audioSource.PlayOneShot(shootSound);
    }
    public void StartCannon() {
        MySaver.Save();
        CutsceneManager.Instance.InitializeCutscene<CutsceneCannon>();
    }
}
