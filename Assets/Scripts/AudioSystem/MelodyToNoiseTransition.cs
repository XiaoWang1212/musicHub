using UnityEngine;
using System.Collections;

public class MelodyToNoiseTransition : MonoBehaviour
{
    [Header("音頻資源")]
    public AudioClip softMelody;        // 溫柔的旋律
    public AudioClip whiteNoise;        // 白噪音/嘈雜聲音
    
    [Header("音頻源")]
    public AudioSource melodySource;    // 旋律音頻源
    public AudioSource noiseSource;     // 噪音音頻源
    
    [Header("過渡設定")]
    [Range(5f, 20f)]
    public float melodyDuration = 15f;   // 旋律單獨播放時間（15秒）
    [Range(3f, 15f)]
    public float transitionDuration = 8f; // 過渡持續時間（漸弱+噪音漸強）
    [Range(0f, 1f)]
    public float melodyStartVolume = 0.7f;  // 旋律初始音量
    [Range(0f, 1f)]
    public float finalMelodyVolume = 0f;  // 最終旋律音量
    [Range(0f, 1f)]
    public float finalNoiseVolume = 0.9f; // 最終噪音音量
    
    [Header("音量曲線")]
    public AnimationCurve melodyVolumeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    public AnimationCurve noiseVolumeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("控制")]
    public KeyCode startKey = KeyCode.M;
    
    private bool isPlaying = false;
    
    void Start()
    {
        InitializeAudioSources();
        
        // 自動開始音效過渡
        StartTransition();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(startKey) && !isPlaying)
        {
            StartTransition();
        }
    }
    
    void InitializeAudioSources()
    {
        // 設置旋律音頻源
        if (melodySource == null)
        {
            melodySource = gameObject.AddComponent<AudioSource>();
        }
        melodySource.clip = softMelody;
        melodySource.loop = true;
        melodySource.volume = 0f;
        melodySource.playOnAwake = false;
        
        // 設置噪音音頻源
        if (noiseSource == null)
        {
            noiseSource = gameObject.AddComponent<AudioSource>();
        }
        noiseSource.clip = whiteNoise;
        noiseSource.loop = true;
        noiseSource.volume = 0f;
        noiseSource.playOnAwake = false;
        
        Debug.Log("✅ 音頻過渡系統已初始化");
    }
    
    [ContextMenu("開始音效過渡")]
    public void StartTransition()
    {
        if (isPlaying)
        {
            Debug.LogWarning("⚠️ 音效過渡已在進行中，忽略重複呼叫");
            return;
        }
        
        // 立即設為 true，防止重複呼叫
        isPlaying = true;
        
        StartCoroutine(MelodyToNoiseSequence());
    }
    
    IEnumerator MelodyToNoiseSequence()
    {
        Debug.Log("🎵 開始播放溫柔旋律...");
        
        // 確保從乾淨的狀態開始
        melodySource.Stop();
        noiseSource.Stop();
        
        // 階段1: 播放溫柔旋律（15秒）
        melodySource.volume = melodyStartVolume;
        melodySource.Play();
        
        Debug.Log($"📊 旋律音量設定為: {melodyStartVolume}");
        
        yield return new WaitForSeconds(melodyDuration);
        
        Debug.Log("🔊 開始音效過渡 - 噪音逐漸覆蓋旋律...");
        
        // 階段2: 在15秒時同時開始噪音和旋律衰減
        noiseSource.Play();
        
        float elapsedTime = 0f;
        float startMelodyVolume = melodySource.volume;
        float startNoiseVolume = 0f;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;
            
            // 直接使用曲線計算音量 (不要雙重插值)
            melodySource.volume = Mathf.Lerp(startMelodyVolume, finalMelodyVolume, progress);
            noiseSource.volume = Mathf.Lerp(startNoiseVolume, finalNoiseVolume, progress);
            
            yield return null;
        }
        
        // 確保最終音量設置正確
        melodySource.volume = finalMelodyVolume;
        noiseSource.volume = finalNoiseVolume;
        
        // 如果旋律音量為0，停止播放以節省性能
        if (finalMelodyVolume <= 0.01f)
        {
            melodySource.Stop();
        }
        
        Debug.Log("✅ 音效過渡完成 - 旋律被噪音完全覆蓋");
        
        isPlaying = false;
    }
    

    
    [ContextMenu("停止所有音效")]
    public void StopAllAudio()
    {
        melodySource.Stop();
        noiseSource.Stop();
        StopAllCoroutines();
        isPlaying = false;
        
        Debug.Log("🔇 所有音效已停止");
    }
    
    [ContextMenu("重置音效")]
    public void ResetAudio()
    {
        StopAllAudio();
        melodySource.volume = 0f;
        noiseSource.volume = 0f;
    }
    
    // 外部調用接口
    public void TriggerTransition()
    {
        StartTransition();
    }
    
    public bool IsPlaying()
    {
        return isPlaying;
    }
    
    // 動態調整參數
    public void SetTransitionDuration(float duration)
    {
        transitionDuration = duration;
    }
    
    public void SetFinalNoiseVolume(float volume)
    {
        finalNoiseVolume = Mathf.Clamp01(volume);
    }
    
    public void StopNoise()
    {
        noiseSource.Stop();
    }
}