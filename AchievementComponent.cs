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
		if (stats.dollarsBurned > 0)
			pass = pass && otherStats.dollarsBurned >= stats.dollarsBurned;
		if (stats.dollarsDuplicated > 0)
			pass = pass && otherStats.dollarsDuplicated >= stats.dollarsDuplicated;
		if (stats.dollarsFlushed > 0)
			pass = pass && otherStats.dollarsFlushed >= stats.dollarsFlushed;
		if (stats.immolations > 0)
			pass = pass && otherStats.immolations >= stats.immolations;
		if (stats.selfImmolations > 0)
			pass = pass && otherStats.selfImmolations >= stats.selfImmolations;

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
	public float dollarsFlushed;
	public float dollarsDuplicated;
	public float dollarsBurned;
	public int immolations;
	public int selfImmolations;
	public AchievementStats(){ }
	public AchievementStats(AchievementStats source){
		secondsPlayed = source.secondsPlayed;
		yogurtEaten = source.yogurtEaten;
		vomit = source.vomit;
		yogurtVomit = source.yogurtVomit;
		dollarsBurned = source.dollarsBurned;
		dollarsDuplicated = source.dollarsDuplicated;
		dollarsFlushed = source.dollarsFlushed;
		immolations = source.immolations;
		selfImmolations = source.selfImmolations;
	}

}