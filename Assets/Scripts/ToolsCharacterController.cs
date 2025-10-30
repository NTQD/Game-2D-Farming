    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Tilemaps;
    using UnityEngine.EventSystems;

    public class ToolsCharacterController : MonoBehaviour
    {
        PlayerControl character;
        Rigidbody2D rgbd2d;
        [SerializeField] MarkerManager markerManager;
        [SerializeField] TileMapReadController tileMapReadController;
        [SerializeField] CropsReadController cropsReadController;
        [SerializeField] float maxDistance = 2f;
        [SerializeField] CropsManager cropsManager;
        [SerializeField] TileData plowableTiles;
        [SerializeField] TileData toMowTiles;
        [SerializeField] TileData toSeedTiles;
        [SerializeField] TileData waterableTiles;
        InventoryController inventoryController;
        ToolbarController toolbarController;
        [SerializeField] GameObject toolbarPanel;

        [SerializeField] float offsetDistance = 1f;
        [SerializeField] float sizeOfInteractableArea = 1.2f;

        private static int cornPickUpCount = 3;
        private static int parsleyPickUpCount = 1;
        private static int potatoPickUpCount = 1;
        private static int strawberryPickUpCount = 1;
        private static int tomatoPickUpCount = 1;

        private static int cornSeedsCount = 4;
        private static int parsleySeedsCount = 3;
        private static int potatoSeedsCount = 1;
        private static int strawberrySeedsCount = 6;
        private static int tomatoSeedsCount = 3;

        Vector3Int selectedTilePosition;
        Vector3Int selectedCropPosition;
        bool selectable;

        public static Dictionary<Vector2Int, TileData> fields;
        public static Dictionary<Vector2Int, CropData> crops;

        UI_ShopController shopPanel;


        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<PlayerControl>();
            rgbd2d = GetComponent<Rigidbody2D>();
            fields = new Dictionary<Vector2Int, TileData>();
            crops = new Dictionary<Vector2Int, CropData>();
            toolbarController = GetComponent<ToolbarController>();
            inventoryController = GetComponent<InventoryController>();

            var shopPanelAll = Resources.FindObjectsOfTypeAll<UI_ShopController>();
            shopPanel = shopPanelAll[0];
        }

        // Update is called once per frame
        void Update()
        {
            SelectTile();
            CanSelectCheck();
            Marker();
            if (Input.GetMouseButtonDown(0)) // lewy przycisk myszki
            {
                if (!inventoryController.isOpen) //you can use tools only if inventory is closed
                {
                    if (UseToolWorld() == true)
                    {
                        return;
                    }
                    UseTool();
                }

            }
        }

        private bool CastRay()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            if (hit)
            {
                //Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.name.Contains("Tree"))
                {
                    return true;
                }
                if (hit.collider.gameObject.name.Contains("CampFire"))
                {
                    return true;
                }
                if (hit.collider.gameObject.name.Contains("Chest"))
                {
                    return true;
                }
            }
            return false;
        }
        private bool CastRayPlayer()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            if (hit)
            {
                if (hit.collider.gameObject.name.Contains("Player"))
                {
                    return true;
                }
            }
            return false;
        }
                private void SelectTile()
        {
            selectedTilePosition = tileMapReadController.GetGridPosition(Input.mousePosition, true);
            TileBase tileBase = tileMapReadController.GetTileBase(selectedTilePosition);
            try
            {
                TileData tileData = tileMapReadController.GetTileData(tileBase);
                if (!(tileData is null))
                {
                    if (!fields.ContainsKey((Vector2Int)selectedTilePosition))
                    {
                        fields.Add((Vector2Int)selectedTilePosition, tileData);
                    }
                    else
                    {
                        fields[(Vector2Int)selectedTilePosition] = tileData;
                    }
                }
            }
            catch
            {
                return;
            }

            selectedCropPosition = cropsReadController.GetGridPosition(Input.mousePosition, true);
            // Khôi phục cách đọc ban đầu: dùng selectedTilePosition
            TileBase cropBase = cropsReadController.GetTileBase(selectedTilePosition);
            try
            {
                CropData cropData = cropsReadController.GetCropData(cropBase);
                if (!(cropData is null))
                {
                    if (!crops.ContainsKey((Vector2Int)selectedTilePosition))
                    {
                        crops.Add((Vector2Int)selectedTilePosition, cropData);
                    }
                    else
                    {
                        crops[(Vector2Int)selectedTilePosition] = cropData;
                    }
                }
                else if (cropBase == null)
                {
                    // Nếu không có crop tile, tạo crop data mặc định cho ô trống
                    CropData emptyCropData = ScriptableObject.CreateInstance<CropData>();
                    emptyCropData.noPlant = true;
                    emptyCropData.planted = false;
                    emptyCropData.collectible = false;
                    emptyCropData.collectibleCorn = false;
                    emptyCropData.collectibleParsley = false;
                    emptyCropData.collectiblePotato = false;
                    emptyCropData.collectibleStrawberry = false;
                    emptyCropData.collectibleTomato = false;
                    
                    if (!crops.ContainsKey((Vector2Int)selectedTilePosition))
                    {
                        crops.Add((Vector2Int)selectedTilePosition, emptyCropData);
                    }
                    else
                    {
                        crops[(Vector2Int)selectedTilePosition] = emptyCropData;
                    }
                }
            }
            catch
            {
                return;
            }

        }

        void CanSelectCheck()
        {
            if (Time.timeScale == 0) //if game paused
                return;

            Vector2 characterPosition = transform.position;
            Vector2 cameraPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectable = Vector2.Distance(characterPosition, cameraPosition) < maxDistance;
            markerManager.Show(selectable);
        }

        private void Marker()
        {
            markerManager.markedCellPosition = selectedTilePosition;
        }

        // interacting with physical objects in the world
        private bool UseToolWorld()
        {
            if (Time.timeScale == 0)
                return false;

            // CUTTING TREE
            Vector2 position = rgbd2d.position + character.lastMotionVector * offsetDistance;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, sizeOfInteractableArea);


            foreach (Collider2D collidor in colliders)
            {
                ToolHit hitTree = collidor.GetComponent<ToolHit>();
                CampFireHit hitFire = collidor.GetComponent<CampFireHit>();
                ChestHit hitChest = collidor.GetComponent<ChestHit>();
                PlayerHit hitPlayer = collidor.GetComponent<PlayerHit>();

                if (hitTree != null && toolbarController.GetItem != null &&
                    toolbarController.GetItem.Name == "Axe" && CastRay() == true)
                {
                    hitTree.Hit();
                    // Debug.Log("we can hit");
                    return true;
                }
                if (hitFire != null && toolbarController.GetItem != null &&
                    toolbarController.GetItem.Name == "Wood" && CastRay() == true)
                {
                    hitFire.Hit();
                    return true;
                }
                if (hitChest != null && CastRay() == true)
                {
                    hitChest.Hit();
                    return true;
                }
                if (hitPlayer != null && toolbarController.GetItem != null && CastRayPlayer() == true && (toolbarController.GetItem.Name == "Food_Corn" || toolbarController.GetItem.Name == "Food_Parsley"
                        || toolbarController.GetItem.Name == "Food_Potato" || toolbarController.GetItem.Name == "Food_Strawberry" || toolbarController.GetItem.Name == "Food_Tomato"))
                {
                    hitPlayer.Hit();
                    return true;
                }
            }

            return false;
        }

        private void RefreshToolbar()
        {
            toolbarPanel.SetActive(!toolbarPanel.activeInHierarchy);
            toolbarPanel.SetActive(true);
        }

        private void UseTool()
        {
            if (Time.timeScale == 0) //if game paused - return
                return;

            // when sth is present on the grid but you can't plant there
            if (selectable == true && toolbarController.GetItem != null)
            {
                TileBase tileBase = tileMapReadController.GetTileBase(selectedTilePosition);
                TileData tileData = tileMapReadController.GetTileData(tileBase);
                //TileData cropData = cropsReadController.GetTileData(tileBase);

                /*if (toolbarController.GetItem.Name == "WateringCan" && fields[(Vector2Int)selectedTilePosition].watered) //if you are using watering can - play sound of water
                    FindObjectOfType<SoundManager>().Play("Water");*/

                if (tileData != plowableTiles && tileData != toMowTiles && tileData != toSeedTiles && tileData != waterableTiles) //if tile doesn't have any ability
                {
                    return;
                }

                // Debug.Log("Wybrane narzędzie: " + toolbarController.GetItem.Name);
                //Debug.Log(crops[(Vector2Int)selectedTilePosition]);
                //if there is no plant on tile
                // Nếu ô chưa có cây hoặc đã thu hoạch
    var key = (Vector2Int)selectedTilePosition;
    bool hasNoPlant = crops.ContainsKey(key) && crops[key].noPlant;
    bool hasNoCropData = !crops.ContainsKey(key);
    
    if (hasNoPlant || hasNoCropData)
    {
        // Dùng xẻng để đào cỏ -> dirt
        if (fields.ContainsKey(key) && fields[key].ableToMow && 
            toolbarController.GetItem.Name == "Shovel" &&
            shopPanel.isOpen == false)
        {
            cropsManager.Dig(selectedTilePosition);
            Debug.Log($"🪓 Đào đất tại {key}");
        }

        // Dùng cuốc để cày dirt -> plowed
        else if (fields.ContainsKey(key) && fields[key].plowable &&
            toolbarController.GetItem.Name == "Hoe")
        {
            Debug.Log($"[DEBUG] Plow check at {key}: plowable={fields[key].plowable}");
            cropsManager.Plow(selectedTilePosition);
            Debug.Log($"⛏️ Cày đất tại {key}");
        }

        // Gieo hạt nếu đất là plowed
        else if (fields.ContainsKey(key) && fields[key].ableToSeed && toolbarController.GetItem.isSeed == true)
        {
            Debug.Log($"[DEBUG] Seed check at {key}: ableToSeed={fields[key].ableToSeed}, item={toolbarController.GetItem.Name}");
            switch (toolbarController.GetItem.Name)
            {
                case "Seeds_Corn":
                    if (GameManager.instance.inventoryContainer.slots[toolbarController.selectedTool].count >= cornSeedsCount)
                    {
                        cropsManager.SeedCrop(selectedTilePosition, "corn");
                        GameManager.instance.inventoryContainer.RemoveItem(toolbarController.GetItem, cornSeedsCount);
                        Debug.Log($"🌽 Gieo hạt ngô tại {key}");
                    }
                    break;

                case "Seeds_Parsley":
                    if (GameManager.instance.inventoryContainer.slots[toolbarController.selectedTool].count >= parsleySeedsCount)
                    {
                        cropsManager.SeedCrop(selectedTilePosition, "parsley");
                        GameManager.instance.inventoryContainer.RemoveItem(toolbarController.GetItem, parsleySeedsCount);
                        Debug.Log($"🌿 Gieo hạt parsley tại {key}");
                    }
                    break;

                case "Seeds_Potato":
                    if (GameManager.instance.inventoryContainer.slots[toolbarController.selectedTool].count >= potatoSeedsCount)
                    {
                        cropsManager.SeedCrop(selectedTilePosition, "potato");
                        GameManager.instance.inventoryContainer.RemoveItem(toolbarController.GetItem, potatoSeedsCount);
                        Debug.Log($"🥔 Gieo hạt khoai tây tại {key}");
                    }
                    break;

                case "Seeds_Strawberry":
                    if (GameManager.instance.inventoryContainer.slots[toolbarController.selectedTool].count >= strawberrySeedsCount)
                    {
                        cropsManager.SeedCrop(selectedTilePosition, "strawberry");
                        GameManager.instance.inventoryContainer.RemoveItem(toolbarController.GetItem, strawberrySeedsCount);
                        Debug.Log($"🍓 Gieo hạt dâu tây tại {key}");
                    }
                    break;

                case "Seeds_Tomato":
                    if (GameManager.instance.inventoryContainer.slots[toolbarController.selectedTool].count >= tomatoSeedsCount)
                    {
                        cropsManager.SeedCrop(selectedTilePosition, "tomato");
                        GameManager.instance.inventoryContainer.RemoveItem(toolbarController.GetItem, tomatoSeedsCount);
                        Debug.Log($"🍅 Gieo hạt cà chua tại {key}");
                    }
                    break;
            }

            RefreshToolbar();
        }
    }


                //usage of tools if there is a planted tile
                else if (crops.ContainsKey(key) && fields.ContainsKey(key) && crops[key].planted && fields[key].waterable && toolbarController.GetItem.Name == "WateringCan")
                {
                    cropsManager.Water(selectedTilePosition);
                    FindObjectOfType<SoundManager>().Play("Water");
                }

                else if (crops.ContainsKey(key) && crops[key].collectibleCorn && toolbarController.GetItem.Name == "Bag")
                {
                    // Kiểm tra thêm với CropsManager để đảm bảo cây thực sự đã chín
                    if (cropsManager.IsCropReadyForHarvest(selectedTilePosition))
                    {
                        cropsManager.Collect(selectedTilePosition, "corn");
                        foreach (ItemSlot itemSlot in GameManager.instance.allItemsContainer.slots)
                        {
                            if (itemSlot.item.Name == "Food_Corn")
                            {
                                GameManager.instance.inventoryContainer.Add(itemSlot.item, cornPickUpCount);
                                RefreshToolbar();
                                break;
                            }
                        }
                        Debug.Log("🌽 Thu hoạch ngô thành công");
                    }
                    else
                    {
                        Debug.Log("🌱 Ngô chưa chín để thu hoạch");
                    }
                }
                else if (crops.ContainsKey(key) && crops[key].collectibleParsley && toolbarController.GetItem.Name == "Bag")
                {
                    if (cropsManager.IsCropReadyForHarvest(selectedTilePosition))
                    {
                        cropsManager.Collect(selectedTilePosition, "parsley");
                        foreach (ItemSlot itemSlot in GameManager.instance.allItemsContainer.slots)
                        {
                            if (itemSlot.item.Name == "Food_Parsley")
                            {
                                GameManager.instance.inventoryContainer.Add(itemSlot.item, parsleyPickUpCount);
                                RefreshToolbar();
                                break;
                            }
                        }
                        Debug.Log("🌿 Thu hoạch parsley thành công");
                    }
                    else
                    {
                        Debug.Log("🌱 Parsley chưa chín để thu hoạch");
                    }
                }
                else if (crops.ContainsKey(key) && crops[key].collectiblePotato && toolbarController.GetItem.Name == "Bag")
                {
                    if (cropsManager.IsCropReadyForHarvest(selectedTilePosition))
                    {
                        cropsManager.Collect(selectedTilePosition, "potato");
                        foreach (ItemSlot itemSlot in GameManager.instance.allItemsContainer.slots)
                        {
                            if (itemSlot.item.Name == "Food_Potato")
                            {
                                GameManager.instance.inventoryContainer.Add(itemSlot.item, potatoPickUpCount);
                                RefreshToolbar();
                                break;
                            }
                        }
                        Debug.Log("🥔 Thu hoạch khoai tây thành công");
                    }
                    else
                    {
                        Debug.Log("🌱 Khoai tây chưa chín để thu hoạch");
                    }
                }
                else if (crops.ContainsKey(key) && crops[key].collectibleStrawberry && toolbarController.GetItem.Name == "Bag")
                {
                    if (cropsManager.IsCropReadyForHarvest(selectedTilePosition))
                    {
                        cropsManager.Collect(selectedTilePosition, "strawberry");
                        foreach (ItemSlot itemSlot in GameManager.instance.allItemsContainer.slots)
                        {
                            if (itemSlot.item.Name == "Food_Strawberry")
                            {
                                GameManager.instance.inventoryContainer.Add(itemSlot.item, strawberryPickUpCount);
                                RefreshToolbar();
                                break;
                            }
                        }
                        Debug.Log("🍓 Thu hoạch dâu tây thành công");
                    }
                    else
                    {
                        Debug.Log("🌱 Dâu tây chưa chín để thu hoạch");
                    }
                }
                else if (crops.ContainsKey(key) && crops[key].collectibleTomato && toolbarController.GetItem.Name == "Bag")
                {
                    if (cropsManager.IsCropReadyForHarvest(selectedTilePosition))
                    {
                        cropsManager.Collect(selectedTilePosition, "tomato");
                        foreach (ItemSlot itemSlot in GameManager.instance.allItemsContainer.slots)
                        {
                            if (itemSlot.item.Name == "Food_Tomato")
                            {
                                GameManager.instance.inventoryContainer.Add(itemSlot.item, tomatoPickUpCount);
                                RefreshToolbar();
                                break;
                            }
                        }
                        Debug.Log("🍅 Thu hoạch cà chua thành công");
                    }
                    else
                    {
                        Debug.Log("🌱 Cà chua chưa chín để thu hoạch");
                    }
                }

            }
        }
    }
