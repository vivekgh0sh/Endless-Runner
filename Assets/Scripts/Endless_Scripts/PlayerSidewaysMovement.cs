// --- START OF FILE PlayerSidewaysMovement.cs ---
using UnityEngine;

public class PlayerSidewaysMovement : MonoBehaviour
{
    public Rigidbody rb;
    public float sidewaysSpeed = 10f; // Speed of sideways movement
    public float laneWidth = 2f;      // How far to move for a lane change
    public float laneChangeSpeed = 10f; // How quickly to snap to new lane

    private int currentLane = 0; // 0 for center, -1 for left, 1 for right
    private float targetXPosition;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        // Ensure Rigidbody settings are appropriate
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ; // Freeze Z position and all rotations
        targetXPosition = transform.position.x;
    }

    void Update() // Input is better in Update
    {
        if (Input.GetKeyDown(KeyCode.A)) // Move Left
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D)) // Move Right
        {
            ChangeLane(1);
        }
    }

    void FixedUpdate() // Movement/Physics in FixedUpdate
    {
        // Smoothly move to the target X position
        Vector3 newPosition = rb.position;
        newPosition.x = Mathf.MoveTowards(rb.position.x, targetXPosition, laneChangeSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // Optional: If you want AddForce based sideways movement instead of lane snapping:
        /*
        float moveHorizontal = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            moveHorizontal = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveHorizontal = 1f;
        }
        Vector3 sidewaysForce = new Vector3(moveHorizontal * sidewaysSpeed, 0, 0);
        rb.AddForce(sidewaysForce * Time.fixedDeltaTime, ForceMode.VelocityChange); // Or ForceMode.Force
        */

        // Check for falling
        if (rb.position.y < -1f && FindObjectOfType<gameManager>() != null && !FindObjectOfType<gameManager>().gameHasEnded)
        {
            FindObjectOfType<gameManager>().EndGame();
        }
    }

    void ChangeLane(int direction)
    {
        int newLane = currentLane + direction;
        // Clamp newLane to -1, 0, 1 (or however many lanes you want)
        // For 3 lanes:
        if (newLane >= -1 && newLane <= 1)
        {
            currentLane = newLane;
            targetXPosition = currentLane * laneWidth;
        }
    }

    // Call this if game restarts to reset player position
    public void ResetPlayer(Vector3 startPosition)
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        currentLane = 0;
        targetXPosition = startPosition.x; // Or 0 if player always starts at world X=0
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Player collided with an Obstacle!");

            // Get the EndlessGameController to signal game over
            EndlessGameController egc = FindObjectOfType<EndlessGameController>();
            if (egc != null && !egc.isGameOver) // Check if EGC exists and game isn't already over
            {
                egc.GameOver();
            }
            else if (egc == null)
            {
                Debug.LogError("EndlessGameController not found in scene for GameOver event!");
            }
        }
    }
}
// --- END OF FILE PlayerSidewaysMovement.cs ---
