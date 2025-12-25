using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// For preserving methods called via Invoke
using UnityEngine.Scripting;

public class PCScript : MonoBehaviour
{
    private enum TextAddTypes
    {
        Immediate,
        Slowly
    }

    private enum Panel
    {
        Vitals,
        Labs,
        Imaging
    }
    
    //time (in s) between each letter appearing
    //this is the default value here, edit this in scene to edit the scene value
    public double speakingDelay = 0.45;
    
    [Header("Connections")] //holds all connections to other gameobjects in the scene. If you create a new scene with this script all these need to be connected in the scene
    public GameObject canvas;
    public GameObject textContainer;
    public GameObject patientInfo;
    public GameObject patientInfoButton;
    public GameObject vitalsButton;
    public GameObject labsButton;
    public GameObject imagingButton;
    public GameObject vitals;
    public GameObject labs;
    public GameObject imaging;
    public GameObject patientName;
    public GameObject labsInfo;
    public GameObject endScreen;
    public GameObject backend;
    
    private TextMeshProUGUI _textMesh;
    private TextMeshProUGUI _patientTextMesh;
    private GameBackend _gameBackend;
    private bool TextFinished = true;

    //holds all the text that still needs to be added to the screen
    private List<(string, TextAddTypes)> _textToAdd =  new List<(string, TextAddTypes)>();
    private Panel _activePanel = Panel.Vitals;

    //used to display time in the upper right corner. The backend is tracking time independently
    private double _timer = 0.0;

    //boolean for tracking if the player is connected to the OSH or not
    private bool _connected = false;
    public bool Connected => _connected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        _gameBackend = backend.GetComponent<GameBackend>();

        // For WebGL builds, we need to wait for the backend to be ready
        yield return new WaitUntil(() => _gameBackend.IsReady);

        Debug.Log("Backend is ready! Starting scenario.");


        _textMesh = textContainer.GetComponent<TextMeshProUGUI>();
        _patientTextMesh = patientName.GetComponent<TextMeshProUGUI>();
        
