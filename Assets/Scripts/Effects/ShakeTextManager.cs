using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ShakeTextManager : MonoBehaviour
{
    // 預設的恐怖文字內容
    private string[] defaultTexts = {
        "你聽到了什麼嗎？",
        "那個聲音越來越近了...",
        "它就在你身後。"
    };

    [System.Serializable]
    public class ShakeText
    {
        [Header("文字設定")]
        public TextMeshProUGUI textComponent;
        public string textContent;

        [Header("震動效果設定")]
        public float shakeIntensity = 4f;
        public float shakeSpeed = 25f;

        [Header("每個字符震動設定")]
        public bool perCharacterShake = true;
        public float characterShakeVariation = 1.0f;

        [Header("顯示設定")]
        public float fadeInDuration = 0.5f;
        public AudioClip soundEffect;

        [HideInInspector]
        public Vector3 originalPosition;
        [HideInInspector]
        public bool isShaking = false;
        [HideInInspector]
        public bool isVisible = false;
        [HideInInspector]
        public Vector3[][] originalVertices; // 儲存每個字符的原始頂點位置
        [HideInInspector]
        public bool isInitialized = false;
    }

    [Header("文字列表")]
    public List<ShakeText> shakeTexts = new List<ShakeText>();

    [Header("背景設定")]
    public Image backgroundImage;
    public Color backgroundColor = Color.black;

    [Header("音效過渡")]
    public MelodyToNoiseTransition audioTransition;

    [Header("音效")]
    public AudioSource audioSource;

    [Header("時間控制設定")]
    public float[] textDisplayTimes = { 15f, 20f, 25f };  // 文字顯示時間點

    [Header("筆記本動畫")]
    public MusicbookDropAnimation musicbookAnimation; // 筆記本動畫控制器
    public float textFadeOutDuration = 2f;          // 文字淡出時間

    [Header("控制設定")]
    public KeyCode nextKey = KeyCode.Space;

    private bool autoSequenceStarted = false;

    void Start()
    {
        CreateDefaultTexts();
        InitializeTexts();
        SetupBackground();

        // 直接開始文字序列（配合音樂時間軸）
        StartCoroutine(TextDisplaySequence());
    }

    void CreateDefaultTexts()
    {
        if (shakeTexts == null || shakeTexts.Count == 0)
        {
            shakeTexts = new List<ShakeText>();
            TextMeshProUGUI[] foundTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);

            for (int i = 0; i < defaultTexts.Length && i < foundTexts.Length; i++)
            {
                ShakeText newShakeText = new ShakeText();
                newShakeText.textComponent = foundTexts[i];
                newShakeText.textContent = defaultTexts[i];
                newShakeText.shakeIntensity = 5f;
                newShakeText.shakeSpeed = 30f;
                newShakeText.perCharacterShake = true;
                newShakeText.characterShakeVariation = 1.0f;
                newShakeText.fadeInDuration = 0.3f;

                if (newShakeText.textComponent != null)
                {
                    newShakeText.textComponent.text = defaultTexts[i];
                    Debug.Log($"✅ 設置文字 {i + 1}: {defaultTexts[i]}");
                }

                shakeTexts.Add(newShakeText);
            }
        }
    }

    void InitializeTexts()
    {
        foreach (ShakeText shakeText in shakeTexts)
        {
            if (shakeText.textComponent != null)
            {
                shakeText.originalPosition = shakeText.textComponent.rectTransform.localPosition;
                shakeText.textComponent.text = shakeText.textContent;

                // 初始設為透明
                Color textColor = shakeText.textComponent.color;
                textColor.a = 0f;
                shakeText.textComponent.color = textColor;

                shakeText.isVisible = false;
                shakeText.isShaking = false;
                shakeText.isInitialized = false;
            }
        }
    }

    void SetupBackground()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }

    void Update()
    {
        HandleInput();
        UpdateShakeEffects();
    }

    void HandleInput()
    {
        // 保留空方法作為未來擴展
    }

    IEnumerator TextDisplaySequence()
    {
        autoSequenceStarted = true;

        // 按照指定時間顯示文字
        for (int i = 0; i < Mathf.Min(textDisplayTimes.Length, shakeTexts.Count); i++)
        {
            // 等待到指定時間
            yield return new WaitForSeconds(textDisplayTimes[i]);

            if (shakeTexts[i].textComponent != null)
            {
                StartCoroutine(DisplayText(shakeTexts[i]));
                Debug.Log($"[{textDisplayTimes[i]}秒] 顯示文字 {i + 1}: {shakeTexts[i].textContent}");
            }
        }

        // 文字淡出
        yield return new WaitForSeconds(20f - textDisplayTimes[textDisplayTimes.Length - 1]);
        StartCoroutine(FadeOutAllTexts());

        // 淡出後音樂書動畫
        yield return new WaitForSeconds(textFadeOutDuration + 0.5f);
        musicbookAnimation.PlayMusicbookDropAnimation();
    }

    IEnumerator FadeOutAllTexts()
    {
        // 同時淡出所有可見的文字
        float elapsedTime = 0f;

        while (elapsedTime < textFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / textFadeOutDuration);

            foreach (ShakeText shakeText in shakeTexts)
            {
                if (shakeText.isVisible && shakeText.textComponent != null)
                {
                    Color textColor = shakeText.textComponent.color;
                    textColor.a = alpha;
                    shakeText.textComponent.color = textColor;
                }
            }

            yield return null;
        }

        // 停止所有震動效果
        foreach (ShakeText shakeText in shakeTexts)
        {
            shakeText.isShaking = false;
            shakeText.isVisible = false;
        }

        // 🔇 關閉噪音
        if (audioTransition != null)
        {
            audioTransition.StopNoise();
            Debug.Log("🔇 噪音已關閉");
        }

        Debug.Log("✅ 所有文字已淡出完成");
    }

    IEnumerator DisplayText(ShakeText shakeText)
    {
        if (shakeText.textComponent == null) yield break;

        // 初始化字符頂點位置
        InitializeCharacterVertices(shakeText);

        // 播放音效
        if (shakeText.soundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(shakeText.soundEffect);
        }

        // 開始震動效果
        shakeText.isShaking = true;

        // 淡入效果
        float elapsedTime = 0f;
        while (elapsedTime < shakeText.fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / shakeText.fadeInDuration);

            Color currentColor = shakeText.textComponent.color;
            currentColor.a = alpha;
            shakeText.textComponent.color = currentColor;

            yield return null;
        }

        // 確保完全不透明
        Color finalColor = shakeText.textComponent.color;
        finalColor.a = 1f;
        shakeText.textComponent.color = finalColor;

        shakeText.isVisible = true;
    }

    void InitializeCharacterVertices(ShakeText shakeText)
    {
        if (shakeText.isInitialized) return;

        TextMeshProUGUI textMesh = shakeText.textComponent;
        textMesh.ForceMeshUpdate();

        TMP_TextInfo textInfo = textMesh.textInfo;

        // 初始化頂點陣列
        shakeText.originalVertices = new Vector3[textInfo.meshInfo.Length][];

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            shakeText.originalVertices[i] = new Vector3[meshInfo.vertices.Length];
            System.Array.Copy(meshInfo.vertices, shakeText.originalVertices[i], meshInfo.vertices.Length);
        }

        shakeText.isInitialized = true;
        Debug.Log($"✅ 已初始化 {textInfo.characterCount} 個字符的頂點位置");
    }

    void UpdateShakeEffects()
    {
        foreach (ShakeText shakeText in shakeTexts)
        {
            if (shakeText.isShaking && shakeText.textComponent != null && shakeText.isInitialized)
            {
                ApplyRealPerCharacterShake(shakeText);
            }
        }
    }

    void ApplyRealPerCharacterShake(ShakeText shakeText)
    {
        TextMeshProUGUI textMesh = shakeText.textComponent;
        TMP_TextInfo textInfo = textMesh.textInfo;

        // 對每個可見字符應用獨立震動
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            // 為這個字符生成獨立的震動偏移
            Vector3 shakeOffset = GenerateCharacterShakeOffset(shakeText, i);

            // 應用震動到這個字符的四個頂點
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] = shakeText.originalVertices[materialIndex][vertexIndex + j] + shakeOffset;
            }
        }

        // 更新所有網格
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textMesh.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    Vector3 GenerateCharacterShakeOffset(ShakeText shakeText, int characterIndex)
    {
        float time = Time.time;

        // 每個字符使用不同的隨機種子和頻率
        float seedX = characterIndex * 17.3f;
        float seedY = characterIndex * 23.7f;

        // 多層震動合成，每個字符都不同
        float x = Mathf.Sin(time * shakeText.shakeSpeed + seedX) * 0.6f +
                 Mathf.Sin(time * shakeText.shakeSpeed * 2.3f + seedX) * 0.3f +
                 Mathf.Sin(time * shakeText.shakeSpeed * 4.1f + seedX) * 0.1f;

        float y = Mathf.Cos(time * shakeText.shakeSpeed * 1.1f + seedY) * 0.7f +
                 Mathf.Cos(time * shakeText.shakeSpeed * 3.7f + seedY) * 0.2f +
                 Mathf.Cos(time * shakeText.shakeSpeed * 5.9f + seedY) * 0.1f;

        // 添加隨機噪音（每個字符獨立）
        System.Random charRandom = new System.Random(characterIndex + (int)(time * 10) % 1000);
        x += ((float)charRandom.NextDouble() - 0.5f) * shakeText.characterShakeVariation;
        y += ((float)charRandom.NextDouble() - 0.5f) * shakeText.characterShakeVariation;

        // 隨機突發強烈震動
        if (charRandom.NextDouble() < 0.1f)
        {
            x *= 2.5f;
            y *= 2.5f;
        }

        return new Vector3(
            x * shakeText.shakeIntensity,
            y * shakeText.shakeIntensity,
            0f
        );
    }

    [ContextMenu("調試 - 強制顯示所有文字")]
    public void ForceShowAllTexts()
    {
        Debug.Log("=== 強制顯示所有震動文字 ===");

        for (int i = 0; i < shakeTexts.Count; i++)
        {
            if (shakeTexts[i].textComponent != null)
            {
                // 初始化頂點
                InitializeCharacterVertices(shakeTexts[i]);

                // 強制設置文字內容
                shakeTexts[i].textComponent.text = shakeTexts[i].textContent;

                // 強制設置為可見
                Color textColor = shakeTexts[i].textComponent.color;
                textColor.a = 1f;
                shakeTexts[i].textComponent.color = textColor;

                // 開始震動
                shakeTexts[i].isShaking = true;
                shakeTexts[i].isVisible = true;

                Debug.Log($"✅ 文字 {i + 1} 已開始每字符震動: {shakeTexts[i].textContent}");
            }
        }
    }

    [ContextMenu("重置所有文字")]
    public void ResetAllTexts()
    {
        autoSequenceStarted = false;

        foreach (ShakeText shakeText in shakeTexts)
        {
            if (shakeText.textComponent != null)
            {
                // 重置透明度
                Color textColor = shakeText.textComponent.color;
                textColor.a = 0f;
                shakeText.textComponent.color = textColor;

                // 重置狀態
                shakeText.isVisible = false;
                shakeText.isShaking = false;

                // 重置頂點位置
                if (shakeText.isInitialized && shakeText.originalVertices != null)
                {
                    TMP_TextInfo textInfo = shakeText.textComponent.textInfo;
                    for (int i = 0; i < textInfo.meshInfo.Length; i++)
                    {
                        if (i < shakeText.originalVertices.Length)
                        {
                            System.Array.Copy(shakeText.originalVertices[i], textInfo.meshInfo[i].vertices,
                                            shakeText.originalVertices[i].Length);
                            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                            shakeText.textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                        }
                    }
                }
            }
        }
    }

    // 外部調用方法 - 用於 MelodyToNoiseTransition
    public void DisplayTextWithShake(string text)
    {
        // 如果有可用的文字組件，使用第一個
        if (shakeTexts.Count > 0 && shakeTexts[0].textComponent != null)
        {
            ShakeText targetShakeText = shakeTexts[0];
            targetShakeText.textContent = text;
            targetShakeText.textComponent.text = text;

            StartCoroutine(DisplayText(targetShakeText));
            Debug.Log($"💬 顯示震動文字: {text}");
        }
        else
        {
            Debug.LogWarning("⚠️ 沒有可用的 ShakeText 組件來顯示文字");
        }
    }

    public void StartShaking()
    {
        // 開始所有可見文字的震動效果
        foreach (ShakeText shakeText in shakeTexts)
        {
            if (shakeText.isVisible)
            {
                shakeText.isShaking = true;
            }
        }
    }

    public void StopShaking()
    {
        // 停止所有文字的震動效果
        foreach (ShakeText shakeText in shakeTexts)
        {
            shakeText.isShaking = false;
        }
    }
}