using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// Handles interactions when the object is gazed at by the eye tracker.
/// </summary>
public class EyeInteractable : MonoBehaviour
{
    /// <summary>
    /// Indicates whether the object is currently being hovered over by the eye gaze.
    /// </summary>
    public bool IsHovered { get; set; }

    /// <summary>
    /// Event triggered when the object is hovered over.
    /// </summary>
    [SerializeField] 
    private UnityEvent<GameObject> OnObjectHovered;

    /// <summary>
    /// Dictionary to store the start time of interactions.
    /// </summary>
    private Dictionary<GameObject, float> startTime = new Dictionary<GameObject, float>();

    /// <summary>
    /// Reference to the EyeTrackerDataManager for managing eye tracking data.
    /// </summary>
    private EyeTrackerDataManager dataManager;

    /// <summary>
    /// GameObject representing the left eye gaze.
    /// </summary>
    [SerializeField]
    private GameObject eyeGazeLeft;

    /// <summary>
    /// GameObject representing the right eye gaze.
    /// </summary>
    [SerializeField]
    private GameObject eyeGazeRight;

    /// <summary>
    /// Timer for tracking the duration of gaze interactions.
    /// </summary>
    private float _timer = 0.0f;

    /// <summary>
    /// Interval for recording data (in seconds).
    /// </summary>
    private float _interval = 0.5f;

    void Start()
    {
        dataManager = FindObjectOfType<EyeTrackerDataManager>();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (IsHovered)
        {
            if (!startTime.ContainsKey(gameObject))
            {
                startTime[gameObject] = Time.time;
            }
        }
        else if (startTime.ContainsKey(gameObject))
        {
            float duration = Time.time - startTime[gameObject];
            float timestamp = Time.time;
            string data = $"{timestamp}, {gameObject.name}, {gameObject.transform.parent.name}, {duration}, " +
                          $"{eyeGazeLeft.transform.position.x}, {eyeGazeLeft.transform.position.y}, " +
                          $"{eyeGazeLeft.transform.position.z}, {eyeGazeLeft.transform.rotation.x}, " +
                          $"{eyeGazeLeft.transform.rotation.y}, {eyeGazeLeft.transform.rotation.z}, " +
                          $"{eyeGazeRight.transform.position.x}, {eyeGazeRight.transform.position.y}, " +
                          $"{eyeGazeRight.transform.position.z}, {eyeGazeRight.transform.rotation.x}, " +
                          $"{eyeGazeRight.transform.rotation.y}, {eyeGazeRight.transform.rotation.z}";

            if (_timer >= _interval)
            {
                dataManager.AddCollisionData(data, "Products");
                startTime.Remove(gameObject);
                _timer = 0.0f;
            }
        }
    }
}
