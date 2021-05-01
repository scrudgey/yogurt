using UnityEngine;
using UnityEngine.UI;
using analysis;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using System;
using System.Linq;
public class NeoCommercialReportMenu : MonoBehaviour {
    static readonly string Spacer = "     ";

    public Text descriptionText;
    public TextMeshProUGUI positiveScore, chaosScore, disgustingScore, disturbingScore, offensiveScore;
    public TextMeshProUGUI highlights, metrics;
    public Commercial commercial;
    public Button continueButton;
    public GameObject positiveBadge, chaosBadge, disgustingBadge, disturbingBadge, offensiveBadge;
    public AudioClip shaky;
    public AudioClip ding;
    public GameObject confetti;
    public static string RatingToSprite(Rating rating) {
        switch (rating) {
            default:
            case Rating.disturbing:
                return "<sprite=2>";
            case Rating.disgusting:
                return "<sprite=1>";
            case Rating.offensive:
                return "<sprite=3>";
            case Rating.chaos:
                return "<sprite=4>";
            case Rating.positive:
                return "<sprite=5>";
        }
    }
    Dictionary<Rating, TextMeshProUGUI> metricScores() {
        return new Dictionary<Rating, TextMeshProUGUI> {
            {Rating.disgusting, disgustingScore},
            {Rating.disturbing, disturbingScore},
            {Rating.offensive, offensiveScore},
            {Rating.chaos, chaosScore},
            {Rating.positive, positiveScore},
            };
    }
    Dictionary<Rating, GameObject> metricBadges() {
        return new Dictionary<Rating, GameObject> {
            {Rating.disgusting, disgustingBadge},
            {Rating.disturbing, disturbingBadge},
            {Rating.offensive, offensiveBadge},
            {Rating.chaos, chaosBadge},
            {Rating.positive, positiveBadge},
            };
    }
    private List<string> applausePaths = new List<string> {
        "sounds/applause/applause",
        "sounds/applause/applause2",
        "sounds/applause/applause3"
    };
    IEnumerator WaitAndPlaySound(float delay, AudioClip clippy) {
        yield return new WaitForSecondsRealtime(delay);
        GameManager.Instance.PlayPublicSound(clippy);
    }
    void Start() {
        GameObject existingPop = GameObject.Find("Poptext(Clone)");
        if (existingPop != null) {
            DestroyImmediate(existingPop);
        }
    }
    public void PlayApplause() {
        AudioClip applauseSound = Resources.Load(applausePaths[UnityEngine.Random.Range(0, applausePaths.Count)]) as AudioClip;
        StartCoroutine(WaitAndPlaySound(0.4f, applauseSound));
    }
    public void PlayShaky() { GameManager.Instance.PlayPublicSound(shaky); }
    public void PlayDing() { GameManager.Instance.PlayPublicSound(ding); }

    public void ContinueButton() {
        UINew.Instance.CloseActiveMenu();
        MySaver.Save();
    }
    public void NewDayButton() {
        UINew.Instance.CloseActiveMenu();
        MySaver.Save();
        // StartCoroutine(FadeOut());
        GameManager.Instance.StartCoroutine(FadeOut(GameManager.Instance.data.activeCommercial));
    }
    public System.Collections.IEnumerator FadeOut(Commercial commercial) {
        UINew.Instance.RefreshUI(active: false);
        // float current = UINew.Instance.fader.fadeOutTime;
        // UINew.Instance.fader.fadeOutTime = 3f;
        UINew.Instance.FadeOut(() => { }, fadeOutTime: 3f);
        float timer = 0;
        while (timer < 3f) {
            timer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Max(0, (float)PennerDoubleAnimation.Linear(timer, 1f, -1f, 3f));
            yield return null;
        }
        UINew.Instance.ClearFaderModules();

        if (commercial != null && commercial.cutscene != "none") {
            if (commercial.gravy || commercial.sabotage) {
                GameManager.Instance.AntiMayorCutscene();
            } else if (commercial.cutscene == "ending") {
                GameManager.Instance.EndingCutscene();
            } else {
                GameManager.Instance.BoardRoomCutscene();
            }
        } else {
            GameManager.Instance.NewDayCutscene();
        }
        yield return null;
    }
    public void ReviewButton() {
        GameObject menuObject = Instantiate(Resources.Load("UI/FocusGroupMenu")) as GameObject;
        FocusGroupMenu menu = menuObject.GetComponent<FocusGroupMenu>();
        menu.commercial = commercial;
    }

