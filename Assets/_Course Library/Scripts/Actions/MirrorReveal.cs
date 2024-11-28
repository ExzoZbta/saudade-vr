using UnityEngine;

public class MirrorReveal : MonoBehaviour
{
    [Header("Mirror Settings")]
    public Camera mirrorCamera;
    public float checkRadius = 5f;
    public LayerMask revealableLayer;

    [Header("Optional Settings")]
    public float fadeSpeed = 2f;
    public bool useFadeEffect = true;

    private void Update()
    {
        // Find all objects on the revealable layer within radius
        Collider[] hitColliders = Physics.OverlapSphere(mirrorCamera.transform.position, checkRadius, revealableLayer);

        foreach (var hitCollider in hitColliders)
        {
            // Check if object is within mirror camera's view
            Vector3 directionToTarget = (hitCollider.transform.position - mirrorCamera.transform.position).normalized;
            float angle = Vector3.Angle(mirrorCamera.transform.forward, directionToTarget);

            // Get the renderer component
            Renderer renderer = hitCollider.GetComponent<Renderer>();
            if (renderer == null) continue;

            // Get or create material property block to modify properties efficiently
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);

            // Check if object is within mirror camera's field of view
            if (angle <= mirrorCamera.fieldOfView * 0.5f)
            {
                // Perform raycast to check for obstacles
                RaycastHit hit;
                if (Physics.Raycast(mirrorCamera.transform.position, directionToTarget, out hit, checkRadius))
                {
                    if (hit.collider == hitCollider)
                    {
                        // Object is visible in mirror
                        if (useFadeEffect)
                        {
                            float currentAlpha = propBlock.GetFloat("_Alpha");
                            float newAlpha = Mathf.Min(currentAlpha + Time.deltaTime * fadeSpeed, 1f);
                            propBlock.SetFloat("_Alpha", newAlpha);
                        }
                        else
                        {
                            propBlock.SetFloat("_Alpha", 1f);
                        }
                    }
                }
            }
            else
            {
                // Object is not in mirror view
                if (useFadeEffect)
                {
                    float currentAlpha = propBlock.GetFloat("_Alpha");
                    float newAlpha = Mathf.Max(currentAlpha - Time.deltaTime * fadeSpeed, 0f);
                    propBlock.SetFloat("_Alpha", newAlpha);
                }
                else
                {
                    propBlock.SetFloat("_Alpha", 0f);
                }
            }

            // Apply the property block to the renderer
            renderer.SetPropertyBlock(propBlock);
        }
    }
}
