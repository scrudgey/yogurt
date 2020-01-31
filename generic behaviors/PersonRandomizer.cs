using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Portrait {
    public List<Sprite> sprites;
    public string baseName;
    public SkinColor skinColor;
}

public class PersonRandomizer : MonoBehaviour, ISaveable {
    public Gender defaultGender;
    public bool randomizeGender; // done
    public bool randomizeHead; // done
    public bool randomizePersonality; // done
    public bool randomizeSkinColor; // done
    public bool randomizeDirection; // done
    public bool randomizeSpeech; // +
    public List<Uniform> randomOutfits; // done
    public List<Pickup> randomItems; // +
    public List<Hat> randomHats; // +

    // public Sprite maleHead;
    // public SerializableDictionary<Sprite, string> randomMalePortraitHeads;
    // public SerializableDictionary<Sprite, string> randomFemalePortraitHeads;
    public List<PortraitComponent> maleHeads; // +
    public List<PortraitComponent> femaleHeads; // +
    public float randomHatProbability;
    public float randomItemProbability;
    public float deleteProbability;
    private bool configured = false;
    void LateUpdate() {
        if (configured)
            return;
        if (UnityEngine.Random.Range(0f, 1f) < deleteProbability) {
            Destroy(gameObject);
            return;
        }

        Gender gender = defaultGender;

        if (randomOutfits != null) {
            Outfit outfit = GetComponent<Outfit>();
            Uniform uniform = GameObject.Instantiate(randomOutfits[UnityEngine.Random.Range(0, randomOutfits.Count)]);
            GameObject removedUniform = outfit.DonUniform(uniform, cleanStains: false);
            if (removedUniform)
                Destroy(removedUniform);
        }

        if (randomizeDirection) {
            Vector2 newDirection = UnityEngine.Random.insideUnitCircle.normalized;
            Controllable controllable = GetComponent<Controllable>();
            controllable.direction = newDirection;
        }

        if (randomizeGender) {
            gender = (Gender)UnityEngine.Random.Range(0, 2);
            Toolbox.SetGender(gameObject, gender);
        }

        if (randomizeHead) {
            HeadAnimation head = GetComponentInChildren<HeadAnimation>();
            Portrait portrait = null;
            switch (gender) {
                case Gender.male:
                    // head.baseName = maleHeads[UnityEngine.Random.Range(0, maleHeads.Count)];
                    portrait = maleHeads[UnityEngine.Random.Range(0, maleHeads.Count)].portrait;
                    break;
                case Gender.female:
                    // head.baseName = femaleHeads[UnityEngine.Random.Range(0, femaleHeads.Count)];
                    portrait = femaleHeads[UnityEngine.Random.Range(0, femaleHeads.Count)].portrait;
                    break;
            }
            head.baseName = portrait.baseName;
            Speech speech = GetComponent<Speech>();
            speech.portrait = portrait.sprites.ToArray();
            Toolbox.SetSkinColor(gameObject, portrait.skinColor);
            head.LoadSprites();

            // if (portrait.overrideGenderFemale) {
            //     Toolbox.SetGender(gameObject, Gender.female);
            // } else if (portrait.overrideGenderMale) {
            //     Toolbox.SetGender(gameObject, Gender.male);
            // }
        } else {
            if (randomizeSkinColor) {
                SkinColor skinColor = (SkinColor)UnityEngine.Random.Range(0, 3);
                Toolbox.SetSkinColor(gameObject, skinColor);
            }
        }

        if (randomizePersonality) {
            System.Random random = new System.Random();

            Array braveries = Enum.GetValues(typeof(Personality.Bravery));
            Array actors = Enum.GetValues(typeof(Personality.CameraPreference));
            Array stoicisms = Enum.GetValues(typeof(Personality.Stoicism));
            Array battleStyles = Enum.GetValues(typeof(Personality.BattleStyle));
            Array suggestibles = Enum.GetValues(typeof(Personality.Suggestible));
            Array socials = Enum.GetValues(typeof(Personality.Social));
            Array combatProfficiencies = Enum.GetValues(typeof(Personality.CombatProfficiency));

            Personality personality = new Personality(
                (Personality.Bravery)braveries.GetValue(random.Next(braveries.Length)),
                Personality.CameraPreference.none,
                // (Personality.CameraPreference)actors.GetValue(random.Next(actors.Length)),
                (Personality.Stoicism)stoicisms.GetValue(random.Next(stoicisms.Length)),
                (Personality.BattleStyle)battleStyles.GetValue(random.Next(battleStyles.Length)),
                (Personality.Suggestible)suggestibles.GetValue(random.Next(suggestibles.Length)),
                (Personality.Social)socials.GetValue(random.Next(socials.Length)),
                (Personality.CombatProfficiency)combatProfficiencies.GetValue(random.Next(combatProfficiencies.Length)),
                Personality.PizzaDeliverer.no
            );

            DecisionMaker decisionMaker = GetComponent<DecisionMaker>();
            decisionMaker.personality = personality;
        }

        if (randomItems != null) {
            if (UnityEngine.Random.Range(0f, 1f) < randomItemProbability) {
                Inventory inv = GetComponent<Inventory>();
                Pickup randomItem = GameObject.Instantiate(randomItems[UnityEngine.Random.Range(0, randomItems.Count)], transform.position, Quaternion.identity);
                inv.GetItem(randomItem);
            }
        }

        if (randomHats != null) {
            if (UnityEngine.Random.Range(0f, 1f) < randomHatProbability) {
                Head head = GetComponentInChildren<Head>();
                Hat randomHat = GameObject.Instantiate(randomHats[UnityEngine.Random.Range(0, randomHats.Count)], transform.position, Quaternion.identity);
                head.DonHat(randomHat);
            }
        }

        if (randomizeSpeech) {
            Speech speech = GetComponent<Speech>();
            string name = StartMenu.SuggestAName();
            // gameObject.name = name;
            speech.speechName = name;
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
