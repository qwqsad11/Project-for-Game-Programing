using UnityEngine;

public class PlayerCharacterSelection : MonoBehaviour
{
    public const string PlayerPrefsKey = "SelectedPlayerCharacter";

    public enum Character
    {
        Goat = 0,
        Deer = 1,
        DeerFemale = 2,
        Elk = 3,
        ElkAlbine = 4,
        Fawn = 5
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
        if (character == Character.Goat)
        {
            UseGoat();
            return;
        }

        CharacterVisual visual = FindVisual(character);
        if (visual != null && visual.Prefab != null)
        {
            UseAnimal(visual);
            return;
        }

        UseGoat();
    }

    public static Character GetSavedCharacter(Character fallback = Character.Goat)
    {
        int savedValue = PlayerPrefs.GetInt(PlayerPrefsKey, (int)fallback);
        return System.Enum.IsDefined(typeof(Character), savedValue) ? (Character)savedValue : fallback;
    }

    public static void SaveSelection(Character character)
    {
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

        GoatMovement movement = GetComponent<GoatMovement>();
        if (movement != null)
        {
            movement.SetAnimator(goatAnimator != null ? goatAnimator : GetComponent<Animator>());
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
        }

        GoatMovement movement = GetComponent<GoatMovement>();
        if (movement != null)
        {
            movement.SetAnimator(animalAnimator);
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
