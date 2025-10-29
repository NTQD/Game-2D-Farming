using UnityEngine;

[CreateAssetMenu(menuName = "Data/Boat Item")]
public class BoatItem : Item
{
    public GameObject boatPrefab; // Assign your Boat_Prefab here in the Inspector

    public override void Use()
    {
        base.Use(); // Call the base Use method

        if (boatPrefab == null)
        {
            Debug.LogError("BoatItem: Boat Prefab is not assigned!");
            return;
        }

        if (GameManager.instance == null || GameManager.instance.boatSpawnPoint == null)
        {
            Debug.LogError("BoatItem: GameManager instance or boatSpawnPoint is null. Cannot spawn boat.");
            return;
        }

        // Instantiate the boat at the designated spawn point
        GameObject spawnedBoat = Instantiate(boatPrefab, GameManager.instance.boatSpawnPoint.position, Quaternion.identity);
        BoatController boatController = spawnedBoat.GetComponent<BoatController>();
        if (boatController != null)
        {
            boatController.AppearOnShore(GameManager.instance.boatSpawnPoint.position); // Use AppearOnShore for initial setup
        }
        else
        {
            Debug.LogWarning("BoatItem: Spawned boat does not have a BoatController.");
        }

        Debug.Log("Boat spawned at designated location.");

        // Remove the item from inventory after use
        if (GameManager.instance != null && GameManager.instance.inventoryContainer != null)
        {
            GameManager.instance.inventoryContainer.Remove(this, 1); // Remove 1 instance of this BoatItem
            Debug.Log($"Removed {Name} from inventory.");
        }
    }
}
