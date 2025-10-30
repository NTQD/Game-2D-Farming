using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HouseInteract : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private KeyCode toggleKey = KeyCode.F;

    private bool playerInRange;
    private bool isInside;

    // Cached components from player for quick enable/disable
    private PlayerControl playerControl;
    private ToolsCharacterController toolsController;
    private Rigidbody2D playerRb;
    private Collider2D[] playerColliders;
    private SpriteRenderer[] playerRenderers;
    private Animator playerAnimator;

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerControl = player.GetComponent<PlayerControl>();
            toolsController = player.GetComponent<ToolsCharacterController>();
            playerRb = player.GetComponent<Rigidbody2D>();
            playerAnimator = player.GetComponent<Animator>();
            playerColliders = player.GetComponents<Collider2D>();
            playerRenderers = player.GetComponentsInChildren<SpriteRenderer>(true);
        }

        // Ensure trigger collider is set correctly on the house
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    private void Update()
    {
        // Cho phép nhấn F cả khi đang ở trong nhà (collider player bị tắt nên có thể mất trigger)
        if (!(playerInRange || isInside)) return;
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePlayerVisibility();
        }
    }

    private void TogglePlayerVisibility()
    {
        if (player == null) return;

        isInside = !isInside;

        // Stop movement immediately
        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
        }

        // Enable/disable control scripts
        if (playerControl != null) playerControl.enabled = !isInside;
        if (toolsController != null) toolsController.enabled = !isInside;

        // Enable/disable colliders so the player doesn't block the doorway
        if (playerColliders != null)
        {
            for (int i = 0; i < playerColliders.Length; i++)
            {
                playerColliders[i].enabled = !isInside;
            }
        }

        // Toggle renderers so the character appears/disappears visually
        if (playerRenderers != null)
        {
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                playerRenderers[i].enabled = !isInside;
            }
        }

        // Update animation state
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("moving", false);
        }

        // Nếu vừa ra khỏi nhà, đảm bảo có thể nhấn F tiếp (vẫn ở trong vùng)
        if (!isInside)
        {
            playerInRange = true; // giữ cho phép toggle khi colliders vừa bật lại
        }

        // NOTE: TemperatureController is not touched; its current value stays as-is
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

}



