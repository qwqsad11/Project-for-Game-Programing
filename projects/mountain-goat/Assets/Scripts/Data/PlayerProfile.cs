using System;

[System.Serializable]
public class PlayerProfile
{
    public string profileId;
    public string profileName;
    public int highScore;
    public int totalCoins;
    public int selectedCharacter;
    public int totalPlays;
    public long lastPlayedTimestamp;
    public long createdTimestamp;

    public static PlayerProfile CreateNew(string name)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return new PlayerProfile
        {
            profileId = Guid.NewGuid().ToString("N"),
            profileName = name,
            highScore = 0,
            totalCoins = 0,
            selectedCharacter = 0,
            totalPlays = 0,
            lastPlayedTimestamp = now,
            createdTimestamp = now
        };
    }

    public string GetLastPlayedText()
    {
        if (lastPlayedTimestamp <= 0)
            return "Never";

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long diff = now - lastPlayedTimestamp;

        if (diff < 60) return "Just now";
        if (diff < 3600) return $"{diff / 60}m ago";
        if (diff < 86400) return $"{diff / 3600}h ago";
        return $"{diff / 86400}d ago";
    }
}
