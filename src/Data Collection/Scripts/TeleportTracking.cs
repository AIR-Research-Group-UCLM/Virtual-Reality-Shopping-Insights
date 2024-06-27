using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Oculus.Interaction;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tracks and records teleportation events and the time spent in different zones.
/// </summary>
public class TeleportTracking : MonoBehaviour
{
    /// <summary>
    /// Indicates whether the object is currently being hovered over.
    /// </summary>
    public bool IsHovered { get; set; }

    /// <summary>
    /// Dictionary to store the start time when an object is hovered over.
    /// </summary>
    private Dictionary<GameObject, float> startTime = new Dictionary<GameObject, float>();

    /// <summary>
    /// Manages the teleportation data.
    /// </summary>
    private TeleportDataManager dataManager;

    /// <summary>
    /// Reference to the OVRCameraRig for head-mounted display tracking.
    /// </summary>
    [SerializeField] private OVRCameraRig hmd;

    /// <summary>
    /// Manages tracking of the head and hands.
    /// </summary>
    [SerializeField] private HeadAndHandsTrackingManager HHManager;

    /// <summary>
    /// Stores the last recorded position of the HMD.
    /// </summary>
    private Vector3 _lastHmdPosition;

    /// <summary>
    /// Distance traveled since the last teleportation event.
    /// </summary>
    private float _distance;

    /// <summary>
    /// Indicates whether a teleportation event occurred.
    /// </summary>
    public bool wasTP { get; set; }

    /// <summary>
    /// Timer to track intervals for data recording.
    /// </summary>
    private float _timer = 0.0f;

    /// <summary>
    /// Interval for recording data (in seconds).
    /// </summary>
    private float _interval = 0.5f;

    /// <summary>
    /// Allows segmentation of data by teleportation zones.
    /// </summary>
    public bool AllowSegmentationByTeleportationZones = false;

    void Start()
    {
        // Find and assign the TeleportDataManager instance.
        dataManager = FindObjectOfType<TeleportDataManager>();

        // Initialize teleportation flag and HMD position.
        wasTP = false;
        _lastHmdPosition = hmd.centerEyeAnchor.position;
    }
    
    void Update()
    {
        // Update timer for data recording interval.
        _timer += Time.deltaTime;

        if (IsHovered)
        {
            // Record the start time when the object is first hovered over.
            if (!startTime.ContainsKey(gameObject))
            {
                startTime[gameObject] = Time.time;
            }
        }
        else if (startTime.ContainsKey(gameObject))
        {
            // Calculate the duration of the hover and prepare data for recording.
            float duration = Time.time - startTime[gameObject];
            float timestamp = Time.time;
            Vector3 headPosition = hmd.centerEyeAnchor.position;
            string[] parts = gameObject.name.Split('_');
            string lastPart = parts[parts.Length - 1];
            string zone = "Unknown";

            if (AllowSegmentationByTeleportationZones)
            {
                // Determine the zone based on the object's name.
                if (lastPart == "Near" || lastPart == "Adjacent")
                {
                    zone = lastPart;
                }
                else
                {
                    zone = "Far";
                }

                HHManager.Zone = zone;
            }

            if (wasTP)
            {
                // Calculate the distance traveled if a teleportation event occurred.
                _distance = CalculateDistance2D(_lastHmdPosition, headPosition);
            }
            else
            {
                _distance = 0.0f;
            }

            // Prepare data string for recording.
            string data = $"{Time.frameCount}, " +
                          $"{timestamp.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{gameObject.name}, {headPosition.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{headPosition.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{duration.ToString("f4", CultureInfo.InvariantCulture)}, {wasTP.ToString()}, {zone}, " +
                          $"{_distance.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{HHManager.rightHand.State.ToString()}, {HHManager.leftHand.State.ToString()}";

            if (_timer >= _interval)
            {
                // Record data and reset the timer.
                dataManager.AddTPData(data);
                _timer = 0.0f;
                startTime.Remove(gameObject);
            }
        }
    }

    /// <summary>
    /// Calculates the 2D distance between two positions, ignoring the Y component.
    /// </summary>
    /// <param name="initialPosition">The initial position.</param>
    /// <param name="finalPosition">The final position.</param>
    /// <returns>The 2D distance between the positions.</returns>
    float CalculateDistance2D(Vector3 initialPosition, Vector3 finalPosition)
    {
        // Ignore Y component, no teleport can occur to a higher floor level.
        return Mathf.Sqrt(Mathf.Pow(finalPosition.x - initialPosition.x, 2) + Mathf.Pow(finalPosition.z - initialPosition.z, 2));
    }
}
