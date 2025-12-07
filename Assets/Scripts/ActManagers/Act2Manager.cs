using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// 第二幕管理器 - S 的房間（清晨）
/// 繼承 BaseActManager，擴展特殊的手機訊息和音樂筆記本功能
/// </summary>
public class Act2Manager : BaseActManager
{
    [Header("Act2 特殊場景物件")]
    public SpriteRenderer musicBookRenderer;  // 音樂筆記本
    
    [Header("Act2 特殊 UI")]
    public GameObject phoneMessagePanel;
    public SpriteRenderer phoneRenderer;
    public TMPro.TextMeshProUGUI phoneMessageText;

    [Header("手機滑入動畫設定")]
    public Vector3 phoneHiddenPosition = new Vector3(10f, 5f, 0f);
    public Vector3 phoneShowPosition = new Vector3(5f, 5f, 0f);
    public float phoneSlideInDuration = 0.8f;
    public float phoneSlideOutDuration = 0.6f;

    [Header("背景音效")]
    public AudioSource bgmSource;
    public AudioClip morningAmbience;
    public AudioClip heartbeatSound;
    public AudioClip drawerCloseSound;
    public AudioClip knockSound;

    [Header("動畫設定")]
    public float bookFadeInDuration = 2f;
    public float bookFadeOutDuration = 0.3f;
    public float shakeIntensity = 0.15f;
    public int shakeCount = 2;
    public float backgroundFadeDuration = 1.5f;  // 新增：背景淡出時間



    protected override void Start()
    {
        // 初始化筆記本為隱藏
        if (musicBookRenderer != null)
        {
            Color c = musicBookRenderer.color;
            c.a = 0f;
            musicBookRenderer.color = c;
        }
        
        // 訂閱對話索引事件
        DialogueManager.OnDialogueIndexChanged += OnDialogueIndexChanged;
        
        // 調用基類 Start，這會自動處理基本的 Act 流程
        base.Start();
    }
    
    protected override void OnDestroy()
    {
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
        if (!isActDialogueActive) return;

        switch (dialogueIndex)
        {
            case 4:
                Debug.Log("[Act2Manager] 對話索引 4：心跳聲");
                PlayHeartbeat();
                break;
            case 5:
                Debug.Log("[Act2Manager] 對話索引 5：顯示筆記本");
                ShowMusicBook();
                break;
            case 6:
                Debug.Log("[Act2Manager] 對話索引 6：呼吸急促 + 額頭冒汗");
                // 角色表情動畫（由 CharacterManager 處理）
                break;
            case 7:
                Debug.Log("[Act2Manager] 對話索引 7：隱藏筆記本 + 抖動 + 顯示手機");
                HideMusicBookAndShake();
                ShowPhoneMessage("【開學提醒：今日為轉學生報到日】");  // 改這裡
                break;
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
            bgmSource.volume = 0.7f;
            bgmSource.loop = true;
            bgmSource.Play();
            Debug.Log("[Act2Manager] 播放清晨氛圍音");
        }
    }



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
            bgmSource.PlayOneShot(drawerCloseSound, 0.9f);
        }
    }

    public void PlayKnockSound()
    {
        if (bgmSource != null && knockSound != null)
        {
            bgmSource.PlayOneShot(knockSound, 0.7f);
        }
    }

    public void ShowMusicBook()
    {
        if (musicBookRenderer != null)
        {
            StopCoroutine(nameof(FadeInMusicBook));
            StartCoroutine(FadeInMusicBook());
        }
    }

    public void HideMusicBookAndShake()
    {
        StopAllCoroutines();
        StartCoroutine(HideBookAndShakeCharacter());
    }

    IEnumerator FadeInMusicBook()
    {
        if (musicBookRenderer == null) yield break;

        float elapsed = 0f;
        while (elapsed < bookFadeInDuration)
        {
            elapsed += Time.deltaTime;
            Color c = musicBookRenderer.color;
            c.a = Mathf.Clamp01(elapsed / bookFadeInDuration);
            musicBookRenderer.color = c;
            yield return null;
        }

        Color finalColor = musicBookRenderer.color;
        finalColor.a = 1f;
        musicBookRenderer.color = finalColor;
        Debug.Log("[Act2Manager] 筆記本淡入完成");
    }

    IEnumerator HideBookAndShakeCharacter()
    {
        yield return StartCoroutine(FadeOutMusicBook());
        yield return StartCoroutine(ShakeCharacter());
        PlayDrawerClose();
    }

    IEnumerator FadeOutMusicBook()
    {
        if (musicBookRenderer == null) yield break;

        float elapsed = 0f;
        while (elapsed < bookFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            Color c = musicBookRenderer.color;
            c.a = 1f - Mathf.Clamp01(elapsed / bookFadeOutDuration);
            musicBookRenderer.color = c;
            yield return null;
        }

        Color finalColor = musicBookRenderer.color;
        finalColor.a = 0f;
        musicBookRenderer.color = finalColor;
        Debug.Log("[Act2Manager] 筆記本淡出完成");
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
