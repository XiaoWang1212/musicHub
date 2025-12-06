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
    
    [Header("選擇系統(可選)")]
    [Tooltip("如果這個 Act 需要選擇,設定選擇序列")]
    public ChoiceSequenceAsset choiceSequence;         // 選擇序列
    
    [Tooltip("選擇後的對話序列(可選)")]
    public DialogueSequenceAsset afterChoiceDialogue;  // 選擇後的對話
    
    [Header("場景物件")]
    public SpriteRenderer backgroundRenderer; // 背景
    
    [Header("轉場設定")]
    public TransitionTrigger transitionToNextScene;  // 轉場到下一場景的 Trigger
    
    [Header("時間設定")]
    public float initialDelay = 0.5f;         // 初始等待時間
    public float backgroundFadeInTime = 1f;   // 背景淡入時間
    public float backgroundFadeOutTime = 2f;  // 背景淡出時間
    public float dialogueStartDelay = 1f;     // 背景淡入後開始對話的延遲
    
    [Header("角色動作設定")]
    public CharacterManager characterManager; // 角色管理器引用
    
    [Header("選擇系統")]
    public ChoiceManager choiceManager;       // 選擇管理器引用
    
    [Header("內部狀態")]
    private bool isActDialogueActive = false;  // 標記對話是否正在進行
    protected bool isWaitingForChoice = false; // 標記是否正在等待玩家選擇

    protected virtual void Start()
    {
        Debug.Log($"🎬 {GetActName()} 開始");
        
        // 初始化背景為透明
        InitializeBackground();
        
        // 訂閱對話結束事件
        DialogueManager.OnDialogueEnd += OnDialogueEnd;
        
        // 訂閱選擇事件
        if (choiceManager != null)
        {
            ChoiceManager.OnChoiceMade += OnChoiceMade;
        }
        
        StartCoroutine(StartActSequence());
    }
    
    protected virtual void OnDestroy()
    {
        // 取消訂閱
        DialogueManager.OnDialogueEnd -= OnDialogueEnd;
        
        if (choiceManager != null)
        {
            ChoiceManager.OnChoiceMade -= OnChoiceMade;
        }
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
        // 如果正在等待選擇，不要進入結束序列
        if (isWaitingForChoice)
        {
            Debug.Log($"⏸️ {GetActName()} 對話結束，但正在等待玩家選擇");
            return;
        }
        
        if (isActDialogueActive)
        {
            // 檢查是否需要顯示選擇
            if (choiceSequence != null && choiceSequence.choices != null && choiceSequence.choices.Length > 0)
            {
                Debug.Log($"🎯 {GetActName()} 對話結束，顯示選擇");
                ShowChoicesFromSequence();
                return;
            }
            
            Debug.Log($"✅ {GetActName()} 對話結束，開始淡出序列");
            isActDialogueActive = false;
            StartCoroutine(ActEndingSequence());
        }
    }
    
    /// <summary>
    /// 從選擇序列顯示選項
    /// </summary>
    protected virtual void ShowChoicesFromSequence()
    {
        if (choiceSequence == null) return;
        
        // 轉換為 ChoiceData 陣列
        ChoiceData[] choices = new ChoiceData[choiceSequence.choices.Length];
        for (int i = 0; i < choiceSequence.choices.Length; i++)
        {
            choices[i] = choiceSequence.choices[i].ToChoiceData();
        }
        
        ShowChoices(choices);
    }
    
    /// <summary>
    /// 選擇完成處理 - 子類可以覆寫來處理特定選擇邏輯
    /// </summary>
    protected virtual void OnChoiceMade(ChoiceData choice)
    {
        Debug.Log($"🎯 {GetActName()} 玩家選擇了: {choice.choiceText}");
        isWaitingForChoice = false;
        
        // 如果有分支對話，ChoiceManager 會自動處理切換
        // 這裡只處理沒有分支對話的情況
        
        // 如果選擇有設定分支對話(branchDialogue)，ChoiceManager 已經自動切換了
        // 不需要在這裡處理
        
        // 如果沒有分支對話，檢查是否有統一的選擇後對話
        if (choice.branchDialogue == null)
        {
            if (afterChoiceDialogue != null)
            {
                Debug.Log($"🎬 播放統一的選擇後對話");
                StartCoroutine(PlayFollowUpDialogue(afterChoiceDialogue));
            }
            else
            {
                // 沒有任何後續對話，直接結束
                Debug.Log($"🎬 沒有後續對話，開始結束序列");
                StartCoroutine(ActEndingSequence());
            }
        }
        // 如果有 branchDialogue，ChoiceManager 會處理，這裡什麼都不做
    }
    
    /// <summary>
    /// 播放後續對話
    /// </summary>
    protected virtual IEnumerator PlayFollowUpDialogue(DialogueSequenceAsset dialogue)
    {
        yield return new WaitForSeconds(0.5f);
        
        isActDialogueActive = true;
        dialogueManager.StartDialogue(dialogue);
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
    
    // ==================== 角色動作方法 ====================
    
    /// <summary>
    /// 讓指定角色搖動（恐懼效果）- 左右抖動兩次
    /// </summary>
    /// <param name="characterName">角色名稱</param>
    /// <param name="intensity">搖動強度</param>
    /// <param name="duration">持續時間（完整搖動動作的總時間）</param>
    public void ShakeCharacter(string characterName, float intensity = 0.05f, float duration = 0.8f)
    {
        if (characterManager == null)
        {
            Debug.LogWarning("⚠️ CharacterManager 未設定，無法執行角色動作");
            return;
        }
        
        SpriteRenderer characterRenderer = characterManager.GetCharacterRenderer(characterName);
        if (characterRenderer != null)
        {
            StartCoroutine(ShakeCharacterCoroutine(characterRenderer, intensity, duration));
        }
        else
        {
            Debug.LogWarning($"⚠️ 找不到角色: {characterName}");
        }
    }
    
    /// <summary>
    /// 讓指定角色跳一下
    /// </summary>
    /// <param name="characterName">角色名稱</param>
    /// <param name="jumpHeight">跳躍高度</param>
    /// <param name="duration">跳躍持續時間</param>
    public void JumpCharacterOnce(string characterName, float jumpHeight = 0.3f, float duration = 1.2f)
    {
        if (characterManager == null)
        {
            Debug.LogWarning("⚠️ CharacterManager 未設定，無法執行角色動作");
            return;
        }
        
        SpriteRenderer characterRenderer = characterManager.GetCharacterRenderer(characterName);
        if (characterRenderer != null)
        {
            StartCoroutine(JumpCharacterCoroutine(characterRenderer, 1, jumpHeight, duration));
        }
        else
        {
            Debug.LogWarning($"⚠️ 找不到角色: {characterName}");
        }
    }
    
    /// <summary>
    /// 讓指定角色跳兩下
    /// </summary>
    /// <param name="characterName">角色名稱</param>
    /// <param name="jumpHeight">跳躍高度</param>
    /// <param name="duration">每次跳躍的持續時間</param>
    public void JumpCharacterTwice(string characterName, float jumpHeight = 0.3f, float duration = 1.2f)
    {
        if (characterManager == null)
        {
            Debug.LogWarning("⚠️ CharacterManager 未設定，無法執行角色動作");
            return;
        }
        
        SpriteRenderer characterRenderer = characterManager.GetCharacterRenderer(characterName);
        if (characterRenderer != null)
        {
            StartCoroutine(JumpCharacterCoroutine(characterRenderer, 2, jumpHeight, duration));
        }
        else
        {
            Debug.LogWarning($"⚠️ 找不到角色: {characterName}");
        }
    }
    
    /// <summary>
    /// 切換指定角色的表情
    /// </summary>
    /// <param name="characterName">角色名稱</param>
    /// <param name="expressionName">表情名稱</param>
    /// <param name="useAnimation">是否使用切換動畫</param>
    public void ChangeCharacterExpression(string characterName, string expressionName, bool useAnimation = true)
    {
        if (characterManager == null)
        {
            Debug.LogWarning("⚠️ CharacterManager 未設定，無法執行角色動作");
            return;
        }
        
        characterManager.ChangeCharacterExpression(characterName, expressionName, useAnimation);
    }
    
    /// <summary>
    /// 設定角色為預設表情
    /// </summary>
    /// <param name="characterName">角色名稱</param>
    public void SetCharacterDefaultExpression(string characterName)
    {
        if (characterManager == null)
        {
            Debug.LogWarning("⚠️ CharacterManager 未設定，無法執行角色動作");
            return;
        }
        
        characterManager.SetCharacterDefaultExpression(characterName);
    }
    
    // ==================== 選擇系統方法 ====================
    
    /// <summary>
    /// 顯示選項
    /// </summary>
    /// <param name="choices">選項資料陣列</param>
    protected void ShowChoices(ChoiceData[] choices)
    {
        if (choiceManager == null)
        {
            Debug.LogError("❌ ChoiceManager 未設定，無法顯示選項!");
            return;
        }
        
        if (RelationshipManager.Instance == null)
        {
            Debug.LogError("❌ RelationshipManager 不存在，請確認已在 MainMenu 場景設定!");
            return;
        }
        
        isWaitingForChoice = true;
        Debug.Log($"🎯 {GetActName()} 顯示 {choices.Length} 個選項");
        choiceManager.ShowChoices(choices);
    }
    
    /// <summary>
    /// 建立選項資料 - 輔助方法
    /// </summary>
    protected ChoiceData CreateChoice(string text, string targetCharacter, RelationshipEffect effect)
    {
        return new ChoiceData
        {
            choiceText = text,
            targetCharacter = targetCharacter,
            relationshipEffect = effect
        };
    }
    
    /// <summary>
    /// 隱藏選項
    /// </summary>
    protected void HideChoices()
    {
        if (choiceManager != null)
        {
            choiceManager.HideChoices();
        }
    }
    
    /// <summary>
    /// 檢查是否正在顯示選項
    /// </summary>
    protected bool IsShowingChoices()
    {
        return choiceManager != null && choiceManager.IsShowingChoices();
    }
    
    // ==================== 內部動作協程 ====================
    
    /// <summary>
    /// 角色搖動協程 - 左右抖動兩次
    /// </summary>
    IEnumerator ShakeCharacterCoroutine(SpriteRenderer renderer, float intensity, float duration)
    {
        Vector3 originalPosition = renderer.transform.position;
        
        Debug.Log($"😰 {renderer.name} 開始搖動 - 原位置: {originalPosition}, 強度: {intensity}, 時長: {duration}s");
        
        // 左右抖動兩次
        float singleShakeDuration = duration / 4f; // 每次抖動的時間（左右來回算一次）
        
        for (int shake = 0; shake < 2; shake++)
        {
            Debug.Log($"🔄 {renderer.name} 第 {shake + 1} 次抖動開始");
            
            // 向右
            yield return StartCoroutine(SmoothMoveToPosition(renderer, 
                originalPosition + new Vector3(intensity, 0f, 0f), singleShakeDuration));
            
            // 向左
            yield return StartCoroutine(SmoothMoveToPosition(renderer, 
                originalPosition + new Vector3(-intensity, 0f, 0f), singleShakeDuration));
        }
        
        // 回到原位置
        yield return StartCoroutine(SmoothMoveToPosition(renderer, originalPosition, singleShakeDuration));
        
        Debug.Log($"✅ {renderer.name} 搖動完成 - 回到原位: {originalPosition}");
    }
    
    /// <summary>
    /// 平滑移動到指定位置
    /// </summary>
    IEnumerator SmoothMoveToPosition(SpriteRenderer renderer, Vector3 targetPosition, float moveDuration)
    {
        Vector3 startPosition = renderer.transform.position;
        float elapsed = 0f;
        
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / moveDuration;
            
            // 使用平滑插值
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            renderer.transform.position = currentPosition;
            
            yield return null;
        }
        
        // 確保到達目標位置
        renderer.transform.position = targetPosition;
    }
    
    /// <summary>
    /// 角色跳躍協程
    /// </summary>
    IEnumerator JumpCharacterCoroutine(SpriteRenderer renderer, int jumpCount, float jumpHeight, float duration)
    {
        Vector3 originalPosition = renderer.transform.position;
        
        // 增加跳躍高度讓效果更明顯 - 設定為誇張數值
        float enhancedJumpHeight = Mathf.Max(jumpHeight, 3.0f);
        
        for (int i = 0; i < jumpCount; i++)
        {
            float elapsed = 0f;
            
            // 上升階段
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (duration / 2f);
                float currentY = Mathf.Lerp(originalPosition.y, originalPosition.y + enhancedJumpHeight, 
                                          Mathf.Sin(progress * Mathf.PI / 2f)); // 使用sin函數讓跳躍更自然
                
                Vector3 newPosition = new Vector3(originalPosition.x, currentY, originalPosition.z);
                renderer.transform.position = newPosition;
                
                yield return null;
            }
            
            // 下降階段
            elapsed = 0f;
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (duration / 2f);
                float currentY = Mathf.Lerp(originalPosition.y + enhancedJumpHeight, originalPosition.y, 
                                          Mathf.Sin(progress * Mathf.PI / 2f));
                
                Vector3 newPosition = new Vector3(originalPosition.x, currentY, originalPosition.z);
                renderer.transform.position = newPosition;
                
                yield return null;
            }
        }
        
        // 確保回到原位
        renderer.transform.position = originalPosition;
    }
}