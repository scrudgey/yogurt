using UnityEngine;

public class Bed : Doorway {
	bool unmade = true;
	public Sprite[] bedSprites;
	public Sprite[] headSprites;
	public Sprite[] bubbleSprites;
	public AudioClip snoreSound;
	private SpriteRenderer head;
	private SpriteRenderer bubble;
	private float animationTimer;
	SpriteRenderer spriteRenderer;
	private bool sleeping;
	private bool frame;
	public AudioClip beddingSound;
	public override void Start(){
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		audioSource.spatialBlend = 0;
		spriteRenderer = GetComponent<SpriteRenderer>();
		head = transform.Find("head").GetComponent<SpriteRenderer>();
		bubble = transform.Find("bubble").GetComponent<SpriteRenderer>();
		Interaction makeBed = new Interaction(this, "Clean", "MakeBed");
		makeBed.validationFunction = true;
		makeBed.descString = "Make the bed";
		Interaction sleep = new Interaction(this, "Sleep", "Sleep");
		sleep.descString = "Go to bed";
		interactions.Add(sleep);
		interactions.Add(makeBed);
		if (!sleeping){
			head.gameObject.SetActive(false);
			bubble.gameObject.SetActive(false);
		} else {
			frame = true;
			animationTimer = 0f;
			spriteRenderer.sprite = bedSprites[0];
			head.gameObject.SetActive(true);
			bubble.gameObject.SetActive(true);
		}
	}
	public void MakeBed(){
		unmade = false;
		if (spriteRenderer){
			spriteRenderer.sprite = bedSprites[0];
			audioSource.PlayOneShot(beddingSound);
		}
	}
	public bool MakeBed_Validation(){
		return unmade;
	}
	public void Sleep(){
		MySaver.Save();
		// MySaver.SaveObjectDatabase();
		GameManager.Instance.NewDayCutscene();
	}
	public void SleepCutscene(){
		sleeping = true;
		// TODO: peter picklebottom cutscene threshhold
		int collectible = 0;
		foreach(Pickup pickup in GameObject.FindObjectsOfType<Pickup>()){
            if (GameManager.Instance.IsItemCollected(pickup.gameObject))
                collectible++;
        }
		if (collectible > 1)
			CutsceneManager.Instance.InitializeCutscene(CutsceneManager.CutsceneType.pickelbottom);
	}
	void Update(){
		if (sleeping){
			animationTimer += Time.deltaTime;
			if (animationTimer > 1f){
				animationTimer = 0f;
				frame = !frame;
				if (frame){
					head.sprite = headSprites[0];
					bubble.sprite = bubbleSprites[0];
				} else {
					head.sprite = headSprites[1];
					bubble.sprite = bubbleSprites[1];
					audioSource.PlayOneShot(snoreSound);
				}
			}
			if (Input.anyKey && GameObject.Find("NewDayReport(Clone)") == null){
				sleeping = false;
				unmade = true;	
				spriteRenderer.sprite = bedSprites[1];
				head.gameObject.SetActive(false);
				bubble.gameObject.SetActive(false);
				GameManager.Instance.playerObject.SetActive(true);
				audioSource.PlayOneShot(beddingSound);
				if (GameManager.Instance.data.days == 1){
					GameManager.Instance.ShowDiaryEntry("diaryNew");
				}
				if (GameManager.Instance.data.deaths == 1 && GameManager.Instance.data.deathCutscenesPlayed == 0){
					GameManager.Instance.data.deathCutscenesPlayed = 1;
					GameManager.Instance.ShowDiaryEntry("death1");
				}
			}
		}
	}
}
