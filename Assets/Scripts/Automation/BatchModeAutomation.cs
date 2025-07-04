using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using GameByte.Automation;
using GameByte.UI;

namespace GameByte.Automation
{
    /// <summary>
    /// Unity batch mode'da inspector işlemlerinin otomatikleştirilmesi için ana class
    /// GameHeader ve diğer UI componentları için optimize edilmiştir
    /// </summary>
    public static class BatchModeAutomation
    {
        // Performance için reflection cache
        private static Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();
        private static Dictionary<Type, Component[]> componentCache = new Dictionary<Type, Component[]>();
        
        /// <summary>
        /// Batch mode'da çalışacak ana otomasyon metodu
        /// Unity command line'dan çağrılabilir: -executeMethod BatchModeAutomation.AutomateAllInspectorOperations
        /// </summary>
        [MenuItem("GameByte/Automation/Run Full Automation")]
        public static void AutomateAllInspectorOperations()
        {
            Debug.Log("Starting GameByte batch mode automation...");
            
            try
            {
                AssetDatabase.DisallowAutoRefresh();
                
                // 1. Tüm script referanslarını bağla
                LinkAllScriptReferences();
                
                // 2. UI component'larını otomatik yapılandır
                ConfigureUIComponents();
                
                // 3. GameHeader specific otomasyonlar
                ConfigureGameHeaders();
                
                // 4. Prefab'ları otomatik işle
                ProcessAllPrefabs();
                
                // 5. Scene objelerini optimize et
                OptimizeSceneObjects();
                
                Debug.Log("GameByte batch mode automation completed successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Batch mode automation failed: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                AssetDatabase.AllowAutoRefresh();
                AssetDatabase.Refresh();
            }
        }
        
        /// <summary>
        /// Tüm MonoBehaviour'lardaki AutoLink attribute'lu field'ları otomatik bağlar
        /// </summary>
        [MenuItem("GameByte/Automation/Link All Script References")]
        public static void LinkAllScriptReferences()
        {
            Debug.Log("Linking all script references...");
            
            var allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            int linkedCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (LinkScriptReferences(obj))
                {
                    linkedCount++;
                }
            }
            
            Debug.Log($"Script reference linking completed. {linkedCount} objects processed.");
        }
        
        /// <summary>
        /// Tek bir MonoBehaviour'daki script referanslarını bağlar
        /// </summary>
        /// <param name="target">Hedef MonoBehaviour</param>
        /// <returns>Herhangi bir değişiklik yapıldı mı</returns>
        public static bool LinkScriptReferences(MonoBehaviour target)
        {
            if (target == null) return false;
            
            var serializedObject = new SerializedObject(target);
            var fields = GetCachedFields(target.GetType());
            bool hasChanges = false;
            
            foreach (var field in fields)
            {
                var autoLinkAttr = field.GetCustomAttribute<AutoLinkAttribute>();
                if (autoLinkAttr == null) continue;
                
                var property = serializedObject.FindProperty(field.Name);
                if (property != null && property.objectReferenceValue == null)
                {
                    Component component = FindComponent(target, field.FieldType, autoLinkAttr);
                    
                    if (component != null)
                    {
                        property.objectReferenceValue = component;
                        hasChanges = true;
                        Debug.Log($"Auto-linked {field.Name} in {target.name} to {component.name}");
                    }
                }
            }
            
            if (hasChanges)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
            
            return hasChanges;
        }
        
        /// <summary>
        /// AutoLink attribute ayarlarına göre component arar
        /// </summary>
        private static Component FindComponent(MonoBehaviour target, Type componentType, AutoLinkAttribute autoLinkAttr)
        {
            Component component = null;
            
            // Name bazında arama
            if (!string.IsNullOrEmpty(autoLinkAttr.searchByName))
            {
                component = FindComponentByName(target, componentType, autoLinkAttr.searchByName, autoLinkAttr);
            }
            // Tag bazında arama
            else if (!string.IsNullOrEmpty(autoLinkAttr.searchByTag))
            {
                component = FindComponentByTag(target, componentType, autoLinkAttr.searchByTag);
            }
            // Hierarchy bazında arama
            else if (autoLinkAttr.searchInParent)
            {
                component = target.GetComponentInParent(componentType);
            }
            else if (autoLinkAttr.searchInChildren)
            {
                component = target.GetComponentInChildren(componentType);
            }
            else
            {
                component = target.GetComponent(componentType);
            }
            
            return component;
        }
        
