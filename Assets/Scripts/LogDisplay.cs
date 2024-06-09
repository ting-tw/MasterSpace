using TMPro;
using UnityEngine;

public class LogDisplay : MonoBehaviour
{
    TMP_Text display;
    void Start()
    {
        display = GetComponent<TMP_Text>();
        Application.logMessageReceived += HandleLog;
    }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        display.text += "\n" + logString;
        Canvas.ForceUpdateCanvases();
    }
}