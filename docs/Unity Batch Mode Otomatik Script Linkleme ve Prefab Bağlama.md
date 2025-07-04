# Unity Batch Mode Otomatik Script Linkleme ve Prefab Bağlama

Unity batch mode'da otomatik script linkleme ve prefab bağlama işlemleri **tamamen mümkün** ve endüstride yaygın olarak kullanılmaktadır. Unity'nin güçlü Editor scripting API'leri sayesinde, normalde inspector'da manuel yapılan tüm işlemler programatik olarak otomatikleştirilebilir.

## Temel yaklaşım: tüm işlemler mümkün

Unity batch mode'da **SerializedObject** ve **SerializedProperty** API'leri kullanılarak inspector'da yapılan tüm işlemler otomatikleştirilebilir. Bu yaklaşım Unity'nin resmi API'leri kullandığı için güvenli, kararlı ve undo sistemi ile uyumludur.

## Script referanslarının otomatik linklenmesi

### 1. Reflection tabanlı otomatik atama

```csharp
using System.Reflection;
using UnityEngine;
using UnityEditor;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class AutoLinkAttribute : System.Attribute
{
    public bool searchInParent;
    public bool searchInChildren;
    
    public AutoLinkAttribute(bool searchInParent = false, bool searchInChildren = false)
    {
        this.searchInParent = searchInParent;
        this.searchInChildren = searchInChildren;
    }
}

public class AutoScriptLinker : MonoBehaviour
{
    // Otomatik bağlanacak script referansları
    [AutoLink] [SerializeField] private PlayerController playerController;
    [AutoLink(searchInChildren: true)] [SerializeField] private UIManager uiManager;
    [AutoLink(searchInParent: true)] [SerializeField] private GameManager gameManager;
    
    // Batch mode'da çalışacak otomatik bağlama metodu
    public static void AutoLinkAllScripts()
    {
        // Tüm MonoBehaviour'ları bul
        var allObjects = FindObjectsOfType<MonoBehaviour>();
        
        foreach (var obj in allObjects)
        {
            LinkScriptReferences(obj);
        }
    }
    
    private static void LinkScriptReferences(MonoBehaviour target)
    {
        var serializedObject = new SerializedObject(target);
        var fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        
        foreach (var field in fields)
        {
            var autoLinkAttr = field.GetCustomAttribute<AutoLinkAttribute>();
            if (autoLinkAttr == null) continue;
            
            var property = serializedObject.FindProperty(field.Name);
            if (property != null && property.objectReferenceValue == null)
            {
                Component component = null;
                
                if (autoLinkAttr.searchInParent)
                {
                    component = target.GetComponentInParent(field.FieldType);
                }
                else if (autoLinkAttr.searchInChildren)
                {
                    component = target.GetComponentInChildren(field.FieldType);
                }
                else
                {
                    component = target.GetComponent(field.FieldType);
                }
                
                if (component != null)
                {
                    property.objectReferenceValue = component;
                }
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
```

### 2. Batch mode'da kullanım

```csharp
public class BatchModeScriptLinker
{
    [MenuItem("Tools/Link All Scripts (Batch Mode Compatible)")]
    public static void LinkAllScriptsInProject()
    {
        // Tüm prefab'ları bul
        string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject");
        
        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ProcessPrefab(assetPath);
        }
        
        // Sahne objelerini işle
        ProcessSceneObjects();
    }
    
    private static void ProcessPrefab(string assetPath)
    {
        // Prefab'ı yükle
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
        
        // Tüm script referanslarını bağla
        var components = prefabRoot.GetComponentsInChildren<MonoBehaviour>();
        foreach (var component in components)
        {
            LinkScriptReferences(component);
        }
        
        // Değişiklikleri kaydet
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }
    
    private static void ProcessSceneObjects()
    {
        var allObjects = FindObjectsOfType<MonoBehaviour>();
        foreach (var obj in allObjects)
        {
            LinkScriptReferences(obj);
        }
    }
}
```

## Prefabların otomatik bağlanması

### 1. Naming convention tabanlı prefab bağlama

