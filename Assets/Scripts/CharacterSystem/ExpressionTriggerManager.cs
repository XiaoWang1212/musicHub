using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 表情觸發管理器 - 提供更靈活的表情切換方式
/// 可以通過對話索引或直接調用來切換角色表情
/// </summary>
public class ExpressionTriggerManager : MonoBehaviour
{
    [System.Serializable]
    public class ExpressionTrigger
    {
        [Header("觸發設定")]
        [Tooltip("在第幾句對話觸發表情切換（從 0 開始，-1 表示手動觸發）")]
        public int dialogueIndex = -1;
        
        [Header("角色表情設定")]
        [Tooltip("角色名稱")]
        public string characterName;
        
        [Tooltip("表情名稱，例如：開心、難過、驚訝、憤怒、害怕、普通")]
        public string expressionName = "普通";
        
        [Tooltip("是否使用切換動畫")]
        public bool useAnimation = true;
        
        [Header("額外設定")]
        [Tooltip("觸發延遲時間（秒）")]
        public float delay = 0f;
        
        [Tooltip("描述說明")]
        public string description = "";
    }
    
    [Header("表情觸發列表")]
    [Tooltip("配置各種表情切換觸發條件")]
    public List<ExpressionTrigger> expressionTriggers = new List<ExpressionTrigger>();
    
    [Header("除錯設定")]
    public bool enableDebugLog = true;
    
    private CharacterManager characterManager;
    
    void Start()
    {
        // 獲取 CharacterManager 引用
        characterManager = FindFirstObjectByType<CharacterManager>();
        if (characterManager == null)
        {
            Debug.LogWarning("⚠️ ExpressionTriggerManager: 找不到 CharacterManager");
        }
        
        // 訂閱對話索引變化事件
        DialogueManager.OnDialogueIndexChanged += OnDialogueIndexChanged;
        
        if (enableDebugLog)
        {
            Debug.Log($"🎭 ExpressionTriggerManager 初始化完成，共 {expressionTriggers.Count} 個觸發器");
        }
    }
    
    void OnDestroy()
    {
        // 取消訂閱
        DialogueManager.OnDialogueIndexChanged -= OnDialogueIndexChanged;
    }
    
    /// <summary>
    /// 處理對話索引變化
    /// </summary>
    void OnDialogueIndexChanged(int dialogueIndex)
    {
        if (enableDebugLog)
        {
            Debug.Log($"🎭 ExpressionTriggerManager: 檢查對話索引 {dialogueIndex}");
        }
        
        // 檢查是否有對應的表情觸發器
        foreach (var trigger in expressionTriggers)
        {
            if (trigger.dialogueIndex == dialogueIndex)
            {
                if (enableDebugLog)
                {
                    Debug.Log($"🎭 觸發表情切換: {trigger.characterName} -> {trigger.expressionName} (索引: {dialogueIndex})");
                }
                
                if (trigger.delay > 0)
                {
                    StartCoroutine(ExecuteExpressionTriggerWithDelay(trigger));
                }
                else
                {
                    ExecuteExpressionTrigger(trigger);
                }
            }
        }
    }
    
    /// <summary>
    /// 執行表情觸發（帶延遲）
    /// </summary>
    System.Collections.IEnumerator ExecuteExpressionTriggerWithDelay(ExpressionTrigger trigger)
    {
        yield return new WaitForSeconds(trigger.delay);
        ExecuteExpressionTrigger(trigger);
    }
    
    /// <summary>
    /// 執行表情觸發
    /// </summary>
    void ExecuteExpressionTrigger(ExpressionTrigger trigger)
    {
        if (characterManager == null)
        {
            Debug.LogWarning("⚠️ CharacterManager 未找到，無法切換表情");
            return;
        }
        
        if (string.IsNullOrEmpty(trigger.characterName))
        {
            Debug.LogWarning("⚠️ 角色名稱為空，無法切換表情");
            return;
        }
        
        if (string.IsNullOrEmpty(trigger.expressionName))
        {
            Debug.LogWarning("⚠️ 表情名稱為空，無法切換表情");
            return;
        }
        
        // 執行表情切換
        characterManager.ChangeCharacterExpression(trigger.characterName, trigger.expressionName, trigger.useAnimation);
        
        if (enableDebugLog)
        {
            Debug.Log($"✅ 成功切換 {trigger.characterName} 的表情為 {trigger.expressionName}");
        }
    }
    
    // ==================== 公用方法 ====================
    
    /// <summary>
    /// 手動觸發表情切換（通過角色名稱和表情名稱）
    /// </summary>
    public void TriggerExpression(string characterName, string expressionName, bool useAnimation = true)
    {
        if (characterManager == null)
        {
            Debug.LogWarning("⚠️ CharacterManager 未找到，無法切換表情");
            return;
        }
        
        characterManager.ChangeCharacterExpression(characterName, expressionName, useAnimation);
        
        if (enableDebugLog)
        {
            Debug.Log($"🎭 手動觸發表情切換: {characterName} -> {expressionName}");
        }
    }
    
