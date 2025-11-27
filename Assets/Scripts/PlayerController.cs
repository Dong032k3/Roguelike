using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    public Vector2Int Cell => m_CellPosition;
    private bool m_IsGameOver;
    private Animator m_Animator;
    private bool m_IsMoving;
    private Vector3 m_MoveTarget;
    private int MoveSpeed = 5;
    [SerializeField] public int m_Damage = 1;

    public void Awake()
    {
        m_Animator = GetComponent<Animator>();
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    public void GameOver()
    {
        m_IsGameOver = true;
        Time.timeScale = 0;
    }
    public void Init()
    {
        m_IsGameOver = false;
    }
    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        MoveTo(cell,true);
    }
    
    public void MoveTo(Vector2Int cell , bool Idle)
    {
        m_CellPosition = cell;
        if(Idle)
        {
            m_IsMoving = false;
            transform.position = m_Board.CellToWorld(m_CellPosition);
        }
        else
        {
            m_IsMoving = true;
            m_MoveTarget = m_Board.CellToWorld(m_CellPosition);
        }
        m_Animator.SetBool("Moving", m_IsMoving);
    }

  
    private void Update()
    {
        if (m_IsGameOver)
        {
            if(Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return;
        }
        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;

        if(Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1;
            hasMoved = true;
        }
        else if(Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1;
            hasMoved = true;
        }

        if(hasMoved)
        {
            //check if the new position is passable, then move there if it is.
            BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

            if(cellData != null && cellData.Passable)
            {
                    GameManager.Instance.TurnManager.Tick();
                    if(cellData.ContainedObject == null)
                    {
                        MoveTo(newCellTarget, false);
                    }else if(cellData.ContainedObject.PlayerWantsToEnter())
                        {
                            MoveTo(newCellTarget, false);
                            cellData.ContainedObject.PlayerEntered();
                    }
            }
        }
        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, Time.deltaTime * MoveSpeed);
            if(transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
                m_Animator.SetBool("Moving", m_IsMoving);

                var cellData = m_Board.GetCellData(m_CellPosition);
                if(cellData.ContainedObject != null)
                {
                    cellData.ContainedObject.PlayerEntered();
                }
            }
            return;
        }
    }
    public void TakeDamage()
    {
        m_Animator.SetTrigger("Attack");
    }
}