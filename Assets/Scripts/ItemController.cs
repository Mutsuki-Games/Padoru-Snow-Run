using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public enum ItemType
    {
        Trap,
        Sock,
        Box
    }
    public ItemType Type;

    public static List<ItemController> itemCollection = new List<ItemController>();

    private GameController gameController;
    private PlayerController playerController;
    private bool _hasTriggered = false;
    private float thetaRad;

    public static void DestroyAll()
    {
        for (int i = itemCollection.Count - 1; i >= 0; i--)
        {
            Destroy(itemCollection[i].gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 只讓標籤為 "Player" 的物件觸發
        if (_hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            _hasTriggered = true;

            // 在這裡呼叫你的 function
            switch (Type)
            {
                case ItemType.Trap: doTrap(); break;
                case ItemType.Sock: doSock(); break;
                case ItemType.Box: doBox(); break;
            }
        }
    }

    void doTrap()
    {
        gameController.GameOver();
    }

    void doSock()
    {
        gameController.addSock();
        Destroy(gameObject);
    }

    void doBox()
    {
        gameController.addBox();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (itemCollection.Contains(this))
            itemCollection.Remove(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemCollection.Add(this);

        gameController = FindFirstObjectByType<GameController>();
        if (gameController == null)
        {
            Debug.LogWarning("PlayerController: GameController not found!");
            enabled = false;
        }
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
        {
            UnityEngine.Debug.LogWarning("GameController: PlayerController not assigned!");
            return;
        }
        // 初始化角度（用目前位置推回來）
        Vector3 p = transform.position;
        if (p.x == 0f && p.z == 0f)
        {
            thetaRad = 0f;
        }
        else
        {
            thetaRad = Mathf.Atan2(p.z, p.x);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float diff = normRad(normRad(thetaRad * Mathf.Rad2Deg) - normRad(playerController.thetaRad * Mathf.Rad2Deg));
        if (diff < 358 && diff > 200)
        {
            Destroy(gameObject);
        }
        
        if (Type == ItemType.Trap) return;
        Vector3 p = gameObject.transform.localEulerAngles;
        p.y += 2;
        gameObject.transform.localEulerAngles = p;
    }

    float normRad(float r)
    {
        r %= 360;
        r += 360;
        r %= 360;
        return r;
    }
}