```csharp
public class AutoPrefabBinder : MonoBehaviour
{
    [SerializeField] private GameObject[] weaponPrefabs;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject[] uiPrefabs;
    
    // Batch mode'da çalışacak prefab bağlama metodu
    public static void AutoBindAllPrefabs()
    {
        var allComponents = FindObjectsOfType<AutoPrefabBinder>();
        
        foreach (var component in allComponents)
        {
            BindPrefabsToComponent(component);
        }
    }
    
    private static void BindPrefabsToComponent(AutoPrefabBinder target)
    {
        var serializedObject = new SerializedObject(target);
        
        // Weapon prefab'larını bağla
        var weaponProperty = serializedObject.FindProperty("weaponPrefabs");
        BindPrefabArray(weaponProperty, "Weapons");
        
        // Enemy prefab'larını bağla
        var enemyProperty = serializedObject.FindProperty("enemyPrefabs");
        BindPrefabArray(enemyProperty, "Enemies");
        
        // UI prefab'larını bağla
        var uiProperty = serializedObject.FindProperty("uiPrefabs");
        BindPrefabArray(uiProperty, "UI");
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private static void BindPrefabArray(SerializedProperty arrayProperty, string category)
    {
        // Kategori bazında prefab'ları bul
        var prefabs = LoadPrefabsByCategory(category);
        
        arrayProperty.arraySize = prefabs.Length;
        for (int i = 0; i < prefabs.Length; i++)
        {
            arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
        }
    }
    
    private static GameObject[] LoadPrefabsByCategory(string category)
    {
        var prefabList = new List<GameObject>();
        
        // Asset veritabanında kategori bazında arama
        string[] guids = AssetDatabase.FindAssets($"t:GameObject {category}");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                prefabList.Add(prefab);
            }
        }
        
        return prefabList.ToArray();
    }
}
```

### 2. ScriptableObject veritabanı ile prefab yönetimi

```csharp
[CreateAssetMenu(fileName = "PrefabDatabase", menuName = "Tools/Prefab Database")]
public class PrefabDatabase : ScriptableObject
{
    [SerializeField] private List<PrefabEntry> prefabs = new List<PrefabEntry>();
    
    [System.Serializable]
    public class PrefabEntry
    {
        public string id;
        public GameObject prefab;
        public string category;
        public string[] tags;
    }
    
    // Batch mode'da otomatik doldurma
    public void AutoPopulateDatabase()
    {
        prefabs.Clear();
        
        // Tüm prefab'ları bul
        string[] guids = AssetDatabase.FindAssets("t:GameObject");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                var entry = new PrefabEntry
                {
                    id = prefab.name,
                    prefab = prefab,
                    category = GetCategoryFromPath(path),
                    tags = GetTagsFromName(prefab.name)
                };
                
                prefabs.Add(entry);
            }
        }
        
        EditorUtility.SetDirty(this);
    }
    
    private string GetCategoryFromPath(string path)
    {
        // Yol bilgisinden kategori çıkar
        string[] pathParts = path.Split('/');
        return pathParts.Length > 2 ? pathParts[2] : "Default";
    }
    
    private string[] GetTagsFromName(string name)
    {
        // İsimden etiket çıkar
        return name.Split('_');
    }
}
```

## Inspector işlemlerinin batch mode'da otomatikleştirilmesi

### 1. Kapsamlı automation sistemi

```csharp
public class BatchModeAutomation
{
    // Batch mode'da çalışacak ana metod
    public static void AutomateAllInspectorOperations()
    {
        Debug.Log("Starting batch mode automation...");
        
        // 1. Tüm script referanslarını bağla
        LinkAllScriptReferences();
        
        // 2. Prefab'ları otomatik bağla
        AutoBindAllPrefabs();
        
        // 3. Component ayarlarını otomatik yapılandır
        ConfigureAllComponents();
        
        // 4. Material ve texture atamalarını yap
        AssignMaterialsAndTextures();
        
        // 5. Prefab varyantlarını oluştur
        CreatePrefabVariants();
        
        Debug.Log("Batch mode automation completed.");
    }
    
    private static void LinkAllScriptReferences()
    {
        var allObjects = FindObjectsOfType<MonoBehaviour>();
        
        foreach (var obj in allObjects)
        {
            AutoLinkComponentReferences(obj);
        }
    }
    
    private static void AutoLinkComponentReferences(MonoBehaviour component)
    {
        var serializedObject = new SerializedObject(component);
        var fields = component.GetType().GetFields(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        
        foreach (var field in fields)
        {
            // Component tipindeki alanları otomatik bağla
            if (field.FieldType.IsSubclassOf(typeof(Component)))
            {
                var property = serializedObject.FindProperty(field.Name);
                if (property != null && property.objectReferenceValue == null)
                {
                    var foundComponent = component.GetComponent(field.FieldType);
                    if (foundComponent != null)
                    {
                        property.objectReferenceValue = foundComponent;
                    }
                }
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private static void ConfigureAllComponents()
    {
        // Collider'ları otomatik yapılandır
        ConfigureColliders();
        
        // Rigidbody'leri otomatik yapılandır
        ConfigureRigidbodies();
        
        // UI elementlerini otomatik yapılandır
        ConfigureUIElements();
    }
    
    private static void ConfigureColliders()
    {
        var colliders = FindObjectsOfType<Collider>();
        
        foreach (var collider in colliders)
        {
            var serializedObject = new SerializedObject(collider);
            
            // İsim bazında trigger ayarı
            if (collider.name.ToLower().Contains("trigger"))
            {
                var isTriggerProperty = serializedObject.FindProperty("m_IsTrigger");
                isTriggerProperty.boolValue = true;
            }
            
            // Mesh collider için convex ayarı
            if (collider is MeshCollider meshCollider)
            {
                var convexProperty = serializedObject.FindProperty("m_Convex");
                convexProperty.boolValue = meshCollider.name.ToLower().Contains("convex");
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
```

