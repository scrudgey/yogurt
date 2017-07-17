using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot("PersistentContainer")]
public class PersistentContainer{
	[XmlArray("PersistentObjects"), XmlArrayItem("Persistent")]
	public List<Persistent> PersistentObjects = new List<Persistent>();
	public PersistentContainer(){
	}
	public PersistentContainer(List<Persistent> persistents){
		PersistentObjects = persistents;
	}
}
