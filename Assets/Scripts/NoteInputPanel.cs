using TMPro;  // Required for TextMeshPro UI elements
using UnityEngine;
using UnityEngine.Rendering;

public class NoteInputPanel : MonoBehaviour
{
    // Reference to the input field (where the user types text)
    public TMP_InputField inputField;

    // Reference to the prefab of the note that will be instantiated
    public GameObject notePrefab;

    // Reference to this input panel (used to show/hide it)
    public GameObject inputPanel;
    public GameObject deleteButton;
    public GameObject noteObj;

    public AudioSource audioSource;
    public AudioClip clickSound;
    public float volume = 0.1f;

    private void PlaySound()
    {
        if (audioSource && clickSound)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }

    // Called when the "Confirm" button is clicked
    public void OnConfirmClick()
    {
        // Get the text entered by the user
        string text = inputField.text;

        // Only proceed if the text is not empty
        if (!string.IsNullOrEmpty(text))
        {
            TextMeshPro tmp;
            if (noteObj == null)
            {
                // 1️⃣ Instantiate a new note object from the prefab
                GameObject note = Instantiate(notePrefab);
                // 2️⃣ Place the note 1 meter in front of the main camera
                note.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.4f;
                // 3️⃣ Find the TextMeshPro component inside the note prefab
                tmp = note.GetComponentInChildren<TextMeshPro>();
            }
            else
            {
                tmp = noteObj.GetComponentInChildren<TextMeshPro>();
            }

            // 4️⃣ Assign the text from the input field to the note’s text component
            if (tmp != null)
            {
                tmp.text = text;
            }

            // Output to the console for debugging purposes
            Debug.Log("Created note with text: " + text);
        }

        // 5️⃣ Hide the input panel and clear the input field
        inputPanel.SetActive(false);
        inputField.text = "";
        noteObj = null;

        PlaySound();
    }

    // Called when the "Cancel" button is clicked
    public void OnCancelClick()
    {
        // Simply hide the input panel and clear the input field
        inputPanel.SetActive(false);
        inputField.text = "";
        noteObj = null;

        PlaySound();
    }

    public void OnDeleteClick()
    {
        deleteButton.SetActive(false);
        inputPanel.SetActive(false);
        inputField.text = "";

        if (noteObj != null)
        {
            Destroy(noteObj);
        }
        noteObj = null;
        PlaySound();
    }
}
