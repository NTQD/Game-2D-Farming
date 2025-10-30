using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject player;
    public ItemContainer inventoryContainer;
    public ItemContainer allItemsContainer;
    public SeedContainer allSeedsContainer;
    public DragAndDropController dragAndDropController;

    public ToolbarController toolbarControllerGlobal;

    public Transform boatSpawnPoint; // New: Transform for where the boat should spawn

    [Header("End Game UI")]
    public GameObject congratulationsScreen;

    public void ShowCongratulationsScreen()
    {
        if (congratulationsScreen != null)
        {
            congratulationsScreen.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameManager: Congratulations screen reference is missing.");
        }
    }
}
