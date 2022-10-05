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
    
    private Dictionary<UsableSlotType, bool> _dependencyFoldout;

    private InventoryController _controller;

    private void OnEnable()
    {
        _dependencyFoldout = Utils.GetEnumValues<UsableSlotType>().ToDictionary(u => u, u => false);
    }

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
        
        slot.EquipBone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.EquipBone)), slot.EquipBone, typeof(Transform), true);

        slot.UnEquipBone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.UnEquipBone)), slot.UnEquipBone, typeof(Transform), true);

        _dependencyFoldout[pair.Key] = EditorGUILayout.Foldout(_dependencyFoldout[pair.Key], 
            Utils.GetDisplayName(nameof(slot.Dependencies)));
        
        if (_dependencyFoldout[pair.Key])
        {
            UsableSlotType[] all = Utils.GetEnumValues<UsableSlotType>().Where(e => e != pair.Key).ToArray();

            UsableSlotType[] dependencies = slot.Dependencies;
        
            foreach (var slotType in all)
            {
                bool dependent = dependencies.Contains(slotType);
            
                if (EditorGUILayout.Toggle(Utils.GetDisplayName($"{slotType}"), dependent))
                {
                    if (!dependent)
                    {
                        //Add dependency
                        slot.AddDependency(slotType);
                        //add counterpart dependency
                        _controller.Usables[slotType].AddDependency(pair.Key);
                    }
                }

                else
                {
                    if (dependent)
                    {
                        //remove dependency
                        slot.RemoveDependency(slotType);
                        //remove counterpart dependency
                        _controller.Usables[slotType].RemoveDependency(pair.Key);
                    }
                }
            }
        }
        
        pair.SetValue(slot);
        
        property.SetValue(pair);
    }
    
    private void DrawWearableSlot(SerializedProperty property)
    {
        var pair = (GenericDictionary<WearableSlotType, WearableSlot>.GenericPair) property.GetValue();

        WearableSlot slot = pair.Value;

        if (slot.controller == null) slot.controller = _controller;
        
        slot.EquipBone = (Transform) EditorGUILayout.ObjectField(Utils
            .GetDisplayName(nameof(slot.EquipBone)), slot.EquipBone, typeof(Transform), true);
        
        pair.SetValue(slot);
        
        property.SetValue(pair);
    }
}
}
