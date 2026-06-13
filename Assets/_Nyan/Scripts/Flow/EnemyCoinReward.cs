using UnityEngine;

public sealed class EnemyCoinReward : MonoBehaviour
{
    [SerializeField] private int rewardAmount = 5;

    public int RewardAmount => Mathf.Max(0, rewardAmount);
}
