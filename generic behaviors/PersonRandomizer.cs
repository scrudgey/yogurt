using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonRandomizer : MonoBehaviour, ISaveable {
    public bool randomizeGender;
    public bool randomizeSkinColor;
    public bool randomizeDirection;
    public List<Uniform> randomOutfits;
    private bool configured = false;
    void LateUpdate() {
        if (configured)
            return;
        if (randomOutfits != null) {
            Outfit outfit = GetComponent<Outfit>();
            Uniform uniform = GameObject.Instantiate(randomOutfits[Random.Range(0, randomOutfits.Count)]);
            GameObject removedUniform = outfit.DonUniform(uniform, cleanStains: false);
            if (removedUniform)
                Destroy(removedUniform);
        }
        if (randomizeGender) {
            Gender newGender = (Gender)Random.Range(0, 2);
            Toolbox.SetGender(gameObject, newGender);
        }
        if (randomizeDirection) {
            Vector2 newDirection = Random.insideUnitCircle.normalized;
            Controllable controllable = GetComponent<Controllable>();
            controllable.direction = newDirection;
        }
        configured = true;
    }

    public void SaveData(PersistentComponent data) {
        data.bools["configured"] = configured;
    }
    public void LoadData(PersistentComponent data) {
        configured = data.bools["configured"];
    }
}
