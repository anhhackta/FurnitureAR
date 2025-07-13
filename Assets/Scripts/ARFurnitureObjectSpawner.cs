using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

/// <summary>
/// Bridge Object Spawner với ARFurnitureManagerSimple
/// Sync Object Spawner với furniture selection từ UI
/// </summary>
public class ARFurnitureObjectSpawner : MonoBehaviour
{
    [Header("ARFurnitureManagerSimple Integration")]
    [SerializeField]
    [Tooltip("Reference to ARFurnitureManagerSimple")]
    ARFurnitureManagerSimple m_FurnitureManager;

    [SerializeField]
    [Tooltip("Reference to Object Spawner component")]
    ObjectSpawner m_ObjectSpawner;

    void Start()
    {
        // Auto-find components
        if (m_ObjectSpawner == null)
            m_ObjectSpawner = GetComponent<ObjectSpawner>();

        if (m_FurnitureManager == null)
#if UNITY_2023_1_OR_NEWER
            m_FurnitureManager = FindAnyObjectByType<ARFurnitureManagerSimple>();
#else
            m_FurnitureManager = FindObjectOfType<ARFurnitureManagerSimple>();
#endif

        if (m_ObjectSpawner != null)
        {
            // Subscribe to spawned event
            m_ObjectSpawner.objectSpawned += OnObjectSpawned;
            
            // Clear prefab list initially
            m_ObjectSpawner.objectPrefabs.Clear();
            m_ObjectSpawner.spawnOptionIndex = -1;
            
            Debug.Log("ARFurnitureObjectSpawner: Connected to Object Spawner, waiting for furniture selection");
        }
    }

    void OnDestroy()
    {
        if (m_ObjectSpawner != null)
            m_ObjectSpawner.objectSpawned -= OnObjectSpawned;
    }

    /// <summary>
    /// Update Object Spawner khi user chọn furniture trong UI
    /// Gọi method này từ ARFurnitureManagerSimple.SelectFurniture()
    /// </summary>
    public void UpdateSelectedFurniture(int selectedIndex)
    {
        if (m_ObjectSpawner == null || m_FurnitureManager == null) return;

        // Clear prefab list
        m_ObjectSpawner.objectPrefabs.Clear();
        
        // Add only selected furniture prefab
        if (selectedIndex >= 0 && selectedIndex < m_FurnitureManager.furnitureDatabase.furnitureItems.Length)
        {
            GameObject selectedPrefab = m_FurnitureManager.furnitureDatabase.furnitureItems[selectedIndex].prefab;
            if (selectedPrefab != null)
            {
                m_ObjectSpawner.objectPrefabs.Add(selectedPrefab);
                m_ObjectSpawner.spawnOptionIndex = 0; // Spawn selected furniture
                
                Debug.Log($"ARFurnitureObjectSpawner: Ready to spawn {selectedPrefab.name}");
            }
        }
        else
        {
            // No selection - disable spawning
            m_ObjectSpawner.spawnOptionIndex = -1;
            Debug.Log("ARFurnitureObjectSpawner: No furniture selected, spawning disabled");
        }
    }

    /// <summary>
    /// Called khi Object Spawner spawn object
    /// </summary>
    void OnObjectSpawned(GameObject spawnedObject)
    {
        if (spawnedObject != null && m_FurnitureManager != null)
        {
            // Setup spawned object
            spawnedObject.tag = "Furniture";
            
            if (spawnedObject.GetComponent<Collider>() == null)
                spawnedObject.AddComponent<BoxCollider>();

            // Move to ground plane stage
            if (m_FurnitureManager.groundPlaneStage != null)
                spawnedObject.transform.SetParent(m_FurnitureManager.groundPlaneStage);

            // Register với ARFurnitureManagerSimple
            m_FurnitureManager.RegisterPlacedObject(spawnedObject);
            
            // Auto-select spawned object
            m_FurnitureManager.SelectPlacedObject(spawnedObject);

            Debug.Log($"ARFurnitureObjectSpawner: Spawned and registered {spawnedObject.name}");
        }
    }

    // Public properties
    public ARFurnitureManagerSimple furnitureManager
    {
        get => m_FurnitureManager;
        set => m_FurnitureManager = value;
    }

    public ObjectSpawner objectSpawner
    {
        get => m_ObjectSpawner;
        set => m_ObjectSpawner = value;
    }
}
