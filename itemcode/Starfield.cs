using UnityEngine;
using System.Collections.Generic;

public class Starfield : MonoBehaviour {
    Vector3 extents;
    public float minTime = 2f;
    public float maxTime = 10f;
    public int maxStars = 100;
    private GameObject star;
    
    public List<Color> colors = new List<Color>()  {
        Color.green,
        Color.blue,
        Color.cyan
    }; 
    // {
    //     Color.red,
    //     Color.white,
    //     Color.yellow
    // }; 
    
	void Start () {
       BoxCollider2D collider = GetComponent<BoxCollider2D>();
       extents = collider.bounds.extents;
       star = Resources.Load("star_pixel") as GameObject;
       for (int i = 0; i < maxStars; i++){
           NewStar(true);
       }
	}
	
    void NewStar(){
        NewStar(false);
    }
    
    void NewStar(bool init){
        Vector3 position = new Vector3();
        position.x = Random.Range(-1.0f * extents.x, extents.x);
        position.y = Random.Range(-1.0f * extents.y, extents.y);
        position += transform.position;
        GameObject newStar = Instantiate(star, position, Quaternion.identity) as GameObject;
        newStar.transform.parent = transform;
        SpriteRenderer starRenderer = newStar.GetComponent<SpriteRenderer>();
        starRenderer.sortingLayerName = "background";
        starRenderer.color = colors[Random.Range(0, colors.Count)];
        if (init){
            Destroy(newStar, Random.Range(0f, maxTime));
        } else {
            Destroy(newStar, Random.Range(minTime, maxTime));
        }
    }

    void Update(){
        if (transform.childCount < maxStars){
            NewStar();
        }
    }
}
