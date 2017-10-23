using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

public class Toolbox : Singleton<Toolbox> {
	protected Toolbox () {} // guarantee this will be always a singleton only - can't use the constructor!
	public string message ="smoke weed every day";
	private CameraControl cameraControl;
	private GameObject tom;
	public T GetOrCreateComponent<T>(GameObject g) where T: Component{
		T component = g.GetComponent<T>();
		if (component){
			return component;
		} else {
			component = g.AddComponent<T>();
			return component;
		}
	}
	public Occurrence OccurenceFlag(GameObject spawner, List<OccurrenceData> datas){
        GameObject flag = Instantiate(Resources.Load("OccurrenceFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        Occurrence occurrence = flag.GetComponent<Occurrence>();
		foreach (OccurrenceData datum in datas)
			occurrence.data.Add(datum);
        return occurrence;
    }
	public Occurrence OccurenceFlag(GameObject spawner, OccurrenceData data){
        GameObject flag = Instantiate(Resources.Load("OccurrenceFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        Occurrence occurrence = flag.GetComponent<Occurrence>();
		occurrence.data.Add(data);
        return occurrence;
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
		// source.minDistance = 0.4f;
		source.minDistance = 1f;
		source.maxDistance = 5.42f;
		source.spatialBlend = 1;
		return source;
	}
	public void AudioSpeaker(string clipName, Vector3 position){
		AudioClip clip = Resources.Load("sounds/"+clipName, typeof(AudioClip)) as AudioClip;
		GameObject speaker = Instantiate(Resources.Load("Speaker"), position, Quaternion.identity) as GameObject;
		speaker.GetComponent<AudioSource>().clip = clip;
		speaker.GetComponent<AudioSource>().Play();
	}
	public void AudioSpeaker(AudioClip clip, Vector3 position){
		GameObject speaker = Instantiate(Resources.Load("Speaker"), position, Quaternion.identity) as GameObject;
		speaker.GetComponent<AudioSource>().clip = clip;
		speaker.GetComponent<AudioSource>().Play();
	}
	///<summary>
	///Spawn a droplet of liquid l at poisition pos.
	///</summary>
	public GameObject SpawnDroplet(Vector3 pos, Liquid l){
		/// this is a test
		Vector2 initialVelocity = Vector2.zero;
		initialVelocity = Random.insideUnitCircle;
		if (initialVelocity.y < 0)
			initialVelocity.y = initialVelocity.y * -1;
		return SpawnDroplet(pos, l, initialVelocity);
	}
	public GameObject SpawnDroplet(Vector3 pos, Liquid l, Vector3 initialVelocity){
		GameObject droplet = Instantiate(Resources.Load("droplet"), pos, Quaternion.identity) as GameObject;
		PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
		phys.initHeight = pos.z;
		phys.impactsMiss = true;
		phys.initVelocity = initialVelocity;
		Liquid.MonoLiquidify(droplet, l);
		return droplet;
	}
    public GameObject SpawnDroplet(Liquid l, float severity, GameObject spiller){
        return SpawnDroplet(l, severity, spiller, 0.01f);
    }
    public GameObject SpawnDroplet(Liquid l, float severity, GameObject spiller, float initHeight){
        Vector3 initialVelocity = Vector2.zero;
        Vector3 randomVelocity = Vector2.zero;
        randomVelocity = spiller.transform.right * Random.Range(-0.2f, 0.2f);

        initialVelocity.x = spiller.transform.up.x * Random.Range(0.8f, 1.3f);
        initialVelocity.z = Random.Range(severity, 0.2f + severity);
        initialVelocity.x += randomVelocity.x;
        initialVelocity.z += randomVelocity.y;
        
        GameObject droplet = Instantiate(Resources.Load("droplet"), transform.position, Quaternion.identity) as GameObject;
        PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
		phys.impactsMiss = true;
        Vector2 initpos = spiller.transform.position;
        Physical pb = spiller.GetComponentInParent<Physical>();
        if (pb != null){ 
            initHeight = pb.height; 
        } 
		// else {
		// 	initHeight = 0.05f;
		// }
        droplet.transform.position = initpos;
        phys.doInit = false;
        phys.InitPhysical(initHeight, initialVelocity);
		phys.physical.StartFlyMode();
		Collider2D[] spillerColliders = spiller.transform.root.GetComponentsInChildren<Collider2D>();
		foreach(Collider2D collider in spillerColliders){
			Physics2D.IgnoreCollision(collider, phys.physical.objectCollider, true);
			Physics2D.IgnoreCollision(collider, phys.physical.horizonCollider, true);
		}
        Liquid.MonoLiquidify(droplet, l);
		return droplet;
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
	public void SendMessage(GameObject host, Component messenger, Message message, bool sendUpwards = true){
		message.messenger = messenger;
		IMessagable[] childReceivers = host.GetComponentsInChildren<IMessagable>();
		List<IMessagable> receivers = new List<IMessagable>(childReceivers);
		if (sendUpwards){
			foreach(IMessagable parentReceiver in host.GetComponentsInParent<IMessagable>()){
				if (!receivers.Contains(parentReceiver))
					receivers.Add(parentReceiver);
			}
		}
		foreach (IMessagable receiver in receivers){
			receiver.ReceiveMessage(message);
		}
	}
	public List<Intrinsic> AddIntrinsic(GameObject host, GameObject donor, bool timeout=true){
		Intrinsics intrinsics = GetOrCreateComponent<Intrinsics>(host);
		Intrinsics donorIntrinsics = GetOrCreateComponent<Intrinsics>(donor);
		return intrinsics.AddIntrinsic(donorIntrinsics, timeout:timeout);
	}
	public void AddIntrinsic(GameObject host, Intrinsic intrinsic){
		if (intrinsic == null)
			return;
		Intrinsics intrinsics = GetOrCreateComponent<Intrinsics>(host);
		intrinsics.AddIntrinsic(intrinsic);
	}
	public void RemoveIntrinsic(GameObject host, GameObject donor){
		Intrinsics intrinsics = GetOrCreateComponent<Intrinsics>(host);
		Intrinsics donorIntrinsics = GetOrCreateComponent<Intrinsics>(donor);
		intrinsics.RemoveIntrinsic(donorIntrinsics);
	}
	public string DirectionToString(Vector2 direction){
		float angle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
		string lastPressed = "right";
		// change lastpressed because this is relevant to animation
		if (angle > 315 || angle < 45){
			lastPressed = "right";
		} else if (angle >= 45 && angle <= 135) {
			lastPressed = "up";
		} else if (angle >= 135 && angle < 225) {
			lastPressed = "left";
		} else if (angle >= 225 && angle < 315) {
			lastPressed = "down";
		}
		return lastPressed;
	}
	public void DisableAndReenable (MonoBehaviour target, float time){
		StartCoroutine(EnableAfterSeconds(target, 1f));
	}
	public IEnumerator EnableAfterSeconds (MonoBehaviour target, float time){
        target.enabled = false;
		// Debug.Log("disabling "+target.ToString()+" on "+target.gameObject.name);
		// Debug.Log(Time.time);
        yield return new WaitForSeconds(time);
		// Debug.Log("enabling "+target.ToString()+" on "+target.gameObject.name);
        target.enabled = true;
		// Debug.Log(Time.time);
		yield return null;
    }
}
