using UnityEngine;

/// <summary>
/// Simple furniture interactor that keeps objects in place during interactions
/// Prevents objects from being "pulled" to camera when selected
/// </summary>
public class ManualFurnitureInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Keep object locked at original position")]
    public bool lockPosition = true;
    
    // Private variables
    private Vector3 originalPosition;
    private bool isSelected = false;
    
    void Start()
    {
        // Store original position
        originalPosition = transform.position;
        Debug.Log($"ManualFurnitureInteractor: Locked position {originalPosition} for {gameObject.name}");
    }
    
    void Update()
    {
        if (lockPosition && isSelected)
        {
            // Force object to stay at original position
            transform.position = originalPosition;
        }
    }
    
    /// <summary>
    /// Enable position locking (called when object is selected)
    /// </summary>
    public void StartPositionLock()
    {
        isSelected = true;
        originalPosition = transform.position;
        Debug.Log($"Position lock started for {gameObject.name} at {originalPosition}");
    }
    
    /// <summary>
    /// Disable position locking (called when object is deselected)
    /// </summary>
    public void StopPositionLock()
    {
        isSelected = false;
        Debug.Log($"Position lock stopped for {gameObject.name}");
    }
    
    /// <summary>
    /// Update original position
    /// </summary>
    public void UpdateOriginalPosition()
    {
        originalPosition = transform.position;
        Debug.Log($"Updated original position to {originalPosition} for {gameObject.name}");
    }
}
