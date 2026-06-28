using UnityEngine;

public class FlipSpriteFaceMoveDirection : MonoBehaviour
{
    public enum Facing { Right, Left }

    [Header("要翻面的 SpriteRenderer（本體；有配件就一起拖入。血條不要放進來！）")]
    public SpriteRenderer[] renderers;

    [Header("這些圖『原本』頭朝哪邊")]
    [Tooltip("大部分朝右就 Right；少數原圖朝左的那隻改 Left")]
    public Facing defaultFacing = Facing.Right;

    [Header("水平移動要超過這個量才更新朝向（避免沿Z上下走時亂翻）")]
    public float deadZone = 0.001f;

    private EnemyController enemy;
    private Vector3 lastPos;

    void Start()
    {
        enemy = GetComponentInParent<EnemyController>();
        lastPos = transform.position;
    }

    void LateUpdate()
    {
        // 取得水平方向：優先用怪物目標點(路徑下一格)，否則用實際位移
        float dx;
        if (enemy != null)
            dx = enemy.targetPosition.x - transform.position.x;
        else
        {
            dx = transform.position.x - lastPos.x;
            lastPos = transform.position;
        }

        if (Mathf.Abs(dx) <= deadZone) return;     // 幾乎沒水平分量 → 維持上一個朝向

        bool movingRight   = dx > 0f;
        bool artFacesRight = defaultFacing == Facing.Right;
        bool flip = movingRight != artFacesRight;   // 想朝向 != 原圖朝向 → 翻

        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].flipX = flip;
    }
}