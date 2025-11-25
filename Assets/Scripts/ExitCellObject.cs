using UnityEngine;
using UnityEngine.Tilemaps;

public class ExitCellObject : CellObject
{
    public Tile ExitTile;
    private Tile m_OriginalTile;
    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(coord);
        GameManager.Instance.BoardManager.SetCellTile(coord, ExitTile);
    }
    public override void PlayerEntered()
    {
        base.PlayerEntered();
        GameManager.Instance.NewLevel();
    }
}
