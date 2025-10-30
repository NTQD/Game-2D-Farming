using UnityEngine;

public class BuildController : MonoBehaviour
{
    [Header("Building Settings")]
    public Camera cam;
    public LayerMask groundLayer;
    public Item_Building selectedBuilding;
    public GameObject previewPrefab; // prefab mờ hiển thị trước khi xây

    [Header("References")]
    [SerializeField] private InventoryPanel inventoryPanel;

    private bool isBuildingMode = false;
    private GameObject previewInstance;

    void Awake()
    {
        // Auto-wire common references to work immediately in play mode
        if (cam == null && Camera.main != null)
            cam = Camera.main;

        if (inventoryPanel == null)
            inventoryPanel = FindObjectOfType<InventoryPanel>();

        // Ensure groundLayer has at least Default if unset
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Default");
    }

    void Update()
    {
        // 1️⃣ Bấm B để bật / tắt chế độ xây
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
        }

        // 2️⃣ Nếu chưa bật chế độ xây, dừng lại
        if (!isBuildingMode || selectedBuilding == null) return;

        // 3️⃣ Cập nhật vị trí preview theo chuột
        UpdatePreviewPosition();

        // 4️⃣ Click chuột trái để xây thật
        if (Input.GetMouseButtonDown(0))
        {
            TryBuild();
        }
    }

    void ToggleBuildMode()
    {
        isBuildingMode = !isBuildingMode;

        if (isBuildingMode)
        {
            if (previewInstance == null && previewPrefab != null)
            {
                previewInstance = Instantiate(previewPrefab);
            }
            Debug.Log("🧱 Bật chế độ xây dựng");
        }
        else
        {
            if (previewInstance != null) Destroy(previewInstance);
            Debug.Log("🚫 Thoát chế độ xây dựng");
        }
    }

    void UpdatePreviewPosition()
{
    if (!isBuildingMode) return;           // chưa bật chế độ xây
    if (previewInstance == null) return;   // chưa có preview thì dừng luôn

    Camera useCam = cam != null ? cam : Camera.main;
    if (useCam == null) return;

    Vector3 mousePos = useCam.ScreenToWorldPoint(Input.mousePosition);
    mousePos.z = 0;

    Vector3 snapped = new Vector3(
        Mathf.Round(mousePos.x * 2f) / 2f,
        Mathf.Round(mousePos.y * 2f) / 2f,
        0
    );

    previewInstance.transform.position = snapped;
}


    void TryBuild()
    {
        if (previewInstance == null)
        {
            Debug.LogError("❌ Preview instance is null!");
            return;
        }

        Vector3 buildPos = previewInstance.transform.position;
        
        // Kiểm tra có thể đặt building không
        if (selectedBuilding == null)
        {
            Debug.LogError("❌ selectedBuilding is null!");
            return;
        }

        if (!selectedBuilding.CanPlace(buildPos))
        {
            Debug.Log("❌ Không thể đặt building tại vị trí này!");
            return;
        }

        int woodCount = GetItemCount("Wood");

        if (woodCount >= selectedBuilding.woodCost)
        {
            selectedBuilding.Place(buildPos, woodCount);
            RemoveItem("Wood", selectedBuilding.woodCost);
            Debug.Log("✅ Đã xây nhà!");
        }
        else
        {
            Debug.Log("❌ Không đủ gỗ!");
        }
    }

    int GetItemCount(string itemName)
    {
        if (GameManager.instance?.inventoryContainer?.slots == null)
        {
            Debug.LogError("❌ inventoryContainer is null!");
            return 0;
        }

        int count = 0;
        foreach (var slot in GameManager.instance.inventoryContainer.slots)
        {
            if (slot?.item != null && slot.item.Name == itemName)
                count += slot.count;
        }
        return count;
    }

    void RemoveItem(string itemName, int amount)
    {
        if (GameManager.instance == null || GameManager.instance.allItemsContainer == null)
        {
            Debug.LogError("❌ GameManager.instance hoặc allItemsContainer is null!");
            return;
        }
        
        // Tìm Item theo tên từ allItemsContainer
        Item itemRef = null;
        foreach (var slot in GameManager.instance.allItemsContainer.slots)
        {
            if (slot?.item != null && slot.item.Name == itemName)
            {
                itemRef = slot.item;
                break;
            }
        }

        if (itemRef == null)
        {
            Debug.LogError($"❌ Không tìm thấy Item '{itemName}' trong allItemsContainer");
            return;
        }

        if (GameManager.instance.inventoryContainer != null)
        {
            GameManager.instance.inventoryContainer.RemoveItem(itemRef, amount);
        }
        else
        {
            Debug.LogError("❌ inventoryContainer is null!");
            return;
        }

        // Refresh UI panels nếu có
        if (inventoryPanel != null)
            inventoryPanel.Show();
        var toolbarPanel = FindObjectOfType<ItemToolbarPanel>();
        if (toolbarPanel != null)
            toolbarPanel.Show();
    }
}
