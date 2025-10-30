using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CropsReadController : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] List<CropData> cropDatas;
    Dictionary<TileBase, CropData> cropsFromTiles;

    private void Start()
    {
        cropsFromTiles = new Dictionary<TileBase, CropData>();

        foreach (CropData cropData in cropDatas)
        {
            foreach (TileBase tile in cropData.tiles)
            {
                if (tile == null) continue;
                if (!cropsFromTiles.ContainsKey(tile))
                    cropsFromTiles.Add(tile, cropData);
            }
        }

        Debug.Log($"✅ CropsReadController: Loaded {cropsFromTiles.Count} crop mappings.");
    }

    public Vector3Int GetGridPosition(Vector2 position, bool mousePosition)
    {
        Vector3 worldPosition;


        if (mousePosition)
        {
            worldPosition = Camera.main.ScreenToWorldPoint(position);
        }
        else
        {
            worldPosition = position;
        }


        Vector3Int gridPosition = tilemap.WorldToCell(worldPosition);

        return gridPosition;
    }


    public TileBase GetTileBase(Vector3Int gridPosition)
    {
        TileBase tile = tilemap.GetTile(gridPosition);

        return tile;
    }

    public CropData GetCropData(TileBase tilebase)
    {
        if (tilebase == null)
        {
            Debug.LogWarning("⚠️ CropsReadController.GetCropData: TileBase null.");
            return null;
        }

        if (!cropsFromTiles.TryGetValue(tilebase, out CropData cropData))
        {
            Debug.LogWarning($"⚠️ CropsReadController: Tile '{tilebase.name}' chưa có trong danh sách cropDatas. Hãy thêm nó vào trong Inspector!");
            return null;
        }

        return cropData;
    }
}


