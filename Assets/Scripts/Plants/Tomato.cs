using UnityEngine;

[CreateAssetMenu(menuName = "Data/Crops/Tomato")]
public class Tomato : Crop
{
    private void OnEnable()
    {
        cropName = "tomato";
        growthTime = 30f;
        totalStages = 6;
    }
}
