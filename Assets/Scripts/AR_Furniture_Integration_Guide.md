# AR Foundation Furniture System Integration Guide

Hướng dẫn này sẽ giúp bạn tích hợp hệ thống furniture của bạn với AR Foundation demo.

## Tổng quan

Tôi đã cập nhật ARFurnitureManager để hỗ trợ đầy đủ cấu trúc UI củ5. **UI không hiển thị hoặc không hoạt động**:
   - Kiểm tra tên object trong scene phải chính xác như hướng dẫn
   - Kiểm tra Console để xem auto-assignment có thành công không
   - Assign thủ công trong Inspector nếu auto-assignment fails

6. **Object không spawn**: 
   - Kiểm tra tag "GroundPlane" và raycast settings
   - Đảm bảo có AR Plane Manager trong scene
   - **Ground Plane Stage missing**: Tạo Empty GameObject làm parent cho furniture

7. **"Không thấy các chấm AR planes"**:
   - Kiểm tra `AR Plane Manager` trong `XR Origin (AR Rig)` có enabled
   - Đảm bảo Plane Prefab được assign trong AR Plane Manager
   - Di chuyển device để scan environment (không giữ yên)
   - Kiểm tra ánh sáng đủ sáng và surface có texture
   - Verify Detection Mode = "Everything" hoặc "Horizontal"

8. **"Touch không hoạt động"**:
   - Kiểm tra `Screen Space Ray Interactor` enabled
   - Verify Input Action Manager có Touch Input Actions
   - Đảm bảo AR Raycast Manager enabled trong XR Origin

8. **"Furniture spawn không đúng vị trí"**:
   - Đảm bảo Ground Plane Stage được assign đúng
   - Kiểm tra AR Camera position
   - Verify raycast từ camera đến plane hoạt động

7. **Color picker không hoạt động**: 
   - Đảm bảo furniture có available materials trong database
   - Kiểm tra FlexibleColorPicker component được assign đúng

