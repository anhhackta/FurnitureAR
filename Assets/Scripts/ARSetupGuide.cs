using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Hướng dẫn setup ARInteractorSpawnTrigger trong scene
/// Script này chỉ để tham khảo, không cần add vào scene
/// </summary>
public class ARSetupGuide : MonoBehaviour
{
    /*
    === HƯỚNG DẪN SETUP ARInteractorSpawnTrigger ===
    
    🎯 BƯỚC 1: Tạo GameObject
    1. Right-click Hierarchy → Create Empty
    2. Đặt tên: "ARInteractorSpawnTrigger"
    
    🔧 BƯỚC 2: Add Components
    1. Add Component → ARInteractorSpawnTrigger
    2. Add Component → ObjectSpawner
    
    ⚙️ BƯỚC 3: Setup ARInteractorSpawnTrigger
    - AR Interactor: Drag "Screen Space Ray Interactor" từ XR Origin/Camera Offset/Main Camera
    - Object Spawner: Drag ObjectSpawner component trên cùng GameObject
    - Spawn Trigger Type: SelectAttempt
    - Require Selection Missed: ✓ (checked)
    
    📦 BƯỚC 4: Setup ObjectSpawner  
    - Object Prefabs: [] (empty array - furniture system sẽ handle)
    - Spawn Surface: Assign AR Plane Manager nếu có
    - Delete Spawned On Disable: false
    
    🔗 BƯỚC 5: Assign vào ARFurnitureManagerSimple
    - Trong Inspector của ARFurnitureManagerSimple
    - AR Foundation Components → AR Interactor Spawn Trigger
    - Drag GameObject ARInteractorSpawnTrigger vào field này
    
    📍 VỊ TRÍ TRONG HIERARCHY:
    Scene
    ├── XR Origin (AR Rig)
    │   ├── Camera Offset  
    │   │   └── Main Camera
    │   │       └── Screen Space Ray Interactor ← Assign này
    │   └── ...
    ├── ARInteractorSpawnTrigger ← GameObject bạn tạo
    │   ├── ARInteractorSpawnTrigger (Script)
    │   └── ObjectSpawner (Script)  
    ├── ARFurnitureManagerSimple
    └── Canvas UI
    
    ✅ KẾT QUỞ:
    - Touch vào AR plane → ARInteractorSpawnTrigger trigger
    - Furniture system intercept và spawn furniture thay ObjectSpawner
    - Objects giữ nguyên trục thẳng như yêu cầu
    - Hoạt động với XR Interaction Toolkit gestures
    */
}
