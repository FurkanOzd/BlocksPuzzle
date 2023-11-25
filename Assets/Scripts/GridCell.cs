using UnityEngine;

public class GridCell : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position,-Vector3.forward);
    }
}