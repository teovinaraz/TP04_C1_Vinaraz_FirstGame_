using UnityEngine;

public static class GameSettingsLoader
{
    public static GameSettings Load()
    {
        GameSettings settings = Resources.Load<GameSettings>("GameSettings");
        if (settings != null)
        {
            return settings;
        }

        Debug.LogWarning("GameSettings.asset was not found in a Resources folder. Default values will be used.");
        return ScriptableObject.CreateInstance<GameSettings>();
    }
}
