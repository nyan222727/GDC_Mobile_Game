using System;
using UnityEngine;

public enum GameplayResult
{
    None,
    Victory,
    Defeat
}

[Serializable]
public sealed class LevelDefinition
{
    [SerializeField] private int levelIndex = 1;
    [SerializeField] private string displayName = "Level 1";
    [SerializeField] private string sceneName = "Nyan_Scene";
    [SerializeField] private bool hasNextLevel;
    [SerializeField] private int nextLevelIndex;

    public LevelDefinition()
    {
    }

    public LevelDefinition(int levelIndex, string displayName, string sceneName, bool hasNextLevel, int nextLevelIndex)
    {
        this.levelIndex = Mathf.Max(1, levelIndex);
        this.displayName = displayName;
        this.sceneName = sceneName;
        this.hasNextLevel = hasNextLevel;
        this.nextLevelIndex = Mathf.Max(0, nextLevelIndex);
    }

    public int LevelIndex => Mathf.Max(1, levelIndex);
    public string DisplayName => string.IsNullOrEmpty(displayName) ? $"Level {LevelIndex}" : displayName;
    public string SceneName => sceneName;
    public bool HasNextLevel => hasNextLevel;
    public int NextLevelIndex => nextLevelIndex;
    public bool IsValid => !string.IsNullOrEmpty(sceneName);

    public LevelDefinition Copy()
    {
        return new LevelDefinition(LevelIndex, DisplayName, SceneName, HasNextLevel, NextLevelIndex);
    }
}

public sealed class GameSession : MonoBehaviour
{
    [SerializeField] private LevelDefinition defaultLevel = new LevelDefinition(1, "Level 1", "Nyan_Scene", true, 2);
    [SerializeField] private string menuSceneName = "Menu_Scene";

    private static GameSession instance;
    private LevelDefinition selectedLevel;
    private GameplayResult lastResult = GameplayResult.None;

    public static GameSession Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindAnyObjectByType<GameSession>();
            if (instance != null)
            {
                instance.InitializeSingleton();
                return instance;
            }

            var sessionObject = new GameObject("Game Session");
            instance = sessionObject.AddComponent<GameSession>();
            instance.InitializeSingleton();
            return instance;
        }
    }

    public LevelDefinition SelectedLevel => selectedLevel ?? defaultLevel;
    public int SelectedLevelIndex => SelectedLevel.LevelIndex;
    public string SelectedLevelSceneName => SelectedLevel.SceneName;
    public string MenuSceneName => string.IsNullOrEmpty(menuSceneName) ? "Menu_Scene" : menuSceneName;
    public GameplayResult LastResult => lastResult;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        InitializeSingleton();
    }

    public void SelectLevel(LevelDefinition level)
    {
        if (level == null || !level.IsValid)
        {
            return;
        }

        selectedLevel = level.Copy();
        lastResult = GameplayResult.None;
    }

    public void SetGameplayResult(GameplayResult result)
    {
        lastResult = result;
    }

    public bool TryGetNextLevelIndex(out int nextLevelIndex)
    {
        LevelDefinition level = SelectedLevel;
        nextLevelIndex = level.NextLevelIndex;
        return level.HasNextLevel && nextLevelIndex > 0;
    }

    private void InitializeSingleton()
    {
        DontDestroyOnLoad(gameObject);

        if (selectedLevel == null && defaultLevel != null && defaultLevel.IsValid)
        {
            selectedLevel = defaultLevel.Copy();
        }
    }
}
