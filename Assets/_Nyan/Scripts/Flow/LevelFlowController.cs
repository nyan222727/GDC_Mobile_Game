using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#pragma warning disable 0649

public interface ILevelMoneySource
{
    int CurrentMoney { get; }
}

public interface ILevelMoneyWallet : ILevelMoneySource
{
    event Action MoneyChanged;

    int AvailableMoney { get; }
    bool CanReserve(int amount);
    bool TryReserve(int amount);
    void ReleaseReserved(int amount);
    void CommitReserved(int amount);
    void AddMoney(int amount);
    void Refund(int amount);
    void ShowInsufficientFunds();
}

public sealed class LevelFlowController : MonoBehaviour
{
    public enum LevelState
    {
        Placement,
        Combat,
        Result
    }

    public enum ResultOutcome
    {
        None,
        Victory,
        Defeat
    }

    [Serializable]
    public sealed class WaveSpawnDefinition
    {
        [SerializeField] private string spawnName = "Spawn";
        [SerializeField] private GenerateDoorSpawner spawner;
        [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();
        [SerializeField] private int spawnCount = 5;
        [SerializeField] private float spawnInterval = 1f;

        public string SpawnName => spawnName;
        public GenerateDoorSpawner Spawner => spawner;
        public IReadOnlyList<GameObject> EnemyPrefabs => enemyPrefabs;
        public int SpawnCount => Mathf.Max(0, spawnCount);
        public float SpawnInterval => Mathf.Max(0f, spawnInterval);
        public bool HasEnemyPrefabs => enemyPrefabs != null && enemyPrefabs.Count > 0;
    }

    [Serializable]
    public sealed class WaveDefinition
    {
        [SerializeField] private string waveName = "Wave";
        [SerializeField] private List<GameObject> enemies = new List<GameObject>();
        [SerializeField] private List<WaveSpawnDefinition> spawns = new List<WaveSpawnDefinition>();
        [SerializeField] private int rewardOnClear;

        public string WaveName => waveName;
        public IReadOnlyList<GameObject> Enemies => enemies;
        public IReadOnlyList<WaveSpawnDefinition> Spawns => spawns;
        public int RewardOnClear => rewardOnClear;
        public bool HasEnemies => enemies != null && enemies.Count > 0;
        public bool HasSpawnEntries => spawns != null && spawns.Count > 0;
        public bool HasContent => HasEnemies || HasSpawnEntries;

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
    [SerializeField] private Text resultMoneyText;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextLevelButton;

    [Header("Money")]
    [SerializeField] private MonoBehaviour moneySource;

    [Header("Waves")]
    [SerializeField] private List<WaveDefinition> waves = new List<WaveDefinition>();
    [SerializeField] private bool autoBuildSingleWaveFromSceneEnemies = true;
    [SerializeField] private bool deactivateEnemiesOnAwake = true;

    [Header("Spawner Waves")]
    [SerializeField] private bool useSpawnerWaves = true;
    [SerializeField] private bool autoUseSceneSpawners = true;
    [SerializeField] private bool includeLegacySceneEnemies;
    [SerializeField] private int defaultSpawnerWaveCount = 3;

    [Header("Base")]
    [SerializeField] private int baseMaxHp = 3;
    [SerializeField] private Text baseHpText;

    private readonly List<WaveDefinition> runtimeWaves = new List<WaveDefinition>();
    private readonly List<GameObject> waveEnemyBuffer = new List<GameObject>();
    private readonly List<GenerateDoorSpawner> spawnerBuffer = new List<GenerateDoorSpawner>();
    private readonly List<Coroutine> activeSpawnRoutines = new List<Coroutine>();
    private readonly HashSet<GameObject> trackedEnemies = new HashSet<GameObject>();
    private readonly HashSet<int> enemiesHandledAtGoal = new HashSet<int>();
    private LevelState currentState;
    private ResultOutcome resultOutcome;
    private int currentWaveIndex;
    private int remainingEnemiesInCurrentWave;
    private int activeSpawnerRoutineCount;
    private int currentBaseHp;
    private bool isQuitting;

