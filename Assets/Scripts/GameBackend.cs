using System;
using System.Collections; // Required for Coroutines
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking; // Required for Web Requests

public class GameBackend : MonoBehaviour
{
    // --- EVENTS & FLAGS ---
    public event Action OnScenariosLoaded;
    public bool IsReady { get; private set; } = false;

    private PCScript pcScript;

    // --- DATA STRUCTURES ---
    private List<ScenarioScoreSummary> scenarioSummaries = new List<ScenarioScoreSummary>();
    public ScenarioScoreSummary currentScenarioSummary;
    private ScenarioFileData activeScenario;
    private DateTime scenarioStartTime;
    private float sessionTotalTime = 0f;
    private string sessionStartTime;
    private string sessionJsonFilePath;

    private string scenariosFolder; // Desktop Path
    private string webScenariosUrl; // WebGL Path

    private Dictionary<int, ScenarioFileData> scenarioFiles = new Dictionary<int, ScenarioFileData>();
    private ScenariosConfig config;

#if !UNITY_WEBGL || UNITY_EDITOR
    private FileSystemWatcher watcher;
#endif

    public static string NetID { get; private set; }
    public static string Difficulty { get; private set; }

    // [System.NonSerialized] prevents the "Serialization depth limit" error
    [System.NonSerialized]
    public List<Element> visibleElements = new List<Element>();

    [System.NonSerialized]
    private List<Element> allScenarioElements = new List<Element>();

    public static void SetNetIDAndDifficulty(string netId, string difficulty)
    {
        NetID = netId;
        Difficulty = difficulty;
        Debug.Log($"GameBackend: NetID set to {NetID}, Difficulty set to {Difficulty}");
    }

    void Awake()
    {
        pcScript = FindFirstObjectByType<PCScript>();

        sessionStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

#if UNITY_WEBGL && !UNITY_EDITOR
        webScenariosUrl = Path.Combine(Application.streamingAssetsPath, "Scenarios");
        StartCoroutine(LoadAllScenariosWeb());
#else
        CreateScenariosFolder();
        LoadAllScenarios();
        MonitorFolder();

        // On Desktop, loading is instant, so we are ready immediately
        IsReady = true;
        OnScenariosLoaded?.Invoke();
#endif
    }

    void Start() { } // Empty. Do NOT put startup logic here.

    // --------------------------------------------------------------------------
    // WEBGL LOADING (Async)
    // --------------------------------------------------------------------------
#if UNITY_WEBGL && !UNITY_EDITOR
    IEnumerator LoadAllScenariosWeb()
    {
        string configUrl = Path.Combine(webScenariosUrl, "ScenariosConfig.json");
        using (UnityWebRequest www = UnityWebRequest.Get(configUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                config = JsonUtility.FromJson<ScenariosConfig>(www.downloadHandler.text);
                
                if (config != null && config.scenarioFiles != null)
                {
                    foreach (string fileName in config.scenarioFiles)
                    {
                        yield return StartCoroutine(LoadScenarioWeb(fileName));
                    }
                }
            }
            else
            {
                Debug.LogError($"WebGL Config Error: {www.error}");
            }
        }

        // --- CRITICAL FIX: Notify the game that loading is done ---
        IsReady = true;
        Debug.Log("WebGL: All Scenarios Loaded. Game is Ready.");
        OnScenariosLoaded?.Invoke();
    }

    IEnumerator LoadScenarioWeb(string fileName)
    {
        string fileUrl = Path.Combine(webScenariosUrl, fileName);
        using (UnityWebRequest www = UnityWebRequest.Get(fileUrl))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                string numStr = fileName.Substring(8, fileName.Length - 13);
                if (int.TryParse(numStr, out int scenarioNum))
                {
                    ScenarioFileData data = JsonUtility.FromJson<ScenarioFileData>(www.downloadHandler.text);
                    if (data != null) scenarioFiles[scenarioNum] = data;
                }
            }
        }
    }
