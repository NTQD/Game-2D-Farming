using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestHit : ToolHit
{
    [SerializeField] GameObject pickUpAxe;
    [SerializeField] GameObject pickUpWateringCan;
    [SerializeField] GameObject pickUpHoe;
    [SerializeField] GameObject pickUpShovel;
    [SerializeField] GameObject pickUpBag;
    [SerializeField] GameObject pickUpPotato;
    [SerializeField] GameObject pickUpChickenFeed;

    [SerializeField] int dropCount = 22;
    [SerializeField] float spread = 0.9f;

    List<GameObject> items;

    private void Start()
    {
        items = new List<GameObject>();
        items.Add(pickUpAxe);
        items.Add(pickUpWateringCan);
        items.Add(pickUpHoe);
        items.Add(pickUpShovel);
        items.Add(pickUpBag);
        items.Add(pickUpPotato);
        items.Add(pickUpPotato);
        items.Add(pickUpPotato);
        items.Add(pickUpPotato);
        items.Add(pickUpPotato);
        items.Add(pickUpPotato);
        items.Add(pickUpPotato);

        for(int i = 0; i < 10; i++)
        {
            items.Add(pickUpChickenFeed);
        }
    }

    public override void Hit()
    {
        // Spawning objects
        for (int i = 0; i < dropCount; i++)
        {
            // Check to prevent out-of-bounds errors if dropCount is larger than the list
            if (i >= items.Count) { break; }

            // Instantiate the item
            GameObject newObject = Instantiate(items[i]);

            // Calculate a random position near the chest
            Vector3 position = transform.position;
            position.x += spread * UnityEngine.Random.value - spread / 2;
            position.y += spread * UnityEngine.Random.value - spread / 2;
            newObject.transform.position = position;
        }

        Destroy(gameObject);
    }
}
