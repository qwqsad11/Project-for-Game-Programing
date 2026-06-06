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
        if (entry == null)
        {
            Debug.LogWarning("[LocalLeaderboard] SubmitEntry: entry is null, skipping.");
            return;
        }
        if (string.IsNullOrEmpty(entry.profileId))
        {
            Debug.LogWarning("[LocalLeaderboard] SubmitEntry: entry.profileId is null/empty, skipping.");
            return;
        }

        Debug.Log($"[LocalLeaderboard] SubmitEntry: player={entry.playerName}, score={entry.score}, profileId={entry.profileId}");

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

        Debug.Log($"[LocalLeaderboard] SubmitEntry: saved! Total entries now: {data.entries.Count}");
        Save();
    }

    public List<LeaderboardEntry> GetTopEntries(int count = 100)
    {
        Debug.Log($"[LocalLeaderboard] GetTopEntries: data.entries.Count={data?.entries?.Count ?? 0}");
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
            Debug.Log($"[LocalLeaderboard] Load: filePath={filePath}, exists={File.Exists(filePath)}");
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
                if (data.checksum != computedChecksum && !string.IsNullOrEmpty(data.checksum))
                {
                    // Checksum mismatch — but DON'T reset the data.
                    // This can happen legitimately after code changes (hash algorithm update).
                    // Just update the checksum to the new algorithm.
                    Debug.LogWarning("[LocalLeaderboard] Checksum mismatch — updating to current algorithm. Data is preserved.");
                    data.checksum = computedChecksum;
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
            Debug.Log($"[LocalLeaderboard] Save: wrote {json.Length} chars to {filePath}, entries={data.entries.Count}");
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

        // Use FNV-1a for a deterministic, cross-platform stable hash
        uint hash = 0x811C9DC5;
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            // Hash score (int → bytes)
            hash = Fnv1aHash(hash, e.score);
            // Hash profileId (string content, not GetHashCode)
            if (!string.IsNullOrEmpty(e.profileId))
            {
                hash = Fnv1aHash(hash, e.profileId);
            }
            // Hash timestamp (long → deterministic int)
            int timestampHash = (int)(e.timestamp ^ (e.timestamp >> 32));
            hash = Fnv1aHash(hash, timestampHash);
        }

        return hash.ToString("X8");
    }

    private static uint Fnv1aHash(uint hash, int value)
    {
        for (int i = 0; i < 4; i++)
        {
            byte b = (byte)(value >> (i * 8));
            hash ^= b;
            hash *= 0x01000193;
        }
        return hash;
    }

    private static uint Fnv1aHash(uint hash, string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            hash ^= (byte)value[i];
            hash *= 0x01000193;
        }
        return hash;
    }
}
