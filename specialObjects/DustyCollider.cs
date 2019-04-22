using UnityEngine;

public class DustyCollider : MonoBehaviour {
    public Transform dust;
    public ParticleSystem particles;
    public CameraControl camControl;
    public AudioClip impactSound;
    public AudioSource audioSource;
    void Start() {
        dust = transform.Find("dust");
        particles = dust.GetComponent<ParticleSystem>();
        camControl = GameObject.FindObjectOfType<CameraControl>();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public void OnCollisionEnter2D(Collision2D coll) {
        if (GameManager.Instance.playerIsDead)
            return;
        particles.Play();
        audioSource.PlayOneShot(impactSound);
        coll.gameObject.GetComponent<AudioSource>().Stop();
    }
    public void OnCollisionStay2D(Collision2D coll) {
        if (GameManager.Instance.playerIsDead)
            return;
        Vector2 contactPoint = coll.contacts[0].point;
        dust.transform.position = new Vector3(contactPoint.x, contactPoint.y, 0);
        camControl.Shake(0.05f);
    }
}
