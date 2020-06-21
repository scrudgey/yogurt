using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

public class TVMenu : MonoBehaviour {
    public Text hideText;
    public Text showText;
    public Image image;
    public TelevisionShow show;
    private Music myTrack;
    public float timer;
    public float animationTimer;
    public float slewTime;
    public float spriteSlewTime;
    public Regex spriteMatcher = new Regex(@"^<(?<index>\d+)>");
    public Regex spriteAnimMatcher = new Regex(@"^<(?<index1>\d+),(?<index2>\d+)>");
    public Regex slewMatcher = new Regex(@"^(?<time>[\d\.]+)");
    public Regex endMatcher = new Regex(@"^<END>");
    public Regex musicMatcher = new Regex(@"<MUSIC:(?<track>\w+)>");
    public List<Sprite> animationSprites = new List<Sprite>();

    public int animationIndex;
    public void ChannelCallback(int number) {
        Debug.Log(number);
    }
    public void PowerButtonCallback() {
        EndShow();
        UINew.Instance.CloseActiveMenu();
    }
    public void Start() {
        slewTime = 0;
        animationTimer = 0;

        // StartShow(TelevisionShow.LoadByFilename("tv1"));
        StartShow(TelevisionShow.LoadByFilename("vampire1"));
    }
    public void StartShow(TelevisionShow show) {
        this.show = show;
        image.color = new Color(255, 255, 255, 255);
        // image.color = new Color(19, 19, 19, 255);
        timer = 0;
        animationTimer = 0;
        slewTime = 0;
        image.sprite = show.sprites[0];
        showText.text = "";
        hideText.text = "";
    }
    public void Update() {
        if (show == null)
            return;
        timer += Time.unscaledDeltaTime;
        if (timer > slewTime) {
            NextElement();
        }

        if (animationSprites.Count > 0) { // sprite flip timer
            animationTimer += Time.unscaledDeltaTime;
            if (animationTimer > spriteSlewTime) {
                animationTimer = 0;
                animationIndex += 1;
                int index = animationIndex % animationSprites.Count;
                image.sprite = animationSprites[index];
            }
        }
    }
    public void NextElement() {
        if (!show.HasNext()) {
            EndShow();
        }
        string line = show.Next();
        if (musicMatcher.IsMatch(line)) {
            if (myTrack != null)
                MusicController.Instance.StopTrack();
            Match musicMatch = musicMatcher.Match(line);
            GroupCollection groups = musicMatch.Groups;

            Music track = TelevisionShow.musicTracks[groups["track"].Value]();
            MusicController.Instance.EnqueueMusic(track);
            myTrack = track;
        } else if (spriteMatcher.IsMatch(line)) {
            Match spriteMatch = spriteMatcher.Match(line);
            GroupCollection groups = spriteMatch.Groups;
            int index = int.Parse(groups["index"].Value);
            image.sprite = show.sprites[index];

            animationSprites = new List<Sprite>();
            spriteSlewTime = float.MaxValue;

            // Debug.Log("spritematch " + index.ToString());
        } else if (spriteAnimMatcher.IsMatch(line)) {
            Match spriteAnimMatch = spriteAnimMatcher.Match(line);
            GroupCollection groups = spriteAnimMatch.Groups;
            int index1 = int.Parse(groups["index1"].Value);
            int index2 = int.Parse(groups["index2"].Value);
            image.sprite = show.sprites[index1];
            animationSprites = new List<Sprite>();

            animationSprites.Add(show.sprites[index1]);
            animationSprites.Add(show.sprites[index2]);
            spriteSlewTime = 0.1f;

            // Debug.Log("anim match " + index1.ToString() + " " + index2.ToString());
        } else if (slewMatcher.IsMatch(line)) {
            Match slewMatch = slewMatcher.Match(line);
            GroupCollection groups = slewMatch.Groups;
            slewTime = float.Parse(groups["time"].Value);
            // timer = slewTime;
            timer = 0;
        } else if (endMatcher.IsMatch(line)) {
            // Debug.Log("end");
            EndShow();
        } else {
            // Debug.Log("set text: " + line);
            hideText.gameObject.SetActive(true);
            if (line == "***") {
                hideText.gameObject.SetActive(false);
            }

            hideText.text = line;
            showText.text = line;
        }
    }

    public void EndShow() {
        show = null;
        image.sprite = null;
        hideText.text = "";
        showText.text = "";
        hideText.gameObject.SetActive(false);
        // image.sprite = null;
        image.color = new Color(19, 19, 19, 255);
        image.enabled = false;
        if (myTrack != null) {
            myTrack = null;
            MusicController.Instance.End();
        }
    }
    void OnDestroy() {
        if (myTrack != null) {
            myTrack = null;
            MusicController.Instance.End();
        }
    }
}