    public LevelState CurrentState => currentState;
    public ResultOutcome CurrentResultOutcome => resultOutcome;
    public int CurrentWaveIndex => currentWaveIndex;
    public int TotalWaveCount => runtimeWaves.Count;
    public int CurrentBaseHp => currentBaseHp;
    public int BaseMaxHp => Mathf.Max(1, baseMaxHp);
    public int CurrentResultMoney => GetCurrentResultMoney();
    public bool IsInPlacementState => currentState == LevelState.Placement;
    public bool IsInCombatState => currentState == LevelState.Combat;

    private void Awake()
    {
        ResolveReferences();
        currentBaseHp = BaseMaxHp;
        BuildRuntimeWaves();
        PrepareWaveEnemies();
        RegisterUiActions();
        EnterPlacementState();
    }

    private void OnDestroy()
    {
        StopActiveSpawnRoutines();
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
            EnterResultState(ResultOutcome.Victory);
            return;
        }

        var wave = runtimeWaves[currentWaveIndex];
        StopActiveSpawnRoutines();
        currentState = LevelState.Combat;
        remainingEnemiesInCurrentWave = 0;
        enemiesHandledAtGoal.Clear();

        if (placementController != null)
        {
            placementController.SetPlacementEnabled(false);
        }

        if (!useSpawnerWaves || includeLegacySceneEnemies)
        {
            CollectWaveEnemies(wave, waveEnemyBuffer);
            for (int i = 0; i < waveEnemyBuffer.Count; i++)
            {
                RegisterWaveEnemy(waveEnemyBuffer[i], currentWaveIndex, true);
            }
        }

        StartSpawnerWave(wave, currentWaveIndex);
        RefreshUi();
        TryCompleteCurrentWaveWhenEmpty();
    }

    public void NotifyEnemyReachedGoal(GameObject enemy, int damageToBase = 1, bool destroyEnemy = true)
    {
        if (isQuitting || currentState != LevelState.Combat || enemy == null)
        {
            return;
        }

        int enemyId = enemy.GetInstanceID();
        if (!enemiesHandledAtGoal.Add(enemyId))
        {
            return;
        }

        currentBaseHp = Mathf.Max(0, currentBaseHp - Mathf.Max(0, damageToBase));

        if (currentBaseHp <= 0)
        {
            WaveEnemyTracker defeatedTracker = enemy.GetComponent<WaveEnemyTracker>();
            if (defeatedTracker != null)
            {
                defeatedTracker.Disarm();
            }

            if (destroyEnemy)
            {
                Destroy(enemy);
            }

            EnterResultState(ResultOutcome.Defeat);
            return;
        }

        WaveEnemyTracker tracker = enemy.GetComponent<WaveEnemyTracker>();
        if (tracker != null)
        {
            tracker.NotifyDefeated(false);
        }

        if (destroyEnemy)
        {
            Destroy(enemy);
        }

        RefreshUi();
    }

    public void RegisterSpawnedEnemy(GameObject enemy)
    {
        RegisterWaveEnemy(enemy, currentWaveIndex, true);
        RefreshUi();
    }

    public void NotifyEnemyDefeated(
        WaveEnemyTracker tracker,
        int waveIndex,
        bool grantsCoinReward)
    {
        if (isQuitting || currentState != LevelState.Combat || waveIndex != currentWaveIndex)
        {
            return;
        }

        if (grantsCoinReward)
        {
            GrantEnemyCoinReward(tracker);
        }

        remainingEnemiesInCurrentWave = Mathf.Max(0, remainingEnemiesInCurrentWave - 1);
        RefreshUi();
        TryCompleteCurrentWaveWhenEmpty();
    }

    private void RegisterWaveEnemy(GameObject enemy, int waveIndex, bool activateEnemy)
    {
        if (enemy == null || currentState != LevelState.Combat || waveIndex != currentWaveIndex)
        {
            return;
        }

        WaveEnemyTracker tracker = enemy.GetComponent<WaveEnemyTracker>();
        if (tracker == null)
        {
            tracker = enemy.AddComponent<WaveEnemyTracker>();
        }

        tracker.Arm(this, waveIndex);
        if (activateEnemy)
        {
            enemy.SetActive(true);
        }

        remainingEnemiesInCurrentWave++;
    }

