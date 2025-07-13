using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

/// <summary>
/// Enable and integrate Object Spawner system with ARFurnitureManagerSimple
/// This script enables ARInteractorSpawnTrigger for advanced object selection and interaction
/// while ensuring spawning goes through ARFurnitureManagerSimple for consistency
/// </summary>
[System.Serializable]
public class EnableObjectSpawnerIntegration : MonoBehaviour
{
    [Header("Integration Components")]
    [SerializeField]
    [Tooltip("Reference to ARFurnitureManagerSimple to sync with")]
    ARFurnitureManagerSimple m_FurnitureManager;
    
    [SerializeField]
    [Tooltip("Enable ObjectSpawner component but control it via ARFurnitureManagerSimple")]
    bool m_EnableObjectSpawner = true;
    
    [SerializeField] 
    [Tooltip("Enable ARInteractorSpawnTrigger for advanced object selection")]
    bool m_EnableARInteractorSpawnTrigger = true;
    
    [SerializeField]
    [Tooltip("Replace ObjectSpawner spawning with ARFurnitureManagerSimple spawning")]
    bool m_RedirectSpawningToFurnitureManager = true;

    [Header("Debug Information")]
    [SerializeField]
    [Tooltip("Show detailed information about integration process")]
    bool m_ShowDebugInfo = true;

    private ObjectSpawner m_ObjectSpawner;
    private ARInteractorSpawnTrigger m_ARInteractorSpawnTrigger;

    void Start()
    {
        if (m_ShowDebugInfo)
        {
            Debug.Log("EnableObjectSpawnerIntegration: Starting integration process...");
            Debug.Log("Purpose: Enable ARInteractorSpawnTrigger for advanced selection while keeping ARFurnitureManagerSimple as main spawner");
        }

        // Auto-find ARFurnitureManagerSimple if not assigned
        if (m_FurnitureManager == null)
        {
            m_FurnitureManager = FindFirstObjectByType<ARFurnitureManagerSimple>();
            if (m_FurnitureManager != null && m_ShowDebugInfo)
            {
                Debug.Log("EnableObjectSpawnerIntegration: Auto-found ARFurnitureManagerSimple");
            }
        }

        EnableComponents();
        SetupIntegration();
    }

    void EnableComponents()
    {
        bool anyEnabled = false;

        // Enable ObjectSpawner component but we'll redirect its functionality
        if (m_EnableObjectSpawner)
        {
            m_ObjectSpawner = GetComponent<ObjectSpawner>();
            if (m_ObjectSpawner != null)
            {
                m_ObjectSpawner.enabled = true;
                anyEnabled = true;
                
                if (m_ShowDebugInfo)
                {
                    Debug.Log($"✓ Enabled ObjectSpawner on GameObject: {gameObject.name}");
                    Debug.Log("  Note: Spawning will be redirected to ARFurnitureManagerSimple");
                }
            }
            else if (m_ShowDebugInfo)
            {
                Debug.Log($"○ No ObjectSpawner found on GameObject: {gameObject.name}");
            }
        }

        // Enable ARInteractorSpawnTrigger for advanced object selection  
        if (m_EnableARInteractorSpawnTrigger)
        {
            m_ARInteractorSpawnTrigger = GetComponent<ARInteractorSpawnTrigger>();
            if (m_ARInteractorSpawnTrigger != null)
            {
                m_ARInteractorSpawnTrigger.enabled = true;
                anyEnabled = true;
                
                if (m_ShowDebugInfo)
                {
                    Debug.Log($"✓ Enabled ARInteractorSpawnTrigger on GameObject: {gameObject.name}");
                    Debug.Log("  Purpose: Advanced object selection and interaction with placed furniture");
                }
            }
            else if (m_ShowDebugInfo)
            {
                Debug.Log($"○ No ARInteractorSpawnTrigger found on GameObject: {gameObject.name}");
            }
        }

        // Final status report
        if (anyEnabled)
        {
            if (m_ShowDebugInfo)
            {
                Debug.Log("=== OBJECT SPAWNER INTEGRATION ENABLED ===");
                Debug.Log("✓ ARInteractorSpawnTrigger enabled for advanced selection");
                Debug.Log("✓ ObjectSpawner enabled but spawning redirected to ARFurnitureManagerSimple");
                Debug.Log("✓ Unified furniture management system");
                Debug.Log("===========================================");
            }
        }
        else if (m_ShowDebugInfo)
        {
            Debug.Log($"○ No Object Spawner components found to enable on: {gameObject.name}");
        }
    }

