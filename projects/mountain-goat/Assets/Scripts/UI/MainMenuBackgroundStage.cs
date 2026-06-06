using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Self-contained dynamic background for the MainMenu scene.
///
/// A Deer jumps upward in place while a mountain made of SafePlatform tiles
/// and Rock models shifts downward by one step after each jump — creating
/// an infinite-climb illusion in an isometric fixed-camera view.
///
/// One jump = one step of mountain scroll. The deer's jump speed directly
/// controls the mountain scroll pacing.
///
/// Does NOT reference any gameplay systems (GameManager, GridManager, Tile,
/// GoatController, etc.) — fully independent.
/// </summary>
[DisallowMultipleComponent]
public class MainMenuBackgroundStage : MonoBehaviour
{
    // ── Deer ────────────────────────────────────────
    [Header("Deer")]
    [SerializeField] private GameObject deerPrefab;
    [SerializeField] private Vector3 deerScale = new Vector3(0.9f, 0.9f, 0.9f);
    [SerializeField] private float deerYOffset = 0.25f;
    [Tooltip("Additional Y rotation (degrees) so the deer visually faces the right direction.")]
    [SerializeField] private float deerFacingAngle = 0f;

    // ── Deer Jump Timing ────────────────────────────
    [Header("Deer Jump")]
    [Tooltip("Total cycle time between jump starts (seconds). Recommended 0.8–1.3.")]
    [SerializeField] private float jumpInterval = 0.95f;
    [Tooltip("Duration of the jump arc (seconds). Match Deer Jump animation (~0.8s).")]
    [SerializeField] private float jumpDuration = 0.80f;
    [Tooltip("Height of the deer's vertical jump arc.")]
    [SerializeField] private float jumpHeight = 2.0f;

    // ── Camera ──────────────────────────────────────
    [Header("Camera")]
    [SerializeField] private Vector3 cameraPosition = new Vector3(0f, 2f, -12f);
    [SerializeField] private Vector3 cameraRotation = new Vector3(15f, 0f, 0f);
    [SerializeField] private float cameraOrthoSize = 7.5f;
    [SerializeField] private Color backgroundColor = new Color(0.15f, 0.18f, 0.24f);

    // ── Mountain ────────────────────────────────────
    [Header("Mountain")]
    [Tooltip("Number of mountain rows in the staircase.")]
    [Range(10, 40)]
    [SerializeField] private int mountainRowCount = 24;
    [Tooltip("Columns per row (1 = single path).")]
    [Range(1, 5)]
    [SerializeField] private int mountainWidth = 1;
    [Tooltip("World-space offset between consecutive steps. E.g. (1,0,1) = flat diagonal path.")]
    [SerializeField] private Vector3 stepOffset = new Vector3(1.5f, 0.5f, 0f);
    [Tooltip("Chance (0–1) to place a rock on a non-center mountain cell.")]
    [Range(0f, 1f)]
    [SerializeField] private float rockChance = 0.6f;

    // ── Platform / Rock Prefabs ─────────────────────
    [Header("Prefabs")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject rockPrefab;

    // ── Lighting ────────────────────────────────────
    [Header("Lighting")]
    [SerializeField] private Color lightColor = new Color(1f, 0.94f, 0.84f);
    [SerializeField] private float lightIntensity = 1.1f;
    [SerializeField] private Vector3 lightRotation = new Vector3(50f, -30f, 0f);

    // ── Internal State ──────────────────────────────
    private Animator deerAnimator;
    private Transform deerTransform;
    private Transform mountainRoot;
    private MountainRow[] mountainRows;
    private Vector3 deerBaseScale;
    private Vector3 stepWorldOffset;
    private int centerCol;

    private class MountainRow
    {
        public Transform rowRoot;
        public int logicalRowIndex;
    }

    // ── Lifetime ────────────────────────────────────

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            enabled = false;
            return;
        }

        deerBaseScale = deerScale;
        centerCol = mountainWidth / 2;
        stepWorldOffset = stepOffset;

