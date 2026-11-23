using UnityEngine;
using HexR;

/// <summary>
/// Changes the cube's material color while any finger is touching it,
/// and reverts to the original color when all fingers are released.
/// Auto-attaches at runtime to every active GameObject that has a CustomTrigger.
/// </summary>
public class CubeTouchColor : MonoBehaviour
{
    [Tooltip("Color shown while at least one finger is touching the cube.")]
    public Color TouchColor = new Color(0.2f, 0.85f, 0.4f);

    private Renderer cubeRenderer;
    private Color originalColor;
    private int fingerCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoAttach()
    {
        foreach (var ct in FindObjectsOfType<CustomTrigger>())
        {
            if (ct.GetComponent<CubeTouchColor>() == null)
                ct.gameObject.AddComponent<CubeTouchColor>();
        }
    }

    void Awake()
    {
        cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
            originalColor = cubeRenderer.material.color;
    }

    void OnDisable()
    {
        fingerCount = 0;
        if (cubeRenderer != null)
            cubeRenderer.material.color = originalColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsFinger(other)) return;
        fingerCount++;
        UpdateColor();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsFinger(other)) return;
        fingerCount = Mathf.Max(0, fingerCount - 1);
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (cubeRenderer == null) return;
        cubeRenderer.material.color = fingerCount > 0 ? TouchColor : originalColor;
    }

    private static bool IsFinger(Collider other)
    {
        return other.TryGetComponent(out HapticFingerTrigger _);
    }
}
