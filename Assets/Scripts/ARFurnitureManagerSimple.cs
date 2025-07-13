using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;
using System.Collections.Generic;

/// <summary>
/// Simplified AR Furniture Manager that manages furniture placement, selection, and UI interactions.
/// This version integrates with XR Interaction Toolkit for advanced object manipulation:
/// - Uses Screen Space Ray Interactor for object selection
/// - Uses XR Grab Interactable for object manipulation (rotate, scale, move)
/// - Uses AR Foundation for plane detection and spawning
/// - Handles UI management and furniture database integration
/// </summary>
public class ARFurnitureManagerSimple : MonoBehaviour
{
    [Header("Database & Core")]
    [SerializeField]
    [Tooltip("Database containing all furniture items")]
    FurnitureDatabase m_FurnitureDatabase;

    [SerializeField]
    [Tooltip("Transform to parent spawned objects")]
    Transform m_GroundPlaneStage;

    [Header("UI Components - Your Canvas Structure")]
    [SerializeField]
    [Tooltip("PanelVatPham - Main furniture panel")]
    GameObject m_PanelVatPham;

    [SerializeField]
    [Tooltip("MenuVatPham - Container for furniture menu")]
    GameObject m_MenuVatPham;

    [SerializeField]
    [Tooltip("Content - Parent for furniture item buttons")]
    GameObject m_ContentParent;

    [SerializeField]
    [Tooltip("PanelThongSo - Info panel")]
    GameObject m_PanelThongSo;

    [SerializeField]
    [Tooltip("TextDoVat - Text component to display selected item name")]
    Text m_TextDoVat;

    [Header("UI Buttons - Your Canvas Structure")]
    [SerializeField]
    [Tooltip("ButtonDel - Delete button in PanelThongSo")]
    Button m_ButtonDel;

    [SerializeField]
    [Tooltip("ButtonClose - Close button for PanelThongSo")]
    Button m_ButtonClose;

    [SerializeField]
    [Tooltip("ButtonUI - Toggle UI button")]
    Button m_ButtonUI;

    [Header("Prefabs & Components")]
    [SerializeField]
    [Tooltip("Prefab for furniture selection buttons")]
    GameObject m_FurnitureButtonPrefab;

    [SerializeField]
    [Tooltip("Color picker component")]
    FlexibleColorPicker m_ColorPicker;

    [Header("AR Foundation Components")]
    [SerializeField]
    [Tooltip("AR Raycast Manager for detecting AR planes")]
    ARRaycastManager m_ARRaycastManager;
    
    [SerializeField]
    [Tooltip("Reference to ARInteractorSpawnTrigger for object selection")]
    ARInteractorSpawnTrigger m_ARInteractorSpawnTrigger;

    [Header("XR Interaction Components")]
    [SerializeField]
    [Tooltip("Screen Space Ray Interactor for selection and manipulation")]
    XRRayInteractor m_ScreenSpaceRayInteractor;

    [SerializeField]
    [Tooltip("Screen Space Select Input component")]
    ScreenSpaceSelectInput m_ScreenSpaceSelectInput;

    [SerializeField]
    [Tooltip("Screen Space Pinch Scale Input component")]
    ScreenSpacePinchScaleInput m_ScreenSpacePinchScaleInput;

    // Private variables
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    private List<GameObject> m_PlacedObjects = new List<GameObject>();
    private GameObject m_SelectedObject;
    private bool m_IsUIVisible = true;
    private int m_SelectedFurnitureIndex = -1;

    // Properties
    public FurnitureDatabase furnitureDatabase
    {
        get => m_FurnitureDatabase;
        set => m_FurnitureDatabase = value;
    }

    public int selectedFurnitureIndex
    {
        get => m_SelectedFurnitureIndex;
        set => m_SelectedFurnitureIndex = value;
    }

    public Transform groundPlaneStage
    {
        get => m_GroundPlaneStage;
        set => m_GroundPlaneStage = value;
    }

    public GameObject panelVatPham
    {
        get => m_PanelVatPham;
        set => m_PanelVatPham = value;
    }

    public GameObject contentParent
    {
        get => m_ContentParent;
        set => m_ContentParent = value;
    }

    public GameObject panelThongSo
    {
        get => m_PanelThongSo;
        set => m_PanelThongSo = value;
    }

    public FlexibleColorPicker colorPicker
    {
        get => m_ColorPicker;
        set => m_ColorPicker = value;
    }

    void OnEnable()
    {
        // Setup button listeners
        SetupButtonListeners();
        
        // Setup Color picker listener with validation
        SetupColorPickerListener();
    }

    void OnDisable()
    {
        // Remove color picker listener
        RemoveColorPickerListener();
            
        // Remove XR selection event listeners
        if (m_ScreenSpaceRayInteractor != null)
        {
            m_ScreenSpaceRayInteractor.selectEntered.RemoveListener(OnXRObjectSelected);
            m_ScreenSpaceRayInteractor.selectExited.RemoveListener(OnXRObjectDeselected);
        }
    }

    /// <summary>
    /// Setup FlexibleColorPicker listener with validation
    /// </summary>
    void SetupColorPickerListener()
    {
        if (m_ColorPicker != null)
        {
            // Remove existing listener first to avoid duplicates
            m_ColorPicker.onColorChange.RemoveListener(OnChangeColor);
            
            // Add listener
            m_ColorPicker.onColorChange.AddListener(OnChangeColor);
            
            Debug.Log($"FlexibleColorPicker listener setup successful: {m_ColorPicker.name}");
        }
        else
        {
            Debug.LogWarning("FlexibleColorPicker is null during OnEnable - will try auto-assign in Start");
        }
    }

    /// <summary>
    /// Remove FlexibleColorPicker listener
    /// </summary>
    void RemoveColorPickerListener()
    {
        if (m_ColorPicker != null)
        {
            m_ColorPicker.onColorChange.RemoveListener(OnChangeColor);
            Debug.Log("FlexibleColorPicker listener removed");
        }
    }

    void Start()
    {
        // Validate components
        if (m_FurnitureDatabase == null || m_FurnitureDatabase.furnitureItems.Length == 0)
        {
            Debug.LogError("FurnitureDatabase is not assigned or empty!", this);
            return;
        }
        
        // Auto-assign UI components from your Canvas structure
        AutoAssignUIComponents();
        
        // Auto-assign AR Foundation components
        AutoAssignARComponents();
        
        // Setup ColorPicker after auto-assignment
        if (m_ColorPicker == null)
        {
            Debug.LogError("FlexibleColorPicker is not assigned and auto-assignment failed!", this);
            return;
        }
        else
        {
            // Re-setup color picker listener after auto-assignment
            SetupColorPickerListener();
            Debug.Log($"FlexibleColorPicker assigned successfully: {m_ColorPicker.name}");
        }
        
        // Validate critical UI components
        GameObject contentContainer = GetContentContainer();
        if (contentContainer == null)
        {
            Debug.LogError("Content container not found! Assign either contentParent!", this);
            return;
        }

        // Initialize UI
        PopulateFurnitureList();
        
        // Setup info panel
        GameObject infoContainer = GetInfoPanel();
        if (infoContainer != null)
            infoContainer.SetActive(false);
    }

