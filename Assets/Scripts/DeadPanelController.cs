using UnityEngine;
using TMPro;

public class DeadPanelController : MonoBehaviour
{
    public GameObject resultPanel;
    public GameObject timeQty;
    public GameObject boxQty;
    public GameObject timePt;
    public GameObject boxPt;
    public GameObject totalPt;
    public GameObject returnTitleBtn;
    public GameObject rankS;
    public GameObject rankA;
    public GameObject rankB;
    public GameObject rankC;
    public GameObject rankD;

    public float startAppearTime;
    public float intervalTime;
    public float rankIntervalTime;
    private float aniStartTime;
    private bool is_timeQty, is_timePt, is_boxQty, is_boxPt, is_totalPt, is_rank, is_returnTitleBtn, isResult;

    private GameController gameController;
    private SoundManager soundManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        gameController = FindFirstObjectByType<GameController>();
        if (gameController == null)
        {
            Debug.LogWarning("DeadPanelController: GameController not found!");
            enabled = false;
            return;
        }
        soundManager = FindFirstObjectByType<SoundManager>();
        if (soundManager == null)
        {

            UnityEngine.Debug.LogWarning("GameController: SoundManager not assigned!");
            enabled = false;
            return;
        }
        resultPanel.SetActive(false);
    }

    void OnDisable()
    {
        resultPanel.SetActive(false);
        timeQty.GetComponent<TextMeshProUGUI>().enabled = false;
        timePt.GetComponent<TextMeshProUGUI>().enabled = false;
        boxQty.GetComponent<TextMeshProUGUI>().enabled = false;
        boxPt.GetComponent<TextMeshProUGUI>().enabled = false;
        totalPt.GetComponent<TextMeshProUGUI>().enabled = false;
        rankS.GetComponent<TextMeshProUGUI>().enabled = false;
        rankA.GetComponent<TextMeshProUGUI>().enabled = false;
        rankB.GetComponent<TextMeshProUGUI>().enabled = false;
        rankC.GetComponent<TextMeshProUGUI>().enabled = false;
        rankD.GetComponent<TextMeshProUGUI>().enabled = false;
        returnTitleBtn.SetActive(false);
    }

    public void ShowResult()
    {
        resultPanel.SetActive(true);
        timeQty.GetComponent<TextMeshProUGUI>().enabled = false;
        timePt.GetComponent<TextMeshProUGUI>().enabled = false;
        boxQty.GetComponent<TextMeshProUGUI>().enabled = false;
        boxPt.GetComponent<TextMeshProUGUI>().enabled = false;
        totalPt.GetComponent<TextMeshProUGUI>().enabled = false;
        rankS.GetComponent<TextMeshProUGUI>().enabled = false;
        rankA.GetComponent<TextMeshProUGUI>().enabled = false;
        rankB.GetComponent<TextMeshProUGUI>().enabled = false;
        rankC.GetComponent<TextMeshProUGUI>().enabled = false;
        rankD.GetComponent<TextMeshProUGUI>().enabled = false;
        returnTitleBtn.SetActive(false);
        is_timeQty = false;
        is_timePt = false;
        is_boxQty = false;
        is_boxPt = false;
        is_totalPt = false;
        is_rank = false;
        is_returnTitleBtn = false;
        isResult = true;
        aniStartTime = Time.time;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isResult)
            return;
        float deltaTime = Time.time - aniStartTime;
        TextMeshProUGUI tmp;

        if (deltaTime > (startAppearTime) && !is_timeQty)
        {
            tmp = timeQty.GetComponent<TextMeshProUGUI>();
            tmp.text = $"{(int)gameController.endTime / 60}:{(int)gameController.endTime % 60:D2}";
            tmp.enabled = true;
            is_timeQty = true;
            soundManager.PlayDing();
        }
        if (deltaTime > (startAppearTime + intervalTime) && !is_timePt)
        {
            tmp = timePt.GetComponent<TextMeshProUGUI>();
            tmp.text = gameController.timePt + " pt";
            tmp.enabled = true;
            is_timePt = true;
            soundManager.PlayDing();
        }
        if (deltaTime > (startAppearTime + intervalTime * 2f) && !is_boxQty)
        {
            tmp = boxQty.GetComponent<TextMeshProUGUI>();
            tmp.text = gameController.getBoxes + "";
            tmp.enabled = true;
            is_boxQty = true;
            soundManager.PlayDing();
        }
        if (deltaTime > (startAppearTime + intervalTime * 3f) && !is_boxPt)
        {
            tmp = boxPt.GetComponent<TextMeshProUGUI>();
            tmp.text = gameController.boxesPt + " pt";
            tmp.enabled = true;
            is_boxPt = true;
            soundManager.PlayDing();
        }
        if (deltaTime > (startAppearTime + intervalTime * 4f) && !is_totalPt)
        {
            tmp = totalPt.GetComponent<TextMeshProUGUI>();
            tmp.text = gameController.totalPt + " pt";
            tmp.enabled = true;
            is_totalPt = true;
            soundManager.PlayDing();
        }
        if (deltaTime > (startAppearTime + intervalTime * 4f + rankIntervalTime) && !is_rank)
        {
            tmp = rankD.GetComponent<TextMeshProUGUI>();
            switch (gameController.totalPtRank)
            {
                case 1: tmp = rankS.GetComponent<TextMeshProUGUI>(); break;
                case 2: tmp = rankA.GetComponent<TextMeshProUGUI>(); break;
                case 3: tmp = rankB.GetComponent<TextMeshProUGUI>(); break;
                case 4: tmp = rankC.GetComponent<TextMeshProUGUI>(); break;
            }
            tmp.enabled = true;
            is_rank = true;
            soundManager.PlayDing();
        }
        if (deltaTime > (startAppearTime + intervalTime * 5f + rankIntervalTime) && !is_returnTitleBtn)
        {
            returnTitleBtn.SetActive(true);
            is_returnTitleBtn = true;
        }

    }
}
