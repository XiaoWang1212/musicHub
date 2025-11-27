using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("載入模式")]
    [Tooltip("使用疊加載入（保留主場景的管理器）")]
    public bool useAdditiveLoading = true;

    [Header("轉場設定")]
    [Tooltip("淡出淡入持續時間")]
    public float fadeDuration = 1f;

    [Header("UI 遮罩")]
    public CanvasGroup fadePanel;

    [Header("Debug")]
    public bool showDebugLog = true;

    private static SceneTransitionManager instance;
    private bool isTransitioning = false;
    private string currentActScene = "";  // 記錄當前載入的劇情場景

    void Awake()
    {
        // 單例模式
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // 🔑 切換場景時不銷毀
            
            if (showDebugLog)
                Debug.Log("✅ SceneTransitionManager 已初始化（單例模式）");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 確保遊戲開始時遮罩是透明的
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 切換到指定劇情場景（使用 Additive Loading）
    /// </summary>
    public static void LoadActScene(string actSceneName)
    {
        if (instance != null && !instance.isTransitioning)
        {
            instance.StartCoroutine(instance.TransitionToActScene(actSceneName));
        }
    }

    /// <summary>
    /// 切換到下一幕
    /// </summary>
    public static void LoadNextAct()
    {
        if (instance == null || string.IsNullOrEmpty(instance.currentActScene))
        {
            Debug.LogWarning("⚠️ 沒有當前劇情場景，無法載入下一幕");
            return;
        }

        string currentScene = instance.currentActScene;
        
        // 假設場景命名為 Act1_xxx, Act2_xxx
        if (currentScene.StartsWith("Act"))
        {
            string numberPart = currentScene.Substring(3).Split('_')[0];
            if (int.TryParse(numberPart, out int actNumber))
            {
                int nextAct = actNumber + 1;
                
                // 尋找下一幕場景（可能有不同後綴）
                string nextSceneName = FindSceneByActNumber(nextAct);
                
                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    LoadActScene(nextSceneName);
                }
                else
                {
                    Debug.LogWarning($"⚠️ 找不到第 {nextAct} 幕的場景");
                }
            }
        }
    }

    IEnumerator TransitionToActScene(string actSceneName)
    {
        isTransitioning = true;
        
        if (showDebugLog)
            Debug.Log($"🎬 開始切換到劇情場景: {actSceneName}");

        // 階段 1: 淡出
        yield return StartCoroutine(FadeOut());

        // 階段 2: 卸載舊的劇情場景（如果有）
        if (!string.IsNullOrEmpty(currentActScene))
        {
            if (showDebugLog)
                Debug.Log($"📤 卸載舊場景: {currentActScene}");
            
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(currentActScene);
            
            while (!unloadOperation.isDone)
            {
                yield return null;
            }
            
            // 清理記憶體
            yield return Resources.UnloadUnusedAssets();
            
            if (showDebugLog)
                Debug.Log($"✅ 舊場景已卸載");
        }

        // 階段 3: 載入新的劇情場景（Additive 模式）
        if (showDebugLog)
            Debug.Log($"📥 載入新場景: {actSceneName}");
        
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(actSceneName, LoadSceneMode.Additive);

        while (!loadOperation.isDone)
        {
            // 可以在這裡顯示載入進度
            if (showDebugLog && loadOperation.progress % 0.1f < 0.01f)
                Debug.Log($"載入進度: {(loadOperation.progress * 100):F0}%");
            
            yield return null;
        }

        // 設置新場景為 Active Scene（方便 GameObject.Find 等操作）
        Scene newScene = SceneManager.GetSceneByName(actSceneName);
        if (newScene.isLoaded)
        {
            SceneManager.SetActiveScene(newScene);
            currentActScene = actSceneName;
            
            if (showDebugLog)
                Debug.Log($"✅ 場景載入完成: {actSceneName}");
        }
        else
        {
            Debug.LogError($"❌ 場景載入失敗: {actSceneName}");
        }

        // 階段 4: 淡入
        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
        
        if (showDebugLog)
            Debug.Log($"🎉 場景轉換完成!");
    }

    IEnumerator FadeOut()
    {
        if (fadePanel == null)
        {
            yield return new WaitForSeconds(fadeDuration);
            yield break;
        }

        fadePanel.blocksRaycasts = true;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    IEnumerator FadeIn()
    {
        if (fadePanel == null)
        {
            yield return new WaitForSeconds(fadeDuration);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
    }

    /// <summary>
    /// 尋找特定幕數的場景名稱
    /// </summary>
    static string FindSceneByActNumber(int actNumber)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneName.StartsWith($"Act{actNumber}"))
            {
                return sceneName;
            }
        }
        return null;
    }

    /// <summary>
    /// 獲取當前劇情場景名稱
    /// </summary>
    public static string GetCurrentActScene()
    {
        return instance != null ? instance.currentActScene : "";
    }

    public static bool IsTransitioning()
    {
        return instance != null && instance.isTransitioning;
    }

    [ContextMenu("🔍 檢查當前載入的場景")]
    void CheckLoadedScenes()
    {
        Debug.Log("=== 當前載入的場景 ===");
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            string activeMarker = scene == SceneManager.GetActiveScene() ? " (Active)" : "";
            Debug.Log($"{i}: {scene.name}{activeMarker}");
        }
        Debug.Log($"當前劇情場景: {currentActScene}");
        Debug.Log("======================");
    }
}
