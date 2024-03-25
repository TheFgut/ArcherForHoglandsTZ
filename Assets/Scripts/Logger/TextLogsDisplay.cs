using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TextLogsDisplay : MonoBehaviour, ILogger
{
    [SerializeField] private Text logsDisplay;
    [SerializeField] private float logLifetime = 3;
    [SerializeField] private int logsCount = 7;
    public void Log(string data)
    {
        if(logs.Count >= 7)
        {
            RemoveLastLog();
        }
        LogData logData = new LogData(data, logLifetime);
        logsDisplay.text += data + '\n';
        logs.AddFirst(logData);
    }

    private void RemoveLastLog()
    {
        LogData last = logs.Last.Value;
        int startIndexOfLastLog = logsDisplay.text.IndexOf('\n');
        string updatedText = logsDisplay.text.Remove(0,startIndexOfLastLog + 1);
        logsDisplay.text = updatedText;
    }

    private LinkedList<LogData> logs = new LinkedList<LogData>();
    private class LogData
    {
        private float lifeTimer;
        public LogData(string data, float lifetime)
        {
            lifeTimer = lifetime;
        }

        public bool tryToLive(float deltaTime)
        {
            lifeTimer -= deltaTime;
            return lifeTimer < 0;
        }
    }
}
