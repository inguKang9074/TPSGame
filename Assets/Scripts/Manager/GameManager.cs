using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    public bool isGameover { get; private set; }
    public int gold { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

    public void AddGold(int gold)
    {
        if (!isGameover)
        {
            this.gold += gold;
            UIManager.Instance.UpdateGoldText(this.gold);
        }
    }
    
    public void EndGame()
    {
        isGameover = true;
        UIManager.Instance.SetActiveGameoverUI(true);
    }

    public void RestartGame()
    {
        isGameover = false;
    }
}