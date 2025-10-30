using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Basic class for the inventory and toolbar panel
public class ItemPanel : MonoBehaviour
{
    public ItemContainer inventory;
    public List<InventoryButton> buttons;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        SetIndex();    // Cicles through inventory and sets indexes to the buttons
        Show();
    }

    private void SetIndex()
    {
        int count = (buttons != null) ? buttons.Count : 0;
        for (int i = 0; i < count; i++)
        {
            buttons[i].SetIndex(i);
        }
    }

    private void OnEnable()
    {
        Show();
    }

    public void Show()
    {
        // Always clean all buttons first to avoid stale placeholder text (e.g., "999")
        for (int b = 0; b < (buttons?.Count ?? 0); b++)
        {
            buttons[b].Clean();
        }

        if (inventory == null || inventory.slots == null || buttons == null)
        {
            return;
        }

        int max = Mathf.Min(inventory.slots.Count, buttons.Count);
        for (int i = 0; i < max; i++)
        {
            ItemSlot slot = inventory.slots[i];
            if (slot != null && slot.item != null)
            {
                // Setting the button to the item in the inventory
                buttons[i].Set(slot);
            }
            // else keep cleaned state
        }
    }

    public virtual void OnClick(int id)
    {

    }
}
