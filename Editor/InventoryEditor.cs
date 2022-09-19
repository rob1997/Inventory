using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Inventory.Editor
{
    [CustomEditor(typeof(Main.Inventory))]
    public class InventoryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            Main.Inventory inventory = (Main.Inventory) target;

            if (inventory == null) return;
        
            if (GUILayout.Button(new GUIContent("Inventory", "Opens Inventory Window")))
            {
                inventory.Initialize();
            
                InventoryWindow.Initialize(inventory);
            }
        
            EditorGUILayout.Space(5f);

            inventory.pouches = Mathf.Clamp(EditorGUILayout
                .IntField(new GUIContent("Pouches", "Total Consumable Holder Inventory Slots"), inventory.pouches), 0, int.MaxValue);
        
            inventory.slots = Mathf.Clamp(EditorGUILayout
                .IntField(new GUIContent("Slots", "Total Usable Inventory Slots"), inventory.slots), 0, int.MaxValue);
        
            inventory.limit = Mathf.Clamp(EditorGUILayout
                .FloatField(new GUIContent("Limit", "Weight Limit for Inventory"), inventory.limit), 0, int.MaxValue);
        }
    }
}
