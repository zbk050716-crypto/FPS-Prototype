using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI levelText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (levelText == null)
        {
            levelText = GetComponent<TextMeshProUGUI>();
        }

        if (levelText == null)
        {
            Debug.LogError("UIManager: 找不到TextMeshProUGUI组件！请手动拖入或确保Level Text对象上有该组件。");
        }
    }

    public void UpdateLevelDisplay(int level, int killCount)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}\nKills: {killCount}";
        }
    }
}
