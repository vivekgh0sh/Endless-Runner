// --- START OF FILE MovingPlatformManager.cs ---
using UnityEngine;
using System.Collections.Generic;

public class MovingPlatformManager : MonoBehaviour
{
    [Header("Platform Settings")]
    public GameObject platformPrefab;
    public float platformLength = 50f;
    public float platformSpawnY = -0.5f; // Y position of platform's ROOT for its surface to be at Y=0
    public int numberOfPlatformsOnScreen = 7;

    [Header("World Settings")]
    public Transform playerTransform;
    public float worldSpeed = 10f;

    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs;
    [Range(0f, 1f)]
    public float obstacleSpawnChance = 0.5f;
    // This Y offset is now relative to the ObstacleAnchor (which is at the platform's root Y level).
    // If ObstacleAnchor is at platform root Y=-0.5, and platform surface is Y=0,
    // and obstacle pivot is at its BASE:
    // To place obstacle base ON the surface (Y=0), obstacleSpawnOffset.y should be +0.5f.
    // (Because local Y 0.5 within an anchor at Y -0.5 results in world Y 0).
    // If obstacle pivot is at its CENTER and height is 2, then its base is -1 from its pivot.
    // To place its base on surface (Y=0), its pivot needs to be at world Y=1.
    // So, obstacleSpawnOffset.y would be 1.5f (because -0.5 + 1.5 = 1).
    // **ADJUST THIS CAREFULLY BASED ON YOUR OBSTACLE PREFAB'S PIVOT AND HEIGHT, AND PLATFORM SPAWN Y**
    public Vector3 obstacleSpawnOffset = new Vector3(0, 0.5f, 0); // Example: for base-pivoted obstacle to sit on surface

    private List<GameObject> activePlatforms = new List<GameObject>();
    private Queue<GameObject> platformPool = new Queue<GameObject>();
    private float lastPlatformEndZ;

    private Dictionary<string, Queue<GameObject>> obstaclePools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, Vector3> originalObstacleScales = new Dictionary<string, Vector3>(); // To store original scales

    void Start()
    {
        if (platformPrefab == null) { Debug.LogError("Platform Prefab not assigned!"); this.enabled = false; return; }
        if (playerTransform == null) { Debug.LogError("Player Transform not assigned!"); this.enabled = false; return; }
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) { Debug.LogWarning("No obstacle prefabs assigned."); }

        InitializePlatformPool();
        InitializeObstaclePoolsAndScales(); // Renamed to also store scales

