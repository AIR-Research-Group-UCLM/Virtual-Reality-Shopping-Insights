using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages tracking and saving of shelf interaction data in batches.
/// </summary>
public class ShelvesTrackerDataManager : MonoBehaviour
{
    /// <summary>
    /// List to store batch data before saving.
    /// </summary>
    private List<string> batchData = new List<string>();

    /// <summary>
    /// Data string for the current batch.
    /// </summary>
    private string _data;

    /// <summary>
    /// Timer interval for batch saving in seconds.
    /// </summary>
    private float _batchTimer = 10.0f;

    /// <summary>
    /// Indicates whether the CSV header has been written.
    /// </summary>
    private static bool headerWritten = false;

    /// <summary>
    /// File path for saving the shelf interaction data.
    /// </summary>
    private string filePath = "";

    /// <summary>
    /// Reference to the DirectoryManager for managing file paths.
    /// </summary>
    [SerializeField] private DirectoryManager directoryManager;

    /// <summary>
    /// Adds shelf interaction data to the batch list.
    /// </summary>
    /// <param name="row">The shelf interaction data to add.</param>
    public void AddData(string row)
    {
        batchData.Add(row);
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
                writer.WriteLine("Frame, Timestamp, Shelf, AOI, HandR_x, HandR_y, " +
                                 "HandR_z, HandR_rot_x, HandR_rot_y, HandR_rot_z, HandL_x, " +
                                 "HandL_y, HandL_z, HandL_rot_x, " +
                                 "HandL_rot_y, HandL_rot_z, Velocity_HandR_x, Velocity_HandR_y, Velocity_HandR_z, " +
                                 "Velocity_HandL_x, Velocity_HandL_y, Velocity_HandL_z");
            }
        }
    }
    
    /// <summary>
    /// Coroutine to save data in batches at regular intervals.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    IEnumerator BatchSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_batchTimer);
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

        string fileName = "ShelvesData" + sceneName + ".csv";
        filePath = Path.Combine(path, fileName);
        if (!headerWritten)
        {
            AddCsvHeader();
            headerWritten = true;
        }

        StartCoroutine(BatchSaveCoroutine());
    }

    /// <summary>
    /// Unsubscribes from the DirectoryReady event when the script is disabled.
    /// </summary>
    void OnDisable()
    {
        DirectoryManager.OnDirectoryReady -= HandleDirectoryReady;
    }
}
