using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System;

/// <summary>
/// 選擇系統管理器 - 處理對話中的選項
/// </summary>
public class ChoiceManager : MonoBehaviour
{
    [Header("UI 組件")]
    public GameObject choicePanel;           // 選擇面板
    public Transform choiceButtonContainer;  // 選項按鈕容器
    public Button choiceButtonPrefab;        // 選項按鈕預製體
    
    [Header("動畫設定")]
    public float panelFadeInDuration = 0.3f;
    public float panelFadeOutDuration = 0.2f;
    public float buttonAppearDelay = 0.1f;   // 按鈕出現間隔
    
    [Header("好感度系統")]
    public RelationshipManager relationshipManager; // 好感度管理器
    
    // 事件
    public static event System.Action<int> OnChoiceMade;
    public static event System.Action OnChoicesShown;
    public static event System.Action OnChoicesHidden;
    
    private List<Button> currentChoiceButtons = new List<Button>();
    private CanvasGroup panelCanvasGroup;
    private bool isShowingChoices = false;
    
    void Awake()
    {
        // 初始化 CanvasGroup
        if (choicePanel != null)
        {
            panelCanvasGroup = choicePanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = choicePanel.AddComponent<CanvasGroup>();
            }
        }
        
        // 初始隱藏面板
        HideChoicesImmediate();
    }
    
    /// <summary>
    /// 顯示選項
    /// </summary>
    /// <param name="choices">選項資料陣列</param>
    public void ShowChoices(ChoiceData[] choices)
    {
        if (choices == null || choices.Length == 0)
        {
            Debug.LogWarning("⚠️ 沒有選項資料");
            return;
        }
        
        StartCoroutine(ShowChoicesCoroutine(choices));
    }
    
    /// <summary>
    /// 顯示選項協程
    /// </summary>
    IEnumerator ShowChoicesCoroutine(ChoiceData[] choices)
    {
        isShowingChoices = true;
        
        // 清除舊的按鈕
        ClearChoiceButtons();
        
        // 顯示面板
        choicePanel.SetActive(true);
        yield return StartCoroutine(FadeInPanel());
        
        // 逐個創建按鈕
        for (int i = 0; i < choices.Length; i++)
        {
            CreateChoiceButton(choices[i], i);
            yield return new WaitForSeconds(buttonAppearDelay);
        }
        
        OnChoicesShown?.Invoke();
        Debug.Log($"🎯 顯示了 {choices.Length} 個選項");
    }
    
    /// <summary>
    /// 創建選項按鈕
    /// </summary>
    void CreateChoiceButton(ChoiceData choiceData, int index)
    {
        if (choiceButtonPrefab == null || choiceButtonContainer == null)
        {
            Debug.LogError("❌ 選項按鈕預製體或容器未設定");
            return;
        }
        
        // 創建按鈕
        Button newButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);
        currentChoiceButtons.Add(newButton);
        
        // 設定按鈕文字
        TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = choiceData.choiceText;
            
            // 根據好感度效果設定顏色
            switch (choiceData.relationshipEffect)
            {
                case RelationshipEffect.Increase:
                    buttonText.color = new Color(0.2f, 0.8f, 0.2f); // 綠色
                    break;
                case RelationshipEffect.Decrease:
                    buttonText.color = new Color(0.8f, 0.2f, 0.2f); // 紅色
                    break;
                default:
                    buttonText.color = Color.white; // 白色
                    break;
            }
        }
        
        // 設定按鈕點擊事件
        newButton.onClick.AddListener(() => OnChoiceSelected(index, choiceData));
        
        // 按鈕動畫
        StartCoroutine(AnimateButtonAppear(newButton));
    }
    
    /// <summary>
    /// 按鈕出現動畫
    /// </summary>
    IEnumerator AnimateButtonAppear(Button button)
    {
        CanvasGroup buttonGroup = button.GetComponent<CanvasGroup>();
        if (buttonGroup == null)
        {
            buttonGroup = button.gameObject.AddComponent<CanvasGroup>();
        }
        
        // 初始透明
        buttonGroup.alpha = 0f;
        button.transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        float duration = 0.2f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            buttonGroup.alpha = progress;
            button.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
            
            yield return null;
        }
        
        buttonGroup.alpha = 1f;
        button.transform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// 處理選項被選中
    /// </summary>
    void OnChoiceSelected(int choiceIndex, ChoiceData choiceData)
    {
        if (!isShowingChoices) return;
        
        Debug.Log($"🎯 選擇了選項 {choiceIndex}: {choiceData.choiceText}");
        
        // 應用好感度變化
        if (relationshipManager != null && !string.IsNullOrEmpty(choiceData.targetCharacter))
        {
            relationshipManager.ModifyRelationship(choiceData.targetCharacter, choiceData.relationshipEffect);
        }
        
        // 觸發角色表情變化
        if (!string.IsNullOrEmpty(choiceData.characterExpression))
        {
            var characterManager = FindFirstObjectByType<CharacterManager>();
            if (characterManager != null)
            {
                characterManager.ChangeCharacterExpression(choiceData.targetCharacter, choiceData.characterExpression);
            }
        }
        
        // 隱藏選項
        StartCoroutine(HideChoicesAfterSelection(choiceIndex));
    }
    
    /// <summary>
    /// 選擇後隱藏選項
    /// </summary>
    IEnumerator HideChoicesAfterSelection(int selectedIndex)
    {
        // 短暫等待讓玩家看到選擇結果
        yield return new WaitForSeconds(0.3f);
        
        // 隱藏選項
        yield return StartCoroutine(HideChoicesCoroutine());
        
        // 觸發選擇完成事件
        OnChoiceMade?.Invoke(selectedIndex);
    }
    
    /// <summary>
    /// 隱藏選項
    /// </summary>
    public void HideChoices()
    {
        if (!isShowingChoices) return;
        StartCoroutine(HideChoicesCoroutine());
    }
    
    /// <summary>
    /// 隱藏選項協程
    /// </summary>
    IEnumerator HideChoicesCoroutine()
    {
        if (!isShowingChoices) yield break;
        
        // 淡出面板
        yield return StartCoroutine(FadeOutPanel());
        
        // 清除按鈕
        ClearChoiceButtons();
        
        // 隱藏面板
        choicePanel.SetActive(false);
        isShowingChoices = false;
        
        OnChoicesHidden?.Invoke();
        Debug.Log("🎯 隱藏選項完成");
    }
    
    /// <summary>
    /// 立即隱藏選項（無動畫）
    /// </summary>
    void HideChoicesImmediate()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
        }
        
        ClearChoiceButtons();
        isShowingChoices = false;
    }
    
    /// <summary>
    /// 面板淡入
    /// </summary>
    IEnumerator FadeInPanel()
    {
        if (panelCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < panelFadeInDuration)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / panelFadeInDuration);
            yield return null;
        }
        
        panelCanvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// 面板淡出
    /// </summary>
    IEnumerator FadeOutPanel()
    {
        if (panelCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < panelFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / panelFadeOutDuration);
            yield return null;
        }
        
        panelCanvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// 清除所有選項按鈕
    /// </summary>
    void ClearChoiceButtons()
    {
        foreach (Button button in currentChoiceButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        currentChoiceButtons.Clear();
    }
    
    /// <summary>
    /// 檢查是否正在顯示選項
    /// </summary>
    public bool IsShowingChoices()
    {
        return isShowingChoices;
    }
}