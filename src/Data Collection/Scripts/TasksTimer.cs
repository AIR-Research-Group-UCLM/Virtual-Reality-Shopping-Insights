using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks and records task completion times.
/// </summary>
public class TasksTimer : MonoBehaviour
{
    /// <summary>
    /// Indicates whether the timer is currently running.
    /// </summary>
    public bool isCounting = false;

    /// <summary>
    /// The time when the timer started.
    /// </summary>
    private float startTime;

    /// <summary>
    /// The elapsed time since the timer started.
    /// </summary>
    private float elapsedTime;

    /// <summary>
    /// Reference to the DirectoryManager for managing file paths.
    /// </summary>
    [SerializeField] private DirectoryManager directoryManager;

    /// <summary>
    /// File path for saving the task completion times.
    /// </summary>
    private string _filePath;

    /// <summary>
    /// Adds a CSV header to the file if it does not already exist.
    /// </summary>
    private void AddCsvHeader()
    {
        if (!File.Exists(_filePath))
        {
            using (StreamWriter writer = new StreamWriter(_filePath, true))
            {
                writer.WriteLine("Frame, Timestamp, TaskDuration");
            }
        }
    }

    /// <summary>
    /// Starts the timer.
    /// </summary>
    public void StartCounting()
    {
        if (!isCounting)
        {
            isCounting = true;
            startTime = Time.time;
        }
    }

    /// <summary>
    /// Stops the timer and records the elapsed time.
    /// </summary>
    public void StopCounting()
    {
        if (isCounting)
        {
            isCounting = false;
            elapsedTime = Time.time - startTime;
            string data = $"{Time.frameCount}, {Time.time.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{elapsedTime.ToString("f4", CultureInfo.InvariantCulture)}";
            SaveData(data);
        }
    }

    /// <summary>
    /// Saves the recorded data to a file.
    /// </summary>
    /// <param name="data">The data to save.</param>
    private void SaveData(string data)
    {
        using (StreamWriter writer = new StreamWriter(_filePath, true))
        {
            writer.WriteLine(data);
        }
    }

    /// <summary>
    /// Gets the elapsed time since the timer started.
    /// </summary>
    /// <returns>The elapsed time.</returns>
    public float GetElapsedTime()
    {
        if (isCounting)
        {
            return Time.time - startTime;
        }

        return elapsedTime;
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

        string fileName = "TaskCompletionTimes" + sceneName + ".csv";
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
