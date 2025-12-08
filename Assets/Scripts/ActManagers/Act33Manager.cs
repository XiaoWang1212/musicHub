using UnityEngine;
using System.Collections;

/// <summary>
/// Act33 管理器 - 根據白石好感度分支到不同場景
/// </summary>
public class Act33Manager : BaseActManager
{
    [Header("Act33 分支場景設定")]
    [Tooltip("白石好感 > 0 時切換到的場景")]
    public TransitionTrigger transitionIfPositive;
    
    [Tooltip("白石好感 <= 0 時切換到的場景")]
    public TransitionTrigger transitionIfNegativeOrZero;
    
    protected override string GetActName()
    {
        return "Act33Manager";
    }
    
    /// <summary>
    /// 覆寫場景轉換邏輯 - 根據白石好感度判斷
    /// </summary>
    protected override void TransitionToNextScene()
    {
        Debug.Log("🎬 Act33 根據白石好感度判斷場景分支");
        
        if (RelationshipManager.Instance == null)
        {
            Debug.LogError("❌ RelationshipManager 不存在!");
            return;
        }
        
        // 獲取白石好感度
        int shiraishiRelationship = RelationshipManager.Instance.GetRelationshipValue("白石 透羽");
        
        Debug.Log($"💝 白石 透羽 當前好感度: {shiraishiRelationship}");
        
        if (shiraishiRelationship > 0)
        {
            Debug.Log("✅ 好感度 > 0, 切換到場景 A");
            if (transitionIfPositive != null)
            {
                transitionIfPositive.TriggerTransition();
            }
            else
            {
                Debug.LogError("❌ Transition If Positive 未設定!");
            }
        }
        else
        {
            Debug.Log("✅ 好感度 <= 0, 切換到場景 B");
            if (transitionIfNegativeOrZero != null)
            {
                transitionIfNegativeOrZero.TriggerTransition();
            }
            else
            {
                Debug.LogError("❌ Transition If Negative Or Zero 未設定!");
            }
        }
    }
}
