using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    private int m_FoodAmout = 100;
    public static GameManager Instance{ get; private set; }
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public TurnManager TurnManager{ get; private set; }
    public UIDocument UIDoc;
    private Label m_FoodLabel;
    private int m_CurrentLevel = 1;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    private void Awake()
   {
       if (Instance != null)
       {
           Destroy(gameObject);
           return;
       }
      
       Instance = this;
   }
    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        NewLevel();

        m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = UIDoc.rootVisualElement.Q<Label>("GameOverMessage");

        m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_FoodLabel.text = "Food: " + m_FoodAmout;
        m_GameOverPanel.style.visibility = Visibility.Hidden;
    }
    public void OnTurnHappen()
    {
        ChangeFood(-1);
    }
    public void ChangeFood(int amout)
    {
        m_FoodAmout += amout;
        m_FoodLabel.text = "Food: " + m_FoodAmout;

        if(m_FoodAmout <= 0)
        {
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = "Game Over!\n\nYou traveled through " + m_CurrentLevel + " levels";
            Time.timeScale = 0;
        }
    }
    public void NewLevel()
    {
        BoardManager.Clean();
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        m_CurrentLevel += 1;
    }
}
