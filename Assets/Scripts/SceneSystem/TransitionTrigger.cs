using UnityEngine;
using System.Collections;

/// <summary>
/// 場景轉換觸發器 - 簡化版,直接切換到目標場景
/// </summary>
public class TransitionTrigger : MonoBehaviour
{
    [Header("場景轉換設定")]
    [Tooltip("目標場景名稱")]
    public string targetSceneName = "Act2_RoomWakeup";

    [Tooltip("延遲時間（秒）")]
    public float delay = 0.5f;

    [Header("回調")]
    public UnityEngine.Events.UnityEvent onTransitionStart;
    public UnityEngine.Events.UnityEvent onTransitionComplete;

    private bool isTransitioning = false;

    /// <summary>
    /// 觸發轉場（公開方法，可從任何地方調用）
    /// </summary>
    public void TriggerTransition()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("⚠️ 轉場進行中，忽略此次請求");
            return;
        }

        StartCoroutine(TransitionSequence());
    }

    /// <summary>
    /// 立即轉場（無延遲）
    /// </summary>
    public void TriggerImmediately()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("⚠️ 轉場進行中，忽略此次請求");
            return;
        }

        StartCoroutine(TransitionSequence(0f));
    }

    /// <summary>
    /// 使用自訂延遲觸發轉場
    /// </summary>
    public void TriggerWithDelay(float customDelay)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("⚠️ 轉場進行中，忽略此次請求");
            return;
        }

        StartCoroutine(TransitionSequence(customDelay));
    }

    IEnumerator TransitionSequence(float? customDelay = null)
    {
        isTransitioning = true;
        float actualDelay = customDelay ?? delay;

        Debug.Log($"🎬 開始場景轉換... 延遲: {actualDelay}秒");
        Debug.Log($"📍 目標場景: {targetSceneName}");

        // 觸發開始回調
        onTransitionStart?.Invoke();

        // 等待延遲
        if (actualDelay > 0)
        {
            Debug.Log($"⏱️ 等待 {actualDelay} 秒...");
            yield return new WaitForSeconds(actualDelay);
        }

        // 檢查場景名稱
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("❌ 沒有設置目標場景名稱！");
            isTransitioning = false;
            yield break;
        }

        // 檢查場景是否在 Build Settings 中
        bool sceneExists = false;
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == targetSceneName)
            {
                sceneExists = true;
                break;
            }
        }

        if (!sceneExists)
        {
            Debug.LogError($"❌ 場景 '{targetSceneName}' 沒有加到 Build Settings!\n請到 File → Build Settings 添加場景");
            isTransitioning = false;
            yield break;
        }

        // 切換到新場景
        Debug.Log($"🎬 切換到場景: {targetSceneName}");
        SceneTransitionManager.LoadActScene(targetSceneName);
        
        // 觸發完成回調
        onTransitionComplete?.Invoke();
        isTransitioning = false;
    }

    /// <summary>
    /// 設置目標場景名稱（動態更改）
    /// </summary>
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        Debug.Log($"📍 目標場景已更改: {sceneName}");
    }

    /// <summary>
    /// 設置延遲時間（動態更改）
    /// </summary>
    public void SetDelay(float newDelay)
    {
        delay = newDelay;
        Debug.Log($"⏱️ 延遲時間已更改: {newDelay}秒");
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    [ContextMenu("🧪 測試場景轉換")]
    void TestTransition()
    {
        TriggerTransition();
    }

    [ContextMenu("🔍 檢查設置")]
    void CheckSettings()
    {
        Debug.Log("=== TransitionTrigger 設置檢查 ===");
        Debug.Log($"目標場景: {(string.IsNullOrEmpty(targetSceneName) ? "❌ 未設置" : targetSceneName)}");
        Debug.Log($"延遲時間: {delay}秒");
        Debug.Log("==============================");
    }
}
