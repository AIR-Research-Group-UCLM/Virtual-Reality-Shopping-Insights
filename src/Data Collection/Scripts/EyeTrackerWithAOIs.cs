using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

/// <summary>
/// Tracks and records eye gaze data with Areas of Interest (AOIs).
/// </summary>
public class EyeTrackerWithAOIs : MonoBehaviour
{
    /// <summary>
    /// Maximum distance for the raycast.
    /// </summary>
    [SerializeField]
    private float rayDistance = 4.0f;
    
    /// <summary>
    /// Raycast hits array.
    /// </summary>
    private RaycastHit[] _hits;

    /// <summary>
    /// Layer mask for filtering raycast hits.
    /// </summary>
    [SerializeField]
    private LayerMask layersToInclude;
    
    /// <summary>
    /// GameObject representing the left eye gaze.
    /// </summary>
    [SerializeField]
    private GameObject _eyeGazeLeft;

    /// <summary>
    /// GameObject representing the right eye gaze.
    /// </summary>
    [SerializeField]
    private GameObject _eyeGazeRight;

    /// <summary>
    /// Reference to the EyeTrackerDataManager for managing eye tracking data.
    /// </summary>
    [SerializeField] private EyeTrackerDataManager etDataManager;
    
    /// <summary>
    /// Reference to the OVRCameraRig for head tracking.
    /// </summary>
    [SerializeField]
    private OVRCameraRig hmd;

    /// <summary>
    /// List to store data items for AOIs.
    /// </summary>
    private List<string> _dataItemsETAOI = new List<string>();

    /// <summary>
    /// List to store data items for products.
    /// </summary>
    private List<string> _dataItemsETProduct = new List<string>();

    /// <summary>
    /// Timestamp for recording data.
    /// </summary>
    private float _timestamp;

    void FixedUpdate()
    {
        Vector3 raycastDirection = transform.TransformDirection(Vector3.forward) * rayDistance;
        Ray ray = new Ray(transform.parent.position, raycastDirection);
        _hits = Physics.RaycastAll(ray, rayDistance, layersToInclude);
        
        Debug.DrawRay(transform.parent.position, raycastDirection, Color.green);
        
        Vector3 headPosition = hmd.centerEyeAnchor.position;
        
        foreach (RaycastHit hit in _hits)
        {
            Vector3 hitPoint = hit.point;
            etDataManager.AddHitPoint(hitPoint);
            string data = $"{Time.frameCount}, " +
                          $"{Time.time.ToString("f4", CultureInfo.InvariantCulture)}, {hit.collider.gameObject.name}, " +
                          $"{hit.collider.gameObject.transform.parent.name}, " +
                          $"{_eyeGazeLeft.transform.position.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeLeft.transform.position.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeLeft.transform.position.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeLeft.transform.rotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeLeft.transform.rotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeLeft.transform.rotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeRight.transform.position.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeRight.transform.position.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeRight.transform.position.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeRight.transform.rotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeRight.transform.rotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{_eyeGazeLeft.transform.rotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{headPosition.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{headPosition.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{headPosition.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{hitPoint.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{hitPoint.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                          $"{hitPoint.z.ToString("f4", CultureInfo.InvariantCulture)}";

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("AOI"))
            {
                _dataItemsETAOI.Add(data);
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Raycastable"))
            {
                _dataItemsETProduct.Add(data);
            }
        }

        foreach (string dataItem in _dataItemsETAOI)
        {
            etDataManager.AddCollisionData(dataItem, "AOI");
        }
        foreach (string dataItem in _dataItemsETProduct)
        {
            etDataManager.AddCollisionData(dataItem, "Products");
        }
        
        _dataItemsETAOI.Clear();
        _dataItemsETProduct.Clear();
    }
}