        float currentSpawnZ = playerTransform.position.z;
        for (int i = 0; i < numberOfPlatformsOnScreen; i++)
        {
            SpawnPlatform(currentSpawnZ, true);
            currentSpawnZ += platformLength;
        }
        lastPlatformEndZ = currentSpawnZ;
    }

    void InitializePlatformPool()
    {
        for (int i = 0; i < numberOfPlatformsOnScreen + 3; i++)
        {
            GameObject go = Instantiate(platformPrefab, Vector3.one * -2000, Quaternion.identity, this.transform);
            go.SetActive(false);
            platformPool.Enqueue(go);
        }
    }

    void InitializeObstaclePoolsAndScales()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        foreach (GameObject obsPrefab in obstaclePrefabs)
        {
            if (obsPrefab == null) continue;
            Queue<GameObject> pool = new Queue<GameObject>();
            // Store original scale
            originalObstacleScales[obsPrefab.name] = obsPrefab.transform.localScale;

            for (int i = 0; i < 15; i++) // << INCREASED POOL SIZE
            {
                GameObject obs = Instantiate(obsPrefab, Vector3.one * -2000, Quaternion.identity, this.transform);
                obs.transform.localScale = originalObstacleScales[obsPrefab.name]; // Ensure it has original scale
                obs.SetActive(false);
                pool.Enqueue(obs);
            }
            obstaclePools[obsPrefab.name] = pool;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0 || worldSpeed <= 0) return;

        float movementDelta = worldSpeed * Time.deltaTime;
        for (int i = 0; i < activePlatforms.Count; i++)
        {
            if (activePlatforms[i] != null)
            {
                activePlatforms[i].transform.Translate(Vector3.back * movementDelta, Space.World);
            }
        }
        lastPlatformEndZ -= movementDelta;

        if (activePlatforms.Count > 0 && activePlatforms[0].transform.position.z + platformLength < playerTransform.position.z - (platformLength * 1.5f))
        {
            RecyclePlatform();
        }

        float desiredTrackEndAhead = playerTransform.position.z + (numberOfPlatformsOnScreen - 1) * platformLength;
        if (lastPlatformEndZ < desiredTrackEndAhead + platformLength)
        {
            SpawnPlatform(lastPlatformEndZ, true);
            lastPlatformEndZ += platformLength;
        }
    }

    void SpawnPlatform(float spawnZ, bool canSpawnObstacles)
    {
        if (platformPool.Count == 0) { Debug.LogWarning("Platform pool empty!"); return; }

        GameObject platformInstance = platformPool.Dequeue();
        platformInstance.transform.position = new Vector3(0, platformSpawnY, spawnZ);
        platformInstance.transform.rotation = Quaternion.identity;
        platformInstance.SetActive(true);
        activePlatforms.Add(platformInstance);

        if (canSpawnObstacles && obstaclePrefabs != null && obstaclePrefabs.Length > 0 && Random.value < obstacleSpawnChance)
        {
            SpawnObstacleOnPlatform(platformInstance);
        }
    }

    void SpawnObstacleOnPlatform(GameObject platformInstance)
    {
        GameObject selectedObstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        if (selectedObstaclePrefab == null) return;

        GameObject obstacleInstance = GetObstacleFromPool(selectedObstaclePrefab.name);
        if (obstacleInstance == null) return;

        Transform obstacleAnchor = platformInstance.transform.Find("ObstacleAnchor");
        if (obstacleAnchor == null)
        {
            Debug.LogError($"Platform {platformInstance.name} is missing ObstacleAnchor child! Obstacle will be parented directly to platform, may cause scaling issues.", platformInstance);
            obstacleInstance.transform.SetParent(platformInstance.transform, false); // worldPositionStays = false
        }
        else
        {
            obstacleInstance.transform.SetParent(obstacleAnchor, false); // worldPositionStays = false
        }

        // --- Positioning Logic ---
        PlayerSidewaysMovement playerMovement = FindObjectOfType<PlayerSidewaysMovement>(); // Consider caching this
        float laneWidth = playerMovement ? playerMovement.laneWidth : 2f;
        int randomLane = Random.Range(-1, 2); // -1 (left), 0 (center), 1 (right)
        float obstacleX = randomLane * laneWidth;

        // Z position on platform: 0 for start, platformLength for end.
        // Let's place it randomly along the middle 80% of the platform length.
        float randomZOnPlatform = Random.Range(platformLength * 0.1f, platformLength * 0.9f);

        // obstacleSpawnOffset.y is the key for vertical placement.
        // It's the local Y position *within the ObstacleAnchor*.
        // The ObstacleAnchor itself is at the platform's root Y (e.g., -0.5 if platform surface is at 0).
        // So, if obstacle pivot is at its base, and you want its base on the platform surface,
        // obstacleSpawnOffset.y should be +0.5f (if platformSpawnY = -0.5 for surface at 0).
        // This makes the obstacle's local Y position 0.5 relative to the anchor.
        // Anchor is at world Y -0.5, so obstacle base is at world Y = -0.5 (anchor) + 0.5 (local offset) = 0.
        obstacleInstance.transform.localPosition = new Vector3(obstacleX, obstacleSpawnOffset.y, randomZOnPlatform + obstacleSpawnOffset.z); // Added Z offset from variable too
        obstacleInstance.transform.localRotation = Quaternion.identity;
        // The scale is reset to its original prefab scale in GetObstacleFromPool

        // ... inside SpawnObstacleOnPlatform(GameObject platformInstance) ...
        // ... after parenting and setting localPosition, localRotation ...

        // The scale is reset to its original prefab scale in GetObstacleFromPool
        // Let's explicitly set it here again just to be sure and log
        if (originalObstacleScales.ContainsKey(selectedObstaclePrefab.name))
        {
            obstacleInstance.transform.localScale = originalObstacleScales[selectedObstaclePrefab.name];
            Debug.Log($"Obstacle: {obstacleInstance.name}, Parent: {obstacleInstance.transform.parent.name}, " +
                      $"Parent Scale: {obstacleInstance.transform.parent.lossyScale}, " +
                      $"Obstacle LocalScale set to: {obstacleInstance.transform.localScale}, " +
                      $"Obstacle LossyScale: {obstacleInstance.transform.lossyScale}");
        }
        else
        {
            Debug.LogError($"Could not find original scale for {selectedObstaclePrefab.name}");
        }


        obstacleInstance.SetActive(true);
    

}

    GameObject GetObstacleFromPool(string prefabName)
    {
        if (obstaclePools.ContainsKey(prefabName) && obstaclePools[prefabName].Count > 0)
        {
            GameObject obs = obstaclePools[prefabName].Dequeue();
            // Ensure scale is correct from prefab, as it might have been changed if parented incorrectly before
            if (originalObstacleScales.ContainsKey(prefabName))
            {
                obs.transform.localScale = originalObstacleScales[prefabName];
            }
            return obs;
        }
        else if (obstaclePools.ContainsKey(prefabName))
        {
            Debug.LogWarning($"Obstacle pool for {prefabName} is empty. Instantiating new one.");
            GameObject originalPrefab = null;
            foreach (GameObject p in obstaclePrefabs) { if (p != null && p.name == prefabName) { originalPrefab = p; break; } }

            if (originalPrefab != null)
            {
                GameObject obs = Instantiate(originalPrefab, Vector3.one * -2000, Quaternion.identity, this.transform);
                obs.transform.localScale = originalObstacleScales[prefabName]; // Set its scale
                return obs;
            }
        }
        Debug.LogError($"Could not find original prefab for {prefabName} to instantiate a new one.");
        return null;
    }

    void RecyclePlatform()
    {
        if (activePlatforms.Count == 0) return;
        GameObject platformToRecycle = activePlatforms[0];

        // Recycle obstacles: Find the anchor first
        Transform obstacleAnchor = platformToRecycle.transform.Find("ObstacleAnchor");
        if (obstacleAnchor != null)
        {
            // Iterate backwards when removing children from a transform in a loop
            for (int i = obstacleAnchor.childCount - 1; i >= 0; i--)
            {
                Transform child = obstacleAnchor.GetChild(i);
                if (child.gameObject.CompareTag("Obstacle")) // Ensure your obstacle prefabs are tagged "Obstacle"
                {
                    RecycleObstacle(child.gameObject);
                }
                else
                {
                    // If other things are parented, decide what to do (e.g., destroy them)
                    // Destroy(child.gameObject);
                }
            }
        }
        else
        { // Fallback if no anchor (shouldn't happen with new setup)
            for (int i = platformToRecycle.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = platformToRecycle.transform.GetChild(i);
                if (child.gameObject.CompareTag("Obstacle")) RecycleObstacle(child.gameObject);
            }
        }


        platformToRecycle.SetActive(false);
        activePlatforms.RemoveAt(0);
        platformPool.Enqueue(platformToRecycle);
    }

    void RecycleObstacle(GameObject obstacleInstance)
    {
        string poolKey = "";
        // Try to find the original prefab name (this is the simplest way, assumes instance name starts with prefab name)
        // A more robust method would be to attach a component to the obstacle storing its prefab ID/name.
        foreach (var kvp in originalObstacleScales) // Iterate stored original prefab names
        {
            if (obstacleInstance.name.StartsWith(kvp.Key)) // kvp.Key is the original prefab name
            {
                poolKey = kvp.Key;
                break;
            }
        }

        if (!string.IsNullOrEmpty(poolKey) && obstaclePools.ContainsKey(poolKey))
        {
            obstacleInstance.SetActive(false);
            obstacleInstance.transform.SetParent(this.transform); // Unparent and parent to manager for pooling
            obstaclePools[poolKey].Enqueue(obstacleInstance);
        }
        else
        {
            Debug.LogWarning($"Could not find pool for obstacle {obstacleInstance.name} (derived key: '{poolKey}'). Destroying.", obstacleInstance);
            Destroy(obstacleInstance);
        }
    }

    public void ResetWorld(Vector3 playerStartPos)
    {
        while (activePlatforms.Count > 0)
        {
            RecyclePlatform(); // This will now also handle recycling obstacles on the platform
        }

        float currentSpawnZ = playerStartPos.z;
        for (int i = 0; i < numberOfPlatformsOnScreen; i++)
        {
            SpawnPlatform(currentSpawnZ, true);
            currentSpawnZ += platformLength;
        }
        lastPlatformEndZ = currentSpawnZ;
    }
}
// --- END OF FILE MovingPlatformManager.cs ---