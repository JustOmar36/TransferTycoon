using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    public AudioClip clickSound;     // The sound clip to play when the button is clicked
    private AudioSource audioSource; // The AudioSource used to play the sound
    private Button button;           // Reference to this button component
    public float volume = 0.1f;

    void Awake()
    {
        // Try to get an existing AudioSource; add one if it doesn’t exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Basic setup for UI (2D) sound playback
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;  // 0 = 2D sound (not affected by distance)
        audioSource.volume = 1f;      // Default volume
    }

    void OnEnable()
    {
        // When the button becomes active (enabled), make sure the sound event is (re)bound.
        // This ensures it still works even if the button is created or shown later in runtime.
        button = GetComponent<Button>();
        if (button != null)
        {
            // Remove any existing listeners to avoid duplicates
            button.onClick.RemoveListener(PlayClickSound);

            // Add our new listener
            button.onClick.AddListener(PlayClickSound);
        }
    }

    // Function that actually plays the click sound
    void PlayClickSound()
    {
        if (clickSound != null)
        {
            // Play the sound once at the current AudioSource
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
}
