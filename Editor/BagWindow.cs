using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.Main.Item;
using Inventory.Main;
using Inventory.Main.Slot;
using UnityEditor;
using UnityEngine;

namespace Inventory.Editor
{
    public class BagWindow : EditorWindow
{
    private int _selected;

    private Vector2 _scrollPosition;

    private readonly Vector2 _cellSize = new Vector2(150f, 150f);

    private float _iconSize = 75f;

    private int _margin = 25;

    private int _columns = 6;

    private GUIStyle _boldButton;
    
    private Bag _bag;
    
    private InventoryController _controller;
    
    public static void Initialize(InventoryController controller)
    {
        BagWindow window = GetWindow<BagWindow>($"{nameof(Bag)} Window");

        window._bag = controller.Bag;

        window._controller = controller;
        
        window.Show();
    }
    
    private void OnGUI()
    {
        if (_boldButton == null) _boldButton = new GUIStyle(GUI.skin.button) { font = EditorStyles.boldFont };
        
        GUIStyle box = new GUIStyle(GUI.skin.box) { margin = new RectOffset(_margin, _margin, _margin, _margin) };
        
        EditorGUILayout.BeginVertical(box);

        float topWidth = 250f;

        _selected = GUILayout.Toolbar(_selected, new[]
        {
            nameof(Gear<GearReference>), nameof(Supplement<SupplementReference>)
        }, GUILayout.Width(topWidth));
        
        int length = 0;

        switch (_selected)
        {
            case 0:
                length = _bag.Gears.Length;
                break;

            case 1:
                length = _bag.Supplements.Length;
                break;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        int rows = Mathf.CeilToInt((float) length / _columns);

        for (int i = 0; i < rows; i++)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(_cellSize.y));

            for (int j = 0; j < _columns; j++)
            {
                int index = i * _columns + j;

                if (index > length - 1) break;

                GUILayout.BeginVertical(box, GUILayout.MaxWidth(_cellSize.x));

                DrawItem(index);

                GUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.EndVertical();
    }
    
    private IItem GetItem(int index)
    {
        switch (_selected)
        {
            case 0:
                return _bag.Gears[index];

            case 1:
                return _bag.Supplements[index];

            default:
                return null;
        }
    }

    private void DrawItem(int index)
    {
        IItem item = GetItem(index);
        
        ItemReference reference = item?.Reference;

        bool isEmpty = item == null || reference == null;
        
        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(_cellSize.x));

        EditorGUILayout.LabelField(isEmpty ? "Empty" : reference.Title, EditorStyles.boldLabel);

        if (!isEmpty && GUILayout.Button(new GUIContent("X", "Remove"), _boldButton)) Remove(index);

        EditorGUILayout.EndHorizontal();

        if (isEmpty) return;

        GUILayout.Box(reference.Icon, GUILayout.Width(_iconSize), GUILayout.Height(_iconSize));

        EditorGUILayout.LabelField(reference.Description,
            //style for alignment and word wrap long text
            new GUIStyle(EditorStyles.label) {wordWrap = true, alignment = TextAnchor.UpperLeft},
            GUILayout.ExpandHeight(true));

        //if supplement
        if (_selected == 1)
        {
            //display count and limit
            ISupplement supplement = (ISupplement) item;

            int limit = ((SupplementReference) item.Reference).Limit;

            supplement.Resize(EditorGUILayout.IntSlider(supplement.Count, 0, limit));
        }
        
        EditorGUILayout.BeginHorizontal();
        
        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.ObjectField(reference, typeof(ItemReference), false);

        EditorGUI.EndDisabledGroup();
        
        //if gear
        if (_selected == 0)
        {
            IGear gear = (IGear) item;

            if (gear.IsEquipped)
            {
                if (GUILayout.Button(new GUIContent("UnEquip", "UnEquips Gear")))
                {
                    bool unequipped = _controller.UnEquip(gear, out string message);
                    
                    if (unequipped) Debug.Log(message);

                    else Debug.LogError(message);
                }
            }

            else
            {
                if (GUILayout.Button(new GUIContent("Equip", "Equips Gear")))
                {
                    bool equipped = _controller.Equip(index, out string message);

                    if (equipped) Debug.Log(message);

                    else Debug.LogError(message);
                }
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void Remove(int index)
    {
        switch (_selected)
        {
            case 0:
                _bag.RemoveGear(index);
                break;

            case 1:
                _bag.RemoveSupplement(index);
                break;
        }
    }
}
}
