using UnityEngine;
// using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
// using System.Xml.Serialization;
// using System.IO;

public static class IDictionaryExtensions
{
	public static TKey FindKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
	{
		TKey returnkey = default (TKey);
		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			if (value.Equals(pair.Value)) returnkey = pair.Key;
		return returnkey;
	}
}
///<summary>
/// str - the source string
/// index- the start location to replace at (0-based)
/// length - the number of characters to be removed before inserting
/// replace - the string that is replacing characters
///</summary>
// public static string ReplaceAt(this string str, int index, int length, string replace)
// 	{
//     return str.Remove(index, Mathf.Min(length, str.Length - index))
//             .Insert(index, replace);
// }

public class Toolbox : Singleton<Toolbox> {

	protected Toolbox () {} // guarantee this will be always a singleton only - can't use the constructor!

	public string message ="smoke weed every day";
	private CameraControl cameraControl;
	private GameObject tom;

	public Qualities GetQuality(GameObject g){
		Qualities q = g.GetComponent<Qualities>();
		if (q){
			return q;
		} else {
			q = g.AddComponent<Qualities>();
			return q;
		}
	}

	public T GetOrCreateComponent<T>(GameObject g) where T: Component{
		T component = g.GetComponent<T>();
		if (component){
			return component;
		} else {
			component = g.AddComponent<T>();
			return component;
		}
	}

    public void PopupCounter(string text, float initValue, float finalValue, VideoCamera video){
		GameObject existingPop = GameObject.Find("Poptext(Clone)");
		if (existingPop == null){
			GameObject pop = Instantiate(Resources.Load("UI/Poptext")) as GameObject;
			Canvas popCanvas = pop.GetComponent<Canvas>();
			popCanvas.worldCamera = GameManager.Instance.cam;
			
			Poptext poptext = pop.GetComponent<Poptext>();
			// poptext.description = text;
			poptext.description.Add(text);
			poptext.initValueList.Add(initValue);
			poptext.finalValueList.Add(finalValue);
			poptext.video = video;
		} else {
			Poptext poptext = existingPop.GetComponent<Poptext>();
			poptext.description.Add(text);
			poptext.initValueList.Add(initValue);
			poptext.finalValueList.Add(finalValue);
		}

    }
	
	public void BounceText(string text, GameObject target){
		GameObject bounce = Instantiate(Resources.Load("UI/BounceText")) as GameObject;
		BounceText bounceScript = bounce.GetComponent<BounceText>();
		if (target){
			bounceScript.target = target;
		}
		bounceScript.text = text;
	}
    
