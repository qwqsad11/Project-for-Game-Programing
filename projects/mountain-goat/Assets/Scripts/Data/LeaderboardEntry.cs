using System;

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public int coins;
    public int characterId;
    public long timestamp;
    public string profileId;

    public static LeaderboardEntry Create(PlayerProfile profile, int score, int sessionCoins)
    {
        return new LeaderboardEntry
        {
            playerName = profile.profileName,
            score = score,
            coins = sessionCoins,
            characterId = profile.selectedCharacter,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            profileId = profile.profileId
        };
    }

    public string GetTimeAgoText()
    {
        if (timestamp <= 0)
            return "";

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long diff = now - timestamp;

        if (diff < 60) return "Just now";
        if (diff < 3600) return $"{diff / 60}m ago";
        if (diff < 86400) return $"{diff / 3600}h ago";
        return $"{diff / 86400}d ago";
    }
}
