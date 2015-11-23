using UnityEngine;
// using System.Collections;
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

	public void SpawnDroplet(Vector3 pos, Liquid l){
		Vector2 initialVelocity = Vector2.zero;
		initialVelocity = Random.insideUnitCircle;
		if (initialVelocity.y < 0)
			initialVelocity.y = initialVelocity.y * -1;
		
		GameObject droplet = Instantiate(Resources.Load("droplet"),pos,Quaternion.identity) as GameObject;
		PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
		phys.initHeight = 0.05f;
		phys.initVelocity = initialVelocity;
		phys.ignoreCollisions = true;

		LiquidCollection.MonoLiquidify(droplet,l);
	}

	#region utility functions

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

	#endregion

}
