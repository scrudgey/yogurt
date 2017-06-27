using System;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

[System.Serializable]
public class Email {
	public string filename;
	public string fromString;
	public string toString;
	public string subject;
	public string content;
	public bool read;

	public static Email LoadEmail(string filename){
		try{
            Email newEmail = null;
            TextAsset xml = Resources.Load("data/emails/"+filename) as TextAsset;
            var serializer = new XmlSerializer(typeof(Email));
            var reader = new System.IO.StringReader(xml.text);
            newEmail = serializer.Deserialize(reader) as Email;
			newEmail.filename = filename;
            return newEmail;
        } catch(Exception e){
            Debug.Log(e.Message);
            return null;
        }
	}

	public static void SaveEmail(Email email){
		var serializer = new XmlSerializer(typeof(Email));
		string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
		path = Path.Combine(path, "email.xml");
		FileStream sceneStream = File.Create(path);
		serializer.Serialize(sceneStream, email);
		sceneStream.Close();
	}
}
