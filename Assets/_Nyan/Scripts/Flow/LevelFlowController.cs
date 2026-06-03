using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

public sealed class LevelFlowController : MonoBehaviour
{
    public enum LevelState
    {
        Placement,
        Combat,
        Result
    }

    [Serializable]
    public sealed class WaveDefinition
    {
        [SerializeField] private string waveName = "Wave";
        [SerializeField] private List<GameObject> enemies = new List<GameObject>();
        [SerializeField] private int rewardOnClear;

        public string WaveName => waveName;
        public IReadOnlyList<GameObject> Enemies => enemies;
        public int RewardOnClear => rewardOnClear;
        public bool HasEnemies => enemies != null && enemies.Count > 0;

        public void AddEnemy(GameObject enemy)
        {
            if (enemy == null)
            {
                return;
            }

            enemies ??= new List<GameObject>();
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
            }
        }
    }

    [Header("References")]
    [SerializeField] private PlacementController placementController;
    [SerializeField] private Button startWaveButton;
    [SerializeField] private Text stateText;
    [SerializeField] private Text waveText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultText;

    [Header("Waves")]
    [SerializeField] private List<WaveDefinition> waves = new List<WaveDefinition>();
    [SerializeField] private bool autoBuildSingleWaveFromSceneEnemies = true;
    [SerializeField] private bool deactivateEnemiesOnAwake = true;

    private readonly List<WaveDefinition> runtimeWaves = new List<WaveDefinition>();
    private readonly List<GameObject> waveEnemyBuffer = new List<GameObject>();
    private readonly HashSet<GameObject> trackedEnemies = new HashSet<GameObject>();
    private LevelState currentState;
    private int currentWaveIndex;
    private int remainingEnemiesInCurrentWave;
    private bool isQuitting;

    public LevelState CurrentState => currentState;
    public int CurrentWaveIndex => currentWaveIndex;
    public int TotalWaveCount => runtimeWaves.Count;
    public bool IsInPlacementState => currentState == LevelState.Placement;
    public bool IsInCombatState => currentState == LevelState.Combat;

    private void Awake()
    {
        ResolveReferences();
        BuildRuntimeWaves();
        PrepareWaveEnemies();
        RegisterUiActions();
        EnterPlacementState();
    }

    private void OnDestroy()
    {
        UnregisterUiActions();
        isQuitting = true;
    }

    public void StartCurrentWave()
    {
        if (currentState != LevelState.Placement)
        {
            return;
        }

        if (currentWaveIndex >= runtimeWaves.Count)
        {
            EnterResultState();
            return;
        }

        var wave = runtimeWaves[currentWaveIndex];
        currentState = LevelState.Combat;
        remainingEnemiesInCurrentWave = 0;
        CollectWaveEnemies(wave, waveEnemyBuffer);

        if (placementController != null)
        {
            placementController.SetPlacementEnabled(false);
        }

        for (int i = 0; i < waveEnemyBuffer.Count; i++)
        {
            GameObject enemy = waveEnemyBuffer[i];
            if (enemy == null)
            {
                continue;
            }

            WaveEnemyTracker tracker = enemy.GetComponent<WaveEnemyTracker>();
            if (tracker == null)
            {
                tracker = enemy.AddComponent<WaveEnemyTracker>();
            }

            tracker.Arm(this, currentWaveIndex);
            enemy.SetActive(true);
            remainingEnemiesInCurrentWave++;
        }

        RefreshUi();

        if (remainingEnemiesInCurrentWave == 0)
        {
            CompleteCurrentWave();
        }
    }

    public void NotifyEnemyDefeated(WaveEnemyTracker tracker, int waveIndex)
    {
        if (isQuitting || currentState != LevelState.Combat || waveIndex != currentWaveIndex)
        {
            return;
        }

        remainingEnemiesInCurrentWave = Mathf.Max(0, remainingEnemiesInCurrentWave - 1);
        RefreshUi();

        if (remainingEnemiesInCurrentWave == 0)
        {
            CompleteCurrentWave();
        }
    }

    private void ResolveReferences()
    {
        if (placementController == null)
        {
            placementController = FindAnyObjectByType<PlacementController>();
        }
    }

    private void BuildRuntimeWaves()
    {
        runtimeWaves.Clear();

        if (waves != null)
        {
            for (int i = 0; i < waves.Count; i++)
            {
                WaveDefinition wave = waves[i];
                if (wave != null && wave.HasEnemies)
                {
                    runtimeWaves.Add(wave);
                }
            }
        }

        if (runtimeWaves.Count > 0 || !autoBuildSingleWaveFromSceneEnemies)
        {
            return;
        }

        var fallbackWave = new WaveDefinition();
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || behaviour.gameObject == gameObject)
            {
                continue;
            }

            if (IsEnemyBehaviour(behaviour))
            {
                fallbackWave.AddEnemy(behaviour.gameObject);
            }
        }

        if (fallbackWave.HasEnemies)
        {
            runtimeWaves.Add(fallbackWave);
        }
    }

    private void PrepareWaveEnemies()
    {
        trackedEnemies.Clear();
        for (int waveIndex = 0; waveIndex < runtimeWaves.Count; waveIndex++)
        {
            WaveDefinition wave = runtimeWaves[waveIndex];
            CollectWaveEnemies(wave, waveEnemyBuffer);

            for (int i = 0; i < waveEnemyBuffer.Count; i++)
            {
                GameObject enemy = waveEnemyBuffer[i];
                if (enemy == null || !trackedEnemies.Add(enemy))
                {
                    continue;
                }

                WaveEnemyTracker tracker = enemy.GetComponent<WaveEnemyTracker>();
                if (tracker == null)
                {
                    tracker = enemy.AddComponent<WaveEnemyTracker>();
                }

                tracker.Disarm();
                if (deactivateEnemiesOnAwake)
                {
                    enemy.SetActive(false);
                }
            }
        }
    }

    private static void CollectWaveEnemies(WaveDefinition wave, List<GameObject> results)
    {
        results.Clear();

        if (wave == null)
        {
            return;
        }

        IReadOnlyList<GameObject> waveEntries = wave.Enemies;
        if (waveEntries == null)
        {
            return;
        }

        for (int i = 0; i < waveEntries.Count; i++)
        {
            CollectEnemyObjects(waveEntries[i], results);
        }
    }

    private static void CollectEnemyObjects(GameObject entry, List<GameObject> results)
    {
        if (entry == null)
        {
            return;
        }

        if (IsEnemyObject(entry))
        {
            AddUniqueEnemy(results, entry);
            return;
        }

        MonoBehaviour[] childBehaviours = entry.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < childBehaviours.Length; i++)
        {
            MonoBehaviour behaviour = childBehaviours[i];
            if (IsTrackableEnemyBehaviour(behaviour))
            {
                AddUniqueEnemy(results, behaviour.gameObject);
            }
        }
    }

    private static bool IsEnemyObject(GameObject candidate)
    {
        if (candidate == null)
        {
            return false;
        }

        if (candidate.GetComponent<WaveEnemyTracker>() != null)
        {
            return true;
        }

        MonoBehaviour[] behaviours = candidate.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (IsTrackableEnemyBehaviour(behaviours[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsTrackableEnemyBehaviour(MonoBehaviour behaviour)
    {
        return behaviour is WaveEnemyTracker || IsEnemyBehaviour(behaviour);
    }

    private static bool IsEnemyBehaviour(MonoBehaviour behaviour)
    {
        if (behaviour == null)
        {
            return false;
        }

        string typeName = behaviour.GetType().Name;
        return typeName == "EnemyController" || typeName == "EnemyTest";
    }

    private static void AddUniqueEnemy(List<GameObject> results, GameObject enemy)
    {
        if (enemy != null && !results.Contains(enemy))
        {
            results.Add(enemy);
        }
    }

    private void EnterPlacementState()
    {
        currentState = LevelState.Placement;
        remainingEnemiesInCurrentWave = 0;

        if (placementController != null)
        {
            placementController.SetPlacementEnabled(true);
        }

        RefreshUi();
    }

    private void CompleteCurrentWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex < runtimeWaves.Count)
        {
            EnterPlacementState();
            return;
        }

        EnterResultState();
    }

    private void EnterResultState()
    {
        currentState = LevelState.Result;
        remainingEnemiesInCurrentWave = 0;

        if (placementController != null)
        {
            placementController.SetPlacementEnabled(false);
        }

        RefreshUi();
    }

    private void RegisterUiActions()
    {
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(StartCurrentWave);
        }
    }

    private void UnregisterUiActions()
    {
        if (startWaveButton != null)
        {
            startWaveButton.onClick.RemoveListener(StartCurrentWave);
        }
    }

    private void RefreshUi()
    {
        if (startWaveButton != null)
        {
            bool canStartWave = currentState == LevelState.Placement && currentWaveIndex < runtimeWaves.Count;
            startWaveButton.gameObject.SetActive(canStartWave);
            startWaveButton.interactable = canStartWave;
        }

        if (stateText != null)
        {
            stateText.text = currentState switch
            {
                LevelState.Placement => "Game State",
                LevelState.Combat => $"Play State ({remainingEnemiesInCurrentWave})",
                LevelState.Result => "Result",
                _ => currentState.ToString()
            };
        }

        if (waveText != null)
        {
            int displayWave = Mathf.Min(currentWaveIndex + 1, Mathf.Max(1, runtimeWaves.Count));
            waveText.text = $"Wave {displayWave}/{Mathf.Max(1, runtimeWaves.Count)}";
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(currentState == LevelState.Result);
        }

        if (resultText != null)
        {
            resultText.text = currentState == LevelState.Result ? "Level Clear" : string.Empty;
        }
    }
}

#pragma warning restore 0649
