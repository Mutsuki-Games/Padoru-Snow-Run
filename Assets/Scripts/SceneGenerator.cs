using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class SceneGenerator : MonoBehaviour
{
    public GameObject fencePrefab; // 圍牆
    public GameObject treePrefab;  // 樹木

    public GameObject BoxPrefab;
    public GameObject SockPrefab;
    public GameObject XmasTreePrefab;
    public GameObject SnowmanPrefab;

    public GameObject Player; // 在場上的遊戲物件


    [Header("環狀參數")]
    public float circleRadius = 100f; // 跑道半徑 (公尺)

    [Header("道路參數")]
    public float roadWidth = 5f; // 跑道寬度
    public float fenceLen = 10f; // 圍牆間隔（沿圓周弧長的間距）

    [Header("樹木參數")]
    public float treeSpawnWidth = 10f; // 樹木範圍寬度（相對於 circleRadius 的甜甜圈總寬）
    public float treeDensity = 0.5f; // 每平方公尺平均密度
    public float treeMinDist = 1f; // 樹木之間最小距離
    public float treeMinScale = 0.5f; // 隨機等比例縮放的最小倍數
    public float treeMaxScale = 1f; // 隨機等比例縮放的最大倍數

    [Header("生成控制")]
    public int treeMaxAttemptsPerTree = 30; // 每棵樹的嘗試次數（拒絕採樣）
    public int treeMaxTotalAttemptsMultiplier = 40; // 總嘗試上限係數（目標棵數 * 此值）

    [Header("路障生成")]
    public float roadblockSpawnBoundOffset = 0.5f; // 生成範圍向內縮
    public float roadblockMinHorizontalDist = 1.1f; // 路障之間的最小水平距離（實際上是平面距離）
    public float roadblockGenerateFrequencyBase = 10f;   // 每秒檢查幾次生成（例如 2 = 一秒檢查兩次）
    public float roadblockGenerateFrequency = 10f;
    public float roadblockGenerateFrontDegreeGameStart = 10f; // 在玩家前方幾度隨機生成
    public float roadblockGenerateFrontDegree = 30f; // 在玩家前方幾度隨機生成
    public float roadblockGenerateFrontDegreeOffset = 0.1f; // 在玩家生成的正負範圍
    public int maxSpawnTryPerTick = 5;              // 一次檢查最多嘗試幾個隨機位置

    public float sockGenerateFrequency = 0.2f;
    public float boxGenerateFrequency = 1f;


    // private paras
    private List<GameObject> activeTrees = new List<GameObject>();
    private List<GameObject> activeFences = new List<GameObject>();

    private List<Transform> itemTransforms = new List<Transform>();
    private float roadblockGenerateTimer = 0f;
    private float boxGenerateTimer = 0f;
    private float sockGenerateTimer = 0f;

    private PlayerController playerController; // 具有thetaRad (玩家繞圈角度，以世界原點為圓心）

    public struct ObjectTransform
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public bool isActive;
        public ObjectTransform(Vector3 p, Quaternion r, Vector3 s, bool a)
        {
            localPosition = p;
            localRotation = r;
            localScale = s;
            isActive = a;
        }
    }

    private List<ObjectTransform> sceneTrees = new List<ObjectTransform>();
    private List<ObjectTransform> sceneFences = new List<ObjectTransform>();

    void Start()
    {
        if (Player == null)
        {
            UnityEngine.Debug.LogWarning("GameController: Player not assigned!");
            return;
        }
        playerController = Player.GetComponent<PlayerController>();

        generateFences("left");
        generateFences("right");
        generateTrees("in");
        generateTrees("out");
    }

    void Update()
    {
        if (playerController.playerStatus != PlayerController.PlayerStatus.Gaming) { return; }
        // 更新計時器
        roadblockGenerateTimer += Time.deltaTime;
        sockGenerateTimer += Time.deltaTime;
        boxGenerateTimer += Time.deltaTime;

        // 根據頻率換算成間隔秒數
        roadblockGenerateFrequency = roadblockGenerateFrequencyBase * (playerController.playerMovingSpeed / playerController.playerMovingSpeedOrigin);
        float roadblockInterval = (roadblockGenerateFrequency <= 0f)
            ? Mathf.Infinity
            : 1f / roadblockGenerateFrequency;
        float sockInterval = (sockGenerateFrequency <= 0f)
            ? Mathf.Infinity
            : 1f / sockGenerateFrequency;
        float boxInterval = (boxGenerateFrequency <= 0f)
            ? Mathf.Infinity
            : 1f / boxGenerateFrequency;

        // 到時間就檢查一次
        if (sockGenerateTimer >= sockInterval)
        {
            sockGenerateTimer = 0f;
            TryGenerateItem(roadblockGenerateFrontDegree, roadblockGenerateFrontDegreeOffset, "sock");
        }
        if (boxGenerateTimer >= boxInterval)
        {
            boxGenerateTimer = 0f;
            TryGenerateItem(roadblockGenerateFrontDegree, roadblockGenerateFrontDegreeOffset, "box");
        }
        if (roadblockGenerateTimer >= roadblockInterval)
        {
            roadblockGenerateTimer = 0f;
            TryGenerateItem(roadblockGenerateFrontDegree, roadblockGenerateFrontDegreeOffset);
        }
    }

    public void generateGameStartRoadblock()
    {
        float roadblockDegPerGen = playerController.playerMovingSpeedOrigin / circleRadius / roadblockGenerateFrequency * Mathf.Rad2Deg;
        float sockDegPerGen = playerController.playerMovingSpeedOrigin / circleRadius / sockGenerateFrequency * Mathf.Rad2Deg;
        float boxDegPerGen = playerController.playerMovingSpeedOrigin / circleRadius / boxGenerateFrequency * Mathf.Rad2Deg;
        for (float d = roadblockGenerateFrontDegreeGameStart;
            d < roadblockGenerateFrontDegree; d += sockDegPerGen)
        {
            TryGenerateItem(d, roadblockGenerateFrontDegreeOffset, "sock");
        }
        for (float d = roadblockGenerateFrontDegreeGameStart;
            d < roadblockGenerateFrontDegree; d += boxDegPerGen)
        {
            TryGenerateItem(d, roadblockGenerateFrontDegreeOffset, "box");
        }
        for (float d = roadblockGenerateFrontDegreeGameStart; 
            d < roadblockGenerateFrontDegree; d += roadblockDegPerGen)
        {
            TryGenerateItem(d, roadblockGenerateFrontDegreeOffset);
        }
    }

    void generateFences(string direct)
    {
        if (fencePrefab == null)
        {
            UnityEngine.Debug.LogWarning("No fencePrefab!");
            return;
        }

        if (fenceLen <= 0f)
        {
            UnityEngine.Debug.LogWarning("fenceLen must be > 0");
            return;
        }

        // 決定左右圍牆半徑
        float radius = circleRadius;
        if (direct == "left") radius -= roadWidth / 2f;
        else if (direct == "right") radius += roadWidth / 2f;
        else
        {
            UnityEngine.Debug.LogWarning($"generateFences: unknown direct='{direct}', use 'left' or 'right'");
            return;
        }

        // 圓周長與圍牆數量
        float circumference = 2f * Mathf.PI * radius;
        int count = Mathf.Max(1, Mathf.CeilToInt(circumference / fenceLen));
        float angleStep = 360f / count;

        // 以 prefab 的 Transform 當作基準（你註解裡要求的做法）
        Vector3 baseScale = fencePrefab.transform.localScale;
        Quaternion baseRotation = fencePrefab.transform.localRotation;

        // 產生圍牆
        for (int i = 0; i < count; i++)
        {
            float angleDeg = i * angleStep;
            float theta = angleDeg * Mathf.Deg2Rad;

            // 圓周位置（XZ 平面）
            Vector3 pos = new Vector3(
                Mathf.Cos(theta) * radius,
                0f,
                Mathf.Sin(theta) * radius
            );

            // 切線方向（沿著角度增加的方向）
            Vector3 tangent = new Vector3(
                -Mathf.Sin(theta),
                0f,
                Mathf.Cos(theta)
            ).normalized;

            // 讓圍牆朝切線方向
            // 注意：這裡假設你的 fencePrefab 的「前方」(Z+) 是你希望沿跑道方向的朝向。
            Quaternion lookRot = Quaternion.LookRotation(tangent, Vector3.up);

            GameObject fence = Instantiate(fencePrefab, transform);

            // 世界座標設定
            fence.transform.localPosition = pos; // 因為 parent 是 SceneGenerator
            fence.transform.localRotation = lookRot * baseRotation;
            fence.transform.localScale = baseScale;

            activeFences.Add(fence);
            sceneFences.Add(new ObjectTransform(pos, lookRot * baseRotation, baseScale, true));
        }
    }

    void generateTrees(string direct)
    {
        if (treePrefab == null)
        {
            UnityEngine.Debug.LogWarning("No treePrefab!");
            return;
        }

        if (treeSpawnWidth <= 0f)
        {
            UnityEngine.Debug.LogWarning("treeSpawnWidth must be > 0");
            return;
        }

        if (treeDensity <= 0f)
        {
            UnityEngine.Debug.LogWarning("treeDensity must be > 0");
            return;
        }

        if (treeMinDist < 0f)
        {
            UnityEngine.Debug.LogWarning("treeMinDist must be >= 0");
            return;
        }

        // 決定甜甜圈範圍的內部邊界與外部邊界半徑
        float interBoundaryRadius = circleRadius - treeSpawnWidth / 2f;
        float outerBoundaryRadius = circleRadius + treeSpawnWidth / 2f;

        if (direct == "in")
            outerBoundaryRadius = circleRadius - roadWidth / 2f - treeMinDist;
        else if (direct == "out")
            interBoundaryRadius = circleRadius + roadWidth / 2f + treeMinDist;
        else
        {
            UnityEngine.Debug.LogWarning($"generateTrees: unknown direct='{direct}', use 'in' or 'out'");
            return;
        }

        // 防呆：半徑關係不合法就不產生
        if (outerBoundaryRadius <= 0f)
        {
            UnityEngine.Debug.LogWarning("generateTrees: outerBoundaryRadius <= 0, skip.");
            return;
        }

        if (interBoundaryRadius < 0f) interBoundaryRadius = 0f;

        if (outerBoundaryRadius <= interBoundaryRadius)
        {
            UnityEngine.Debug.LogWarning($"generateTrees: invalid annulus (outer <= inner). inner={interBoundaryRadius}, outer={outerBoundaryRadius}");
            return;
        }

        // =========================
        // 1) 產生以(-outer, -outer)到(outer, outer)的正方形區域內，
        //    密度為treeDensity的隨機座標
        // =========================

        float squareSide = outerBoundaryRadius * 2f;
        float squareArea = squareSide * squareSide;

        // 期望點數（密度 * 面積）
        int expectedCount = Mathf.CeilToInt(squareArea * treeDensity);

        // 你可以視情況調大/調小這個倍率
        // 因為後面還要篩甜甜圈與 minDist，預先多抽一些點會比較穩
        int oversampleCount = Mathf.CeilToInt(expectedCount * 1.2f);

        List<Vector2> candidates = new List<Vector2>(oversampleCount);

        for (int i = 0; i < oversampleCount; i++)
        {
            float x = UnityEngine.Random.Range(-outerBoundaryRadius, outerBoundaryRadius);
            float z = UnityEngine.Random.Range(-outerBoundaryRadius, outerBoundaryRadius);
            candidates.Add(new Vector2(x, z));
        }

        // =========================
        // 2) 刪除甜甜圈範圍以外的座標項目
        // =========================

        float innerR2 = interBoundaryRadius * interBoundaryRadius;
        float outerR2 = outerBoundaryRadius * outerBoundaryRadius;

        List<Vector2> ringPoints = new List<Vector2>(candidates.Count);

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2 p = candidates[i];
            float d2 = p.sqrMagnitude;

            if (d2 >= innerR2 && d2 <= outerR2)
                ringPoints.Add(p);
        }

        if (ringPoints.Count == 0)
        {
            // 沒點就直接結束
            return;
        }

        // =========================
        // 3) 刪除與其他座標距離小於treeMinDist的項目
        //    用「貪婪保留法」：逐一嘗試加入 accepted
        // =========================

        // 洗牌讓分佈更自然
        for (int i = 0; i < ringPoints.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, ringPoints.Count);
            (ringPoints[i], ringPoints[j]) = (ringPoints[j], ringPoints[i]);
        }

        float minDist2 = treeMinDist * treeMinDist;
        List<Vector2> accepted = new List<Vector2>(ringPoints.Count);

        if (treeMinDist <= 0f)
        {
            // 不需要距離限制，全部保留
            accepted = ringPoints;
        }
        else
        {
            for (int i = 0; i < ringPoints.Count; i++)
            {
                Vector2 p = ringPoints[i];
                bool ok = true;

                // O(n^2) 檢查
                // 跑一次 Start 用，數量不會太誇張的話很夠用
                for (int k = 0; k < accepted.Count; k++)
                {
                    if ((accepted[k] - p).sqrMagnitude < minDist2)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok) accepted.Add(p);
            }
        }

        if (accepted.Count == 0)
            return;

        // =========================
        // 4) 產生樹木，用隨機函數決定產生樹木的等比例縮放
        // =========================

        Vector3 baseScale = treePrefab.transform.localScale;
        Quaternion baseRotation = treePrefab.transform.localRotation;

        for (int i = 0; i < accepted.Count; i++)
        {
            Vector2 p = accepted[i];
            Vector3 localPosition = new Vector3(p.x, 0f, p.y);
            float randomYaw = UnityEngine.Random.Range(0f, 360f);
            Quaternion localRotation = Quaternion.Euler(0f, randomYaw, 0f) * baseRotation;
            float s = UnityEngine.Random.Range(treeMinScale, treeMaxScale);
            Vector3 localScale = baseScale * s;

            GameObject tree = Instantiate(treePrefab, transform);
            tree.transform.localPosition = localPosition;
            tree.transform.localRotation = localRotation;
            tree.transform.localScale = localScale;

            activeTrees.Add(tree);
            sceneTrees.Add(new ObjectTransform(localPosition, localRotation, localScale, true));
        }
    }


    /// <summary>
    /// 嘗試在玩家前方生成一個路障（用多次隨機嘗試）
    /// </summary>
    private void TryGenerateItem(float centerDegree, float offsetDegree, string type = "block")
    {
        // 清掉已經被 Destroy 的元素
        itemTransforms.RemoveAll(rb => rb == null);

        // 多次嘗試找一個不太擁擠的位置
        for (int i = 0; i < maxSpawnTryPerTick; i++)
        {
            // 1. 決定「前後」：在玩家前方 0 ~ roadblockGenerateFrontDegree 度之間
            float offsetDeg = Random.Range(
                centerDegree - offsetDegree,
                centerDegree + offsetDegree);
            float angleRad = playerController.thetaRad + offsetDeg * Mathf.Deg2Rad;

            // 2. 決定「左右」：在道路寬度內隨機
            float halfWidth = roadWidth * 0.5f - roadblockSpawnBoundOffset;
            float horizontalOffset = Random.Range(-halfWidth, halfWidth);

            // 3. 把（角度＋內外距離）轉成世界座標
            Vector3 worldPos = TrackToWorld(angleRad, horizontalOffset);

            // 4. 檢查是否跟其他路障太近（平面距離）
            if (IsTooCloseToOthers(worldPos))
                continue;

            // 5. 生成路障，並記錄在 List 裡
            GameObject genPrefab;
            if (type == "block")
            {
                float dice = Random.Range(0f, 1f);
                if (dice < 0.5f)
                    genPrefab = XmasTreePrefab;
                else
                    genPrefab = SnowmanPrefab;
            }
            else if (type == "box")
            {
                genPrefab = BoxPrefab;
            }
            else
            {
                genPrefab = SockPrefab;
            }
            Transform rb = Instantiate(genPrefab, worldPos, 
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform).transform;
            itemTransforms.Add(rb);
            break; // 這次成功生出一個，就結束
        }
    }

    /// <summary>
    /// 把「沿跑道角度 + 道路寬度方向」轉成世界座標 (XZ 平面上的環)
    /// </summary>
    private Vector3 TrackToWorld(float angleRad, float horizontalOffset)
    {
        // dir：從原點指向這個角度的方向
        Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));

        // circleRadius 是跑道中線半徑，horizontalOffset 往內外偏移
        float radius = circleRadius + horizontalOffset;

        return dir * radius; // y = 0
    }

    /// <summary>
    /// 判斷候選位置是否離現有路障太近（只看 XZ 平面距離）
    /// </summary>
    private bool IsTooCloseToOthers(Vector3 candidatePos)
    {
        Vector3 a = candidatePos;
        a.y = 0f;

        foreach (var rb in itemTransforms)
        {
            if (rb == null) continue;

            Vector3 b = rb.position;
            b.y = 0f;

            float dist = Vector3.Distance(a, b);
            if (dist < roadblockMinHorizontalDist)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 在 Scene 視窗畫出玩家前方「生成扇形區域」的 Gizmos，方便調參
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (playerController == null) return;

        float startAngleRad = playerController.thetaRad;
        float endAngleRad = playerController.thetaRad + roadblockGenerateFrontDegree * Mathf.Deg2Rad;

        float innerR = circleRadius - roadWidth * 0.5f;
        float outerR = circleRadius + roadWidth * 0.5f;

        Gizmos.color = Color.yellow;
        int step = 16;

        // 畫內外圈線
        Vector3 prevInner = Vector3.zero;
        Vector3 prevOuter = Vector3.zero;

        for (int i = 0; i <= step; i++)
        {
            float t = i / (float)step;
            float angle = Mathf.Lerp(startAngleRad, endAngleRad, t);

            Vector3 inner = new Vector3(Mathf.Cos(angle) * innerR, 0f, Mathf.Sin(angle) * innerR);
            Vector3 outer = new Vector3(Mathf.Cos(angle) * outerR, 0f, Mathf.Sin(angle) * outerR);

            if (i > 0)
            {
                Gizmos.DrawLine(prevInner, inner);
                Gizmos.DrawLine(prevOuter, outer);
            }

            prevInner = inner;
            prevOuter = outer;
        }

        // 兩條邊界線
        Vector3 leftInner = new Vector3(Mathf.Cos(startAngleRad) * innerR, 0f, Mathf.Sin(startAngleRad) * innerR);
        Vector3 leftOuter = new Vector3(Mathf.Cos(startAngleRad) * outerR, 0f, Mathf.Sin(startAngleRad) * outerR);
        Vector3 rightInner = new Vector3(Mathf.Cos(endAngleRad) * innerR, 0f, Mathf.Sin(endAngleRad) * innerR);
        Vector3 rightOuter = new Vector3(Mathf.Cos(endAngleRad) * outerR, 0f, Mathf.Sin(endAngleRad) * outerR);

        Gizmos.DrawLine(leftInner, leftOuter);
        Gizmos.DrawLine(rightInner, rightOuter);
    }
}
