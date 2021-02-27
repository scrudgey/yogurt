using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Nimrod;

[System.Serializable]
public class Portrait {
    public List<Sprite> sprites;
    public string baseName;
    public SkinColor skinColor;
}

public class PersonRandomizer : MonoBehaviour, ISaveable {
    public Gender defaultGender;
    public bool normalName;
    public bool randomizeGender;
    public bool randomizeHead;
    public bool randomizePersonality;
    public bool randomizeSkinColor;
    public bool randomizeDirection;
    public bool randomizeSpeech;
    public List<Uniform> randomOutfits;
    public List<Uniform> randomFemaleOutfits;
    public List<Pickup> randomItems;
    public List<Hat> randomHats;
    public List<PortraitComponent> maleHeads;
    public List<PortraitComponent> femaleHeads;
    public List<Voice> randomVoices;
    public List<String> randomFlavor;
    // public List<String> randomNames;
    public String randomNameGrammar;
    public float randomHatProbability;
    public float randomItemProbability;
    public float deleteProbability;
    public bool configured = false;
    public bool continuous;
    public void LateUpdate() {
        if (configured)
            return;
        if (UnityEngine.Random.Range(0f, 1f) < deleteProbability) {
            Destroy(gameObject);
            return;
        }

        Gender gender = defaultGender;

        if (randomizeGender) {
            gender = (Gender)UnityEngine.Random.Range(0, 2);
            Toolbox.SetGender(gameObject, gender);
        }

        if (randomizeDirection) {
            Vector2 newDirection = UnityEngine.Random.insideUnitCircle.normalized;
            Controllable controllable = GetComponent<Controllable>();
            controllable.direction = newDirection;
        }

        Outfit outfit = GetComponent<Outfit>();
        Uniform uniform = null;

        if (outfit != null)
            switch (gender) {
                case Gender.male:
                    if (randomOutfits != null && randomOutfits.Count > 0) {
                        uniform = GameObject.Instantiate(randomOutfits[UnityEngine.Random.Range(0, randomOutfits.Count)]);
                        GameObject removedUniform = outfit.DonUniform(uniform, cleanStains: false);
                        if (removedUniform)
                            Destroy(removedUniform);
                    }
                    break;
                case Gender.female:
                    if (randomFemaleOutfits != null && randomFemaleOutfits.Count > 0) {
                        uniform = GameObject.Instantiate(randomFemaleOutfits[UnityEngine.Random.Range(0, randomFemaleOutfits.Count)]);
                        GameObject removedUniform = outfit.DonUniform(uniform, cleanStains: false);
                        if (removedUniform)
                            Destroy(removedUniform);
                    }
                    break;
            }


        HeadAnimation headAnimation = GetComponentInChildren<HeadAnimation>();

        if (randomizeHead && headAnimation != null) {
            Portrait portrait = null;
            switch (gender) {
                case Gender.male:
                    portrait = maleHeads[UnityEngine.Random.Range(0, maleHeads.Count)].portrait;
                    break;
                case Gender.female:
                    portrait = femaleHeads[UnityEngine.Random.Range(0, femaleHeads.Count)].portrait;
                    break;
            }
            headAnimation.baseName = portrait.baseName;
            Speech speech = GetComponent<Speech>();
            speech.portrait = portrait.sprites.ToArray();
            Toolbox.SetSkinColor(gameObject, portrait.skinColor);
            headAnimation.LoadSprites();
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

            List<Personality.CameraPreference> camPrefs = new List<Personality.CameraPreference>{
                Personality.CameraPreference.ambivalent,
                Personality.CameraPreference.avoidant,
                Personality.CameraPreference.excited,
                Personality.CameraPreference.none,
                };

            Personality personality = new Personality(
                (Personality.Bravery)braveries.GetValue(random.Next(braveries.Length)),
                camPrefs[random.Next(camPrefs.Count)],
                (Personality.Stoicism)stoicisms.GetValue(random.Next(stoicisms.Length)),
                (Personality.BattleStyle)battleStyles.GetValue(random.Next(battleStyles.Length)),
                (Personality.Suggestible)suggestibles.GetValue(random.Next(suggestibles.Length)),
                (Personality.Social)socials.GetValue(random.Next(socials.Length)),
                (Personality.CombatProfficiency)combatProfficiencies.GetValue(random.Next(combatProfficiencies.Length)),
                Personality.PizzaDeliverer.no,
                Personality.Dancer.no,
                Personality.Haunt.no
            );

            DecisionMaker decisionMaker = GetComponent<DecisionMaker>();
            if (decisionMaker != null) {
                decisionMaker.personality = personality;
                decisionMaker.Initialize();
            }
        }

        if (randomItems != null && randomItems.Count > 0) {
            if (UnityEngine.Random.Range(0f, 1f) < randomItemProbability) {
                Inventory inv = GetComponent<Inventory>();
                Pickup randomItem = GameObject.Instantiate(randomItems[UnityEngine.Random.Range(0, randomItems.Count)], transform.position, Quaternion.identity);
                inv.GetItem(randomItem);
            }
        }
        Head head = GetComponentInChildren<Head>();

        if (head != null && randomHats != null && randomHats.Count > 0) {
            if (UnityEngine.Random.Range(0f, 1f) < randomHatProbability) {
                Hat randomHat = GameObject.Instantiate(randomHats[UnityEngine.Random.Range(0, randomHats.Count)], transform.position, Quaternion.identity);
                GameObject removed = head.DonHat(randomHat);
                if (removed != null) {
                    Destroy(removed);
                }
            }
        }

        if (randomizeSpeech) {
            Speech speech = GetComponent<Speech>();
            speech.flavor = randomFlavor[UnityEngine.Random.Range(0, randomFlavor.Count)];
            speech.LoadGrammar();
            if (normalName) {
                speech.speechName = Toolbox.SuggestNormalName(gender);
            } else {
                speech.speechName = Toolbox.SuggestWeirdName();
            }
            Voice voice = randomVoices[UnityEngine.Random.Range(0, randomVoices.Count)];
            speech.voice = voice.speechSet;
            speech.speakSounds = voice.sounds.ToArray();
            float pitchLow = UnityEngine.Random.Range(voice.randomPitchLow.low, voice.randomPitchLow.high);
            float pitchHigh = UnityEngine.Random.Range(voice.randomPitchHigh.low, voice.randomPitchHigh.high);
            float spacingLow = UnityEngine.Random.Range(voice.randomSpacingLow.low, voice.randomSpacingLow.high);
            float spacingHigh = UnityEngine.Random.Range(voice.randomSpacingHigh.low, voice.randomSpacingHigh.high);

            speech.pitchRange = new Vector2(pitchLow, pitchHigh);
            speech.spacingRange = new Vector2(spacingLow, spacingHigh);

            SoundGibberizer gibberizer = GetComponent<SoundGibberizer>();
            gibberizer.pitchRange = speech.pitchRange;
            gibberizer.spacingRange = speech.spacingRange;
            gibberizer.sounds = speech.speakSounds;
        }
        if (randomNameGrammar != null && randomNameGrammar != "") {
            Speech speech = GetComponent<Speech>();
            Grammar grammar = new Grammar();
            grammar.Load(randomNameGrammar);
            speech.speechName = grammar.Parse("{main}");
        }
        // if (randomNames.Count > 0) {
        //     Speech speech = GetComponent<Speech>();
        //     speech.speechName = randomNames[UnityEngine.Random.Range(0, randomNames.Count)];
        // }
        Awareness awareness = GetComponent<Awareness>();
        if (awareness) {
            awareness.socializationTimer = -1f * UnityEngine.Random.Range(0f, 10f);
        }
        if (!continuous)
            configured = true;
    }

    public void SaveData(PersistentComponent data) {
        data.bools["configured"] = configured;
    }
    public void LoadData(PersistentComponent data) {
        configured = data.bools["configured"];
    }
}
