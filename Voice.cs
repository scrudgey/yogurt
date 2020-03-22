using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voice : MonoBehaviour {
    public string speechSet;
    public List<AudioClip> sounds;
    public LoHi randomPitchLow;
    public LoHi randomPitchHigh;
    public LoHi randomSpacingLow;
    public LoHi randomSpacingHigh;
}
