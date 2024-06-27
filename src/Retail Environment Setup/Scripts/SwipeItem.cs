using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for managing swiping between items in Unity for feature customisation of products (for example, different colours).
/// </summary>
public class SwipeItem : MonoBehaviour
{
    /// <summary>
    /// List of GameObjects that can be swiped through.
    /// </summary>
    public List<GameObject> objectsSwiped;

    /// <summary>
    /// Changes the active item in the list to the next one.
    /// </summary>
    public void ChangeItem()
    {
        GameObject goingToBeActive;
        int indexObj = 0;

        // Iterate through the list of objects to find the currently active one.
        foreach (GameObject obj in objectsSwiped)
        {
            if (obj.activeSelf)
            {
                // Deactivate the currently active object.
                obj.SetActive(false);

                // Store the index of the currently active object.
                indexObj = objectsSwiped.IndexOf(obj);
                continue;
            }
        }

        // Calculate the next object's index and activate it.
        goingToBeActive = objectsSwiped[(indexObj + 1) % objectsSwiped.Count];
        goingToBeActive.SetActive(true);
    }
}