8. **Button không respond**:ARFurnitureManager.cs` - Quản lý toàn bộ furniture system với auto-assignment cho UI structure của bạn
2. Cập nhật `ARSampleMenuManager.cs` - Tích hợp với AR Foundation

**Tính năng mới:**
- Auto-assignment các UI component theo tên object trong scene
- Hỗ trợ cấu trúc UI của bạn (PanelVatPham, MenuVatPham, PanelThongSo, etc.)
- Fallback system: nếu không tìm thấy UI mới sẽ dùng legacy UI
- Validation và error logging để debug dễ dàng

**2 Versions Available:**
- ~~`ARFurnitureManager.cs`~~ - Full version (DISABLED due to XR conflicts)
- `ARFurnitureManagerSimple.cs` - **USE THIS VERSION** - Works without XR dependencies

**Quick Fix Steps:**
1. Rename `ARFurnitureManager.cs` to `ARFurnitureManager.cs.bak`
2. Wait for Unity to recompile
3. Use `ARFurnitureManagerSimple` component only

**🔄 AR Foundation Setup - Updated for Trackable Issues:**

**Issue:** Trackable AR planes không thể thêm tag "GroundPlane" trực tiếp

**Solution:** Sử dụng ARRaycastManager thay vì tag-based detection

1. **AR Components đã có sẵn (tự động detection):**
   ```
   Scene Hierarchy (✅ Auto-detected):
   ├── XR Origin (AR Rig) 
   │   ├── XR Origin component ✅
   │   ├── Input Action Manager ✅
   │   ├── AR Plane Manager ✅ (tạo "các chấm")
   │   ├── AR Raycast Manager ✅ (detect touch trên planes - AUTO ASSIGNED)
   │   └── Camera Offset
   │       ├── Main Camera ✅ (AR camera)
   │       └── Screen Space Ray Interactor ✅ (touch handling)
   ```

2. **New Detection Method:**
   - ❌ **Old:** `if (hit.transform.CompareTag("GroundPlane"))`
   - ✅ **New:** `ARRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon)`
   - **Auto-fallback:** Nếu ARRaycastManager không available, sử dụng Physics.Raycast với any surface

3. **Object Selection Enhanced:**
   - ✅ **Furniture Selection:** Tap furniture objects để mở PanelThongSo
   - ✅ **ARInteractorSpawnTrigger Integration:** Enable cho advanced selection và gesture controls
   - ✅ **Unified System:** ARFurnitureManagerSimple quản lý toàn bộ, ARInteractorSpawnTrigger hỗ trợ interaction

**🎯 Ground Plane Stage Setup (Updated):**

## Cấu trúc UI thực tế trong scene

### Canvas Cũ (Canvas của bạn):
```
Canvas
├── PanelVatPham (GameObject - panel chính chứa toàn bộ furniture UI)
│   ├── MenuVatPham (GameObject)
│   │   └── Content (GameObject với prefabs furniture đã tạo - text, image)
│   ├── PanelThongSo (GameObject - hiển thị khi chọn vật phẩm)
│   │   └── Panel (GameObject)
│   │       ├── MenuThongSo (GameObject)
│   │       │   ├── TextDoVat (Text component - hiển thị tên vật phẩm)
│   │       │   ├── ButtonDel (Button - thay thế DeleteButton của demo)
│   │       │   └── ButtonClose (Button - đóng panel)
│   │       └── FlexibleColorPicker (FlexibleColorPicker component)
│   └── ButtonUI (Button - có thể dùng để toggle UI)
```

### Canvas Demo AR Foundation (UI):
```
UI
├── CreateButton (Button - sẽ ẩn hoặc thay thế bằng PanelVatPham)
├── DeleteButton (Button - sẽ ẩn, thay bằng ButtonDel)
└── Object Menu Animator (GameObject - có thể bỏ qua)
```

## Các bước setup

### Bước 1: Tích hợp 2 Canvas

**Cách tích hợp trong scene:**

1. **Thêm Canvas cũ vào scene AR Foundation:**
   - Drag canvas cũ của bạn vào scene
   - Đảm bảo nó có UI Scale Mode phù hợp (Screen Space - Overlay)

2. **Ẩn hoặc xóa UI demo:**
   - Ẩn CreateButton và DeleteButton của demo
   - Hoặc có thể giữ để so sánh

3. **Mapping các component:**

| Chức năng | AR Foundation Demo | Canvas của bạn |
|-----------|-------------------|----------------|
| Menu furniture | CreateButton | PanelVatPham → MenuVatPham → Content |
| Xóa object | DeleteButton | PanelVatPham → PanelThongSo → Panel → MenuThongSo → ButtonDel |
| Thông tin object | (không có) | PanelVatPham → PanelThongSo |
| Color picker | (không có) | PanelVatPham → PanelThongSo → Panel → FlexibleColorPicker |
| Toggle UI | (không có) | PanelVatPham → ButtonUI |

### Bước 2: Setup ARFurnitureManager

**⚠️ Critical Fix:** Do có conflict với XR packages, sử dụng **`ARFurnitureManagerSimple`** only:

1. **Disable ARFurnitureManager.cs:** Đổi tên file thành `ARFurnitureManager.cs.bak` để tránh compilation errors
2. Tạo một GameObject mới trong scene và đặt tên "ARFurnitureManager" 
3. Add component `ARFurnitureManagerSimple` vào GameObject này
4. Assign các references trong Inspector theo cấu trúc canvas của bạn:

**Database & Core:**
- `Furniture Database`: Assign FurnitureDatabase ScriptableObject của bạn
- `Ground Plane Stage`: Assign Transform để parent các object được spawn (xem hướng dẫn Ground Plane setup bên dưới)

**🎯 Ground Plane Stage Setup:**

**Ground Plane Stage** là nơi các furniture objects sẽ được spawn. Trong AR Foundation demo này, cấu trúc thực tế là:

1. **Cấu trúc AR Foundation thực tế trong scene:**
   ```
   XR Origin (AR Rig) - GameObject chính với components:
   ├── XR Origin (component)
   ├── Input Action Manager (component)  
   ├── AR Plane Manager ✅ (tạo ra "các chấm" khi detect surfaces)
   ├── AR Raycast Manager ✅ (detect touch trên planes)
   └── Camera Offset (GameObject con)
       ├── Main Camera (GameObject với AR Camera component)
       └── Screen Space Ray Interactor (GameObject với interaction components)
           ├── XR Ray Interactor
           ├── XR Interaction Group  
           ├── Screen Space Ray Pose Driver
           ├── Touchscreen Hover Filter
           └── Input controls (Select/Rotate/Scale)
   ```

2. **Setup Ground Plane Stage đúng cách:**
   
   **Option 1 - Recommended (Tạo mới):**
   ```
   XR Origin (AR Rig)
   └── Camera Offset
       ├── Main Camera
       ├── Screen Space Ray Interactor  
       └── GroundPlaneStage ← Tạo Empty GameObject này
   ```
   - Tạo Empty GameObject con của `Camera Offset`
   - Đặt tên: "GroundPlaneStage" 
   - Position: (0, 0, 0) relative to Camera Offset
   - Assign GameObject này vào `Ground Plane Stage` field trong ARFurnitureManagerSimple

   **Option 2 - Quick (Dùng có sẵn):**
   - Assign `Camera Offset` GameObject vào Ground Plane Stage field
   - Objects sẽ spawn dưới Camera Offset
   
   **Option 3 - Alternative:**
   - Assign `XR Origin (AR Rig)` vào Ground Plane Stage field
   - Objects sẽ spawn ở root level của AR system

3. **AR Components đã có sẵn (không cần tạo thêm):**
   - ✅ **AR Plane Manager**: Tạo ra các chấm khi detect surfaces
   - ✅ **AR Raycast Manager**: Detect tap trên planes  
   - ✅ **Screen Space Ray Interactor**: Handle touch input
   - ✅ **Main Camera**: AR camera để scan environment
   - ✅ **Input Action Manager**: Handle input events

**UI Components - Your Canvas Structure (ưu tiên):**
- `Panel Vat Pham`: Assign PanelVatPham GameObject
- `Menu Vat Pham`: Assign MenuVatPham GameObject  
- `Content Parent`: Assign Content GameObject (nơi chứa furniture buttons)
- `Panel Thong So`: Assign PanelThongSo GameObject (info panel)
- `Text Do Vat`: Assign TextDoVat Text component (hiển thị tên vật phẩm)

**UI Buttons - Your Canvas Structure:**
- `Button Del`: Assign ButtonDel Button (xóa object)
- `Button Close`: Assign ButtonClose Button (đóng info panel)  
- `Button UI`: Assign ButtonUI Button (toggle UI)

**Color Picker:**
- `Color Picker`: Assign FlexibleColorPicker component trong PanelThongSo

**Prefabs:**
- `Furniture Button Prefab`: Assign prefab button cho furniture list (có TextDoVat và ImageDoVat)

**Legacy UI (tự động fallback nếu UI mới không có):**
- `Furniture List Panel`: Assign PanelVatPham nếu không có UI mới
- `Furniture List Content`: Assign Content nếu không có UI mới  
- `Info Panel`: Assign PanelThongSo nếu không có UI mới

### Bước 3: Auto-Assignment và Validation

ARFurnitureManager có thể tự động tìm và assign các UI component nếu bạn đặt đúng tên trong scene:

**Tên object phải chính xác:**
- `PanelVatPham` (tên GameObject chính)
- `MenuVatPham` (child của PanelVatPham)  
- `Content` (child của MenuVatPham)
- `PanelThongSo` (child của PanelVatPham)
- `Panel` (child của PanelThongSo)
- `MenuThongSo` (child của Panel)
- `TextDoVat` (child của MenuThongSo, có Text component)
- `ButtonDel` (child của MenuThongSo, có Button component)
- `ButtonClose` (child của MenuThongSo, có Button component)
- `FlexibleColorPicker` (child của Panel, có FlexibleColorPicker component)
- `ButtonUI` (child của PanelVatPham, có Button component)

**Validation:**
- Mở Console window để xem các error messages
- ARFurnitureManager sẽ log các component thiếu hoặc không tìm thấy
- Nếu auto-assignment không hoạt động, assign thủ công trong Inspector

### Bước 4: Setup ARSampleMenuManager

1. Tìm GameObject có `ARSampleMenuManager` component trong scene
2. Trong Inspector, assign thêm:

**AR Foundation Integration (Mapping với UI cũ):**
- `Create Button`: Assign **PanelVatPham** (hoặc tạo invisible button nếu cần)
- `Delete Button`: Assign **ButtonDel** (trong MenuThongSo)
- `Object Menu`: Assign **MenuVatPham**
- `Cancel Button`: Assign **ButtonClose** (trong MenuThongSo)

**Furniture Integration:**
- `Furniture Manager`: Assign GameObject có ARFurnitureManager component

### Bước 4: Cấu hình UI Behavior

**PanelVatPham (thay CreateButton):**
- Đặt `SetActive(true)` để hiển thị luôn từ đầu
- Không cần animation show/hide như demo
- Content sẽ populate furniture items

**PanelThongSo (thay Info Panel):**
- Ban đầu `SetActive(false)`
- Hiện khi chọn furniture từ Content
- Chứa TextDovat, ButtonDel, ButtonClose, FlexibleColorPicker

**ButtonUI:**
- Chức năng toggle hiện/ẩn toàn bộ PanelVatPham
- Giữ nguyên behavior như cũ

### Bước 5: Update Furniture Button Prefab

Sử dụng prefab button có sẵn của bạn với cấu trúc:

```
FurnitureButton (GameObject với Button component)
├── Text component (hiển thị tên furniture)
└── Image component (hiển thị thumbnail)
```

**Lưu ý quan trọng:** 
- Đảm bảo prefab có Button component
- Text và Image components phải accessible (có thể là child objects)
- ARFurnitureManager sẽ tự động populate Content với các button này

### Bước 6: Cấu hình Content Layout

**Content GameObject (trong MenuVatPham):**
- Đảm bảo có **GridLayoutGroup** hoặc **VerticalLayoutGroup**
- Add **ContentSizeFitter** nếu cần scroll
- Parent ScrollRect trong MenuVatPham nếu có nhiều items

### Bước 7: AR Foundation Setup (Important for Vuforia users!)

**Khác biệt chính giữa Vuforia và AR Foundation:**

| Aspect | Vuforia | AR Foundation |
|--------|---------|---------------|
| Plane Detection | Image targets | Environmental planes |
| Setup | ImageTarget objects | AR Plane Manager |
| Tracking | Marker-based | Plane-based |
| Ground Detection | Manual positioning | Automatic plane detection |

**Setup AR Foundation cho plane detection:**

1. **Scene của bạn đã có sẵn cấu trúc đúng:**
   ```
   Scene Hierarchy (✅ Ready):
   ├── XR Origin (AR Rig) 
   │   ├── XR Origin component ✅
   │   ├── Input Action Manager ✅
   │   ├── AR Plane Manager ✅ (tạo "các chấm")
   │   ├── AR Raycast Manager ✅ (detect touch trên planes)
   │   └── Camera Offset
   │       ├── Main Camera ✅ (AR camera)
   │       └── Screen Space Ray Interactor ✅ (touch handling)
   └── GroundPlaneStage (cần tạo để parent furniture)
   ```

2. **AR Plane Manager Settings (kiểm tra):**
   - Trong `XR Origin (AR Rig)` → `AR Plane Manager` component:
   - **Detection Mode**: `Everything` hoặc `Horizontal` 
   - **Plane Prefab**: Phải được assign (tạo ra visualization chấm)
   - **Tracking State**: `Tracking`

3. **Tạo Tags cần thiết:**
   - Window → Tags and Layers
   - Tạo tag `"GroundPlane"` 
   - Tạo tag `"Furniture"`

4. **Assign Tags đúng cách:**
   
   **Tag "GroundPlane":**
   - **Gán cho:** AR Plane Prefab trong AR Plane Manager
   - **Cách gán:** 
     - Tìm AR Plane Manager component trong XR Origin (AR Rig)
     - Kiểm tra field "Plane Prefab" 
     - Select plane prefab → Inspector → Tag dropdown → chọn "GroundPlane"
     - **Mục đích:** Để raycast detect được AR planes khi touch

   **Tag "Furniture":**
   - **Gán cho:** Các furniture objects sau khi spawn
   - **Tự động gán:** FurnitureObjectSpawner và ARFurnitureManagerSimple tự động assign tag này
   - **Mục đích:** Để có thể select và interact với furniture objects

5. **Verify Touch Input hoạt động:**
   - `Screen Space Ray Interactor` đã có Select Input, Rotate Input, Scale Input
   - Components này handle touch gestures cho furniture manipulation

**Cách làm xuất hiện "các chấm" (AR Planes):**

1. **Plane Visualization:**
   - AR Foundation sử dụng plane prefab để visualize detected planes
   - Thường là một material với dots pattern
   - Có thể tùy chỉnh material trong AR Plane Manager

2. **Detection Process:**
   - Di chuyển device để camera scan environment
   - Planes sẽ hiện khi AR Foundation detect được flat surfaces
   - Horizontal planes (sàn, bàn) và vertical planes (tường) đều có thể detect

3. **Troubleshooting Plane Detection:**
   - Đảm bảo đủ ánh sáng
   - Surface phải có texture (không phải hoàn toàn trơn)
   - Camera phải di chuyển để gather environmental data

1. Đảm bảo có tag "GroundPlane" cho các surface có thể place object
2. Đảm bảo có tag "Furniture" cho các furniture object
3. Setup Input System nếu chưa có

## Cách sử dụng

### Chức năng chính:

1. **Xem danh sách furniture**: Furniture list sẽ hiển thị khi mở app
2. **Chọn furniture**: Tap vào button trong furniture list
3. **Đặt furniture**: Tap vào ground plane để đặt furniture đã chọn
4. **Chọn object đã đặt**: Tap vào furniture object đã đặt
5. **Thay đổi màu**: Sử dụng color picker trong info panel
6. **Xóa object**: Sử dụng delete button trong info panel hoặc delete button của AR Foundation
7. **Ẩn/hiện UI**: Sử dụng toggle UI button
8. **Rotate/Scale object**: Sử dụng two-finger gestures trên object đã chọn

### Touch Controls:

- **Single tap**: Chọn furniture hoặc đặt furniture
- **Two finger rotation**: Rotate object được chọn
- **Pinch to scale**: Scale object được chọn

### Integration với AR Foundation:

- Sử dụng create button của AR Foundation để mở menu
- Sử dụng delete button của AR Foundation để xóa object focused
- Tích hợp với XR Interaction Toolkit để detect focus và selection

## Troubleshooting

### Các lỗi thường gặp:

1. **"Tag: GroundPlane is not defined" hoặc "Tag: Furniture is not defined"**:
   - **SOLUTION:** Tạo tags trong Unity Editor
   - Window → Tags and Layers → Click `+` → Add "GroundPlane" và "Furniture"
   - **Assign tag "GroundPlane":**
     - Tìm XR Origin (AR Rig) → AR Plane Manager component
     - Trong field "Plane Prefab", select prefab 
     - Inspector → Tag → chọn "GroundPlane"
   - **Tag "Furniture" tự động assign** bởi scripts khi spawn objects
   - Restart scene sau khi tạo tags

2. **"CS1061 'Count' does not exist for array"**:
   - **SOLUTION:** Đã fix trong FurnitureObjectSpawner.cs
   - Array sử dụng `.Length` thay vì `.Count`
   - List sử dụng `.Count`, Array sử dụng `.Length`

2. **"Object Spawner spawn cube thay vì furniture"**:
   - **SOLUTION:** Add component `FurnitureObjectSpawner` vào Object Spawner GameObject
   - **Setup**: FurnitureObjectSpawner tự động connect với GameManager
   - **Kiểm tra**: Console log "Connected to FurnitureObjectSpawner" và "Set to spawn [FurnitureName]"
   - **Flow**: Chọn furniture từ panel trước khi touch AR plane

3. **"Spawn wrong furniture hoặc random furniture"**:
   - **Cause**: Chưa chọn furniture từ panel trước khi touch AR plane
   - **Solution**: Click furniture button trong panel trước → sau đó touch AR plane
   - **Verify**: Console log "Set to spawn [SelectedFurnitureName]"

3. **"Can't add script component 'ARFurnitureManager' because the script class cannot be found"**:
   - **SOLUTION:** Disable ARFurnitureManager.cs bằng cách đổi tên thành `.cs.bak`
   - Chỉ sử dụng `ARFurnitureManagerSimple` 
   - ARFurnitureManagerSimple không có dependency conflicts
   - Restart Unity Editor sau khi disable file

2. **"Can't add script component 'ARFurnitureManagerSimple'"**:
   - Kiểm tra Console window có compilation errors khác không
   - Đảm bảo đã disable ARFurnitureManager.cs (đổi tên .cs.bak)
   - Clear Unity cache: Delete Library folder và reopen project
   - Restart Unity Editor để refresh scripts

3. **"Content container not found"**: 
   - Kiểm tra tên object "Content" trong MenuVatPham phải chính xác
   - Hoặc assign thủ công Content Parent trong Inspector
   - Debug: Sử dụng GameObject.Find("Content") để test

4. **"FurnitureDatabase is not assigned"**: 
   - Đảm bảo đã tạo FurnitureDatabase ScriptableObject
   - Assign vào ARFurnitureManagerSimple trong Inspector

5. **"FurnitureButtonPrefab missing required components"**: 
   - Đảm bảo button prefab có child objects tên "TextDoVat" và "ImageDoVat"
   - TextDoVat phải có Text component
   - ImageDoVat phải có Image component

### Troubleshooting AR Foundation (cho người dùng từ Vuforia):

6. **Không thấy "các chấm" (AR planes) trên màn hình**:
   - **Camera permission**: Đảm bảo app có permission truy cập camera
   - **AR Plane Manager**: Kiểm tra component này có enable và plane prefab đã assign
   - **Environment**: AR Foundation cần surface có texture để detect (không phải bề mặt trơn, đơn màu)
   - **Lighting**: Cần đủ ánh sáng, không được quá tối
   - **Movement**: Di chuyển thiết bị từ từ để scan môi trường
   - **Plane prefab**: Phải assign plane prefab trong AR Plane Manager để visualization

7. **Touch không hoạt động trên AR planes**:
   - **Screen Space Ray Interactor**: Kiểm tra component này trong Camera Offset
   - **Input System**: Đảm bảo Input System package đã được cài
   - **EventSystem**: Cần EventSystem trong scene cho UI input
   - **Tag "GroundPlane"**: Plane objects cần có tag này để raycast detect được

8. **Furniture spawn không đúng vị trí**:
   - **AR Raycast Manager**: Component này cần enable trong XR Origin
   - **Ground Plane Stage**: Đảm bảo đã assign đúng Transform
   - **Raycast hit**: Debug raycast để kiểm tra hit point trên plane
   - **Coordinate system**: Kiểm tra world space vs local space positioning

9. **Furniture bị xoay hoặc scale sai**:
   - **Transform inheritance**: Kiểm tra Ground Plane Stage transform
   - **AR Camera alignment**: Đảm bảo Main Camera có AR Camera component
   - **Input controls**: Kiểm tra Rotate Input và Scale Input trong Screen Space Ray Interactor

### Khác biệt Vuforia vs AR Foundation (Quan trọng!):

| Vấn đề | Vuforia (cũ) | AR Foundation (mới) |
|--------|--------------|---------------------|
| **Plane Detection** | Dùng Image Target, marker | Scan environment, tự động detect planes |
| **Setup** | Drag ImageTarget vào scene | Cần AR Plane Manager + scan |
| **Visual Feedback** | Marker hiển thị khi detect | "Chấm" xuất hiện khi detect planes |
| **Touch Input** | Direct touch on marker | Touch through Screen Space Ray Interactor |
| **Stability** | Ổn định với marker | Cần đủ ánh sáng và texture |

### Cách debug AR Foundation:

10. **Enable AR Plane visualization** để thấy detected planes:
    ```csharp
    // Debug: Kiểm tra có plane nào được detect không
    ARPlaneManager planeManager = FindObjectOfType<ARPlaneManager>();
    Debug.Log($"Detected planes: {planeManager.trackables.count}");
    ```

11. **Debug raycast hit**:
    ```csharp
    // Trong ARFurnitureManagerSimple, thêm debug log
    if (arRaycastManager.Raycast(touchPosition, hits))
    {
        Debug.Log($"Raycast hit: {hits[0].pose.position}");
    }
    else 
    {
        Debug.Log("No raycast hit detected");
    }
    ```

### UI Structure Debugging:
   - Kiểm tra FlexibleColorPicker component được assign đúng

7. **Button không respond**:
   - Kiểm tra Canvas có GraphicRaycaster component
   - Kiểm tra EventSystem có trong scene
   - Kiểm tra Button component có Interactable = true

### UI Structure Debugging:

Nếu auto-assignment không hoạt động, kiểm tra hierarchy như sau:

```
Canvas
└── PanelVatPham ← Đây phải là tên chính xác
    ├── MenuVatPham ← Đây phải là tên chính xác
    │   └── Content ← Đây phải là tên chính xác
    ├── PanelThongSo ← Đây phải là tên chính xác
    │   └── Panel ← Đây phải là tên chính xác
    │       ├── MenuThongSo ← Đây phải là tên chính xác
    │       │   ├── TextDoVat (Text)
    │       │   ├── ButtonDel (Button)
    │       │   └── ButtonClose (Button)
    │       └── FlexibleColorPicker
    └── ButtonUI (Button)