    /// <summary>
    /// 手動觸發表情切換（通過觸發器索引）
    /// </summary>
    public void TriggerExpressionByIndex(int triggerIndex)
    {
        if (triggerIndex >= 0 && triggerIndex < expressionTriggers.Count)
        {
            var trigger = expressionTriggers[triggerIndex];
            ExecuteExpressionTrigger(trigger);
        }
        else
        {
            Debug.LogWarning($"⚠️ 觸發器索引 {triggerIndex} 超出範圍");
        }
    }
    
    /// <summary>
    /// 批量設定多個角色的表情
    /// </summary>
    public void SetMultipleExpressions(Dictionary<string, string> characterExpressions, bool useAnimation = true)
    {
        foreach (var pair in characterExpressions)
        {
            TriggerExpression(pair.Key, pair.Value, useAnimation);
        }
    }
    
    /// <summary>
    /// 重置所有角色為預設表情
    /// </summary>
    public void ResetAllToDefaultExpression()
    {
        if (characterManager == null) return;
        
        // 從已配置的觸發器中獲取所有角色名稱
        HashSet<string> characterNames = new HashSet<string>();
        foreach (var trigger in expressionTriggers)
        {
            if (!string.IsNullOrEmpty(trigger.characterName))
            {
                characterNames.Add(trigger.characterName);
            }
        }
        
        // 重置每個角色為預設表情
        foreach (string characterName in characterNames)
        {
            characterManager.SetCharacterDefaultExpression(characterName);
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"🎭 重置 {characterNames.Count} 個角色為預設表情");
        }
    }
    
    // ==================== 除錯和測試方法 ====================
    
    [ContextMenu("檢查表情觸發器設定")]
    public void CheckTriggerSetup()
    {
        Debug.Log("🎭 === 表情觸發器設定檢查 ===");
        
        if (expressionTriggers.Count == 0)
        {
            Debug.LogWarning("⚠️ 沒有設定任何表情觸發器");
            return;
        }
        
        Debug.Log($"✅ 已設定 {expressionTriggers.Count} 個表情觸發器:");
        
        for (int i = 0; i < expressionTriggers.Count; i++)
        {
            var trigger = expressionTriggers[i];
            string triggerType = trigger.dialogueIndex >= 0 ? $"對話索引 {trigger.dialogueIndex}" : "手動觸發";
            Debug.Log($"  [{i}] {triggerType}:");
            Debug.Log($"      角色: {trigger.characterName}");
            Debug.Log($"      表情: {trigger.expressionName}");
            Debug.Log($"      動畫: {trigger.useAnimation}");
            Debug.Log($"      延遲: {trigger.delay}s");
            if (!string.IsNullOrEmpty(trigger.description))
            {
                Debug.Log($"      說明: {trigger.description}");
            }
        }
        
        // 檢查 CharacterManager
        if (characterManager == null)
        {
            Debug.LogError("❌ 找不到 CharacterManager，請確保場景中有 CharacterManager 物件");
        }
        else
        {
            Debug.Log("✅ CharacterManager 連接正常");
        }
    }
    
    [ContextMenu("測試第一個觸發器")]
    public void TestFirstTrigger()
    {
        if (expressionTriggers.Count > 0)
        {
            var trigger = expressionTriggers[0];
            ExecuteExpressionTrigger(trigger);
            Debug.Log($"🧪 測試觸發器: {trigger.characterName} -> {trigger.expressionName}");
        }
        else
        {
            Debug.LogWarning("⚠️ 沒有觸發器可供測試");
        }
    }
    
    [ContextMenu("重置所有表情")]
    public void TestResetAllExpressions()
    {
        ResetAllToDefaultExpression();
    }
    
    /// <summary>
    /// 快速設定常用表情觸發器
    /// </summary>
    [ContextMenu("設定範例觸發器")]
    public void SetupExampleTriggers()
    {
        expressionTriggers.Clear();
        
        expressionTriggers.AddRange(new ExpressionTrigger[]
        {
            new ExpressionTrigger
            {
                dialogueIndex = 1,
                characterName = "白石 透羽",
                expressionName = "驚訝",
                useAnimation = true,
                description = "聽到意外消息時的反應"
            },
            
            new ExpressionTrigger
            {
                dialogueIndex = 5,
                characterName = "白石 透羽",
                expressionName = "難過",
                useAnimation = true,
                delay = 0.5f,
                description = "感到失望"
            },
            
            new ExpressionTrigger
            {
                dialogueIndex = 10,
                characterName = "導師",
                expressionName = "微笑",
                useAnimation = true,
                description = "給予鼓勵"
            },
            
            new ExpressionTrigger
            {
                dialogueIndex = 15,
                characterName = "白石 透羽",
                expressionName = "開心",
                useAnimation = true,
                description = "得到幫助後的喜悅"
            }
        });
        
        Debug.Log($"✅ 已設定 {expressionTriggers.Count} 個範例觸發器");
    }
}