    private void StartSpawnerWave(WaveDefinition wave, int waveIndex)
    {
        if (!useSpawnerWaves)
        {
            return;
        }

        bool startedSpawner = false;
        IReadOnlyList<WaveSpawnDefinition> spawnEntries = wave != null ? wave.Spawns : null;
        if (spawnEntries != null)
        {
            for (int i = 0; i < spawnEntries.Count; i++)
            {
                WaveSpawnDefinition spawnEntry = spawnEntries[i];
                if (spawnEntry == null)
                {
                    continue;
                }

                if (spawnEntry.Spawner != null)
                {
                    StartSpawnerRoutine(spawnEntry.Spawner, spawnEntry, waveIndex);
                    startedSpawner = true;
                    continue;
                }

                if (autoUseSceneSpawners)
                {
                    FindSceneSpawners(spawnerBuffer);
                    for (int spawnerIndex = 0; spawnerIndex < spawnerBuffer.Count; spawnerIndex++)
                    {
                        StartSpawnerRoutine(spawnerBuffer[spawnerIndex], spawnEntry, waveIndex);
                        startedSpawner = true;
                    }
                }
            }
        }

        if (!startedSpawner && autoUseSceneSpawners)
        {
            FindSceneSpawners(spawnerBuffer);
            for (int i = 0; i < spawnerBuffer.Count; i++)
            {
                StartSpawnerRoutine(spawnerBuffer[i], null, waveIndex);
            }
        }
    }

    private void StartSpawnerRoutine(
        GenerateDoorSpawner spawner,
        WaveSpawnDefinition spawnEntry,
        int waveIndex)
    {
        if (spawner == null)
        {
            return;
        }

        Coroutine routine = StartCoroutine(SpawnWaveRoutine(spawner, spawnEntry, waveIndex));
        activeSpawnRoutines.Add(routine);
    }

    private IEnumerator SpawnWaveRoutine(
        GenerateDoorSpawner spawner,
        WaveSpawnDefinition spawnEntry,
        int waveIndex)
    {
        activeSpawnerRoutineCount++;

        IReadOnlyList<GameObject> enemyPrefabs = spawnEntry != null && spawnEntry.HasEnemyPrefabs
            ? spawnEntry.EnemyPrefabs
            : null;

        int spawnCount = spawnEntry != null ? spawnEntry.SpawnCount : 0;
        float spawnInterval = spawnEntry != null ? spawnEntry.SpawnInterval : 0f;

        yield return spawner.SpawnWave(
            enemyPrefabs,
            spawnCount,
            spawnInterval,
            enemy => RegisterWaveEnemy(enemy, waveIndex, true));

        activeSpawnerRoutineCount = Mathf.Max(0, activeSpawnerRoutineCount - 1);
        RefreshUi();
        TryCompleteCurrentWaveWhenEmpty();
    }

    private void StopActiveSpawnRoutines()
    {
        for (int i = 0; i < activeSpawnRoutines.Count; i++)
        {
            if (activeSpawnRoutines[i] != null)
            {
                StopCoroutine(activeSpawnRoutines[i]);
            }
        }

        activeSpawnRoutines.Clear();
        activeSpawnerRoutineCount = 0;
    }

    private static void FindSceneSpawners(List<GenerateDoorSpawner> results)
    {
        results.Clear();
        GenerateDoorSpawner[] spawners = FindObjectsByType<GenerateDoorSpawner>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        for (int i = 0; i < spawners.Length; i++)
        {
            if (spawners[i] != null)
            {
                results.Add(spawners[i]);
            }
        }
    }

    private void TryCompleteCurrentWaveWhenEmpty()
    {
        if (currentState == LevelState.Combat &&
            activeSpawnerRoutineCount == 0 &&
            remainingEnemiesInCurrentWave == 0)
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

        if (!(moneySource is ILevelMoneyWallet))
        {
            moneySource = FindMoneyWallet();
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
                if (wave != null && (wave.HasContent || useSpawnerWaves))
                {
                    runtimeWaves.Add(wave);
                }
            }
        }

        if (useSpawnerWaves && runtimeWaves.Count == 0)
        {
            int waveCount = Mathf.Max(1, defaultSpawnerWaveCount);
            for (int i = 0; i < waveCount; i++)
            {
                runtimeWaves.Add(new WaveDefinition());
            }

            return;
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
        resultOutcome = ResultOutcome.None;
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

        EnterResultState(ResultOutcome.Victory);
    }

