using UnityEngine;

public enum MoveDirection { Forward, Back, Left, Right, End }

public class PathDetactor : MonoBehaviour
{
    // 在 Inspector 視窗中設定這格要往哪走
    public MoveDirection nextDirection;

    // 方便視覺化：在編輯器畫面顯示一個箭頭
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 directionVector = GetVectorFromDirection(nextDirection);
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, directionVector * 0.8f);
    }

    public Vector3 GetVectorFromDirection(MoveDirection dir)
    {
        switch (dir)
        {
            case MoveDirection.Forward: return Vector3.forward; // 假設 Y 是高度，Z 是前
            case MoveDirection.Back: return Vector3.back;
            case MoveDirection.Left: return Vector3.left;
            case MoveDirection.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }

    public void findPrevPath(MoveDirection thisDir) // 
    {
        // Debug.Log("start find");
        nextDirection = thisDir;
        if(thisDir != MoveDirection.Forward)
        {
            // Debug.Log("ray up");
            RaycastHit prevPath;
            if (Physics.Raycast(transform.position, Vector3.forward, out prevPath, 2f, LayerMask.GetMask("Path")))
            {
                // Debug.Log("ray up find");
                PathDetactor prevPathScript = prevPath.collider.GetComponent<PathDetactor>();
                prevPathScript.findPrevPath(MoveDirection.Back); // 路徑與射線相反
            }
        }
        if(thisDir != MoveDirection.Right)
        {
            // Debug.Log("ray right");
            RaycastHit prevPath;
            if (Physics.Raycast(transform.position, Vector3.right, out prevPath, 2f, LayerMask.GetMask("Path")))
            {
                // Debug.Log("ray right find");
                PathDetactor prevPathScript = prevPath.collider.GetComponent<PathDetactor>();
                prevPathScript.findPrevPath(MoveDirection.Left); // 路徑與射線相反
            }
        }
        if(thisDir != MoveDirection.Back)
        {
            // Debug.Log("ray down");
            RaycastHit prevPath;
            if (Physics.Raycast(transform.position, Vector3.back, out prevPath, 2f, LayerMask.GetMask("Path")))
            {
                // Debug.Log("ray down find");
                PathDetactor prevPathScript = prevPath.collider.GetComponent<PathDetactor>();
                prevPathScript.findPrevPath(MoveDirection.Forward); // 路徑與射線相反
            }
        }
        if(thisDir != MoveDirection.Left)
        {
            // Debug.Log("ray left");
            RaycastHit prevPath;
            if (Physics.Raycast(transform.position, Vector3.left, out prevPath, 2f, LayerMask.GetMask("Path")))
            {
                // Debug.Log("ray left find");
                PathDetactor prevPathScript = prevPath.collider.GetComponent<PathDetactor>();
                prevPathScript.findPrevPath(MoveDirection.Right); // 路徑與射線相反
            }
        }
    }
}