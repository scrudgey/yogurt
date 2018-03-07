using UnityEngine;
using UnityEngine.UI;

public class UIStatusIcon : MonoBehaviour {
    public Buff buff;
    public RectTransform lifeBar;
    public Text text;
    private Vector2 lifebarDefaultSize;
    public void Initialize(BuffType type, Buff buff) {
        this.buff = buff;
        text = transform.Find("Text").GetComponent<Text>();
        lifeBar = transform.Find("lifebar/mask/fill").GetComponent<RectTransform>();

        text.text = type.ToString();
        if (lifebarDefaultSize == Vector2.zero)
            lifebarDefaultSize = new Vector2(lifeBar.rect.width, lifeBar.rect.height);
        if (buff.lifetime == 0) {
            transform.Find("lifebar").gameObject.SetActive(false);
        }
    }
    public void Update() {
        if (buff == null)
            Destroy(gameObject);
        if (buff.lifetime > 0) {
            float width = (1 - (buff.time / buff.lifetime)) * lifebarDefaultSize.x;
            lifeBar.sizeDelta = new Vector2(width, lifebarDefaultSize.y);
        }
    }
}
