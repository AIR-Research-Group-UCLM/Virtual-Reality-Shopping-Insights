using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages the creation of directories for user sessions and handles initialization events.
/// </summary>
public class DirectoryManager : MonoBehaviour
{
    /// <summary>
    /// Event triggered when the directory is ready.
    /// </summary>
    public static event Action OnDirectoryReady;

    /// <summary>
    /// Singleton instance of the DirectoryManager.
    /// </summary>
    public static DirectoryManager Instance { get; private set; }

    /// <summary>
    /// Username of the logged-in user.
    /// </summary>
    public string Username = null;

    /// <summary>
    /// Path to the current session directory.
    /// </summary>
    public string sessionPath = null;

    private string _userDirectoryPath = null;
    private string _sessionDirectoryName = null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        CreateDirectories();
    }

    /// <summary>
    /// Creates directories for the user and session.
    /// </summary>
    void CreateDirectories()
    {
        if (string.IsNullOrEmpty(Username))
        {
            Debug.LogError("User name is not set!");
            return;
        }

        // Create user directory if it does not exist
        _userDirectoryPath = Path.Combine(Application.persistentDataPath, Username);
        if (!Directory.Exists(_userDirectoryPath))
        {
            Directory.CreateDirectory(_userDirectoryPath);
            Debug.Log($"User directory created at: {_userDirectoryPath}");
        }
        else
        {
            Debug.Log($"User directory already exists at: {_userDirectoryPath}");
        }

        // Create session directory
        DateTime now = DateTime.Now;
        _sessionDirectoryName = "SESSION_" + now.ToString("yyyy-MM-dd_HH-mm-ss");
        sessionPath = Path.Combine(_userDirectoryPath, _sessionDirectoryName);
        Directory.CreateDirectory(sessionPath);
        Debug.Log($"Session directory created at: {sessionPath}");

        // Trigger the event indicating that the directory is ready
        Debug.Log("OnDirectoryReady event fired");
        OnDirectoryReady?.Invoke();
    }
}
