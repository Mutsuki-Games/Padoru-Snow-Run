using UnityEngine;
using TMPro;

public class GamePanelController : MonoBehaviour
{
    public GameObject timeQty;
    public GameObject boxQty;
    public GameObject sockQty;
    public GameObject speedUpHint;
    public float speedUpHintFlashIntervalTime;
    public int speedUpHintFlashCount;

    private TextMeshProUGUI speedUpHintTmp;
    private GameController gameController;
    private int speedUpHintAppearCounter;
    private float speedUpHintSwitchTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameController = FindFirstObjectByType<GameController>();
        if (gameController == null)
        {
            Debug.LogWarning("GamePanelController: GameController not found!");
            enabled = false;
            return;
        }
        resetSpeedUpHint();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TextMeshProUGUI tmp;
        tmp = timeQty.GetComponent<TextMeshProUGUI>();
        tmp.text = $"{(int)gameController.nowTime / 60}:{(int)gameController.nowTime % 60:D2}";
        tmp = boxQty.GetComponent<TextMeshProUGUI>();
        tmp.text = "Gift Box: " + gameController.getBoxes;
        tmp = sockQty.GetComponent<TextMeshProUGUI>();
        tmp.text = "Sock: " + gameController.getSocks;

        if (speedUpHintAppearCounter < speedUpHintFlashCount 
            && Time.time - speedUpHintSwitchTime >= speedUpHintFlashIntervalTime)
        {
            if (speedUpHintTmp.enabled)
            {
                speedUpHintTmp.enabled = false;
                speedUpHintAppearCounter++;
            }
            else
            {
                speedUpHintTmp.enabled = true;
            }
            speedUpHintSwitchTime = Time.time;
        }
    }

    public void showSpeedUpHint()
    {
        speedUpHintAppearCounter = 0;
    }

    public void resetSpeedUpHint()
    {
        speedUpHintTmp = speedUpHint.GetComponent<TextMeshProUGUI>();
        speedUpHintTmp.enabled = false;
        speedUpHintAppearCounter = 99999;
    }
}
