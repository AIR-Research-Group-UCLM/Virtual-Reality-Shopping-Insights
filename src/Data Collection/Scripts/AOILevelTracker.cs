using System.Globalization;
using Oculus.Interaction.Throw;
using UnityEngine;

/// <summary>
/// Tracks and records hand interactions within an Area of Interest (AOI) level.
/// </summary>
public class AOILevelTracker : MonoBehaviour
{
    /// <summary>
    /// Indicates whether tracking is active.
    /// </summary>
    private bool _startTracking;

    /// <summary>
    /// Reference to the right hand controller.
    /// </summary>
    [SerializeField] private OVRInput.Controller _handRight;

    /// <summary>
    /// Reference to the left hand controller.
    /// </summary>
    [SerializeField] private OVRInput.Controller _handLeft;

    /// <summary>
    /// Reference to the ShelvesTrackerDataManager for managing tracking data.
    /// </summary>
    [SerializeField] private ShelvesTrackerDataManager _shelvesTrackerDataManager;

    /// <summary>
    /// Last position of the left hand.
    /// </summary>
    private Vector3 _lastPositionL;

    /// <summary>
    /// Last position of the right hand.
    /// </summary>
    private Vector3 _lastPositionR;

    void Start()
    {
        _lastPositionL = Vector3.zero;
        _lastPositionR = Vector3.zero;
    }

    void Update()
    {
        if (_startTracking)
        {
            Vector3 handVelocityR = Vector3.zero;
            Vector3 handVelocityL = Vector3.zero;
            float timestamp = Time.time;
            Vector3 handRPosition = OVRInput.GetLocalControllerPosition(_handRight);
            Vector3 handLPosition = OVRInput.GetLocalControllerPosition(_handLeft);
            Vector3 handRRotation = OVRInput.GetLocalControllerRotation(_handRight).eulerAngles;
            Vector3 handLRotation = OVRInput.GetLocalControllerRotation(_handLeft).eulerAngles;

            // Hand velocity calculation
            handVelocityR = VelocityCalculatorUtilMethods.ToLinearVelocity(_lastPositionR, handRPosition, Time.deltaTime);
            handVelocityL = VelocityCalculatorUtilMethods.ToLinearVelocity(_lastPositionL, handLPosition, Time.deltaTime);
            
            string data = $"{Time.frameCount}, " +
                          $"{timestamp.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{gameObject.transform.parent}, {gameObject.name}, " +
                          $"{handRPosition.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handRPosition.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handRPosition.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handRRotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handRRotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handRRotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handLPosition.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handLPosition.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handLPosition.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handLRotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handLRotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handLRotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handVelocityR.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handVelocityR.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handVelocityR.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handVelocityL.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handVelocityL.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{handVelocityL.z.ToString("f4", CultureInfo.InvariantCulture)}";

            // Update last position for next velocity calculation
            _lastPositionL = handLPosition;
            _lastPositionR = handRPosition;
            
            _shelvesTrackerDataManager.AddData(data);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HandGrabInteractor"))
        {
            _startTracking = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("HandGrabInteractor"))
        {
            _startTracking = false;
        }
    }
}
