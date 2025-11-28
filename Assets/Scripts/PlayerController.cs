using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Singleton: Biến static để truy cập PlayerController từ bất kỳ đâu
    public static PlayerController Instance;

    private BoardManager m_Board;       // Tham chiếu đến hệ thống quản lý lưới (Grid)
    private Vector2Int m_CellPosition;  // Tọa độ logic của người chơi trên lưới (ví dụ: 1, 2)
    public Vector2Int Cell => m_CellPosition; // Property để các script khác lấy tọa độ
    
    private bool m_IsGameOver;          // Trạng thái game đã kết thúc hay chưa
    private Animator m_Animator;        
    
    private bool m_IsMoving;           
    private Vector3 m_MoveTarget;       // Vị trí thực tế trong không gian 3D mà nhân vật cần đến
    private int MoveSpeed = 5;       
    
    [SerializeField] public int m_Damage = 1; 

    public void Awake()
    {
        m_Animator = GetComponent<Animator>();

        // --- Mẫu Singleton ---
        // Nếu đã có một Instance tồn tại rồi thì hủy object mới này đi để tránh trùng lặp
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Nếu chưa có thì gán Instance là object này
        Instance = this;
    }

    // Hàm xử lý khi thua game
    public void GameOver()
    {
        m_IsGameOver = true;
        Time.timeScale = 0; 
    }

    // Khởi tạo lại trạng thái khi bắt đầu game mới
    public void Init()
    {
        m_IsGameOver = false;
        Time.timeScale = 1;
    }

    // Hàm sinh ra người chơi tại vị trí chỉ định
    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        // Di chuyển ngay lập tức đến ô (Idle = true nghĩa là không chạy animation trượt)
        MoveTo(cell, true);
    }
    
    // Hàm xử lý logic di chuyển đến một ô
    public void MoveTo(Vector2Int cell , bool Idle)
    {
        m_CellPosition = cell; // Cập nhật tọa độ lưới mới

        if(Idle)
        {
            // Di chuyển tức thời (Teleport) - Dùng khi mới Spawn
            m_IsMoving = false;
            transform.position = m_Board.CellToWorld(m_CellPosition);
        }
        else
        {
            // Bắt đầu di chuyển mượt (Animation) - Dùng khi người chơi bấm nút
            m_IsMoving = true;
            // Lấy vị trí thế giới thực của ô đích để nhân vật trượt tới
            m_MoveTarget = m_Board.CellToWorld(m_CellPosition);
        }
        
        // Cập nhật trạng thái cho Animator (để chuyển từ Idle sang Run/Walk)
        m_Animator.SetBool("Moving", m_IsMoving);
    }

  
    private void Update()
    {
        // 1. Nếu game đã kết thúc
        if (m_IsGameOver)
        {
            // Chờ người chơi bấm Enter để chơi lại
            if(Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return; // Thoát khỏi hàm Update, không xử lý di chuyển nữa
        }

        // 2. Xử lý Input (Nhập liệu từ bàn phím)
        Vector2Int newCellTarget = m_CellPosition; // Giả định đích đến là vị trí hiện tại
        bool hasMoved = false; // Cờ kiểm tra xem có bấm phím không

        // Kiểm tra các phím mũi tên và tính toán tọa độ ô mới
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

        // 3. Logic kiểm tra tính hợp lệ của nước đi
        if(hasMoved)
        {
            // Lấy dữ liệu của ô mà người chơi muốn đi vào
            BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

            // Kiểm tra: Ô đó có tồn tại và có cho phép đi qua (Passable) không?
            if(cellData != null && cellData.Passable)
            {
                    // Quan trọng: Báo cho GameManager biết là người chơi đã thực hiện 1 lượt
                    GameManager.Instance.TurnManager.Tick();

                    // Trường hợp 1: Ô trống, không có vật thể gì
                    if(cellData.ContainedObject == null)
                    {
                        MoveTo(newCellTarget, false); // Di chuyển mượt đến đó
                    }
                    // Trường hợp 2: Có vật thể (Quái, Item) và vật thể đó cho phép người chơi đi vào (hoặc tương tác)
                    else if(cellData.ContainedObject.PlayerWantsToEnter())
                    {
                        MoveTo(newCellTarget, false); // Di chuyển
                        // Gọi hàm sự kiện khi người chơi bước vào ô chứa vật thể này
                        cellData.ContainedObject.PlayerEntered();
                    }
            }
        }

        // 4. Thực hiện Animation di chuyển (Mỗi khung hình)
        if (m_IsMoving)
        {
            // Dùng MoveTowards để dịch chuyển nhân vật từ từ đến vị trí đích
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, Time.deltaTime * MoveSpeed);

            // Nếu đã đến nơi (vị trí hiện tại trùng với đích)
            if(transform.position == m_MoveTarget)
            {
                m_IsMoving = false; // Kết thúc di chuyển
                m_Animator.SetBool("Moving", m_IsMoving); // Tắt animation chạy

                // Kiểm tra lại ô vừa đến, nếu có vật thể thì kích hoạt sự kiện "Đã bước vào"
                // (Ví dụ: Nhặt item sau khi đã đứng hẳn vào ô)
                var cellData = m_Board.GetCellData(m_CellPosition);
                if(cellData.ContainedObject != null)
                {
                    cellData.ContainedObject.PlayerEntered();
                }
            }
            return;
        }
    }

    public void Attack()
    {
        m_Animator.SetTrigger("Attack");
    }
}