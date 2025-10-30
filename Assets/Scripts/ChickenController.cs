using UnityEngine;
using System.Collections;

public class ChickenController : MonoBehaviour
{
    // General settings
    public float moveSpeed = 1f;
    public float fleeSpeed = 3f;
    public float wanderRadius = 3f;

    // Feeding settings
    public float detectionRadius = 5f;
    public float eatDuration = 3f;
    public string feedTag = "Feed";

    // Egg laying settings
    [SerializeField] private GameObject eggPrefab;
    public float timeToLayEgg = 5f;

    // Incubation settings
    [SerializeField] private GameObject chickenPrefab;
    [SerializeField] private GameObject eggPickupPrefab;
    public float incubationTime = 10f;

    // State machine
    private enum ChickenState { Wandering, MovingToFood, Eating, LayingEgg, Incubating, Fleeing }
    private ChickenState currentState;

    // Private variables
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 startingPosition;
    private Vector2 targetPosition;
    private Transform targetFeed = null;
    private float stateTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startingPosition = transform.position;
        SetState(ChickenState.Wandering);
    }

    void Update()
    {
        switch (currentState)
        {
            case ChickenState.Wandering:
                Wander();
                LookForFood();
                break;
            case ChickenState.MovingToFood:
                MoveToFood();
                break;
            case ChickenState.Eating:
                // Logic is handled in coroutine
                break;
            case ChickenState.LayingEgg:
                Wander(); // Can move while preparing to lay egg
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    LayEgg();
                }
                break;
            case ChickenState.Incubating:
                // Logic is handled in coroutine
                break;
            case ChickenState.Fleeing:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    SetState(ChickenState.Wandering);
                }
                break;
        }
    }

    private void SetState(ChickenState newState)
    {
        currentState = newState;
        StopAllCoroutines(); // Stop any running behaviors

        switch (currentState)
        {
            case ChickenState.Wandering:
                StartCoroutine(WanderRoutine());
                break;
            case ChickenState.MovingToFood:
                StartCoroutine(MoveToFoodRoutine());
                break;
            case ChickenState.Eating:
                StartCoroutine(EatRoutine());
                break;
            case ChickenState.LayingEgg:
                stateTimer = timeToLayEgg;
                StartCoroutine(WanderRoutine());
                break;
            case ChickenState.Incubating:
                StartCoroutine(IncubateRoutine());
                break;
            case ChickenState.Fleeing:
                // Fleeing is triggered by collision, not directly by SetState with argument
                break;
        }
    }

    private void LookForFood()
    {
        if (targetFeed != null) return;

        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var obj in detectedObjects)
        {
            if (obj.CompareTag(feedTag))
            {
                targetFeed = obj.transform;
                SetState(ChickenState.MovingToFood);
                return;
            }
        }
    }

    private void Wander()
    {
        // The WanderRoutine handles the movement. This is just for conceptual clarity.
    }

    private IEnumerator WanderRoutine()
    {
        while (currentState == ChickenState.Wandering || currentState == ChickenState.LayingEgg)
        {
            targetPosition = startingPosition + Random.insideUnitCircle * wanderRadius;
            animator.SetBool("isWalking", true);

            while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
            {
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                rb.velocity = direction * moveSpeed;
                UpdateSpriteDirection(direction);
                yield return null;
            }

            rb.velocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            yield return new WaitForSeconds(Random.Range(1f, 3f)); // Brief idle
        }
    }

    private void MoveToFood()
    {
        // The MoveToFoodRoutine handles the movement.
    }

    private IEnumerator MoveToFoodRoutine()
    {
        animator.SetBool("isWalking", true);
        while (targetFeed != null && Vector2.Distance(transform.position, targetFeed.position) > 0.5f)
        {
            Vector2 direction = (targetFeed.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
            UpdateSpriteDirection(direction);
            yield return null;
        }

        if (targetFeed != null)
        {
            SetState(ChickenState.Eating);
        }
        else // Food disappeared
        {
            SetState(ChickenState.Wandering);
        }
    }

    private IEnumerator EatRoutine()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("isWalking", false);
        animator.SetBool("isEating", true);

        yield return new WaitForSeconds(eatDuration);

        if (targetFeed != null)
        {
            Destroy(targetFeed.gameObject);
        }

        animator.SetBool("isEating", false);
        targetFeed = null;
        SetState(ChickenState.LayingEgg);
    }

    private void LayEgg()
    {
        Instantiate(eggPrefab, transform.position, Quaternion.identity);
        Debug.Log("Chicken laid an egg!");
        SetState(ChickenState.Wandering);
    }

    public void StartIncubation()
    {
        SetState(ChickenState.Incubating);
    }

    private IEnumerator IncubateRoutine()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("isWalking", false);
        Debug.Log("Chicken has started incubating.");

        yield return new WaitForSeconds(incubationTime);

        Hatch();
        SetState(ChickenState.Wandering);
    }

    private void Hatch()
    {
        Debug.Log("A new chicken has hatched!");
        Instantiate(chickenPrefab, transform.position, Quaternion.identity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (currentState == ChickenState.Incubating)
            {
                StopAllCoroutines();
                Debug.Log("Incubation was interrupted!");
                if (eggPickupPrefab != null)
                {
                    Instantiate(eggPickupPrefab, transform.position, Quaternion.identity);
                }
                Flee(collision.transform);
            }
            else if (currentState != ChickenState.Fleeing)
            {
                Flee(collision.transform);
            }
        }
    }

    private void Flee(Transform player)
    {
        StopAllCoroutines();
        currentState = ChickenState.Fleeing;
        stateTimer = 1.5f; // Flee for 1.5 seconds

        Debug.Log("Chicken is fleeing!");
        animator.SetBool("isWalking", true);

        Vector2 fleeDirection = (transform.position - player.position).normalized;
        rb.velocity = fleeDirection * fleeSpeed;
        UpdateSpriteDirection(fleeDirection);
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startingPosition, wanderRadius);
    }
}