using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to automatically setup AR Furniture System in the scene
/// </summary>
public class ARFurnitureSetupHelper : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField]
    [Tooltip("Canvas to create UI elements under")]
    Canvas m_TargetCanvas;
    
    [SerializeField]
    [Tooltip("Furniture Database to use")]
    FurnitureDatabase m_FurnitureDatabase;
    
    [SerializeField]
    [Tooltip("Furniture Button Prefab")]
    GameObject m_FurnitureButtonPrefab;

    [Header("Generated References")]
    [SerializeField]
    GameObject m_FurnitureListPanel;
    
    [SerializeField]
    GameObject m_FurnitureListContent;
    
    [SerializeField]
    GameObject m_InfoPanel;
    
    [SerializeField]
    Text m_ItemNameText;
    
    [SerializeField]
    FlexibleColorPicker m_ColorPicker;
    
    [SerializeField]
    Button m_ToggleUIButton;
    
    [SerializeField]
    Button m_CloseInfoButton;
    
    [SerializeField]
    Button m_DeleteButton;

    #if UNITY_EDITOR
    [ContextMenu("Auto Setup AR Furniture System")]
    public void AutoSetup()
    {
        if (m_TargetCanvas == null)
        {
            m_TargetCanvas = FindFirstObjectByType<Canvas>();
            if (m_TargetCanvas == null)
            {
                Debug.LogError("No Canvas found in scene!");
                return;
            }
        }

        CreateUIElements();
        SetupARFurnitureManager();
        Debug.Log("AR Furniture System setup completed!");
    }

    void CreateUIElements()
    {
        // Create Furniture List Panel
        if (m_FurnitureListPanel == null)
        {
            m_FurnitureListPanel = new GameObject("FurnitureListPanel");
            m_FurnitureListPanel.transform.SetParent(m_TargetCanvas.transform, false);
            
            RectTransform rect = m_FurnitureListPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0.3f, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            m_FurnitureListPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        }

        // Create Furniture List Content
        if (m_FurnitureListContent == null)
        {
            m_FurnitureListContent = new GameObject("FurnitureListContent");
            m_FurnitureListContent.transform.SetParent(m_FurnitureListPanel.transform, false);
            
            RectTransform rect = m_FurnitureListContent.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            ScrollRect scrollRect = m_FurnitureListPanel.AddComponent<ScrollRect>();
            scrollRect.content = rect;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            
            GridLayoutGroup grid = m_FurnitureListContent.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(80, 80);
            grid.spacing = new Vector2(10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
        }

        // Create Info Panel
        if (m_InfoPanel == null)
        {
            m_InfoPanel = new GameObject("InfoPanel");
            m_InfoPanel.transform.SetParent(m_TargetCanvas.transform, false);
            
            RectTransform rect = m_InfoPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.7f, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            m_InfoPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
            
            // Create Item Name Text
            GameObject itemNameObj = new GameObject("ItemNameText");
            itemNameObj.transform.SetParent(m_InfoPanel.transform, false);
            
            RectTransform textRect = itemNameObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.9f);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            m_ItemNameText = itemNameObj.AddComponent<Text>();
            m_ItemNameText.text = "Item Name";
            m_ItemNameText.alignment = TextAnchor.MiddleCenter;
            m_ItemNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            // Create Close Button
            GameObject closeButtonObj = new GameObject("CloseInfoButton");
            closeButtonObj.transform.SetParent(m_InfoPanel.transform, false);
            
            RectTransform closeRect = closeButtonObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0, 0.1f);
            closeRect.anchorMax = new Vector2(0.5f, 0.2f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            
            m_CloseInfoButton = closeButtonObj.AddComponent<Button>();
            closeButtonObj.AddComponent<Image>().color = Color.gray;
            
            GameObject closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeButtonObj.transform, false);
            Text closeText = closeTextObj.AddComponent<Text>();
            closeText.text = "Close";
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;
            
            // Create Delete Button
            GameObject deleteButtonObj = new GameObject("DeleteButton");
            deleteButtonObj.transform.SetParent(m_InfoPanel.transform, false);
            
            RectTransform deleteRect = deleteButtonObj.AddComponent<RectTransform>();
            deleteRect.anchorMin = new Vector2(0.5f, 0.1f);
            deleteRect.anchorMax = new Vector2(1, 0.2f);
            deleteRect.offsetMin = Vector2.zero;
            deleteRect.offsetMax = Vector2.zero;
            
            m_DeleteButton = deleteButtonObj.AddComponent<Button>();
            deleteButtonObj.AddComponent<Image>().color = Color.red;
            
            GameObject deleteTextObj = new GameObject("Text");
            deleteTextObj.transform.SetParent(deleteButtonObj.transform, false);
            Text deleteText = deleteTextObj.AddComponent<Text>();
            deleteText.text = "Delete";
            deleteText.alignment = TextAnchor.MiddleCenter;
            deleteText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            RectTransform deleteTextRect = deleteTextObj.GetComponent<RectTransform>();
            deleteTextRect.anchorMin = Vector2.zero;
            deleteTextRect.anchorMax = Vector2.one;
            deleteTextRect.offsetMin = Vector2.zero;
            deleteTextRect.offsetMax = Vector2.zero;
        }

        // Create Toggle UI Button
        if (m_ToggleUIButton == null)
        {
            GameObject toggleButtonObj = new GameObject("ToggleUIButton");
            toggleButtonObj.transform.SetParent(m_TargetCanvas.transform, false);
            
            RectTransform toggleRect = toggleButtonObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0.9f, 0.9f);
            toggleRect.anchorMax = new Vector2(1, 1);
            toggleRect.offsetMin = Vector2.zero;
            toggleRect.offsetMax = Vector2.zero;
            
            m_ToggleUIButton = toggleButtonObj.AddComponent<Button>();
            toggleButtonObj.AddComponent<Image>().color = Color.blue;
            
            GameObject toggleTextObj = new GameObject("Text");
            toggleTextObj.transform.SetParent(toggleButtonObj.transform, false);
            Text toggleText = toggleTextObj.AddComponent<Text>();
            toggleText.text = "UI";
            toggleText.alignment = TextAnchor.MiddleCenter;
            toggleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            RectTransform toggleTextRect = toggleTextObj.GetComponent<RectTransform>();
            toggleTextRect.anchorMin = Vector2.zero;
            toggleTextRect.anchorMax = Vector2.one;
            toggleTextRect.offsetMin = Vector2.zero;
            toggleTextRect.offsetMax = Vector2.zero;
        }
    }

    void SetupARFurnitureManager()
    {
        Debug.LogWarning("ARFurnitureManager setup is temporarily disabled due to compilation issues.");
        Debug.LogWarning("Please use FurnitureUISetupHelper.cs instead, or manually assign references to ARFurnitureManager after UI creation.");
        
        /*
        // Use reflection to find ARFurnitureManager type safely
        System.Type furnitureManagerType = System.Type.GetType("ARFurnitureManager");
        if (furnitureManagerType == null)
        {
            Debug.LogWarning("ARFurnitureManager type not found. Make sure AR_FOUNDATION_PRESENT is defined and ARFurnitureManager.cs is compiled.");
            return;
        }

        // Find or create ARFurnitureManager using reflection
        MonoBehaviour furnitureManager = FindFirstObjectByType(furnitureManagerType) as MonoBehaviour;
        if (furnitureManager == null)
        {
            GameObject furnitureManagerObj = new GameObject("ARFurnitureManager");
            furnitureManager = furnitureManagerObj.AddComponent(furnitureManagerType) as MonoBehaviour;
        }

        if (furnitureManager == null)
        {
            Debug.LogError("Failed to create ARFurnitureManager component!");
            return;
        }

        // Use reflection to assign properties
        AssignProperty(furnitureManager, "furnitureDatabase", m_FurnitureDatabase);
        AssignProperty(furnitureManager, "furnitureListPanel", m_FurnitureListPanel);
        AssignProperty(furnitureManager, "furnitureListContent", m_FurnitureListContent);
        AssignProperty(furnitureManager, "infoPanel", m_InfoPanel);
        AssignProperty(furnitureManager, "toggleUIButton", m_ToggleUIButton);
        AssignProperty(furnitureManager, "closeInfoButton", m_CloseInfoButton);
        AssignProperty(furnitureManager, "deleteButton", m_DeleteButton);
        AssignProperty(furnitureManager, "itemNameText", m_ItemNameText);
        AssignProperty(furnitureManager, "furnitureButtonPrefab", m_FurnitureButtonPrefab);

        // Try to find and assign color picker
        FlexibleColorPicker colorPicker = FindFirstObjectByType<FlexibleColorPicker>();
        if (colorPicker != null)
            AssignProperty(furnitureManager, "colorPicker", colorPicker);

        // Mark scene as dirty
        EditorUtility.SetDirty(furnitureManager);
        if (m_InfoPanel != null)
            EditorUtility.SetDirty(m_InfoPanel);
            
        Debug.Log("ARFurnitureManager setup completed using reflection!");
        */
    }

    void AssignProperty(MonoBehaviour target, string propertyName, object value)
    {
        if (target == null || value == null) return;

        try
        {
            System.Type targetType = target.GetType();
            var property = targetType.GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value);
                Debug.Log($"Assigned {propertyName} = {value.GetType().Name}");
            }
            else
            {
                Debug.LogWarning($"Property {propertyName} not found or not writable on {targetType.Name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to assign {propertyName}: {e.Message}");
        }
    }

    [ContextMenu("Find and Assign Existing Components")]
    public void FindExistingComponents()
    {
        // Find existing UI components
        m_FurnitureListPanel = GameObject.Find("FurnitureListPanel");
        m_FurnitureListContent = GameObject.Find("FurnitureListContent");
        m_InfoPanel = GameObject.Find("InfoPanel");
        
        if (m_InfoPanel != null)
        {
            m_ItemNameText = m_InfoPanel.GetComponentInChildren<Text>();
            m_CloseInfoButton = m_InfoPanel.transform.Find("CloseInfoButton")?.GetComponent<Button>();
            m_DeleteButton = m_InfoPanel.transform.Find("DeleteButton")?.GetComponent<Button>();
        }
        
        m_ToggleUIButton = GameObject.Find("ToggleUIButton")?.GetComponent<Button>();
        m_ColorPicker = FindFirstObjectByType<FlexibleColorPicker>();
        
        Debug.Log("Found and assigned existing components");
    }
    #endif
}
