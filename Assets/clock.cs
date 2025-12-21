using System;
using TMPro;
using UnityEngine;

public class clock : MonoBehaviour
{
    public float time = 0.0f;

    private TextMeshProUGUI _text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        double minutes = Math.Floor(time / 60);
        double seconds = Math.Floor(time % 60);
        _text.text = ("On call: " + minutes.ToString("00") + ":" + seconds.ToString("00"));
        
    }

    public void Reset()
    {
        time = 0.0f;
    }
}
