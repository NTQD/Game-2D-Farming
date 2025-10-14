
using System.Collections;
using UnityEngine;

public class ChickenController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float wanderRadius = 3f; // The area in which the chicken can wander from its starting point
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;
    public float minWanderTime = 3f;
    public float maxWanderTime = 6f;

    [Header("Feeding Settings")]
    public float detectionRadius = 5f;
    public float eatDuration = 3f;
    public string feedTag = "Feed";

    [Header("Interaction Settings")]
    public float fleeSpeed = 3f;

    [Header("Incubation Settings")]
    [SerializeField] private GameObject chickenPrefab;
    [SerializeField] private GameObject eggPickupPrefab; // The item to drop when interrupted
    public float incubationTime = 10f;
    private bool isIncubating = false;

    [Header("Egg Laying Settings")]
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private float timeToLayEgg = 20f;
    private float eggTimer;
    private bool canLayEgg = false;

    private Transform targetFeed = null;
    private bool isEating = false;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 startingPosition;
    private Vector2 targetPosition;

    private Coroutine currentStateCoroutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startingPosition = transform.position;
        
        // Start the behavior loop
        currentStateCoroutine = StartCoroutine(Idle());
    }

    private void Update()
    {
        if (isIncubating) { return; }

        // If we aren't currently eating or heading to food, look for some.
        if (targetFeed == null)
        {
            LookForFood();
        }

        // Egg laying timer logic
        if (canLayEgg)
        {
            eggTimer -= Time.deltaTime;
            if (eggTimer <= 0f)
            {
                LayEgg();
            }
        }
    }

    private void LayEgg()
    {
        canLayEgg = false; // Ensure it only lays one egg per cycle
        Instantiate(eggPrefab, transform.position, Quaternion.identity);
        Debug.Log("Chicken laid an egg!");
    }

    private void LookForFood()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var obj in detectedObjects)
        {
            if (obj.CompareTag(feedTag))
            {
                targetFeed = obj.transform;
                // Stop current behavior and start eating routine
                if (currentStateCoroutine != null) { StopCoroutine(currentStateCoroutine); }
                currentStateCoroutine = StartCoroutine(MoveToAndEat(targetFeed));
                return; // Found food, no need to check for more
            }
        }
    }

    private IEnumerator MoveToAndEat(Transform food)
    {
        isEating = true;
        animator.SetBool("isWalking", true);

        // Move towards the food until we are close enough
        while (food != null && Vector2.Distance(transform.position, food.position) > 0.5f)
        {
            Vector2 direction = (food.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
            UpdateSpriteDirection(direction);
            yield return null;
        }

        // If food is null here, it means it was destroyed by another chicken while we were on our way.
        if (food == null)
        {
            isEating = false;
            targetFeed = null;
            currentStateCoroutine = StartCoroutine(Idle());
            yield break; // Exit the coroutine
        }

        // We've arrived. Stop moving and start eating.
        rb.velocity = Vector2.zero;
        animator.SetBool("isWalking", false);
        animator.SetBool("isEating", true);

        yield return new WaitForSeconds(eatDuration);

        Destroy(food.gameObject);

        // Finished eating
        animator.SetBool("isEating", false);
        targetFeed = null;
        isEating = false;

        // Start the egg timer
        canLayEgg = true;
        eggTimer = timeToLayEgg;
        Debug.Log("Chicken has eaten and will lay an egg in " + timeToLayEgg + " seconds.");

        // Go back to idling
        currentStateCoroutine = StartCoroutine(Idle());
    }

    private IEnumerator Idle()
    {
        // Stop moving
        rb.velocity = Vector2.zero;
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));

        // Start wandering
        currentStateCoroutine = StartCoroutine(Wander());
    }

    private IEnumerator Wander()
    {
        // Pick a random point within the wander radius
        targetPosition = startingPosition + Random.insideUnitCircle * wanderRadius;

        float wanderTime = Random.Range(minWanderTime, maxWanderTime);
        float elapsedTime = 0f;

        animator.SetBool("isWalking", true);

        while (elapsedTime < wanderTime)
        {
            // Don't wander if we are in the process of eating
            if (isEating) { yield break; }

            // Move towards the target position
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            rb.velocity = direction * moveSpeed;
            UpdateSpriteDirection(direction);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Stop wandering and go back to idle
        currentStateCoroutine = StartCoroutine(Idle());
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        // Read the current scale
        Vector3 currentScale = transform.localScale;

        // Flip sprite based on direction
        if (direction.x > 0)
        {
            // Face Right
            currentScale.x = Mathf.Abs(currentScale.x);
        }
        else if (direction.x < 0)
        {
            // Face Left
            currentScale.x = -Mathf.Abs(currentScale.x);
        }

        // Apply the new scale
        transform.localScale = currentScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the Player
        if (collision.gameObject.CompareTag("Player"))
        {
            // If the chicken is incubating, interrupt it
            if (isIncubating)
            {
                isIncubating = false;
                if (currentStateCoroutine != null) { StopCoroutine(currentStateCoroutine); }
                Debug.Log("Incubation was interrupted!");

                // Drop an egg pickup
                if(eggPickupPrefab != null)
                {
                    Instantiate(eggPickupPrefab, transform.position, Quaternion.identity);
                }
            }

            // Flee from the player
            if (currentStateCoroutine != null) { StopCoroutine(currentStateCoroutine); }
            currentStateCoroutine = StartCoroutine(Flee(collision.transform));
        }
    }

    private IEnumerator Flee(Transform player)
    {
        Debug.Log("Chicken is fleeing!");
        animator.SetBool("isWalking", true);

        // Calculate direction away from the player
        Vector2 fleeDirection = (transform.position - player.position).normalized;
        rb.velocity = fleeDirection * fleeSpeed;

        // Flee for 1 second
        yield return new WaitForSeconds(1.0f);

        // Stop and return to normal behavior
        currentStateCoroutine = StartCoroutine(Idle());
    }

    public void StartIncubation()
    {
        if (isIncubating) { return; } // Can't incubate if already incubating

        Debug.Log("Chicken has started incubating.");
        isIncubating = true;
        if (currentStateCoroutine != null) { StopCoroutine(currentStateCoroutine); }
        currentStateCoroutine = StartCoroutine(Incubate());
    }

    private IEnumerator Incubate()
    {
        // Stand still
        rb.velocity = Vector2.zero;
        animator.SetBool("isWalking", false);

        // Wait for the incubation time
        yield return new WaitForSeconds(incubationTime);

        Hatch();

        // Return to normal
        isIncubating = false;
        currentStateCoroutine = StartCoroutine(Idle());
    }

    private void Hatch()
    {
        Debug.Log("A new chicken has hatched!");
        // Spawn a new chicken at the current position
        Instantiate(chickenPrefab, transform.position, Quaternion.identity);
    }

    // Optional: Draw the wander and detection radii in the editor for easy visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startingPosition == Vector2.zero ? (Vector2)transform.position : startingPosition, wanderRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
