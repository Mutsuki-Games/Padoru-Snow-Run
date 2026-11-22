# PADORU Snow Run

![PADORU Snow Run](Assets/Images/cover.png)

一款以聖誕迷因 **PADORU** 為主題的 3D 冬季街機閃避跑酷小遊戲，使用 **Unity 6** 與 **Blender** 製作。  
在結冰的環狀雪道上失控加速旋轉與漂移，收集禮物盒、閃避雪人與聖誕樹，挑戰最高分與 **Rank S**！

- **itch.io 商店頁面（Windows 下載）**：https://mutsukinoyoru.itch.io/padoru-snow-run
- **版本**：`v0.1.0`
- **平台**：Windows

---

## 玩法與操作（How to Play）

| 操作 / 物件 | 說明 |
| --- | --- |
| `A` | 讓 Padoru 向左旋轉並漂移 |
| `D` | 讓 Padoru 向右旋轉並漂移 |
| **禮物盒（Gift Box）** | 收集可增加分數 |
| **聖誕襪（Sock）** | 提升旋轉與左右橫移速度（不直接加分，但更容易閃避障礙） |
| **雪人 / 聖誕樹** | 障礙物，撞到一次即 **Game Over** |

隨著生存時間增加，前進速度會越來越快；結算時會依據**生存時間**與**禮物盒數量**計算總分並給予 **S / A / B / C / D** 評價。

---

## 開發環境與技術

- **引擎**：Unity `6000.3.0f1`（Unity 6）
- **渲染管線**：URP（Universal Render Pipeline `17.3.0`）+ Unity Toon Shader（`com.unity.toonshader`）
- **輸入系統**：Unity Input System（`1.16.0`）
- **3D 建模**：Blender

---

## 專案結構

```text
Assets/
├── Fonts/        # 源石黑體 (GenSekiGothic2JP) 字型與 TMP SDF 資源
├── Images/       # 封面、Logo 與 UI 圖片
├── Materials/    # 角色與場景材質、貼圖
├── Models/       # 3D 模型（Padoru、雪人、聖誕樹、禮物盒、襪子、圍牆等）
├── Prefabs/      # 場上物件預製體
├── Scenes/       # 主場景 (0.unity)
├── Scripts/      # 遊戲核心腳本
│   ├── GameController.cs       # 遊戲流程、分數與評級控制
│   ├── PlayerController.cs     # 環狀跑道移動、旋轉漂移與相機控制
│   ├── SceneGenerator.cs       # 環狀跑道、樹木、障礙物與道具生成
│   ├── ItemController.cs       # 道具與障礙物碰撞觸發與回收
│   ├── GamePanelController.cs  # 遊戲中 HUD 與加速提示
│   ├── DeadPanelController.cs  # 結算畫面動畫與評級顯示
│   ├── SoundManager.cs         # BGM 與音效管理
│   └── GameSettingHandler.cs   # 解析度、全螢幕與 FPS 設定
└── Sounds/       # BGM 與音效檔
```

---

## 本機執行

1. 安裝 **Unity `6000.3.0f1`**（建議透過 Unity Hub）。
2. 若儲存庫啟用 Git LFS，clone 前請先執行：
   ```bash
   git lfs install
   ```
3. 使用 Unity Hub 開啟本專案資料夾。
4. 開啟場景 `Assets/Scenes/0.unity` 後點擊 **Play** 即可測試。

---

## Credits

- **Developed by**：無月の夜
- **Modeling / Art / Programming**：夜月ヤミ
- **Music Assets**：`@monnekey` · `@ugroundz1` (YouTube)
- **Font**：GenSekiGothic2 JP（SIL Open Font License 1.1）
