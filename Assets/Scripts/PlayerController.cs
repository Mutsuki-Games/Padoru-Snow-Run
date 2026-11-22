using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player參數")]
    public float playerMovingSpeedOrigin;
    public float playerMovingSpeedIncTimeRate = 1f;
    public float playerMovingSpeed;
    // 玩家一直位於半徑circleRadius的圓圈跑道移動

    [Header("Navigator參數")]
    public GameObject navigator;
    // 玩家有一個navigator的child
    public float navigatorRotationSpeedOrigin;
    public float navigatorRotationSpeedIncItemRate = 1f;
    public float navigatorRotationSpeed;
    public float navigatorRotationSpeedUpRate;
    public float navigatorRotationRealSpeed;
    public float navigatorRotationShowRate;
    // 可以navigator控制Y軸旋轉速度(>0代表順時針，<0代表逆時針)
    public float navigatorMovingSpeedRate;
    // navigator會在X軸做modelMovingSpeedRate * modelRotationSpeed的等速運動
    public float navigatorMovingBoundOffset;
    // 玩家等速運動到達+-(roadWidth/2 - playerMovingBoundOffset)時，
    // 只能朝反向移動，否則不移動(但仍會繼續轉動)

    [Header("Camera參數")]
    public GameObject childCamera;
    // 玩家有一個camera的child
    public float cameraMovingSpeed;
    // camera會根據cameraMovingSpeed與navigator的X軸位子，絲滑的移動X軸到對齊navigator

    [Header("Input Action (optional)")]
    public InputActionReference moveActionRef;
    // 可不指定：若不拖，會自動用 Project-wide 的 Move
    public float moveTapThreshold = 0.5f;
    // 超過這個值視為「按下某方向」

    private InputAction moveAction;
    private bool leftHeld, rightHeld, upHeld, downHeld;

    private SceneGenerator sceneGenerator; // 具有circleRadius, roadWidth
    private GameController gameController;

    public float thetaRad; // 玩家繞圈角度（以世界原點為圓心）
    private float baseY;    // 保留Player的高度

    public enum PlayerStatus { Title, Gaming, Dead }
    public PlayerStatus playerStatus = PlayerStatus.Title;

    public void initPlayerGameStartStatus()
    {
        playerMovingSpeed = playerMovingSpeedOrigin;
        navigatorRotationSpeed = navigatorRotationSpeedOrigin;
        navigatorRotationRealSpeed = 0;
        float r = sceneGenerator.circleRadius;
        Vector3 p = gameObject.transform.position;
        gameObject.transform.position = new Vector3(r, 0f, 0f);
        gameObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        navigator.transform.localPosition = new Vector3(0f, 0f, 0f);
        playerStatus = PlayerStatus.Gaming;
    }

    public void initPlayerGameTitleStatus()
    {
        playerMovingSpeed = 0;
        navigatorRotationSpeed = 0;
        navigatorRotationRealSpeed = 0;
        float r = sceneGenerator.circleRadius;
        Vector3 p = gameObject.transform.position;
        gameObject.transform.position = new Vector3(r, 0f, 0f);
        gameObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        navigator.transform.localPosition = new Vector3(0f, 0f, 0f);
        navigator.transform.localEulerAngles = new Vector3(0f, 170f, 0f);
        childCamera.transform.localPosition = new Vector3(0.5f, 0.4f, -1.5f);
        childCamera.transform.localEulerAngles = new Vector3(-5f, 0f, 0f);
        playerStatus = PlayerStatus.Title;
    }

    public void initPlayerDeadStatus()
    {
        playerMovingSpeed = 0;
        navigatorRotationSpeed = 0;
        navigatorRotationRealSpeed = 0;
        Vector3 p = navigator.transform.localPosition;
        navigator.transform.localPosition = new Vector3(p.x, 0.3f, p.z);
        navigator.transform.localEulerAngles = new Vector3(-100f, 0f, 0f);
        playerStatus = PlayerStatus.Dead;
    }

    void Start()
    {
        // 找到 SceneGenerator
        sceneGenerator = FindFirstObjectByType<SceneGenerator>();
        if (sceneGenerator == null)
        {
            Debug.LogWarning("PlayerController: SceneGenerator not found!");
            enabled = false;
            return;
        }
        gameController = FindFirstObjectByType<GameController>();
        if (gameController == null)
        {
            Debug.LogWarning("PlayerController: GameController not found!");
            enabled = false;
            return;
        }

        // navigator 防呆：若沒指定，嘗試用子物件名找
        if (navigator == null)
        {
            Transform t = transform.Find("navigator");
            if (t != null) navigator = t.gameObject;
        }

        if (navigator == null)
        {
            Debug.LogWarning("PlayerController: navigator not assigned/found!");
            enabled = false;
            return;
        }

        // camera 防呆：若沒指定，嘗試用子物件名找
        if (childCamera == null)
        {
            Transform t = transform.Find("Main Camera");
            if (t != null) childCamera = t.gameObject;
        }

        if (childCamera == null)
        {
            Debug.LogWarning("PlayerController: childCamera not assigned/found!");
            enabled = false;
            return;
        }
        initPlayerGameTitleStatus();
        // 初始化角度（用目前位置推回來）
        Vector3 p = transform.position;
        baseY = p.y;

        // 避免 (0,0,0) 造成角度無意義
        if (p.x == 0f && p.z == 0f)
        {
            // 若玩家剛好在原點，就放到預設角度 0（+X 方向）
            thetaRad = 0f;
        }
        else
        {
            thetaRad = Mathf.Atan2(p.z, p.x);
        }
        //initPlayerGameStartStatus();
    }

    void OnEnable()
    {
        // 優先用 Inspector 指派的 Action
        if (moveActionRef != null)
        {
            moveAction = moveActionRef.action;
        }
        else
        {
            // 否則嘗試從 Project-wide Actions 找 Move
            moveAction = InputSystem.actions.FindAction("Move", true);
        }

        if (moveAction != null)
            moveAction.Enable();
    }

    void OnDisable()
    {
        if (moveAction != null)
            moveAction.Disable();
    }

    void Update()
    {

        if (moveAction == null) return;
        if (playerStatus != PlayerStatus.Gaming) return;

        Vector2 v = moveAction.ReadValue<Vector2>();

        // 方向判定（用閾值）
        bool nowLeft = v.x <= -moveTapThreshold;
        bool nowRight = v.x >= moveTapThreshold;
        bool nowUp = v.y >= moveTapThreshold;
        bool nowDown = v.y <= -moveTapThreshold;

        // 左右：改變 modelRotationSpeed 的正負
        if (nowLeft && !leftHeld)
        {
            if (navigatorRotationSpeed > 0f)
                navigatorRotationSpeed *= -1f;
        }
        else if (nowRight && !rightHeld)
        {
            if (navigatorRotationSpeed < 0f)
                navigatorRotationSpeed *= -1f;
        }

        // 更新 held 狀態（用於「只觸發一次」）
        leftHeld = nowLeft;
        rightHeld = nowRight;
        upHeld = nowUp;
        downHeld = nowDown;

        // 更新速度
        playerMovingSpeed = playerMovingSpeedOrigin + gameController.nowTime * playerMovingSpeedIncTimeRate;
        float sign = 1f;
        if (navigatorRotationSpeed < 0) 
            sign = -1;
        navigatorRotationSpeed = sign * (navigatorRotationSpeedOrigin + gameController.getSocks * navigatorRotationSpeedIncItemRate);
    }

    void FixedUpdate()
    {
        if (playerStatus == PlayerStatus.Gaming)
        {
            MovingPlayer();
            MovingNavigator();
        }
        MovingCamera();

    }

    void MovingPlayer()
    {
        if (sceneGenerator == null) return;

        float r = sceneGenerator.circleRadius;
        if (r <= 0.0001f) return;

        float dt = Time.fixedDeltaTime;

        // =========================
        // 1) 更新自身位置與旋轉（繞圈）
        // =========================

        // 線速度 v -> 角速度 w = v / r
        float angularSpeed = playerMovingSpeed / r;
        thetaRad += angularSpeed * dt;

        // 位置（固定半徑）
        Vector3 pos = new Vector3(
            Mathf.Cos(thetaRad) * r,
            baseY,
            Mathf.Sin(thetaRad) * r
        );
        transform.position = pos;

        // 朝向切線方向
        Vector3 tangent = new Vector3(
            -Mathf.Sin(thetaRad),
            0f,
            Mathf.Cos(thetaRad)
        );

        if (tangent.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(tangent.normalized, Vector3.up);
    }
    void MovingNavigator()
    {

        float dt = Time.fixedDeltaTime;
        // =========================
        // 2) 更新 navigator 的旋轉
        // =========================
        if (navigator != null)
        {

            float alphaDt = 1f - Mathf.Pow(1f - navigatorRotationSpeedUpRate * (1f / 60f), dt / (1f / 60f));
            navigatorRotationRealSpeed += (navigatorRotationSpeed - navigatorRotationRealSpeed) * alphaDt;
            //navigatorRotationRealSpeed += (navigatorRotationSpeed - navigatorRotationRealSpeed) * navigatorRotationSpeedUpRate * dt;
            Vector3 euler = navigator.transform.localEulerAngles;
            euler.y += navigatorRotationRealSpeed * dt * navigatorRotationShowRate;
            navigator.transform.localEulerAngles = euler;

            // =========================
            // 3) 判斷 navigator X 軸邊界與旋轉方向，決定是否更新X
            // =========================

            float halfRoad = sceneGenerator.roadWidth * 0.5f;
            float maxX = Mathf.Max(0f, halfRoad - navigatorMovingBoundOffset);

            Vector3 localPos = navigator.transform.localPosition;

            float deltaX = navigatorMovingSpeedRate * navigatorRotationRealSpeed * dt;

            bool atRightBoundAndGoingOut = (localPos.x >= maxX && deltaX > 0f);
            bool atLeftBoundAndGoingOut = (localPos.x <= -maxX && deltaX < 0f);

            if (!atRightBoundAndGoingOut && !atLeftBoundAndGoingOut)
            {
                localPos.x += deltaX;
                localPos.x = Mathf.Clamp(localPos.x, -maxX, maxX);
                navigator.transform.localPosition = localPos;
            }
        }
    }

    void MovingCamera()
    {
        if (childCamera == null || navigator == null) return;

        if (playerStatus == PlayerStatus.Title)
        {
            if (Mouse.current != null)
            {
                Vector2 mp = Mouse.current.position.ReadValue();
                float x = (mp.x / 1920f - 0.5f) * 0.1f;
                float y = (mp.y / 1080f - 0.5f) * 0.1f * (1080f/1920f);
                childCamera.transform.localPosition = new Vector3(0.5f + x, 0.4f + y, -1.5f);
            }
            return;
        }
        float dt = Time.fixedDeltaTime;

        Vector3 camLocal = childCamera.transform.localPosition;
        Vector3 navLocal = navigator.transform.localPosition;

        // 指數平滑的安全寫法（不吃 frame rate）
        float t = 1f - Mathf.Exp(-cameraMovingSpeed * dt);

        camLocal.x = Mathf.Lerp(camLocal.x, navLocal.x, t);

        childCamera.transform.localPosition = camLocal;

        if (playerStatus == PlayerStatus.Dead)
        {
            Vector3 p = childCamera.transform.localEulerAngles;
            if (p.x > 180f) p.x -= 360f;
            childCamera.transform.localEulerAngles = new Vector3(p.x + (15f - p.x) * 0.02f, 0f, 0f);
        }
        if (playerStatus == PlayerStatus.Gaming)
        {
            Vector3 p = childCamera.transform.localEulerAngles;
            if (p.x > 180f) p.x -= 360f;
            childCamera.transform.localEulerAngles = new Vector3(p.x + (10f - p.x) * 0.02f, 0f, 0f);
            p = childCamera.transform.localPosition;
            childCamera.transform.localPosition = new Vector3(p.x, p.y + (1.5f - p.y) * 0.02f, p.z + (-2.5f - p.z) * 0.02f);
        }
    }

}