### 2. Custom PropertyDrawer ile gelişmiş otomatik atama

```csharp
[CustomPropertyDrawer(typeof(AutoAssignAttribute))]
public class AutoAssignDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // Property alanı
        var fieldRect = new Rect(position.x, position.y, position.width - 60, position.height);
        EditorGUI.PropertyField(fieldRect, property, label);
        
        // Otomatik atama butonu
        var buttonRect = new Rect(position.x + position.width - 55, position.y, 55, position.height);
        if (GUI.Button(buttonRect, "Auto"))
        {
            AutoAssignReference(property);
        }
        
        EditorGUI.EndProperty();
    }
    
    private void AutoAssignReference(SerializedProperty property)
    {
        var target = property.serializedObject.targetObject as MonoBehaviour;
        if (target == null) return;
        
        var fieldInfo = GetFieldInfo(property);
        if (fieldInfo == null) return;
        
        // Attribute'tan ayarları al
        var autoAssignAttr = fieldInfo.GetCustomAttribute<AutoAssignAttribute>();
        
        Component component = null;
        
        if (autoAssignAttr.searchInChildren)
        {
            component = target.GetComponentInChildren(fieldInfo.FieldType);
        }
        else if (autoAssignAttr.searchInParent)
        {
            component = target.GetComponentInParent(fieldInfo.FieldType);
        }
        else
        {
            component = target.GetComponent(fieldInfo.FieldType);
        }
        
        if (component != null)
        {
            property.objectReferenceValue = component;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
```

## Batch mode command line kullanımı

### 1. Temel batch mode komutu

```bash
# Windows
"C:\Program Files\Unity\Hub\Editor\2023.2.0f1\Editor\Unity.exe" ^
  -batchmode ^
  -quit ^
  -projectPath "C:\MyProject" ^
  -executeMethod BatchModeAutomation.AutomateAllInspectorOperations ^
  -logFile "C:\automation.log"

# macOS/Linux
/Applications/Unity/Hub/Editor/2023.2.0f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -quit \
  -projectPath ~/MyProject \
  -executeMethod BatchModeAutomation.AutomateAllInspectorOperations \
  -logFile ~/automation.log
```

### 2. CI/CD pipeline entegrasyonu

```yaml
# GitHub Actions örneği
name: Unity Automation
on: [push, pull_request]

jobs:
  automate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          lfs: true
      
      - uses: game-ci/unity-builder@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
        with:
          targetPlatform: StandaloneWindows64
          customParameters: '-executeMethod BatchModeAutomation.AutomateAllInspectorOperations'
```

## Gelişmiş teknikler ve optimizasyon

### 1. Reflection cache ile performans optimizasyonu

```csharp
public static class ReflectionCache
{
    private static Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();
    
    public static FieldInfo[] GetFields(Type type)
    {
        if (!fieldCache.TryGetValue(type, out var fields))
        {
            fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            fieldCache[type] = fields;
        }
        return fields;
    }
}
```

### 2. Asset postprocessor ile otomatik işleme

```csharp
public class AutomationPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.EndsWith(".prefab"))
            {
                AutoProcessPrefab(assetPath);
            }
        }
    }
    
    private static void AutoProcessPrefab(string assetPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        
        if (prefab != null)
        {
            // Prefab'ı otomatik işle
            var components = prefab.GetComponentsInChildren<MonoBehaviour>();
            foreach (var component in components)
            {
                AutoLinkComponentReferences(component);
            }
        }
    }
}
```

## Sonuç ve öneriler

Unity batch mode'da **tüm inspector işlemleri otomatikleştirilebilir**. Başarılı implementasyon için:

1. **SerializedObject/SerializedProperty API'lerini kullanın** - Unity'nin resmi ve güvenli yolu
2. **Reflection'ı dikkatli kullanın** - Performance için cache mekanizmaları implementedin
3. **Attribute tabanlı yaklaşımlar** - Deklaratif ve sürdürülebilir kod için
4. **Batch işlemlerde AssetDatabase.DisallowAutoRefresh() kullanın** - Performance için kritik
5. **Comprehensive error handling** - Batch mode'da hata ayıklama zor olabilir
6. **CI/CD pipeline entegrasyonu** - Automation'u sürekli geliştirme sürecine dahil edin

Bu yaklaşımlar kullanılarak Unity projelerinde manuel inspector işlemlerinin %90'ından fazlası otomatikleştirilebilir ve büyük projeler için önemli zaman tasarrufu sağlanabilir.