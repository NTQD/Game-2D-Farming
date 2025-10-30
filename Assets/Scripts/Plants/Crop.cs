using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Data/CropBase")]
public class Crop : ScriptableObject
{
    [Header("Tiles per Growth Stage")]
    public TileBase state0;
    public TileBase state1;
    public TileBase state2;
    public TileBase state3;
    public TileBase state4;
    public TileBase state5;

    [Header("Crop Data")]
    public string cropName;
    public float growthTime = 60f; // tổng thời gian phát triển
    public int totalStages = 6;    // số stage (0–5)
    public GameObject harvestPrefab; // vật phẩm rơi ra khi thu hoạch
}
