using UnityEngine;
// using System.Collections;
//using Vexe.Runtime.Serialization;

public class saverloader : MonoBehaviour {

//	public GameObject savetarget;
//	private StoreManager store;

//	void Start() {
//		SaveManager.Converters.Add(typeof(Rigidbody2D), () => new Rigidbody2DConverter() );
//		SaveManager.Converters.Add(typeof(CircleCollider2D), () => new CircleCollider2DConverter() );
//		SaveManager.Converters.Add(typeof(SpriteRenderer), () => new SpriteRendererConverter() );
//		SaveManager.Converters.Add(typeof(BoxCollider2D), () => new BoxCollider2DConverter() );
//		SaveManager.Converters.Add(typeof(PolygonCollider2D), () => new PolygonCollider2DConverter() );
//		SaveManager.Converters.Add(typeof(EdgeCollider2D), () => new EdgeCollider2DConverter() );
//
//		SaveManager.Converters.Add(typeof(SpringJoint2D), () => new SpringJoint2DConverter() );
//		SaveManager.Converters.Add(typeof(DistanceJoint2D), () => new DistanceJoint2DConverter() );
//		SaveManager.Converters.Add(typeof(HingeJoint2D), () => new HingeJoint2DConverter() );
//		SaveManager.Converters.Add(typeof(SliderJoint2D), () => new SliderJoint2DConverter() );
//		SaveManager.Converters.Add(typeof(WheelJoint2D), () => new WheelJoint2DConverter() );
//
//	}

	void Save(){
		MySaver.Save();
	}

	void Load(){
		MySaver.LoadScene();
	}

	void SpawnEggplant(){
		Instantiate(Resources.Load("prefabs/eggplant"));
	}

	void OnGUI(){
		if (GUI.Button(new Rect(110,10,100,30),"Save") ){
			Save ();
		}
		if (GUI.Button(new Rect(110,40,100,30), "Load") ){
			Load ();
		}
		if (GUI.Button(new Rect(110,70,100,30),"Eggplant")){
			SpawnEggplant();
		}
	}

}
