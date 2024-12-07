using UnityEngine;

public class RuntimeLoggerService : ICustomLoggerService
{
    public void Log(string message, string type = "Info")
    {
        Debug.Log($"[CUSTOM] {type}: {message}");
    }
}
