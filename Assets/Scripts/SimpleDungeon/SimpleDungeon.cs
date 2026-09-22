using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleDungeon : MonoBehaviour
{
    [Header("Dungeon")]
    [Min(2)] public int roomCount = 8;
    [Min(3)] public int minRoomSize = 4;
    [Min(3)] public int maxRoomSize = 7;
    [Min(5)] public int roomSpacing = 8;
    [Min(10)] public int maxPlacementAttempts = 200;
    public bool generateOnStart = true;

    [Header("Kenney Models")]
    [Tooltip("template-floor.fbx 또는 바닥 프리팹")]
    public GameObject floorModel;
    [Tooltip("template-wall.fbx 또는 벽 프리팹")]
    public GameObject wallModel;
    [Tooltip("보물로 사용할 모델. 비어 있으면 Cube를 사용합니다.")]
    public GameObject treasureModel;
    [Min(0.1f)] public float tileSize = 2f;
    [Min(0.02f)] public float wallThickness = 0.2f;
    [Tooltip("FBX 벽의 기본 방향이 맞지 않을 때 90도 단위로 조정합니다.")]
    public float wallRotationOffset;
    public bool automaticallyFitModels = true;

    [Header("Objects")]
    public bool spawnMarkers = true;
    [Min(0)] public int enemiesPerNormalRoom = 2;

    private readonly Dictionary<Vector2Int, Room> rooms = new();
    private readonly HashSet<Vector2Int> floors = new();
    private readonly HashSet<Vector2Int> roomFloors = new();
    private Transform generatedRoot;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private void Start()
    {
        Generate();
    }

    private void Update()
    {
        // R 키를 누르면 던전을 다시 생성한다.
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            Generate();
    }

    [ContextMenu("Generate Dungeon")]
    public void Generate()
    {
        ClearDungeon();
        CreateRooms();
        ConnectRooms();
        RenderFloors();
        RenderWalls();
    }

    private void CreateRooms()
    {
        // 실습 1: 원점에 시작 방을 만든다.

        // 실습 2: 기존 방을 기준으로 새 방을 반복 생성한다.
    }

    private bool TryAddRoom(Vector2Int center, int size, RoomType type)
    {
        // 실습 3: 새 방이 기존 바닥과 겹치는지 검사한다.

        // 실습 4: 겹치지 않으면 방과 바닥 좌표를 저장한다.

        return false;
    }

    private void ConnectRooms()
    {
        // 실습 5: 두 방의 중심 좌표를 복도 생성 함수에 전달한다.
    }

    private void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        // 실습 6: X축과 Y축으로 이동하며 L자 복도를 만든다.
    }

    private void RenderFloors()
    {
        // 실습 7: 모든 바닥 좌표에 바닥 모델을 생성한다.
    }

    private void RenderWalls()
    {
        // 실습 8: 인접 바닥이 없는 방향에만 벽을 생성한다.
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        // 격자 좌표를 Unity 월드 좌표로 변환한다.
        return transform.position + new Vector3(cell.x * tileSize, 0f, cell.y * tileSize);
    }

    private void ClearDungeon()
    {
        rooms.Clear();
        floors.Clear();

        Transform oldRoot = transform.Find("Generated Dungeon");
        if (oldRoot != null)
            Destroy(oldRoot.gameObject);

        generatedRoot = new GameObject("Generated Dungeon").transform;
        generatedRoot.SetParent(transform, false);
    }
}
