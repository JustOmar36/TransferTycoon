using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class ScenariosManagerTestUI : MonoBehaviour
{
    public GameBackend gameBackend; // Reference to GameBackend instance
    public TMP_Dropdown scenarioDropdown;
    public Button reloadButton;
    public TMP_Text configText;
    public TMP_Text contentText;

    private List<int> scenarioNumbers = new List<int>();

    void Start()
    {
        reloadButton.onClick.AddListener(ReloadAll);
        scenarioDropdown.onValueChanged.AddListener(delegate { ShowScenario(); });
        ReloadAll();
    }

    void ReloadAll()
    {
        scenarioNumbers = new List<int>(gameBackend.GetAvailableScenarioNumbers());
        scenarioNumbers.Sort();
        scenarioDropdown.ClearOptions();
        foreach (var n in scenarioNumbers)
            scenarioDropdown.options.Add(new TMP_Dropdown.OptionData("Scenario " + n));
        scenarioDropdown.RefreshShownValue();

        var config = gameBackend.GetConfig();
        configText.text = config != null
            ? $"Desc: {config.description}\nCount: {config.scenarioCount}"
            : "No Config";
        scenarioDropdown.value = 0;
        ShowScenario();
    }

    void ShowScenario()
    {
        if (scenarioNumbers.Count == 0)
        {
            contentText.text = "No scenarios loaded.";
            return;
        }
        int scenarioNum = scenarioNumbers[scenarioDropdown.value];
        ScenarioFileData scenario = gameBackend.GetScenario(scenarioNum);
        contentText.text = scenario != null ? FormatScenario(scenario) : "Scenario not loaded.";
    }

    string FormatScenario(ScenarioFileData scenario)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<b>Scenario Name:</b> {scenario.ScenarioName}\n");

        var allContents = new List<string>();

        if (scenario.Elements != null && scenario.Elements.Count > 0)
        {
            foreach (var el in scenario.Elements)
            {
                // Gather all possible "content" from the element

                // FIXED: Check if list exists AND has enough elements before accessing index 2
                if (el.LearnerResponse != null && el.LearnerResponse.Count > 2)
                {
                     if (!string.IsNullOrEmpty(el.LearnerResponse[2]))
                        allContents.Add($"Learner Response: {el.LearnerResponse[2]}");
                }

                // FIXED: Check if list exists AND has enough elements before accessing index 2
                if (el.Answer != null && el.Answer.Count > 2)
                {
                    if (!string.IsNullOrEmpty(el.Answer[2]))
                        allContents.Add($"Answer: {el.Answer[2]}");
                }

                if (el.Matches != null && el.Matches.Count > 0)
                    allContents.Add($"Matches: {string.Join(", ", el.Matches)}");

                if (el.Function != null && el.Function.Count > 0)
                    allContents.Add($"Functions: {string.Join(", ", el.Function)}");
            }
        }
        else
        {
            allContents.Add("No elements in this scenario.");
        }

        // Show all content, separated by double new lines for clarity
        sb.AppendLine("<b>All Scenario Element Contents:</b>\n");
        sb.AppendLine(string.Join("\n\n", allContents));

        return sb.ToString();
    }
}