```

### Performance Tips:

1. Giới hạn số lượng furniture được place cùng lúc
2. Sử dụng object pooling cho button prefabs nếu có nhiều furniture items
3. Optimize texture size cho thumbnails

## Customization

### Thêm chức năng mới:

1. **Save/Load**: Implement save/load system trong ARFurnitureManager
2. **Undo/Redo**: Add command pattern cho furniture placement
3. **Multi-selection**: Extend selection system để chọn nhiều object
4. **Animation**: Add animation cho furniture placement/removal

### Modify UI:

1. Thay đổi layout trong PopulateFurnitureList()
2. Customize color picker behavior trong OnChangeColor()
3. Add more info trong info panel

## Quick Setup Guide (cho người chuyển từ Vuforia)

### Checklist nhanh - 5 phút setup:

**✅ Step 1: Tạo Tags cần thiết**
- Window → Tags and Layers (hoặc Edit → Project Settings → Tags and Layers)
- Create tag: `"GroundPlane"`
- Create tag: `"Furniture"`
- ⚠️ **CRITICAL**: Phải tạo tags trước khi test để tránh lỗi "Tag is not defined"

**✅ Step 2: Disable old ARFurnitureManager**
- Đổi tên `ARFurnitureManager.cs` thành `ARFurnitureManager.cs.bak`
- Restart Unity Editor

**✅ Step 3: Create ARFurnitureManagerSimple**
- Tạo Empty GameObject mới tên "ARFurnitureManager"
- Add component `ARFurnitureManagerSimple`

**✅ Step 4: Setup Ground Plane Stage**
- Trong hierarchy: `XR Origin (AR Rig)` → `Camera Offset`
- Tạo Empty GameObject con tên "GroundPlaneStage"
- Assign vào Ground Plane Stage field

**✅ Step 5: Enable Object Spawner Integration (NEW)**
- Tìm GameObject "Object Spawner" trong scene
- **Option A - For Advanced Selection:** Add component `EnableObjectSpawnerIntegration`
  - ✅ Enable ARInteractorSpawnTrigger cho advanced object selection
  - ✅ Enable ObjectSpawner nhưng redirect spawning về ARFurnitureManagerSimple  
  - ✅ Unified system: ARFurnitureManagerSimple làm main controller
- **Option B - Disable Completely:** Add component `DisableObjectSpawnerComponents`
  - ✅ Disable ObjectSpawner và ARInteractorSpawnTrigger hoàn toàn
  - ✅ Chỉ dùng ARFurnitureManagerSimple cho tất cả

**Object Spawner New Role trong hệ thống:**
```
Updated User Action Flow:
1. Click furniture button trong panel → ARFurnitureManagerSimple.SelectFurniture()
2. Touch AR plane → ARRaycastManager detects plane (no tag needed)
3. ARFurnitureManagerSimple.SpawnFurniture() → Spawn từ FurnitureDatabase
4. Touch spawned furniture → ARInteractorSpawnTrigger detects (if enabled)
5. ARFurnitureManagerSimple.SelectPlacedObject() → Show PanelThongSo
6. Use color picker, scale, rotate trong PanelThongSo
```

**✅ Step 6: Assign UI Canvas**
- Drag Canvas của bạn vào scene
- Auto-assignment sẽ tìm PanelVatPham, MenuVatPham, Content, etc.
- Kiểm tra Console messages để xem assignment có thành công

**✅ Step 7: Test**
- Build và run trên device
- Scan environment để thấy "các chấm" 
- Touch chấm để place furniture (từ database, không phải cube)

### Object Spawner Integration:

**Vấn đề cũ:** Object Spawner spawn cube defaults
**Fix mới:** FurnitureObjectSpawner connects Object Spawner với FurnitureDatabase

```
Object Spawner GameObject:
├── ObjectSpawner (component) ✅ 
├── ARInteractorSpawnTrigger (component) ✅
└── FurnitureObjectSpawner (component) ← Add this
    ├── Assign FurnitureDatabase
    ├── Assign ARFurnitureManagerSimple  
    └── Auto-updates Object Spawner prefabs
