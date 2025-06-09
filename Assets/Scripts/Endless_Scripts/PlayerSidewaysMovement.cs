// --- START OF FILE PlayerSidewaysMovement.cs ---
using UnityEngine;

public class PlayerSidewaysMovement : MonoBehaviour
{
    public Rigidbody rb;
    // public float sidewaysSpeed = 10f; // Used if you opt for continuous movement
    public float laneWidth = 2f;
    public float laneChangeSpeed = 10f;

    private int currentLane = 0; // 0 for center, -1 for left, 1 for right (assuming 3 lanes)
    private int maxLanes = 1;    // Max deviation from center lane (e.g., 1 for 3 lanes total: -1, 0, 1)
    private float targetXPosition;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        rb.useGravity = true;
        // Freeze Z position and all rotations. Allow Y for gravity, X for movement.
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        targetXPosition = transform.position.x; // Initialize with current/starting X
    }

    void Update() // Input is best in Update
    {
        // Keyboard Controls (keep for testing in editor)
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(1);
        }

        // Touch Controls
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // Get the first touch

            // Check if the touch just began (to avoid multiple moves per continuous touch)
            if (touch.phase == TouchPhase.Began)
            {
                if (touch.position.x < Screen.width / 2) // Touched left half of the screen
                {
                    ChangeLane(-1); // Move Left
                }
                else if (touch.position.x > Screen.width / 2) // Touched right half of the screen
                {
                    ChangeLane(1); // Move Right
                }
            }
        }
    }

    void FixedUpdate() // Movement/Physics in FixedUpdate
    {
        // Smoothly move to the target X position for lane snapping
        Vector3 newPosition = rb.position; // Get current position
        // Only modify X, keep Y and Z from physics/constraints
        newPosition.x = Mathf.MoveTowards(rb.position.x, targetXPosition, laneChangeSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);


        // Check for falling (if you have a game over condition for falling)
        if (transform.position.y < -5f) // Adjust Y threshold as needed
        {
            EndlessGameController egc = FindObjectOfType<EndlessGameController>();
            if (egc != null && !egc.isGameOver)
            {
                Debug.Log("Player fell off!");
                egc.GameOver();
            }
        }
    }

    void ChangeLane(int direction)
    {
        int newLaneTarget = currentLane + direction;

        // Clamp newLaneTarget to be within your defined lane limits (e.g., -1 to 1 for 3 lanes)
        currentLane = Mathf.Clamp(newLaneTarget, -maxLanes, maxLanes);

        targetXPosition = currentLane * laneWidth;
    }

    public void ResetPlayer(Vector3 startPosition)
    {
        transform.position = startPosition; // Directly set position
        rb.linearVelocity = Vector3.zero;         // Reset physics velocity
        rb.angularVelocity = Vector3.zero;  // Reset angular velocity
        currentLane = 0;                    // Reset to center lane
        targetXPosition = startPosition.x;  // Set target X based on start position's X
        // Ensure player is active and script enabled if it was disabled on game over
        this.enabled = true;
        gameObject.SetActive(true);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle")) // Ensure your obstacles are tagged "Obstacle"
        {
            Debug.Log("Player collided with an Obstacle!");

            EndlessGameController egc = FindObjectOfType<EndlessGameController>();
            if (egc != null && !egc.isGameOver)
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