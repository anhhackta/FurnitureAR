using UnityEngine;

/// <summary>
/// Component that constrains object movement to X,Z plane only
/// Prevents Y-axis movement while allowing horizontal movement
/// </summary>
public class XZPlaneMovementConstraint : MonoBehaviour
{
    [Header("Movement Constraint Settings")]
    [Tooltip("The Y position to maintain (set automatically on start)")]
    public float fixedYPosition = 0f;
    
    [Tooltip("Allow automatic Y position adjustment")]
    public bool autoAdjustY = true;
    
    [Tooltip("Smooth Y position correction")]
    public bool smoothCorrection = true;
    
    [Tooltip("Speed of Y position correction")]
    public float correctionSpeed = 10f;
    
    // Private variables
    private bool isInitialized = false;
    private Vector3 lastValidPosition;
    
    void Start()
    {
        InitializeConstraint();
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Constrain movement to X,Z plane
        ConstrainToXZPlane();
    }
    
    /// <summary>
    /// Initialize the constraint with current position
    /// </summary>
    void InitializeConstraint()
    {
        fixedYPosition = transform.position.y;
        lastValidPosition = transform.position;
        isInitialized = true;
        
        Debug.Log($"XZPlaneMovementConstraint: Fixed Y position at {fixedYPosition} for {gameObject.name}");
    }
    
    /// <summary>
    /// Constrain object movement to X,Z plane only
    /// </summary>
    void ConstrainToXZPlane()
    {
        Vector3 currentPos = transform.position;
        
        // Check if Y position has changed
        if (Mathf.Abs(currentPos.y - fixedYPosition) > 0.01f)
        {
            // Correct Y position while keeping X,Z movement
            Vector3 correctedPos = new Vector3(currentPos.x, fixedYPosition, currentPos.z);
            
            if (smoothCorrection)
            {
                // Smooth correction
                transform.position = Vector3.Lerp(currentPos, correctedPos, correctionSpeed * Time.deltaTime);
            }
            else
            {
                // Immediate correction
                transform.position = correctedPos;
            }
            
            // Debug log for tracking Y corrections
            if (Vector3.Distance(currentPos, lastValidPosition) > 0.1f)
            {
                Debug.Log($"XZPlaneMovementConstraint: Corrected Y position for {gameObject.name} from {currentPos.y:F2} to {fixedYPosition:F2}");
            }
        }
        
        lastValidPosition = transform.position;
    }
    
    /// <summary>
    /// Update the fixed Y position to current position
    /// </summary>
    public void UpdateFixedYPosition()
    {
        fixedYPosition = transform.position.y;
        Debug.Log($"XZPlaneMovementConstraint: Updated fixed Y position to {fixedYPosition} for {gameObject.name}");
    }
    
    /// <summary>
    /// Set a specific Y position
    /// </summary>
    public void SetFixedYPosition(float yPos)
    {
        fixedYPosition = yPos;
        
        // Immediately apply the new Y position
        Vector3 currentPos = transform.position;
        transform.position = new Vector3(currentPos.x, fixedYPosition, currentPos.z);
        
        Debug.Log($"XZPlaneMovementConstraint: Set fixed Y position to {fixedYPosition} for {gameObject.name}");
    }
    
    /// <summary>
    /// Enable/disable the constraint
    /// </summary>
    public void SetConstraintEnabled(bool enabled)
    {
        this.enabled = enabled;
        Debug.Log($"XZPlaneMovementConstraint: {(enabled ? "Enabled" : "Disabled")} for {gameObject.name}");
    }
    
    /// <summary>
    /// Reset to original position
    /// </summary>
    public void ResetToOriginalPosition()
    {
        if (isInitialized)
        {
            Vector3 currentPos = transform.position;
            transform.position = new Vector3(currentPos.x, fixedYPosition, currentPos.z);
            Debug.Log($"XZPlaneMovementConstraint: Reset Y position for {gameObject.name}");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!isInitialized) return;
        
        // Draw a plane to visualize the constraint
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(transform.position.x, fixedYPosition, transform.position.z);
        Gizmos.DrawWireCube(center, new Vector3(2f, 0.01f, 2f));
        
        // Draw current position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        
        // Draw fixed Y line
        Gizmos.color = Color.green;
        Vector3 fixedPos = new Vector3(transform.position.x, fixedYPosition, transform.position.z);
        Gizmos.DrawWireSphere(fixedPos, 0.05f);
    }
}
