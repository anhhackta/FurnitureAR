using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// Component that keeps furniture objects attached to trackable surfaces
/// Prevents objects from floating and keeps them on detected AR planes
/// </summary>
public class FurnitureSurfaceTracker : MonoBehaviour
{
    [Header("Surface Tracking Settings")]
    [Tooltip("AR Raycast Manager for surface detection")]
    public ARRaycastManager arRaycastManager;
    
    [Tooltip("How often to check for surface (in seconds)")]
    public float trackingInterval = 0.1f;
    
    [Tooltip("Maximum distance to search for surface below object")]
    public float maxTrackingDistance = 2f;
    
    [Tooltip("Offset from surface to place object")]
    public float surfaceOffset = 0.01f;
    
    [Tooltip("Smooth transition to new position")]
    public bool smoothTransition = true;
    
    [Tooltip("Speed of smooth transition")]
    public float transitionSpeed = 10f;
    
    // Private variables
    private Camera arCamera;
    private float lastTrackingTime = 0f;
    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private Vector3 targetPosition;
    private bool hasValidTarget = false;
    
    void Start()
    {
        // Get AR Camera
        arCamera = Camera.main;
        if (arCamera == null)
        {
            arCamera = FindObjectOfType<Camera>();
        }
        
        // Auto-assign ARRaycastManager if not set
        if (arRaycastManager == null)
        {
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
        }
        
        if (arRaycastManager == null)
        {
            Debug.LogWarning($"FurnitureSurfaceTracker on {gameObject.name}: No ARRaycastManager found. Surface tracking disabled.");
            enabled = false;
            return;
        }
        
        // Initialize target position
        targetPosition = transform.position;
        hasValidTarget = true;
        
        Debug.Log($"FurnitureSurfaceTracker initialized for {gameObject.name}");
    }
    
    void Update()
    {
        // Check if enough time has passed since last tracking
        if (Time.time - lastTrackingTime < trackingInterval)
            return;
            
        lastTrackingTime = Time.time;
        
        // Track surface under object
        TrackSurface();
        
        // Apply position if smooth transition enabled
        if (smoothTransition && hasValidTarget)
        {
            // Only smooth Y position, keep X,Z for free movement
            Vector3 currentPos = transform.position;
            Vector3 newPos = new Vector3(currentPos.x, targetPosition.y, currentPos.z);
            transform.position = Vector3.Lerp(transform.position, newPos, transitionSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Track surface under the object and update target position
    /// </summary>
    void TrackSurface()
    {
        if (arRaycastManager == null || arCamera == null)
            return;
            
        // Get screen position of object
        Vector3 screenPosition = arCamera.WorldToScreenPoint(transform.position);
        
        // Check if object is visible on screen
        if (screenPosition.z < 0 || screenPosition.x < 0 || screenPosition.x > Screen.width || 
            screenPosition.y < 0 || screenPosition.y > Screen.height)
        {
            // Object is off-screen, use raycast from object position downward
            TrackSurfaceFromWorldPosition();
            return;
        }
        
        // Raycast from object's screen position to find surface
        if (arRaycastManager.Raycast(screenPosition, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            // Found a trackable surface
            ARRaycastHit hit = raycastHits[0];
            Vector3 newPosition = hit.pose.position + Vector3.up * surfaceOffset;
            
            // Check if new position is reasonable (not too far from current position)
            float distance = Vector3.Distance(transform.position, newPosition);
            if (distance < maxTrackingDistance)
            {
                // Only update Y position to stick to surface, keep X,Z for movement
                targetPosition = new Vector3(transform.position.x, newPosition.y, transform.position.z);
                hasValidTarget = true;
                
                if (!smoothTransition)
                {
                    Vector3 currentPos = transform.position;
                    transform.position = new Vector3(currentPos.x, targetPosition.y, currentPos.z);
                }
            }
        }
        else
        {
            // No surface found at screen position, try world position raycast
            TrackSurfaceFromWorldPosition();
        }
    }
    
    /// <summary>
    /// Fallback method to track surface using world position raycast
    /// </summary>
    void TrackSurfaceFromWorldPosition()
    {
        // Simple raycast downward from object position to find any surface
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        
        RaycastHit hit;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, maxTrackingDistance))
        {
            // Accept any surface that's not the object itself
            if (hit.collider.gameObject != gameObject)
            {
                Vector3 newPosition = hit.point + Vector3.up * surfaceOffset;
                
                // Only update Y position to stick to surface, keep X,Z for movement
                targetPosition = new Vector3(transform.position.x, newPosition.y, transform.position.z);
                hasValidTarget = true;
                
                if (!smoothTransition)
                {
                    Vector3 currentPos = transform.position;
                    transform.position = new Vector3(currentPos.x, targetPosition.y, currentPos.z);
                }
            }
        }
    }
    
    /// <summary>
    /// Force update surface tracking (useful when object is moved)
    /// </summary>
    public void ForceUpdateSurfaceTracking()
    {
        lastTrackingTime = 0f; // Force immediate update
        TrackSurface();
    }
    
    /// <summary>
    /// Enable/disable surface tracking
    /// </summary>
    public void SetTrackingEnabled(bool enabled)
    {
        this.enabled = enabled;
        Debug.Log($"Surface tracking {(enabled ? "enabled" : "disabled")} for {gameObject.name}");
    }
    
    /// <summary>
    /// Set new tracking interval
    /// </summary>
    public void SetTrackingInterval(float interval)
    {
        trackingInterval = Mathf.Max(0.05f, interval); // Minimum 50ms interval
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw raycast debug visualization
        Gizmos.color = Color.yellow;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        Vector3 rayEnd = transform.position - Vector3.up * maxTrackingDistance;
        Gizmos.DrawLine(rayStart, rayEnd);
        
        // Draw target position if available
        if (hasValidTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, 0.1f);
        }
    }
}