#endif

    // --------------------------------------------------------------------------
    // DESKTOP LOADING (Sync)
    // --------------------------------------------------------------------------
    void CreateScenariosFolder()
    {
        scenariosFolder = Path.Combine(Application.dataPath, "../Scenarios");
#if !UNITY_WEBGL || UNITY_EDITOR
        if (!Directory.Exists(scenariosFolder)) Directory.CreateDirectory(scenariosFolder);
#endif
    }

    void LoadAllScenarios()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        string configPath = Path.Combine(scenariosFolder, "ScenariosConfig.json");
        if (File.Exists(configPath)) LoadConfigFile(configPath);

        if (Directory.Exists(scenariosFolder))
        {
            foreach (string file in Directory.GetFiles(scenariosFolder, "Scenario*.json"))
            {
                string fileName = Path.GetFileName(file);
                if (fileName != "ScenariosConfig.json") LoadScenarioFile(file);
            }
        }
#endif
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    void MonitorFolder()
    {
        watcher = new FileSystemWatcher(scenariosFolder, "*.json");
        watcher.Created += (s, e) => OnJsonFileChanged(e.FullPath);
        watcher.Changed += (s, e) => OnJsonFileChanged(e.FullPath);
        watcher.Deleted += (s, e) => OnJsonFileDeleted(e.Name);
        watcher.EnableRaisingEvents = true;
    }

    void OnJsonFileChanged(string path)
    {
        // Simple reload logic
        string fileName = Path.GetFileName(path);
        if (fileName == "ScenariosConfig.json") LoadConfigFile(path);
        else if (fileName.StartsWith("Scenario")) LoadScenarioFile(path);
    }

    void OnJsonFileDeleted(string fileName) { /* Keep your existing deletion logic */ }
