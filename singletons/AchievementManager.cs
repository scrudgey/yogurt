using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UnityEngine.Audio;
using Steamworks;
using System.Linq;

public class AchievementManager : Singleton<AchievementManager> {
    protected AchievementManager() { }
    // Our GameID
    private CGameID m_GameID;
    // Callback for Achievement stored
    protected Callback<UserAchievementStored_t> m_UserAchievementStored;
    void Start() {
        // DebugAchievements();
        // if (SteamManager.Initialized)
        //     SteamUserStats.ResetAllStats(true);
    }
    public static List<Achievement> LoadAchievements() {
        List<Achievement> achievements = new List<Achievement>();
        GameObject[] achievementPrefabs = Resources.LoadAll("achievements/", typeof(GameObject))
            .Cast<GameObject>()
            .ToArray();
        foreach (GameObject prefab in achievementPrefabs) {
            if (GameManager.Instance.debug && prefab.name == "StartGame")
                continue;
            AchievementComponent component = prefab.GetComponent<AchievementComponent>();
            if (component) {
                Achievement cloneAchievement = new Achievement(component.achivement);
                achievements.Add(cloneAchievement);
            }
        }
        return achievements;
    }
    public List<Achievement> CheckAchievements(GameData data) {
        List<Achievement> completeAchievements = new List<Achievement>();
        foreach (Achievement achievement in data.achievements) {
            if (!achievement.complete) {
                bool pass = achievement.Evaluate(data.stats);
                if (pass) {
                    UnlockAchievement(achievement);
                    completeAchievements.Add(achievement);
                }
            }
        }
        return completeAchievements;
    }

    private void DebugAchievements() {
        if (SteamManager.Initialized) {
            Debug.Log("SteamManager initialized.");
            // Cache the GameID for use in the Callbacks
            m_GameID = new CGameID(SteamUtils.GetAppID());
            m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
            // PrintAchievements();
        } else {
            Debug.LogWarning("SteamManager NOT initialized!");
        }
    }

    private void PrintAchievements() {
        foreach (Achievement achievement in LoadAchievements()) {
            bool ret = SteamUserStats.GetAchievement(achievement.steamId, out achievement.steamAchieved);
            if (ret) {
                string Name = SteamUserStats.GetAchievementDisplayAttribute(achievement.steamId, "name");
                string Description = SteamUserStats.GetAchievementDisplayAttribute(achievement.steamId, "desc");
                Debug.LogFormat("Achievement Name: {0} --- Unlocked: {1}", achievement.title, achievement.steamAchieved);
            } else {
                Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + achievement.steamId + "\nIs it registered in the Steam Partner site?");
            }
        }
    }

    /// 

    /// Calls SetAchiemement. Steam docs: 
    /// This method sets a given achievement to achieved and sends the results to Steam. You can set a given achievement multiple times so you don't
    /// need to worry about only setting achievements that aren't already set.
    /// This is an asynchronous call which will trigger two callbacks: OnUserStatsStored() and OnAchievementStored()
    /// 
    /// 
    public void UnlockAchievement(Achievement achievement) {
        Debug.Log($"unlocking {achievement.title}");
        achievement.complete = true;
        achievement.completedTime = System.DateTime.Now;
        if (SteamManager.Initialized) {
            SteamUserStats.SetAchievement(achievement.steamId);
            SteamUserStats.StoreStats();
        }

    }

    //-----------------------------------------------------------------------------
    // Purpose: An achievement was stored
    //-----------------------------------------------------------------------------
    public void OnAchievementStored(UserAchievementStored_t pCallback) {
        // We may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID) {
            if (0 == pCallback.m_nMaxProgress) {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
                // SteamManager.StoreStats();
            } else {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
            }
        }
    }
}