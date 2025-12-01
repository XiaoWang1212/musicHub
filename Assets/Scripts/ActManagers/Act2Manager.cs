using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Act2Manager : MonoBehaviour
{
    [Header("對話系統")]
    public DialogueManager dialogueManager;
    public DialogueSequenceAsset act2DialogueSequence;

    [Header("場景物件")]
    public SpriteRenderer musicBookRenderer;
    public CharacterManager characterManager;
    public SpriteRenderer backgroundRenderer;

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

    [Header("時間控制")]
    public float initialDelay = 1f;

    [Header("轉場設定")]
    public TransitionTrigger transitionToAct3;

    [Header("內部狀態")]
    private bool isAct2DialogueActive = false;
    private bool act2Finished = false;  // 新增：防止重複執行

    void Start()
    {
        CleanUpMultipleEventSystems();

        if (musicBookRenderer != null)
        {
            Color c = musicBookRenderer.color;
            c.a = 0f;
            musicBookRenderer.color = c;
        }

        if (backgroundRenderer != null)
        {
            Color c = backgroundRenderer.color;
            c.a = 1f;
            backgroundRenderer.color = c;
        }

        // 改用靜態事件監聽
        DialogueManager.OnDialogueIndexChanged += OnDialogueIndexChanged;

        StartCoroutine(Act2Sequence());
    }

    void OnDestroy()
    {
        DialogueManager.OnDialogueIndexChanged -= OnDialogueIndexChanged;
    }
    // ...existing code...

    void CleanUpMultipleEventSystems()
    {
        var existingSystems = FindObjectsOfType<EventSystem>(true);
        if (existingSystems.Length > 1)
        {
            Debug.LogWarning($"[Act2Manager] 發現 {existingSystems.Length} 個 EventSystem，移除多餘的。");
            for (int i = 1; i < existingSystems.Length; i++)
            {
                if (existingSystems[i] != null)
                {
                    Destroy(existingSystems[i].gameObject);
                }
            }
        }
    }


    void OnDialogueIndexChanged(int dialogueIndex)
    {
        if (!isAct2DialogueActive) return;

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
            case 9:
                Debug.Log("[Act2Manager] 對話索引 9：對話完成，開始 Act2 結束序列");
                if (!act2Finished)
                {
                    act2Finished = true;
                    StartCoroutine(HidePhoneAndEndAct2());  // 改這裡
                }
                break;
        }
    }
    // 新增：顯示手機訊息
    void ShowPhoneMessage(string message)
    {
        if (phoneMessagePanel != null)
        {
            phoneMessagePanel.SetActive(true);  // 啟用手機訊息面板
            if (phoneMessageText != null)
                phoneMessageText.text = message;
        }
        StartCoroutine(SlidePhoneIn());
    }

    // 新增：隱藏手機
    void HidePhoneMessage()
    {
        StartCoroutine(SlidePhoneOut());
    }

    // 新增：手機滑入動畫
    IEnumerator SlidePhoneIn()
    {
        if (phoneRenderer == null) yield break;

        phoneRenderer.gameObject.SetActive(true);
        Vector3 startPos = phoneHiddenPosition;
        Vector3 endPos = phoneShowPosition;
        float elapsed = 0f;

        while (elapsed < phoneSlideInDuration)
        {
            elapsed += Time.deltaTime;
            phoneRenderer.transform.localPosition = Vector3.Lerp(startPos, endPos, elapsed / phoneSlideInDuration);
            yield return null;
        }

        phoneRenderer.transform.localPosition = endPos;
        Debug.Log("[Act2Manager] 手機滑入完成，訊息已顯示");
    }

    // 新增：手機滑出動畫
    IEnumerator SlidePhoneOut()
    {
        if (phoneRenderer == null) yield break;

        Vector3 startPos = phoneShowPosition;
        Vector3 endPos = phoneHiddenPosition;
        float elapsed = 0f;

        while (elapsed < phoneSlideOutDuration)
        {
            elapsed += Time.deltaTime;
            phoneRenderer.transform.localPosition = Vector3.Lerp(startPos, endPos, elapsed / phoneSlideOutDuration);
            yield return null;
        }

        phoneRenderer.transform.localPosition = endPos;
        phoneRenderer.gameObject.SetActive(false);
        if (phoneMessagePanel != null)
            phoneMessagePanel.SetActive(false);
        Debug.Log("[Act2Manager] 手機滑出完成");
    }
    IEnumerator HidePhoneAndEndAct2()
    {
        yield return StartCoroutine(SlidePhoneOut());
        yield return StartCoroutine(Act2Ending());
    }


    IEnumerator Act2Sequence()
    {
        yield return new WaitForSeconds(initialDelay);

        Debug.Log("[Act2Manager] 🎬 Act2 開始");
        PlayAmbience();

        isAct2DialogueActive = true;
        StartDialogue();
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

    void StartDialogue()
    {
        if (act2DialogueSequence != null && dialogueManager != null)
        {
            dialogueManager.StartDialogue(act2DialogueSequence);
            Debug.Log("[Act2Manager] 開始 Act2 對話序列");
        }
        else
        {
            Debug.LogError("[Act2Manager] DialogueSequenceAsset 或 DialogueManager 未指派");
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
        if (characterManager == null) yield break;

        var activeCharacter = characterManager.GetCurrentActiveCharacter();
        if (activeCharacter == null || activeCharacter.renderer == null) yield break;

        Vector3 originalPos = activeCharacter.renderer.transform.localPosition;

        for (int i = 0; i < shakeCount; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Vector3 randomOffset = Random.insideUnitCircle * shakeIntensity;
                activeCharacter.renderer.transform.localPosition = originalPos + randomOffset;
                yield return new WaitForSeconds(0.05f);
            }
        }

        activeCharacter.renderer.transform.localPosition = originalPos;
        Debug.Log("[Act2Manager] 角色抖動完成");
    }

    IEnumerator Act2Ending()
    {
        Debug.Log("[Act2Manager] 🎬 開始 Act2 結束序列");

        isAct2DialogueActive = false;

        // 淡出對話系統
        if (dialogueManager != null)
        {
            yield return StartCoroutine(dialogueManager.FadeOutDialoguePanel());
        }

        // 淡出背景
        yield return StartCoroutine(FadeOutBackground());

        // 等待短暫時間確保淡出完成
        yield return new WaitForSeconds(0.5f);

        Debug.Log("[Act2Manager] ✅ Act2 完成，轉換到 Act3");

        // 轉換到 Act3
        if (transitionToAct3 != null)
        {
            transitionToAct3.TriggerTransition();
        }
        else
        {
            Debug.LogError("[Act2Manager] transitionToAct3 未指派");
        }
    }

    IEnumerator FadeOutBackground()
    {
        if (backgroundRenderer == null) yield break;

        float elapsed = 0f;
        while (elapsed < backgroundFadeDuration)
        {
            elapsed += Time.deltaTime;
            Color c = backgroundRenderer.color;
            c.a = 1f - Mathf.Clamp01(elapsed / backgroundFadeDuration);
            backgroundRenderer.color = c;
            yield return null;
        }

        Color finalColor = backgroundRenderer.color;
        finalColor.a = 0f;
        backgroundRenderer.color = finalColor;
        Debug.Log("[Act2Manager] 背景淡出完成");
    }
}