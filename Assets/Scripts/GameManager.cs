using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public int m_FoodAmout = 100;

    // Singleton để truy cập GameManager từ bất kỳ đâu
    public static GameManager Instance { get; private set; }

    // Tham chiếu tới BoardManager để quản lý map
    public BoardManager BoardManager;

    // Tham chiếu tới controller của player
    public PlayerController PlayerController;

    // TurnManager quản lý lượt đi; được khởi tạo trong Start
    public TurnManager TurnManager { get; private set; }

    // UI Document (UI Toolkit) chứa các element hiển thị
    public UIDocument UIDoc;
    public Label m_FoodLabel;

    // Level hiện tại (số tầng đã đi qua)
    private int m_CurrentLevel = 1;

    // Panel và label dùng khi Game Over
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        // Thiết lập Singleton
        Instance = this;
    }
    void Start()
    {
        // Khởi tạo TurnManager (không phụ thuộc vào MonoBehaviour)
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        // Thiết lập UI (lấy các element từ UIDocument)
        m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = UIDoc.rootVisualElement.Q<Label>("GameOverMessage");

        // Bắt đầu game mới
        StartNewGame();
    }
    public void StartNewGame()
    {
        // Ẩn panel Game Over (nếu đang hiển thị)
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        // Đặt lại trạng thái game cơ bản
        m_CurrentLevel = 1;
        m_FoodAmout = 20;
        m_FoodLabel.text = "Food: " + m_FoodAmout;

        // Xoá map cũ rồi khởi tạo map mới
        BoardManager.Clean();
        BoardManager.Init();

        // Khởi tạo player và spawn tại ô (1,1)
        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

    }
    public void OnTurnHappen()
    {
        // Mỗi lượt giảm 1 food (chi phí đi lại)
        ChangeFood(-1);
    }
    public void ChangeFood(int amout)
    {
        // Cập nhật giá trị food và UI
        m_FoodAmout += amout;
        m_FoodLabel.text = "Food: " + m_FoodAmout;

        // Nếu hết food -> game over
        if (m_FoodAmout <= 0)
        {
            PlayerController.GameOver();
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_FoodLabel.text = "Food: 0";
            m_GameOverMessage.text = "Game Over!\n\nYou traveled through " + m_CurrentLevel + " levels";
        }
    }
    public void NewLevel()
    {
        // Tăng level và khởi tạo map mới
        m_CurrentLevel += 1;

        // Nếu muốn tăng kích thước map ở level mới, có thể bật 2 dòng dưới
        // BoardManager.Width += 2;
        // BoardManager.Height += 2;

        BoardManager.Clean();
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
    }
}
