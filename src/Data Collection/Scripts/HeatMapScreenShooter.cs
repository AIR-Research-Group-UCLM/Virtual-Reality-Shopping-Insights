using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Captures screenshots from a list of cameras and saves them as PNG files.
/// </summary>
public class HeatMapScreenShooter : MonoBehaviour
{
    /// <summary>
    /// List of game objects containing cameras.
    /// </summary>
    public List<GameObject> cameras;

    /// <summary>
    /// Retrieves the Camera components from the list of game objects.
    /// </summary>
    /// <returns>List of Camera components.</returns>
    public List<Camera> GetCameras()
    {
        List<Camera> camerasComponents = new List<Camera>();
        foreach (var go in cameras)
        {
            Debug.Log(go.GetComponent<Camera>().gameObject.name);
            camerasComponents.Add(go.GetComponent<Camera>());
        }

        return camerasComponents;
    }

    /// <summary>
    /// Captures screenshots from the cameras in the list.
    /// </summary>
    public void CaptureCameras()
    {
        if (cameras == null || cameras.Count == 0)
        {
            Debug.LogError("No cameras set to capture.");
            return;
        }
        
        List<Camera> camerasComponents = GetCameras();

        foreach (Camera camera in camerasComponents)
        {
            if (camera != null)
            {
                Debug.Log("Capturing camera: " + camera.gameObject.name + "...");
                CaptureCamera(camera, camerasComponents.IndexOf(camera));
            }
        }
    }
    
    /// <summary>
    /// Captures a screenshot from a single camera.
    /// </summary>
    /// <param name="cam">The camera to capture from.</param>
    /// <param name="index">The index of the camera in the list.</param>
    private void CaptureCamera(Camera cam, int index)
    {
        RenderTexture renderTexture = new RenderTexture(2048, 2048, 24);
        cam.targetTexture = renderTexture;
        Texture2D screenShot = new Texture2D(2048, 2048, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);
        cam.targetTexture = null;
        RenderTexture.active = null; // Added to avoid errors
        DestroyImmediate(renderTexture);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(2048, 2048, index);
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
    }

    /// <summary>
    /// Generates a filename for the screenshot.
    /// </summary>
    /// <param name="width">The width of the screenshot.</param>
    /// <param name="height">The height of the screenshot.</param>
    /// <param name="index">The index of the camera in the list.</param>
    /// <returns>The generated filename.</returns>
    private string ScreenShotName(int width, int height, int index)
    {
        if (!System.IO.Directory.Exists(Application.dataPath + "/Screenshots"))
            System.IO.Directory.CreateDirectory(Application.dataPath + "/Screenshots");
        
        return string.Format("{0}/Screenshots/screen_{1}x{2}_{3}.png", 
            Application.dataPath, 
            width, height, 
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + index);
    }
}
