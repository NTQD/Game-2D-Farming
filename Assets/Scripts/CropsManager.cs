using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Trạng thái động cho từng ô đất
public class TileState
{
    public bool plowable;
    public bool ableToSeed;
    public bool waterable;
    public bool ableToMow;
}

public class CropsManager : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap cropTilemap;

    [Header("Ground Tiles")]
    [SerializeField] private TileBase grass;
    [SerializeField] private TileBase dirt;
    [SerializeField] private TileBase plowed;
    [SerializeField] private TileBase watered;
    [SerializeField] private TileBase invisible;
    [SerializeField] private TileBase toWater;

    [Header("Crop Catalog (assign ScriptableObjects here)")]
    [SerializeField] private Crop[] cropCatalog;

    private Dictionary<Vector3Int, CropInstance> activeCrops = new();
    public static Dictionary<Vector2Int, TileState> fieldStates = new();
    
    // Cache references để tránh FindObjectOfType
    private TileMapReadController tileReader;
    private CropsReadController cropReader;

    private void Start()
    {
        // Cache references một lần duy nhất
        tileReader = FindObjectOfType<TileMapReadController>();
        cropReader = FindObjectOfType<CropsReadController>();
        
        if (tileReader == null)
            Debug.LogError("❌ [CropsManager] Không tìm thấy TileMapReadController!");
        if (cropReader == null)
            Debug.LogError("❌ [CropsManager] Không tìm thấy CropsReadController!");
    }

    [System.Serializable]
    private class CropInstance
    {
        public Crop crop;
        public int stage;
        public float timer;
        public bool isGrown;
    }

    // === GIEO HẠT ===
    public void SeedCrop(Vector3Int pos, Crop crop)
    {
        if (crop == null)
        {
            Debug.LogError("❌ [CropsManager] SeedCrop: Crop is null!");
            return;
        }
        if (groundTilemap == null || cropTilemap == null)
        {
            Debug.LogError("❌ [CropsManager] Tilemaps chưa được gán!");
            return;
        }
        if (activeCrops.ContainsKey(pos)) return;

        CropInstance instance = new CropInstance
        {
            crop = crop,
            stage = 0,
            timer = 0,
            isGrown = false
        };

        activeCrops.Add(pos, instance);
        cropTilemap.SetTile(pos, crop.state0);
        groundTilemap.SetTile(pos, toWater);
        
        // Tạo crop data ban đầu cho ToolsCharacterController
        CreateInitialCropData(pos, crop.cropName);
    }

    public void SeedCrop(Vector3Int pos, string cropName)
    {
        Crop c = GetCropByName(cropName);
        if (c == null)
        {
            Debug.LogWarning($"SeedCrop: Crop '{cropName}' not found in catalog.");
            return;
        }
        SeedCrop(pos, c);
    }

    public void SeedCrop(Vector2Int pos2, string cropName) => SeedCrop(new Vector3Int(pos2.x, pos2.y, 0), cropName);
    public void SeedCrop(Vector2Int pos2, Crop crop) => SeedCrop(new Vector3Int(pos2.x, pos2.y, 0), crop);

    // === CẬP NHẬT CÂY ===
    private void Update()
    {
        var keys = new List<Vector3Int>(activeCrops.Keys);

        foreach (var pos in keys)
        {
            var inst = activeCrops[pos];
            if (inst.isGrown) continue;

            TileBase currentTile = groundTilemap.GetTile(pos);
            if (currentTile != watered)
                continue;

            inst.timer += Time.deltaTime;
            // Tránh chia cho 0
            if (inst.crop == null || inst.crop.totalStages <= 0)
            {
                Debug.LogError($"❌ [CropsManager] Crop null hoặc totalStages <= 0 tại {pos}");
                continue;
            }
            float stageTime = inst.crop.growthTime / inst.crop.totalStages;

            if (inst.timer >= stageTime)
            {
                inst.timer = 0;
                inst.stage++;
                SetStage(pos, inst);
                groundTilemap.SetTile(pos, toWater);
            }
        }
    }

    private void SetStage(Vector3Int pos, CropInstance inst)
    {
        TileBase tile = null;
        switch (inst.stage)
        {
            case 0: tile = inst.crop.state0; break;
            case 1: tile = inst.crop.state1; break;
            case 2: tile = inst.crop.state2; break;
            case 3: tile = inst.crop.state3; break;
            case 4:
                tile = inst.crop.state4;
                if (inst.crop.totalStages == 5)
                {
                    inst.isGrown = true;
                    SyncCollectibleFlags(pos, inst.crop.cropName, true);
                }
                break;
            case 5:
                tile = inst.crop.state5;
                inst.isGrown = true;
                SyncCollectibleFlags(pos, inst.crop.cropName, true);
                break;
            default:
                inst.stage = Mathf.Min(inst.stage, inst.crop.totalStages);
                tile = inst.crop.state5;
                inst.isGrown = true;
                SyncCollectibleFlags(pos, inst.crop.cropName, true);
                break;
        }
        cropTilemap.SetTile(pos, tile);
    }

    // === ĐÀO ĐẤT ===
    public void Dig(Vector3Int pos)
    {
        if (groundTilemap == null)
        {
            Debug.LogError("❌ [CropsManager] groundTilemap is null!");
            return;
        }
        TileBase current = groundTilemap.GetTile(pos);

        // Cho phép đào nếu tile hiện tại là cỏ hoặc tile có khả năng mowable theo TileData
        bool canMowByTile = current == grass;
        bool canMowByData = false;
        if (tileReader != null)
        {
            TileData td = tileReader.GetTileData(current);
            canMowByData = td != null && td.ableToMow;
        }

        if (canMowByTile || canMowByData)
        {
            groundTilemap.SetTile(pos, dirt);
            Debug.Log("🪓 Shoveled -> dirt at: " + pos);

            // Đồng bộ fields cho ToolsCharacterController
            Vector2Int pos2 = new Vector2Int(pos.x, pos.y);
            if (tileReader != null)
            {
                TileBase newTile = groundTilemap.GetTile(pos);
                TileData newTileData = tileReader.GetTileData(newTile);
                if (newTileData != null)
                {
                    if (ToolsCharacterController.fields.ContainsKey(pos2))
                        ToolsCharacterController.fields[pos2] = newTileData;
                    else
                        ToolsCharacterController.fields.Add(pos2, newTileData);
                }
            }

            // Cập nhật trạng thái động cơ bản
            TileState state = new TileState
            {
                plowable = true,
                ableToSeed = false,
                waterable = false,
                ableToMow = false
            };
            if (fieldStates.ContainsKey(pos2)) fieldStates[pos2] = state; else fieldStates.Add(pos2, state);
        }
        else
        {
            Debug.Log($"⚠️ Cannot shovel at {pos}, current tile = {current?.name ?? "null"}");
        }
    }

    // === CÀY ĐẤT ===
    public void Plow(Vector3Int pos)
    {
        if (groundTilemap == null)
        {
            Debug.LogError("❌ [CropsManager] groundTilemap is null!");
            return;
        }
        TileBase current = groundTilemap.GetTile(pos);
        if (current == dirt)
        {
            groundTilemap.SetTile(pos, plowed);
            Debug.Log("⛏️ Hoed dirt -> plowed soil at: " + pos);
        }
    }

    // === TƯỚI NƯỚC ===
    public void Water(Vector3Int pos)
    {
        if (groundTilemap == null)
        {
            Debug.LogError("❌ [CropsManager] groundTilemap is null!");
            return;
        }
        TileBase current = groundTilemap.GetTile(pos);
        if (current == toWater || current == plowed)
        {
            groundTilemap.SetTile(pos, watered);
            Debug.Log("💧 Watered soil at: " + pos);
        }
        else
        {
            Debug.Log($"Cannot water at {pos}, current tile = {current?.name ?? "null"}");
        }
    }

    public void Water(Vector2Int pos2) => Water(new Vector3Int(pos2.x, pos2.y, 0));

    // === THU HOẠCH ===
    public void Harvest(Vector3Int pos)
    {
        if (cropTilemap == null || groundTilemap == null)
        {
            Debug.LogError("❌ [CropsManager] Tilemaps chưa được gán!");
            return;
        }
        if (!activeCrops.ContainsKey(pos))
        {
            Debug.Log($"❌ Không có cây nào tại ô {pos}");
            return;
        }

        var inst = activeCrops[pos];
        if (!inst.isGrown)
        {
            Debug.Log($"🌱 Cây tại {pos} chưa chín để thu hoạch.");
            return;
        }

        // Xóa cây khỏi cropTilemap
        cropTilemap.SetTile(pos, null);
        activeCrops.Remove(pos);

        // Tạo vật phẩm thu hoạch
        if (inst.crop.harvestPrefab != null)
        {
            Vector3 dropPos = cropTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0);
            Instantiate(inst.crop.harvestPrefab, dropPos, Quaternion.identity);
        }

        // Đổi tile thành dirt
        groundTilemap.SetTile(pos, dirt);

        // === Đồng bộ dữ liệu với ToolsCharacterController ===
        SyncTileWithToolController(pos, inst.crop.cropName);
        
        // Đồng bộ lại dữ liệu để đảm bảo có thể khai thác lại
        RefreshTileData(pos);

        Debug.Log($"🌾 Thu hoạch '{inst.crop.cropName}' tại {pos}. Đất đã sẵn sàng gieo lại hạt.");
    }

    // === COLLECT ===
    public void Collect(Vector3Int pos) => Harvest(pos);
    public void Collect(Vector2Int pos2) => Collect(new Vector3Int(pos2.x, pos2.y, 0));
    public void Collect(Vector3Int pos, string cropName)
    {
        if (!activeCrops.ContainsKey(pos))
        {
            Debug.Log($"❌ Không có cây nào tại {pos}");
            return;
        }

        var inst = activeCrops[pos];
        if (!inst.isGrown)
        {
            Debug.Log($"🌱 '{cropName}' tại {pos} chưa chín để thu hoạch.");
            return;
        }

        cropTilemap.SetTile(pos, null);
        activeCrops.Remove(pos);

        if (inst.crop.harvestPrefab != null)
        {
            Vector3 dropPos = cropTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0);
            Instantiate(inst.crop.harvestPrefab, dropPos, Quaternion.identity);
        }

        groundTilemap.SetTile(pos, dirt);
        SyncTileWithToolController(pos, cropName);
        
        // Đồng bộ lại dữ liệu để đảm bảo có thể khai thác lại
        RefreshTileData(pos);
        
        Debug.Log($"🌾 Đã thu hoạch '{cropName}' tại {pos}. Đất sẵn sàng gieo lại.");
    }

    // === ĐỒNG BỘ TILE DỮ LIỆU ===
    private void SyncTileWithToolController(Vector3Int pos, string cropName)
    {
        Vector2Int pos2 = new Vector2Int(pos.x, pos.y);

        if (tileReader != null)
        {
            TileBase newTile = groundTilemap.GetTile(pos);
            TileData newTileData = tileReader.GetTileData(newTile);

            // Cập nhật fields dictionary
            if (ToolsCharacterController.fields.ContainsKey(pos2))
                ToolsCharacterController.fields[pos2] = newTileData;
            else
                ToolsCharacterController.fields.Add(pos2, newTileData);

            Debug.Log($"✅ [SYNC] Đã cập nhật tile tại {pos} -> {newTile?.name ?? "null"}");
        }

        // Xóa crop khỏi crops dictionary vì đã thu hoạch
        if (ToolsCharacterController.crops.ContainsKey(pos2))
            ToolsCharacterController.crops.Remove(pos2);

        // Cập nhật crop data để đảm bảo có thể khai thác lại
        if (cropReader != null)
        {
            TileBase cropTile = cropTilemap.GetTile(pos);
            CropData cropData = cropReader.GetCropData(cropTile);
            
            if (cropData != null)
            {
                ToolsCharacterController.crops[pos2] = cropData;
            }
            else
            {
                // Tạo crop data mặc định cho ô trống (có thể khai thác)
                CropData emptyCropData = ScriptableObject.CreateInstance<CropData>();
                emptyCropData.noPlant = true;
                emptyCropData.planted = false;
                emptyCropData.collectible = false;
                emptyCropData.collectibleCorn = false;
                emptyCropData.collectibleParsley = false;
                emptyCropData.collectiblePotato = false;
                emptyCropData.collectibleStrawberry = false;
                emptyCropData.collectibleTomato = false;
                
                ToolsCharacterController.crops[pos2] = emptyCropData;
            }
        }

        // Cập nhật trạng thái động
        TileState state = new TileState
        {
            plowable = true,
            ableToSeed = true,
            waterable = false,
            ableToMow = false
        };
        if (fieldStates.ContainsKey(pos2))
            fieldStates[pos2] = state;
        else
            fieldStates.Add(pos2, state);

        // Cộng tiền thưởng
        int reward = GetRewardByCropName(cropName);
        MoneyController.money += reward;
        
        Debug.Log($"💰 Thu hoạch '{cropName}' nhận được {reward} tiền. Tổng: {MoneyController.money}");
    }

    // === TẠO CROP DATA BAN ĐẦU ===
    private void CreateInitialCropData(Vector3Int pos, string cropName)
    {
        Vector2Int pos2 = new Vector2Int(pos.x, pos.y);
        
        // Sử dụng ScriptableObject.CreateInstance nhưng cần quản lý memory
        CropData cropData = ScriptableObject.CreateInstance<CropData>();
        cropData.noPlant = false;
        cropData.planted = true;
        cropData.collectible = false;
        cropData.collectibleCorn = false;
        cropData.collectibleParsley = false;
        cropData.collectiblePotato = false;
        cropData.collectibleStrawberry = false;
        cropData.collectibleTomato = false;
        
        ToolsCharacterController.crops[pos2] = cropData;
        
        Debug.Log($"🌱 [INIT] Đã tạo crop data ban đầu cho '{cropName}' tại {pos}");
    }

    // === ĐỒNG BỘ COLLECTIBLE FLAGS ===
    private void SyncCollectibleFlags(Vector3Int pos, string cropName, bool isCollectible)
    {
        Vector2Int pos2 = new Vector2Int(pos.x, pos.y);
        
        if (!ToolsCharacterController.crops.ContainsKey(pos2))
        {
            Debug.LogWarning($"⚠️ [SYNC] Không tìm thấy crop data tại {pos} để đồng bộ collectible flags");
            return;
        }
        
        CropData cropData = ToolsCharacterController.crops[pos2];
        cropData.collectible = isCollectible;
        
        // Đặt flag collectible cụ thể cho từng loại cây
        switch (cropName.Trim().ToLower())
        {
            case "corn":
                cropData.collectibleCorn = isCollectible;
                break;
            case "parsley":
                cropData.collectibleParsley = isCollectible;
                break;
            case "potato":
                cropData.collectiblePotato = isCollectible;
                break;
            case "strawberry":
                cropData.collectibleStrawberry = isCollectible;
                break;
            case "tomato":
                cropData.collectibleTomato = isCollectible;
                break;
        }
        
        Debug.Log($"🔄 [SYNC] Đã đồng bộ collectible flags cho '{cropName}' tại {pos}: {isCollectible}");
    }

    // === ĐỒNG BỘ LẠI DỮ LIỆU ===
    public void RefreshTileData(Vector3Int pos)
    {
        Vector2Int pos2 = new Vector2Int(pos.x, pos.y);
        
        // Cập nhật tile data
        if (tileReader != null)
        {
            TileBase currentTile = groundTilemap.GetTile(pos);
            TileData tileData = tileReader.GetTileData(currentTile);
            
            if (tileData != null)
            {
                if (ToolsCharacterController.fields.ContainsKey(pos2))
                    ToolsCharacterController.fields[pos2] = tileData;
                else
                    ToolsCharacterController.fields.Add(pos2, tileData);
            }
        }
        
        // Cập nhật crop data
        if (cropReader != null)
        {
            TileBase cropTile = cropTilemap.GetTile(pos);
            CropData cropData = cropReader.GetCropData(cropTile);
            
            if (cropData != null)
            {
                if (ToolsCharacterController.crops.ContainsKey(pos2))
                    ToolsCharacterController.crops[pos2] = cropData;
                else
                    ToolsCharacterController.crops.Add(pos2, cropData);
            }
            else if (cropTile == null)
            {
                // Tạo crop data mặc định cho ô trống
                CropData emptyCropData = ScriptableObject.CreateInstance<CropData>();
                emptyCropData.noPlant = true;
                emptyCropData.planted = false;
                emptyCropData.collectible = false;
                emptyCropData.collectibleCorn = false;
                emptyCropData.collectibleParsley = false;
                emptyCropData.collectiblePotato = false;
                emptyCropData.collectibleStrawberry = false;
                emptyCropData.collectibleTomato = false;
                
                if (ToolsCharacterController.crops.ContainsKey(pos2))
                    ToolsCharacterController.crops[pos2] = emptyCropData;
                else
                    ToolsCharacterController.crops.Add(pos2, emptyCropData);
            }
        }
        
        Debug.Log($"🔄 [REFRESH] Đã đồng bộ lại dữ liệu tại {pos}");
    }

    // === HELPER ===
    private Crop GetCropByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || cropCatalog == null) return null;
        string n = name.Trim().ToLower();

        foreach (var c in cropCatalog)
        {
            if (c == null) continue;
            if (c.name.Trim().ToLower() == n)
                return c;
        }
        return null;
    }

    private int GetRewardByCropName(string cropName)
    {
        switch (cropName.Trim().ToLower())
        {
            case "parsley": return 5;
            case "carrot": return 10;
            case "potato": return 15;
            case "corn": return 25;
            case "strawberry": return 30;
            case "tomato": return 20;
            default: return 20;
        }
    }

    // === KIỂM TRA CÂY ĐÃ CHÍN ===
    public bool IsCropReadyForHarvest(Vector3Int pos)
    {
        if (!activeCrops.ContainsKey(pos))
        {
            Debug.Log($"❌ [CHECK] Không có cây nào tại {pos}");
            return false;
        }

        var inst = activeCrops[pos];
        bool isReady = inst.isGrown;
        
        Debug.Log($"🔍 [CHECK] Cây '{inst.crop.cropName}' tại {pos}: isGrown={inst.isGrown}, stage={inst.stage}, ready={isReady}");
        
        return isReady;
    }

    // === TEST CHU TRÌNH THU HOẠCH ===
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void TestHarvestCycle(Vector3Int pos)
    {
        Vector2Int pos2 = new Vector2Int(pos.x, pos.y);
        
        Debug.Log($"🧪 [TEST] Bắt đầu test chu trình thu hoạch tại {pos}");
        
        // Kiểm tra trạng thái ban đầu
        bool hasField = ToolsCharacterController.fields.ContainsKey(pos2);
        bool hasCrop = ToolsCharacterController.crops.ContainsKey(pos2);
        
        Debug.Log($"📊 [TEST] Trạng thái ban đầu: hasField={hasField}, hasCrop={hasCrop}");
        
        if (hasField)
        {
            TileData fieldData = ToolsCharacterController.fields[pos2];
            Debug.Log($"🌍 [TEST] Field data: plowable={fieldData.plowable}, ableToSeed={fieldData.ableToSeed}, waterable={fieldData.waterable}, ableToMow={fieldData.ableToMow}");
        }
        
        if (hasCrop)
        {
            CropData cropData = ToolsCharacterController.crops[pos2];
            Debug.Log($"🌱 [TEST] Crop data: noPlant={cropData.noPlant}, planted={cropData.planted}, collectible={cropData.collectible}");
        }
        
        // Kiểm tra tile hiện tại
        TileBase groundTile = groundTilemap.GetTile(pos);
        TileBase cropTile = cropTilemap.GetTile(pos);
        
        Debug.Log($"🎯 [TEST] Tiles: ground={groundTile?.name ?? "null"}, crop={cropTile?.name ?? "null"}");
        
        Debug.Log($"✅ [TEST] Chu trình test hoàn thành tại {pos}");
    }

    // === CLEANUP MEMORY ===
    private void OnDestroy()
    {
        // Cleanup các ScriptableObject được tạo động
        foreach (var cropData in ToolsCharacterController.crops.Values)
        {
            if (cropData != null && cropData.name == "")
            {
                // Chỉ destroy những ScriptableObject được tạo động (không có name)
                DestroyImmediate(cropData);
            }
        }
    }
}