    /// <summary>
    /// Auto-assign UI components if not manually assigned
    /// </summary>
    void AutoAssignUIComponents()
    {
        // Try to find PanelVatPham if not assigned
        if (m_PanelVatPham == null)
            m_PanelVatPham = GameObject.Find("PanelVatPham");
            
        if (m_PanelVatPham != null)
        {
            // Find MenuVatPham
            if (m_MenuVatPham == null)
            {
                Transform menuTransform = FindChildByName(m_PanelVatPham.transform, "MenuVatPham");
                if (menuTransform != null)
                    m_MenuVatPham = menuTransform.gameObject;
            }
                
            // Find Content
            if (m_ContentParent == null && m_MenuVatPham != null)
            {
                Transform contentTransform = FindChildByName(m_MenuVatPham.transform, "Content");
                if (contentTransform != null)
                    m_ContentParent = contentTransform.gameObject;
            }
                
            // Find PanelThongSo
            if (m_PanelThongSo == null)
            {
                Transform panelTransform = FindChildByName(m_PanelVatPham.transform, "PanelThongSo");
                if (panelTransform != null)
                    m_PanelThongSo = panelTransform.gameObject;
            }
                
            if (m_PanelThongSo != null)
            {
                // Find Panel inside PanelThongSo
                Transform panelTransform = FindChildByName(m_PanelThongSo.transform, "Panel");
                if (panelTransform != null)
                {
                    // Find MenuThongSo
                    Transform menuThongSoTransform = FindChildByName(panelTransform, "MenuThongSo");
                    if (menuThongSoTransform != null)
                    {
                        // Find TextDoVat
                        if (m_TextDoVat == null)
                        {
                            Transform textTransform = FindChildByName(menuThongSoTransform, "TextDoVat");
                            if (textTransform != null)
                                m_TextDoVat = textTransform.GetComponent<Text>();
                        }
                        
                        // Find ButtonDel
                        if (m_ButtonDel == null)
                        {
                            Transform buttonTransform = FindChildByName(menuThongSoTransform, "ButtonDel");
                            if (buttonTransform != null)
                                m_ButtonDel = buttonTransform.GetComponent<Button>();
                        }
                        
                        // Find ButtonClose
                        if (m_ButtonClose == null)
                        {
                            Transform buttonTransform = FindChildByName(menuThongSoTransform, "ButtonClose");
                            if (buttonTransform != null)
                                m_ButtonClose = buttonTransform.GetComponent<Button>();
                        }
                    }
                    
                    // Find FlexibleColorPicker
                    if (m_ColorPicker == null)
                    {
                        Transform colorPickerTransform = FindChildByName(panelTransform, "FlexibleColorPicker");
                        if (colorPickerTransform != null)
                            m_ColorPicker = colorPickerTransform.GetComponent<FlexibleColorPicker>();
                    }
                }
            }
            
            // Find ButtonUI
            if (m_ButtonUI == null)
            {
                Transform buttonTransform = FindChildByName(m_PanelVatPham.transform, "ButtonUI");
                if (buttonTransform != null)
                    m_ButtonUI = buttonTransform.GetComponent<Button>();
            }
        }
        
        // Setup button listeners
        SetupButtonListeners();
    }

    /// <summary>
    /// Setup button listeners for your UI
    /// </summary>
    void SetupButtonListeners()
    {
        // ButtonDel
        if (m_ButtonDel != null)
            m_ButtonDel.onClick.AddListener(DeleteSelectedObject);
            
        // ButtonClose (close info panel)
        if (m_ButtonClose != null)
            m_ButtonClose.onClick.AddListener(CloseInfoPanel);
            
        // ButtonUI (toggle UI)
        if (m_ButtonUI != null)
            m_ButtonUI.onClick.AddListener(ToggleUI);
    }

