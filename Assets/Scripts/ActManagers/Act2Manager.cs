using UnityEngine;
using System.Collections;

/// <summary>
/// 第二幕管理器 - S 的房間（清晨）
/// 繼承 BaseActManager，擴展特殊的手機訊息和音樂筆記本功能
/// </summary>
public class Act2Manager : BaseActManager
{
    [Header("Act2 特殊場景物件")]
    public SpriteRenderer musicBookRenderer;  // 音樂筆記本
    
    [Header("Act2 特殊 UI")]
    public GameObject phoneMessagePanel;      // 手機訊息面板
    public SpriteRenderer phoneRenderer;      // 手機圖片
    public TMPro.TextMeshProUGUI phoneMessageText; // 手機訊息文字
    
    [Header("手機滑入動畫設定")]
    public Vector3 phoneHiddenPosition = new Vector3(10f, 5f, 0f);   // 手機隱藏位置 (螢幕外)
    public Vector3 phoneShowPosition = new Vector3(5f, 5f, 0f);      // 手機顯示位置
    public float phoneSlideInDuration = 0.8f;     // 滑入時間
    public float phoneSlideOutDuration = 0.6f;    // 滑出時間

    [Header("背景音效")]
    public AudioSource bgmSource;
    public AudioClip morningAmbience;      // 清晨氛圍音
    public AudioClip heartbeatSound;       // 心跳聲
    public AudioClip drawerCloseSound;     // 抽屜關閉音效
    public AudioClip knockSound;           // 敲門聲

    [Header("動畫設定")]
    public float bookFadeInDuration = 2f;  // 筆記本淡入時間
    public float bookFadeOutDuration = 0.3f; // 筆記本淡出時間
    public float shakeIntensity = 0.15f;    // 抖動強度
    public int shakeCount = 2;              // 抖動次數



    protected override void Start()
    {
        // 初始化筆記本為隱藏
        if (musicBookRenderer != null)
        {
            Color color = musicBookRenderer.color;
            color.a = 0f;
            musicBookRenderer.color = color;
            musicBookRenderer.gameObject.SetActive(false);
        }
        
        // 訂閱對話索引事件
        DialogueManager.OnDialogueIndexChanged += OnDialogueIndexChanged;
        
        // 調用基類 Start，這會自動處理基本的 Act 流程
        base.Start();
    }
    
    protected override void OnDestroy()
    {
        // 取消訂閱
        DialogueManager.OnDialogueIndexChanged -= OnDialogueIndexChanged;
        
        // 調用基類清理
        base.OnDestroy();
    }
    
    protected override string GetActName()
    {
        return "Act2 - S的房間";
    }
    
    void OnDialogueIndexChanged(int dialogueIndex)
    {
        // 顯示當前對話索引 (方便除錯)
        Debug.Log($"📝 當前對話索引: {dialogueIndex}");
        
        // 根據對話索引觸發特定效果
        switch (dialogueIndex)
        {
            case 4:  // 第 5 句話 (索引從 0 開始)
                Debug.Log("🎵 觸發音樂筆記本顯示");
                ShowMusicBook();
                break;
            case 5:  // 第 6 句話
                Debug.Log("📔 觸發筆記本隱藏和角色抖動");
                HideMusicBookAndShake();
                break;
            case 7: // 手機訊息出現
                Debug.Log("📱 觸發手機通知顯示");
                ShowPhoneMessage("【開學提醒】\n今日為轉學生報到日");
                break;
            case 8: // 第二次按空白鍵：文字變紅 + 主角害怕抖動
                Debug.Log("😰 觸發文字變紅和主角害怕抖動");
                HighlightPhoneTextAndShakeCharacter();
                break;
            case 9: // 第三次按空白鍵：手機滑出隱藏
                Debug.Log("📱 觸發手機滑出隱藏");
                HidePhoneMessage();
                break;
                
            // 其他對話索引不需特殊處理，BaseActManager 會處理對話結束
        }
    }

    protected override IEnumerator StartActSequence()
    {
        // 調用基類的開始序列
        yield return StartCoroutine(base.StartActSequence());
        
        // Act2 特殊初始化：播放清晨氛圍音
        PlayAmbience();
    }

