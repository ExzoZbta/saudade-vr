using UnityEngine;

public class MirrorController : MonoBehaviour
{
    [Header("Mirror Settings")]
    public Camera mirrorCamera;
    public RenderTexture mirrorTexture;
    public Transform mirrorSurface;
    public LayerMask mirrorVisibleLayers;

    [Header("Mirror Interaction")]
    public float maxMirrorDistance = 10f;
    public Transform playerCamera;

    private Renderer mirrorRenderer;

    void Start()
    {
        // Set up the mirror renderer
        mirrorRenderer = mirrorSurface.GetComponent<Renderer>();
        if (mirrorRenderer != null && mirrorTexture != null)
        {
            // Assign the render texture to the mirror's material
            mirrorRenderer.material.mainTexture = mirrorTexture;
        }

        // Configure the mirror camera
        if (mirrorCamera != null)
        {
            // Set the camera to only render objects on specific layers
            mirrorCamera.cullingMask = mirrorVisibleLayers;

            // Set the render texture as the target
            mirrorCamera.targetTexture = mirrorTexture;
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null || mirrorCamera == null)
            return;

    }
}