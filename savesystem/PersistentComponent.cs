// using UnityEngine;
// using System.Collections;
using System.Collections.Generic;
// using System.Xml;
using System.Xml.Serialization;

public class PersistentComponent {

	public string type;
	public string parentObject;
	public SerializableDictionary<string,string> strings =		 new SerializableDictionary<string,string>();
	public SerializableDictionary<string,int> ints = 			 new SerializableDictionary<string,int> ();
	public SerializableDictionary<string,float> floats = 		 new SerializableDictionary<string,float>() ;
	public SerializableDictionary<string,bool> bools = 			 new SerializableDictionary<string,bool >();
	public List<Intrinsic> intrinsics;
	[XmlIgnoreAttribute]	
	public Persistent persistent;
	public PersistentComponent(){
		// Needed for XML serialization
	}
	public PersistentComponent(Persistent owner){
		persistent = owner;
	}

}
