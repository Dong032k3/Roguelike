using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

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
        PlayerController.Instance.m_IsAttack = true;
        PlayerController.Instance.Attack();
        m_CurrentHealth -= PlayerController.Instance.m_Damage;
        TakeDamageEnemy();

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
        m_Animator.SetBool("Moving",true);

        return true;
    }

    void TurnHappened()
    {
        var playerCell = GameManager.Instance.PlayerController.Cell;
        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;
        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);
        
        if ((xDist == 0 && absYDist == 1)
        || (yDist == 0 && absXDist == 1))
        {
            PlayerController.Instance.m_IsAttack = false;
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
        if(PlayerController.Instance.m_IsAttack == false)
        {
            GameManager.Instance.ChangeFood(-m_DamageEnemy);
            m_Animator.SetTrigger("Attack");
            Debug.Log("Enemy Attack");
        }
       
    }
    public void TakeDamageEnemy()
    {
        if(PlayerController.Instance.m_IsAttack == true)
        {
            m_Animator.Play("EnemyDamage");
            StartCoroutine(ResetTakeDamageEnemy());
        }
        else
        {
            return;
        }
    }
    IEnumerator ResetTakeDamageEnemy()
    {
        yield return new WaitForSeconds(0.2f);
        m_Animator.Play("EnemyIdle");
        
    }
    public void Update() {
        if(m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTargetEnemy, MoveSpeedEnemy * Time.deltaTime);
            // Kiểm tra nếu đã đến đích
            if(transform.position == m_MoveTargetEnemy)
            {
                m_IsMoving = false;
                m_Animator.SetBool("Moving", false); 
            }
        }
    }
}