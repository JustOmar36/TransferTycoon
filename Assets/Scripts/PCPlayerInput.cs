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
        char[] delimiters = new char[] { ' ', ',', '.', ';', ':', '!' }; // List of delimiters to split player input on
        float threshold = 0.4f; // Threshold of similarity to allow partial matching

        string rawInput = questionInput.GetComponent<TMP_InputField>().text.ToLower(); // Player input lower cased
        string[] inputWords = rawInput.Split(delimiters, StringSplitOptions.RemoveEmptyEntries); // Splut player input into separate words


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
        
        // loop for finding all elements in the scenario to be shown
        for (int i = 0; i < _backend.visibleElements.Count; i++)
        {
            // continue to the next loop if the current element is not the right topic
            if (!_backend.visibleElements[i].Category.Equals(topic))
            {
                continue;
            }

            bool isMatchFound = false;

            // checks to see if any of the matches of this element include the string the player typed
            foreach (string uWord in inputWords)
            {
                foreach (var match in _backend.visibleElements[i].Matches)
                {
                    string matchLower = match.ToLower();
                    float similarity = GetSimilarity(uWord, matchLower);

                    // Match if the word is contained within the keyword OR if it's a close typo
                    if (matchLower.Contains(uWord) || similarity > threshold)
                    {
                        CreateOption(_backend.visibleElements[i].LearnerResponse[2]);
                        isMatchFound = true;
                        break;
                    }
                }

                if (isMatchFound) break; // Stop looking through input words once we've found a valid match
            }
        }
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

    // Helper function to check similarity of a word to avoid constant one to one matches between player input and JSON matches
    public static float GetSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0;
        if (source == target) return 1.0f;

        int n = source.Length;
        int m = target.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 0; j <= m; d[0, j] = j++) ;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        float maxLen = Mathf.Max(n, m);
        return 1.0f - (d[n, m] / maxLen);
    }

    // Helper Function to create and push Player Options Prefab
    private void CreateOption(string textValue)
    {
        GameObject option = Instantiate(OptionPrefab, ContentParent);
        TMP_Text optionText = option.GetComponentInChildren<TMP_Text>();
        optionText.text = textValue;
        option.SetActive(true);

        var btn = option.GetComponentInChildren<Button>(true);
        if (btn != null)
        {
            // Clear previous listeners to avoid double-firing if reusing objects
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => CheckDropdownValue(textValue));
        }
    }
}