    public void Report(Commercial activeCommercial) {
        this.commercial = activeCommercial;

        // for debug / research
        commercial.WriteReport();

        descriptionText.text = string.Join(" ", Interpretation.Review(commercial)).Trim();

        SetMetrics();

        SetHighlights();

        SetEvents();

        // disable continue button if we want to force a cutscene
        if (!GameManager.Instance.data.completeCommercials.Contains(commercial))
            if (commercial.name == "I Hurt, You Hurt" || commercial.name == "Ultimate Yogurt Commercial of All Time") {
                continueButton.interactable = false;
            }
    }
    IEnumerator BubbleInBadge(GameObject badge) {
        badge.SetActive(true);
        ImagePulser pulser = badge.GetComponent<ImagePulser>();
        pulser.enabled = false;
        badge.transform.localScale = Vector2.zero;
        float timer = 0;
        while (timer < 1f) {
            timer += Time.unscaledDeltaTime;
            float scale = (float)PennerDoubleAnimation.BackEaseOut(timer, 0f, 0.5f, 1f);
            badge.transform.localScale = scale * Vector2.one;
            yield return null;
        }
        badge.transform.localScale = 0.5f * Vector2.one;
        pulser.enabled = true;
    }
    public void SetMetrics() {
        foreach (Rating rating in Enum.GetValues(typeof(Rating))) {
            metricBadges()[rating].SetActive(false);
        }
        StartCoroutine(DisplayMetrics());
    }
    IEnumerator DisplayMetrics() {
        float timer = 0;
        float fliptimer = 0;
        // play shaky ding
        PlayShaky();
        // randomize scores
        while (timer < 2f) {
            timer += Time.unscaledDeltaTime;
            fliptimer += Time.unscaledDeltaTime;
            if (fliptimer > 0.1f) {
                fliptimer = 0;
                foreach (Rating rating in Enum.GetValues(typeof(Rating))) {
                    metricScores()[rating].text = ((int)UnityEngine.Random.Range(10, 100f)).ToString();
                }
            }
            yield return null;
        }
        // Ding!
        GameManager.Instance.publicAudio.Stop();
        PlayDing();
        confetti.SetActive(true);
        // play applause
        PlayApplause();
        // set the real scores
        SetMetricTextAndBadges();
    }
    public void SetMetricTextAndBadges() {
        foreach (Rating rating in Enum.GetValues(typeof(Rating))) {
            SetMetric(rating);
            if (commercial.quality[rating] > GameManager.Instance.data.topRatings[rating]) {
                if (GameManager.Instance.data.topRatings[rating] > 0) {
                    StartCoroutine(BubbleInBadge(metricBadges()[rating]));
                }
                GameManager.Instance.data.topRatings[rating] = commercial.quality[rating];
            }
        }
    }
    public void SetMetric(Rating rating) {
        float commercialRating = commercial.quality[rating];
        TextMeshProUGUI text = metricScores()[rating];
        text.text = ((int)commercialRating).ToString();
        if (commercialRating > 50 && commercialRating < 100) {
            text.fontStyle = FontStyles.Bold;
            StartCoroutine(BubbleText(0.5f, text.transform));
        } else if (commercialRating > 100 && commercialRating < 300) {
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.enableVertexGradient = true;
            VertexGradient newGrad = new VertexGradient();
            newGrad.topLeft = Color.yellow;
            newGrad.topRight = Color.yellow;
            newGrad.bottomLeft = Color.red;
            newGrad.bottomRight = Color.red;
            text.colorGradient = newGrad;
            StartCoroutine(BubbleText(1f, text.transform));
        } else if (commercialRating > 300) {
            text.fontStyle = FontStyles.Italic;
            text.color = Color.white;
            text.enableVertexGradient = true;
            VertexGradient newGrad = new VertexGradient();
            newGrad.topLeft = Color.red;
            newGrad.topRight = Color.red;
            newGrad.bottomLeft = Color.green;
            newGrad.bottomRight = Color.green;
            text.colorGradient = newGrad;
            StartCoroutine(BubbleText(1f, text.transform));
        }
        // trigger easing bubble
    }
    IEnumerator BubbleText(float amount, Transform target) {
        float timer = 0f;
        float duration = 0.3f;
        // ease in: regular scale -> enlarged
        while (timer < duration) {
            target.localScale = (float)PennerDoubleAnimation.QuintEaseOut(timer, 1f, amount, duration) * Vector2.one;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        // ease out: enlarged -> overshoot down -> regular scale
        duration = 0.3f;
        timer = 0f;
        while (timer < duration) {
            target.localScale = (float)PennerDoubleAnimation.ElasticEaseOut(timer, 1f + amount, -1f * amount, duration) * Vector2.one;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        target.localScale = Vector2.one;
    }

    public void SetHighlights() {
        highlights.text = "";
        CommercialAnalysis analysis = new CommercialAnalysis(commercial);
        List<DescribableOccurrenceData> events = analysis.TopEvents();
        StartCoroutine(WriteHighLights(events));
    }
    IEnumerator WriteHighLights(List<DescribableOccurrenceData> events) {
        yield return new WaitForSecondsRealtime(0.3f);
        int maxIndex = Mathf.Min(10, events.Count);
        for (int i = 0; i < maxIndex; i++) {
            DescribableOccurrenceData oc = events[i];
            // Debug.Log($"{oc.whatHappened}");
            string entry = oc.whatHappened;
            foreach (Rating rating in Enum.GetValues(typeof(Rating))) {
                if (oc.quality[rating] > 0 && oc.quality[rating] < 5) {
                    // Debug.Log($"{rating}: {oc.quality[rating]}");
                    entry += Spacer + String.Concat(Enumerable.Repeat(RatingToSprite(rating), (int)oc.quality[rating]));
                } else if (oc.quality[rating] >= 5) {
                    entry += Spacer + $"{RatingToSprite(rating)}   x{oc.quality[rating]}";
                }
            }
            entry += Spacer + "\n";
            highlights.text += entry;
            yield return new WaitForSecondsRealtime(0.15f);
        }
    }

    public void SetEvents() {
        metrics.text = "";
        StartCoroutine(WriteEvents());
    }
    IEnumerator WriteEvents() {
        yield return new WaitForSecondsRealtime(0.3f);
        foreach (string key in commercial.properties.Keys) {
            CommercialProperty prop = commercial.properties[key];
            string line = prop.desc + ": " + prop.val.ToString() + "\n";
            metrics.text += line;
            yield return new WaitForSecondsRealtime(0.15f);
        }
    }

}
