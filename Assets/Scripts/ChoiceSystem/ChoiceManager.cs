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
    
    [Header("顯示設定")]
    [Tooltip("是否隨機打亂選項順序")]
    public bool randomizeChoiceOrder = true;
    
    [Header("Hover 效果設定")]
    [Tooltip("滑鼠懸停時的縮放大小")]
    public float hoverScale = 1.05f;  // 寬長型按鈕用較小的縮放
    [Tooltip("Hover 動畫時間")]
    public float hoverAnimationDuration = 0.15f;  // 更快的反應
    [Tooltip("Hover 時的顏色 - 黑底白字建議用深灰色")]
    public Color hoverColor = new Color(0.25f, 0.25f, 0.25f, 1f);  // 深灰色 (RGB: 64, 64, 64)
    [Tooltip("是否啟用 Hover 顏色變化")]
    public bool enableHoverColorChange = true;
    [Tooltip("Hover 時向右偏移的距離")]
    public float hoverOffsetX = 10f;  // 寬長型按鈕適合用水平位移;
    
    // 事件
    public static event System.Action<ChoiceData> OnChoiceMade;
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
            
            // 確保初始狀態正確
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = false;  // 初始不阻擋射線
        }
        
        // 初始隱藏面板
        HideChoicesImmediate();
        
        // 檢查 EventSystem
        CheckEventSystem();
    }
    
    void CheckEventSystem()
    {
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogWarning("⚠️ 場景中沒有 EventSystem,正在自動創建...");
            
            // 自動創建 EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            Debug.Log("✅ EventSystem 已自動創建");
        }
        else
        {
            Debug.Log($"✅ EventSystem 存在: {eventSystem.name}");
        }
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
        
        // 隨機打亂選項順序
        ChoiceData[] displayChoices = choices;
        if (randomizeChoiceOrder && choices.Length > 1)
        {
            displayChoices = ShuffleArray(choices);
            Debug.Log("🎲 選項順序已隨機打亂");
        }
        
        // 顯示面板
        choicePanel.SetActive(true);
        yield return StartCoroutine(FadeInPanel());
        
        // 逐個創建按鈕
        for (int i = 0; i < displayChoices.Length; i++)
        {
            CreateChoiceButton(displayChoices[i], i);
            yield return new WaitForSeconds(buttonAppearDelay);
        }
        
        OnChoicesShown?.Invoke();
        Debug.Log($"🎯 顯示了 {displayChoices.Length} 個選項");
    }
    
    /// <summary>
    /// 隨機打亂陣列順序 (Fisher-Yates 洗牌算法)
    /// </summary>
    ChoiceData[] ShuffleArray(ChoiceData[] array)
    {
        ChoiceData[] shuffled = new ChoiceData[array.Length];
        System.Array.Copy(array, shuffled, array.Length);
        
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            ChoiceData temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }
        
        return shuffled;
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
        
        // 確保按鈕可以互動
        newButton.interactable = true;
        
        // 設定按鈕文字
        TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = choiceData.choiceText;
            Debug.Log($"✅ 創建按鈕 {index}: {choiceData.choiceText}");
        }
        
        // 添加 Hover 效果
        AddHoverEffect(newButton);
        
        // 設定按鈕點擊事件
        newButton.onClick.AddListener(() => {
            Debug.Log($"🖱️ 按鈕被點擊: {choiceData.choiceText}");
            OnChoiceSelected(index, choiceData);
        });
        
        // 檢查按鈕層級和設定
        Debug.Log($"📍 按鈕狀態: interactable={newButton.interactable}, raycastTarget={newButton.GetComponent<UnityEngine.UI.Image>()?.raycastTarget}");
        Debug.Log($"📍 按鈕層級: {newButton.gameObject.layer}, 父物件層級: {choiceButtonContainer.gameObject.layer}");
        
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
        
        // 確保 CanvasGroup 不會阻擋射線檢測
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;
        
        // 初始透明
        buttonGroup.alpha = 0f;
        button.transform.localScale = Vector3.zero;
        
        // 確保按鈕本身可以互動
        button.interactable = true;
        
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
        
        // 動畫完成後初始化 Hover 效果
        var hoverEffect = button.GetComponent<ChoiceButtonHoverEffect>();
        if (hoverEffect != null)
        {
            hoverEffect.InitializeAfterAnimation();
        }
    }
    
    /// <summary>
    /// 處理選項被選中
    /// </summary>
    void OnChoiceSelected(int choiceIndex, ChoiceData choiceData)
    {
        if (!isShowingChoices) return;
        
        Debug.Log($"🎯 選擇了選項 {choiceIndex}: {choiceData.choiceText}");
        
        // 應用好感度變化
        if (RelationshipManager.Instance != null && !string.IsNullOrEmpty(choiceData.targetCharacter))
        {
            RelationshipManager.Instance.ModifyRelationship(choiceData.targetCharacter, choiceData.relationshipEffect);
            Debug.Log($"💝 {choiceData.targetCharacter} 好感度變化: {choiceData.relationshipEffect}");
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
        
        // 隱藏選項並觸發事件
        StartCoroutine(HideChoicesAfterSelection(choiceData));
    }
    
    /// <summary>
    /// 選擇後隱藏選項
    /// </summary>
    IEnumerator HideChoicesAfterSelection(ChoiceData choiceData)
    {
        // 短暫等待讓玩家看到選擇結果
        yield return new WaitForSeconds(0.3f);
        
        // 隱藏選項
        yield return StartCoroutine(HideChoicesCoroutine());
        
        // 觸發選擇完成事件,由 DialogueManager 統一處理後續流程
        OnChoiceMade?.Invoke(choiceData);
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
        
        // 啟用射線檢測,讓按鈕可以被點擊
        panelCanvasGroup.blocksRaycasts = true;
        panelCanvasGroup.interactable = true;
        
        float elapsed = 0f;
        
        while (elapsed < panelFadeInDuration)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / panelFadeInDuration);
            yield return null;
        }
        
        panelCanvasGroup.alpha = 1f;
        Debug.Log("✅ 面板淡入完成,blocksRaycasts=" + panelCanvasGroup.blocksRaycasts);
    }
    
    /// <summary>
    /// 面板淡出
    /// </summary>
    IEnumerator FadeOutPanel()
    {
        if (panelCanvasGroup == null) yield break;
        
        // 禁用射線檢測和互動
        panelCanvasGroup.blocksRaycasts = false;
        panelCanvasGroup.interactable = false;
        
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
    
    /// <summary>
    /// 為按鈕添加 Hover 效果
    /// </summary>
    void AddHoverEffect(Button button)
    {
        var hoverEffect = button.gameObject.AddComponent<ChoiceButtonHoverEffect>();
        hoverEffect.hoverScale = hoverScale;
        hoverEffect.animationDuration = hoverAnimationDuration;
        hoverEffect.hoverColor = hoverColor;
        hoverEffect.enableColorChange = enableHoverColorChange;
        hoverEffect.hoverOffsetX = hoverOffsetX;
    }
}

