using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy : CellObject
{
    public int Health = 3;
      
    private int m_CurrentHealth;
    private Animator m_Animator;
    private bool m_IsMoving;
    private float MoveSpeedEnemy = 5f;
    private Vector3 m_MoveTargetEnemy;
    [SerializeField] public int m_DamageEnemy = 1;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        GameManager.Instance.TurnManager.OnTick += TurnHappened;
    }

    private void OnDestroy()
    {
        GameManager.Instance.TurnManager.OnTick -= TurnHappened;
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = Health;
    }

    public override bool PlayerWantsToEnter()
    {
        PlayerController.Instance.Attack();
        m_CurrentHealth -= PlayerController.Instance.m_Damage;

        if (m_CurrentHealth < 0)
        {
            Destroy(gameObject);
            return true;
        }else if( m_CurrentHealth == 0)
        {
            Destroy(gameObject);
            return false;
        }
        else
        {
            return false;
        }

        
    }

    bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell =  board.GetCellData(coord);

        if (targetCell == null
            || !targetCell.Passable
            || targetCell.ContainedObject != null)
        {
            return false;
        }
        //remove enemy from current cell
        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;
        
        //add it to the next cell
        targetCell.ContainedObject = this;
        m_Cell = coord;
        m_MoveTargetEnemy = board.CellToWorld(m_Cell);
        m_IsMoving = true;

        return true;
    }

    void TurnHappened()
    {
        //We added a public property that return the player current cell!
        var playerCell = GameManager.Instance.PlayerController.Cell;

        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);
        bool m_Action = false;
        if(m_Action){

            if ((xDist == 0 && absYDist == 2)
            || (yDist == 0 && absXDist == 2))
            {
                m_Action = false;
                m_IsMoving = false;
                m_Animator.SetBool("Moving",false);
            }
        }
        else 
        {
            m_Action = true;
            if ((xDist == 0 && absYDist == 1)
            || (yDist == 0 && absXDist == 1))
            {
                //we are adjacent to the player, attack!
                AttackPlayer();
            }
            else
            {
                if (absXDist > absYDist)
                {
                    if (!TryMoveInX(xDist))
                    {
                        TryMoveInY(yDist);
                    }
                }
                else
                {
                    if (!TryMoveInY(yDist))
                    {
                        TryMoveInX(xDist);
                    }
                }
            }
        }
    }

    bool TryMoveInX(int xDist)
    {
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }
        return MoveTo(m_Cell + Vector2Int.left);
    }

    bool TryMoveInY(int yDist)
    {
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }
        return MoveTo(m_Cell + Vector2Int.down);
    }
    public void AttackPlayer()
    {
        GameManager.Instance.ChangeFood(-m_DamageEnemy);
        m_Animator.SetTrigger("Attack");
    }
    public void Update() {
        if(m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTargetEnemy, MoveSpeedEnemy * Time.deltaTime);
            // Kiểm tra nếu đã đến đích
            if(GameManager.Instance.PlayerController.Cell.x == m_MoveTargetEnemy.x 
            && Mathf.Abs(GameManager.Instance.PlayerController.Cell.y - m_MoveTargetEnemy.y) == 1
            || GameManager.Instance.PlayerController.Cell.y == m_MoveTargetEnemy.y 
            && Mathf.Abs(GameManager.Instance.PlayerController.Cell.x - m_MoveTargetEnemy.x) == 1)
            {
                m_IsMoving = false;
                m_Animator.SetBool("Moving", false); 
            }
        }
    }
}