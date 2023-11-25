using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Level Difficulty")]

public class Difficulty : ScriptableObject
{
    public int maxBoardSize;
    public int minBoardSize;

    public int maxPieceCount;
    public int minPieceCount;
}
