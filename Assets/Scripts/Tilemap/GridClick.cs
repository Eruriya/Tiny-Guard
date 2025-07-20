using System.Collections.Generic;
using UnityEngine;

public class GridClickSpawner : MonoBehaviour
{
    [Header("프리팹 설정")]
    public GameObject spawnPrefab;          // 소환할 실제 캐릭터
    public GameObject highlightPrefab;      // 기본 셀 하이라이트
    public GameObject hoverHighlightPrefab; // 마우스 오버용 하이라이트
    public GameObject previewPrefab;        // 캐릭터 미리보기 (투명)

    [Header("범위 및 셀 설정")]
    public Vector2 bottomLeft = new Vector2(-4.5f, -5.5f);
    public Vector2 topRight = new Vector2(4.5f, -1.5f);
    public float cellSize = 1f;
    public float cellPadding = 0.1f;

    private List<GameObject> highlightInstances = new();
    private GameObject hoverHighlightInstance;
    private GameObject previewInstance;

    private int width;
    private int height;
    private Vector2 startPos;

    void Start()
    {
        width = Mathf.RoundToInt((topRight.x - bottomLeft.x) / cellSize);
        height = Mathf.RoundToInt((topRight.y - bottomLeft.y) / cellSize);
        startPos = bottomLeft;
        DrawGrid();
    }

    void Update()
    {
        HandleHover();

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;

            if (IsInsideGrid(world, out Vector3 cellPos))
            {
                Instantiate(spawnPrefab, cellPos, Quaternion.identity);
                Debug.Log($"[소환됨] 위치: {cellPos}");
            }
        }
    }

    void DrawGrid()
    {
        foreach (var go in highlightInstances)
            Destroy(go);
        highlightInstances.Clear();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(
                    startPos.x + x * cellSize + cellSize * 0.5f,
                    startPos.y + y * cellSize + cellSize * 0.5f,
                    0f
                );

                GameObject cell = Instantiate(highlightPrefab, pos, Quaternion.identity, transform);
                highlightInstances.Add(cell);
            }
        }
    }

    void HandleHover()
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        if (IsInsideGrid(world, out Vector3 cellPos))
        {
            // 하이라이트 프리팹 위치
            if (hoverHighlightInstance == null)
                hoverHighlightInstance = Instantiate(hoverHighlightPrefab, cellPos, Quaternion.identity, transform);
            else
                hoverHighlightInstance.transform.position = cellPos;

            // 캐릭터 미리보기
            if (previewInstance == null)
            {
                previewInstance = Instantiate(previewPrefab, cellPos, Quaternion.identity, transform);

                // Animator 꺼줌
                Animator anim = previewInstance.GetComponent<Animator>();
                if (anim != null) anim.enabled = false;
                foreach (var child in previewInstance.GetComponentsInChildren<Animator>())
                    child.enabled = false;

                // 반투명 설정
                SetTransparent(previewInstance, 0.5f);
            }
            else
            {
                previewInstance.transform.position = cellPos;
            }
        }
        else
        {
            if (hoverHighlightInstance != null)
                hoverHighlightInstance.SetActive(false);
            if (previewInstance != null)
                previewInstance.SetActive(false);
        }

        if (hoverHighlightInstance != null && IsInsideGrid(world, out _))
            hoverHighlightInstance.SetActive(true);
        if (previewInstance != null && IsInsideGrid(world, out _))
            previewInstance.SetActive(true);
    }

    bool IsInsideGrid(Vector3 worldPos, out Vector3 cellCenter)
    {
        float x = Mathf.Floor((worldPos.x - startPos.x) / cellSize);
        float y = Mathf.Floor((worldPos.y - startPos.y) / cellSize);

        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            cellCenter = Vector3.zero;
            return false;
        }

        float px = startPos.x + x * cellSize + cellSize * 0.5f;
        float py = startPos.y + y * cellSize + cellSize * 0.5f;
        cellCenter = new Vector3(px, py, 0f);
        return true;
    }

    void SetTransparent(GameObject go, float alpha)
    {
        foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>())
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
