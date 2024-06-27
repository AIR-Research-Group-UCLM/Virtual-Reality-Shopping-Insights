using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Oculus.Interaction.HandGrab;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages tracking and saving of product interaction data.
/// </summary>
public class ProductInteractionManager : MonoBehaviour
{
    /// <summary>
    /// List to store batch data before saving.
    /// </summary>
    private List<string> batchData = new List<string>();

    /// <summary>
    /// Timer interval for batch saving in seconds.
    /// </summary>
    private float batchTimer = 8.0f;

    /// <summary>
    /// File path for saving the product interaction data.
    /// </summary>
    private string filePath, distantFilePath;

    /// <summary>
    /// Indicates whether the CSV header has been written.
    /// </summary>
    private static bool headerWritten = false;

    /// <summary>
    /// Indicates whether accessibility features are enabled.
    /// </summary>
    public bool isAccesibility = false;

    /// <summary>
    /// Reference to the DirectoryManager for managing file paths.
    /// </summary>
    [SerializeField] private DirectoryManager directoryManager;

    /// <summary>
    /// Reference to the right hand grab interactor.
    /// </summary>
    public HandGrabInteractor rightHand;

    /// <summary>
    /// Reference to the left hand grab interactor.
    /// </summary>
    public HandGrabInteractor leftHand;

    /// <summary>
    /// Adds product interaction data to the batch list.
    /// </summary>
    /// <param name="data">The product interaction data to add.</param>
    public void AddProductInteractionData(string data)
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
    /// Saves distant grab interaction data to a separate file.
    /// </summary>
    /// <param name="data">The distant grab interaction data to save.</param>
    public void SaveDistantGrabData(string data)
    {
        using (StreamWriter writer = new StreamWriter(distantFilePath, true))
        {
            writer.WriteLine(data);
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
                writer.WriteLine("Frame, Timestamp, Object, Section, State, Position_x, Position_y, Position_z, Rotation_x, " +
                                 "Rotation_y, Rotation_z, Scale_x, Scale_y, Scale_z, RightHand_State, LeftHand_State");
            }

            if (isAccesibility)
            {
                using (StreamWriter writer = new StreamWriter(distantFilePath, true))
                {
                    writer.WriteLine("Frame, Timestamp, Object, State, Handedness");
                }
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

        string fileName = "ProductInteractionData" + sceneName + ".csv";
        filePath = Path.Combine(path, fileName);
        StartCoroutine(BatchSaveCoroutine());

        if (isAccesibility)
        {
            string fileName2 = "DistantGrabData" + sceneName + ".csv";
            distantFilePath = Path.Combine(path, fileName2);
        }
        
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
