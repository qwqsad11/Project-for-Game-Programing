using UnityEngine;

public class PlayerCharacterSelection : MonoBehaviour
{
    public const string PlayerPrefsKey = "SelectedPlayerCharacter";

    public enum Character
    {
        Goat = 0,
        GoatDark = 1,
        SheepWhite = 2,
        SheepCream = 3,
        SheepDark = 4,
        Fawn = 5,
        Deer = 6,
        DeerFemale = 7,
        Elk = 8,
        ElkAlbine = 9
    }

    [Header("Selection")]
    [SerializeField] private Character defaultCharacter = Character.Goat;

    [Header("Goat Visual")]
    [SerializeField] private Animator goatAnimator;
    [SerializeField] private Transform[] goatVisualRoots;

    [System.Serializable]
    private class CharacterVisual
    {
        [SerializeField] private Character character;
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector3 localPosition = Vector3.zero;
        [SerializeField] private Vector3 localEulerAngles = new Vector3(0f, 180f, 0f);
        [SerializeField] private Vector3 localScale = new Vector3(0.85f, 0.85f, 0.85f);

        public Character Character => character;
        public GameObject Prefab => prefab;
        public Vector3 LocalPosition => localPosition;
        public Vector3 LocalEulerAngles => localEulerAngles;
        public Vector3 LocalScale => localScale;
    }

    [Header("Animal Visuals")]
    [SerializeField] private CharacterVisual[] animalVisuals;

    private static readonly string[] IdleStateNames =
    {
        "idle",
        "DIdle 1",
        "DIdle Look",
        "DIdle Scratch",
        "DIdle Head Shake"
    };

    private GameObject activeAnimalInstance;

    private void Awake()
    {
        ApplySavedSelection();
    }

    public void ApplySavedSelection()
    {
        ApplySelection(GetSavedCharacter(defaultCharacter));
    }

    public void ApplySelection(Character character)
    {
        CharacterVisual visual = FindVisual(character);
        if (visual != null && visual.Prefab != null)
        {
            UseAnimal(visual);
            return;
        }

        if (character == Character.Goat)
        {
            UseGoat();
            return;
        }

        UseGoat();
    }

    public static Character GetSavedCharacter(Character fallback = Character.Goat)
    {
        if (ProfileManager.Instance != null && ProfileManager.Instance.HasActiveProfile)
        {
            int savedValue = ProfileManager.Instance.GetCharacterSelection();
            return System.Enum.IsDefined(typeof(Character), savedValue) ? (Character)savedValue : fallback;
        }

        // Fallback to PlayerPrefs during migration/transition
        int ppValue = PlayerPrefs.GetInt(PlayerPrefsKey, (int)fallback);
        return System.Enum.IsDefined(typeof(Character), ppValue) ? (Character)ppValue : fallback;
    }

    public static void SaveSelection(Character character)
    {
        if (ProfileManager.Instance != null && ProfileManager.Instance.HasActiveProfile)
        {
            ProfileManager.Instance.SaveCharacterSelection((int)character);
        }

        // Also save to PlayerPrefs for backward compatibility during transition
        PlayerPrefs.SetInt(PlayerPrefsKey, (int)character);
        PlayerPrefs.Save();
    }

    private void UseGoat()
    {
        SetGoatVisualsActive(true);

        if (activeAnimalInstance != null)
        {
            activeAnimalInstance.SetActive(false);
        }

        Animator anim = goatAnimator != null ? goatAnimator : GetComponent<Animator>();

        GoatMovement movement = GetComponent<GoatMovement>();
        if (movement != null)
        {
            movement.enabled = true;
            movement.SetAnimator(anim);
        }

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }

    private void UseAnimal(CharacterVisual visual)
    {
        SetGoatVisualsActive(false);

        if (activeAnimalInstance != null)
        {
            Destroy(activeAnimalInstance);
        }

        activeAnimalInstance = Instantiate(visual.Prefab, transform);
        activeAnimalInstance.name = visual.Prefab.name + " Visual";

        Transform animalTransform = activeAnimalInstance.transform;
        animalTransform.localPosition = visual.LocalPosition;
        animalTransform.localRotation = Quaternion.Euler(visual.LocalEulerAngles);
        animalTransform.localScale = visual.LocalScale;
        activeAnimalInstance.SetActive(true);

        Animator animalAnimator = activeAnimalInstance.GetComponentInChildren<Animator>();
        if (animalAnimator != null)
        {
            animalAnimator.applyRootMotion = false;
            PlayIdle(animalAnimator);
        }

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            // Dedicated deer movement system available — use it
            GoatMovement goatMovement = GetComponent<GoatMovement>();
            if (goatMovement != null)
            {
                goatMovement.enabled = false;
            }

            playerMovement.enabled = true;
            playerMovement.SetAnimator(animalAnimator);
        }
        else
        {
            // Fallback: no PlayerMovement on this GameObject, route animal animator through GoatMovement
            GoatMovement goatMovement = GetComponent<GoatMovement>();
            if (goatMovement != null)
            {
                goatMovement.enabled = true;
                goatMovement.SetAnimator(animalAnimator);
            }
        }
    }

    private CharacterVisual FindVisual(Character character)
    {
        for (int i = 0; i < animalVisuals.Length; i++)
        {
            if (animalVisuals[i] != null && animalVisuals[i].Character == character)
            {
                return animalVisuals[i];
            }
        }

        return null;
    }

    private static void PlayIdle(Animator animator)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        for (int i = 0; i < IdleStateNames.Length; i++)
        {
            int stateHash = Animator.StringToHash(IdleStateNames[i]);
            if (animator.HasState(0, stateHash))
            {
                animator.Play(stateHash, 0, 0f);
                animator.Update(0f);
                return;
            }
        }
    }

    private void SetGoatVisualsActive(bool active)
    {
        for (int i = 0; i < goatVisualRoots.Length; i++)
        {
            if (goatVisualRoots[i] != null)
            {
                goatVisualRoots[i].gameObject.SetActive(active);
            }
        }
    }
}
