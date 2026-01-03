using System;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering.RenderGraphModule;

public class PCPlayerInput : MonoBehaviour
{
    public GameObject topicSelector;
    public GameObject questionInput;
    public GameObject pc;
    
    public GameObject scenarioManager;
    private GameBackend _backend;
    
    private PCScript _pcScript;

    [Header("Player Options")]
    public GameObject OptionPrefab;
    public Transform ContentParent;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        _backend = scenarioManager.GetComponent<GameBackend>();

        // 1. Wait here until IsReady becomes true.
        // This handles fast internet (0.1s) and slow internet (10s) automatically.
        yield return new WaitUntil(() => _backend.IsReady);
        
        _pcScript = pc.GetComponent<PCScript>();
        DisableOptions();
    }

    
    private void Update()
    {
        // passes only if we click on a non-option or if our input field is back to being empty
        if (Input.GetKeyDown(KeyCode.Mouse0) || questionInput.GetComponent<TMP_InputField>().text == "")
        {
            DisableOptions();
        }

    }
    
    //disables the buttons that show input options in order to hide them from the screen
    //invoke is used here for the time delay so that the text of the options can be read before they disable if the player selects one 
    public void DisableOptions()
    {
        Invoke(nameof(Disable), 0.2f);
        
    }

    //helper function for DisableOptions to do the actual disabling
    void Disable()
    {
        // Each Option is a unique prefab
        // Destroy any unnecessary prefabs to ensure to duplicates
        foreach (Transform child in ContentParent)
        {
            Destroy(child.gameObject);
        }
    }

    // Shows and updates all options for player input
    // Called on Panel-PlayerInput's Input Field (TMP) GameObject if the text field has changed
    public void ShowOptions()
    {
        Disable();
        string topic = "";
        //switch for determining what topic the player has selected so only things in that topic will be shown
        switch (topicSelector.GetComponent<TMP_Dropdown>().value)
        {
            case 0:
                topic = "TransferCenter";
                break;
            case 1:
                topic = "PresentIllnessHistory";
                break;
            case 2:
                topic = "ExamsLabsImaging";
                break;
            case 3:
                topic = "PriorInterventions";
                break;
            case 4:
                topic = "RecommendInterventions";
                break;
            case 5:
                topic = "Disposition";
                break;
            default: 
                break;
        }
        
        //loop for finding all elements in the scenario to be shown
        for (int i = 0; i < _backend.visibleElements.Count; i++)
        {
            //continue to the next loop if the current element is not the right topic
            if (!_backend.visibleElements[i].Category.Equals(topic))
            {
                continue;
            }

            //checks to see if any of the matches of this element include the string the player typed
            foreach (var matches in _backend.visibleElements[i].Matches) {
                if (matches.Contains(questionInput.GetComponent<TMP_InputField>().text))
                {
                    GameObject option = Instantiate(OptionPrefab, ContentParent);
                    TMP_Text optionText = option.GetComponentInChildren<TMP_Text>();
                    optionText.text = _backend.visibleElements[i].LearnerResponse[2];
                    option.SetActive(true);
                    
                    // Add Event Listenr to Invoke Check Drop Down Value function on each new Match Option Prefab
                    var btn = option.GetComponentInChildren<Button>(true);
                    if (btn != null)
                    {
                        btn.onClick.AddListener(() => CheckDropdownValue(optionText.text));
                        Debug.Log($"Added onClick listener to a Match option based on player Input");
                    }
                    else {
                        Debug.LogWarning($"Error finding Button Component for option: {optionText.text}");
                    }
                    break;
                }
            }
        };
    }

    // Function to call Parse Input on a valid Input Option
    public void CheckDropdownValue(string option)
    {
        if (option != null)
        {
            Debug.Log($"{option} Clicked. Parsing Option");
            questionInput.GetComponent<TMP_InputField>().text = "";
            _pcScript.ParseInput(option);
        }
        else {
            Debug.LogWarning("Check Drop Down Value Function called with Null Option");
        }

    }
}
