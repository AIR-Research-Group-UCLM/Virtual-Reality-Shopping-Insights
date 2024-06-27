using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks eye gaze and interacts with objects using raycasting.
/// </summary>
public class EyeTracker : MonoBehaviour
{
    /// <summary>
    /// Maximum distance for the raycast.
    /// </summary>
    [SerializeField]
    private float rayDistance = 1.0f;

    /// <summary>
    /// Layer mask for filtering raycast hits.
    /// </summary>
    [SerializeField]
    private LayerMask layersToInclude;

    /// <summary>
    /// Default color for the ray.
    /// </summary>
    [SerializeField]
    private Color rayColorDefault = Color.yellow;

    /// <summary>
    /// Color for the ray when hovering over an interactable object.
    /// </summary>
    [SerializeField]
    private Color rayColorHover = Color.blue;

    /// <summary>
    /// List of interactable objects currently being hovered over.
    /// </summary>
    private List<EyeInteractable> eyeInteractables = new List<EyeInteractable>();

    private void FixedUpdate()
    {
        RaycastHit hit;
        Vector3 raycastDirection = transform.TransformDirection(Vector3.forward) * rayDistance;
        Debug.DrawRay(transform.parent.position, raycastDirection, Color.green);

        if (Physics.Raycast(transform.parent.position, raycastDirection, out hit, Mathf.Infinity, layersToInclude))
        {
            // If something is already selected, unselect it first
            UnSelect();
            var eyeInteractable = hit.transform.GetComponent<EyeInteractable>();
            eyeInteractables.Add(eyeInteractable);
            eyeInteractable.IsHovered = true;
        }
        else
        {
            UnSelect(true);
        }
    }

    /// <summary>
    /// Unselects all interactable objects.
    /// </summary>
    /// <param name="clear">Indicates whether to clear the list of interactables.</param>
    void UnSelect(bool clear = false)
    {
        foreach (var interactable in eyeInteractables)
        {
            interactable.IsHovered = false;
        }

        if (clear)
        {
            eyeInteractables.Clear();
        }
    }
}