    private void EnterResultState(ResultOutcome outcome)
    {
        StopActiveSpawnRoutines();
        currentState = LevelState.Result;
        resultOutcome = outcome;
        remainingEnemiesInCurrentWave = 0;

        if (placementController != null)
        {
            placementController.SetPlacementEnabled(false);
        }

        GameSession.Instance.SetGameplayResult(
            outcome == ResultOutcome.Defeat ? GameplayResult.Defeat : GameplayResult.Victory);

        RefreshUi();
    }

    private void RegisterUiActions()
    {
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(StartCurrentWave);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(ReturnToMenu);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryLevel);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        }
    }

    private void UnregisterUiActions()
    {
        if (startWaveButton != null)
        {
            startWaveButton.onClick.RemoveListener(StartCurrentWave);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(ReturnToMenu);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(RetryLevel);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveListener(LoadNextLevel);
        }
    }

    private void RefreshUi()
    {
        bool isResult = currentState == LevelState.Result;
        bool isVictory = isResult && resultOutcome == ResultOutcome.Victory;

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
                LevelState.Result => GetResultLabel(),
                _ => currentState.ToString()
            };
        }

        if (waveText != null)
        {
            int displayWave = Mathf.Min(currentWaveIndex + 1, Mathf.Max(1, runtimeWaves.Count));
            waveText.text = $"Wave {displayWave}/{Mathf.Max(1, runtimeWaves.Count)} | Base HP {currentBaseHp}/{BaseMaxHp}";
        }

        if (baseHpText != null)
        {
            baseHpText.text = $"Base HP {currentBaseHp}/{BaseMaxHp}";
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(isResult);
        }

        if (resultText != null)
        {
            resultText.text = isResult ? GetResultLabel() : string.Empty;
        }

        if (resultMoneyText != null)
        {
            resultMoneyText.gameObject.SetActive(isVictory);
            resultMoneyText.text = isVictory ? $"Money: {CurrentResultMoney}" : string.Empty;
        }

        if (menuButton != null)
        {
            menuButton.interactable = isResult;
        }

        if (retryButton != null)
        {
            retryButton.interactable = isResult;
        }

        if (nextLevelButton != null)
        {
            bool canLoadNextLevel = isVictory && GameSession.Instance.TryPeekNextLevel(out _);
            nextLevelButton.gameObject.SetActive(canLoadNextLevel);
            nextLevelButton.interactable = canLoadNextLevel;
        }
    }

    private string GetResultLabel()
    {
        return resultOutcome == ResultOutcome.Defeat ? "Defeat" : "Victory";
    }

    private int GetCurrentResultMoney()
    {
        if (moneySource is ILevelMoneySource resultMoneySource)
        {
            return Mathf.Max(0, resultMoneySource.CurrentMoney);
        }

        return 0;
    }

    private void GrantEnemyCoinReward(WaveEnemyTracker tracker)
    {
        if (tracker == null || !(moneySource is ILevelMoneyWallet wallet))
        {
            return;
        }

        EnemyCoinReward reward = tracker.GetComponent<EnemyCoinReward>();
        if (reward == null)
        {
            Debug.LogWarning(
                $"Enemy '{tracker.name}' has no {nameof(EnemyCoinReward)} and grants no money.",
                tracker);
            return;
        }

        wallet.AddMoney(reward.RewardAmount);
    }

    private static MonoBehaviour FindMoneyWallet()
    {
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour is ILevelMoneyWallet)
            {
                return behaviour;
            }
        }

        return null;
    }

    private void ReturnToMenu()
    {
        string menuSceneName = GameSession.Instance.MenuSceneName;
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }

    private void RetryLevel()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(activeSceneName))
        {
            SceneManager.LoadScene(activeSceneName);
        }
    }

    private void LoadNextLevel()
    {
        if (GameSession.Instance.TrySelectNextLevel(out LevelDefinition nextLevel))
        {
            SceneManager.LoadScene(nextLevel.SceneName);
        }
    }
}

#pragma warning restore 0649
