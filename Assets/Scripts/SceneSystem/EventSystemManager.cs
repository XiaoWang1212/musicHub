using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// EventSystem 管理器 - 確保場景中只有一個 EventSystem
/// 放在 MainScene 的 EventSystem 上
/// </summary>
[RequireComponent(typeof(EventSystem))]
public class EventSystemManager : MonoBehaviour
{
    private static EventSystemManager instance;

    void Awake()
    {
        // 如果已經有實例,銷毀自己
        if (instance != null && instance != this)
        {
            Debug.Log("⚠️ 發現重複的 EventSystem,已移除");
            Destroy(gameObject);
            return;
        }

        // 設定為唯一實例並保持存在
        instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("✅ EventSystem 已初始化並設為持久物件");
    }

    void OnDestroy()
    {
        // 清空實例引用
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// 檢查並移除場景中多餘的 EventSystem
    /// 在載入新場景後呼叫
    /// </summary>
    public static void CleanupDuplicates()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        
        if (eventSystems.Length > 1)
        {
            Debug.Log($"⚠️ 發現 {eventSystems.Length} 個 EventSystem,正在清理...");

            // 保留第一個(MainScene 的),移除其他的
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Debug.Log($"  移除重複的 EventSystem: {eventSystems[i].gameObject.name}");
                Destroy(eventSystems[i].gameObject);
            }
        }
    }
}
