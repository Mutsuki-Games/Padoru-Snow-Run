using System.Diagnostics;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject Player; // 在場上的遊戲物件
    public GameObject TitlePanel;
    public GameObject GamingPanel;
    public GameObject DeadPanel;
    public GameObject ShowHowToPlay;
    public GameObject ShowCredit;

    [Header("分數參數")]
    public int boxesPtRate = 1000;
    public int timePtRate = 10;
    public int boundRankS;
    public int boundRankA;
    public int boundRankB;
    public int boundRankC;

    [Header("分數狀況")]
    public float startTime = 0f;
    public float endTime = 0f;
    public float nowTime => Time.time - startTime;
    public int getSocks = 0;
    public int getBoxes = 0;
    public int boxesPt = 0;
    public int timePt = 0;
    public int totalPt = 0;
    public int totalPtRank = 0;


    private SceneGenerator sceneGenerator; // 具有circleRadius
    private PlayerController playerController;
    private SoundManager soundManager;

    void Start()
    {
        // 找到 SceneGenerator
        sceneGenerator = FindFirstObjectByType<SceneGenerator>();
        if (sceneGenerator == null)
        {
            UnityEngine.Debug.LogWarning("GameController: SceneGenerator not found!");
            return;
        }

        if (Player == null)
        {
            UnityEngine.Debug.LogWarning("GameController: Player not assigned!");
            return;
        }
        playerController = Player.GetComponent<PlayerController>();

        if (TitlePanel == null)
        {
            UnityEngine.Debug.LogWarning("GameController: TitlePanel not assigned!");
            return;
        }
        soundManager = gameObject.GetComponent<SoundManager>();
        if (soundManager == null)
        {

            UnityEngine.Debug.LogWarning("GameController: SoundManager not assigned!");
            return;
        }

        BackToTitle();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // 在 Unity 編輯器裡：停止播放模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 實際 Build 出去的遊戲：關閉程式
        Application.Quit();
#endif
    }

    public void GameStart()
    {
        startTime = Time.time;
        getSocks = 0;
        getBoxes = 0;
        TitlePanel.SetActive(false);
        GamingPanel.SetActive(true);
        DeadPanel.SetActive(false);
        GamingPanel.GetComponent<GamePanelController>().resetSpeedUpHint();
        playerController.initPlayerGameStartStatus();
        soundManager.SetBgmGaming();
    }

    public void GameOver()
    {
        endTime = nowTime;
        boxesPt = getBoxes * boxesPtRate;
        timePt = (int)(endTime * timePtRate);
        totalPt = boxesPt + timePt;
        totalPtRank = (totalPt >= boundRankS) ? 1 :
            (totalPt >= boundRankA) ? 2 :
            (totalPt >= boundRankB) ? 3 :
            (totalPt >= boundRankC) ? 4 :
            5;
        TitlePanel.SetActive(false);
        GamingPanel.SetActive(false);
        DeadPanel.SetActive(true);
        playerController.initPlayerDeadStatus();

        soundManager.SetBgmNone();
        soundManager.PlayDead();
    }

    public void BackToTitle()
    {
        ItemController.DestroyAll();
        TitlePanel.SetActive(true);
        GamingPanel.SetActive(false);
        DeadPanel.SetActive(false);
        playerController.initPlayerGameTitleStatus();
        sceneGenerator.generateGameStartRoadblock();
        soundManager.SetBgmTitle();
    }

    public void toShowHowToPlay()
    {
        ShowHowToPlay.SetActive(true);
    }

    public void toHideHowToPlay()
    {
        ShowHowToPlay.SetActive(false);
    }

    public void toShowCredit()
    {
        ShowCredit.SetActive(true);
    }

    public void toHideCredit()
    {
        ShowCredit.SetActive(false);
    }
    public void addSock()
    {
        getSocks++;
        GamingPanel.GetComponent<GamePanelController>().showSpeedUpHint();
        soundManager.PlayDing();
    }

    public void addBox()
    {
        getBoxes++;
        soundManager.PlayDing();
    }

    void FixedUpdate()
    {
        
    }
}
