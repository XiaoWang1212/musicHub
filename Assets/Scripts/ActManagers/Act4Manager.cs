using UnityEngine;
using System.Collections;

/// <summary>
/// 第四幕管理器 - 離家場景
/// 包含主角向前走的動畫效果
/// </summary>
public class Act4Manager : BaseActManager
{
    [Header("主角移動動畫設定")]
    public SpriteRenderer protagonistRenderer;
    
    [Header("移動參數")]
    [Tooltip("向前移動的距離")]
    public float moveDistance = 4f;
    
    [Tooltip("移動動畫的持續時間")]
    public float moveDuration = 2f;
    
    [Tooltip("上跳的高度")]
    public float jumpHeight = 0.8f;
    
    [Tooltip("上跳的頻率（每秒幾次跳躍）")]
    public float jumpFrequency = 2f;
    
    [Header("動畫曲線")]
    [Tooltip("水平移動的緩動曲線")]
    public AnimationCurve moveEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Tooltip("垂直跳躍的緩動曲線")]
    public AnimationCurve jumpEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("劇情觸發設定")]
    public int walkAnimationTriggerIndex = -1; // -1 表示不自動觸發
    
    [Header("淡出設定")]
    [Tooltip("主角淡出開始的時間點（動畫完成度 0-1）")]
    public float fadeOutStartProgress = 0.7f; // 70% 時開始淡出
    
    [Tooltip("主角淡出持續時間")]
    public float protagonistFadeOutDuration = 1f;

    protected override string GetActName()
    {
        return "Act4";
    }
    
    protected override void Start()
    {
        base.Start();
        
        // 訂閱對話索引變化事件
        DialogueManager.OnDialogueIndexChanged += OnDialogueIndexChanged;
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // 取消訂閱
        DialogueManager.OnDialogueIndexChanged -= OnDialogueIndexChanged;
    }
    
    /// <summary>
    /// 對話索引變化處理 - 類似 Act2 的 case 系統
    /// </summary>
    void OnDialogueIndexChanged(int dialogueIndex)
    {
        Debug.Log($"📝 Act4 當前對話索引: {dialogueIndex}");
        
        // 根據對話索引觸發不同事件
        switch (dialogueIndex)
        {
            case var index when index == walkAnimationTriggerIndex:
                Debug.Log($"🚶‍♂️ 觸發主角行走動畫 (對話索引: {index})");
                PlayProtagonistWalkAnimation();
                break;
                
            case -1: // 對話結束標記
                Debug.Log("✅ Act4 對話結束");
                break;
                
            default:
                // 其他對話索引暫時不處理
                break;
        }
    }
    
    /// <summary>
    /// 播放主角向前走的動畫
    /// 包含平移和上跳的組合效果
    /// </summary>
    public void PlayProtagonistWalkAnimation()
    {
        if (protagonistRenderer == null)
        {
            Debug.LogWarning("⚠️ 主角 SpriteRenderer 未設定！請在 Inspector 中指定");
            return;
        }
        
        Debug.Log("🚶‍♂️ 開始播放主角向前走動畫...");
        StartCoroutine(WalkAnimationCoroutine());
    }
    
    /// <summary>
    /// 主角行走動畫協程
    /// </summary>
    IEnumerator WalkAnimationCoroutine()
    {
        Vector3 startPosition = protagonistRenderer.transform.position;
        Vector3 targetPosition = startPosition + new Vector3(moveDistance, 0f, 0f);
        
        float elapsed = 0f;
        bool fadeStarted = false;
        Color originalColor = protagonistRenderer.color;
        
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / moveDuration;
            
            // 水平移動 (使用緩動曲線)
            float horizontalProgress = moveEasing.Evaluate(progress);
            float currentX = Mathf.Lerp(startPosition.x, targetPosition.x, horizontalProgress);
            
            // 垂直跳躍 (正弦波模擬步行的上下起伏)
            float jumpProgress = elapsed * jumpFrequency;
            float currentY = startPosition.y + Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight * jumpEasing.Evaluate(Mathf.PingPong(jumpProgress, 1f));
            
            // 應用新位置
            protagonistRenderer.transform.position = new Vector3(currentX, currentY, startPosition.z);
            
            // 在動畫進行到一定程度時開始淡出
            if (progress >= fadeOutStartProgress && !fadeStarted)
            {
                fadeStarted = true;
                Debug.Log($"🌅 開始淡出主角 (進度: {progress:F2})");
                StartCoroutine(FadeOutProtagonist(originalColor));
            }
            
            yield return null;
        }
        
        // 確保最終位置準確
        protagonistRenderer.transform.position = new Vector3(targetPosition.x, startPosition.y, startPosition.z);
        
        Debug.Log("✅ 主角向前走動畫完成");
        
        // 動畫完成後，處理後續流程
        yield return StartCoroutine(HandleAnimationComplete());
    }
    
    /// <summary>
    /// 主角淡出效果
    /// </summary>
    IEnumerator FadeOutProtagonist(Color originalColor)
    {
        float elapsed = 0f;
        
        while (elapsed < protagonistFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / protagonistFadeOutDuration);
            
            Color currentColor = originalColor;
            currentColor.a = alpha;
            protagonistRenderer.color = currentColor;
            
            yield return null;
        }
        
        // 確保完全透明
        Color finalColor = originalColor;
        finalColor.a = 0f;
        protagonistRenderer.color = finalColor;
        
        Debug.Log("👻 主角已完全淡出");
    }
    
    /// <summary>
    /// 動畫完成後的處理
    /// </summary>
    IEnumerator HandleAnimationComplete()
    {
        // 等待淡出完成
        yield return new WaitForSeconds(protagonistFadeOutDuration);
        
        Debug.Log("🎬 主角已完全淡出，準備進入下一階段");
        
        // 檢查是否為最後一句對話
        bool isLastDialogue = IsLastDialogue();
        
        if (isLastDialogue)
        {
            Debug.Log("📖 這是最後一句對話，開始結束序列");
            // 直接開始 Act 結束序列
            StartCoroutine(ActEndingSequence());
        }
        else
        {
            Debug.Log("📖 自動播放下一句對話");
            // 自動繼續下一句對話
            if (dialogueManager != null)
            {
                dialogueManager.ContinueDialogue();
            }
        }
    }
    
    /// <summary>
    /// 檢查是否為最後一句對話
    /// </summary>
    bool IsLastDialogue()
    {
        if (actDialogueSequence == null || dialogueManager == null) 
            return true;
            
        int currentIndex = dialogueManager.GetCurrentDialogueIndex();
        var dialogueSequence = actDialogueSequence.ToDialogueSequence();
        
        return currentIndex >= dialogueSequence.dialogues.Count - 1;
    }

}
