using UnityEngine;

public class SpriteWalkAnimation : MonoBehaviour
{
    public SpriteRenderer sr;             // 角色的 Sprite 子物件
    public Sprite[] frames;               // 走路圖，依播放順序
    public float frameInterval = 0.2f;    // 幾秒換一張（越小越快）

    private int index;
    private float timer;

    void Update()
    {
        if (sr == null || frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameInterval)
        {
            timer -= frameInterval;
            index = (index + 1) % frames.Length;   // 循環
            sr.sprite = frames[index];
        }
    }
}