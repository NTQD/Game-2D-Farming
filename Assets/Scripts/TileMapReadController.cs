using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapReadController : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<TileData> tileDatas;

    private Dictionary<TileBase, TileData> dataFromTiles;

    private void Start()
    {
        InitializeTileData();
    }

    /// <summary>
    /// Khởi tạo toàn bộ mapping giữa TileBase và TileData
    /// </summary>
    private void InitializeTileData()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (TileData tileData in tileDatas)
        {
            foreach (TileBase tile in tileData.tiles)
            {
                if (tile == null) continue;
                if (!dataFromTiles.ContainsKey(tile))
                    dataFromTiles.Add(tile, tileData);
            }
        }

        Debug.Log($"✅ TileMapReadController: Loaded {dataFromTiles.Count} tile mappings.");
    }

    /// <summary>
    /// Lấy vị trí grid từ vị trí chuột hoặc thế giới.
    /// </summary>
    public Vector3Int GetGridPosition(Vector2 position, bool mousePosition)
    {
        Vector3 worldPosition = mousePosition
            ? Camera.main.ScreenToWorldPoint(position)
            : (Vector3)position;

        return tilemap.WorldToCell(worldPosition);
    }

    /// <summary>
    /// Lấy tile hiện tại tại vị trí grid.
    /// </summary>
    public TileBase GetTileBase(Vector3Int gridPosition)
    {
        return tilemap.GetTile(gridPosition);
    }

    /// <summary>
    /// Lấy thông tin TileData tương ứng với TileBase.
    /// </summary>
    public TileData GetTileData(TileBase tilebase)
    {
        if (tilebase == null)
        {
            Debug.LogWarning("⚠️ TileMapReadController.GetTileData: TileBase null.");
            return null;
        }

        if (!dataFromTiles.TryGetValue(tilebase, out TileData tileData))
        {
            Debug.LogWarning($"⚠️ TileMapReadController: Tile '{tilebase.name}' chưa có trong danh sách tileDatas. Hãy thêm nó vào trong Inspector!");
            return null;
        }

        return tileData;
    }

    /// <summary>
    /// Gọi hàm này sau khi thay đổi tile động để cập nhật lại mapping (ví dụ sau khi thu hoạch).
    /// </summary>
    public void RefreshTileDataMapping()
    {
        InitializeTileData();
        Debug.Log("🔄 TileMapReadController: Refreshed tile mapping data.");
    }
}
