using UnityEngine;
public class HatAnimation : MonoBehaviour, IDirectable {
	public Sprite rightSprite;
	public Sprite downSprite;
	public Sprite upSprite;
	private SpriteRenderer spriteRenderer;
	public string lastPressed;
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	public void RegisterDirectable(){
		MessageDirectable directableMessage = new MessageDirectable();
		directableMessage.addDirectable = (IDirectable)this;
		Toolbox.Instance.SendMessage(gameObject, this, directableMessage);
	}
	public void RemoveDirectable(){
		MessageDirectable directableMessage = new MessageDirectable();
		directableMessage.removeDirectable = (IDirectable)this;
		Toolbox.Instance.SendMessage(gameObject, this, directableMessage);
	}
	public void UpdateSprite(){
		if (spriteRenderer == null)
			return;
		switch (lastPressed){
		case "down":
			spriteRenderer.sprite = downSprite;
			break;
		case "up":
			spriteRenderer.sprite = upSprite;
			break;
		default:
			spriteRenderer.sprite = rightSprite;
			break;
		}
	}
	public void DirectionChange(Vector2 newdir){
		lastPressed = Toolbox.Instance.DirectionToString(newdir);
		UpdateSprite();
	}
	public void OnDestroy(){
		// Debug.Log("hatanimation on OnDestroy");
		// RemoveDirectable();
	}
}