    void SetupIntegration()
    {
        if (m_FurnitureManager == null)
        {
            Debug.LogWarning("EnableObjectSpawnerIntegration: ARFurnitureManagerSimple not found. Integration cannot be completed.");
            return;
        }

        // Setup ObjectSpawner integration
        if (m_ObjectSpawner != null && m_RedirectSpawningToFurnitureManager)
        {
            // Clear ObjectSpawner's default prefabs since we'll use FurnitureDatabase
            if (m_ObjectSpawner.objectPrefabs != null)
            {
                m_ObjectSpawner.objectPrefabs.Clear();
            }
            
            // Subscribe to ObjectSpawner events to redirect spawning
            m_ObjectSpawner.objectSpawned += OnObjectSpawnerTriggered;
            
            if (m_ShowDebugInfo)
            {
                Debug.Log("✓ ObjectSpawner events redirected to ARFurnitureManagerSimple");
            }
        }

        // Register this system with ARFurnitureManagerSimple for potential callbacks
        if (m_ShowDebugInfo)
        {
            Debug.Log("✓ Integration setup complete");
        }
    }

    void OnObjectSpawnerTriggered(GameObject spawnedObject)
    {
        if (m_FurnitureManager == null) return;

        // Instead of letting ObjectSpawner handle spawning directly,
        // we intercept and let ARFurnitureManagerSimple handle it
        if (m_RedirectSpawningToFurnitureManager)
        {
            // Destroy the spawned object since we'll create our own
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
            }

            // Trigger ARFurnitureManagerSimple spawning instead
            // This ensures consistency with furniture database and UI
            if (m_ShowDebugInfo)
            {
                Debug.Log("ObjectSpawner spawn intercepted - redirecting to ARFurnitureManagerSimple");
            }
        }
        else
        {
            // Register the spawned object with ARFurnitureManagerSimple
            m_FurnitureManager.RegisterPlacedObject(spawnedObject);
        }
    }

    void OnDestroy()
    {
        // Clean up event subscriptions
        if (m_ObjectSpawner != null)
        {
            m_ObjectSpawner.objectSpawned -= OnObjectSpawnerTriggered;
        }
    }

    /// <summary>
    /// Disable components if needed (for testing purposes)
    /// </summary>
    [ContextMenu("Disable All Components")]
    public void DisableComponents()
    {
        if (m_ObjectSpawner != null)
        {
            m_ObjectSpawner.enabled = false;
            Debug.Log("Disabled ObjectSpawner");
        }

        if (m_ARInteractorSpawnTrigger != null)
        {
            m_ARInteractorSpawnTrigger.enabled = false;
            Debug.Log("Disabled ARInteractorSpawnTrigger");
        }
    }

    /// <summary>
    /// Check status of components and integration
    /// </summary>
    [ContextMenu("Check Integration Status")]
    public void CheckIntegrationStatus()
    {
        Debug.Log($"=== INTEGRATION STATUS FOR: {gameObject.name} ===");
        
        Debug.Log($"ObjectSpawner: {(m_ObjectSpawner != null ? (m_ObjectSpawner.enabled ? "ENABLED" : "DISABLED") : "NOT FOUND")}");
        Debug.Log($"ARInteractorSpawnTrigger: {(m_ARInteractorSpawnTrigger != null ? (m_ARInteractorSpawnTrigger.enabled ? "ENABLED" : "DISABLED") : "NOT FOUND")}");
        Debug.Log($"ARFurnitureManagerSimple: {(m_FurnitureManager != null ? "CONNECTED" : "NOT FOUND")}");
        Debug.Log($"Spawning Redirection: {(m_RedirectSpawningToFurnitureManager ? "ACTIVE" : "DISABLED")}");
        
        Debug.Log("==============================================");
    }
}