#endif

    void LoadConfigFile(string path)
    {
        config = JsonUtility.FromJson<ScenariosConfig>(File.ReadAllText(path));
    }

    void LoadScenarioFile(string path)
    {
        string fileName = Path.GetFileName(path);
        string numStr = fileName.Substring(8, fileName.Length - 13);
        if (int.TryParse(numStr, out int scenarioNum))
        {
            scenarioFiles[scenarioNum] = JsonUtility.FromJson<ScenarioFileData>(File.ReadAllText(path));
        }
    }

    // ===================================================================================
    // SHARED GAMEPLAY LOGIC 
    // ===================================================================================

    public ScenariosConfig GetConfig()
    {
        Debug.Log("GetConfig called.");
        return config;
    }

    public ScenarioFileData GetScenario(int scenarioNum)
    {
        Debug.Log($"GetScenario called for Scenario {scenarioNum}");
        return scenarioFiles.ContainsKey(scenarioNum) ? scenarioFiles[scenarioNum] : null;
    }

    public List<int> GetAvailableScenarioNumbers()
    {
        Debug.Log("GetAvailableScenarioNumbers called.");
        return new List<int>(scenarioFiles.Keys);
    }

    public void StartScenario(int scenarioNum)
    {
        activeScenario = GetScenario(scenarioNum);
        if (activeScenario == null)
        {
            Debug.LogError($"Scenario {scenarioNum} not found! Is it loaded?");
            return;
        }

        currentScenarioSummary = new ScenarioScoreSummary { scenarioName = activeScenario.ScenarioName };
        scenarioStartTime = DateTime.Now;

        visibleElements.Clear();
        allScenarioElements.Clear();

        if (activeScenario.Elements != null)
        {
            foreach (var el in activeScenario.Elements)
            {
                visibleElements.Add(el);
                GatherElementTree(el, allScenarioElements);
            }
        }
    }

    // Recursively flattens tree for scoring: DFS
    private void GatherElementTree(Element element, List<Element> collector)
    {
        collector.Add(element);
        if (element.Child != null)
        {
            foreach (var child in element.Child)
            {
                GatherElementTree(child, collector);
                Debug.Log($"Gathered child element: {child.Category}");
            }
        }
    }

    private void CollectAllChildren(Element element, HashSet<Element> collector)
    {
        if (element.Child != null)
        {
            foreach (var child in element.Child)
            {
                if (!collector.Contains(child))
                {
                    collector.Add(child);
                    CollectAllChildren(child, collector);
                }
            }
        }
    }

    // Flat list traversal for scoring (allScenarioElements is already built)
    public void EndScenarioAndRecordScores()
    {
        // Add all visible elements first to allScenarioElements for scoring
        foreach (var element in visibleElements)
        {
            if (!allScenarioElements.Contains(element))
            {
                allScenarioElements.Add(element);
            }
        }

        float elapsed = (float)(DateTime.Now - scenarioStartTime).TotalSeconds;

        int piEarned = 0, piMax = 0;
        int ehEarned = 0, ehMax = 0;
        int interEarned = 0, interMax = 0;
        int actionsEarned = 0, actionsMax = 0;

        foreach (var el in allScenarioElements)
        {
            switch (el.Category)
            {
                case "PresentIllnessHistory":
                    piMax += el.Score;
                    if (el.Visited != null && el.Visited.Count > 0) piEarned += el.Score;
                    break;
                case "ElicitedHistory":
                    ehMax += el.Score;
                    if (el.Visited != null && el.Visited.Count > 0) ehEarned += el.Score;
                    break;
                case "Interventions":
                case "InterventionsAndRecommendations":
                    interMax += el.Score;
                    if (el.Visited != null && el.Visited.Count > 0) interEarned += el.Score;
                    break;
                case "Disposition":
                    actionsMax += el.Score;
                    if (el.Visited != null && el.Visited.Count > 0) actionsEarned += el.Score;
                    break;
            }
        }

        currentScenarioSummary.PresentIllnessHistoryEarned = piEarned;
        currentScenarioSummary.PresentIllnessHistoryMax = piMax;
        currentScenarioSummary.ElicitedHistoryEarned = ehEarned;
        currentScenarioSummary.ElicitedHistoryMax = ehMax;
        currentScenarioSummary.InterventionsEarned = interEarned;
        currentScenarioSummary.InterventionsMax = interMax;
        currentScenarioSummary.ActionsEarned = actionsEarned;
        currentScenarioSummary.ActionsMax = actionsMax;

        // Timing scoring using TimingPointsMap
        int timingPoints = 0;
        int timingPointsMax = 0;
        if (activeScenario != null && activeScenario.TimingPointsMap != null && activeScenario.TimingPointsMap.Count > 0)
        {
            timingPointsMax = activeScenario.TimingPointsMap[0].points; // Highest possible is the first in list
            foreach (var tp in activeScenario.TimingPointsMap)
            {
                if (elapsed <= tp.timeSeconds)
                {
                    timingPoints = tp.points;
                    break;
                }
            }
        }

        currentScenarioSummary.TimingPointsEarned = timingPoints;
        currentScenarioSummary.TimingPointsMax = timingPointsMax;
        currentScenarioSummary.TotalPoints = piEarned + ehEarned + interEarned + actionsEarned + timingPoints;
        currentScenarioSummary.TotalMax = piMax + ehMax + interMax + actionsMax + timingPointsMax;
        currentScenarioSummary.timeElapsedSeconds = elapsed;

        // Append all visible elements to the score summary (AFTER scoring is complete)
        currentScenarioSummary.VisibleElements.Clear(); // Clear any existing
        currentScenarioSummary.VisibleElements.AddRange(visibleElements);

        scenarioSummaries.Add(currentScenarioSummary);
        sessionTotalTime += elapsed;
    }

    public List<KeyValuePair<string, string>> GetVitalSigns()
    {
        var result = new List<KeyValuePair<string, string>>();

        // Regex pattern to capture keys and values (string or number) from flat JSON
        string pattern = "\\\"(?<key>[^\\\"]+)\\\"\\s*:\\s*(?:\\\"(?<sVal>[^\\\"]+)\\\"|(?<nVal>[^,}\\s]+))";

        foreach (var el in visibleElements)
        {
            if (el.Category == "VitalSigns")
            {
                if (el.Answer != null && el.Answer.Count >= 3)
                {
                    string json = el.Answer[2];
                    MatchCollection matches = Regex.Matches(json, pattern);

                    foreach (Match m in matches)
                    {
                        string key = m.Groups["key"].Value;
                        string value = m.Groups["sVal"].Success ? m.Groups["sVal"].Value : m.Groups["nVal"].Value;
                        result.Add(new KeyValuePair<string, string>(key, value));
                    }
                }
            }
        }
        return result;
    }

    public List<KeyValuePair<string, string>> GetLabResult()
    {
        var result = new List<KeyValuePair<string, string>>();

        string pattern = "\\\"(?<key>[^\\\"]+)\\\"\\s*:\\s*(?:\\\"(?<sVal>[^\\\"]+)\\\"|(?<nVal>[^,}\\s]+))";

        foreach (var el in visibleElements)
        {
            if (el.Category == "LabResult")
            {
                if (el.Answer != null && el.Answer.Count >= 3)
                {
                    string json = el.Answer[2];
                    MatchCollection matches = Regex.Matches(json, pattern);

                    foreach (Match m in matches)
                    {
                        string key = m.Groups["key"].Value;
                        string value = m.Groups["sVal"].Success ? m.Groups["sVal"].Value : m.Groups["nVal"].Value;
                        result.Add(new KeyValuePair<string, string>(key, value));
                    }
                }
            }
        }
        return result;
    }

    public void VisitElement(Element el)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        if (el.Visited == null)
            el.Visited = new List<string>();
        el.Visited.Add(timestamp);

        if (el.Child != null)
        {
            foreach (var child in el.Child)
            {
                if (!visibleElements.Contains(child))
                    visibleElements.Add(child);
            }
        }

        // Need to make sure bed status question is only recorded once.
        if (el.Category == "TransferCenter" && el.LearnerResponse[2] == "What's our bed status?")
        {
            if (currentScenarioSummary.BedStatusAsked == BedStatusAsked.NotAsked)
            {
                RecordBedStatusAsked(pcScript.Connected);
                Debug.Log($"Recording bed status question timing. Connected: {pcScript.Connected}");
            }
            else
            {
                Debug.LogWarning("Bed status question already recorded; skipping.");
            }
        }
    }

    public void RecordBedStatusAsked(bool isCallAlreadyConnected)
    {
        if (currentScenarioSummary == null)
        {
            Debug.LogError("RecordBedStatusAsked called but current ScenarioSummary is null!");
            return;
        }

        if (isCallAlreadyConnected)
            currentScenarioSummary.BedStatusAsked = BedStatusAsked.AskedAfterConnected;
        else
            currentScenarioSummary.BedStatusAsked = BedStatusAsked.AskedBeforeConnected;
    }

    // reset points to zero
    public void exitScenario() {
        foreach (var items in scenarioSummaries)
        {
            items.TotalPoints = 0;
            items.TotalMax = 0;
        }
    }

    public void EndSessionAndSave()
    {
        int totalEarned = 0, totalMax = 0;
        foreach (var sum in scenarioSummaries)
        {
            totalEarned += sum.TotalPoints;
            totalMax += sum.TotalMax;
        }

        var session = new FullSessionSummary
        {
            sessionStartTime = sessionStartTime,
            netId = NetID,
            difficulty = Difficulty,
            scenarioSummaries = scenarioSummaries,
            totalEarned = totalEarned,
            totalMax = totalMax,
            totalTimeSeconds = sessionTotalTime
        };

        string jsonString = JsonUtility.ToJson(session, true);
        string filename = $"session_{NetID}_{DateTime.Now:yyyyMMddHHmmss}.json";

#if UNITY_WEBGL && !UNITY_EDITOR
        // --- WEBGL SAVING (Download) ---
        // Requires a JS plugin helper (assumed DownloadFileHelper class exists)
        if (typeof(DownloadFileHelper) != null) {
             DownloadFileHelper.DownloadToFile(jsonString, filename);
             Debug.Log($"Session ready for download: {filename}");
        } else {
             Debug.LogError("DownloadFileHelper class is missing!");
        }
        sessionJsonFilePath = filename;
#else
        // --- DESKTOP SAVING (Local File) ---
        string folderPath = Path.Combine(Application.persistentDataPath, "SessionRecords");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        sessionJsonFilePath = Path.Combine(folderPath, filename);
        File.WriteAllText(sessionJsonFilePath, jsonString);
        Debug.Log($"Session saved to: {sessionJsonFilePath}");
#endif
    }

    public void EndScenario()
    {
        pcScript.EndScenario();
    }

    // ===================================================================================
    // HELPER FUNCTIONS
    // ===================================================================================
    public Dictionary<int, ScenarioFileData> GetScenariosTable() {
        return scenarioFiles;
    }
}