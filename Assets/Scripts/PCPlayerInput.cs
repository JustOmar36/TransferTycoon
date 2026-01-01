using System;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PCPlayerInput : MonoBehaviour
{
    public GameObject topicSelector;
    public GameObject questionInput;
    public GameObject pc;
    //list of all the buttons that are used for player input, each is an option for what to say based on the keyword
    public GameObject[] options;
    
    public GameObject scenarioManager;
    private GameBackend _backend;
    
    private PCScript _pcScript;
    //boolean for tracking if the player is currently interacting with the input text box
    private bool _interacting = false;


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
        if (_interacting == true && questionInput.GetComponent<TMP_InputField>().text != "")
        {
            ShowOptions();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) || _interacting == false || questionInput.GetComponent<TMP_InputField>().text == "")
        {
            DisableOptions();
        }
    }

    // called whenever the player starts interacting with the text input
    public void StartInteraction()
    {
        _interacting = true;
    }

    //called whenever the player stops interacting with the text input
    public void StopInteraction()
    {
        _interacting = false;
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
        foreach (var option in options)
        {
            option.GetComponentInChildren<TextMeshProUGUI>().text = "";
            option.SetActive(false);
        }
    }

    //Shows and updates all options for player input
    public void ShowOptions()
    {
        Disable();
        string topic = "";
        var currentOption = 0; 
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
                        options[currentOption].GetComponentInChildren<TextMeshProUGUI>().text = _backend.visibleElements[i].LearnerResponse[2];
                        options[currentOption].SetActive(true);
                        currentOption++; 
                        break;
                    }
            }
        };
        {
            if (currentOption >= options.Length) return;
        }
    }

    //used to read what is in a given option
    //This function is written really poorly, idk what I was doing when I wrote it
    public void CheckDropdownValue(string option)
    {
        string output = "";
        
        switch (option)
        {
            case "Option1":
            {
                output = options[0].GetComponentInChildren<TextMeshProUGUI>().text;
                break;
            }
            case "Option2":
            {
                output = options[1].GetComponentInChildren<TextMeshProUGUI>().text;
                break;
            }
            case "Option3":
            {
                output = options[2].GetComponentInChildren<TextMeshProUGUI>().text;
                break;
            }
            case "Option4":
            {
                output = options[3].GetComponentInChildren<TextMeshProUGUI>().text;
                break;
            }
        }
        _pcScript.ParseInput(output);
        
    }
}
