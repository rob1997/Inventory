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
        
        slot.equipBone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.equipBone)), slot.equipBone, typeof(Transform), true);
        
        EditorGUILayout.Space(1.25f);
        
        slot.unEquipBone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.unEquipBone)), slot.unEquipBone, typeof(Transform), true);
        
        pair.SetValue(slot);
        
        property.SetValue(pair);
    }
    
    private void DrawWearableSlot(SerializedProperty property)
    {
        var pair = (GenericDictionary<WearableSlotType, WearableSlot>.GenericPair) property.GetValue();

        WearableSlot slot = pair.Value;
        
        slot.equipBone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.equipBone)), slot.equipBone, typeof(Transform), true);
        
        EditorGUILayout.Space(1.25f);
        
        slot.unEquipBone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.unEquipBone)), slot.unEquipBone, typeof(Transform), true);
        
        pair.SetValue(slot);
        
        property.SetValue(pair);
    }
}
}
