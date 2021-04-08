using UnityEngine;

public class Duplicator : Interactive, IDirectable {
    public AudioClip dupSound;
    public AudioClip failSound;
    private AudioSource audioSource;
    public ParticleSystem particles;
    public void DirectionChange(Vector2 d) {
        if (particles) {
            particles.transform.rotation = Quaternion.AngleAxis(Toolbox.Instance.ProperAngle(d.x, d.y) - 20f, new Vector3(0, 0, 1));
        }
    }
    void Start() {
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        Interaction dup = new Interaction(this, "Duplicate", "Duplicate");
        // dup.debug = true;
        dup.unlimitedRange = true;
        dup.validationFunction = true;
        interactions.Add(dup);
    }
    public void Duplicate(Duplicatable duplicatable) {
        Vector3 jitter = new Vector3(Random.Range(0, 0.1f), Random.Range(0, 0.1f), 0);
        GameObject dupObj = Instantiate(duplicatable.duplicationPrefab, duplicatable.transform.position + jitter, Quaternion.identity) as GameObject;
        dupObj.name = Toolbox.Instance.CloneRemover(dupObj.name);
        audioSource.PlayOneShot(dupSound);
        Instantiate(Resources.Load("particles/poof"), dupObj.transform.position, Quaternion.identity);
        if (duplicatable.duplicationPrefab.name == "dollar") {
            GameManager.Instance.IncrementStat(StatType.dollarsDuplicated, 1);
        }
        if (duplicatable.gameObject == GameManager.Instance.playerObject) {
            Duplicatable newDuplicatabale = dupObj.GetComponent<Duplicatable>();
            if (newDuplicatabale) {
                newDuplicatabale.adoptedName = GameManager.Instance.saveGameName;
            }
        }
        if (particles != null) {
            particles.Play();
        }

        EventData nullData = EventData.Duplication(duplicatable.gameObject);
        Toolbox.Instance.OccurenceFlag(gameObject, nullData);
    }
    public bool Duplicate_Validation(Duplicatable duplicatable) {
        if (!duplicatable.duplicatable)
            return false;
        if (duplicatable.duplicationPrefab == null)
            return false;
        if (duplicatable.gameObject == gameObject) {
            return false;
        } else {
            return true;
        }
    }
    public string Duplicate_desc(Duplicatable duplicatable) {
        return "Duplicate " + Toolbox.Instance.GetName(duplicatable.gameObject);
    }
}
