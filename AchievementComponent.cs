using UnityEngine;

public class AchievementComponent : MonoBehaviour {
	public Achievement achivement;
}

[System.Serializable]
public class Achievement {
	public string iconName;
	public bool complete;
	public string title;
	public string description;
	public AchievementStats stats = new AchievementStats();
	public bool Evaluate(AchievementStats otherStats){
		bool pass = true;
		if (stats.secondsPlayed > 0)
			pass = pass && otherStats.secondsPlayed >= stats.secondsPlayed;
		if (stats.yogurtEaten > 0)
			pass = pass && otherStats.yogurtEaten >= stats.yogurtEaten;
		if (stats.vomit > 0)
			pass = pass && otherStats.vomit >= stats.vomit;
		return pass;
	}
	public Achievement(){ }
	public Achievement(Achievement source){
		iconName = source.iconName;
		complete = source.complete;
		title = source.title;
		description = source.description;
		stats = new AchievementStats(source.stats);
	}
}

[System.Serializable]
public class AchievementStats {
	public float secondsPlayed;
	public float yogurtEaten;
	public float vomit;
	public float yogurtVomit;
	public AchievementStats(){ }
	public AchievementStats(AchievementStats source){
		secondsPlayed = source.secondsPlayed;
		yogurtEaten = source.yogurtEaten;
		vomit = source.vomit;
		yogurtVomit = source.yogurtVomit;
	}

}