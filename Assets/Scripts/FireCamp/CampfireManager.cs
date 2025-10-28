using UnityEngine;

public class CampfireManager : MonoBehaviour
{
    public static CampfireManager instance;

    [SerializeField] private DayTimeController dayTimeController;
    [SerializeField] private float nightStartTime = 72000f; // 8 PM in seconds
    [SerializeField] private float nightEndTime = 18000f; // 5 AM in seconds (next day)

    public bool isCampfireLit = false;
    private int consecutiveNightsLit = 0;
    private int consecutiveNightsUnlit = 0;

    public System.Action OnCampfireStateChanged; // Event to notify others of state changes

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (CampfireManager.instance != null)
        {
            CampfireManager.instance.OnCampfireStateChanged += HandleCampfireStateChanged;
        }
        DayTimeController.OnDayEnded += ProcessNightTransition;
    }

    private void OnDisable()
    {
        if (CampfireManager.instance != null)
        {
            CampfireManager.instance.OnCampfireStateChanged -= HandleCampfireStateChanged;
        }
        DayTimeController.OnDayEnded -= ProcessNightTransition;
    }

    private void Start()
    {
        if (dayTimeController == null)
        {
            dayTimeController = FindObjectOfType<DayTimeController>();
            if (dayTimeController == null)
            {
                Debug.LogError("CampfireManager: DayTimeController not found in scene!");
            }
        }
        // Subscribe to day change event from DayTimeController if it exists
        // (Assuming DayTimeController has an event for day change, if not, we'll need to add one)
        // For now, we'll check time in Update, but an event would be cleaner.
    }

    private void ProcessNightTransition()
    {
        if (isCampfireLit)
        {
            consecutiveNightsLit++;
            consecutiveNightsUnlit = 0;
            Debug.Log($"Campfire lit for {consecutiveNightsLit} consecutive nights.");
        }
        else
        {
            consecutiveNightsUnlit++;
            consecutiveNightsLit = 0;
            Debug.Log($"Campfire unlit for {consecutiveNightsUnlit} consecutive nights.");
        }
        OnCampfireStateChanged?.Invoke(); // Notify listeners
    }

    public void SetCampfireLit(bool lit)
    {
        if (isCampfireLit != lit)
        {
            isCampfireLit = lit;
            OnCampfireStateChanged?.Invoke(); // Notify listeners
            Debug.Log($"Campfire state changed to: {isCampfireLit}");
        }
    }

    public int GetConsecutiveNightsLit()
    {
        return consecutiveNightsLit;
    }

    public int GetConsecutiveNightsUnlit()
    {
        return consecutiveNightsUnlit;
    }
}