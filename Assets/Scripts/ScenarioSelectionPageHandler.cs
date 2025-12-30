using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioSelectionPageHandler : MonoBehaviour
{

    [SerializeField] private GameBackend _gameBackend;

    [Header("UI")]
    [SerializeField] private Transform ContentParent;
    [SerializeField] private GameObject ScenarioButtonPrefab;

    private Dictionary<int, ScenarioFileData> scenarioFiles = new Dictionary<int, ScenarioFileData> ();

    private PCScript pcScript;


    private void Awake()
    {
        pcScript = FindFirstObjectByType<PCScript>();
        if (pcScript == null)
        {
            Debug.LogError("ScenarioSelectionPageHandler: PCScript not found in the scene during Awake. Check scene setup.");
        }
    }

    void Start()
    {
        scenarioFiles = _gameBackend.GetScenariosTable();
        PushScenarioButtons();
    }

    void PushScenarioButtons()
    {
        if (_gameBackend == null)
        {
            Debug.LogError("ScenarioSelectionPageHandler: _gameBackend is not assigned in the Inspector.");
            return;
        }

        foreach (var item in scenarioFiles)
        {
            GameObject button = Instantiate(ScenarioButtonPrefab, ContentParent, false);
            TMP_Text scenarioName = button.GetComponentInChildren<TMP_Text>();
            ScenarioButtonData uniqueScenarioID = button.GetComponent<ScenarioButtonData>();

            if (scenarioName != null)
            {
                scenarioName.text = item.Value.ScenarioName;
                Debug.Log($"Scenario selection button created for scenario with : {scenarioName.text}");
            }

            if (uniqueScenarioID != null)
            {
                uniqueScenarioID.SetItemID(item.Key);
                Debug.Log($"Scenario selection button created for scenario with ID: {uniqueScenarioID.GetItemID()}");

                // Add onClick listener to the Button component so clicking starts the scenario on the PCScript.
                var btn = button.GetComponentInChildren<Button>(true);
                if (btn != null && pcScript != null)
                {
                    // Capture the ID in a local variable to avoid closure issues
                    int scenarioId = item.Key;
                    btn.onClick.AddListener(() => pcScript.InitializeGame(scenarioId));
                    Debug.Log($"Added onClick listener to scenario button (ID: {scenarioId}) to call PCScript.InitializeGame.");
                }
                else
                {
                    if (btn == null) Debug.LogWarning($"Scenario button prefab is missing a Button component. Prefab: {ScenarioButtonPrefab.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Unique ID Button with name {scenarioName?.text} unable to set");
            }
        }
    }
}