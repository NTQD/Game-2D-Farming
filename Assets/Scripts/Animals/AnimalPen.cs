using System.Collections.Generic;
using UnityEngine;

public class AnimalPen : MonoBehaviour
{
    private class AnimalState
    {
        public float produceTimer;
        public bool isFed;
    }

    [SerializeField] private AnimalData animalData;
    [SerializeField] private int startingAnimals;
    [SerializeField] private int maxAnimals = 6;
    [SerializeField] private bool autoCollectProduce = true;
    [SerializeField] private AudioSource audioSource;

    private readonly List<AnimalState> _animals = new List<AnimalState>();
    private AnimalManager _manager;
    private int _storedProduce;

    public AnimalData Data => animalData;
    public int CurrentAnimalCount => _animals.Count;
    public int MaxAnimals => maxAnimals;
    public bool AutoCollect => autoCollectProduce;
    public int StoredProduce => _storedProduce;

    private void Awake()
    {
        if (animalData != null && animalData.feedPerAnimal < 1)
        {
            animalData.feedPerAnimal = 1;
        }
    }

    private void OnEnable()
    {
        if (_manager == null)
        {
            _manager = GetComponentInParent<AnimalManager>();
            if (_manager == null)
            {
                _manager = FindObjectOfType<AnimalManager>();
            }
        }

        if (_manager != null)
        {
            _manager.RegisterPen(this);
        }

        if (_animals.Count == 0 && startingAnimals > 0)
        {
            for (int i = 0; i < Mathf.Min(startingAnimals, maxAnimals); i++)
            {
                AddAnimalInternal();
            }
        }
    }

    private void Update()
    {
        if (animalData == null || _animals.Count == 0)
        {
            return;
        }

        float delta = Time.deltaTime;
        bool producedThisFrame = false;

        foreach (AnimalState state in _animals)
        {
            if (!state.isFed)
            {
                continue;
            }

            state.produceTimer -= delta;
            if (state.produceTimer <= 0f)
            {
                state.isFed = false;
                state.produceTimer = 0f;
                _storedProduce += Mathf.Max(1, animalData.produceAmount);
                producedThisFrame = true;
            }
        }

        if (producedThisFrame)
        {
            if (autoCollectProduce)
            {
                TryDeliverProduce();
            }

            if (animalData.produceReadySfx != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(animalData.produceReadySfx);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(animalData.produceReadySfx, transform.position);
                }
            }
        }
    }

    public bool TryBuyAnimal()
    {
        if (_manager == null || animalData == null)
        {
            return false;
        }

        return _manager.TryPurchaseAnimal(this);
    }

    public bool TryFeedPen()
    {
        if (_manager == null || animalData == null || animalData.requiredFeedItem == null)
        {
            return false;
        }

        ItemContainer inventory = _manager.Inventory;
        if (inventory == null)
        {
            return false;
        }

        int animalsToFeed = _animals.Count;
        if (animalsToFeed == 0)
        {
            return false;
        }

        int feedPerAnimal = Mathf.Max(1, animalData.feedPerAnimal);
        int availableFeed = inventory.GetCount(animalData.requiredFeedItem);
        if (availableFeed < feedPerAnimal)
        {
            return false;
        }

        int maximumFeedableAnimals = Mathf.Min(animalsToFeed, availableFeed / feedPerAnimal);
        if (maximumFeedableAnimals <= 0)
        {
            return false;
        }

        inventory.RemoveItem(animalData.requiredFeedItem, maximumFeedableAnimals * feedPerAnimal);

        int fedAnimals = 0;
        for (int i = 0; i < _animals.Count && fedAnimals < maximumFeedableAnimals; i++)
        {
            AnimalState state = _animals[i];
            state.isFed = true;
            state.produceTimer = Mathf.Max(0.1f, animalData.timeBetweenProduce);
            fedAnimals++;
        }

        return fedAnimals > 0;
    }

    public bool TryCollectProduce()
    {
        if (_manager == null)
        {
            return false;
        }

        return _manager.TryCollectProduce(this);
    }

    internal void AddAnimalFromManager()
    {
        if (_animals.Count >= maxAnimals)
        {
            return;
        }

        AddAnimalInternal();
    }

    internal int TakeStoredProduce()
    {
        int produce = _storedProduce;
        _storedProduce = 0;
        return produce;
    }

    internal Item GetProduceItem()
    {
        return animalData != null ? animalData.produceItem : null;
    }

    internal void TryDeliverProduce()
    {
        if (_manager == null)
        {
            return;
        }

        Item produceItem = GetProduceItem();
        if (produceItem == null)
        {
            return;
        }

        int produce = _storedProduce;
        if (produce <= 0)
        {
            return;
        }

        ItemContainer target = _manager.Inventory;
        if (target == null)
        {
            return;
        }

        target.Add(produceItem, produce);
        _storedProduce = 0;
    }

    internal void SetManager(AnimalManager manager)
    {
        _manager = manager;
    }

    private void AddAnimalInternal()
    {
        _animals.Add(new AnimalState());
    }
}
