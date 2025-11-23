using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LeaderboardEntry
{
    public string name;
    public float score;
    public string date;
}

[Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class LeaderboardManager : GenericSingleton<LeaderboardManager>
{
    private const string PrefKey = "MinimaxLeaderboard";
    [SerializeField]
    private int maxEntries = 10;

    private LeaderboardData data = new LeaderboardData();

    internal override void Init()
    {
    // 씬 전환 시에도 유지
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(PrefKey))
        {
            try
            {
                string json = PlayerPrefs.GetString(PrefKey);
                data = JsonUtility.FromJson<LeaderboardData>(json) ?? new LeaderboardData();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to load leaderboard: " + ex.Message);
                data = new LeaderboardData();
            }
        }
        else
        {
            data = new LeaderboardData();
        }
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PrefKey, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to save leaderboard: " + ex.Message);
        }
    }

    public int AddEntry(string name, float score)
    {
        if (string.IsNullOrEmpty(name)) name = "Player";

        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        var entry = new LeaderboardEntry
        {
            name = name,
            score = score,
            date = date
        };

        data.entries.Add(entry);

    // 점수 내림차순 정렬
        data.entries.Sort((a, b) => b.score.CompareTo(a.score));

    // 최대 항목수만 남기고 잘라냄
        if (data.entries.Count > maxEntries)
        {
            data.entries.RemoveRange(maxEntries, data.entries.Count - maxEntries);
        }

        Save();

    // 방금 추가한 항목의 인덱스 찾음
        int idx = data.entries.FindIndex(e => e.name == name && Mathf.Approximately(e.score, score) && e.date == date);
        if (idx < 0) idx = 0;
        return idx;
    }

    public List<LeaderboardEntry> GetEntries()
    {
        return new List<LeaderboardEntry>(data.entries);
    }

    public void Clear()
    {
        data.entries.Clear();
        Save();
    }
}
