using UnityEngine;

public class SolarSystemInteractor : MonoBehaviour
{
    public Camera mainCamera;
    public CameraFocusController cameraController;
    public SolarSystemUIManager uiManager;
    public LayerMask clickableLayers = ~0;
    public string defaultMessage = "欢迎来到太阳系！点击行星或卫星了解它们。右键可返回主视角。";

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (uiManager != null)
        {
            uiManager.ShowDefaultMessage(defaultMessage);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (mainCamera == null)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayers))
            {
                CelestialBody body = hit.collider.GetComponentInParent<CelestialBody>();
                if (body != null)
                {
                    body.OnSelected();
                    cameraController?.FocusOn(body.transform);
                    uiManager?.ShowBodyInfo(body);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            ResetView();
        }
    }

    public void ResetView()
    {
        cameraController?.ResetView();
        uiManager?.ShowDefaultMessage(defaultMessage);
    }
}
