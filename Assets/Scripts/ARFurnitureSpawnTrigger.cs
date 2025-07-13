using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets
{
    /// <summary>
    /// Modified ARInteractorSpawnTrigger that works with ARFurnitureManagerSimple instead of ObjectSpawner
    /// Spawns furniture from ARFurnitureManagerSimple when a trigger is activated on AR planes.
    /// </summary>
    public class ARFurnitureSpawnTrigger : MonoBehaviour
    {
        /// <summary>
        /// The type of trigger to use to spawn an object.
        /// </summary>
        public enum SpawnTriggerType
        {
            /// <summary>
            /// Spawn an object when the interactor activates its select input
            /// but no selection actually occurs.
            /// </summary>
            SelectAttempt,

            /// <summary>
            /// Spawn an object when an input is performed.
            /// </summary>
            InputAction,
        }

        [SerializeField]
        [Tooltip("The AR ray interactor that determines where to spawn the object.")]
        XRRayInteractor m_ARInteractor;

        /// <summary>
        /// The AR ray interactor that determines where to spawn the object.
        /// </summary>
        public XRRayInteractor arInteractor
        {
            get => m_ARInteractor;
            set => m_ARInteractor = value;
        }

        [SerializeField]
        [Tooltip("The ARFurnitureManagerSimple to use for spawning furniture.")]
        ARFurnitureManagerSimple m_FurnitureManager;

        /// <summary>
        /// The ARFurnitureManagerSimple to use for spawning furniture.
        /// </summary>
        public ARFurnitureManagerSimple furnitureManager
        {
            get => m_FurnitureManager;
            set => m_FurnitureManager = value;
        }

        [SerializeField]
        [Tooltip("Whether to require that the AR Interactor hits an AR Plane with a horizontal up alignment in order to spawn anything.")]
        bool m_RequireHorizontalUpSurface = true;

        /// <summary>
        /// Whether to require that the AR Interactor hits an AR Plane with a horizontal up alignment in order to spawn anything.
        /// </summary>
        public bool requireHorizontalUpSurface
        {
            get => m_RequireHorizontalUpSurface;
            set => m_RequireHorizontalUpSurface = value;
        }

        [SerializeField]
        [Tooltip("The type of trigger to use to spawn an object.")]
        SpawnTriggerType m_SpawnTriggerType = SpawnTriggerType.SelectAttempt;

        /// <summary>
        /// The type of trigger to use to spawn an object.
        /// </summary>
        public SpawnTriggerType spawnTriggerType
        {
            get => m_SpawnTriggerType;
            set => m_SpawnTriggerType = value;
        }

        [SerializeField]
        XRInputButtonReader m_SpawnObjectInput = new XRInputButtonReader("Spawn Object");

        /// <summary>
        /// The input used to trigger spawn, if <see cref="spawnTriggerType"/> is set to <see cref="SpawnTriggerType.InputAction"/>.
        /// </summary>
        public XRInputButtonReader spawnObjectInput
        {
            get => m_SpawnObjectInput;
            set => XRInputReaderUtility.SetInputProperty(ref m_SpawnObjectInput, value, this);
        }

        [SerializeField]
        [Tooltip("When enabled, spawn will not be triggered if an object is currently selected.")]
        bool m_BlockSpawnWhenInteractorHasSelection = true;

        /// <summary>
        /// When enabled, spawn will not be triggered if an object is currently selected.
        /// </summary>
        public bool blockSpawnWhenInteractorHasSelection
        {
            get => m_BlockSpawnWhenInteractorHasSelection;
            set => m_BlockSpawnWhenInteractorHasSelection = value;
        }

        bool m_AttemptSpawn;
        bool m_EverHadSelection;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnEnable()
        {
            m_SpawnObjectInput.EnableDirectActionIfModeUsed();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnDisable()
        {
            m_SpawnObjectInput.DisableDirectActionIfModeUsed();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Start()
        {
            // Auto-find ARFurnitureManagerSimple if not assigned
            if (m_FurnitureManager == null)
            {
                m_FurnitureManager = FindFirstObjectByType<ARFurnitureManagerSimple>();
                if (m_FurnitureManager != null)
                {
                    Debug.Log("ARFurnitureSpawnTrigger: Auto-found ARFurnitureManagerSimple");
                }
                else
                {
                    Debug.LogError("ARFurnitureSpawnTrigger: No ARFurnitureManagerSimple found in scene!", this);
                }
            }

            if (m_ARInteractor == null)
            {
                Debug.LogError("Missing AR Interactor reference, disabling component.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Update()
        {
            // Wait a frame after the Spawn Object input is triggered to actually cast against AR planes and spawn
            if (m_AttemptSpawn)
            {
                m_AttemptSpawn = false;

                // Cancel the spawn if the select was delayed until the frame after the spawn trigger.
                if (m_ARInteractor.hasSelection)
                    return;

                // Don't spawn the object if the tap was over screen space UI.
                var isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
                if (!isPointerOverUI && m_ARInteractor.TryGetCurrentARRaycastHit(out var arRaycastHit))
                {
                    if (!(arRaycastHit.trackable is ARPlane arPlane))
                        return;

                    if (m_RequireHorizontalUpSurface && arPlane.alignment != PlaneAlignment.HorizontalUp)
                        return;

                    // Use ARFurnitureManagerSimple instead of ObjectSpawner
                    if (m_FurnitureManager != null && m_FurnitureManager.selectedFurnitureIndex >= 0)
                    {
                        // Call private method via reflection or create public method
                        // For now, we'll trigger the spawning through ARFurnitureManagerSimple
                        TriggerFurnitureSpawn(arRaycastHit.pose.position);
                    }
                    else
                    {
                        Debug.Log("ARFurnitureSpawnTrigger: No furniture selected or FurnitureManager not available");
                    }
                }

                return;
            }

            var selectState = m_ARInteractor.logicalSelectState;

            if (m_BlockSpawnWhenInteractorHasSelection)
            {
                if (selectState.wasPerformedThisFrame)
                    m_EverHadSelection = m_ARInteractor.hasSelection;
                else if (selectState.active)
                    m_EverHadSelection |= m_ARInteractor.hasSelection;
            }

            m_AttemptSpawn = false;
            switch (m_SpawnTriggerType)
            {
                case SpawnTriggerType.SelectAttempt:
                    if (selectState.wasCompletedThisFrame)
                        m_AttemptSpawn = !m_ARInteractor.hasSelection && !m_EverHadSelection;
                    break;

                case SpawnTriggerType.InputAction:
                    if (m_SpawnObjectInput.ReadWasPerformedThisFrame())
                        m_AttemptSpawn = !m_ARInteractor.hasSelection && !m_EverHadSelection;
                    break;
            }
        }

        /// <summary>
        /// Trigger furniture spawning through ARFurnitureManagerSimple
        /// </summary>
        void TriggerFurnitureSpawn(Vector3 position)
        {
            if (m_FurnitureManager == null) return;

            // We need to call a public method in ARFurnitureManagerSimple
            // Since SpawnFurniture is private, we'll create a public method for this
            m_FurnitureManager.SpawnFurnitureAtPosition(position);
            
            Debug.Log($"ARFurnitureSpawnTrigger: Triggered furniture spawn at {position}");
        }
    }
}
