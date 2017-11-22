using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot("PersistentContainer")]
public class PersistentContainer{
	[XmlArray("PersistentObjects"), XmlArrayItem("Persistent")]
	public List<PersistentObject> PersistentObjects = new List<PersistentObject>();
	public PersistentContainer(){
	}
	public PersistentContainer(List<PersistentObject> persistents){
		PersistentObjects = persistents;
	}
}