```

### Tags cần tạo (Updated):
- Window → Tags and Layers
- Create tag: `"Furniture"` ✅ (vẫn cần cho object selection)
- ~~Create tag: `"GroundPlane"`~~ ❌ (không còn cần thiết)

### Chi tiết Tags Assignment (Updated):

**🏷️ Tag "Furniture" (Still Required):**  
- **Assign cho:** Spawned furniture objects (tự động)
- **Scripts tự assign:** 
  - `ARFurnitureManagerSimple.SpawnFurniture()`
- **Mục đích:** Identify furniture objects để select/interact

~~**🏷️ Tag "GroundPlane" (NO LONGER NEEDED):**~~
- ~~**Old method:** Assign cho AR Plane Prefab~~
- ~~**Why removed:** Trackable planes không thể assign tags trực tiếp~~
- ~~**Replaced by:** ARRaycastManager detection~~

### Visual Detection Setup Guide (Updated):

```
AR Plane Detection Flow (New Method):
1. AR Plane Manager → detects surface → spawns plane visualization
2. ARRaycastManager → detects touch on planes (no tag needed)
3. Touch trên plane → ARRaycastManager.Raycast() returns hit
4. ARFurnitureManagerSimple → spawn furniture at hit position
5. Spawned furniture có tag "Furniture" → có thể select/interact

