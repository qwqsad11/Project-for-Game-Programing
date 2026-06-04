using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class LocalLeaderboardProvider : ILeaderboardProvider
{
    private const int MaxEntries = 100;
    private readonly string filePath;
    private LeaderboardData data;

    public int EntryCount => data?.entries?.Count ?? 0;

    public LocalLeaderboardProvider()
    {
        filePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");
        Load();
    }

    public void SubmitEntry(LeaderboardEntry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.profileId))
            return;

        // Remove existing entry for this profile (keep only highest score)
        for (int i = data.entries.Count - 1; i >= 0; i--)
        {
            if (data.entries[i].profileId == entry.profileId)
            {
                data.entries.RemoveAt(i);
            }
        }

        data.entries.Add(entry);

        // Sort descending by score
        data.entries.Sort((a, b) => b.score.CompareTo(a.score));

        // Trim to top N
        if (data.entries.Count > MaxEntries)
        {
            data.entries.RemoveRange(MaxEntries, data.entries.Count - MaxEntries);
        }

        Save();
    }

    public List<LeaderboardEntry> GetTopEntries(int count = 100)
    {
        if (data.entries.Count <= count)
            return new List<LeaderboardEntry>(data.entries);

        return data.entries.GetRange(0, count);
    }

    public int GetRank(string profileId)
    {
        if (string.IsNullOrEmpty(profileId))
            return 0;

        for (int i = 0; i < data.entries.Count; i++)
        {
            if (data.entries[i].profileId == profileId)
                return i + 1; // 1-based rank
        }

        return 0;
    }

    public LeaderboardEntry GetBestEntry(string profileId)
    {
        if (string.IsNullOrEmpty(profileId))
            return null;

        for (int i = 0; i < data.entries.Count; i++)
        {
            if (data.entries[i].profileId == profileId)
                return data.entries[i];
        }

        return null;
    }

    public void ClearAll()
    {
        data = new LeaderboardData();
        Save();
    }

    private void Load()
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                data = JsonUtility.FromJson<LeaderboardData>(json);

                // Validate
                if (data == null)
                {
                    data = new LeaderboardData();
                    return;
                }

                if (data.entries == null)
                {
                    data.entries = new List<LeaderboardEntry>();
                }

                string computedChecksum = ComputeChecksum(data.entries);
                if (data.checksum != computedChecksum)
                {
                    Debug.LogWarning("[LocalLeaderboard] Checksum mismatch — data may have been tampered with or corrupted. Resetting leaderboard.");
                    data = new LeaderboardData();
                    Save();
                }
            }
            else
            {
                data = new LeaderboardData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LocalLeaderboard] Failed to load leaderboard: {ex.Message}. Starting fresh.");
            data = new LeaderboardData();
        }
    }

    private void Save()
    {
        try
        {
            data.checksum = ComputeChecksum(data.entries);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LocalLeaderboard] Failed to save leaderboard: {ex.Message}");
        }
    }

    private static string ComputeChecksum(List<LeaderboardEntry> entries)
    {
        if (entries == null || entries.Count == 0)
            return "empty";

        // Simple XOR-based hash of scores and profile IDs
        int hash = 0x5A3C1F7E;
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            hash ^= e.score * 31 + (e.profileId?.GetHashCode() ?? 0);
            hash ^= e.timestamp.GetHashCode();
            hash = (hash << 7) | (int)((uint)hash >> 25);
        }

        return hash.ToString("X8");
    }
}
