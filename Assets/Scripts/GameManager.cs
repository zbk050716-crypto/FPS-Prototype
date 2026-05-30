using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Start,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ================= STATE =================
    public GameState CurrentState { get; private set; }

    // ================= UI =================
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;

    // ================= WAVE DATA =================
    private int level = 1;
    private int killCount = 0;
    private int targetEnemyCount = 1;

    [Header("Difficulty")]
    public float baseSpeedMultiplier = 0.8f;
    public float speedIncreasePerLevel = 0.05f;

    // ================= ENEMY REGISTRY =================
    private readonly List<EnemyAI> activeEnemies = new List<EnemyAI>();

    void Awake()
    {
        Instance = this;
        InitializeUI();
    }

    void InitializeUI()
    {
        if (startPanel == null)
            startPanel = GameObject.Find("StartPanel");
        if (gamePanel == null)
            gamePanel = GameObject.Find("GamePanel");
        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel");

        if (startPanel == null || gamePanel == null || gameOverPanel == null)
        {
            Debug.LogError("GameManager: 未找到所有UI Panel！请在场景中创建 StartPanel, GamePanel, GameOverPanel");
        }
    }

    void Start()
    {
        EnterState(GameState.Start);
    }

    // =====================================================
    // GAME STATE CONTROL（只管流程）
    // =====================================================

    public void EnterState(GameState newState)
    {
        CurrentState = newState;

        HideAllUI();

        switch (CurrentState)
        {
            case GameState.Start:
                if (startPanel != null) startPanel.SetActive(true);
                Time.timeScale = 0;
                break;

            case GameState.Playing:
                if (gamePanel != null) gamePanel.SetActive(true);
                Time.timeScale = 1;
                ResetGame();
                SpawnInitialEnemy();
                break;

            case GameState.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                Time.timeScale = 0;
                break;
        }
    }

    void HideAllUI()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // =====================================================
    // PUBLIC FLOW API（按钮调用）
    // =====================================================

    public void StartGame()
    {
        EnterState(GameState.Playing);
    }

    public void GameOver()
    {
        EnterState(GameState.GameOver);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ResetGame()
    {
        level = 1;
        killCount = 0;
        targetEnemyCount = 1;
        activeEnemies.Clear();
    }

    // =====================================================
    // WAVE SYSTEM（只管节奏）
    // =====================================================

    void SpawnInitialEnemy()
    {
        EnemyPool.Instance.GetEnemy();
    }

    public void OnEnemyKilled()
    {
        killCount++;

        UpdateWave();
        UpdateUI();
        MaintainEnemyCount();
    }

    void MaintainEnemyCount()
    {
        int activeCount = EnemyPool.Instance.GetActiveEnemyCount();

        while (activeCount < targetEnemyCount)
        {
            EnemyPool.Instance.GetEnemy();
            activeCount = EnemyPool.Instance.GetActiveEnemyCount();
        }
    }

    void UpdateWave()
    {
        int newTarget = 1 + (killCount / 5);

        if (newTarget > targetEnemyCount)
        {
            level++;
            targetEnemyCount = newTarget;

            ApplyDifficulty();
        }
    }

    void ApplyDifficulty()
    {
        float multiplier =
            baseSpeedMultiplier + (level - 1) * speedIncreasePerLevel;

        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                enemy.UpdateSpeed(multiplier);
        }
    }

    public float GetEnemySpeedMultiplier()
    {
        return baseSpeedMultiplier + (level - 1) * speedIncreasePerLevel;
    }

    // =====================================================
    // ENEMY REGISTRY（只管列表）
    // =====================================================

    public void RegisterEnemy(EnemyAI enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyAI enemy)
    {
        activeEnemies.Remove(enemy);
    }

    // =====================================================
    // UI
    // =====================================================

    void UpdateUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateLevelDisplay(level, killCount);
    }
}