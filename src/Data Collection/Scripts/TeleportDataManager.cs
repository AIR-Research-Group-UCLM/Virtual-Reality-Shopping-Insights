using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the collection and saving of teleportation data in batches.
/// </summary>
public class TeleportDataManager : MonoBehaviour
{
    /// <summary>
    /// List to store batch data before saving.
    /// </summary>
    private List<string> batchData = new List<string>();

    /// <summary>
    /// Timer interval for batch saving in seconds.
    /// </summary>
    private float batchTimer = 42.0f;

    /// <summary>
    /// File path for saving the teleportation data.
    /// </summary>
    private string filePath;

    /// <summary>
    /// Indicates whether the CSV header has been written.
    /// </summary>
    private static bool headerWritten = false;

    /// <summary>
    /// Reference to the DirectoryManager for managing file paths.
    /// </summary>
    [SerializeField] private DirectoryManager directoryManager;

    /// <summary>
    /// Adds teleportation data to the batch list.
    /// </summary>
    /// <param name="data">The teleportation data to add.</param>
    public void AddTPData(string data)
    {
        batchData.Add(data);
    }

    /// <summary>
    /// Coroutine to save data in batches at regular intervals.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    IEnumerator BatchSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(batchTimer);
            SaveData();
        }
    }

    /// <summary>
    /// Saves the collected batch data to a file.
    /// </summary>
    void SaveData()
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            foreach (string data in batchData)
            {
                writer.WriteLine(data);
            }
        }
        batchData.Clear();
    }

    /// <summary>
    /// Adds a CSV header to the file if it does not already exist.
    /// </summary>
    void AddCsvHeader()
    {
        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("Frame, Timestamp, TPHotspot, HMD_x, HMD_z, Duration, WasTP, ZOI, Distance, RightHand_State, LeftHand_State");
            }
        }
    }
    
    /// <summary>
    /// Saves remaining data when the object is destroyed.
    /// </summary>
    void OnDestroy()
    {
        SaveData(); // Save remaining data on destruction.
    }
    
    /// <summary>
    /// Subscribes to the DirectoryReady event when the script is enabled.
    /// </summary>
    void OnEnable()
    {
        DirectoryManager.OnDirectoryReady += HandleDirectoryReady;
    }

    /// <summary>
    /// Handles the event when the directory is ready by setting up the file path and starting the batch save coroutine.
    /// </summary>
    private void HandleDirectoryReady()
    {
        string path = directoryManager.sessionPath;
        string sceneName = SceneManager.GetActiveScene().name;

        string fileName = "TeleportData" + sceneName + ".csv";
        filePath = Path.Combine(path, fileName);
        Debug.Log(filePath);
        StartCoroutine(BatchSaveCoroutine());
        if (!headerWritten)
        {
            AddCsvHeader();
            headerWritten = true;
        }
    }

    /// <summary>
    /// Unsubscribes from the DirectoryReady event when the script is disabled.
    /// </summary>
    void OnDisable()
    {
        DirectoryManager.OnDirectoryReady -= HandleDirectoryReady;
    }
}
