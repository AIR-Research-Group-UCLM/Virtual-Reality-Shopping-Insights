using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Oculus.Interaction.Locomotion;
using Oculus.Interaction.Throw;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks and records head and hands movement data.
/// </summary>
public class HeadAndHandsTrackingManager : MonoBehaviour
{
    /// <summary>
    /// List to store head positions.
    /// </summary>
    public List<Vector3> headPositions = new List<Vector3>();

    /// <summary>
    /// Indicates whether the CSV header has been written.
    /// </summary>
    private static bool headerWritten = false;

    /// <summary>
    /// File path for saving the tracking data.
    /// </summary>
    private string filePath = "";

    /// <summary>
    /// The current zone.
    /// </summary>
    public string Zone { get; set; }

    /// <summary>
    /// Ray for tracking.
    /// </summary>
    private Ray _ray;

    /// <summary>
    /// Distance for tracking.
    /// </summary>
    private float _distance = 0.0f;

    /// <summary>
    /// Timer for distance tracking.
    /// </summary>
    private float _timerDistance = 0.0f;

    /// <summary>
    /// Indicates whether to track zones.
    /// </summary>
    [SerializeField] private bool trackZones = false;

    /// <summary>
    /// Reference to the OVRCameraRig.
    /// </summary>
    [SerializeField] private OVRCameraRig _hmd;

    /// <summary>
    /// Reference to the right hand controller.
    /// </summary>
    [SerializeField] private OVRInput.Controller _handRight;

    /// <summary>
    /// Reference to the left hand controller.
    /// </summary>
    [SerializeField] private OVRInput.Controller _handLeft;

    /// <summary>
    /// Layer mask for tracking.
    /// </summary>
    [SerializeField] private LayerMask layer;

    /// <summary>
    /// Reference to the DirectoryManager for managing file paths.
    /// </summary>
    [SerializeField] private DirectoryManager directoryManager;

    /// <summary>
    /// Reference to the left hand teleport interactor.
    /// </summary>
    public TeleportInteractor leftHand;

    /// <summary>
    /// Reference to the right hand teleport interactor.
    /// </summary>
    public TeleportInteractor rightHand;

    /// <summary>
    /// List of BoxColliders for shelves.
    /// </summary>
    [SerializeField]
    private List<BoxCollider> _shelvesColliders = new List<BoxCollider>();

    /// <summary>
    /// Last position of the left hand.
    /// </summary>
    private Vector3 _lastPositionL, _lastPositionR;

    /// <summary>
    /// Closest collider to the HMD.
    /// </summary>
    private BoxCollider closestCollider = null;

    /// <summary>
    /// Indicates whether to start collecting data.
    /// </summary>
    private bool _startCollecting = false;

    /// <summary>
    /// Previous position of the HMD.
    /// </summary>
    private Vector3 previousPosition;

    /// <summary>
    /// Status of the tracker.
    /// </summary>
    private string status;

    /// <summary>
    /// Data string for the current tracking data.
    /// </summary>
    private string _data;

    void Update()
    {
        if (_startCollecting)
        {
            Vector3 handVelocityR = Vector3.zero;
            Vector3 handVelocityL = Vector3.zero;
            float timestamp = Time.time;
            Vector3 rotationAngles = _hmd.centerEyeAnchor.rotation.eulerAngles;
            Vector3 headPosition = _hmd.centerEyeAnchor.position;
            Vector3 handRpostion = OVRInput.GetLocalControllerPosition(_handRight);
            Vector3 handLpostion = OVRInput.GetLocalControllerPosition(_handLeft);
            Vector3 handRrotation = OVRInput.GetLocalControllerRotation(_handRight).eulerAngles;
            Vector3 handLrotation = OVRInput.GetLocalControllerRotation(_handLeft).eulerAngles;

            headPositions.Add(headPosition);

            // Hand velocity calculation
            handVelocityR = VelocityCalculatorUtilMethods.ToLinearVelocity(_lastPositionR, handRpostion, Time.deltaTime);
            handVelocityL = VelocityCalculatorUtilMethods.ToLinearVelocity(_lastPositionL, handLpostion, Time.deltaTime);

            // Distance from head
            if (_timerDistance >= 1.0f)
            {
                ComputeClosestBoxCollider();
                _timerDistance = 0.0f;
            }
            else
            {
                _timerDistance += Time.deltaTime;
            }

            if (closestCollider == null)
            {
                _distance = 0.0f;
            }
            else
            {
                _distance = Vector3.Distance(headPosition, closestCollider.ClosestPoint(headPosition));
            }

            if (trackZones)
            {
                _data = $"{Time.frameCount}, " +
                        $"{timestamp.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{headPosition.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{headPosition.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{headPosition.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{rotationAngles.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{rotationAngles.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{rotationAngles.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRpostion.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRpostion.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRpostion.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRrotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRrotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRrotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLpostion.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLpostion.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLpostion.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLrotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLrotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLrotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityR.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityR.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityR.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityL.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityL.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityL.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{Zone.ToString()}, {_distance.ToString("f4", CultureInfo.InvariantCulture)}";
            }
            else
            {
                _data = $"{Time.frameCount}, " +
                        $"{timestamp.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{headPosition.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{headPosition.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{headPosition.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{rotationAngles.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{rotationAngles.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{rotationAngles.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRpostion.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRpostion.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRpostion.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRrotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRrotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handRrotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLpostion.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLpostion.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLpostion.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLrotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLrotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handLrotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityR.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityR.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityR.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityL.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityL.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{handVelocityL.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                        $"{_distance.ToString("f4", CultureInfo.InvariantCulture)}";
            }

            // Update last position for next velocity calculation
            _lastPositionL = handLpostion;
            _lastPositionR = handRpostion;

            SaveData(_data);   
        }
    }
    
