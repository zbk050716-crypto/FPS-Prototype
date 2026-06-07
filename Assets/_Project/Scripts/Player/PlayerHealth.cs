using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("血量设置")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public TMP_Text healthText;
    public GameObject gameOverPanel;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        UpdateUI();

        // 开始时隐藏GameOver界面
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth;
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("玩家死亡");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}