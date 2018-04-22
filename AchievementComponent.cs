using UnityEngine;

public class AchievementComponent : MonoBehaviour {
    public Achievement achivement;
}

[System.Serializable]
public class Achievement {
    // public Sprite icon;
    public bool complete;
    public string title;
    public string description;
    public AchievementStats stats = new AchievementStats();
    public bool Evaluate(AchievementStats otherStats) {
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
        if (stats.swordsEaten > 0)
            pass = pass && otherStats.swordsEaten >= stats.swordsEaten;
        if (stats.hatsEaten > 0)
            pass = pass && otherStats.hatsEaten >= stats.hatsEaten;
        if (stats.deathByCombat > 0)
            pass = pass && otherStats.deathByCombat >= stats.deathByCombat;
        if (stats.deathByMisadventure > 0)
            pass = pass && otherStats.deathByMisadventure >= stats.deathByMisadventure;
        if (stats.deathByAsphyxiation > 0)
            pass = pass && otherStats.deathByAsphyxiation >= stats.deathByAsphyxiation;
        return pass;
    }
    public Achievement() { }
    public Achievement(Achievement source) {
        // icon = source.icon;
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
    public int swordsEaten;
    public int hatsEaten;
    public int immolations;
    public int selfImmolations;
    public int deathByCombat;
    public int deathByMisadventure;
    public int deathByAsphyxiation;
    public AchievementStats() { }
    public AchievementStats(AchievementStats source) {
        secondsPlayed = source.secondsPlayed;
        yogurtEaten = source.yogurtEaten;
        vomit = source.vomit;
        yogurtVomit = source.yogurtVomit;
        dollarsBurned = source.dollarsBurned;
        dollarsDuplicated = source.dollarsDuplicated;
        dollarsFlushed = source.dollarsFlushed;
        immolations = source.immolations;
        selfImmolations = source.selfImmolations;
        swordsEaten = source.swordsEaten;
        hatsEaten = source.hatsEaten;
        deathByCombat = source.deathByCombat;
        deathByMisadventure = source.deathByMisadventure;
        deathByAsphyxiation = source.deathByAsphyxiation;
    }
}