using UnityEngine;

[CreateAssetMenu(menuName = "Data/Crops/Strawberry")]
public class Strawberry : Crop
{
    private void OnEnable()
    {
        cropName = "strawberry";
        growthTime = 20f;
        totalStages = 6;
    }
}
