using System.Globalization;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

/// <summary>
/// Class for managing items stored in a shopping cart within the Unity environment.
/// </summary>
public class StoreInCart : MonoBehaviour
{
    /// <summary>
    /// List of colliders that can be interacted with.
    /// </summary>
    public Collider[] interactableColliders;

    /// <summary>
    /// Manages shopping cart tracking data.
    /// </summary>
    private ShoppingCartTrackerManager dataManager;

    [SerializeField] private HandGrabInteractor rightHand;
    [SerializeField] private HandGrabInteractor leftHand;

    [SerializeField] private bool isAdapted = false;

    private void Start()
    {
        // Find and assign the ShoppingCartTrackerManager instance.
        dataManager = FindObjectOfType<ShoppingCartTrackerManager>();
    }

    /// <summary>
    /// Trigger event handler for when a product is put in the shopping cart.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider is in the list of interactable colliders.
        foreach (Collider interactableCollider in interactableColliders)
        {
            if (other == interactableCollider)
            {
                // Set the entering object as a child of this object.
                other.transform.SetParent(transform);
                IInteractable interactable = other.transform.gameObject.GetComponent<IInteractable>();

                // Launch particles on interaction.
                ParticleLauncher pl = gameObject.GetComponent<ParticleLauncher>();
                pl.LaunchParticles();

                // Prevent position reset for the interacting object.
                PositionReset pr = other.GetComponent<PositionReset>();
                pr.DenyReset();

                // Handle object release if adapted version is used.
                if (isAdapted)
                {
                    if (rightHand.State == InteractorState.Select)
                        rightHand.ForceRelease();
                    else if (leftHand.State == InteractorState.Select)
                        leftHand.ForceRelease();
                }
                else
                {
                    interactable.Disable();
                }

                // Make the object be positioned at the center of the shopping cart.
                BoxCollider parentBoxCollider = gameObject.GetComponent<BoxCollider>();
                other.transform.SetParent(transform);

                // Calculate the world position of the center of the BoxCollider.
                Vector3 parentCenter = parentBoxCollider.center;
                Vector3 worldCenter = transform.TransformPoint(parentCenter);
                other.gameObject.transform.position = worldCenter;

                // Record interaction data.
                float timestamp = Time.time;
                string objectName = other.gameObject.name;
                string data = $"{Time.frameCount}, {timestamp.ToString("f4", CultureInfo.InvariantCulture)}, {objectName}, ADD";
                dataManager.AddShoppingData(data);

                // Enable the interactable object if it was disabled.
                if (interactable.State == InteractableState.Disabled)
                    interactable.Enable();
            }
        }
    }

    /// <summary>
    /// Adds a product to the cart without needing a physical collision.
    /// </summary>
    /// <param name="product">The product to add.</param>
    public void AddProductNoCollision(GameObject product)
    {
        float timestamp = Time.time;
        string objectName = product.name;
        string data = $"{timestamp}, {objectName}";

        dataManager.AddShoppingData(data);
    }

    /// <summary>
    /// Trigger event handler for when a product is taken out of the shopping cart.
    /// </summary>
    /// <param name="other">The collider that exited the trigger.</param>
    private void OnTriggerExit(Collider other)
    {
        // Check if the collider is in the list of interactable colliders.
        foreach (Collider interactableCollider in interactableColliders)
        {
            if (other == interactableCollider)
            {
                // Detach the object from this object.
                other.transform.SetParent(null);

                // Allow position reset for the exiting object.
                PositionReset pr = other.GetComponent<PositionReset>();
                if (pr != null)
                {
                    pr.AllowReset();
                }

                // Record interaction data.
                float timestamp = Time.time;
                string objectName = other.gameObject.name;
                string data = $"{Time.frameCount}, {timestamp.ToString("f4", CultureInfo.InvariantCulture)}, {objectName}, REMOVE";
                dataManager.AddShoppingData(data);
            }
        }
    }
}
