using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using Editor.Core;
using Inventory.Main;
using Inventory.Main.Slot;
using UnityEditor;
using UnityEngine;

namespace Inventory.Editor
{
    [CustomEditor(typeof(InventoryController))]
public class InventoryControllerEditor : UnityEditor.Editor
{
    private bool _usablesFoldout;
    
    private bool _wearablesFoldout;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        InventoryController controller = (InventoryController) target;

        if (GUILayout.Button(new GUIContent(nameof(Bag), "Opens Bag Window")))
        {
            controller.Bag.Initialize();
            
            BagWindow.Initialize(controller);
        }
        
        DrawUsableDict(controller);
        
        DrawWearableDict(controller);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawUsableDict(InventoryController controller)
    {
        SerializedProperty usableDictProperty = serializedObject.FindProperty(InventoryController.UsableName);
        
        _usablesFoldout = EditorGUILayout.Foldout(_usablesFoldout, usableDictProperty.displayName);

        if (!_usablesFoldout) return;

        var usables = controller.Usables;
        
        BaseEditor.DrawEnumDict(usableDictProperty, ref usables, DrawUsableSlot);
    }
    
    private void DrawWearableDict(InventoryController controller)
    {
        SerializedProperty wearableDictProperty = serializedObject.FindProperty(InventoryController.WearableName);
        
        _wearablesFoldout = EditorGUILayout.Foldout(_wearablesFoldout, wearableDictProperty.displayName);

        if (!_wearablesFoldout) return;

        var wearables = controller.Wearables;
        
        BaseEditor.DrawEnumDict(wearableDictProperty, ref wearables, DrawWearableSlot);
    }
    
    private void DrawUsableSlot(SerializedProperty property)
    {
        var pair = (GenericDictionary<UsableSlotType, UsableSlot>.GenericPair) property.GetValue();

        UsableSlot slot = pair.Value;
        
        slot.bone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.bone)), slot.bone, typeof(Transform), true);

        if (slot.IsEquipped)
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.ObjectField(slot.adapter.Obj, typeof(GameObject), true);

            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button(new GUIContent("UnEquip", "UnEquips Gear")))
            {
                bool unequipped = slot.UnEquip(out string message);
                
                if (unequipped) Debug.Log(message);

                else Debug.LogError(message);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        else
        {
            EditorGUILayout.LabelField("Not Equipped");
        }
        
        pair.SetValue(slot);
        
        property.SetValue(pair);
    }
    
    private void DrawWearableSlot(SerializedProperty property)
    {
        var pair = (GenericDictionary<WearableSlotType, WearableSlot>.GenericPair) property.GetValue();

        WearableSlot slot = pair.Value;
        
        slot.bone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.bone)), slot.bone, typeof(Transform), true);

        if (slot.IsEquipped)
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.ObjectField(slot.adapter.Obj, typeof(GameObject), true);

            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button(new GUIContent("UnEquip", "UnEquips Gear")))
            {
                bool unequipped = slot.UnEquip(out string message);
                
                if (unequipped) Debug.Log(message);

                else Debug.LogError(message);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        else
        {
            EditorGUILayout.LabelField("Not Equipped");
        }
        
        pair.SetValue(slot);
        
        property.SetValue(pair);
    }
}
}
