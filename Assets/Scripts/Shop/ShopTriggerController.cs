using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTriggerController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (UI_ShopController.instance != null)
        {
            UI_ShopController.instance.Show();
            FindObjectOfType<SoundManager>().Play("Parrot");
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (UI_ShopController.instance != null)
        {
            UI_ShopController.instance.Hide();
        }
    }
}
