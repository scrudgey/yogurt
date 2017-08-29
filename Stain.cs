using UnityEngine;

public class Stain : MonoBehaviour {
	public SpriteMask parentMask;
	public SpriteRenderer parentRenderer;
	public GameObject parent;
	void Update(){
		if (parentRenderer != null){
			parentMask.sprite = parentRenderer.sprite;
		}
	}
	public void ConfigureParentObject(GameObject parent){
		this.parent = parent;
		parentMask = Toolbox.Instance.GetOrCreateComponent<SpriteMask>(parent);
		parentRenderer = parent.GetComponent<SpriteRenderer>();
		SpriteRenderer stainRenderer = GetComponent<SpriteRenderer>();
		stainRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
		transform.SetParent(parent.transform, true);
	}
	public void RemoveStain(){
		Destroy(gameObject);
	}
}
