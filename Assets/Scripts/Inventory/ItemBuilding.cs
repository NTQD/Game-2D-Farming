using UnityEngine;

[CreateAssetMenu(menuName = "Items/Building Item")]
public class Item_Building : Item
{
    public GameObject buildingPrefab;
    public int woodCost = 10; // Cần 1 gỗ để xây

    public bool CanPlace(Vector3 position)
    {
        Collider2D hit = Physics2D.OverlapBox(position, Vector2.one * 0.9f, 0);
        return hit == null;
    }

    public void Place(Vector3 position, int woodCount)
{
    if (buildingPrefab == null)
    {
        Debug.LogError("❌ Building Prefab is NULL!");
        return;
    }

    position.z = 0; // ép về mặt phẳng camera
    GameObject newHouse = Instantiate(buildingPrefab, position, Quaternion.identity);
    Debug.Log("✅ Spawned: " + newHouse.name + " at " + position);
}

}
