using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    private const int MaxProfiles = 3;
    private const string ActiveProfileIndexKey = "ActiveProfileIndex";
    private const string MigrationDoneKey = "ProfileMigrated";

    private static ProfileManager _instance;
    public static ProfileManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ProfileManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ProfileManager");
                    _instance = go.AddComponent<ProfileManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private List<PlayerProfile> profiles = new List<PlayerProfile>();
    private int activeProfileIndex = -1;
    private string filePath;

    // Events
    public event Action OnActiveProfileChanged;
    public event Action OnProfileUpdated;
    public event Action<PlayerProfile> OnProfileCreated;
    public event Action<PlayerProfile> OnProfileDeleted;

    public int ProfileCount => profiles.Count;
    public bool HasActiveProfile => activeProfileIndex >= 0 && activeProfileIndex < profiles.Count;
    public bool NeedsProfileCreation => profiles.Count == 0;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        filePath = Path.Combine(Application.persistentDataPath, "profiles.json");
        LoadProfiles();
    }

    public void LoadProfiles()
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                ProfileListWrapper wrapper = JsonUtility.FromJson<ProfileListWrapper>(json);
                profiles = wrapper?.profiles ?? new List<PlayerProfile>();

                // Remove null entries
                profiles.RemoveAll(p => p == null);

                if (profiles.Count > MaxProfiles)
                {
                    profiles = profiles.GetRange(0, MaxProfiles);
                }
            }
            else
            {
                profiles = new List<PlayerProfile>();
                TryMigrateFromPlayerPrefs();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ProfileManager] Failed to load profiles: {ex.Message}");
            profiles = new List<PlayerProfile>();
        }

        // Load active profile index
        activeProfileIndex = PlayerPrefs.GetInt(ActiveProfileIndexKey, -1);
        if (activeProfileIndex < 0 || activeProfileIndex >= profiles.Count)
        {
            activeProfileIndex = profiles.Count > 0 ? 0 : -1;
        }

        if (HasActiveProfile)
        {
            OnActiveProfileChanged?.Invoke();
        }
    }

    public void SaveProfiles()
    {
        try
        {
            ProfileListWrapper wrapper = new ProfileListWrapper { profiles = profiles };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ProfileManager] Failed to save profiles: {ex.Message}");
        }
    }

    public PlayerProfile CreateProfile(string name)
    {
        if (profiles.Count >= MaxProfiles)
        {
            Debug.LogWarning("[ProfileManager] Maximum profile limit reached.");
            return null;
        }

        string trimmedName = name?.Trim() ?? "";
        if (trimmedName.Length < 3 || trimmedName.Length > 16)
        {
            Debug.LogWarning("[ProfileManager] Profile name must be between 3 and 16 characters.");
            return null;
        }

        // Check for duplicate names
        for (int i = 0; i < profiles.Count; i++)
        {
            if (profiles[i].profileName.Equals(trimmedName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning("[ProfileManager] A profile with this name already exists.");
                return null;
            }
        }

        PlayerProfile profile = PlayerProfile.CreateNew(trimmedName);
        profiles.Add(profile);

        if (activeProfileIndex < 0)
        {
            activeProfileIndex = 0;
        }

        SaveProfiles();
        PlayerPrefs.SetInt(ActiveProfileIndexKey, activeProfileIndex);
        PlayerPrefs.Save();

        OnProfileCreated?.Invoke(profile);
        OnActiveProfileChanged?.Invoke();

        return profile;
    }

    public bool DeleteProfile(string profileId)
    {
        if (profiles.Count <= 1)
        {
            Debug.LogWarning("[ProfileManager] Cannot delete the last profile.");
            return false;
        }

        int index = FindProfileIndex(profileId);
        if (index < 0)
            return false;

        PlayerProfile deleted = profiles[index];
        profiles.RemoveAt(index);

        if (activeProfileIndex >= profiles.Count)
        {
            activeProfileIndex = profiles.Count - 1;
        }

        SaveProfiles();
        PlayerPrefs.SetInt(ActiveProfileIndexKey, activeProfileIndex);
        PlayerPrefs.Save();

        OnProfileDeleted?.Invoke(deleted);
        OnActiveProfileChanged?.Invoke();

        return true;
    }

    public PlayerProfile GetActiveProfile()
    {
        if (!HasActiveProfile)
            return null;

        return profiles[activeProfileIndex];
    }

    public bool SetActiveProfile(string profileId)
    {
        int index = FindProfileIndex(profileId);
        if (index < 0)
            return false;

        if (activeProfileIndex == index)
            return true;

        activeProfileIndex = index;
        PlayerPrefs.SetInt(ActiveProfileIndexKey, activeProfileIndex);
        PlayerPrefs.Save();

        OnActiveProfileChanged?.Invoke();
        return true;
    }

    public List<PlayerProfile> GetAllProfiles()
    {
        return new List<PlayerProfile>(profiles);
    }

    public List<PlayerProfile> GetAllProfilesWithEmptySlots()
    {
        List<PlayerProfile> result = new List<PlayerProfile>(profiles);
        // Pad with null entries to represent empty slots
        while (result.Count < MaxProfiles)
        {
            result.Add(null);
        }
        return result;
    }

    public int MaxSlotCount => MaxProfiles;

    public void UpdateHighScore(int score)
    {
        PlayerProfile active = GetActiveProfile();
        if (active == null)
            return;

        if (score > active.highScore)
        {
            active.highScore = score;
            SaveProfiles();
            OnProfileUpdated?.Invoke();
        }
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        PlayerProfile active = GetActiveProfile();
        if (active == null)
            return;

        active.totalCoins += amount;
        SaveProfiles();
        OnProfileUpdated?.Invoke();
    }

    public void SaveCharacterSelection(int characterValue)
    {
        PlayerProfile active = GetActiveProfile();
        if (active == null)
            return;

        active.selectedCharacter = characterValue;
        SaveProfiles();
        OnProfileUpdated?.Invoke();
    }

    public int GetCharacterSelection()
    {
        PlayerProfile active = GetActiveProfile();
        if (active == null)
            return 0;

        return active.selectedCharacter;
    }

    public void IncrementPlayCount()
    {
        PlayerProfile active = GetActiveProfile();
        if (active == null)
            return;

        active.totalPlays++;
        active.lastPlayedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        SaveProfiles();
        OnProfileUpdated?.Invoke();
    }

    private int FindProfileIndex(string profileId)
    {
        for (int i = 0; i < profiles.Count; i++)
        {
            if (profiles[i].profileId == profileId)
                return i;
        }
        return -1;
    }

    private void TryMigrateFromPlayerPrefs()
    {
        if (PlayerPrefs.GetInt(MigrationDoneKey, 0) == 1)
            return;

        if (!PlayerPrefs.HasKey("HighScore") && !PlayerPrefs.HasKey("TotalCoins"))
            return;

        Debug.Log("[ProfileManager] Migrating data from PlayerPrefs...");

        PlayerProfile migrated = PlayerProfile.CreateNew("Player");
        migrated.highScore = PlayerPrefs.GetInt("HighScore", 0);
        migrated.totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (PlayerPrefs.HasKey("SelectedPlayerCharacter"))
        {
            migrated.selectedCharacter = PlayerPrefs.GetInt("SelectedPlayerCharacter", 0);
        }

        profiles.Add(migrated);
        activeProfileIndex = 0;

        SaveProfiles();
        PlayerPrefs.SetInt(ActiveProfileIndexKey, 0);
        PlayerPrefs.SetInt(MigrationDoneKey, 1);
        PlayerPrefs.Save();

        Debug.Log("[ProfileManager] Migration complete. Created profile 'Player' with existing data.");
    }

    [System.Serializable]
    private class ProfileListWrapper
    {
        public List<PlayerProfile> profiles = new List<PlayerProfile>();
    }
}