    /// <summary>
    /// Adds a CSV header to the file if it does not already exist.
    /// </summary>
    void AddCsvHeader()
    {
        if (!File.Exists(filePath))
        {
            if (trackZones)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Frame, Timestamp, HMD_x, HMD_y, HMD_z, HMD_rot_x, HMD_rot_y, HMD_rot_z, HandR_x, HandR_y, " +
                                     "HandR_z, HandR_rot_x, HandR_rot_y, HandR_rot_z, HandL_x, HandL_y, HandL_z, HandL_rot_x, " +
                                     "HandL_rot_y, HandL_rot_z, Velocity_HandR_x, Velocity_HandR_y, Velocity_HandR_z, Velocity_HandL_x, " +
                                     "Velocity_HandL_y, Velocity_HandL_z, Zone, Distance");
                } 
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Frame, Timestamp, HMD_x, HMD_y, HMD_z, HMD_rot_x, HMD_rot_y, HMD_rot_z, HandR_x, HandR_y, " +
                                     "HandR_z, HandR_rot_x, HandR_rot_y, HandR_rot_z, HandL_x, HandL_y, HandL_z, HandL_rot_x, " +
                                     "HandL_rot_y, HandL_rot_z, Velocity_HandR_x, Velocity_HandR_y, Velocity_HandR_z, Velocity_HandL_x, " +
                                     "Velocity_HandL_y, Velocity_HandL_z, Distance");
                }    
            }
        }
    }
    
    /// <summary>
    /// Saves the recorded data to a file.
    /// </summary>
    /// <param name="data">The data to save.</param>
    void SaveData(string data)
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
           writer.WriteLine(data);
        }
    }

    private void OnApplicationQuit()
    {
        Vector3SerializationHelper.SerializeVector3List(headPositions, $"PositionPlayerData{DateTime.Now.ToString("yyyy-MM-dddd-HH-mm-ss")}.json");
    }

    /// <summary>
    /// Computes the closest BoxCollider to the HMD.
    /// </summary>
    public void ComputeClosestBoxCollider()
    {
        float minDistance = float.MaxValue;
        
        foreach (BoxCollider collider in _shelvesColliders)
        {
            Vector3 closestPoint = collider.ClosestPoint(_hmd.centerEyeAnchor.position);
            float distance = Vector3.Distance(_hmd.centerEyeAnchor.position, closestPoint);

            if (distance < minDistance)
            {
                closestCollider = collider;
                minDistance = distance;
                Debug.Log("Closest collider GO: " + closestCollider.gameObject.name);
            }
        }
    }
    
    void OnEnable()
    {
        DirectoryManager.OnDirectoryReady += HandleDirectoryReady;
    }

    private void HandleDirectoryReady()
    {
        string path = directoryManager.sessionPath;
        string sceneName = SceneManager.GetActiveScene().name;

        string fileName = "HeadHandsData" + sceneName + ".csv";
        filePath = Path.Combine(path, fileName);
        if (!headerWritten)
        {
            AddCsvHeader();
            headerWritten = true;
        }
        
        _lastPositionL = Vector3.zero;
        _lastPositionR = Vector3.zero;

        Zone = "Start";
        _startCollecting = true;
        previousPosition = _hmd.centerEyeAnchor.position;
    }

    void OnDisable()
    {
        DirectoryManager.OnDirectoryReady -= HandleDirectoryReady;
    }
}
