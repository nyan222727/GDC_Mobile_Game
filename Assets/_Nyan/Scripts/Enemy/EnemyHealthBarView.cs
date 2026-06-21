using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyHealthBarView : MonoBehaviour
{
    [SerializeField] private EnemyController enemy;
    [SerializeField] private Transform fill;

    private Vector3 fillFullScale;
    private Vector3 fillFullPosition;

    private void Awake()
    {
        if (enemy == null)
        {
            enemy = GetComponentInParent<EnemyController>();
        }

        if (fill != null)
        {
            fillFullScale = fill.localScale;
            fillFullPosition = fill.localPosition;
        }
    }

    private void LateUpdate()
    {
        if (enemy == null || fill == null)
        {
            return;
        }

        float ratio = Mathf.Clamp01(enemy.HP / (float)Mathf.Max(1, enemy.maxHP));
        Vector3 scale = fillFullScale;
        scale.x *= ratio;
        fill.localScale = scale;

        Vector3 position = fillFullPosition;
        position.x -= (fillFullScale.x - scale.x) * 0.5f;
        fill.localPosition = position;
    }
}
