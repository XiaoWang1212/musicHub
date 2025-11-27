using UnityEngine;

/// <summary>
/// 遊戲初始化器 - 自動載入第一幕
/// 放在 MainScene 中
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("首次載入設定")]
    [Tooltip("遊戲開始時要載入的第一幕場景名稱")]
    public string firstActScene = "Act1_MusicbookDrop";

    [Tooltip("首次載入前的延遲（秒）")]
    public float initialDelay = 0.5f;

    void Start()
    {
        Debug.Log("🎮 遊戲初始化...");
        
        // 延遲後載入第一幕
        Invoke(nameof(LoadFirstAct), initialDelay);
    }

    void LoadFirstAct()
    {
        Debug.Log($"📥 載入第一幕: {firstActScene}");
        SceneTransitionManager.LoadActScene(firstActScene);
    }
}