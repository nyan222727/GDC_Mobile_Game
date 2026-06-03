using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[Serializable]
public sealed class LevelButtonBinding
{
    [SerializeField] private Button button;
    [SerializeField] private Text label;
    [SerializeField] private LevelDefinition level = new LevelDefinition();

    public Button Button => button;
    public LevelDefinition Level => level;

    public void RefreshLabel()
    {
        if (label != null && level != null)
        {
            label.text = level.DisplayName;
        }

        if (button != null)
        {
            button.interactable = level != null && level.IsValid;
        }
    }
}

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject startMenuScreen;
    [SerializeField] private GameObject levelSelectScreen;
    [SerializeField] private Button playButton;
    [SerializeField] private Button backButton;
    [SerializeField] private LevelButtonBinding[] levelButtons = Array.Empty<LevelButtonBinding>();
    [SerializeField] private bool showStartMenuOnAwake = true;

    private UnityAction[] levelButtonActions = Array.Empty<UnityAction>();

    private void Awake()
    {
        _ = GameSession.Instance;
        RegisterLevelsWithSession();
        ConfigureEventSystemInputModule();
        RegisterButtons();
        RefreshLevelButtons();

        if (showStartMenuOnAwake)
        {
            ShowStartMenu();
        }
        else
        {
            ShowLevelSelect();
        }
    }

    private void OnDestroy()
    {
        UnregisterButtons();
    }

    public void ShowStartMenu()
    {
        SetScreenActive(startMenuScreen, true);
        SetScreenActive(levelSelectScreen, false);
    }

    public void ShowLevelSelect()
    {
        SetScreenActive(startMenuScreen, false);
        SetScreenActive(levelSelectScreen, true);
    }

    public void StartLevelByIndex(int levelIndex)
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            LevelDefinition level = levelButtons[i].Level;
            if (level != null && level.LevelIndex == levelIndex)
            {
                StartLevel(level);
                return;
            }
        }
    }

    private void RegisterButtons()
    {
        levelButtonActions = new UnityAction[levelButtons.Length];

        if (playButton != null)
        {
            playButton.onClick.AddListener(ShowLevelSelect);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(ShowStartMenu);
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int bindingIndex = i;
            Button button = levelButtons[i].Button;
            if (button != null)
            {
                levelButtonActions[i] = () => StartLevel(bindingIndex);
                button.onClick.AddListener(levelButtonActions[i]);
            }
        }
    }

    private void UnregisterButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(ShowLevelSelect);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(ShowStartMenu);
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            Button button = levelButtons[i].Button;
            if (button != null && i < levelButtonActions.Length && levelButtonActions[i] != null)
            {
                button.onClick.RemoveListener(levelButtonActions[i]);
            }
        }
    }

    private void StartLevel(int bindingIndex)
    {
        if (bindingIndex < 0 || bindingIndex >= levelButtons.Length)
        {
            return;
        }

        StartLevel(levelButtons[bindingIndex].Level);
    }

    private void RegisterLevelsWithSession()
    {
        if (levelButtons == null)
        {
            return;
        }

        var levels = new LevelDefinition[levelButtons.Length];
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levels[i] = levelButtons[i].Level;
        }

        GameSession.Instance.SetAvailableLevels(levels);
    }

    private static void StartLevel(LevelDefinition level)
    {
        if (level == null || !level.IsValid)
        {
            return;
        }

        GameSession.Instance.SelectLevel(level);
        SceneManager.LoadScene(level.SceneName);
    }

    private void RefreshLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].RefreshLabel();
        }
    }

    private static void SetScreenActive(GameObject screen, bool active)
    {
        if (screen != null)
        {
            screen.SetActive(active);
        }
    }

    private static void ConfigureEventSystemInputModule()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = FindAnyObjectByType<EventSystem>();
        }

        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }
}
