using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    // Tilemap của board (dùng để set tile trên Tilemap của Unity)
    private Tilemap m_Tilemap;

    // Kích thước bản đồ theo số ô
    public int Width;
    public int Height;

    // Mảng tile sử dụng cho mặt đất và tường (chọn ngẫu nhiên khi sinh)
    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    // Tham chiếu tới Grid (dùng để chuyển từ tọa độ ô sang thế giới)
    private Grid m_Grid;

    // Danh sách các ô trống có thể spawn đối tượng (items, enemies...)
    private List<Vector2Int> m_EmptyCellsList;

    // Mảng 2 chiều lưu thông tin ô: có thể đi qua hay không, object chứa trong ô
    private CellData[,] m_BoardData;

    // Tham chiếu tới player prefab/instance nếu cần
    public PlayerController Player;

    // Prefab cho các object sẽ spawn trên map
    public FoodObject[] FoodPrefab;
    public Enemy EnemyPrefabs;
    public WallObject WallPrefabs;
    public ExitCellObject ExitCellPrefab;

    // Lớp con chứa dữ liệu một ô
    public class CellData
    {
        // Ô có cho phép đi qua không
        public bool Passable;

        // Object (Item, Enemy, Wall, Exit...) đang nằm trên ô
        public CellObject ContainedObject;
    }
    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }
    // Chuyển từ chỉ số ô sang vị trí thế giới (Vector3) - dùng để đặt transform
    public void Init()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        m_EmptyCellsList = new List<Vector2Int>();
        m_BoardData = new CellData[Width, Height];

        // Khởi tạo từng ô: đặt tile (Wall hoặc Ground) và đánh dấu ô trống
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                // Viền map là tường, không cho đi qua
                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    // Các ô bên trong là đất, có thể đi qua và thêm vào danh sách ô trống
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                // Cập nhật Tilemap với tile vừa chọn
                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        // Loại bỏ ô spawn của player khỏi danh sách ô trống
        m_EmptyCellsList.Remove(new Vector2Int(1, 1)); // remove player spawn point

        // Sinh Exit ở góc gần cuối và loại khỏi danh sách ô trống
        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(ExitCellPrefab), endCoord);
        m_EmptyCellsList.Remove(endCoord);

        // Sinh các object khác: tường nội bộ, đồ ăn, quái
        GenerateWall();
        GenerateFood();
        GenerateEnemy();
    }
    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width
            || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }
    private void AddObject(CellObject obj, Vector2Int coord)
    {
        // Đặt object lên ô: cập nhật vị trí transform, lưu vào CellData và gọi Init
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }
    private void GenerateFood()
    {
        int foodCount = Random.Range(3, 6);
        for (int i = 0; i < foodCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);
            FoodObject newFood = Instantiate(FoodPrefab[Random.Range(0, FoodPrefab.Length)]);
            AddObject(newFood, coord);

        }
    }
    private void GenerateWall()
    {
        int wallCount = Random.Range(3, 6);
        for (int i = 0; i < wallCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);
            WallObject newWall = Instantiate(WallPrefabs);

            AddObject(newWall, coord);

        }
    }
    private void GenerateEnemy()
    {
        int enemyCount = Random.Range(1, 3);
        for (int i = 0; i < enemyCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);
            Enemy newEnemy = Instantiate(EnemyPrefabs);

            AddObject(newEnemy, coord);

        }
    }
    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }
    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }
    public void Clean()
    {
        if (m_BoardData == null)
            return;

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                var cellData = m_BoardData[x, y];
                if (cellData.ContainedObject != null)
                {
                    Destroy(cellData.ContainedObject.gameObject);
                }
                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }
}
