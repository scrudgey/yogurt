using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DeathMenu : MonoBehaviour {
    public bool buttonVisible;
    public GameObject button;
    public List<Rigidbody2D> letters;
    public bool lettersInactive;
    private float timer;
    void Start() {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = GameManager.Instance.cam;

        button = transform.Find("Button").gameObject;
        lettersInactive = false;
        button.SetActive(false);
        letters.Add(transform.Find("Y").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("O").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("U").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("D").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("I").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("E").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("D2").GetComponent<Rigidbody2D>());

        UINew.Instance.Start();
        UIColliderManager colliderManager = UINew.Instance.UICanvas.GetComponent<UIColliderManager>();
        colliderManager.Start();
        Collider2D UICollider = colliderManager.topEdge;
        foreach (Rigidbody2D letter in letters) {
            Collider2D letterCollider = letter.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(letterCollider, UICollider, true);
        }
        GameManager.Instance.StartCoroutine(ChangeMusic());
    }
    public void ButtonCallback() {
        GameManager.Instance.NewDayCutscene();
    }
    IEnumerator ChangeMusic() {
        MusicController.Instance.StopTrack();
        yield return new WaitForSeconds(2f);
        if (MusicController.Instance.nowPlaying == MusicTrack.none)
            MusicController.Instance.PlayTrack(MusicTrack.tweakDelay);
    }
    void Update() {
        timer += Time.deltaTime;
        if (Input.anyKeyDown && timer > 1) {
            if (!buttonVisible) {
                buttonVisible = true;
                button.SetActive(true);
            } else {

            }
        }
        bool tempLettersInactive = true;
        if (!lettersInactive && timer > 1) {
            foreach (Rigidbody2D letter in letters) {
                if (letter.velocity.sqrMagnitude > 0.000001f) {
                    tempLettersInactive = false;
                    break;
                }
            }
            if (tempLettersInactive) {
                lettersInactive = true;
            }
        }
        if (lettersInactive && !buttonVisible) {
            buttonVisible = true;
            button.SetActive(true);
        }
    }
}
