using UnityEngine;
using UnityEngine.UI;

public class ConfirmButtonAction : MonoBehaviour
{
    public AudioClip clickSound;        // The sound played when clicking
    public GameObject inputPanel;       // Reference to the InputPanel (to hide it later)
    public NoteInputPanel noteManager;  // Reference to your NoteInputPanel script
    private AudioSource audioSource;    // The AudioSource used for playback

    void Start()
    {
        // Add AudioSource if missing
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;

        // Add button click listener
        GetComponent<Button>().onClick.AddListener(OnConfirmClick);
    }

    void OnConfirmClick()
    {
        // 1?? Play sound
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // 2?? Call the note creation logic
        if (noteManager != null)
        {
            noteManager.OnConfirmClick();
        }

        // 3?? Hide the panel
        if (inputPanel != null)
        {
            inputPanel.SetActive(false);
        }
    }
}
