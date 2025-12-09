using UnityEngine;
using System.Collections;

/// <summary>
/// 第六幕管理器 - 導師辦公室場景
/// 包含角色動作控制系統
/// </summary>
public class Act6Manager : BaseActManager
{
    [Header("角色動作觸發設定")]
    [Tooltip("在指定的對話索引觸發角色動作")]
    public CharacterActionTrigger[] actionTriggers;

    [Header("背景音效")]
    public AudioSource bgmSource;
    public AudioClip actBGM; // <-- 這個欄位已存在，用於接收 BGM 檔案

    protected override string GetActName()
    {
        return "Act6 - 導師辦公室";
    }

    protected override void Start()
    {
        base.Start();

        // 訂閱對話索引變化事件
        DialogueManager.OnDialogueIndexChanged += OnDialogueIndexChanged;
    }
    // =========================================================================
    // ⬇️ 新增的音樂播放邏輯 ⬇️
    // =========================================================================

    protected override IEnumerator StartActSequence()
    {
        // 調用基類的開始序列（通常處理淡入、對話開始）
        yield return StartCoroutine(base.StartActSequence());

        // 【新增】 Act4 特殊初始化：播放 BGM
        PlayBGM();
    }

    /// <summary>
    /// 播放 Act4 背景音樂
    /// </summary>
    public void PlayBGM()
    {
        if (bgmSource != null && actBGM != null)
        {
            bgmSource.clip = actBGM;
            bgmSource.volume = 0.7f; // 初始音量可以調整
            bgmSource.loop = true;
            bgmSource.Play();
            Debug.Log("[Act4Manager] Act BGM 開始播放");
        }
        else
        {
            Debug.LogWarning("⚠️ 無法播放 BGM: Bgm Source 或 Act BGM 尚未在 Inspector 中綁定。");
        }
    }

