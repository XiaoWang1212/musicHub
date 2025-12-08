using UnityEngine;
using System.Collections;

/// <summary>
/// BeforeEnd 管理器 - 根據四人好感總和與鼓手是否回歸判定結局
/// </summary>
public class BeforeEndManager : BaseActManager
{
    [Header("BeforeEnd 結局分支設定")]
    [Tooltip("四人好感總和 < 9 且鼓手回來 → 結局1")]
    public TransitionTrigger transitionLowScoreDrummerReturned;
    
    [Tooltip("四人好感總和 < 9 或鼓手沒回來 → 結局2")]
    public TransitionTrigger transitionLowScoreOrDrummerLeft;
    
    [Tooltip("四人好感總和 >= 9 且鼓手回來 → 結局3")]
    public TransitionTrigger transitionHighScoreDrummerReturned;
    
    protected override string GetActName()
    {
        return "BeforeEndManager";
    }
    
    /// <summary>
    /// 覆寫場景轉換邏輯 - 根據好感總和與鼓手回歸判定結局
    /// </summary>
    protected override void TransitionToNextScene()
    {
        Debug.Log("🎬 BeforeEnd 開始判定結局分支");
        
        if (RelationshipManager.Instance == null)
        {
            Debug.LogError("❌ RelationshipManager 不存在!");
            return;
        }
        
        // 獲取四人好感總和
        int totalRelationship = RelationshipManager.Instance.GetTotalRelationship();
        
        // 獲取鼓手是否回歸的標記
        bool drummerReturned = RelationshipManager.Instance.GetStoryFlag("hinata_returned");
        
        Debug.Log($"💝 四人好感總和: {totalRelationship}");
        Debug.Log($"🥁 鼓手(日向)是否回來: {drummerReturned}");
        
        // 判定結局
        if (totalRelationship < 9 && drummerReturned)
        {
            // 結局1: 四人好感總和 < 9 且鼓手回來
            Debug.Log("✅ 結局1: 四人好感總和 < 9 且鼓手回來");
            if (transitionLowScoreDrummerReturned != null)
            {
                transitionLowScoreDrummerReturned.TriggerTransition();
            }
            else
            {
                Debug.LogError("❌ Transition Low Score Drummer Returned 未設定!");
            }
        }
        else if (totalRelationship < 9 || !drummerReturned)
        {
            // 結局2: 四人好感總和 < 9 或鼓手沒回來
            Debug.Log("✅ 結局2: 四人好感總和 < 9 或鼓手沒回來");
            if (transitionLowScoreOrDrummerLeft != null)
            {
                transitionLowScoreOrDrummerLeft.TriggerTransition();
            }
            else
            {
                Debug.LogError("❌ Transition Low Score Or Drummer Left 未設定!");
            }
        }
        else // totalRelationship >= 9 && drummerReturned
        {
            // 結局3: 四人好感總和 >= 9 且鼓手回來
            Debug.Log("✅ 結局3: 四人好感總和 >= 9 且鼓手回來");
            if (transitionHighScoreDrummerReturned != null)
            {
                transitionHighScoreDrummerReturned.TriggerTransition();
            }
            else
            {
                Debug.LogError("❌ Transition High Score Drummer Returned 未設定!");
            }
        }
    }
}
