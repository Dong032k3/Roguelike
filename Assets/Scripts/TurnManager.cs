using UnityEngine;

public class TurnManager 
{
    public event System.Action OnTick;
    private int m_TurnCount;
    void Start()
    {
        m_TurnCount = 1;
    }
    public void Tick()
    {
        OnTick?.Invoke();
        m_TurnCount++;
        Debug.Log("Turn: " + m_TurnCount);
    }

}