        ResolvePrefabs();
        SetupCamera();
        SetupLighting();
        CreateDeer();
        CreateMountain();
        StartCoroutine(DeerJumpLoop());
    }

    private void OnDestroy()
    {
        // Runtime materials cleaned up with objects
    }

    // ── Setup ───────────────────────────────────────

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        cam.orthographic = true;
        cam.orthographicSize = cameraOrthoSize;
        cam.transform.position = cameraPosition;
        cam.transform.rotation = Quaternion.Euler(cameraRotation);
        cam.backgroundColor = backgroundColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    private void SetupLighting()
    {
        Light light = FindObjectOfType<Light>();
        if (light == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
        }

        light.color = lightColor;
        light.intensity = lightIntensity;
        light.transform.rotation = Quaternion.Euler(lightRotation);
    }

    private void ResolvePrefabs()
    {
#if UNITY_EDITOR
        if (deerPrefab == null)
        {
            deerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Malbers Animations/Animals Packs/01 Forest Pack/Deer/Models/Deer.prefab");
        }

        if (platformPrefab == null)
        {
            platformPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/SafePlatform.prefab");
        }

        if (rockPrefab == null)
        {
            rockPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Stylized Rock - Desert style free/Prefabs/Rock_desert.prefab");
        }
#endif

        if (deerPrefab == null)
            Debug.LogWarning("MainMenuBackgroundStage: Deer prefab not found.");
        if (platformPrefab == null)
            Debug.LogWarning("MainMenuBackgroundStage: Platform prefab not found.");
    }

    // ── Deer ────────────────────────────────────────

    private void CreateDeer()
    {
        if (deerPrefab != null)
        {
            GameObject deerObj = Instantiate(deerPrefab, transform);
            deerObj.name = "BackgroundDeer";
            deerTransform = deerObj.transform;
            deerTransform.localScale = deerScale;

            deerObj.tag = "Untagged";

            deerAnimator = deerObj.GetComponent<Animator>();
            if (deerAnimator != null)
            {
                deerAnimator.applyRootMotion = false;
            }

            StripGameplayComponents(deerObj);
        }
        else
        {
            CreateFallbackDeer();
        }

        deerTransform.position = new Vector3(0f, deerYOffset, 0f);
        FaceStepDirection();
    }

    private void CreateFallbackDeer()
    {
        GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        fallback.name = "FallbackDeer";
        fallback.transform.SetParent(transform);
        deerTransform = fallback.transform;
        deerTransform.localScale = deerScale;

        Renderer renderer = fallback.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = new Color(0.5f, 0.35f, 0.2f);
            renderer.material = mat;
        }
    }

    private static void StripGameplayComponents(GameObject obj)
    {
        System.Type[] typesToRemove = new System.Type[]
        {
            System.Type.GetType("GoatController"),
            System.Type.GetType("GoatMovement"),
            System.Type.GetType("PlayerController"),
            System.Type.GetType("JumpController"),
        };

        foreach (System.Type type in typesToRemove)
        {
            if (type == null) continue;
            Component comp = obj.GetComponent(type);
            if (comp != null) Destroy(comp);
        }
    }

    private void FaceStepDirection()
    {
        Vector3 dir = stepWorldOffset;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion baseRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        Quaternion facingOffset = Quaternion.Euler(0f, deerFacingAngle, 0f);
        deerTransform.rotation = baseRotation * facingOffset;
    }

    // ── Mountain ────────────────────────────────────

    private void CreateMountain()
    {
        GameObject rootObj = new GameObject("MountainRoot");
        mountainRoot = rootObj.transform;
        mountainRoot.SetParent(transform);

        mountainRows = new MountainRow[mountainRowCount];

        for (int i = 0; i < mountainRowCount; i++)
        {
            mountainRows[i] = CreateMountainRow(i);
        }

        // Center the mountain so the deer is in the middle
        int midRow = mountainRowCount / 2;
        mountainRoot.position = -(midRow * stepWorldOffset);
        mountainRoot.position += new Vector3(0f, -deerYOffset, 0f);
    }

    private MountainRow CreateMountainRow(int logicalIndex)
    {
        GameObject rowRoot = new GameObject($"MountainRow_{logicalIndex:D3}");
        rowRoot.transform.SetParent(mountainRoot);
        rowRoot.transform.localPosition = logicalIndex * stepWorldOffset;
        rowRoot.transform.localRotation = Quaternion.identity;

        for (int col = 0; col < mountainWidth; col++)
        {
            Vector3 cellLocalPos = GetCellOffset(col);
            bool isCenter = (col == centerCol);

            if (platformPrefab != null)
            {
                GameObject plat = Instantiate(platformPrefab, rowRoot.transform);
                plat.name = $"Platform_{logicalIndex}_{col}";
                plat.transform.localPosition = cellLocalPos;
                plat.transform.localRotation = Quaternion.identity;
                DisableGameplayComponents(plat);
            }

            if (!isCenter && rockPrefab != null && Random.value < rockChance)
            {
                GameObject rock = Instantiate(rockPrefab, rowRoot.transform);
                rock.name = $"Rock_{logicalIndex}_{col}";
                rock.transform.localPosition = cellLocalPos + new Vector3(
                    Random.Range(-0.3f, 0.3f), 0.05f, Random.Range(-0.3f, 0.3f));
                rock.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                rock.transform.localScale = Vector3.one * Random.Range(0.5f, 1.0f);
                DisableGameplayComponents(rock);
            }
        }

        return new MountainRow { rowRoot = rowRoot.transform, logicalRowIndex = logicalIndex };
    }

    private static void DisableGameplayComponents(GameObject obj)
    {
        if (obj == null) return;

        MonoBehaviour[] behaviours = obj.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour mb in behaviours)
        {
            if (mb == null) continue;
            string typeName = mb.GetType().Name;
            if (typeName == "Tile" || typeName == "SafePlatform" ||
                typeName == "CoinPickup" || typeName == "GrassPickup" ||
                typeName == "TreasureChestPickup" || typeName == "Obstacle" ||
                typeName == "ThundercloudHazard" || typeName == "RollingObstacleSpawner")
            {
                mb.enabled = false;
            }
        }

        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        foreach (Collider c in colliders)
        {
            if (c != null) Destroy(c);
        }
    }

    private Vector3 GetCellOffset(int col)
    {
        float colOffset = col - centerCol;
        Vector3 perpDir = new Vector3(-stepWorldOffset.z, 0f, stepWorldOffset.x).normalized;
        return perpDir * (colOffset * 1.8f);
    }

    // ── Mountain Recycling ──────────────────────────

    private void ShiftMountainDownOneStep()
    {
        // Move entire mountain back by one step (opposite of jump direction)
        mountainRoot.position -= stepWorldOffset;

        // Recycle rows that scrolled too far behind the deer
        // Use dot product along step direction instead of Y (works for any step angle)
        Vector3 dir = stepWorldOffset.normalized;
        float behindThreshold = -mountainRowCount * 0.5f * stepWorldOffset.magnitude;

        for (int i = 0; i < mountainRows.Length; i++)
        {
            if (mountainRows[i] == null || mountainRows[i].rowRoot == null) continue;

            float dot = Vector3.Dot(mountainRows[i].rowRoot.position, dir);
            if (dot < behindThreshold)
            {
                int highestIdx = FindHighestRowIndex();
                if (highestIdx >= 0)
                {
                    mountainRows[i].rowRoot.localPosition =
                        mountainRows[highestIdx].rowRoot.localPosition + stepWorldOffset;
                    mountainRows[i].logicalRowIndex = mountainRows[highestIdx].logicalRowIndex + 1;
                }
            }
        }
    }

    private int FindHighestRowIndex()
    {
        int bestIdx = -1;
        float bestDot = float.MinValue;
        Vector3 dir = stepWorldOffset.normalized;

        for (int i = 0; i < mountainRows.Length; i++)
        {
            if (mountainRows[i] == null || mountainRows[i].rowRoot == null) continue;
            float dot = Vector3.Dot(mountainRows[i].rowRoot.localPosition, dir);
            if (dot > bestDot) { bestDot = dot; bestIdx = i; }
        }
        return bestIdx;
    }

    // ── Deer Jump Loop ──────────────────────────────

    /// <summary>
    /// Main loop: deer jumps forward along the staircase → lands on next platform →
    /// mountain + deer shift back one step → repeat.
    /// One forward jump = one step of mountain scroll. Matches gameplay jump feel.
    /// </summary>
    private IEnumerator DeerJumpLoop()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            // ── Idle pause ──
            float idleTime = jumpInterval - jumpDuration;
            if (idleTime < 0.05f) idleTime = 0.05f;
            yield return new WaitForSeconds(idleTime);

            // ── Forward jump along the staircase ──
            if (deerAnimator != null)
            {
                deerAnimator.SetBool("isJump", true);
            }

            FaceStepDirection();

            Vector3 startPos = deerTransform.position;
            Vector3 targetPos = startPos + stepWorldOffset;

            float elapsed = 0f;
            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / jumpDuration);

                // Linear interpolation + sine arc on top (same as gameplay JumpController)
                Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;
                deerTransform.position = pos;

                ApplySquashStretch(t);

                yield return null;
            }

            // ── Land on next platform ──
            deerTransform.position = targetPos;
            deerTransform.localScale = deerBaseScale;

            if (deerAnimator != null)
            {
                deerAnimator.SetBool("isJump", false);
            }

            // ── Shift mountain + deer back one step ──
            // (mountain moves down-left, deer returns to screen center)
            ShiftMountainDownOneStep();
            deerTransform.position -= stepWorldOffset;
        }
    }

    /// <summary>
    /// Squash & stretch: squash at start/end, stretch at mid-arc.
    /// Single-parameter version (peak is at t=0.5).
    /// </summary>
    private void ApplySquashStretch(float t)
    {
        float phase = t < 0.5f ? t / 0.5f : (1f - t) / 0.5f;
        float sx = Mathf.Lerp(1f, 0.82f, phase);
        float sy = Mathf.Lerp(1f, 1.18f, phase);
        float sz = Mathf.Lerp(1f, 0.82f, phase);

        deerTransform.localScale = new Vector3(
            deerBaseScale.x * sx,
            deerBaseScale.y * sy,
            deerBaseScale.z * sz);
    }

    // ── Helpers ─────────────────────────────────────

    // ── Editor ──────────────────────────────────────

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (jumpInterval < 0.3f) jumpInterval = 0.3f;
        if (jumpInterval > 4f) jumpInterval = 4f;
        if (jumpDuration < 0.2f) jumpDuration = 0.2f;
        if (jumpDuration > 1.5f) jumpDuration = 1.5f;
        if (jumpHeight < 0.3f) jumpHeight = 0.3f;
        if (cameraOrthoSize < 2f) cameraOrthoSize = 2f;
        if (mountainRowCount < 6) mountainRowCount = 6;
        if (mountainRowCount > 50) mountainRowCount = 50;
    }
#endif
}
