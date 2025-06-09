// --- START OF FILE PlatformManager.cs ---
using UnityEngine;
using System.Collections.Generic;

public class PlatformManager : MonoBehaviour
{
    public GameObject platformPrefab;
    public Transform playerTransform;
    public int initialPlatforms = 5;
    public float platformLength = 50f;
    public float platformSpawnY = 0f; // <--- ADD THIS PUBLIC VARIABLE

    private List<GameObject> activePlatforms = new List<GameObject>();
    private Queue<GameObject> platformPool = new Queue<GameObject>();
    private float nextSpawnZ = 0f;

    void Start()
    {
        if (platformPrefab == null)
        {
            Debug.LogError("Platform Prefab not assigned in PlatformManager!");
            this.enabled = false;
            return;
        }
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform not assigned in PlatformManager!");
            this.enabled = false;
            return;
        }

        // Determine the offset from the prefab's root to its actual surface
        // This is a bit advanced to do automatically if the prefab structure is complex.
        // For now, we rely on platformSpawnY being set correctly in the Inspector.
        // A common convention: if platformSpawnY is set to 0, it means the PLATFORM'S SURFACE
        // should be at Y=0. If the prefab's pivot is at its center and height is 1,
        // then the prefab's root must be spawned at Y = -0.5f for its surface to be at Y=0.

        int poolSize = initialPlatforms + 2;
        for (int i = 0; i < poolSize; i++)
        {
            // Instantiate at a non-interfering position initially
            GameObject platform = Instantiate(platformPrefab, new Vector3(0, platformSpawnY - 1000, 0), Quaternion.identity, this.transform);
            platform.SetActive(false);
            platformPool.Enqueue(platform);
        }

        // Adjust initial nextSpawnZ if your player doesn't start at Z=0 facing positive Z
        // This example assumes player starts near Z=0 and moves along +Z
        nextSpawnZ = 0f; // Or adjust: playerTransform.position.z - (platformLength * (initialPlatforms / 2f))

        for (int i = 0; i < initialPlatforms; i++)
        {
            SpawnPlatform();
        }
    }

    void Update()
    {
        if (playerTransform.position.z + (platformLength * 1.5f) > nextSpawnZ)
        {
            SpawnPlatform();
        }

        if (activePlatforms.Count > 0)
        {
            GameObject oldestPlatform = activePlatforms[0];
            if (playerTransform.position.z > oldestPlatform.transform.position.z + platformLength + (platformLength * 0.5f))
            {
                RecyclePlatform(oldestPlatform);
            }
        }
    }

    void SpawnPlatform()
    {
        if (platformPool.Count == 0)
        {
            Debug.LogWarning("Platform pool is empty.");
            return;
        }

        GameObject platformToSpawn = platformPool.Dequeue();

        // Spawn the platform ROOT at the specified Y position
        platformToSpawn.transform.position = new Vector3(0, platformSpawnY, nextSpawnZ);
        platformToSpawn.transform.rotation = Quaternion.identity;
        platformToSpawn.SetActive(true);

        activePlatforms.Add(platformToSpawn);
        nextSpawnZ += platformLength;
    }

    void RecyclePlatform(GameObject platformToRecycle)
    {
        platformToRecycle.SetActive(false);
        activePlatforms.RemoveAt(0);
        platformPool.Enqueue(platformToRecycle);
    }

    public void ResetPlatforms()
    {
        while (activePlatforms.Count > 0)
        {
            GameObject platform = activePlatforms[0];
            platform.SetActive(false);
            activePlatforms.RemoveAt(0);
            platformPool.Enqueue(platform);
        }
        nextSpawnZ = 0f; // Reset based on your game's starting Z
        for (int i = 0; i < initialPlatforms; i++)
        {
            if (platformPool.Count > 0)
            {
                SpawnPlatform();
            }
            else { break; }
        }
    }
}
// --- END OF FILE PlatformManager.cs ---