    public Occurrence OccurenceFlag(GameObject spawner){
        GameObject flag = Instantiate(Resources.Load("OccurrenceFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        Occurrence occurrence = flag.GetComponent<Occurrence>();
        return occurrence;
    }

	public Occurrence DataFlag(GameObject spawner, float chaos, float disgusting, float disturbing, float offensive, float positive){
		GameObject flag = Instantiate(Resources.Load("OccurrenceFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        Occurrence occurrence = flag.GetComponent<Occurrence>();

		OccurrenceData data = new OccurrenceData();
		data.chaos = chaos;
		data.disgusting = disgusting;
		data.disturbing = disturbing;
		data.offensive = offensive;
		data.positive = positive;

		occurrence.data.Add(data);

        return occurrence;
	}

	public AudioSource SetUpAudioSource(GameObject g){
		AudioSource source = g.GetComponent<AudioSource>();
		if (!source){
			source = g.AddComponent<AudioSource>();
		}
		source.rolloffMode = AudioRolloffMode.Logarithmic;
		source.minDistance = 0.4f;
		source.maxDistance = 5.42f;
		return source;
	}

	///<summary>
	///Spawn a droplet of liquid l at poisition pos.
	///</summary>
	public void SpawnDroplet(Vector3 pos, Liquid l){
		/// this is a test
		Vector2 initialVelocity = Vector2.zero;
		initialVelocity = Random.insideUnitCircle;
		if (initialVelocity.y < 0)
			initialVelocity.y = initialVelocity.y * -1;
		GameObject droplet = Instantiate(Resources.Load("droplet"), pos, Quaternion.identity) as GameObject;
		PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
		phys.initHeight = 0.05f;
		phys.initVelocity = initialVelocity;
		phys.ignoreCollisions = true;
		LiquidCollection.MonoLiquidify(droplet, l);
	}
    
    public void SpawnDroplet(Liquid l, float severity, GameObject spiller){
        SpawnDroplet(l, severity, spiller, 0.01f);
    }
    public void SpawnDroplet(Liquid l, float severity, GameObject spiller, float initHeight){
        Vector3 initialVelocity = Vector2.zero;
        Vector3 randomVelocity = Vector2.zero;
        randomVelocity = transform.right * Random.Range(-0.2f, 0.2f);
        initialVelocity.x = transform.up.x * Random.Range(0.8f, 1.3f);
        initialVelocity.z = Random.Range(severity, 0.2f + severity);
        initialVelocity.x += randomVelocity.x;
        initialVelocity.z += randomVelocity.y;
        
        GameObject droplet = Instantiate(Resources.Load("droplet"), transform.position, Quaternion.identity) as GameObject;
        PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
        
        Vector2 initpos = spiller.transform.position;
        // float initHeight = 0.01f;
        phys.ignoreCollisions = true;
        Physical pb = spiller.GetComponentInParent<Physical>();
        if (pb != null){ 
            initHeight = pb.height; 
        } else {
			initHeight = 0.05f;
		}
        droplet.transform.position = initpos;
        phys.doInit = false;
        phys.InitPhysical(initHeight, initialVelocity);
		phys.physical.StartFlyMode();
		Collider2D[] spillerColliders = spiller.transform.root.GetComponentsInChildren<Collider2D>();
		foreach(Collider2D collider in spillerColliders){
			Physics2D.IgnoreCollision(collider, phys.physical.objectCollider, true);
			Physics2D.IgnoreCollision(collider, phys.physical.groundCollider, true);
		}
        LiquidCollection.MonoLiquidify(droplet, l);
    }

	public Component CopyComponent(Component original, GameObject destination)
	{
		System.Type type = original.GetType();
		Component copy = destination.AddComponent(type);
		// Copied fields can be restricted with BindingFlags
		System.Reflection.FieldInfo[] fields = type.GetFields(); 
		foreach (System.Reflection.FieldInfo field in fields)
		{
			field.SetValue(copy, field.GetValue(original));
		}
		return copy;
	}

	public Vector2 RotateZ(Vector2 v, float angle )
		
	{
		float sin = Mathf.Sin( angle );
		float cos = Mathf.Cos( angle );

		float tx = v.x;
		float ty = v.y;
		
		v.x = (cos * tx) - (sin * ty);
		v.y = (cos * ty) + (sin * tx);

		return v;
	}

	public float ProperAngle(float x,float y){
		float angle = Mathf.Atan2(y,x)* Mathf.Rad2Deg;
		if (angle < 0)
			angle += 360;
		return angle ;
	}

	public string ScrubText(string input){
		string output = "";
		if (input != null){
			Regex cloneFinder = new Regex(@"(.+)\(Clone\)$", RegexOptions.Multiline);
			Regex underscoreFinder = new Regex(@"_", RegexOptions.Multiline);
			string name = input;
			MatchCollection matches = cloneFinder.Matches(input);
			if (matches.Count > 0){									// the object is a clone, capture just the normal name
				foreach (Match match in matches){
					name = match.Groups[1].Value;
				}
			}
			output = underscoreFinder.Replace(name, " ");
		}
		return output;
	}

	public string ReplaceUnderscore(string input){
		string output = "";
		if (input != null){
			Regex spaceFinder = new Regex(@" ", RegexOptions.Multiline);
			output = spaceFinder.Replace(input, "_");
		}
		return output;
	}

	public string CloneRemover(string input){
		string output = input;
		if (input != null){
			Regex cloneFinder = new Regex(@"(.+)\(Clone\)$", RegexOptions.Multiline);
			MatchCollection matches = cloneFinder.Matches(input);
			if (matches.Count > 0){									// the object is a clone, capture just the normal name
				foreach (Match match in matches){
					output = match.Groups[1].Value;
				}
			}
		}
		return output;
	}

	public string GetName(GameObject obj){
		// TODO: include extra description, like "vomited up"
		// possibly also use intrinsics
		string nameOut = "";
		Item item = obj.GetComponent<Item>();
		if (item){
			nameOut = item.itemName;
		} else {
			nameOut = obj.name;
		}
		Edible edible = obj.GetComponent<Edible>();
		if (edible){
			if (edible.vomit)
				nameOut = "vomited-up "+nameOut;
		}
		nameOut = ScrubText(nameOut);
		return nameOut;
	}



}