Scene Hierarchy (Updated):
├── XR Origin (AR Rig)
│   ├── AR Plane Manager (creates visual dots)
│   ├── AR Raycast Manager (detects touch - AUTO ASSIGNED)
│   └── Camera Offset
│       └── GroundPlaneStage
│           └── Spawned Furniture (tag: "Furniture") ← tự động assign
```

### Khác biệt chính vs Vuforia:
- **Không cần marker/image target** → Scan environment để detect planes
- **"Các chấm" = detected planes** → Touch chấm để place object
- **Move device around** → Để AR Foundation detect more surfaces
- **Good lighting needed** → AR Foundation cần đủ ánh sáng và texture

---

## Chi tiết implementation

### Cấu trúc File và Dependencies:

**Core Files:**
- `ARFurnitureManagerSimple.cs` ✅ (dùng file này)
- `ARFurnitureManager.cs.bak` ❌ (đã disable)
- `FurnitureDatabase.cs` ✅
- `FlexibleColorPicker.cs` ✅

**Dependencies:**
- ✅ UnityEngine (base Unity)
- ✅ UnityEngine.UI (Unity UI system) 
- ✅ UnityEngine.EventSystems (UI events)
- ✅ UnityEngine.XR.ARFoundation (AR plane detection)
- ❌ XR Interaction Toolkit (removed to avoid conflicts)

2. Implement category system
3. Add search/filter functionality

## Next Steps

1. Test trên device để đảm bảo touch controls hoạt động tốt
2. Add error handling và validation
3. Implement analytics để track usage
4. Add tutorial/onboarding cho user
5. Optimize performance cho low-end devices

## Support

Nếu có vấn đề gì, hãy check:
1. Console logs để xem error messages
2. Đảm bảo tất cả references được assign đúng
3. Kiểm tra Input System settings
4. Verify AR Foundation setup

Chúc bạn thành công với việc tích hợp!
