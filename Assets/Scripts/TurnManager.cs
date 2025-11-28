using UnityEngine;

public class TurnManager
{
    // Sự kiện được kích hoạt khi một lượt xảy ra (tick)
    public event System.Action OnTick;

    // Bộ đếm lượt để theo dõi số lượt đã đi
    private int m_TurnCount;

    void Start()
    {
        m_TurnCount = 1;
    }

    // Gọi Tick khi một lượt của người chơi kết thúc
    public void Tick()
    {
        OnTick?.Invoke();
        m_TurnCount++;
        Debug.Log("Turn: " + m_TurnCount);
    }

}