    void PlayAmbience()
    {
        if (bgmSource != null && morningAmbience != null)
        {
            bgmSource.clip = morningAmbience;
            bgmSource.loop = true;
            bgmSource.volume = 0.3f;
            bgmSource.Play();
        }
    }



    // 可以從 UnityEvent 或對話系統呼叫的音效方法
    public void PlayHeartbeat()
    {
        if (bgmSource != null && heartbeatSound != null)
        {
            bgmSource.PlayOneShot(heartbeatSound, 0.8f);
        }
    }

    public void PlayDrawerClose()
    {
        if (bgmSource != null && drawerCloseSound != null)
        {
            bgmSource.PlayOneShot(drawerCloseSound);
        }
    }

    public void PlayKnockSound()
    {
        if (bgmSource != null && knockSound != null)
        {
            bgmSource.PlayOneShot(knockSound, 0.8f);
        }
    }

    // 第五句話:慢慢顯現筆記本
    public void ShowMusicBook()
    {
        if (musicBookRenderer != null)
        {
            StartCoroutine(FadeInMusicBook());
        }
    }

    // 第六句話:筆記本快速消失 + 角色抖動
    public void HideMusicBookAndShake()
    {
        StartCoroutine(HideBookAndShakeCharacter());
    }

    IEnumerator FadeInMusicBook()
    {
        if (musicBookRenderer == null) yield break;

        musicBookRenderer.gameObject.SetActive(true);
        
        float elapsed = 0f;
        Color color = musicBookRenderer.color;
        
        while (elapsed < bookFadeInDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / bookFadeInDuration);
            musicBookRenderer.color = color;
            yield return null;
        }
        
