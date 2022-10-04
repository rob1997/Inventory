using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private InventoryController _controller;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (_controller == null) _controller = (InventoryController) target;

        if (GUILayout.Button(new GUIContent(nameof(Bag), "Opens Bag Window")))
        {
            _controller.Bag.Initialize();
            
            BagWindow.Initialize(_controller);
        }
        
        DrawUsableDict();
        
        DrawWearableDict();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawUsableDict()
    {
        SerializedProperty usableDictProperty = serializedObject.FindProperty(InventoryController.UsableName);
        
        _usablesFoldout = EditorGUILayout.Foldout(_usablesFoldout, usableDictProperty.displayName);

        if (!_usablesFoldout) return;

        var usables = _controller.Usables;
        
        BaseEditor.DrawEnumDict(usableDictProperty, ref usables, DrawUsableSlot);
    }
    
    private void DrawWearableDict()
    {
        SerializedProperty wearableDictProperty = serializedObject.FindProperty(InventoryController.WearableName);
        
        _wearablesFoldout = EditorGUILayout.Foldout(_wearablesFoldout, wearableDictProperty.displayName);

        if (!_wearablesFoldout) return;

        var wearables = _controller.Wearables;
        
        BaseEditor.DrawEnumDict(wearableDictProperty, ref wearables, DrawWearableSlot);
    }
    
    private void DrawUsableSlot(SerializedProperty property)
    {
        var pair = (GenericDictionary<UsableSlotType, UsableSlot>.GenericPair) property.GetValue();

        UsableSlot slot = pair.Value;
        
        if (slot.controller == null) slot.controller = _controller;
        
        slot.equipBone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.equipBone)), slot.equipBone, typeof(Transform), true);
        
        EditorGUILayout.Space(1.25f);
        
        slot.unEquipBone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.unEquipBone)), slot.unEquipBone, typeof(Transform), true);

        UsableSlotType[] allSlots = Utils.GetEnumValues<UsableSlotType>().Where(e => e != pair.Key).ToArray();

        UsableSlotType[] dependencies = slot.dependencies;
        
        foreach (var slotType in allSlots)
        {
            bool hasDependency = dependencies.Contains(slotType);
            
            if (EditorGUILayout.Toggle(slotType.ToString(), hasDependency))
            {
                if (!hasDependency)
                {
                    //Add dependency
                    slot.dependencies = slot.dependencies.Append(slotType).ToArray();
                    
                    //add counterpart dependency
                    _controller.Usables[slotType].dependencies =
                        _controller.Usables[slotType].dependencies.Append(pair.Key).ToArray();
                }
            }

            else
            {
                if (hasDependency)
                {
                    //remove dependency
                    slot.dependencies = slot.dependencies.Where(d => d != slotType).ToArray();
                    
                    //remove counterpart dependency
                    _controller.Usables[slotType].dependencies =
                        _controller.Usables[slotType].dependencies.Where(d => d != pair.Key).ToArray();
                }
            }
        }
        
        //EditorGUILayout.PropertyField(property.FindPropertyRelative(BaseEditor.ValueName).FindPropertyRelative(nameof(UsableSlot.dependencies)));
        
        pair.SetValue(slot);
        
        property.SetValue(pair);
    }
    
    private void DrawWearableSlot(SerializedProperty property)
    {
        var pair = (GenericDictionary<WearableSlotType, WearableSlot>.GenericPair) property.GetValue();

        WearableSlot slot = pair.Value;

        if (slot.controller == null) slot.controller = _controller;
        
        slot.equipBone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.equipBone)), slot.equipBone, typeof(Transform), true);
        
        pair.SetValue(slot);
        
        property.SetValue(pair);
    }
}
}
