using UnityEngine;

public class GetTileElement : MonoBehaviour
{
    public Element tileElement = Element.None;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DetactElement();
    }

    void DetactElement()
    {
        RaycastHit hitTile;
        if (Physics.Raycast(transform.position, Vector3.down, out hitTile, 2f, LayerMask.GetMask("Tile")))
        {
            TileProperty tileScript = hitTile.collider.GetComponent<TileProperty>();
            if (tileScript != null)
            {
                tileElement = tileScript.element;
            }
        }
    }
}