        // 確保完全顯示
        color.a = 1f;
        musicBookRenderer.color = color;
    }

    IEnumerator HideBookAndShakeCharacter()
    {
        // 同時進行淡出和抖動
        Coroutine fadeOut = StartCoroutine(FadeOutMusicBook());
        Coroutine shake = StartCoroutine(ShakeCharacter());
        
        // 等待兩個動畫完成
        yield return fadeOut;
        yield return shake;
        
        // 播放抽屜關閉音效
        PlayDrawerClose();
    }

    IEnumerator FadeOutMusicBook()
    {
        if (musicBookRenderer == null) yield break;

        float elapsed = 0f;
        Color color = musicBookRenderer.color;
        float startAlpha = color.a;
        
        while (elapsed < bookFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsed / bookFadeOutDuration);
            musicBookRenderer.color = color;
            yield return null;
        }
        
        // 確保完全隱藏
        color.a = 0f;
        musicBookRenderer.color = color;
        musicBookRenderer.gameObject.SetActive(false);
    }

    IEnumerator ShakeCharacter()
    {
        // 使用 CharacterManager 獲取當前活躍角色進行抖動
        if (characterManager == null) yield break;
        
        var activeCharacter = characterManager.GetCurrentActiveCharacter();
        if (activeCharacter != null)
        {
            // 使用 BaseActManager 提供的抖動方法
            ShakeCharacter(activeCharacter.characterName, shakeIntensity, 0.1f);
        }
        
        yield return new WaitForSeconds(0.4f); // 等待抖動完成
    }



    // 顯示手機訊息 (Act2 專用)
    public void ShowPhoneMessage(string message)
    {
        StartCoroutine(ShowPhoneWithSlideIn(message));
    }
    
    // 手機滑入動畫
    IEnumerator ShowPhoneWithSlideIn(string message)
    {
        // 設定初始位置為隱藏狀態
        if (phoneRenderer != null)
        {
            phoneRenderer.transform.position = phoneHiddenPosition;
            phoneRenderer.gameObject.SetActive(true);
        }
        
        if (phoneMessagePanel != null)
        {
            phoneMessagePanel.transform.position = phoneHiddenPosition;
            if (phoneMessageText != null)
            {
                phoneMessageText.text = message;
            }
            phoneMessagePanel.SetActive(true);
        }
        
        // 滑入動畫
        yield return StartCoroutine(SlidePhoneIn());
        
        Debug.Log($"📱 手機滑入完成: {message}");
    }

    // 隱藏手機訊息
    public void HidePhoneMessage()
    {
        StartCoroutine(HidePhoneWithSlideOut());
    }
    
    // 手機滑出動畫
    IEnumerator HidePhoneWithSlideOut()
    {
        yield return StartCoroutine(SlidePhoneOut());
        
        // 動畫完成後隱藏物件
        if (phoneRenderer != null)
        {
            phoneRenderer.gameObject.SetActive(false);
        }
        
        if (phoneMessagePanel != null)
        {
            phoneMessagePanel.SetActive(false);
        }
        
        Debug.Log("📱 手機滑出完成");
    }

    // 文字變紅 + 主角抖動 (害怕效果)
    public void HighlightPhoneTextAndShakeCharacter()
    {
        StartCoroutine(HighlightTextAndShakeSequence());
    }
    
    IEnumerator HighlightTextAndShakeSequence()
    {
        // 1. 文字變紅
        if (phoneMessageText != null)
        {
            string redText = "【開學提醒】\n今日為<color=red>轉學生</color>報到日";
            phoneMessageText.text = redText;
            Debug.Log("🔴 手機文字已變紅強調");
        }

        yield return new WaitForSeconds(0.5f); // 短暫等待，增強效果感
        
        // 2. 主角抖動 (害怕效果)
        if (characterManager != null)
        {
            var activeCharacter = characterManager.GetCurrentActiveCharacter();
            if (activeCharacter != null)
            {
                // 使用 BaseActManager 的抖動方法，強度更高
                ShakeCharacter(activeCharacter.characterName, shakeIntensity * 1.5f, 0.1f);
                yield return new WaitForSeconds(0.8f); // 等待抖動完成
            }
        }
        
        Debug.Log("😰 害怕效果完成");
    }
    


    // 手機訊息淡入動畫
    IEnumerator FadeInPhoneMessage()
    {
        CanvasGroup canvasGroup = phoneMessagePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) 
        {
            canvasGroup = phoneMessagePanel.AddComponent<CanvasGroup>();
        }

        float elapsed = 0f;
        float fadeDuration = 0.5f;
        canvasGroup.alpha = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    // 手機滑入動畫
    IEnumerator SlidePhoneIn()
    {
        float elapsed = 0f;
        
        while (elapsed < phoneSlideInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / phoneSlideInDuration;
            
            // 使用 ease-out 曲線讓動畫更自然
            float smoothProgress = 1f - Mathf.Pow(1f - progress, 3f);
            
            Vector3 currentPos = Vector3.Lerp(phoneHiddenPosition, phoneShowPosition, smoothProgress);
            
            // 同時移動手機圖片和訊息面板
            if (phoneRenderer != null)
            {
                phoneRenderer.transform.position = currentPos;
            }
            
            if (phoneMessagePanel != null)
            {
                phoneMessagePanel.transform.position = currentPos;
            }
            
            yield return null;
        }
        
        // 確保最終位置正確
        if (phoneRenderer != null)
        {
            phoneRenderer.transform.position = phoneShowPosition;
        }
        if (phoneMessagePanel != null)
        {
            phoneMessagePanel.transform.position = phoneShowPosition;
        }
    }
    
    // 手機滑出動畫
    IEnumerator SlidePhoneOut()
    {
        float elapsed = 0f;
        
        while (elapsed < phoneSlideOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / phoneSlideOutDuration;
            
            // 使用 ease-in 曲線
            float smoothProgress = Mathf.Pow(progress, 2f);
            
            Vector3 currentPos = Vector3.Lerp(phoneShowPosition, phoneHiddenPosition, smoothProgress);
            
            // 同時移動手機圖片和訊息面板
            if (phoneRenderer != null)
            {
                phoneRenderer.transform.position = currentPos;
            }
            
            if (phoneMessagePanel != null)
            {
                phoneMessagePanel.transform.position = currentPos;
            }
            
            yield return null;
        }
    }

    protected override IEnumerator ActEndingSequence()
    {
        Debug.Log("🎬 開始 Act2 特殊結束序列");

        // 停止背景音樂
        if (bgmSource != null)
        {
            yield return StartCoroutine(FadeOutBGM());
        }

        // 調用基類的結束序列（處理對話淡出、背景淡出、場景轉換）
        yield return StartCoroutine(base.ActEndingSequence());
    }

    // BGM 淡出
    IEnumerator FadeOutBGM()
    {
        if (bgmSource == null || !bgmSource.isPlaying) yield break;

        float elapsed = 0f;
        float fadeDuration = 1.5f;
        float startVolume = bgmSource.volume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();
    }


}