        ClearText();

        
        _gameBackend.StartScenario(0);
        for (int i = 0; i < _gameBackend.visibleElements.Count; i++)
        {
            if (_gameBackend.visibleElements[i].Category == "Opening")
            {
                AddText(_gameBackend.visibleElements[i].Answer[0], _gameBackend.visibleElements[i].Answer[2]);
            }
        }
        
    }

    void HandleScenariosLoaded()
    {
        // Always unsubscribe from events!
        _gameBackend.OnScenariosLoaded -= HandleScenariosLoaded;
        Debug.Log("Scenarios downloaded. Starting game.");
        InitializeGame();
    }

    void InitializeGame()
    {
        // NOW it is safe to ask for data
        _gameBackend.StartScenario(0);
    }

    // Update is called once per frame
    void Update()
    //handles adding text slowly by only calling AddNext() if enough time has passed
    {
        _timer += Time.deltaTime;
        if (_timer >= speakingDelay)
        {
            _timer -= speakingDelay;
            AddNext();
        }
    }
    
    //effectively turns the in game computer on and off by enabiling/disabling the canvas that everything on the screen is children of
    public void TogglePower()
    {
        canvas.SetActive(!canvas.activeSelf);
    }
    
    #region player input

    [Preserve]
    void Connect()
    {
        _connected = true;
        Debug.Log("Connected to OSH");
    }
    
    //called by PCPlayerInput whenever the player says something
    public void ParseInput(string input)
    {
        //First, add what the player is saying
        AddText("Learner", input);
        
        var found = false;
        if (!_connected)
        {
            //search through the elements for one that matches the input
            foreach (var element in _gameBackend.visibleElements)
            {
                //if the element's response and answer aren't formatted properly continue to the next one
                if (element.LearnerResponse.Count != 3 || element.Answer.Count != 3)
                {
                    continue;
                }
                //we only consider things directed at the transfer center if the player hasn't connected yet
                if (element.LearnerResponse[2].Equals(input) && element.Category.Equals("TransferCenter"))
                {
                    found = true;
                    AddText(element.Answer[0], element.Answer[2]);
                    _gameBackend.VisitElement(element); //for the backend's ability to track player input
                    Debug.Log("Visited element: " + element.Category);
                    if (element.Function.IsUnityNull() || element.Function.Count == 0)
                    {
                        break;
                    }
                    foreach (var function in element.Function)
                    {
                        Debug.Log("Invoking function: " + function);
                        Invoke(function, 0);
                    }
                    break;
                }
            }

            if (found == false)
            {
                AddText("TransferCenter", "That's something you should ask the OSH");
            }
        }
        else
        {
            foreach (var element in _gameBackend.visibleElements)
            {
                if (element.LearnerResponse.Count != 3 || element.Answer.Count != 3)
                {
                    continue;
                }
                if (element.LearnerResponse[2].Equals(input) && !element.Category.Equals("TransferCenter"))
                {
                    found = true;
                    AddText(element.Answer[0], element.Answer[2]);
                    _gameBackend.VisitElement(element);
                    Debug.Log("Visited element: " + element.Category);
                    if (element.Function.IsUnityNull() || element.Function.Count == 0)
                    {
                        break;
                    }
                    foreach (var function in element.Function)
                    {
                        Debug.Log("Invoking function: " + function);
                        Invoke(function, 0);
                    }
                    break;
                }
            }

            if (found == false)
            {
                // THIS SHOULD BE AN ERROR AND SHOULD NEVER HAPPEN?!
                Debug.LogError("No matching element found for input: " + input);
                AddText("OSH", "I'm not sure what you mean.");
            }
        }
        
    }

    [Preserve]
    public void EndScenario()
    {
        Debug.Log("End Scenario Invoked");
        _gameBackend.EndScenarioAndRecordScores();
        _gameBackend.EndSessionAndSave();
        Debug.Log("End Game Invoked");
        endScreen.SetActive(true);
        Debug.Log("End Screen Activated");
        var questionScore = _gameBackend.currentScenarioSummary.PresentIllnessHistoryEarned +
                            _gameBackend.currentScenarioSummary.ElicitedHistoryEarned +
                            _gameBackend.currentScenarioSummary.InterventionsEarned;
        var decisionScore = _gameBackend.currentScenarioSummary.ActionsEarned;
        var timeScore = _gameBackend.currentScenarioSummary.TimingPointsEarned;
        Debug.Log("Scores Calculated"); 
        endScreen.GetComponent<ResultsScript>().UpdateResults(questionScore, decisionScore, timeScore);

        AudioUIManager.Instance.PlayScorePageBGM();
    }
    
    #endregion player input
    
    #region inputFunctions
    // all functions that are called as a result of function calls in scenario elements
    
    //ends the tutorial (Do not call if not in the tutorial!)
    public void FinishTutorial()
    {
        _connected = false;
        _gameBackend.StartScenario(2);
        for (int i = 0; i < _gameBackend.visibleElements.Count; i++)
        {
            if (_gameBackend.visibleElements[i].Category == "Opening")
            {
                AddText(_gameBackend.visibleElements[i].Answer[0], _gameBackend.visibleElements[i].Answer[2]);
            }
        }
    }
    
    #endregion inputFunctions
    
    #region text
    //all functions having to do with actually adding text to the screen. 
    
    //adds the next piece of text. This is one symbol unless it needs to be added instantly
    private void AddNext()
    {
        // Remove all empty entries at the front of the list
        while (_textToAdd.Count > 0 && string.IsNullOrEmpty(_textToAdd[0].Item1))
        {
            _textToAdd.RemoveAt(0);
        }

        // If list is empty after cleanup, we know all text is done
        if (_textToAdd.Count == 0)
        {
            // Only trigger sound effect once when text just finished
            if (!TextFinished)
            {
                TextFinished = true;
                AudioUIManager.Instance.PlayDialogueReminder(0.2f);
            }
            return;
        }
        TextFinished = false;

        var result = AddLetter(_textToAdd[0].Item1, _textToAdd[0].Item2);
        switch (result.Item2)
        {
            case TextAddTypes.Immediate:
                _textToAdd.RemoveAt(0);
                AddNext();
                break;
            case TextAddTypes.Slowly:
                _textToAdd[0] = (result.Item1, TextAddTypes.Slowly);
                break;
        }
    }

    private (string, TextAddTypes) AddLetter(string text, TextAddTypes speed)
    {
        switch (speed)
        {
            case TextAddTypes.Immediate:
                _textMesh.text += text;
                return (text, TextAddTypes.Immediate);
            case TextAddTypes.Slowly:
                _textMesh.text += text.Substring(0,1);
                return (text.Remove(0,1), TextAddTypes.Slowly);
        }
        return (text, speed);
    }

    public void AddText(string speaker, string saying)
    {
        TextFinished = false;

        if (String.Compare(speaker, "Learner", StringComparison.Ordinal) == 0)
        {
            _textToAdd.Add(("<size=35>\n \n</size>", TextAddTypes.Immediate)); 
            _textToAdd.Add(("<color=#96DFFF><size=35><b><align=right>" + speaker + "</align></b></size></color>",  TextAddTypes.Immediate));
            _textToAdd.Add(("\n",  TextAddTypes.Slowly));
            _textToAdd.Add(("<align=right>" + saying + "</align>", TextAddTypes.Immediate));
        }
        else
        {
            _textToAdd.Add(("<size=35>\n \n</size>", TextAddTypes.Immediate)); 
            if (!string.IsNullOrEmpty(speaker))
            {
                _textToAdd.Add(("<color=#96DFFF><size=35><b>" + speaker + "</b></size></color>",  TextAddTypes.Immediate));
                _textToAdd.Add(("\n", TextAddTypes.Slowly)); 
            }

            _textToAdd.Add((saying, TextAddTypes.Slowly));
            _textToAdd.Add(("     ", TextAddTypes.Slowly));
        }
    }
    
    public void ClearText()
    {
        _textMesh.text = "";
        _textToAdd.Clear();
    }
    #endregion text
    
    #region patientInfo
    //functions for handling the player navigating the patent info section of the UI
    
    public void TogglePatientInfo()
    {
        patientInfo.SetActive(!patientInfo.activeSelf);
        
        vitals.SetActive(true);
        labs.SetActive(true);
        imaging.SetActive(true);
        if (_activePanel != Panel.Vitals) vitals.SetActive(false);
        if (_activePanel != Panel.Labs) labs.SetActive(false);
        if (_activePanel != Panel.Imaging) imaging.SetActive(false);
        
        if (patientInfo.activeSelf)
        {
            patientInfoButton.GetComponent<Button>().image.color = new Color(1, .4f, .4f);
        }
        else
        {
            patientInfoButton.GetComponent<Button>().image.color = new Color(0, 0.3f, 0.5f);
        }
    }
    
    private void ChangePanel(Panel panel)
    {
        vitals.SetActive(false);
        labs.SetActive(false);
        imaging.SetActive(false);
        vitalsButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
        labsButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
        imagingButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
        switch (panel)
        {
            case Panel.Vitals:
                vitals.SetActive(true);
                vitalsButton.GetComponent<Button>().image.color = new Color(1, 1, 1);
                break;
            case Panel.Labs:
                labs.SetActive(true);
                labsButton.GetComponent<Button>().image.color = new Color(1, 1, 1);
                break;
            case Panel.Imaging:
                imaging.SetActive(true);
                imagingButton.GetComponent<Button>().image.color = new Color(1, 1, 1);
                break;
        }
    }

    public void SwitchToLabs()
    {
        ChangePanel(Panel.Labs);
    }
    public void SwitchToVitals()
    {
        ChangePanel(Panel.Vitals);
    }
    public void SwitchToImaging()
    {
        ChangePanel(Panel.Imaging);
    }
    #endregion patientInfo
    
    
    
}
