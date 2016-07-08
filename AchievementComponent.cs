using UnityEngine;
// using System.Collections;

public class AchievementComponent : MonoBehaviour {
	public Achievement achivement;
}

[System.Serializable]
public class Achievement {
	public Sprite icon;
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
}

[System.Serializable]
public class AchievementStats {
	public float secondsPlayed;
	public float yogurtEaten;
	public float vomit;
	public float yogurtVomit;
	public AchievementStats(){

	}

	// private void UpdateStats(){
	// 	if (GameManager.Instance.data.achievementStats == this){
	// 		GameManager.Instance.CheckAchievements();
	// 	}
	// }
}