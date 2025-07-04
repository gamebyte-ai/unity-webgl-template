#if UNITY_EDITOR
using System.Reflection;
using UnityEngine;
using UnityEditor;
using GameByte.Automation;

namespace GameByte.Editor
{
    /// <summary>
    /// AutoLink attribute için custom property drawer
    /// Inspector'da "Auto" butonu ile otomatik referans atama
    /// </summary>
    [CustomPropertyDrawer(typeof(AutoLinkAttribute))]
    public class AutoAssignDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Property alanı (original field width - button width)
            var fieldRect = new Rect(position.x, position.y, position.width - 60, position.height);
            EditorGUI.PropertyField(fieldRect, property, label);
            
            // Otomatik atama butonu
            var buttonRect = new Rect(position.x + position.width - 55, position.y, 55, position.height);
            
            // Butonu sadece null referanslarda aktif göster
            bool wasEnabled = GUI.enabled;
            GUI.enabled = property.objectReferenceValue == null;
            
            if (GUI.Button(buttonRect, new GUIContent("Auto", "Otomatik referans atama")))
            {
                AutoAssignReference(property);
            }
            
            GUI.enabled = wasEnabled;
            EditorGUI.EndProperty();
        }
        
        /// <summary>
        /// Property'e otomatik referans atar
        /// </summary>
        /// <param name="property">Hedef SerializedProperty</param>
        private void AutoAssignReference(SerializedProperty property)
        {
            var target = property.serializedObject.targetObject as MonoBehaviour;
            if (target == null) 
            {
                Debug.LogWarning("AutoAssign: Target is not a MonoBehaviour");
                return;
            }
            
            var fieldInfo = GetFieldInfo(property);
            if (fieldInfo == null) 
            {
                Debug.LogWarning($"AutoAssign: Could not find field info for {property.name}");
                return;
            }
            
            // Attribute'tan ayarları al
            var autoLinkAttr = fieldInfo.GetCustomAttribute<AutoLinkAttribute>();
            if (autoLinkAttr == null) 
            {
                Debug.LogWarning($"AutoAssign: AutoLink attribute not found on {fieldInfo.Name}");
                return;
            }
            
            Component component = FindComponent(target, fieldInfo.FieldType, autoLinkAttr);
            
            if (component != null)
            {
                property.objectReferenceValue = component;
                property.serializedObject.ApplyModifiedProperties();
                Debug.Log($"AutoAssign: Successfully linked {fieldInfo.Name} to {component.name}");
            }
            else
            {
                Debug.LogWarning($"AutoAssign: Could not find component of type {fieldInfo.FieldType.Name} for {fieldInfo.Name}");
            }
        }
        
        /// <summary>
        /// Component'ı AutoLink attribute ayarlarına göre arar
        /// </summary>
        private Component FindComponent(MonoBehaviour target, System.Type componentType, AutoLinkAttribute autoLinkAttr)
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
        private Component FindComponentByName(MonoBehaviour target, System.Type componentType, string name, AutoLinkAttribute autoLinkAttr)
        {
            Transform searchRoot = target.transform;
            
            if (autoLinkAttr.searchInParent && target.transform.parent != null)
            {
                searchRoot = target.transform.parent;
            }
            
            // Breadth-first search
            System.Collections.Generic.Queue<Transform> searchQueue = new System.Collections.Generic.Queue<Transform>();
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
        private Component FindComponentByTag(MonoBehaviour target, System.Type componentType, string tag)
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
        /// SerializedProperty'den FieldInfo çıkarır
        /// </summary>
        private FieldInfo GetFieldInfo(SerializedProperty property)
        {
            var targetType = property.serializedObject.targetObject.GetType();
            var fieldName = property.name;
            
            return targetType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif 