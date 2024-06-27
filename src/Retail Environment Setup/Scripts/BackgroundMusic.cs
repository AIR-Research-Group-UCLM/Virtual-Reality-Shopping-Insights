using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioClip musicClip;

    void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.Play();
    }
}
