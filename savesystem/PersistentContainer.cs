// using System.Xml.Serialization;

// [XmlRoot("PersistentContainer")]
public class PersistentContainer{
	// [XmlArray("PersistentObjects"), XmlArrayItem("Persistent")]
	public SerializableDictionary<int, PersistentObject> PersistentObjects = new SerializableDictionary<int, PersistentObject>();
	public PersistentContainer(){
	}
	public PersistentContainer(SerializableDictionary<int, PersistentObject> persistents){
		PersistentObjects = persistents;
	}
}

