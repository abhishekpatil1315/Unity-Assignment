using UnityEngine;

public interface IAI
{
    void TakeTurn();
    bool IsTakingTurn();
    Vector2Int GetGridPosition();
    void Initialize(int gridX, int gridY);
}
