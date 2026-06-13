using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleMenuUI : MonoBehaviour
{
    private bool _initialized;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        EnsureEventSystem();
        BindButtons();
        _initialized = true;
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void BindButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            string buttonName = button.gameObject.name;
            button.onClick.RemoveListener(StartGame);
            button.onClick.RemoveListener(ReturnToMenu);
            button.onClick.RemoveListener(RestartGame);
            button.onClick.RemoveListener(QuitGame);

            if (buttonName.Contains("Start"))
            {
                button.onClick.AddListener(StartGame);
            }
            else if (buttonName.Contains("Quit"))
            {
                button.onClick.AddListener(QuitGame);
            }
            else if (buttonName.Contains("Back"))
            {
                button.onClick.AddListener(ReturnToMenu);
            }
            else if (buttonName.Contains("Restart"))
            {
                button.onClick.AddListener(RestartGame);
            }
        }
    }

    public void StartGame()
    {
        Debug.Log("SimpleMenuUI: StartGame clicked");
        GameManager.Instance.StartGame();
    }

    public void ReturnToMenu()
    {
        Debug.Log("SimpleMenuUI: ReturnToMenu clicked");
        GameManager.Instance.ReturnToMenu();
    }

    public void RestartGame()
    {
        Debug.Log("SimpleMenuUI: RestartGame clicked");
        GameManager.Instance.RestartGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("SimpleMenuUI: QuitGame clicked in Editor");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