/// <summary>
/// 選項按鈕 Hover 效果組件
/// </summary>
public class ChoiceButtonHoverEffect : MonoBehaviour, 
    UnityEngine.EventSystems.IPointerEnterHandler, 
    UnityEngine.EventSystems.IPointerExitHandler
{
    [HideInInspector] public float hoverScale = 1.05f;
    [HideInInspector] public float animationDuration = 0.15f;
    [HideInInspector] public Color hoverColor = Color.white;
    [HideInInspector] public bool enableColorChange = true;
    [HideInInspector] public float hoverOffsetX = 10f;
    
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Color originalColor;
    private Image buttonImage;
    private RectTransform rectTransform;
    private Coroutine animationCoroutine;
    private bool isInitialized = false;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
    }
    
    /// <summary>
    /// 在按鈕出現動畫完成後調用此方法初始化
    /// </summary>
    public void InitializeAfterAnimation()
    {
        originalScale = transform.localScale;
        originalPosition = rectTransform.anchoredPosition;
        
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        
        isInitialized = true;
        Debug.Log($"✅ Hover 效果初始化完成: originalScale = {originalScale}");
    }
    
    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (!isInitialized) return;
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateHover(true));
    }
    
    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (!isInitialized) return;
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateHover(false));
    }
    
    IEnumerator AnimateHover(bool isEntering)
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = isEntering ? (originalScale * hoverScale) : originalScale;
        
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 targetPos = isEntering ? (originalPosition + new Vector3(hoverOffsetX, 0, 0)) : originalPosition;
        
        Color startColor = buttonImage != null ? buttonImage.color : Color.white;
        Color targetColor = isEntering ? hoverColor : originalColor;
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            
            // 使用緩動函數讓動畫更流暢
            float t = isEntering ? EaseOutCubic(progress) : EaseInOutCubic(progress);
            
            // 縮放動畫
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            // 位移動畫 (水平滑動)
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
            }
            
            // 顏色動畫
            if (enableColorChange && buttonImage != null)
            {
                buttonImage.color = Color.Lerp(startColor, targetColor, t);
            }
            
            yield return null;
        }
        
        // 確保最終狀態正確
        transform.localScale = targetScale;
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = targetPos;
        }
        if (enableColorChange && buttonImage != null)
        {
            buttonImage.color = targetColor;
        }
    }
    
    // 緩動函數
    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
    
    float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}