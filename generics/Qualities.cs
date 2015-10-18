using UnityEngine;
using System.Collections;

[System.Serializable]
public class Quality{
	public bool protection;
	public bool weapon;
	public bool edible;
	public enum toolType{none,fireExt}
	public toolType tool;
	public bool flaming;
	public float disgusting;

}

public class Qualities : MonoBehaviour {
	public Quality quality = new Quality();
}
