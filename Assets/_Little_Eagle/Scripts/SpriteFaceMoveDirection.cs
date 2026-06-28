using UnityEngine;

public class SpriteFaceMoveDirection : MonoBehaviour
{
    public enum Facing { Right, Left }

    [Header("要翻的容器（裝著 本體+配件 的 Visual 物件）")]
    public Transform visual;

    [Header("這些圖『原本』頭朝哪邊")]
    [Tooltip("大部分朝右就選 Right；少數原圖朝左的那隻改成 Left")]
    public Facing defaultFacing = Facing.Right;

    [Header("水平移動要超過這個量才更新朝向（避免沿Z上下走時亂翻）")]
    public float deadZone = 0.001f;

    private EnemyController enemy;
    private Vector3 baseScale;     // 記住容器原本的縮放大小，翻面只改正負號
    private Vector3 lastPos;

    void Start()
    {
        enemy = GetComponentInParent<EnemyController>();
        if (visual == null) visual = transform;   // 沒指定就用自己
        baseScale = visual.localScale;
        lastPos = transform.position;
    }

    void LateUpdate()
    {
        // 取得水平方向：優先用怪物的目標點(路徑下一格)，否則用實際位移
        float dx;
        if (enemy != null)
            dx = enemy.targetPosition.x - transform.position.x;
        else
        {
            dx = transform.position.x - lastPos.x;
            lastPos = transform.position;
        }

        if (Mathf.Abs(dx) <= deadZone) return;          // 幾乎沒水平分量 → 維持上一個朝向

        bool movingRight   = dx > 0f;
        bool artFacesRight = defaultFacing == Facing.Right;
        bool flip = movingRight != artFacesRight;        // 想朝向 != 原圖朝向 → 鏡像

        Vector3 s = baseScale;
        s.x = Mathf.Abs(baseScale.x) * (flip ? -1f : 1f); // 只翻正負號，大小不變
        visual.localScale = s;
    }
}