using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    public Tile[] ObstacleTile;
    public Tile[] DamageTile;
    public int MaxHealth = 3;
    private int m_HealthPoint;
    private Tile m_OriginalTile;
    private int X;
    public override void Init(Vector2Int cell)
    {
        X = Random.Range(0, ObstacleTile.Length);
        base.Init(cell);
        m_HealthPoint = MaxHealth;
        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, ObstacleTile[X]);
    }
    public override bool PlayerWantsToEnter()
    {
        PlayerController.Instance.Attack();
        m_HealthPoint -= PlayerController.Instance.m_Damage;
        if(m_HealthPoint < 0)
        {
            GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_OriginalTile);
            Destroy(gameObject);
            return true;
        }
        else if (m_HealthPoint == 1)
        {
            GameManager.Instance.BoardManager.SetCellTile(m_Cell, DamageTile[X]);
            return false;
        }
        else if(m_HealthPoint == 0)
        {
            GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_OriginalTile);
            Destroy(gameObject);
            return false;
        }
        else
        {
            return false;
        }
    }
}
