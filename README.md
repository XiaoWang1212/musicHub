# 劇情向遊戲框架使用指南

## 概述

這個框架為Unity提供了一個完整的劇情向遊戲基礎系統，包含對話系統、人物顯示、選擇分支、背景切換等核心功能。

## 主要組件

### 1. GameManager（遊戲管理器）
- **位置**: `Assets/Scripts/GameManager.cs`
- **功能**: 統籌管理整個遊戲流程
- **設置**: 將此腳本掛載到場景中的空物件上

### 2. DialogueManager（對話管理器）
- **位置**: `Assets/Scripts/DialogueSystem/DialogueManager.cs`
- **功能**: 處理對話顯示、打字效果、選擇分支
- **需要的UI元件**:
  - dialoguePanel: 對話面板
  - characterNameText: 角色名字文本（TextMeshPro）
  - dialogueText: 對話內容文本（TextMeshPro）
  - nextButton: 下一步按鈕
  - choiceButtonContainer: 選擇按鈕容器
  - choiceButtonPrefab: 選擇按鈕預製體

### 3. CharacterDisplay（人物顯示系統）
- **位置**: `Assets/Scripts/CharacterSystem/CharacterDisplay.cs`
- **功能**: 管理角色圖片的顯示和動畫效果
- **支持位置**: 左、右、中間三個位置
- **動畫效果**: 淡入淡出、滑入滑出、彈出等

### 4. BackgroundManager（背景管理器）
- **位置**: `Assets/Scripts/BackgroundSystem/BackgroundManager.cs`
- **功能**: 處理背景圖片切換和轉場效果
- **轉場類型**: 淡化、滑動、交叉淡化等

### 5. UIManager（UI管理器）
- **位置**: `Assets/Scripts/UI/UIManager.cs`
- **功能**: 管理遊戲的所有UI界面
- **包含界面**: 主選單、遊戲界面、暫停選單、設定界面

## 快速設置步驟

### 1. 場景設置
1. 創建一個新場景
2. 創建Canvas並設置為Screen Space - Overlay
3. 在Canvas下創建以下UI結構：

```
Canvas
├── MainMenuPanel（主選單面板）
│   ├── StartButton
│   ├── LoadButton
│   ├── SettingsButton
│   └── ExitButton
├── GamePanel（遊戲面板）
│   ├── DialoguePanel（對話面板）
│   │   ├── CharacterNameText（TextMeshPro）
│   │   ├── DialogueText（TextMeshPro）
│   │   ├── NextButton
│   │   └── ChoiceContainer（選擇按鈕容器）
│   ├── CharacterLeft（角色左側Image）
│   ├── CharacterRight（角色右側Image）
│   ├── CharacterCenter（角色中間Image）
│   └── Background（背景Image）
├── PauseMenuPanel（暫停選單面板）
└── SettingsPanel（設定面板）
```

### 2. 腳本掛載
1. 創建空物件"GameManager"，掛載`GameManager.cs`
2. 創建空物件"DialogueManager"，掛載`DialogueManager.cs`
3. 創建空物件"CharacterDisplay"，掛載`CharacterDisplay.cs`
4. 創建空物件"BackgroundManager"，掛載`BackgroundManager.cs`
5. 在Canvas上掛載`UIManager.cs`

### 3. 組件連接
在各個Manager中將對應的UI元件拖拽到相應的欄位中。

### 4. 創建對話內容
1. 創建空物件並掛載`SampleDialogueCreator.cs`
2. 在Inspector中右鍵選單選擇"Create Multiple Sample Sequences"
3. 這會自動創建示範對話內容

## 使用方法

### 創建對話序列
```csharp
DialogueSequence newSequence = new DialogueSequence
{
    id = 0,
    sequenceName = "序列名稱",
    dialogues = new List<DialogueData>()
};

DialogueData dialogue = new DialogueData
{
    characterName = "角色名稱",
    dialogueText = "對話內容",
    characterSprite = characterSprite, // 角色圖片
    backgroundSprite = backgroundSprite, // 背景圖片
    choices = new List<ChoiceData>() // 選擇選項
};

newSequence.dialogues.Add(dialogue);
```

### 添加選擇分支
```csharp
dialogue.choices.Add(new ChoiceData("選項文字", 下一個對話ID));
```

### 控制角色顯示
```csharp
characterDisplay.ShowCharacter(sprite, CharacterDisplay.CharacterPosition.Left);
characterDisplay.HideCharacter(CharacterDisplay.CharacterPosition.Right);
```

### 切換背景
```csharp
backgroundManager.ChangeBackground(newBackground, BackgroundManager.TransitionType.Fade);
```

## 輸入控制

- **Space/滑鼠左鍵**: 推進對話
- **Escape**: 跳過對話/暫停遊戲
- **A鍵**: 切換自動模式
- **S鍵**: 快速存檔
- **L鍵**: 快速讀檔

## 擴展功能

### 添加音效系統
在DialogueManager中已預留了音效介面：
- `voiceAudioSource`: 語音播放
- `musicAudioSource`: 背景音樂播放

### 添加存檔系統
基礎的存檔系統已在GameManager中實現，可以根據需要擴展。

### 自定義對話效果
在DialogueData中的effect欄位可以設置特殊效果：
- None: 無效果
- Shake: 震動效果
- Flash: 閃爍效果
- SlowText: 慢速打字
- FastText: 快速打字

## 注意事項

1. 確保所有TextMeshPro組件都已正確設置字體
2. 角色圖片和背景圖片需要設置為Sprite類型
3. 選擇按鈕預製體需要包含Button和TextMeshPro組件
4. 建議在專案設置中啟用新的輸入系統以獲得更好的輸入支援

## 故障排除

### 常見問題
1. **對話不顯示**: 檢查DialogueManager的UI元件是否正確連接
2. **按鈕無反應**: 確認EventSystem存在於場景中
3. **圖片不顯示**: 檢查Sprite的Import Settings是否設置為Sprite模式
4. **音效無法播放**: 確認AudioSource組件存在且AudioClip已指派

### 除錯提示
- 啟用Console視窗查看Debug.Log輸出
- 使用Unity的Profiler監控性能
- 在遊戲運行時檢查Inspector中的變量值

---

## 恐怖音效過渡系統

### 新增功能
- **MelodyToNoiseTransition**: 負責15秒溫柔音樂漸變為恐怖噪音
- **ShakeTextManager**: 自動配合音樂時間軸顯示震動恐怖文字
  - 15秒: 第一句「你聽到了什麼嗎？」
  - 20秒: 第二句「那個聲音越來越近了...」  
  - 25秒: 第三句「它就在你身後。」

### 使用方法
1. 掛載 `MelodyToNoiseTransition.cs` 到音效物件
2. 掛載 `ShakeTextManager.cs` 到文字物件  
3. 在 ShakeTextManager 中引用 MelodyToNoiseTransition
4. 按 M 鍵啟動恐怖序列

這個框架提供了劇情向遊戲的基礎功能，並包含專業的恐怖氛圍營造系統。