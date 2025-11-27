using UnityEngine;

/// <summary>
/// AudioListener 管理器 - 確保場景中只有一個 AudioListener
/// 放在每個 Act 場景的 Main Camera 上
/// </summary>
[RequireComponent(typeof(Camera))]
public class AudioListenerManager : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("是否在場景啟動時自動檢查並添加 AudioListener")]
    public bool autoAddIfMissing = true;

    void Awake()
    {
        if (autoAddIfMissing)
        {
            EnsureAudioListener();
        }
    }

    /// <summary>
    /// 確保場景中有且只有一個 AudioListener
    /// </summary>
    void EnsureAudioListener()
    {
        // 尋找場景中所有的 AudioListener
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);

        if (listeners.Length == 0)
        {
            // 沒有 AudioListener,添加一個到這個 Camera 上
            AudioListener listener = gameObject.AddComponent<AudioListener>();
            Debug.Log($"✅ 已自動添加 AudioListener 到 {gameObject.name}");
        }
        else if (listeners.Length == 1)
        {
            // 只有一個,正常情況
            Debug.Log($"✅ 場景中有一個 AudioListener: {listeners[0].gameObject.name}");
        }
        else
        {
            // 有多個 AudioListener,移除這個 Camera 上的(如果有)
            AudioListener myListener = GetComponent<AudioListener>();
            if (myListener != null)
            {
                Destroy(myListener);
                Debug.Log($"⚠️ 移除 {gameObject.name} 上的重複 AudioListener (場景中共有 {listeners.Length} 個)");
            }
        }
    }

    [ContextMenu("🔍 檢查 AudioListener 狀態")]
    void CheckAudioListeners()
    {
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        Debug.Log("=== AudioListener 檢查 ===");
        Debug.Log($"場景中的 AudioListener 數量: {listeners.Length}");
        
        for (int i = 0; i < listeners.Length; i++)
        {
            Debug.Log($"  {i + 1}. {listeners[i].gameObject.name}");
        }
        
        AudioListener myListener = GetComponent<AudioListener>();
        Debug.Log($"當前 Camera 有 AudioListener: {(myListener != null ? "✅" : "❌")}");
        Debug.Log("========================");
    }

    [ContextMenu("🔧 強制添加 AudioListener")]
    void ForceAddAudioListener()
    {
        AudioListener myListener = GetComponent<AudioListener>();
        if (myListener == null)
        {
            gameObject.AddComponent<AudioListener>();
            Debug.Log($"✅ 已添加 AudioListener 到 {gameObject.name}");
        }
        else
        {
            Debug.Log($"⚠️ {gameObject.name} 已經有 AudioListener");
        }
    }
}
