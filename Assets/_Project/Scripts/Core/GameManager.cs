using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Start,
    Playing,
    Pause,
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
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    public GameObject crosshair;
    public GameObject weaponInfoPanel;

    // ================= WAVE DATA =================
    private int level = 1;
    private int killCount = 0;
    private int targetEnemyCount = 1;

    public static Action<int, int> OnKillCountChanged;

    [Header("Difficulty")]
    public float baseSpeedMultiplier = 0.8f;
    public float speedIncreasePerLevel = 0.05f;

    // ================= ENEMY REGISTRY =================
    private readonly List<EnemyAI> activeEnemies = new List<EnemyAI>();

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        EnemyAI.OnEnemyKilled += HandleEnemyKilled;
    }

    void OnDisable()
    {
        EnemyAI.OnEnemyKilled -= HandleEnemyKilled;
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
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case GameState.Playing:
                if (gamePanel != null) gamePanel.SetActive(true);
                if (crosshair != null) crosshair.SetActive(true);
                if (weaponInfoPanel != null) weaponInfoPanel.SetActive(true);
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case GameState.Pause:
                if (pausePanel != null) pausePanel.SetActive(true);
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case GameState.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }

    void HideAllUI()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (crosshair != null) crosshair.SetActive(false);
        if (weaponInfoPanel != null) weaponInfoPanel.SetActive(false);
    }

    // =====================================================
    // PUBLIC FLOW API（按钮调用）
    // =====================================================

    public void StartGame()
    {
        ResetGame();
        ResetPlayer();
        OnKillCountChanged?.Invoke(level, killCount);
        SpawnInitialEnemy();
        EnterState(GameState.Playing);
    }

    void ResetPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
                health.ResetHealth();
        }
    }

    public void GameOver()
    {
        EnterState(GameState.GameOver);
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            EnterState(GameState.Pause);
        else if (CurrentState == GameState.Pause)
            EnterState(GameState.Playing);
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Pause)
            EnterState(GameState.Playing);
    }

    public void ReturnToMainMenu()
    {
        ReturnAllEnemies();
        ResetGame();
        EnterState(GameState.Start);
    }

    void ReturnAllEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] != null)
                activeEnemies[i].ReturnToPool();
        }
        activeEnemies.Clear();
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

    public void QuitGame()
    {
        Application.Quit();
    }

    // =====================================================
    // WAVE SYSTEM（只管节奏）
    // =====================================================

    void SpawnInitialEnemy()
    {
        if (EnemyPool.Instance.GetEnemy() == null)
            Debug.LogError("[GameManager] 对象池已耗尽，无法生成初始敌人");
    }

    public void HandleEnemyKilled()
    {

        Debug.Log("HandleEnemyKilled 被调用");
        killCount++;

        UpdateWave();
        MaintainEnemyCount();

        OnKillCountChanged?.Invoke(level, killCount);
    }

    void MaintainEnemyCount()
    {
        int activeCount = EnemyPool.Instance.GetActiveEnemyCount();

        while (activeCount < targetEnemyCount)
        {
            if (EnemyPool.Instance.GetEnemy() == null)
                break;
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
}