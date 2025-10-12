using UnityEngine;

[CreateAssetMenu(menuName = "Data/Animal")]
public class AnimalData : ScriptableObject
{
    [Header("Basic info")]
    public string animalName = "Chicken";
    public Sprite icon;
    [Tooltip("Cost to purchase a single animal.")]
    public long purchaseCost = 100;

    [Header("Care settings")]
    [Tooltip("Item required to feed one animal.")]
    public Item requiredFeedItem;
    [Tooltip("Amount of feed consumed per animal when feeding.")]
    public int feedPerAnimal = 1;

    [Header("Production")]
    [Tooltip("Item produced after the animal has been fed and the timer finishes.")]
    public Item produceItem;
    [Tooltip("Number of items produced each time an animal finishes a production cycle.")]
    public int produceAmount = 1;
    [Tooltip("Time in seconds that must pass after feeding before produce becomes available.")]
    public float timeBetweenProduce = 60f;
    [Tooltip("Optional sound effect played when produce is generated.")]
    public AudioClip produceReadySfx;
}
