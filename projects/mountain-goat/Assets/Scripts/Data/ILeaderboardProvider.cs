using System.Collections.Generic;

public interface ILeaderboardProvider
{
    void SubmitEntry(LeaderboardEntry entry);
    List<LeaderboardEntry> GetTopEntries(int count = 100);
    int GetRank(string profileId);
    LeaderboardEntry GetBestEntry(string profileId);
    void ClearAll();
    int EntryCount { get; }
}
