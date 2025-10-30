using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UI_ShopController : MonoBehaviour
{
    public static UI_ShopController instance;

    private Transform container;
    private Transform shopItemTemplate;
    [SerializeField] private MoneyController money;
    public Button btn;
    [SerializeField] private GameObject toolbarPanel;
    [SerializeField] private GameObject inventoryPanel;
    public bool isOpen;

    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject questPanel;
    [SerializeField] private QuestGiver questGiver;
    [SerializeField] private QuestUI questUI;

    private bool missingShopPanelLogged;
    private bool missingQuestPanelLogged;

    private GameObject LocatePanel(string panelName)
    {
        if (string.IsNullOrEmpty(panelName))
        {
            return null;
        }

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == panelName)
            {
                return child.gameObject;
            }
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.name == panelName && go.scene.IsValid())
            {
                return go;
            }
        }

        return null;
    }

    private void CachePanelReferences()
    {
        if (shopPanel == null)
        {
            shopPanel = LocatePanel("ShopPanel");
        }

        if (questUI == null)
        {
            questUI = FindObjectOfType<QuestUI>(includeInactive: true);
        }

        if (questPanel == null)
        {
            if (questUI != null && questUI.questPanel != null)
            {
                questPanel = questUI.questPanel;
            }
            else
            {
                questPanel = LocatePanel("QuestPanel");
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        container = transform.Find("container");
        shopItemTemplate = container != null ? container.Find("shopItemTemplate") : null;

        CachePanelReferences();
    }

    private void Start()
    {
        Dictionary<string, Sprite> plantsDictionary = CreateSeedsFromSprite();

        CreateItemButton(plantsDictionary["Seeds_Corn"], "Seeds_Corn", 100, 0, "Corn Seeds");
        CreateItemButton(plantsDictionary["Seeds_Parsley"], "Seeds_Parsley", 30, 1, "Parsley Seeds");
        CreateItemButton(plantsDictionary["Seeds_Tomato"], "Seeds_Tomato", 60, 2, "Tomato Seeds");
        CreateItemButton(plantsDictionary["Seeds_Strawberry"], "Seeds_Strawberry", 150, 3, "Strawberry seeds");
        CreateItemButton(plantsDictionary["Seeds_Potato"], "Seeds_Potato", 110, 4, "Potato tuber");

        CachePanelReferences();
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        isOpen = false;
        gameObject.SetActive(false);
    }

    private Dictionary<string, Sprite> CreateSeedsFromSprite()
    {
        Dictionary<string, Sprite> plantsDictionary = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Plants");

        foreach (Sprite sprite in sprites)
        {
            plantsDictionary.Add(sprite.name, sprite);
        }

        return plantsDictionary;
    }

    private void CreateItemButton(Sprite itemSprite, string itemName, int itemCost, int positionIndex, string displayedName)
    {
        if (container == null || shopItemTemplate == null)
        {
            Debug.LogError("UI_ShopController: container or shopItemTemplate is missing.");
            return;
        }

        Transform shopItemTransform = Instantiate(shopItemTemplate, container);
        RectTransform shopItemRectTransform = shopItemTransform.GetComponent<RectTransform>();
        float shopItemHeight = 60f;
        shopItemRectTransform.anchoredPosition = new Vector2(0, 150 + (-shopItemHeight * positionIndex));
        shopItemTransform.Find("nameText").GetComponent<TextMeshProUGUI>().SetText(displayedName);
        shopItemTransform.Find("priceText").GetComponent<TextMeshProUGUI>().SetText(itemCost.ToString());
        shopItemTransform.Find("itemIcon").GetComponent<Image>().sprite = itemSprite;

        Item newItem = ScriptableObject.CreateInstance<Item>();

        foreach (ItemSlot itemSlot in GameManager.instance.allItemsContainer.slots)
        {
            if (itemSlot.item.Name == itemName)
            {
                newItem = itemSlot.item;
            }
        }

        btn = shopItemTransform.GetComponent<Button>();
        btn.onClick.AddListener(delegate { TaskWithParameters(itemCost, newItem); });
    }

    void TaskWithParameters(long itemCost, Item item)
    {
        if (money.canBuyItems(itemCost))
        {
            money.substractMoney(itemCost);
            FindObjectOfType<SoundManager>().Play("Money");

            if (item.Name.Contains("Seeds_Corn"))
            {
                GameManager.instance.inventoryContainer.Add(item, 4);
            }
            else if (item.Name.Contains("Seeds_Tomato"))
            {
                GameManager.instance.inventoryContainer.Add(item, 3);
            }
            else if (item.Name.Contains("Seeds_Strawberry"))
            {
                GameManager.instance.inventoryContainer.Add(item, 6);
            }
            else if (item.Name.Contains("Seeds_Parsley"))
            {
                GameManager.instance.inventoryContainer.Add(item, 3);
            }
            else if (item.Name.Contains("Seeds_Potato"))
            {
                GameManager.instance.inventoryContainer.Add(item, 1);
            }
            else
            {
                GameManager.instance.inventoryContainer.Add(item);
            }
        }

        if (toolbarPanel != null)
        {
            toolbarPanel.SetActive(!toolbarPanel.activeInHierarchy);
            toolbarPanel.SetActive(true);
        }
    }

    public void ShowShopPanel()
    {
        if (!EnsurePanels())
        {
            return;
        }

        shopPanel.SetActive(true);
        questPanel.SetActive(false);
    }

    public void ShowQuestPanel()
    {
        if (!EnsurePanels())
        {
            return;
        }

        shopPanel.SetActive(false);
        questPanel.SetActive(true);
        if (questGiver != null && questUI != null)
        {
            List<Quest> allQuests = questGiver.GetAllQuests();
            questUI.DisplayAvailableQuests(allQuests);
        }
    }

    public void Show()
    {
        isOpen = true;
        gameObject.SetActive(true);
        ShowShopPanel();
    }

    public void Hide()
    {
        isOpen = false;
        if (!EnsurePanels())
        {
            return;
        }

        questPanel.SetActive(false);
        shopPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    private bool EnsurePanels()
    {
        CachePanelReferences();

        bool hasAll = true;

        if (shopPanel == null)
        {
            if (!missingShopPanelLogged)
            {
                Debug.LogWarning("UI_ShopController: shopPanel reference missing; assign it in the inspector.");
                missingShopPanelLogged = true;
            }
            hasAll = false;
        }

        if (questPanel == null)
        {
            if (!missingQuestPanelLogged)
            {
                Debug.LogWarning("UI_ShopController: questPanel reference missing; assign it in the inspector.");
                missingQuestPanelLogged = true;
            }
            hasAll = false;
        }

        return hasAll;
    }

    //dodac obsluge klikniecia i zakup przedmiotu
}
