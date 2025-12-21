using TMPro;
using UnityEngine;

public class ResultsScript : MonoBehaviour
{
    public GameObject infoGatheringScoreTMP;
    public GameObject admissionDecisionScoreTMP;
    public GameObject timeScoreTMP;
    public GameObject totalScoreTMP;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateResults(int questionScore, int decisionScore, int timeScore)
    {
        infoGatheringScoreTMP.GetComponent<TextMeshProUGUI>().text = questionScore.ToString();
        admissionDecisionScoreTMP.GetComponent<TextMeshProUGUI>().text = decisionScore.ToString();
        timeScoreTMP.GetComponent<TextMeshProUGUI>().text = timeScore.ToString();
        totalScoreTMP.GetComponent<TextMeshProUGUI>().text = (questionScore + decisionScore + timeScore).ToString();
    }
}