    // =========================================================================
    // ⬆️ 新增的音樂播放邏輯 ⬆️
    // =========================================================================


    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 取消訂閱
        DialogueManager.OnDialogueIndexChanged -= OnDialogueIndexChanged;
    }

    /// <summary>
    /// 對話索引變化處理
    /// </summary>
    void OnDialogueIndexChanged(int dialogueIndex)
    {
        // 檢查動作觸發器陣列
        if (actionTriggers == null || actionTriggers.Length == 0)
        {
            return;
        }

        // 檢查是否有對應的動作觸發器
        foreach (var trigger in actionTriggers)
        {
            if (trigger.dialogueIndex == dialogueIndex)
            {
                ExecuteCharacterAction(trigger);
            }
        }
    }

    /// <summary>
    /// 執行角色動作
    /// </summary>
    void ExecuteCharacterAction(CharacterActionTrigger trigger)
    {
        // 檢查 SpriteRenderer 是否存在
        if (trigger.targetRenderer == null)
        {
            return;
        }

        // 使用Inspector設定的參數，只有當參數為0時才使用預設值
        float actualIntensity = trigger.intensity > 0 ? trigger.intensity : 0.05f;
        float actualJumpHeight = trigger.jumpHeight > 0 ? trigger.jumpHeight : 0.1f;
        float actualDuration = trigger.duration > 0 ? trigger.duration : 0.4f;

        switch (trigger.actionType)
        {
            case CharacterActionType.Shake:
                StartCoroutine(ShakeRendererCoroutine(trigger.targetRenderer, actualIntensity, actualDuration));
                break;

            case CharacterActionType.JumpOnce:
                StartCoroutine(JumpRendererCoroutine(trigger.targetRenderer, 1, actualJumpHeight, actualDuration));
                break;

            case CharacterActionType.JumpTwice:
                StartCoroutine(JumpRendererCoroutine(trigger.targetRenderer, 2, actualJumpHeight, actualDuration));
                break;
        }
    }

    /// <summary>
    /// 搖動 SpriteRenderer 協程 - 左右抖動兩次，確保回到原位置
    /// </summary>
    IEnumerator ShakeRendererCoroutine(SpriteRenderer renderer, float intensity, float duration)
    {
        // 記錄原始位置
        Vector3 originalPosition = renderer.transform.position;

        // 左右抖動兩次
        float singleShakeDuration = duration / 5f; // 總共5個動作：右左右左回原位

        for (int shake = 0; shake < 2; shake++)
        {
            // 向右
            yield return StartCoroutine(SmoothMoveToPositionAct6(renderer,
                originalPosition + new Vector3(intensity, 0f, 0f), singleShakeDuration));

            // 向左
            yield return StartCoroutine(SmoothMoveToPositionAct6(renderer,
                originalPosition + new Vector3(-intensity, 0f, 0f), singleShakeDuration));
        }

        // 確保回到原始位置
        yield return StartCoroutine(SmoothMoveToPositionAct6(renderer, originalPosition, singleShakeDuration));

        // 強制設置到精確位置
        renderer.transform.position = originalPosition;
    }

    /// <summary>
    /// 平滑移動到指定位置 - Act6專用
    /// </summary>
    IEnumerator SmoothMoveToPositionAct6(SpriteRenderer renderer, Vector3 targetPosition, float moveDuration)
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
    /// 跳躍 SpriteRenderer 協程 - 絕對控制模式
    /// </summary>
    IEnumerator JumpRendererCoroutine(SpriteRenderer renderer, int jumpCount, float jumpHeight, float duration)
    {
        for (int i = 0; i < jumpCount; i++)
        {
            // 每次跳躍都重新獲取當前位置作為基準
            Vector3 jumpBasePosition = renderer.transform.position;

            float elapsed = 0f;

            // 上升階段
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (duration / 2f);

                // 計算目標Y位置
                float targetY = jumpBasePosition.y + (jumpHeight * Mathf.Sin(progress * Mathf.PI / 2f));
                Vector3 targetPosition = new Vector3(jumpBasePosition.x, targetY, jumpBasePosition.z);

                // 強制設定位置
                renderer.transform.position = targetPosition;

                yield return new WaitForSeconds(0.05f);  // 固定間隔而非每幀
            }

            // 下降階段
            elapsed = 0f;
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (duration / 2f);

                // 計算目標Y位置
                float targetY = jumpBasePosition.y + jumpHeight - (jumpHeight * Mathf.Sin(progress * Mathf.PI / 2f));
                Vector3 targetPosition = new Vector3(jumpBasePosition.x, targetY, jumpBasePosition.z);

                // 強制設定位置
                renderer.transform.position = targetPosition;

                yield return new WaitForSeconds(0.05f);  // 固定間隔而非每幀
            }
        }
    }

    /// <summary>
    /// 角色動作觸發器 - 可在 Inspector 中設定
    /// </summary>
    [System.Serializable]
    public class CharacterActionTrigger
    {
        [Header("觸發設定")]
        [Tooltip("在第幾句對話觸發動作（從 0 開始）")]
        public int dialogueIndex;

        [Header("角色設定")]
        [Tooltip("直接拖拉要執行動作的角色 SpriteRenderer")]
        public SpriteRenderer targetRenderer;

        [Tooltip("角色名稱（僅用於顯示，可選）")]
        public string characterName = "角色名稱";

        [Header("動作設定")]
        [Tooltip("動作類型")]
        public CharacterActionType actionType = CharacterActionType.JumpOnce;

        [Header("參數設定")]
        [Tooltip("搖動強度 (Shake 專用)")]
        [Range(0.005f, 1.0f)]
        public float intensity = 0.3f;

        [Tooltip("跳躍高度 (Jump 專用)")]
        [Range(0.01f, 1.0f)]
        public float jumpHeight = 0.2f;

        [Tooltip("動作持續時間")]
        [Range(0.05f, 2.0f)]
        public float duration = 0.1f;

        [Header("表情設定 (ChangeExpression 專用)")]
        [Tooltip("要切換的表情名稱 (普通/開心/難過/驚訝/憤怒/害怕/困惑/害羞)")]
        public string expressionName = "普通";

        [Tooltip("是否使用表情切換動畫")]
        public bool useExpressionAnimation = true;
    }
}
    