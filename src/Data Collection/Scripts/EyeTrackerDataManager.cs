using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the tracking and saving of eye tracker data for AOIs and products.
/// </summary>
public class EyeTrackerDataManager : MonoBehaviour
{
    /// <summary>
    /// List to store batch data for AOIs.
    /// </summary>
    private List<string> batchDataAOI = new List<string>();

    /// <summary>
    /// List to store batch data for products.
    /// </summary>
    private List<string> batchDataProducts = new List<string>();

    /// <summary>
    /// Timer interval for batch saving in seconds.
    /// </summary>
    private float batchTimer = 10.0f;

    /// <summary>
    /// File path for saving AOI data.
    /// </summary>
    private string filePath;

    /// <summary>
    /// File path for saving product data.
    /// </summary>
    private string filePath2;

    /// <summary>
    /// Indicates whether the CSV header has been written.
    /// </summary>
    private static bool headerWritten = false;

    /// <summary>
    /// List to store hit points for serialization.
    /// </summary>
    private List<Vector3> _hitPoints = new List<Vector3>();

    /// <summary>
    /// Reference to the DirectoryManager for managing file paths.
    /// </summary>
    [SerializeField] private DirectoryManager directoryManager;

    /// <summary>
    /// Adds collision data to the appropriate batch list based on the filename.
    /// </summary>
    /// <param name="data">The collision data to add.</param>
    /// <param name="filename">The filename indicating the type of data ("AOI" or "Products").</param>
    public void AddCollisionData(string data, string filename)
    {
        if (filename == "AOI")
        {
            batchDataAOI.Add(data);
        }
        else if (filename == "Products")
        {
            batchDataProducts.Add(data);
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
            yield return new WaitForSeconds(batchTimer);
            Debug.Log("SAVING EYE TRACKING DATA");
            SaveData();
        }
    }

    /// <summary>
    /// Saves the collected batch data to the respective files.
    /// </summary>
    void SaveData()
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            foreach (string data in batchDataAOI)
            {
                writer.WriteLine(data);
            }
        }
        batchDataAOI.Clear();
        
        using (StreamWriter writer = new StreamWriter(filePath2, true))
        {
            foreach (string data in batchDataProducts)
            {
                writer.WriteLine(data);
            }
        }
        batchDataProducts.Clear();
    }

    /// <summary>
    /// Adds a CSV header to the files if they do not already exist.
    /// </summary>
    void AddCsvHeader()
    {
        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("Frame, Timestamp, Product/AOI, Section/Shelf, EyeLeftPosition_x, " +
                                 "EyeLeftPosition_y, EyeLeftPosition_z, EyeLeftRotation_x, EyeLeftRotation_y, " +
                                 "EyeLeftRotation_z, EyeRightPosition_x, EyeRightPosition_y, EyeRightPosition_z," +
                                 "EyeRightRotation_x, EyeRightRotation_y, EyeRightRotation_z, HMD_x, HMD_y, HMD_z, " +
                                 "RCHit_x, RCHit_y, RCHit_z");
            }
        }
        if (!File.Exists(filePath2))
        {
            using (StreamWriter writer = new StreamWriter(filePath2, true))
            {
                writer.WriteLine("Frame, Timestamp, Product/AOI, Section/Shelf, EyeLeftPosition_x, " +
                                 "EyeLeftPosition_y, EyeLeftPosition_z, EyeLeftRotation_x, EyeLeftRotation_y, " +
                                 "EyeLeftRotation_z, EyeRightPosition_x, EyeRightPosition_y, EyeRightPosition_z," +
                                 "EyeRightRotation_x, EyeRightRotation_y, EyeRightRotation_z, HMD_x, HMD_y, HMD_z, " +
                                 "RCHit_x, RCHit_y, RCHit_z");
            }
        }
    }
    
    /// <summary>
    /// Saves remaining data when the object is destroyed.
    /// </summary>
    void OnDestroy()
    {
        SaveData(); // Save remaining data on destruction
    }

    /// <summary>
    /// Adds a hit point to the list for serialization.
    /// </summary>
    /// <param name="hitpoint">The hit point to add.</param>
    public void AddHitPoint(Vector3 hitpoint)
    {
        _hitPoints.Add(hitpoint);
    } 

    /// <summary>
    /// Serializes the hit points to a JSON file on application quit.
    /// </summary>
    private void OnApplicationQuit()
    {
        Vector3SerializationHelper.SerializeVector3List(_hitPoints, $"ETPlayerData{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.json");
    }
    
    /// <summary>
    /// Subscribes to the DirectoryReady event when the script is enabled.
    /// </summary>
    void OnEnable()
    {
        DirectoryManager.OnDirectoryReady += HandleDirectoryReady;
    }

    /// <summary>
    /// Handles the event when the directory is ready by setting up the file paths and starting the batch save coroutine.
    /// </summary>
    private void HandleDirectoryReady()
    {
        string path = directoryManager.sessionPath;
        string sceneName = SceneManager.GetActiveScene().name;

        string fileName = "EyeTrackerData-AOI" + sceneName + ".csv";
        string fileName2 = "EyeTrackerData-Products" + sceneName + ".csv";
        
        filePath = Path.Combine(path, fileName);
        filePath2 = Path.Combine(path, fileName2);
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
