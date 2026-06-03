using UnityEngine;

#pragma warning disable 0649

[DisallowMultipleComponent]
public sealed class EnemyGoalTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelFlowController levelFlowController;

    [Header("Goal Rules")]
    [SerializeField] private int damageToBase = 1;
    [SerializeField] private bool destroyEnemyOnReachGoal = true;
    [SerializeField] private LayerMask enemyLayerMask = ~0;

    [Header("Physics")]
    [SerializeField] private bool ensureTriggerCollider = true;
    [SerializeField] private bool ensureKinematicRigidbody = true;

    private void Reset()
    {
        ResolveReferences();
        ConfigurePhysicsComponents();
    }

    private void Awake()
    {
        ResolveReferences();
        ConfigurePhysicsComponents();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (levelFlowController == null || other == null || !IsLayerAllowed(other.gameObject.layer))
        {
            return;
        }

        if (TryGetEnemyRoot(other, out GameObject enemy))
        {
            levelFlowController.NotifyEnemyReachedGoal(enemy, damageToBase, destroyEnemyOnReachGoal);
        }
    }

    private void ResolveReferences()
    {
        if (levelFlowController == null)
        {
            levelFlowController = FindAnyObjectByType<LevelFlowController>();
        }
    }

    private void ConfigurePhysicsComponents()
    {
        if (ensureTriggerCollider)
        {
            Collider triggerCollider = GetComponent<Collider>();
            if (triggerCollider == null)
            {
                triggerCollider = gameObject.AddComponent<BoxCollider>();
            }

            triggerCollider.isTrigger = true;
        }

        if (ensureKinematicRigidbody)
        {
            Rigidbody body = GetComponent<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }

            body.isKinematic = true;
            body.useGravity = false;
        }
    }

    private bool IsLayerAllowed(int layer)
    {
        return (enemyLayerMask.value & (1 << layer)) != 0;
    }

    private static bool TryGetEnemyRoot(Collider collider, out GameObject enemy)
    {
        enemy = null;

        WaveEnemyTracker tracker = collider.GetComponentInParent<WaveEnemyTracker>();
        if (tracker != null)
        {
            enemy = tracker.gameObject;
            return true;
        }

        MonoBehaviour[] behaviours = collider.GetComponentsInParent<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (IsEnemyBehaviour(behaviour))
            {
                enemy = behaviour.gameObject;
                return true;
            }
        }

        return false;
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
}

#pragma warning restore 0649
