using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameplayBgm : MonoBehaviour
{
    [SerializeField] private LevelFlowController flow;   // 留空會自動找
    [SerializeField] private AudioClip combatMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip defeatMusic;

    private AudioSource src;
    private AudioClip current;

    void Awake() {
        src = GetComponent<AudioSource>();
        if (flow == null) flow = FindAnyObjectByType<LevelFlowController>();
    }

    void Update() {
        AudioClip want = PickClip();
        if (want != current) {           // 只有該換時才換,避免每幀重播
            current = want;
            src.clip = want;

            if (want == victoryMusic || want == defeatMusic) {
                src.loop = false;        // 勝利或失敗不循環
                src.volume = 1.0f;        // 勝利或失敗音樂音量固定
            } else {
                src.loop = true;         // 其他音樂（如戰鬥、放置）保持循環
                src.volume = 0.2f;        // 戰鬥或放置音樂音量較小
            }

            if (want != null) src.Play(); else src.Stop();
        }
    }

    AudioClip PickClip() {
        if (flow == null) return combatMusic;

        if (flow.CurrentState == LevelFlowController.LevelState.Result)
            return flow.CurrentResultOutcome == LevelFlowController.ResultOutcome.Victory
                 ? victoryMusic : defeatMusic;

        if (flow.CurrentState == LevelFlowController.LevelState.Combat) {
            return combatMusic;
        }
        return combatMusic;              // 放置階段:先沿用(你也可換別的)
    }
}