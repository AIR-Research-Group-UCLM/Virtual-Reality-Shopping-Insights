using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks and records the release of objects after being grabbed.
/// </summary>
public class ObjectReleasesTracker : MonoBehaviour
{
    /// <summary>
    /// Reference to the right hand grab interactor.
    /// </summary>
    public HandGrabInteractor rightHand;

    /// <summary>
    /// Reference to the left hand grab interactor.
    /// </summary>
    public HandGrabInteractor leftHand;

    /// <summary>
    /// Reference to the DirectoryManager for managing file paths.
    /// </summary>
    [SerializeField] private DirectoryManager directoryManager;

    /// <summary>
    /// File path for saving the object release data.
    /// </summary>
    private string _filePath;

    /// <summary>
    /// Name of the currently grabbed object.
    /// </summary>
    private string _objectName;

    /// <summary>
    /// Counter for the duration the object is being grabbed.
    /// </summary>
    private float _counter = 0.0f;

    /// <summary>
    /// Flags indicating whether the hands were grabbing.
    /// </summary>
    private bool wasGrabbingR, wasGrabbingL = false;

    void Update()
    {
        bool isGrabbingRight = rightHand.State == InteractorState.Select;
        bool isGrabbingLeft = leftHand.State == InteractorState.Select;
        
        // Check which hand is grabbing and object, or if both are grabbing.
        if (isGrabbingRight)
        {
            _counter += Time.deltaTime;
            _objectName = rightHand.Interactable.gameObject.name;
            wasGrabbingR = true;
        }
        if (isGrabbingLeft)
        {
            _counter += Time.deltaTime;
            _objectName = leftHand.Interactable.gameObject.name;
            wasGrabbingL = true;
        }
        
        // Save data depending on the hand that was grabbing.
        if (wasGrabbingR && !isGrabbingRight)
        {
            string data = $"{Time.frameCount}, {Time.time.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_objectName}, {_counter.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{rightHand.State.ToString()}, {leftHand.State.ToString()}";
            SaveData(data);
            _counter = 0.0f;
            wasGrabbingR = false;
        }
        if (wasGrabbingL && !isGrabbingLeft)
        {
            string data = $"{Time.frameCount}, {Time.time.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_objectName}, {_counter.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{rightHand.State.ToString()}, {leftHand.State.ToString()}";
            SaveData(data);
            _counter = 0.0f;
            wasGrabbingL = false;
        }
    }

    /// <summary>
    /// Adds a CSV header to the file if it does not already exist.
    /// </summary>
    void AddCsvHeader()
    {
        if (!File.Exists(_filePath))
        {
            using (StreamWriter writer = new StreamWriter(_filePath, true))
            {
                writer.WriteLine("Frame, Timestamp, Object, DurationUntilRelease, RightHand_State, LeftHand_State");
            }
        }
    }
    
    /// <summary>
    /// Saves the recorded data to a file.
    /// </summary>
    /// <param name="data">The data to save.</param>
    void SaveData(string data)
    {
        using (StreamWriter writer = new StreamWriter(_filePath, true))
        {
            writer.WriteLine(data);
        }
    }
    
    /// <summary>
    /// Subscribes to the DirectoryReady event when the script is enabled.
    /// </summary>
    void OnEnable()
    {
        DirectoryManager.OnDirectoryReady += HandleDirectoryReady;
    }

    /// <summary>
    /// Handles the event when the directory is ready by setting up the file path and adding the CSV header.
    /// </summary>
    private void HandleDirectoryReady()
    {
        string path = directoryManager.sessionPath;
        string sceneName = SceneManager.GetActiveScene().name;

        string fileName = "ProductReleases" + sceneName + ".csv";
        _filePath = Path.Combine(path, fileName);
        AddCsvHeader();
    }

    /// <summary>
    /// Unsubscribes from the DirectoryReady event when the script is disabled.
    /// </summary>
    void OnDisable()
    {
        DirectoryManager.OnDirectoryReady -= HandleDirectoryReady;
    }
}
