using UnityEngine;

[CreateAssetMenu(menuName = "Data/Corn")]
public class Corn : Crop
{
    private void OnEnable()
    {
        cropName = "corn";
        growthTime = 20f;
        totalStages = 6;
    }
}
