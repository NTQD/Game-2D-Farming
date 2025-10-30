using UnityEngine;

[CreateAssetMenu(menuName = "Data/Crops/Parsley")]
public class Parsley : Crop
{
    private void OnEnable()
    {
        cropName = "parsley";
        growthTime = 10f;
        totalStages = 5;
    }
}
