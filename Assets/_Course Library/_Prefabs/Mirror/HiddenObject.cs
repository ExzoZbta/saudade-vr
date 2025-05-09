using UnityEngine;

public class HiddenObject : MonoBehaviour
{
    [Header("Visibility Settings")]
    public bool visibleToPlayer = false;
    public bool visibleToMirror = true;

    void Start()
    {
        UpdateVisibility();
    }

    public void UpdateVisibility()
    {
        // Get the object's current layer
        int currentLayer = gameObject.layer;

        // Determine which layer it should be on based on visibility settings
        if (visibleToPlayer)
        {
            gameObject.layer = LayerMask.NameToLayer("VisibleToPlayer");
        }
        else if (visibleToMirror)
        {
            gameObject.layer = LayerMask.NameToLayer("VisibleToMirror");
        }
    }

    // Method to toggle visibility (optional)
    public void TogglePlayerVisibility()
    {
        visibleToPlayer = !visibleToPlayer;
        UpdateVisibility();
    }

    public void ToggleMirrorVisibility()
    {
        visibleToMirror = !visibleToMirror;
        UpdateVisibility();
    }
}