using UnityEngine;
using UnityEngine.UI;

public class SolarSystemUIManager : MonoBehaviour
{
    public Text infoText;
    public Button resetButton;
    public string defaultText = "点击一个行星或卫星，发现它的秘密！";

    private void Start()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }

        ShowDefaultMessage(defaultText);
    }

    public void ShowBodyInfo(CelestialBody body)
    {
        if (infoText == null || body == null)
        {
            return;
        }

        infoText.text = $"{body.bodyName}\n{body.description}";
    }

    public void ShowDefaultMessage(string message)
    {
        if (infoText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(message))
        {
            message = defaultText;
        }

        infoText.text = message;
    }

    private void OnResetButtonClicked()
    {
        SolarSystemInteractor interactor = FindObjectOfType<SolarSystemInteractor>();
        if (interactor != null)
        {
            interactor.ResetView();
        }
    }
}
