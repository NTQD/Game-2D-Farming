using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // New: For scene management

public class BoatController : MonoBehaviour
{
    public Sprite[] boatStates; // Assign your 4 boat state sprites here
    public float animationSpeed = 0.5f; // Speed of animation during sailing
    public float sailingSpeed = 1.0f; // Speed of boat moving out to sea
    public Vector3 sailingDirection = new Vector3(-1, -1, 0).normalized; // Direction boat sails off-screen (bottom-left)

    private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex = 0;
    private bool isSailing = false;
    private bool playerIsNear = false; // New: To track if player is in proximity

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("BoatController: SpriteRenderer not found!");
            enabled = false; // Disable script if no SpriteRenderer
            return;
        }

        if (boatStates == null || boatStates.Length == 0)
        {
            Debug.LogWarning("BoatController: No boat states (sprites) assigned. Boat will not animate.");
            // Still allow interaction and sailing, just no animation
        } else if (boatStates.Length > 0) {
            spriteRenderer.sprite = boatStates[0]; // Set initial sprite
        }
    }

    private void Update()
    {
        if (playerIsNear && Input.GetKeyDown(KeyCode.F) && !isSailing)
        {
            StartSailing();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
            Debug.Log("Player is near the boat. Press F to get on board.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            Debug.Log("Player left the boat area.");
        }
    }

    public void StartSailing()
    {
        if (!isSailing)
        {
            isSailing = true;
            Debug.Log("Boat is starting to sail...");

            // Deactivate the player GameObject
            if (GameManager.instance != null && GameManager.instance.player != null)
            {
                GameManager.instance.player.SetActive(false);
            }

            if (boatStates != null && boatStates.Length > 1)
            {
                StartCoroutine(AnimateBoat());
            }
            StartCoroutine(SailAway());
        }
    }

    private IEnumerator AnimateBoat()
    {
        while (isSailing)
        {
            currentSpriteIndex = (currentSpriteIndex + 1) % boatStates.Length;
            spriteRenderer.sprite = boatStates[currentSpriteIndex];
            yield return new WaitForSeconds(animationSpeed);
        }
    }

    private IEnumerator SailAway()
    {
        // Fade out or move off-screen
        while (true)
        {
            transform.position += sailingDirection.normalized * sailingSpeed * Time.deltaTime;
            // You might add a check here to destroy/deactivate the boat after it goes off-screen
            // For now, let's just indicate game complete after a short delay
            if (Vector3.Distance(Vector3.zero, transform.position) > 100f) // Example: 100 units away from origin
            {
                Debug.Log("Boat has sailed away. Game Completed! Loading Victory Scene...");
                // Load the Victory Scene
                SceneManager.LoadScene("VictorySceneName"); // IMPORTANT: Replace "VictorySceneName" with your actual Victory scene name
                yield break;
            }
            yield return null;
        }
    }

    // Method to call when the boat should appear on the shore
    public void AppearOnShore(Vector3 position)
    {
        // Set initial position and enable the GameObject
        transform.position = position;
        gameObject.SetActive(true);
        isSailing = false;
        if (spriteRenderer != null && boatStates != null && boatStates.Length > 0)
        {
            spriteRenderer.sprite = boatStates[0]; // Reset to initial sprite
        }
    }

    // Hide boat or destroy it after game completion flow
    public void HideBoat()
    {
        gameObject.SetActive(false);
    }
}
