using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    Transform player;
    [SerializeField] float speed = 5f;  // Speed of moving the object towards the player
    [SerializeField] float pickUpDistance = 1.5f;     // Distance of picking up items
    [SerializeField] Item item;
    public int count = 1;
    GameObject toolbar;

    private void Start()
    {
        // Try to get player from GameManager, otherwise fall back to finding by tag
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            player = GameManager.instance.player.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.LogWarning("PickUpItem: Player reference was not available on GameManager. Falling back to GameObject.FindWithTag('Player').");
            }
            else
            {
                Debug.LogError("PickUpItem: Player reference not found. Make sure GameManager.instance.player is set or the player GameObject has the 'Player' tag.");
            }
        }

        toolbar = GameObject.FindWithTag("toolbar");
        if (toolbar == null)
        {
            Debug.LogWarning("PickUpItem: toolbar GameObject with tag 'toolbar' not found.");
        }
    }

    private void Update()
    {
        if (player == null)
        {
            // If player is not assigned we cannot proceed
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // If the player is not in the distance to pick up logs no function is executed
        if (distance > pickUpDistance)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        if (distance < 0.1f)
        {
            if (GameManager.instance != null && GameManager.instance.inventoryContainer != null)
            {
                GameManager.instance.inventoryContainer.Add(item, count);

                // Notify QuestManager about the gathered item
                if (QuestManager.instance != null)
                {
                    QuestManager.instance.CheckGatherObjective(item, count);
                }

                if (toolbar != null)
                {
                    toolbar.SetActive(!toolbar.activeInHierarchy);
                    toolbar.SetActive(true);
                }
            }
            else
            {
                Debug.LogWarning("No inventory container attached to game manager");
            }

            Destroy(gameObject);
            
        }
    }
}
