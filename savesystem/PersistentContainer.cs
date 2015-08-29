using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Runtime.Serialization.

[XmlRoot("PersistentContainer")]
public class PersistentContainer{

	[XmlArray("PersistentObjects"),XmlArrayItem("Persistent")]
	public List<Persistent> PersistentObjects = new List<Persistent>();


	public PersistentContainer(){
	}

//	public PersistentContainer(
//		List<GameObject> gameObjectList, 
//		Dictionary<GameObject, int> idRegister, 
//		int initialId)
//	{
//		
//		int idIndex = initialId;
//		// create a persistent for each gameobject with the appropriate data
//		foreach (GameObject gameObject in gameObjectList){
//			Persistent persistent = new Persistent(gameObject);
//			persistent.id = idIndex;
//			PersistentObjects.Add(persistent);
//			idRegister.Add(gameObject,idIndex);
//			idIndex++;
//		}
//
//
//	}

	public PersistentContainer( List<Persistent> persistents){
		PersistentObjects = persistents;
	}

	
}
