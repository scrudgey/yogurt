using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.IO;
public class VideoCamera : MonoBehaviour {
	BoxCollider2D view;
	Text rec;
	Image recBox;
	float blinkTimer;
	// public float yogurtsEaten;
	
	public Commercial commercial = new Commercial();
	
	void Start () {
		view = transform.Find("Graphic").GetComponent<BoxCollider2D>();
		rec = transform.Find("Graphic/Rec").GetComponent<Text>();
		recBox = transform.Find("Graphic").GetComponent<Image>();
	}
	
	void Update(){
		blinkTimer += Time.deltaTime;
		if (blinkTimer > 1 && rec.enabled == true){
			rec.enabled = false;
			// recBox.enabled = false;
		}
		if (blinkTimer > 2){
			rec.enabled = true;
			// recBox.enabled = true;
			blinkTimer = 0f;
		}
	}
	
	void OnTriggerEnter2D(Collider2D col){
		if (col.name != "OccurrenceFlag(Clone)")
		return;
		Occurrence occurrence = col.gameObject.GetComponent<Occurrence>();
		if (occurrence == null)
		return;
		
		if (occurrence.subjectName.Contains("yogurt") && occurrence.functionName == "Drink"){
			Debug.Log("yogurt eaten");
			commercial.properties["yogurt"].val ++;
			// yogurtsEaten ++;
			SaveCommercial();
		}
	}
	
	void SaveCommercial(){
		var serializer = new XmlSerializer(typeof(Commercial));
			string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
			path = Path.Combine(path, "commercial.xml");
			// GameData data = new GameData(this);
			FileStream sceneStream = File.Create(path);
			serializer.Serialize(sceneStream, commercial);
			sceneStream.Close();
	}
}
