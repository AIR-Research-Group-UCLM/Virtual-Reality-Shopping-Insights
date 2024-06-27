using System;
using System.Globalization;
using System.IO;
using Oculus.Interaction;
using Oculus.Interaction.Locomotion;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks and records the turning interactions of the player using Oculus hand interactors.
/// </summary>
public class TurnerTracker : MonoBehaviour
{
    /// <summary>
    /// Interactor for the right hand's locomotion turning.
    /// </summary>
    public LocomotionTurnerInteractor rightHand;

    /// <summary>
    /// Interactor for the left hand's locomotion turning.
    /// </summary>
    public LocomotionTurnerInteractor leftHand;

    /// <summary>
    /// Manager for handling directory paths.
    /// </summary>
    [SerializeField] private DirectoryManager directoryManager;

    /// <summary>
    /// File path for saving the turning data.
    /// </summary>
    private string _filePath;

    /// <summary>
    /// Update method called once per frame. Checks the state of hand interactors and saves data if necessary.
    /// </summary>
    private void Update()
    {
        if (rightHand.State == InteractorState.Select || leftHand.State == InteractorState.Select)
        {
            SaveData();
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
                writer.WriteLine("Frame, Timestamp, RightHand_State, LeftHand_State");
            }
        }
    }

    /// <summary>
    /// Saves the current state data of the hand interactors to the CSV file.
    /// </summary>
    public void SaveData()
    {
        string data = $"{Time.frameCount}, {Time.time.ToString("f4", CultureInfo.InvariantCulture)}, " +
                      $"{rightHand.State.ToString()}, {leftHand.State.ToString()}";
        using (StreamWriter writer = new StreamWriter(_filePath, true))
        {
            writer.WriteLine(data);
        }
    }

    /// <summary>
    /// Method called when the script is enabled. Subscribes to the directory ready event.
    /// </summary>
    void OnEnable()
    {
        DirectoryManager.OnDirectoryReady += HandleDirectoryReady;
    }

    /// <summary>
    /// Handles the event when the directory for storing csv files is ready. Sets up the file path and adds the CSV header.
    /// </summary>
    private void HandleDirectoryReady()
    {
        string path = directoryManager.sessionPath;
        string sceneName = SceneManager.GetActiveScene().name;

        string fileName = "Turnings" + sceneName + ".csv";
        _filePath = Path.Combine(path, fileName);
        AddCsvHeader();
    }

    /// <summary>
    /// Method called when the script is disabled. Unsubscribes from the directory ready event.
    /// </summary>
    void OnDisable()
    {
        DirectoryManager.OnDirectoryReady -= HandleDirectoryReady;
    }
}
