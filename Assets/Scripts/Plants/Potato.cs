using UnityEngine;

[CreateAssetMenu(menuName = "Data/Crops/Potato")]
public class Potato : Crop
{
    private void OnEnable()
    {
        cropName = "potato";
        growthTime = 15f;
        totalStages = 6;
    }
}
