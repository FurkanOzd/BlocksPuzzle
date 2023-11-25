using UnityEngine;

public class GridCell : MonoBehaviour
{
    public bool IsEmpty { get; private set; }
    
    public void ToggleCell(bool toggle)
    {
        IsEmpty = toggle;
    }
}