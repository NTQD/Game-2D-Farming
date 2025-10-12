using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    [SerializeField] private MoneyController moneyController;
    [SerializeField] private ItemContainer inventory;

    private readonly List<AnimalPen> _pens = new List<AnimalPen>();

    public IReadOnlyList<AnimalPen> Pens => _pens;
    public ItemContainer Inventory => inventory;

    private void Awake()
    {
        if (inventory == null && GameManager.instance != null)
        {
            inventory = GameManager.instance.inventoryContainer;
        }

        if (moneyController == null)
        {
            moneyController = FindObjectOfType<MoneyController>();
        }
    }

    public void RegisterPen(AnimalPen pen)
    {
        if (pen == null)
        {
            return;
        }

        if (!_pens.Contains(pen))
        {
            _pens.Add(pen);
            pen.SetManager(this);
        }
    }

    public bool TryPurchaseAnimal(AnimalPen pen)
    {
        if (pen == null || pen.Data == null)
        {
            return false;
        }

        if (pen.CurrentAnimalCount >= pen.MaxAnimals)
        {
            return false;
        }

        long cost = Mathf.Max(0, pen.Data.purchaseCost);
        if (cost > 0)
        {
            if (moneyController == null || !moneyController.canBuyItems(cost))
            {
                return false;
            }

            moneyController.substractMoney(cost);
        }

        pen.AddAnimalFromManager();
        return true;
    }

    public bool TryCollectProduce(AnimalPen pen)
    {
        if (pen == null)
        {
            return false;
        }

        if (inventory == null)
        {
            return false;
        }

        Item produceItem = pen.GetProduceItem();
        if (produceItem == null)
        {
            return false;
        }

        int produce = pen.TakeStoredProduce();
        if (produce <= 0)
        {
            return false;
        }

        inventory.Add(produceItem, produce);
        return true;
    }
}