    /// <summary>
    /// Helper method to find child by name recursively
    /// </summary>
    Transform FindChildByName(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;
            
        foreach (Transform child in parent)
        {
            Transform result = FindChildByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    void Update()
    {
        // Only handle spawning input - XR Interaction Toolkit handles selection/manipulation
        HandleSpawnInput();
        
        #if UNITY_EDITOR
        // Add PC testing controls for convenience
        HandlePCTestingControls();
        #endif
    }

    #region Input Handling
    /// <summary>
    /// Handle input for spawning furniture only - XR Interaction Toolkit handles selection
    /// </summary>
    void HandleSpawnInput()
    {
        // Only handle input when we have a furniture type selected
        if (m_SelectedFurnitureIndex < 0) 
        {
            return; // Don't log repeatedly
        }

        // Handle touch input for mobile devices
        if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                ProcessTouchInput(touch.position);
            }
        }
        // Handle mouse input for editor testing
        else if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            ProcessTouchInput(Input.mousePosition);
        }
    }

    #if UNITY_EDITOR
    /// <summary>
    /// PC testing controls for Unity Editor
    /// </summary>
    void HandlePCTestingControls()
    {
        if (m_SelectedObject == null) return;

        // E/R keys for rotation
        if (Input.GetKey(KeyCode.E))
        {
            m_SelectedObject.transform.Rotate(0f, 50f * Time.deltaTime, 0f, Space.World);
        }
        else if (Input.GetKey(KeyCode.R))
        {
            m_SelectedObject.transform.Rotate(0f, -50f * Time.deltaTime, 0f, Space.World);
        }

        // Mouse wheel for scaling
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            Vector3 currentScale = m_SelectedObject.transform.localScale;
            Vector3 newScale = currentScale + Vector3.one * scrollInput * 0.1f;
            
            // Clamp scale to reasonable values
            newScale = Vector3.Max(newScale, Vector3.one * 0.1f);
            newScale = Vector3.Min(newScale, Vector3.one * 5f);
            
            m_SelectedObject.transform.localScale = newScale;
        }
    }
    #endif

    /// <summary>
    /// Legacy method - now only used for manual selection fallback
    /// XR Interaction Toolkit handles most selection automatically
    /// </summary>
    void HandleMouseInput()
    {
        // This method is kept for fallback purposes
        // XR Interaction Toolkit should handle selection automatically
        if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                ProcessTouchInput(touch.position);
            }
        }
        // Handle mouse input for editor testing
        else if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            ProcessTouchInput(Input.mousePosition);
        }
    }

    /// <summary>
    /// Process touch/click input - prioritize selection over spawning
    /// </summary>
    void ProcessTouchInput(Vector2 screenPosition)
    {
        // First priority: Try to select existing furniture objects
        if (TrySelectExistingFurniture(screenPosition))
        {
            Debug.Log("Selected existing furniture object");
            return;
        }

        // Second priority: If no existing object hit, try to spawn new furniture
        if (m_SelectedFurnitureIndex >= 0)
        {
            Debug.Log($"No existing furniture hit - trying to spawn {m_FurnitureDatabase.furnitureItems[m_SelectedFurnitureIndex].name}");
            TrySpawnOnARPlaneWithCollisionCheck(screenPosition);
        }
    }

    /// <summary>
    /// Try to select existing furniture object with precise collider-based detection
    /// </summary>
    bool TrySelectExistingFurniture(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        // Use precise raycast with furniture colliders only
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.transform.CompareTag("Furniture"))
            {
                Debug.Log($"Hit existing furniture: {hit.transform.name} at precise collider point");
                SelectPlacedObject(hit.transform.gameObject);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Try to spawn on AR plane with collision detection
    /// </summary>
    bool TrySpawnOnARPlaneWithCollisionCheck(Vector2 screenPosition)
    {
        if (m_ARRaycastManager == null)
        {
            Debug.LogWarning("ARRaycastManager not assigned. Using fallback method.");
            return TrySpawnWithFallbackAndCollisionCheck(screenPosition);
        }

        if (m_ARRaycastManager.Raycast(screenPosition, m_Hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = m_Hits[0].pose;
            
            // Check for collision at spawn position
            if (IsPositionClearForSpawning(hitPose.position))
            {
                SpawnFurniture(hitPose.position);
                return true;
            }
            else
            {
                Debug.Log("Cannot spawn here - position is occupied by another object");
                return false;
            }
        }
        
        Debug.Log("No AR plane found at touch position. Make sure to scan the environment first.");
        return false;
    }

    /// <summary>
    /// Check if position is clear for spawning using precise collider bounds
    /// </summary>
    bool IsPositionClearForSpawning(Vector3 position)
    {
        if (m_SelectedFurnitureIndex < 0 || m_SelectedFurnitureIndex >= m_FurnitureDatabase.furnitureItems.Length)
            return false;

        // Get the prefab to check its collider bounds
        GameObject prefab = m_FurnitureDatabase.furnitureItems[m_SelectedFurnitureIndex].prefab;
        
        // Get collider bounds for precise checking
        Collider prefabCollider = prefab.GetComponent<Collider>();
        if (prefabCollider == null)
            prefabCollider = prefab.GetComponentInChildren<Collider>();
            
        if (prefabCollider != null)
        {
            // Use actual collider bounds for precise collision detection
            Vector3 colliderSize = prefabCollider.bounds.size;
            Vector3 colliderCenter = position + prefabCollider.bounds.center - prefabCollider.transform.position;
            
            // Create bounds at spawn position
            Bounds spawnBounds = new Bounds(colliderCenter, colliderSize);
            
            // Check for overlapping furniture colliders
            foreach (GameObject placedObj in m_PlacedObjects)
            {
                if (placedObj == null) continue;
                
                Collider placedCollider = placedObj.GetComponent<Collider>();
                if (placedCollider == null)
                    placedCollider = placedObj.GetComponentInChildren<Collider>();
                    
                if (placedCollider != null)
                {
                    // Check if bounds overlap
                    if (spawnBounds.Intersects(placedCollider.bounds))
                    {
                        Debug.Log($"Spawn position blocked by: {placedObj.name} - collider bounds overlap");
                        return false;
                    }
                }
            }
            
            Debug.Log($"Position clear for spawning (checked collider bounds: {colliderSize})");
            return true;
        }
        else
        {
            // Fallback to small radius if no collider found
            float checkRadius = 0.3f; // Smaller default radius
            Collider[] overlapping = Physics.OverlapSphere(position, checkRadius);
            
            foreach (Collider col in overlapping)
            {
                if (col.CompareTag("Furniture"))
                {
                    Debug.Log($"Spawn position blocked by: {col.name} (fallback radius check)");
                    return false;
                }
            }
            
            Debug.Log($"Position clear for spawning (fallback radius: {checkRadius:F2}m)");
            return true;
        }
    }

    bool TrySelectFurnitureObject(Vector2 screenPosition)
    {
        // This method is now replaced by TrySelectExistingFurniture
        return TrySelectExistingFurniture(screenPosition);
    }

    bool TrySpawnOnARPlane(Vector2 screenPosition)
    {
        if (m_ARRaycastManager == null)
        {
            Debug.LogWarning("ARRaycastManager not assigned. Using fallback method.");
            return TrySpawnWithFallbackAndCollisionCheck(screenPosition);
        }

        if (m_ARRaycastManager.Raycast(screenPosition, m_Hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = m_Hits[0].pose;
            
            // Check for collision at spawn position
            if (IsPositionClearForSpawning(hitPose.position))
            {
                SpawnFurniture(hitPose.position);
                return true;
            }
            else
            {
                Debug.Log("Cannot spawn here - position is occupied by another object");
                return false;
            }
        }
        
        Debug.Log("No AR plane found at touch position. Make sure to scan the environment first.");
        return false;
    }

    /// <summary>
    /// Try to spawn with fallback method and collision check
    /// </summary>
    bool TrySpawnWithFallbackAndCollisionCheck(Vector2 screenPosition)
    {
        // Fallback method when ARRaycastManager is not available
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Spawn on any surface that's not UI or furniture
            if (!hit.transform.CompareTag("Furniture"))
            {
                // Check for collision at spawn position
                if (IsPositionClearForSpawning(hit.point))
                {
                    SpawnFurniture(hit.point);
                    return true;
                }
                else
                {
                    Debug.Log("Cannot spawn here - position is occupied by another object");
                    return false;
                }
            }
        }
        
        Debug.Log("No suitable surface found for spawning.");
        return false;
    }
    #endregion

    #region Furniture Management
    void SpawnFurniture(Vector3 position)
    {
        if (m_SelectedFurnitureIndex < 0 || m_SelectedFurnitureIndex >= m_FurnitureDatabase.furnitureItems.Length)
            return;
            
        Transform parent = m_GroundPlaneStage != null ? m_GroundPlaneStage : transform;
        GameObject newObject = Instantiate(m_FurnitureDatabase.furnitureItems[m_SelectedFurnitureIndex].prefab, position, Quaternion.identity, parent);
        newObject.tag = "Furniture";
        
        // Apply material from database if available
        ApplyDatabaseMaterial(newObject, m_SelectedFurnitureIndex);
        
        // Ensure proper collider setup for precise interaction
        SetupPreciseCollider(newObject);
        
        // Add XR Grab Interactable for XR Interaction Toolkit integration
        SetupXRInteractable(newObject);
        
        m_PlacedObjects.Add(newObject);
        SelectPlacedObject(newObject);
        
        Debug.Log($"Spawned furniture with database material and XR integration: {newObject.name}");
    }

    /// <summary>
    /// Public method for external spawning (called by ARFurnitureSpawnTrigger)
    /// </summary>
    public void SpawnFurnitureAtPosition(Vector3 position)
    {
        SpawnFurniture(position);
    }

    /// <summary>
    /// Apply material from database to spawned furniture object
    /// </summary>
    void ApplyDatabaseMaterial(GameObject furnitureObject, int furnitureIndex)
    {
        if (furnitureIndex < 0 || furnitureIndex >= m_FurnitureDatabase.furnitureItems.Length)
            return;
            
        var furnitureItem = m_FurnitureDatabase.furnitureItems[furnitureIndex];
        Material[] availableMaterials = furnitureItem.availableMaterials;
        
        if (availableMaterials.Length > 0 && availableMaterials[0] != null)
        {
            // Find Renderer - check object itself first, then children
            Renderer renderer = furnitureObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                renderer = furnitureObject.GetComponentInChildren<Renderer>();
            }
            
            if (renderer != null)
            {
                // Apply first available material from database
                renderer.material = availableMaterials[0];
                Debug.Log($"Applied database material '{availableMaterials[0].name}' to {furnitureObject.name}");
            }
            else
            {
                Debug.LogWarning($"No Renderer found on {furnitureObject.name} to apply material");
            }
        }
        else
        {
            Debug.LogWarning($"No available materials in database for {furnitureItem.name}");
        }
    }

    /// <summary>
    /// Setup XR Grab Interactable for furniture objects
    /// </summary>
    void SetupXRInteractable(GameObject furnitureObject)
    {
        // Add XR Grab Interactable if not present
        XRGrabInteractable grabInteractable = furnitureObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = furnitureObject.AddComponent<XRGrabInteractable>();
            
            // CRITICAL: Configure to allow X,Z movement but prevent Y movement (no attachment)
            grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
            
            // IMPORTANT: Disable attachment to prevent object being pulled to interactor
            grabInteractable.attachTransform = null; // No attach point
            grabInteractable.useDynamicAttach = false; // Don't create dynamic attach
            
            // Configure tracking - allow X,Z movement but NO rotation changes
            grabInteractable.trackPosition = true;   // ENABLE position tracking for X,Z movement
            grabInteractable.trackRotation = false;  // DISABLE rotation tracking completely
            grabInteractable.trackScale = false;     // DISABLE scaling to prevent accidental changes
            
            // Configure selection
            grabInteractable.selectMode = InteractableSelectMode.Single;
            
            // Keep object at original position
            grabInteractable.retainTransformParent = true;
            
            // IMPORTANT: Use manual movement instead of automatic attachment
            grabInteractable.throwOnDetach = false;
            
            // Add hover and select events for visual feedback
            grabInteractable.hoverEntered.AddListener(OnFurnitureHoverEntered);
            grabInteractable.hoverExited.AddListener(OnFurnitureHoverExited);
            grabInteractable.selectEntered.AddListener(OnFurnitureSelectEntered);
            grabInteractable.selectExited.AddListener(OnFurnitureSelectExited);
            
            Debug.Log($"Added XRGrabInteractable to {furnitureObject.name} with X,Z movement only (no Y attachment)");
        }

        // Handle Rigidbody - Allow X,Z movement but freeze Y
        Rigidbody rb = furnitureObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Make kinematic for controlled movement
            rb.isKinematic = true;
            rb.useGravity = false;
            
            // IMPORTANT: Freeze ALL rotations - only allow X,Z position movement
            rb.constraints = RigidbodyConstraints.FreezePositionY | 
                           RigidbodyConstraints.FreezeRotationX | 
                           RigidbodyConstraints.FreezeRotationY |
                           RigidbodyConstraints.FreezeRotationZ;
            
            Debug.Log($"Set Rigidbody constraints: Allow X,Z movement only, freeze Y position and ALL rotations for {furnitureObject.name}");
        }
        else
        {
            // Add kinematic Rigidbody for controlled movement
            rb = furnitureObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            
            // Allow X,Z movement, freeze Y position and ALL rotations
            rb.constraints = RigidbodyConstraints.FreezePositionY | 
                           RigidbodyConstraints.FreezeRotationX | 
                           RigidbodyConstraints.FreezeRotationY |
                           RigidbodyConstraints.FreezeRotationZ;
            
            Debug.Log($"Added kinematic Rigidbody with X,Z movement only (no rotation) to {furnitureObject.name}");
        }
        
        // Add movement constraint handler for X,Z plane only
        // XZPlaneMovementConstraint constraint = furnitureObject.GetComponent<XZPlaneMovementConstraint>();
        // if (constraint == null)
        // {
        //     constraint = furnitureObject.AddComponent<XZPlaneMovementConstraint>();
        // }
        
        // Note: Object will only move in X,Z plane with NO rotation changes
        Debug.Log($"Configured {furnitureObject.name} for X,Z plane movement only (no Y movement, no rotation)");
    }

    /// <summary>
    /// Register a placed object from external spawner (like Object Spawner)
    /// </summary>
    public void RegisterPlacedObject(GameObject placedObject)
    {
        if (placedObject != null && !m_PlacedObjects.Contains(placedObject))
        {
            m_PlacedObjects.Add(placedObject);
            Debug.Log($"ARFurnitureManagerSimple: Registered placed object: {placedObject.name}");
        }
    }

    void PopulateFurnitureList()
    {
        GameObject contentContainer = GetContentContainer();
        if (contentContainer == null) 
        {
            Debug.LogError("Content container not found!", this);
            return;
        }
        
        Transform contentTransform = contentContainer.transform;
        
        // Clear existing buttons
        foreach (Transform child in contentTransform)
            Destroy(child.gameObject);

        // Create furniture buttons
        for (int i = 0; i < m_FurnitureDatabase.furnitureItems.Length; i++)
        {
            int index = i;
            GameObject buttonObj = Instantiate(m_FurnitureButtonPrefab, contentTransform);
            
            Transform textTransform = buttonObj.transform.Find("TextDoVat");
            Transform imageTransform = buttonObj.transform.Find("ImageDoVat");
            
            Text buttonText = textTransform?.GetComponent<Text>();
            UnityEngine.UI.Image buttonImage = imageTransform?.GetComponent<UnityEngine.UI.Image>();
            
            if (buttonText == null || buttonImage == null)
            {
                Debug.LogError($"FurnitureButtonPrefab missing required components at index {i}!", this);
                continue;
            }
            
            buttonText.text = m_FurnitureDatabase.furnitureItems[index].name;
            
            if (m_FurnitureDatabase.furnitureItems[index].thumbnail != null)
                buttonImage.sprite = m_FurnitureDatabase.furnitureItems[index].thumbnail;
            
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => SelectFurniture(index));
        }
    }

    /// <summary>
    /// Get the content container
    /// </summary>
    GameObject GetContentContainer()
    {
        if (m_ContentParent != null)
            return m_ContentParent;
        return null;
    }

    /// <summary>
    /// Get the info panel
    /// </summary>
    GameObject GetInfoPanel()
    {
        if (m_PanelThongSo != null)
            return m_PanelThongSo;
        return null;
    }

    /// <summary>
    /// Get the name text component
    /// </summary>
    Text GetNameText()
    {
        if (m_TextDoVat != null)
            return m_TextDoVat;
        return null;
    }

    public void SelectFurniture(int index)
    {
        if (index < 0 || index >= m_FurnitureDatabase.furnitureItems.Length)
            return;
            
        m_SelectedFurnitureIndex = index;
        // DON'T clear m_SelectedObject here - let user place multiple items
        
        GameObject infoPanel = GetInfoPanel();
        if (infoPanel != null)
            infoPanel.SetActive(true);
            
        // Display item name in TextDoVat
        Text nameText = GetNameText();
        if (nameText != null)
            nameText.text = m_FurnitureDatabase.furnitureItems[index].name;
            
        // Setup ColorPicker with first available material from database
        UpdateColorPickerForFurnitureType(index);
            
        HighlightFurnitureButton(index);
        
        Debug.Log($"✅ Selected furniture type: {m_FurnitureDatabase.furnitureItems[index].name}. Tap to place on AR plane.");
    }

    /// <summary>
    /// Update ColorPicker for selected furniture type - use first available material
    /// </summary>
    void UpdateColorPickerForFurnitureType(int furnitureIndex)
    {
        if (m_ColorPicker == null)
            return;
        
        if (furnitureIndex < 0 || furnitureIndex >= m_FurnitureDatabase.furnitureItems.Length)
            return;
            
        var furnitureItem = m_FurnitureDatabase.furnitureItems[furnitureIndex];
        Material[] availableMaterials = furnitureItem.availableMaterials;
        
        if (availableMaterials.Length > 0 && availableMaterials[0] != null)
        {
            // Set ColorPicker to first available material color
            m_ColorPicker.color = availableMaterials[0].color;
            Debug.Log($"ColorPicker set to first material color: {availableMaterials[0].color} for {furnitureItem.name}");
        }
        else
        {
            Debug.LogWarning($"No available materials found for {furnitureItem.name}");
        }
    }

    public void SelectPlacedObject(GameObject obj)
    {
        m_SelectedObject = obj;
        m_SelectedFurnitureIndex = FindFurnitureIndex(obj);
        
        Debug.Log($"SelectPlacedObject called for: {obj.name}, Found index: {m_SelectedFurnitureIndex}");
        
        GameObject infoPanel = GetInfoPanel();
        if (infoPanel != null)
        {
            if (m_SelectedFurnitureIndex >= 0)
            {
                infoPanel.SetActive(true);
                Debug.Log("✅ PanelThongSo activated");
            }
            else
            {
                Debug.LogWarning("❌ PanelThongSo NOT activated because furniture index not found");
                // Still show panel but with warning
                infoPanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("❌ PanelThongSo is null!");
        }
            
        // Display object name from FurnitureDatabase, not prefab name
        Text nameText = GetNameText();
        if (nameText != null)
        {
            if (m_SelectedFurnitureIndex >= 0 && m_SelectedFurnitureIndex < m_FurnitureDatabase.furnitureItems.Length)
            {
                // Use name from FurnitureDatabase instead of prefab name
                nameText.text = m_FurnitureDatabase.furnitureItems[m_SelectedFurnitureIndex].name;
                Debug.Log($"Displaying FurnitureDatabase name: {m_FurnitureDatabase.furnitureItems[m_SelectedFurnitureIndex].name}");
            }
            else
            {
                // Fallback to cleaned prefab name if furniture index not found
                nameText.text = obj.name.Replace("(Clone)", "").Trim();
                Debug.LogWarning($"Furniture index not found, using prefab name: {nameText.text}");
            }
        }
            
        // Update ColorPicker to match current object's material color
        UpdateColorPickerForSelectedObject(obj);
        
        HighlightFurnitureButton(m_SelectedFurnitureIndex);
    }

    /// <summary>
    /// Update ColorPicker to show current object's material color
    /// </summary>
    void UpdateColorPickerForSelectedObject(GameObject obj)
    {
        if (m_ColorPicker == null)
            return;
        
        // Find Renderer - check object itself first, then children
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            // If no renderer on parent, look in children (for wrapper GameObjects)
            renderer = obj.GetComponentInChildren<Renderer>();
        }
        
        if (renderer != null && renderer.material != null)
        {
            // Simply show current material color
            m_ColorPicker.color = renderer.material.color;
            Debug.Log($"ColorPicker updated to show current color of {renderer.gameObject.name} (from {obj.name})");
        }
        else
        {
            Debug.LogWarning($"No renderer with material found on {obj.name} or its children");
        }
    }

    public void OnChangeColor(Color color)
    {
        Debug.Log($"OnChangeColor called with color: ({color.r:F2}, {color.g:F2}, {color.b:F2})");
        
        if (m_SelectedObject == null)
        {
            Debug.LogWarning("No object selected - cannot change color");
            return;
        }

        // Find Renderer - check object itself first, then children
        Renderer renderer = m_SelectedObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            // If no renderer on parent, look in children (for wrapper GameObjects like Sofas1)
            renderer = m_SelectedObject.GetComponentInChildren<Renderer>();
        }
        
        if (renderer == null) 
        {
            Debug.LogError($"No Renderer found on selected object or its children: {m_SelectedObject.name}");
            Debug.Log($"Object structure: {GetObjectStructure(m_SelectedObject)}");
            return;
        }
        
        // Simple direct color change - just change the material color
        if (renderer.material != null)
        {
            renderer.material.color = color;
            Debug.Log($"✅ Changed color of {renderer.gameObject.name} (child of {m_SelectedObject.name}) to ({color.r:F2}, {color.g:F2}, {color.b:F2})");
        }
        else
        {
            Debug.LogError("Renderer has no material to change color");
        }
    }

    /// <summary>
    /// Helper method to log object structure for debugging
    /// </summary>
    string GetObjectStructure(GameObject obj)
    {
        string structure = obj.name;
        if (obj.transform.childCount > 0)
        {
            structure += " -> Children: ";
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                structure += child.name;
                if (child.GetComponent<Renderer>() != null)
                    structure += "(Has Renderer)";
                if (i < obj.transform.childCount - 1)
                    structure += ", ";
            }
        }
        return structure;
    }

    void HighlightFurnitureButton(int index)
    {
        GameObject contentContainer = GetContentContainer();
        if (contentContainer == null) return;
        
        Transform contentTransform = contentContainer.transform;
        
        for (int i = 0; i < contentTransform.childCount; i++)
        {
            Transform button = contentTransform.GetChild(i);
            UnityEngine.UI.Image img = button.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.color = index == i ? Color.yellow : Color.white;
        }
    }

    int FindFurnitureIndex(GameObject obj)
    {
        string prefabName = obj.name.Replace("(Clone)", "").Trim();
        
        Debug.Log($"Looking for furniture index for prefab: '{prefabName}'");
        
        for (int i = 0; i < m_FurnitureDatabase.furnitureItems.Length; i++)
        {
            string databasePrefabName = m_FurnitureDatabase.furnitureItems[i].prefab.name;
            Debug.Log($"Comparing with database[{i}]: '{databasePrefabName}' vs '{prefabName}'");
            
            // Compare with prefab name to find matching furniture item
            if (databasePrefabName == prefabName)
            {
                Debug.Log($"✅ Found furniture index {i} for prefab '{prefabName}' -> Database name: '{m_FurnitureDatabase.furnitureItems[i].name}'");
                return i;
            }
        }
        
        Debug.LogError($"❌ No furniture database entry found for prefab: '{prefabName}'");
        Debug.Log("Available prefabs in database:");
        for (int i = 0; i < m_FurnitureDatabase.furnitureItems.Length; i++)
        {
            Debug.Log($"  [{i}] {m_FurnitureDatabase.furnitureItems[i].prefab.name} -> {m_FurnitureDatabase.furnitureItems[i].name}");
        }
        return -1;
    }

    public void DeleteSelectedObject()
    {
        if (m_SelectedObject != null)
        {
            Debug.Log($"Deleting selected object: {m_SelectedObject.name}");
            
            // Remove XR Grab Interactable events before destroying
            XRGrabInteractable grabInteractable = m_SelectedObject.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.hoverEntered.RemoveAllListeners();
                grabInteractable.hoverExited.RemoveAllListeners();
                grabInteractable.selectEntered.RemoveAllListeners();
                grabInteractable.selectExited.RemoveAllListeners();
                
                Debug.Log("Removed XR Grab Interactable events");
            }
            
            // Remove from placed objects list
            m_PlacedObjects.Remove(m_SelectedObject);
            
            // Destroy the object
            Destroy(m_SelectedObject);
            m_SelectedObject = null;
            
            // Close info panel
            GameObject infoPanel = GetInfoPanel();
            if (infoPanel != null)
                infoPanel.SetActive(false);
                
            // Clear furniture selection to allow placing new items
            m_SelectedFurnitureIndex = -1;
            
            // Reset button highlights
            GameObject contentContainer = GetContentContainer();
            if (contentContainer != null)
            {
                Transform contentTransform = contentContainer.transform;
                for (int i = 0; i < contentTransform.childCount; i++)
                {
                    Transform button = contentTransform.GetChild(i);
                    UnityEngine.UI.Image img = button.GetComponent<UnityEngine.UI.Image>();
                    if (img != null)
                        img.color = Color.white;
                }
            }
            
            Debug.Log("✅ Object deleted successfully. Ready to place new furniture.");
        }
        else
        {
            Debug.LogWarning("No object selected to delete");
        }
    }
    #endregion

    #region UI Management
    public void ToggleUI()
    {
        GameObject mainPanel = m_PanelVatPham;
        if (mainPanel == null) return;

        GameObject infoPanel = GetInfoPanel();
        bool wasInfoPanelVisible = infoPanel != null && infoPanel.activeSelf;
        m_IsUIVisible = !m_IsUIVisible;

        // Try SetActive first
        mainPanel.SetActive(m_IsUIVisible);
        
        // Fallback to CanvasGroup if SetActive fails
        if (mainPanel.activeSelf != m_IsUIVisible)
        {
            CanvasGroup cg = mainPanel.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = mainPanel.AddComponent<CanvasGroup>();
            
            mainPanel.SetActive(true);
            
            if (m_IsUIVisible)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            else
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }

        // Handle InfoPanel
        if (infoPanel != null)
        {
            if (m_IsUIVisible && wasInfoPanelVisible)
                infoPanel.SetActive(true);
            else if (!m_IsUIVisible)
                infoPanel.SetActive(false);
        }
    }

    public void CloseInfoPanel()
    {
        GameObject infoPanel = GetInfoPanel();
        if (infoPanel != null)
            infoPanel.SetActive(false);
            
        // Clear selected object but keep furniture type selection for easy placement
        m_SelectedObject = null;
        // DON'T reset m_SelectedFurnitureIndex - keep it for easy placement
        
        // Reset button highlights
        GameObject contentContainer = GetContentContainer();
        if (contentContainer != null)
        {
            Transform contentTransform = contentContainer.transform;
            for (int i = 0; i < contentTransform.childCount; i++)
            {
                Transform button = contentTransform.GetChild(i);
                UnityEngine.UI.Image img = button.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    // Keep the selected furniture button highlighted
                    img.color = (i == m_SelectedFurnitureIndex) ? Color.yellow : Color.white;
                }
            }
        }
        
        Debug.Log($"Info panel closed. Ready to place furniture type: {(m_SelectedFurnitureIndex >= 0 ? m_FurnitureDatabase.furnitureItems[m_SelectedFurnitureIndex].name : "None")}");
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Clear all placed objects
    /// </summary>
    public void ClearAllObjects()
    {
        foreach (GameObject obj in m_PlacedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        m_PlacedObjects.Clear();
        
        m_SelectedObject = null;
        GameObject infoPanel = GetInfoPanel();
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    /// <summary>
    /// Get list of all placed objects
    /// </summary>
    public List<GameObject> GetPlacedObjects()
    {
        m_PlacedObjects.RemoveAll(obj => obj == null);
        return new List<GameObject>(m_PlacedObjects);
    }

    /// <summary>
    /// Set the furniture database
    /// </summary>
    public void SetFurnitureDatabase(FurnitureDatabase database)
    {
        m_FurnitureDatabase = database;
        if (database != null)
            PopulateFurnitureList();
    }
    #endregion

    #region AR Foundation Management
    /// <summary>
    /// Auto-assign AR Foundation components if not manually assigned
    /// </summary>
    void AutoAssignARComponents()
    {
        // Find ARRaycastManager if not assigned
        if (m_ARRaycastManager == null)
        {
            m_ARRaycastManager = FindFirstObjectByType<ARRaycastManager>();
            if (m_ARRaycastManager != null)
            {
                Debug.Log("ARFurnitureManagerSimple: Auto-assigned ARRaycastManager");
            }
            else
            {
                Debug.LogWarning("ARRaycastManager not found. Furniture will use fallback placement method.");
            }
        }

        // Find ARInteractorSpawnTrigger if not assigned  
        if (m_ARInteractorSpawnTrigger == null)
        {
            m_ARInteractorSpawnTrigger = FindFirstObjectByType<ARInteractorSpawnTrigger>();
            if (m_ARInteractorSpawnTrigger != null)
            {
                Debug.Log("ARFurnitureManagerSimple: Found ARInteractorSpawnTrigger - integrating with furniture system");
                
                // Replace ObjectSpawner reference with our furniture spawning
                IntegrateWithARInteractorSpawnTrigger();
                
                // Optional: Sync database with ObjectSpawner for Inspector visibility
                SyncDatabaseWithObjectSpawner();
            }
            else
            {
                Debug.Log("ARInteractorSpawnTrigger not found - using direct touch spawning");
            }
        }

        // Find Screen Space Ray Interactor
        if (m_ScreenSpaceRayInteractor == null)
        {
            m_ScreenSpaceRayInteractor = FindFirstObjectByType<XRRayInteractor>();
            if (m_ScreenSpaceRayInteractor != null)
            {
                Debug.Log("ARFurnitureManagerSimple: Found Screen Space Ray Interactor");
                
                // Subscribe to XR selection events
                m_ScreenSpaceRayInteractor.selectEntered.AddListener(OnXRObjectSelected);
                m_ScreenSpaceRayInteractor.selectExited.AddListener(OnXRObjectDeselected);
                
                Debug.Log("ARFurnitureManagerSimple: Subscribed to XR selection events");
            }
            else
            {
                Debug.LogWarning("Screen Space Ray Interactor not found. Using fallback selection method.");
            }
        }

        // Find Screen Space input components
        if (m_ScreenSpaceSelectInput == null)
        {
            m_ScreenSpaceSelectInput = FindFirstObjectByType<ScreenSpaceSelectInput>();
            if (m_ScreenSpaceSelectInput != null)
            {
                Debug.Log("ARFurnitureManagerSimple: Found Screen Space Select Input");
            }
        }
        
        if (m_ScreenSpacePinchScaleInput == null)
        {
            m_ScreenSpacePinchScaleInput = FindFirstObjectByType<ScreenSpacePinchScaleInput>();
            if (m_ScreenSpacePinchScaleInput != null)
            {
                Debug.Log("ARFurnitureManagerSimple: Found Screen Space Pinch Scale Input");
            }
        }
    }
    #endregion

    #region XR Interaction Events
    /// <summary>
    /// Called when XR Ray Interactor selects an object
    /// </summary>
    void OnXRObjectSelected(SelectEnterEventArgs args)
    {
        GameObject selectedObject = args.interactableObject.transform.gameObject;
        
        // Only handle furniture objects
        if (selectedObject.CompareTag("Furniture"))
        {
            Debug.Log($"XR Selected furniture: {selectedObject.name}");
            SelectPlacedObject(selectedObject);
        }
    }

    /// <summary>
    /// Called when XR Ray Interactor deselects an object
    /// </summary>
    void OnXRObjectDeselected(SelectExitEventArgs args)
    {
        GameObject deselectedObject = args.interactableObject.transform.gameObject;
        Debug.Log($"XR Deselected object: {deselectedObject.name}");
        
        // Optional: Handle deselection behavior here
        // For now, we keep the object selected in our UI until user explicitly closes panel
    }
    #endregion

    #region Furniture XR Interaction Events
    /// <summary>
    /// Called when ray hovers over furniture object
    /// </summary>
    void OnFurnitureHoverEntered(HoverEnterEventArgs args)
    {
        GameObject hoveredObject = args.interactableObject.transform.gameObject;
        Debug.Log($"Hovering over furniture: {hoveredObject.name}");
        
        // Optional: Add visual hover feedback (outline, highlight, etc.)
        AddHoverEffect(hoveredObject);
    }

    /// <summary>
    /// Called when ray stops hovering over furniture object
    /// </summary>
    void OnFurnitureHoverExited(HoverExitEventArgs args)
    {
        GameObject hoveredObject = args.interactableObject.transform.gameObject;
        Debug.Log($"Stopped hovering over furniture: {hoveredObject.name}");
        
        // Remove hover effect
        RemoveHoverEffect(hoveredObject);
    }

    /// <summary>
    /// Called when furniture object is selected via XR interaction
    /// </summary>
    void OnFurnitureSelectEntered(SelectEnterEventArgs args)
    {
        GameObject selectedObject = args.interactableObject.transform.gameObject;
        Debug.Log($"XR Furniture selected: {selectedObject.name}");
        
        // Update Y constraint to current position (in case object was moved)
        // XZPlaneMovementConstraint constraint = selectedObject.GetComponent<XZPlaneMovementConstraint>();
        // if (constraint != null)
        // {
        //     constraint.UpdateFixedYPosition();
        // }
        
        // This will update our UI and show PanelThongSo
        SelectPlacedObject(selectedObject);
        
        // Add selection visual effect
        AddSelectionEffect(selectedObject);
    }

    /// <summary>
    /// Called when furniture object is deselected via XR interaction
    /// </summary>
    void OnFurnitureSelectExited(SelectExitEventArgs args)
    {
        GameObject deselectedObject = args.interactableObject.transform.gameObject;
        Debug.Log($"XR Furniture deselected: {deselectedObject.name}");
        
        // Remove selection visual effect
        RemoveSelectionEffect(deselectedObject);
        
        // Note: We don't close PanelThongSo here to allow continued editing
        // User can close it manually with ButtonClose
    }

    /// <summary>
    /// Add visual hover effect to furniture object
    /// </summary>
    void AddHoverEffect(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Simple hover effect - brighten the material
            Material material = renderer.material;
            Color originalColor = material.color;
            Color hoverColor = originalColor * 1.2f; // Brighten by 20%
            hoverColor.a = originalColor.a; // Keep original alpha
            material.color = hoverColor;
        }
    }

    /// <summary>
    /// Remove visual hover effect from furniture object
    /// </summary>
    void RemoveHoverEffect(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
            renderer = obj.GetComponentInChildren<Renderer>();
            
        if (renderer != null)
        {
            // Reset to original color from database (if not selected)
            if (obj != m_SelectedObject)
            {
                int furnitureIndex = FindFurnitureIndex(obj);
                if (furnitureIndex >= 0 && furnitureIndex < m_FurnitureDatabase.furnitureItems.Length)
                {
                    Material[] materials = m_FurnitureDatabase.furnitureItems[furnitureIndex].availableMaterials;
                    if (materials.Length > 0 && materials[0] != null)
                    {
                        renderer.material = materials[0]; // Apply full material, not just color
                        Debug.Log($"Reset hover effect to database material for {obj.name}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Add visual selection effect to furniture object
    /// </summary>
    void AddSelectionEffect(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Simple selection effect - add slight green tint
            Material material = renderer.material;
            Color originalColor = material.color;
            Color selectionColor = Color.Lerp(originalColor, Color.green, 0.3f);
            material.color = selectionColor;
        }
    }

    /// <summary>
    /// Remove visual selection effect from furniture object
    /// </summary>
    void RemoveSelectionEffect(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
            renderer = obj.GetComponentInChildren<Renderer>();
            
        if (renderer != null)
        {
            // Reset to original material from database
            int furnitureIndex = FindFurnitureIndex(obj);
            if (furnitureIndex >= 0 && furnitureIndex < m_FurnitureDatabase.furnitureItems.Length)
            {
                Material[] materials = m_FurnitureDatabase.furnitureItems[furnitureIndex].availableMaterials;
                if (materials.Length > 0 && materials[0] != null)
                {
                    renderer.material = materials[0]; // Apply full material, not just color
                    Debug.Log($"Reset selection effect to database material for {obj.name}");
                }
            }
        }
    }
    #endregion

    #region ARInteractorSpawnTrigger Integration
    /// <summary>
    /// Integrate ARInteractorSpawnTrigger with our furniture system
    /// </summary>
    void IntegrateWithARInteractorSpawnTrigger()
    {
        if (m_ARInteractorSpawnTrigger == null) return;

        // Get the ObjectSpawner component that ARInteractorSpawnTrigger uses
        var objectSpawner = m_ARInteractorSpawnTrigger.objectSpawner;
        if (objectSpawner != null)
        {
            // IMPORTANT: Add a dummy prefab to prevent index out of range error
            if (objectSpawner.objectPrefabs.Count == 0)
            {
                // Create a temporary empty GameObject as placeholder
                GameObject dummyPrefab = new GameObject("DummySpawnerObject");
                dummyPrefab.SetActive(false); // Keep it inactive
                objectSpawner.objectPrefabs.Add(dummyPrefab);
                
                Debug.Log("Added dummy prefab to ObjectSpawner to prevent index errors");
            }
            
            // Subscribe to ObjectSpawner events to intercept spawning
            objectSpawner.objectSpawned += OnARInteractorSpawned;
            
            Debug.Log("ARFurnitureManagerSimple: Integrated with ARInteractorSpawnTrigger's ObjectSpawner");
        }
    }

    /// <summary>
    /// Called when ARInteractorSpawnTrigger attempts to spawn an object
    /// We intercept this and spawn our furniture instead WITH PRESERVED VERTICAL ORIENTATION
    /// </summary>
    void OnARInteractorSpawned(GameObject spawnedObject)
    {
        if (spawnedObject == null) return;

        // Get spawn position but ignore the rotation from AR hit surface
        Vector3 spawnPosition = spawnedObject.transform.position;
        
        // Destroy the default spawned object immediately
        Destroy(spawnedObject);

        // Spawn our furniture at the same position but with PRESERVED UPRIGHT ORIENTATION
        if (m_SelectedFurnitureIndex >= 0 && m_FurnitureDatabase != null && 
            m_SelectedFurnitureIndex < m_FurnitureDatabase.furnitureItems.Length)
        {
            // Get the selected furniture from database
            var selectedFurniture = m_FurnitureDatabase.furnitureItems[m_SelectedFurnitureIndex];
            
            // Create the furniture instance with identity rotation (upright)
            GameObject newFurniture = Instantiate(selectedFurniture.prefab, spawnPosition, Quaternion.identity);
            
            // CRITICAL: Force upright orientation regardless of surface angle
            newFurniture.transform.rotation = Quaternion.identity;
            
            // Setup XR components with rotation preservation
            SetupXRInteractable(newFurniture);
            
            // Add to placed objects list
            m_PlacedObjects.Add(newFurniture);
            
            Debug.Log($"ARFurnitureManagerSimple: Spawned {selectedFurniture.name} at {spawnPosition} with preserved upright orientation");
        }
        else
        {
            Debug.LogWarning("ARFurnitureManagerSimple: AR spawn triggered but no furniture selected or invalid database");
        }
    }
    #endregion

    #region Debugging Utilities
    /// <summary>
    /// Optional: Sync furniture database with ObjectSpawner for Inspector visibility
    /// </summary>
    void SyncDatabaseWithObjectSpawner()
    {
        if (m_ARInteractorSpawnTrigger == null || m_FurnitureDatabase == null) return;
        
        var objectSpawner = m_ARInteractorSpawnTrigger.objectSpawner;
        if (objectSpawner != null)
        {
            // Extract prefabs from furniture database
            List<GameObject> furniturePrefabs = new List<GameObject>();
            foreach (var item in m_FurnitureDatabase.furnitureItems)
            {
                if (item.prefab != null)
                    furniturePrefabs.Add(item.prefab);
            }
            
            // Assign to ObjectSpawner for Inspector visibility (but won't be used for spawning)
            objectSpawner.objectPrefabs = furniturePrefabs;
            
            Debug.Log($"Synced {furniturePrefabs.Count} furniture prefabs with ObjectSpawner for Inspector display");
        }
    }

    /// <summary>
    /// Debug method to log available materials for selected furniture
    /// </summary>
    void LogAvailableMaterials(int furnitureIndex)
    {
        if (furnitureIndex < 0 || furnitureIndex >= m_FurnitureDatabase.furnitureItems.Length)
        {
            Debug.LogWarning("Invalid furniture index for material logging");
            return;
        }
        
        var furnitureItem = m_FurnitureDatabase.furnitureItems[furnitureIndex];
        Material[] materials = furnitureItem.availableMaterials;
        
        Debug.Log($"=== Available Materials for '{furnitureItem.name}' ===");
        Debug.Log($"Total materials: {materials.Length}");
        
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null)
            {
                Color matColor = materials[i].color;
                Debug.Log($"Material {i}: {materials[i].name} - Color: ({matColor.r:F2}, {matColor.g:F2}, {matColor.b:F2}, {matColor.a:F2})");
            }
            else
            {
                Debug.LogWarning($"Material {i}: NULL");
            }
        }
    }
    #endregion

    #region Test Methods
    /// <summary>
    /// Test method to check if FlexibleColorPicker is working correctly
    /// Call this from Inspector or via debug console
    /// </summary>
    [ContextMenu("Test Color Picker")]
    public void TestColorPicker()
    {
        Debug.Log("=== Testing FlexibleColorPicker ===");
        
        if (m_ColorPicker == null)
        {
            Debug.LogError("FlexibleColorPicker is null!");
            return;
        }
        
        Debug.Log($"FlexibleColorPicker found: {m_ColorPicker.name}");
        Debug.Log($"Current color: {m_ColorPicker.color}");
        
        if (m_SelectedObject == null)
        {
            Debug.LogWarning("No object selected - select a furniture object first");
            return;
        }
        
        Debug.Log($"Selected object: {m_SelectedObject.name}");
        
        // Test color change
        Color testColor = Color.red;
        Debug.Log($"Testing color change to red: {testColor}");
        OnChangeColor(testColor);
        
        // Log available materials for debugging
        if (m_SelectedFurnitureIndex >= 0)
        {
            LogAvailableMaterials(m_SelectedFurnitureIndex);
        }
    }

    /// <summary>
    /// Force re-setup FlexibleColorPicker connection
    /// </summary>
    [ContextMenu("Reconnect Color Picker")]
    public void ReconnectColorPicker()
    {
        Debug.Log("=== Reconnecting FlexibleColorPicker ===");
        
        // Remove existing listener
        RemoveColorPickerListener();
        
        // Try to find ColorPicker again
        if (m_ColorPicker == null)
        {
            m_ColorPicker = FindFirstObjectByType<FlexibleColorPicker>();
            if (m_ColorPicker != null)
            {
                Debug.Log($"Found FlexibleColorPicker: {m_ColorPicker.name}");
            }
            else
            {
                Debug.LogError("FlexibleColorPicker not found in scene!");
                return;
            }
        }
        
        // Re-setup listener
        SetupColorPickerListener();
        
        Debug.Log("FlexibleColorPicker reconnected successfully");
    }
    #endregion

    /// <summary>
    /// Setup precise collider for accurate furniture interaction
    /// </summary>
    void SetupPreciseCollider(GameObject furnitureObject)
    {
        // Check if object already has a collider
        Collider existingCollider = furnitureObject.GetComponent<Collider>();
        if (existingCollider == null)
        {
            // Check children for colliders
            existingCollider = furnitureObject.GetComponentInChildren<Collider>();
        }
        
        if (existingCollider == null)
        {
            // Add BoxCollider if no collider exists
            BoxCollider boxCollider = furnitureObject.AddComponent<BoxCollider>();
            
            // Try to fit collider to mesh bounds
            Renderer renderer = furnitureObject.GetComponent<Renderer>();
            if (renderer == null)
                renderer = furnitureObject.GetComponentInChildren<Renderer>();
                
            if (renderer != null)
            {
                // Set collider size to match visual bounds
                boxCollider.center = renderer.bounds.center - furnitureObject.transform.position;
                boxCollider.size = renderer.bounds.size;
                Debug.Log($"Added precise BoxCollider to {furnitureObject.name} (size: {boxCollider.size})");
            }
            else
            {
                // Default size if no renderer found
                boxCollider.size = Vector3.one;
                Debug.Log($"Added default BoxCollider to {furnitureObject.name}");
            }
        }
        else
        {
            Debug.Log($"Using existing collider on {furnitureObject.name}: {existingCollider.GetType().Name}");
        }
        
        // Ensure collider is set as trigger for better interaction
        if (existingCollider != null)
        {
            existingCollider.isTrigger = false; // Keep as solid collider for precise physics
        }
    }
}

/* 
=== ARInteractorSpawnTrigger Integration Guide ===

SETUP INSTRUCTIONS:
1. Make sure your AR scene has an ARInteractorSpawnTrigger component
2. Ensure the ARInteractorSpawnTrigger has an ObjectSpawner component attached
3. In ARFurnitureManagerSimple Inspector, assign the ARInteractorSpawnTrigger reference
4. The system will automatically integrate on Start()

HOW IT WORKS:
- When ARInteractorSpawnTrigger detects a spawn trigger (touch/gesture)
- Our system intercepts the ObjectSpawner.objectSpawned event
- We destroy the default spawned object and replace it with our selected furniture
- Objects spawn with preserved vertical orientation (always upright)
- XR Interaction Toolkit components are added for manipulation

AXIS PRESERVATION:
- trackRotation = false (prevents automatic rotation changes)
- Rigidbody constraints freeze X/Z rotation (only Y-axis rotation allowed)
- Objects always spawn with Quaternion.identity (upright orientation)
- Touch interactions won't change the vertical axis orientation

ADVANTAGES:
- Leverages existing ARInteractorSpawnTrigger system
- Maintains all gesture recognition features
- Seamless integration with XR Interaction Toolkit
- Preserves object orientation as requested
*/
