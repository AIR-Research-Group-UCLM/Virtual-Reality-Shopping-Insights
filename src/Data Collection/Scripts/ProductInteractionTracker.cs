using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// Tracks and records product interaction data.
/// </summary>
public class ProductInteractionTracker : MonoBehaviour
{
    /// <summary>
    /// Indicates whether the product is currently selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Dictionary to store interaction start times.
    /// </summary>
    private Dictionary<GameObject, float> interactions = new Dictionary<GameObject, float>();

    /// <summary>
    /// Manages the product interaction data.
    /// </summary>
    private ProductInteractionManager dataManager;

    /// <summary>
    /// Represents the type of interaction.
    /// </summary>
    private IInteractable interactionType;

    /// <summary>
    /// Section of the product.
    /// </summary>
    private string _section;

    /// <summary>
    /// Data string for the current interaction.
    /// </summary>
    private string data;

    /// <summary>
    /// Timer to track intervals for data recording.
    /// </summary>
    private float _timer = 0.0f;

    /// <summary>
    /// Interval for recording data (in seconds).
    /// </summary>
    private float _interval = 0.03f;
    
    void Start()
    {
        interactionType = GetComponent<IInteractable>();
        dataManager = FindObjectOfType<ProductInteractionManager>();
        Transform parent = this.transform.parent;
        _section = parent.gameObject.name;
    }

    void Update()
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        InteractableState state = interactionType.State;
        _timer += Time.deltaTime;
        if(IsSelected && _timer >= _interval)
        {
            if (!interactions.ContainsKey(gameObject))
                interactions[gameObject] = Time.time;
            
            if (interactionType == null)
            {
                
                data = $"{Time.frameCount}, " +
                       $"{Time.time.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{gameObject.name}, {_section}, {state.ToString()}, " +
                       $"{transform.position.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.position.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.position.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{rotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{rotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{rotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.localScale.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.localScale.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.localScale.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{dataManager.rightHand.State.ToString()}, {dataManager.leftHand.State.ToString()}";
            }
            else
            {
                data = $"{Time.frameCount}, " +
                       $"{Time.time.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{gameObject.name}, {_section}, {state.ToString()}, " +
                       $"{transform.position.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.position.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.position.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{rotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{rotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{rotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.localScale.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.localScale.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.localScale.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{dataManager.rightHand.State.ToString()}, {dataManager.leftHand.State.ToString()}";
            }
            
            dataManager.AddProductInteractionData(data);
            _timer = 0.0f;
        }
        else if (_timer >= _interval)
        {
            if (interactions.ContainsKey(gameObject))
            {
                interactions.Remove(gameObject);
                data = $"{Time.frameCount.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{Time.time.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{gameObject.name}, {_section}, {state.ToString()}, " +
                       $"{transform.position.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.position.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.position.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{rotation.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{rotation.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{rotation.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.localScale.x.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.localScale.y.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{transform.localScale.z.ToString("f4", CultureInfo.InvariantCulture)}, " +
                       $"{dataManager.rightHand.State.ToString()}, {dataManager.leftHand.State.ToString()}";
                dataManager.AddProductInteractionData(data);
            }

            _timer = 0.0f;
        }
    }

}
