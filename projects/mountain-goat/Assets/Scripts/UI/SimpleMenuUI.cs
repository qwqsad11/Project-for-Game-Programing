using UnityEngine;

public class SimpleMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance.StartGame();
    }

    public void ReturnToMenu()
    {
        GameManager.Instance.ReturnToMenu();
    }

    public void RestartGame()
    {
        GameManager.Instance.RestartGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
