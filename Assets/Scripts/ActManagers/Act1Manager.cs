using UnityEngine;
using System.Collections;

/// <summary>
/// 第一幕管理器 - 控制第一幕的流程
/// </summary>
public class Act1Manager : MonoBehaviour
{
    [Header("該幕的組件")]
    public ShakeTextManager shakeTextManager;
    public MelodyToNoiseTransition audioTransition;
    public MusicbookDropAnimation musicbookAnimation;
    public TransitionTrigger transitionToAct2;

    void Start()
    {
        Debug.Log("🎬 第一幕開始");
        
        // 自動開始第一幕的流程
        StartCoroutine(Act1Sequence());
    }

    IEnumerator Act1Sequence()
    {
        // 第一幕的流程控制
        Debug.Log("📝 開始震動文字序列...");
        
        // ShakeTextManager 會自動執行文字顯示
        // 等待文字全部顯示完畢（假設總共需要 50 秒）
        yield return new WaitForSeconds(50f);

        Debug.Log("📔 文字完成，開始筆記本動畫...");
        
        // 等待筆記本動畫完成
        if (musicbookAnimation != null)
        {
            while (musicbookAnimation.IsAnimating())
            {
                yield return null;
            }
        }

        Debug.Log("✅ 第一幕完成");
        
        Debug.Log("🎬 準備切換到第二幕...");
        GoToNextAct();
    }

    public void GoToNextAct()
    {
        Debug.Log("🔍 檢查轉場組件...");
        
        if (transitionToAct2 != null)
        {
            Debug.Log("✅ 使用 TransitionTrigger 切換場景");
            transitionToAct2.TriggerTransition();
        }
    }
    
    /// <summary>
    /// 跳過 Act1 - 直接跳到 Act2 (按 F1 鍵觸發)
    /// </summary>
    [ContextMenu("跳過 Act1")]
    public void SkipAct1()
    {
        Debug.Log("⏩ 跳過 Act1，直接前往 Act2");
        
        // 停止所有正在進行的協程
        StopAllCoroutines();
        
        // 立即跳轉到下一幕
        GoToNextAct();
    }
}