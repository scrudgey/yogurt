using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using TMPro;

public class TVMenu : MonoBehaviour {
    public TextMeshProUGUI showText;
    public Text episodeNameText;
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
    public Regex newlineMatcher = new Regex(@"::");
    public List<Sprite> animationSprites = new List<Sprite>();
    public Television television;
    public Button leftButton;
    public Button rightButton;
    public int animationIndex;
    private int _showIndex;
    public int showIndex {
        get { return _showIndex; }
        set {
            _showIndex = value;
            SetEpisodeButtons();
        }
    }
    public void ChannelCallback(int number) {
        showIndex += number;
        if (showIndex >= GameManager.Instance.data.televisionShows.Count) {
            showIndex = 0;
        } else if (showIndex < 0) {
            showIndex = GameManager.Instance.data.televisionShows.Count - 1;
        }
        StartShow();
    }
    public void SetEpisodeButtons() {
        leftButton.interactable = true;
        rightButton.interactable = true;
        if (showIndex <= 0) {
            leftButton.interactable = false;
        }
        if (showIndex == GameManager.Instance.data.televisionShows.Count - 1) {
            rightButton.interactable = false;
        }
    }
    public void PowerButtonCallback() {
        EndShow();
        UINew.Instance.CloseActiveMenu();
    }
    public void Start() {
        slewTime = 0;
        animationTimer = 0;
        showIndex = GameManager.Instance.data.televisionShows.Count - 1;
        StartShow();
    }
    public void StartShow() {
        if (showIndex == -1) {
            show = null;
            timer = 0;
            animationTimer = 0;
            slewTime = 0;
            image.color = new Color(19, 19, 19, 255);
            image.enabled = false;
            showText.text = "Looks like there's nothing on TV.";
            episodeNameText.text = "";
            return;
        }
        string filename = GameManager.Instance.data.televisionShows[showIndex];
        if (GameManager.Instance.data.newTelevisionShows.Contains(filename)) {
            GameManager.Instance.data.newTelevisionShows = new HashSet<string>();
        }
        show = TelevisionShow.LoadByFilename(filename);
        // image.color = new Color(255, 255, 255, 255);
        // image.color = new Color(19, 19, 19, 255);
        timer = 0;
        animationTimer = 0;
        slewTime = 0;
        // image.sprite = show.sprites[0];
        image.color = new Color(19, 19, 19, 255);
        image.enabled = false;
        showText.text = "";
        // hideText.text = "";
        episodeNameText.text = show.name;
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
        string line = "";
        while (line == "")
            line = show.Next();
        if (musicMatcher.IsMatch(line)) {
            if (myTrack != null)
                MusicController.Instance.End();
            Match musicMatch = musicMatcher.Match(line);
            GroupCollection groups = musicMatch.Groups;

            Music track = TelevisionShow.musicTracks[groups["track"].Value]();
            MusicController.Instance.EnqueueMusic(track);
            myTrack = track;
        } else if (spriteMatcher.IsMatch(line)) {
            image.color = new Color(255, 255, 255, 255);
            image.enabled = true;
            Match spriteMatch = spriteMatcher.Match(line);
            GroupCollection groups = spriteMatch.Groups;
            int index = int.Parse(groups["index"].Value);
            image.sprite = show.sprites[index];

            animationSprites = new List<Sprite>();
            spriteSlewTime = float.MaxValue;

            // Debug.Log("spritematch " + index.ToString());
        } else if (spriteAnimMatcher.IsMatch(line)) {
            image.color = new Color(255, 255, 255, 255);
            image.enabled = true;
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
            showText.gameObject.SetActive(true);
            if (line == "***") {
                showText.gameObject.SetActive(false);
            }
            line = newlineMatcher.Replace(line, "\n");
            showText.text = $"<font=\"Roboto-Bold SDF\"><mark=#000000 padding=\"30, 30, 30, 30\">{line}</mark></font>";
            // showText.text = line;
        }
    }

    public void EndShow() {
        show = null;
        image.sprite = null;
        // hideText.text = "";
        showText.text = "";
        showText.gameObject.SetActive(false);
        image.color = new Color(19, 19, 19, 255);
        image.enabled = false;
        if (myTrack != null) {
            myTrack = null;
            MusicController.Instance.End();
        }
    }
    void OnDestroy() {
        television.CheckBubble();
        if (myTrack != null) {
            myTrack = null;
            MusicController.Instance.End();
        }
    }
}
