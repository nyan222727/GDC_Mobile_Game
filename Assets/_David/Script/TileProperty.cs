using UnityEngine;

public enum Element { Fire, Ice, None }

public class TileProperty : MonoBehaviour
{
    public Element  element;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        element = Element.None;
    }

    // Update is called once per frame
    void Update()
    {
        switch(element){
            case Element.Fire:
                transform.position = new Vector3(transform.position.x, -0.2f, transform.position.z);
                break;
            case Element.Ice:
                transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);
                break;
            case Element.None:
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                break; 
        }
    }
}
