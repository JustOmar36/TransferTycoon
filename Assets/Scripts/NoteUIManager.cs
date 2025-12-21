using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;  
public class NoteUIManager : MonoBehaviour
{
    public GameObject inputPanel;
    public GameObject deleteButton;

    
    public void OnAddNoteButtonClick()
    {
        inputPanel.SetActive(true);
        deleteButton.SetActive(false);
        Debug.Log("Button was clicked！");  
    }

    public void AddTextToNoteInputField(string text, GameObject nodeObj = null)
    {
        if (nodeObj != null)
        {
            deleteButton.SetActive(true);
        }
        else
        {
            deleteButton.SetActive(false);
        }

        // Find NoteInputField
        Transform inputFieldTransform = inputPanel.transform.Find("NoteInputField");
        NoteInputPanel input = inputPanel.GetComponentInChildren<NoteInputPanel>();
        input.noteObj = nodeObj;
        if (inputFieldTransform == null)
        {
            return;
        }

        // Get TextMeshPro InputField or TMP_Text
        TMP_InputField tmpInputField = inputFieldTransform.GetComponent<TMP_InputField>();
        if (tmpInputField != null)
        {
            tmpInputField.text = text;
            return;
        }

        // If it's TextMeshProUGUI type
        TMP_Text tmpText = inputFieldTransform.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = text;
            return;
        }
    }
}
