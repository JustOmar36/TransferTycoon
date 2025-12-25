using System;
using System.Collections.Generic;
using UnityEngine;

// Used for scoring when a player asks about bed status.
public enum BedStatusAsked
{
    NotAsked, // Default
    AskedAfterConnected,
    AskedBeforeConnected
}

// --------- Universal Game Element Data Structure ---------
[Serializable]
public class Element
{
    public string Category;
    [HideInInspector]
    public List<string> LearnerResponse; // [Transmitter, Receiver, Message]
    public List<string> Matches;
    public List<string> Answer;          // [Transmitter, Receiver, Message]
    public int Score;
    public List<string> Visited; // timestamps or null
    public List<string> Function; // optional
    [HideInInspector]
    public List<Element> Child; // optional
}

// --------- Scenario File Data Structure ---------
[Serializable]
public class TimingPoint
{
    public float timeSeconds;
    public int points;
}

[Serializable]
public class ScenarioFileData
{
    public string ScenarioName;
    public List<Element> Elements;
    public List<TimingPoint> TimingPointsMap; 
}

// --------- Scenario Config Structure ---------
[Serializable]
public class ScenariosConfig
{
    public string description;
    public int scenarioCount;
    // This list tells the WebGL build which files to fetch.
    public List<string> scenarioFiles;
}

// --------- Scenario Score Summary ---------
[Serializable]
public class ScenarioScoreSummary
{
    public string scenarioName;
    public int PresentIllnessHistoryEarned;
    public int PresentIllnessHistoryMax;
    public int ElicitedHistoryEarned;
    public int ElicitedHistoryMax;
    public int InterventionsEarned;
    public int InterventionsMax;
    public int ActionsEarned;
    public int ActionsMax;
    public int TimingPointsEarned;
    public int TimingPointsMax;
    public int TotalPoints;
    public int TotalMax;
    public float timeElapsedSeconds;

    public BedStatusAsked BedStatusAsked;

    // ADD THIS FIELD:
    public List<Element> VisibleElements = new List<Element>();
}

// --------- Full Session Summary ---------
[Serializable]
public class FullSessionSummary
{
    public string sessionStartTime;
    public string netId;
    public string difficulty;
    public List<ScenarioScoreSummary> scenarioSummaries = new List<ScenarioScoreSummary>();
    public int totalEarned;
    public int totalMax;
    public float totalTimeSeconds;
}