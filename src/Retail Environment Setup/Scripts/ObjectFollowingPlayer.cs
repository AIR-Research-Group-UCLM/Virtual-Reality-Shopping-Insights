using UnityEngine;

/// <summary>
/// Class for making objects follow the player's head movement in Unity.
/// </summary>
public class ObjectFollowingPlayer : MonoBehaviour
{
    /// <summary>
    /// Transform representing the player's head.
    /// </summary>
    public Transform head;

    /// <summary>
    /// Transform representing the object on the left side of the player.
    /// </summary>
    public Transform leftObject;

    /// <summary>
    /// Transform representing the object on the right side of the player.
    /// </summary>
    public Transform rightObject;

    /// <summary>
    /// Distance from the head to the right object.
    /// </summary>
    public float distanceFromHeadRight = 0.5f;

    /// <summary>
    /// Distance from the head to the left object.
    /// </summary>
    private float distanceFromHeadLeft = 0.6f;

    /// <summary>
    /// Stores the previous position of the head.
    /// </summary>
    private Vector3 previousHeadPosition;

    /// <summary>
    /// Stores the previous rotation of the head.
    /// </summary>
    private Quaternion previousHeadRotation;
    
    private void Start()
    {
        // Store the initial position and rotation of the head.
        previousHeadPosition = head.position;
        previousHeadRotation = head.rotation;
    }
    
    private void Update()
    {
        // If any required transform is missing, exit the method.
        if (head == null || leftObject == null || rightObject == null) return;

        // Calculate the direction to the right and left based on the head's rotation.
        Vector3 rightDirection = head.right * distanceFromHeadRight;
        Vector3 leftDirection = -head.right * distanceFromHeadLeft;

        // Adjust the positions of the objects, maintaining the desired heights.
        // Position the left object relative to the player, with a height adjustment.
        leftObject.position = head.position + leftDirection + Vector3.up * 0.465f; // Adjust to maintain desired height.

        // Position the right object to the right of the player, with a specific height adjustment.
        rightObject.position = head.position + rightDirection; // Place the cart to the right of the player.
        rightObject.position = new Vector3(rightObject.position.x, 1.23197f, rightObject.position.z); // Adjust the cart's height.
    }
}
