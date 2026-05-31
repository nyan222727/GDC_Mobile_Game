using UnityEngine;

public sealed class MapTile : MonoBehaviour
{
    [SerializeField] private PlacementController controller;
    [SerializeField] private Transform occupant;

    public bool IsOccupied => occupant != null;

    public void SetOccupant(Transform newOccupant)
    {
        occupant = newOccupant;
    }

    private void Awake()
    {
        if (controller == null)
        {
            controller = FindAnyObjectByType<PlacementController>();
        }
    }
}