        /// <summary>
        /// GameObject adına göre component arar
        /// </summary>
        private static Component FindComponentByName(MonoBehaviour target, Type componentType, string name, AutoLinkAttribute autoLinkAttr)
        {
            Transform searchRoot = target.transform;
            
            if (autoLinkAttr.searchInParent && target.transform.parent != null)
            {
                searchRoot = target.transform.parent;
            }
            
            // Breadth-first search
            Queue<Transform> searchQueue = new Queue<Transform>();
            searchQueue.Enqueue(searchRoot);
            
            while (searchQueue.Count > 0)
            {
                Transform current = searchQueue.Dequeue();
                
                if (current.name.Contains(name))
                {
                    Component component = current.GetComponent(componentType);
                    if (component != null) return component;
                }
                
                // Children'ı search queue'ya ekle
                if (autoLinkAttr.searchInChildren || current == searchRoot)
                {
                    for (int i = 0; i < current.childCount; i++)
                    {
                        searchQueue.Enqueue(current.GetChild(i));
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Tag'e göre component arar
        /// </summary>
        private static Component FindComponentByTag(MonoBehaviour target, Type componentType, string tag)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
            
            foreach (var obj in taggedObjects)
            {
                Component component = obj.GetComponent(componentType);
                if (component != null) return component;
            }
            
            return null;
        }
        
        /// <summary>
        /// GameHeader specific konfigürasyonlar
        /// </summary>
        [MenuItem("GameByte/Automation/Configure Game Headers")]
        public static void ConfigureGameHeaders()
        {
            Debug.Log("Configuring GameHeader components...");
            
            var gameHeaders = UnityEngine.Object.FindObjectsOfType<GameHeader>();
            
            foreach (var gameHeader in gameHeaders)
            {
                ConfigureGameHeader(gameHeader);
            }
            
            Debug.Log($"GameHeader configuration completed. {gameHeaders.Length} headers processed.");
        }
        
        /// <summary>
        /// Tek bir GameHeader'ı konfigüre eder
        /// </summary>
        private static void ConfigureGameHeader(GameHeader gameHeader)
        {
            var serializedObject = new SerializedObject(gameHeader);
            bool hasChanges = false;
            
            // Container referanslarını otomatik bağla
            hasChanges |= AutoLinkContainer(serializedObject, "leftContainer", "LeftContainer");
            hasChanges |= AutoLinkContainer(serializedObject, "centerContainer", "CenterContainer");
            hasChanges |= AutoLinkContainer(serializedObject, "rightContainer", "RightContainer");
            
            // Text component'larını otomatik bağla
            hasChanges |= AutoLinkTextComponent(serializedObject, "scoreText", "ScoreText", gameHeader.transform);
            hasChanges |= AutoLinkTextComponent(serializedObject, "coinsText", "CoinsText", gameHeader.transform);
            hasChanges |= AutoLinkTextComponent(serializedObject, "healthText", "HealthText", gameHeader.transform);
            
            // Icon'ları otomatik bağla (eğer varsa)
            hasChanges |= AutoLinkImageComponent(serializedObject, "coinIcon", "CoinIcon", gameHeader.transform);
            hasChanges |= AutoLinkImageComponent(serializedObject, "healthIcon", "HealthIcon", gameHeader.transform);
            
            if (hasChanges)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(gameHeader);
                Debug.Log($"GameHeader {gameHeader.name} configured successfully.");
            }
        }
        
        /// <summary>
        /// Container referansını otomatik bağlar
        /// </summary>
        private static bool AutoLinkContainer(SerializedObject serializedObject, string propertyName, string containerName)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.objectReferenceValue == null)
            {
                var target = serializedObject.targetObject as MonoBehaviour;
                Transform container = target.transform.Find(containerName);
                
                if (container != null)
                {
                    property.objectReferenceValue = container.GetComponent<RectTransform>();
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Text component'ını otomatik bağlar
        /// </summary>
        private static bool AutoLinkTextComponent(SerializedObject serializedObject, string propertyName, string textName, Transform root)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.objectReferenceValue == null)
            {
                TextMeshProUGUI textComponent = FindTextComponentInChildren(root, textName);
                
                if (textComponent != null)
                {
                    property.objectReferenceValue = textComponent;
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Image component'ını otomatik bağlar
        /// </summary>
        private static bool AutoLinkImageComponent(SerializedObject serializedObject, string propertyName, string imageName, Transform root)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.objectReferenceValue == null)
            {
                Image imageComponent = FindImageComponentInChildren(root, imageName);
                
                if (imageComponent != null)
                {
                    property.objectReferenceValue = imageComponent;
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Child'larda TextMeshProUGUI component arar
        /// </summary>
        private static TextMeshProUGUI FindTextComponentInChildren(Transform root, string name)
        {
            var textComponents = root.GetComponentsInChildren<TextMeshProUGUI>();
            
            foreach (var text in textComponents)
            {
                if (text.name.Contains(name))
                {
                    return text;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Child'larda Image component arar
        /// </summary>
        private static Image FindImageComponentInChildren(Transform root, string name)
        {
            var imageComponents = root.GetComponentsInChildren<Image>();
            
            foreach (var image in imageComponents)
            {
                if (image.name.Contains(name))
                {
                    return image;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// UI component'larını otomatik yapılandırır
        /// </summary>
        [MenuItem("GameByte/Automation/Configure UI Components")]
        public static void ConfigureUIComponents()
        {
            Debug.Log("Configuring UI components...");
            
            // TextMeshPro component'larını yapılandır
            ConfigureTextMeshProComponents();
            
            // Button component'larını yapılandır
            ConfigureButtonComponents();
            
            // Image component'larını yapılandır
            ConfigureImageComponents();
            
            Debug.Log("UI component configuration completed.");
        }
        
        /// <summary>
        /// TextMeshPro component'larını yapılandırır
        /// </summary>
        private static void ConfigureTextMeshProComponents()
        {
            var textComponents = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
            
            foreach (var text in textComponents)
            {
                var serializedObject = new SerializedObject(text);
                bool hasChanges = false;
                
                // Font size ayarla (eğer çok küçük veya çok büyükse)
                var fontSizeProperty = serializedObject.FindProperty("m_fontSize");
                if (fontSizeProperty != null && (fontSizeProperty.floatValue < 12f || fontSizeProperty.floatValue > 72f))
                {
                    fontSizeProperty.floatValue = 18f;
                    hasChanges = true;
                }
                
                // Raycast target'ı false yap (performance için)
                var raycastProperty = serializedObject.FindProperty("m_RaycastTarget");
                if (raycastProperty != null && raycastProperty.boolValue)
                {
                    raycastProperty.boolValue = false;
                    hasChanges = true;
                }
                
                if (hasChanges)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(text);
                }
            }
        }
        
        /// <summary>
        /// Button component'larını yapılandırır
        /// </summary>
        private static void ConfigureButtonComponents()
        {
            var buttons = UnityEngine.Object.FindObjectsOfType<Button>();
            
            foreach (var button in buttons)
            {
                var serializedObject = new SerializedObject(button);
                bool hasChanges = false;
                
                // Transition tipini kontrol et
                var transitionProperty = serializedObject.FindProperty("m_Transition");
                if (transitionProperty != null && transitionProperty.intValue == 0) // None
                {
                    transitionProperty.intValue = 1; // ColorTint
                    hasChanges = true;
                }
                
                if (hasChanges)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(button);
                }
            }
        }
        
        /// <summary>
        /// Image component'larını yapılandırır
        /// </summary>
        private static void ConfigureImageComponents()
        {
            var images = UnityEngine.Object.FindObjectsOfType<Image>();
            
            foreach (var image in images)
            {
                var serializedObject = new SerializedObject(image);
                bool hasChanges = false;
                
                // Eğer sprite yoksa raycast target'ı false yap
                if (image.sprite == null)
                {
                    var raycastProperty = serializedObject.FindProperty("m_RaycastTarget");
                    if (raycastProperty != null && raycastProperty.boolValue)
                    {
                        raycastProperty.boolValue = false;
                        hasChanges = true;
                    }
                }
                
                if (hasChanges)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(image);
                }
            }
        }
        
        /// <summary>
        /// Tüm prefab'ları işler
        /// </summary>
        [MenuItem("GameByte/Automation/Process All Prefabs")]
        public static void ProcessAllPrefabs()
        {
            Debug.Log("Processing all prefabs...");
            
            string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Prefabs" });
            int processedCount = 0;
            
            foreach (string guid in prefabGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (ProcessPrefab(assetPath))
                {
                    processedCount++;
                }
            }
            
            Debug.Log($"Prefab processing completed. {processedCount} prefabs processed.");
        }
        
        /// <summary>
        /// Tek bir prefab'ı işler
        /// </summary>
        private static bool ProcessPrefab(string assetPath)
        {
            try
            {
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                bool hasChanges = false;
                
                // Prefab'daki tüm MonoBehaviour'ları işle
                var components = prefabRoot.GetComponentsInChildren<MonoBehaviour>();
                foreach (var component in components)
                {
                    if (LinkScriptReferences(component))
                    {
                        hasChanges = true;
                    }
                }
                
                // GameHeader'ları özel olarak işle
                var gameHeaders = prefabRoot.GetComponentsInChildren<GameHeader>();
                foreach (var gameHeader in gameHeaders)
                {
                    ConfigureGameHeader(gameHeader);
                    hasChanges = true;
                }
                
                if (hasChanges)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
                    Debug.Log($"Processed prefab: {assetPath}");
                }
                
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                return hasChanges;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to process prefab {assetPath}: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Scene objelerini optimize eder
        /// </summary>
        [MenuItem("GameByte/Automation/Optimize Scene Objects")]
        public static void OptimizeSceneObjects()
        {
            Debug.Log("Optimizing scene objects...");
            
            // Canvas'ları optimize et
            OptimizeCanvases();
            
            // Camera'ları optimize et
            OptimizeCameras();
            
            // Light'ları optimize et
            OptimizeLights();
            
            Debug.Log("Scene object optimization completed.");
        }
        
        /// <summary>
        /// Canvas'ları optimize eder
        /// </summary>
        private static void OptimizeCanvases()
        {
            var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
            
            foreach (var canvas in canvases)
            {
                var serializedObject = new SerializedObject(canvas);
                bool hasChanges = false;
                
                // Pixel Perfect'i false yap (performance için)
                var pixelPerfectProperty = serializedObject.FindProperty("m_PixelPerfect");
                if (pixelPerfectProperty != null && pixelPerfectProperty.boolValue)
                {
                    pixelPerfectProperty.boolValue = false;
                    hasChanges = true;
                }
                
                if (hasChanges)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(canvas);
                }
            }
        }
        
        /// <summary>
        /// Camera'ları optimize eder
        /// </summary>
        private static void OptimizeCameras()
        {
            var cameras = UnityEngine.Object.FindObjectsOfType<Camera>();
            
            foreach (var camera in cameras)
            {
                var serializedObject = new SerializedObject(camera);
                bool hasChanges = false;
                
                // Main Camera için optimizasyonlar
                if (camera.CompareTag("MainCamera"))
                {
                    // HDR'ı kapatabilirsiniz (performance için)
                    var allowHDRProperty = serializedObject.FindProperty("m_AllowHDR");
                    if (allowHDRProperty != null && allowHDRProperty.boolValue)
                    {
                        allowHDRProperty.boolValue = false;
                        hasChanges = true;
                    }
                }
                
                if (hasChanges)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(camera);
                }
            }
        }
        
        /// <summary>
        /// Light'ları optimize eder
        /// </summary>
        private static void OptimizeLights()
        {
            var lights = UnityEngine.Object.FindObjectsOfType<Light>();
            
            foreach (var light in lights)
            {
                var serializedObject = new SerializedObject(light);
                bool hasChanges = false;
                
                // Shadow ayarlarını optimize et
                if (light.type == LightType.Directional)
                {
                    var shadowsProperty = serializedObject.FindProperty("m_Shadows");
                    if (shadowsProperty != null && shadowsProperty.intValue == 0) // None
                    {
                        shadowsProperty.intValue = 1; // Hard Shadows
                        hasChanges = true;
                    }
                }
                
                if (hasChanges)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(light);
                }
            }
        }
        
        /// <summary>
        /// Performance için field cache sistemi
        /// </summary>
        private static FieldInfo[] GetCachedFields(Type type)
        {
            if (!fieldCache.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                fieldCache[type] = fields;
            }
            return fields;
        }
        
        /// <summary>
        /// Cache'i temizle
        /// </summary>
        [MenuItem("GameByte/Automation/Clear Cache")]
        public static void ClearCache()
        {
            fieldCache.Clear();
            componentCache.Clear();
            Debug.Log("Automation cache cleared.");
        }

        /// <summary>
        /// Automation sistem metrikleri ve raporlama
        /// </summary>
        [MenuItem("GameByte/Automation/Generate Automation Report")]
        public static void GenerateAutomationReport()
        {
            Debug.Log("Generating automation metrics report...");
            
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== GameByte Automation Report ===");
            report.AppendLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            
            // Script reference analysis
            var allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            int totalAutoLinkFields = 0;
            int linkedFields = 0;
            int nullFields = 0;
            
            foreach (var obj in allObjects)
            {
                var fields = GetCachedFields(obj.GetType());
                foreach (var field in fields)
                {
                    var autoLinkAttr = field.GetCustomAttribute<AutoLinkAttribute>();
                    if (autoLinkAttr != null)
                    {
                        totalAutoLinkFields++;
                        var value = field.GetValue(obj);
                        if (value != null)
                            linkedFields++;
                        else
                            nullFields++;
                    }
                }
            }
            
            report.AppendLine("--- AutoLink Field Analysis ---");
            report.AppendLine($"Total AutoLink fields: {totalAutoLinkFields}");
            report.AppendLine($"Linked fields: {linkedFields} ({((float)linkedFields/totalAutoLinkFields*100):F1}%)");
            report.AppendLine($"Null fields: {nullFields} ({((float)nullFields/totalAutoLinkFields*100):F1}%)");
            report.AppendLine();
            
            // GameHeader analysis
            var gameHeaders = UnityEngine.Object.FindObjectsOfType<GameHeader>();
            report.AppendLine("--- GameHeader Analysis ---");
            report.AppendLine($"Total GameHeaders in scene: {gameHeaders.Length}");
            
            // Prefab analysis
            string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
            report.AppendLine("--- Prefab Analysis ---");
            report.AppendLine($"Total prefabs in project: {prefabGUIDs.Length}");
            
            // Cache performance metrics
            report.AppendLine("--- Cache Performance ---");
            report.AppendLine($"Cached field types: {fieldCache.Count}");
            report.AppendLine($"Cached component types: {componentCache.Count}");
            
            Debug.Log(report.ToString());
            
            // Save report to file
            string reportPath = "Assets/GameByteAutomationReport.txt";
            System.IO.File.WriteAllText(reportPath, report.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"Report saved to: {reportPath}");
        }

        /// <summary>
        /// Automation sistemi doğrulama ve sağlık kontrolü
        /// </summary>
        [MenuItem("GameByte/Automation/Validate Automation Health")]
        public static void ValidateAutomationHealth()
        {
            Debug.Log("Validating automation system health...");
            
            bool hasErrors = false;
            var issues = new System.Collections.Generic.List<string>();
            
            // AutoLink attribute validation
            var allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var obj in allObjects)
            {
                var objectIssues = ValidateObjectAutoLinks(obj);
                if (objectIssues.Count > 0)
                {
                    hasErrors = true;
                    issues.AddRange(objectIssues);
                }
            }
            
            // Scene structure validation
            var sceneIssues = ValidateSceneStructure();
            if (sceneIssues.Count > 0)
            {
                hasErrors = true;
                issues.AddRange(sceneIssues);
            }
            
            // Prefab validation
            var prefabIssues = ValidatePrefabStructure();
            if (prefabIssues.Count > 0)
            {
                hasErrors = true;
                issues.AddRange(prefabIssues);
            }
            
            if (hasErrors)
            {
                Debug.LogError("Automation validation found issues:");
                foreach (var issue in issues)
                {
                    Debug.LogError($"  - {issue}");
                }
            }
            else
            {
                Debug.Log("✅ Automation system validation passed! No issues found.");
            }
        }

        /// <summary>
        /// Tek bir MonoBehaviour'un AutoLink doğrulaması
        /// </summary>
        private static System.Collections.Generic.List<string> ValidateObjectAutoLinks(MonoBehaviour obj)
        {
            var issues = new System.Collections.Generic.List<string>();
            var fields = GetCachedFields(obj.GetType());
            
            foreach (var field in fields)
            {
                var autoLinkAttr = field.GetCustomAttribute<AutoLinkAttribute>();
                if (autoLinkAttr == null) continue;
                
                var value = field.GetValue(obj);
                if (value == null)
                {
                    // Try to find the component to see if it's missing or just unlinked
                    var component = FindComponent(obj, field.FieldType, autoLinkAttr);
                    if (component == null)
                    {
                        issues.Add($"{obj.name}.{field.Name}: Component not found with current AutoLink settings");
                    }
                    else
                    {
                        issues.Add($"{obj.name}.{field.Name}: Component found but not linked - run automation");
                    }
                }
            }
            
            return issues;
        }

        /// <summary>
        /// Scene yapısı doğrulaması
        /// </summary>
        private static System.Collections.Generic.List<string> ValidateSceneStructure()
        {
            var issues = new System.Collections.Generic.List<string>();
            
            // Canvas validation
            var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
            if (canvases.Length == 0)
            {
                issues.Add("No Canvas found in scene");
            }
            
            // GameHeader validation
            var gameHeaders = UnityEngine.Object.FindObjectsOfType<GameHeader>();
            foreach (var header in gameHeaders)
            {
                if (header.LeftContainer == null || header.CenterContainer == null || header.RightContainer == null)
                {
                    issues.Add($"GameHeader {header.name} missing container references");
                }
            }
            
            return issues;
        }

        /// <summary>
        /// Prefab yapısı doğrulaması
        /// </summary>
        private static System.Collections.Generic.List<string> ValidatePrefabStructure()
        {
            var issues = new System.Collections.Generic.List<string>();
            
            string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
            int validatedCount = 0;
            
            foreach (string guid in prefabGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    var monoBehaviours = prefab.GetComponentsInChildren<MonoBehaviour>();
                    foreach (var mb in monoBehaviours)
                    {
                        if (mb == null) continue; // Missing script
                        
                        var objectIssues = ValidateObjectAutoLinks(mb);
                        foreach (var issue in objectIssues)
                        {
                            issues.Add($"Prefab {path}: {issue}");
                        }
                    }
                    validatedCount++;
                }
            }
            
            if (validatedCount == 0)
            {
                issues.Add("No prefabs found to validate");
            }
            
            return issues;
        }

        /// <summary>
        /// Performance profiling için automation benchmark
        /// </summary>
        [MenuItem("GameByte/Automation/Run Performance Benchmark")]
        public static void RunPerformanceBenchmark()
        {
            Debug.Log("Running automation performance benchmark...");
            
            var stopwatch = new System.Diagnostics.Stopwatch();
            
            // Script linking benchmark
            stopwatch.Start();
            LinkAllScriptReferences();
            stopwatch.Stop();
            long scriptLinkingTime = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Restart();
            ConfigureGameHeaders();
            stopwatch.Stop();
            long gameHeaderTime = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Restart();
            ConfigureUIComponents();
            stopwatch.Stop();
            long uiConfigTime = stopwatch.ElapsedMilliseconds;
            
            Debug.Log("=== Performance Benchmark Results ===");
            Debug.Log($"Script Linking: {scriptLinkingTime}ms");
            Debug.Log($"GameHeader Config: {gameHeaderTime}ms");
            Debug.Log($"UI Component Config: {uiConfigTime}ms");
            Debug.Log($"Total Time: {scriptLinkingTime + gameHeaderTime + uiConfigTime}ms");
            
            // Cache efficiency
            Debug.Log($"Field Cache Entries: {fieldCache.Count}");
            Debug.Log($"Component Cache Entries: {componentCache.Count}");
        }
    }
}
#endif 