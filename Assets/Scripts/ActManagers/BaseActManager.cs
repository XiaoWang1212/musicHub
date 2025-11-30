using UnityEngine;
using System.Collections;

/// <summary>
/// 基礎 Act 管理器 - 提供所有 Act 的通用功能
/// 包含：對話管理、背景淡入淡出、場景轉換
/// 其他 Act 可以繼承此類並擴展特殊功能
/// </summary>
public class BaseActManager : MonoBehaviour
{
    [Header("對話系統")]
    public DialogueManager dialogueManager;
    public DialogueSequenceAsset actDialogueSequence;  // 在 Inspector 中設定對話序列
    
    [Header("場景物件")]
    public SpriteRenderer backgroundRenderer; // 背景
    
    [Header("轉場設定")]
    public TransitionTrigger transitionToNextScene;  // 轉場到下一場景的 Trigger
    
    [Header("時間設定")]
    public float initialDelay = 0.5f;         // 初始等待時間
    public float backgroundFadeInTime = 1f;   // 背景淡入時間
    public float backgroundFadeOutTime = 2f;  // 背景淡出時間
    public float dialogueStartDelay = 1f;     // 背景淡入後開始對話的延遲
    
    [Header("內部狀態")]
    private bool isActDialogueActive = false;  // 標記對話是否正在進行

    protected virtual void Start()
    {
        Debug.Log($"🎬 {GetActName()} 開始");
        
        // 初始化背景為透明
        InitializeBackground();
        
        // 訂閱對話結束事件
        DialogueManager.OnDialogueEnd += OnDialogueEnd;
        
        StartCoroutine(StartActSequence());
    }
    
    protected virtual void OnDestroy()
    {
        // 取消訂閱
        DialogueManager.OnDialogueEnd -= OnDialogueEnd;
    }
    
    /// <summary>
    /// 獲取當前 Act 名稱 - 子類可以覆寫
    /// </summary>
    protected virtual string GetActName()
    {
        return this.GetType().Name;
    }
    
    /// <summary>
    /// 初始化背景
    /// </summary>
    protected virtual void InitializeBackground()
    {
        if (backgroundRenderer != null)
        {
            Color bgColor = backgroundRenderer.color;
            bgColor.a = 0f;
            backgroundRenderer.color = bgColor;
        }
    }
    
    /// <summary>
    /// 對話結束處理
    /// </summary>
    protected virtual void OnDialogueEnd()
    {
        if (isActDialogueActive)
        {
            Debug.Log($"✅ {GetActName()} 對話結束，開始淡出序列");
            isActDialogueActive = false;
            StartCoroutine(ActEndingSequence());
        }
    }

    /// <summary>
    /// Act 開始序列 - 子類可以覆寫來添加自定義邏輯
    /// </summary>
    protected virtual IEnumerator StartActSequence()
    {
        yield return new WaitForSeconds(initialDelay);
        
        // 背景淡入
        StartCoroutine(FadeInBackground());
        
        // 等待背景淡入完成後開始對話
        yield return new WaitForSeconds(dialogueStartDelay);
        StartDialogue();
    }

    /// <summary>
    /// 開始對話
    /// </summary>
    protected virtual void StartDialogue()
    {
        if (actDialogueSequence != null && dialogueManager != null)
        {
            isActDialogueActive = true;
            Debug.Log($"🎯 {GetActName()} 對話開始");
            
            dialogueManager.StartDialogue(actDialogueSequence);
        }
        else
        {
            Debug.LogWarning($"⚠️ 請在 Inspector 中設定 {GetActName()} Dialogue Sequence!");
        }
    }

    /// <summary>
    /// Act 結束序列 - 子類可以覆寫來添加自定義淡出邏輯
    /// </summary>
    protected virtual IEnumerator ActEndingSequence()
    {
        Debug.Log($"🎬 開始 {GetActName()} 結束序列");

        // 淡出對話系統
        if (dialogueManager != null)
        {
            yield return StartCoroutine(dialogueManager.FadeOutDialoguePanel());
        }

        // 淡出背景
        yield return StartCoroutine(FadeOutBackground());

        // 等待一點時間確保淡出完成
        yield return new WaitForSeconds(0.5f);

        Debug.Log($"✅ {GetActName()} 淡出完成，準備轉換場景");

        // 轉換到下一場景
        TransitionToNextScene();
    }

    /// <summary>
    /// 轉換到下一場景
    /// </summary>
    protected virtual void TransitionToNextScene()
    {
        Debug.Log($"🎬 {GetActName()} 準備轉換到下一場景");
        
        if (transitionToNextScene != null)
        {
            Debug.Log("✅ 使用 TransitionTrigger 切換場景");
            transitionToNextScene.TriggerTransition();
        }
        else
        {
            Debug.LogWarning("⚠️ TransitionTrigger 為空，請在 Inspector 中設定");
        }
    }

    /// <summary>
    /// 背景淡入效果
    /// </summary>
    protected virtual IEnumerator FadeInBackground()
    {
        if (backgroundRenderer == null) yield break;

        float elapsed = 0f;
        Color color = backgroundRenderer.color;
        
        while (elapsed < backgroundFadeInTime)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / backgroundFadeInTime);
            backgroundRenderer.color = color;
            yield return null;
        }
        
        color.a = 1f;
        backgroundRenderer.color = color;
        
        Debug.Log($"🎨 {GetActName()} 背景淡入完成");
    }

    /// <summary>
    /// 背景淡出效果
    /// </summary>
    protected virtual IEnumerator FadeOutBackground()
    {
        if (backgroundRenderer == null) yield break;

        float elapsed = 0f;
        Color color = backgroundRenderer.color;
        float startAlpha = color.a;

        while (elapsed < backgroundFadeOutTime)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsed / backgroundFadeOutTime);
            backgroundRenderer.color = color;
            yield return null;
        }

        color.a = 0f;
        backgroundRenderer.color = color;
        
        Debug.Log($"🎨 {GetActName()} 背景淡出完成");
    }
}