using UnityEngine;

public class GameSettingHandler : MonoBehaviour
{
    [Header("Singleton / Lifetime")]
    public bool dontDestroyOnLoad = true;

    [Header("Frame Rate")]
    [Tooltip("目標 FPS。注意：若 vSyncCount > 0，這個值通常會被忽略。")]
    public int targetFPS = 60;

    [Tooltip("0=關閉 VSync, 1=每個螢幕更新同步一次, 2=每兩次更新同步一次...")]
    public int vSyncCount = 0;

    [Header("Display Mode")]
    public FullScreenMode fullscreenMode = FullScreenMode.FullScreenWindow;

    [Header("Resolution")]
    public bool useCustomResolution = false;

    [Tooltip("自訂解析度寬")]
    public int width = 1920;

    [Tooltip("自訂解析度高")]
    public int height = 1080;

    [Tooltip("0 代表使用目前螢幕預設/不強制指定。")]
    public int preferredRefreshRate = 0;

    [Header("Quality (Optional)")]
    public bool applyQualityLevel = false;

    [Tooltip("Unity Quality 等級索引。0=最低。")]
    public int qualityLevelIndex = 2;

    private static GameSettingHandler _instance;

    public void Apply()
    {
        // ---- Quality ----
        if (applyQualityLevel)
        {
            qualityLevelIndex = Mathf.Clamp(qualityLevelIndex, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(qualityLevelIndex, true);
        }

        // ---- VSync & FPS ----
        vSyncCount = Mathf.Max(0, vSyncCount);
        QualitySettings.vSyncCount = vSyncCount;

        // 一般規則：
        // - vSyncCount > 0 時，Application.targetFrameRate 多半不會生效
        // - 你要「固定 60FPS」通常會選擇：vSync=0 + targetFPS=60
        targetFPS = Mathf.Max(-1, targetFPS);
        Application.targetFrameRate = targetFPS;

        // ---- Screen Mode & Resolution ----
        // 先決定要套用的解析度來源
        int applyW = Screen.currentResolution.width;
        int applyH = Screen.currentResolution.height;

        if (useCustomResolution)
        {
            applyW = Mathf.Max(1, width);
            applyH = Mathf.Max(1, height);
        }

        if (preferredRefreshRate > 0)
        {
            var rr = new RefreshRate { numerator = (uint)preferredRefreshRate, denominator = 1 };
            Screen.SetResolution(applyW, applyH, fullscreenMode, rr);
        }
        else
        {
            Screen.SetResolution(applyW, applyH, fullscreenMode);
        }
    }

    private void Awake()
    {
        // 簡單單例防重複（避免多場景重複套用）
        if (_instance != null)
        {
            return;
        }
        _instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        Apply();
    }
}
