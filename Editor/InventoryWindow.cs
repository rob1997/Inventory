using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.Item;
using Inventory.Main;
using UnityEditor;
using UnityEngine;

namespace Inventory.Editor
{
    public class InventoryWindow : EditorWindow
    {
        private int _selected;

        private Vector2 _scrollPosition;

        private Item.Item _newItem;

        private int _newItemCount;

        private Main.Inventory _inventory;

        private readonly Vector2 _cellSize = new Vector2(150f, 150f);

        private float _iconSize = 75f;

        private int _margin = 25;

        private int _columns = 6;

        private GUIStyle _boldButton;

        private string _output;

        private MessageType _outputType;

        public static void Initialize(Main.Inventory inventory)
        {
            InventoryWindow window = (InventoryWindow) GetWindow(typeof(InventoryWindow));

            window._inventory = inventory;

            window.Show();
        }

        private void OnEnable()
        {
            _boldButton = new GUIStyle(GUI.skin.button) {font = EditorStyles.boldFont};
        }

        private void OnGUI()
        {
            GUIStyle box = new GUIStyle(GUI.skin.box) {margin = new RectOffset(_margin, _margin, _margin, _margin)};

            EditorGUILayout.BeginVertical(box);

            float topWidth = 250f;

            _selected = GUILayout.Toolbar(_selected, new[] {nameof(Usable), nameof(Consumable)},
                GUILayout.Width(topWidth));

            EditorGUILayout.BeginHorizontal(GUILayout.Width(topWidth));

            _newItem = (Item.Item) EditorGUILayout.ObjectField(_newItem, typeof(Item.Item), false);

            if (_newItem != null && GUILayout.Button(new GUIContent("+", "Add"), _boldButton))
            {
                bool added = Add(out _output);

                _outputType = added ? MessageType.Info : MessageType.Warning;
            }

            EditorGUILayout.EndHorizontal();

            if (_newItem != null && _newItem is Consumable consumable)
            {
                _newItemCount =
                    EditorGUILayout.IntSlider(_newItemCount, 0, consumable.limit, GUILayout.Width(topWidth));
            }

            else
                _newItemCount = 1;

            EditorGUILayout.BeginVertical(GUILayout.Width(topWidth));

            if (!string.IsNullOrEmpty(_output)) EditorGUILayout.HelpBox(_output, _outputType);

            EditorGUILayout.EndVertical();

            int length = 0;

            switch (_selected)
            {
                case 0:
                    length = _inventory.Usables.Length;
                    break;

                case 1:
                    length = _inventory.Consumables.Length;
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

        private bool Add(out string message)
        {
            bool added = false;

            message = string.Empty;

            switch (_newItem)
            {
                case Usable usable:
                    added = _inventory.AddUsable(usable, out message);
                    break;

                case Consumable consumable:
                    added = _inventory.AddConsumable(consumable, _newItemCount, out message);
                    break;
            }

            if (added) _newItem = null;

            return added;
        }

        private void Remove(int index)
        {
            switch (_selected)
            {
                case 0:
                    _inventory.RemoveUsable(index);
                    break;

                case 1:
                    _inventory.RemoveConsumable(index);
                    break;
            }
        }

        private Item.Item GetItem(int index)
        {
            switch (_selected)
            {
                case 0:
                    return _inventory.Usables[index];

                case 1:
                    return _inventory.Consumables[index].Consumable;

                default:
                    return null;
            }
        }

        private void DrawItem(int index)
        {
            Item.Item item = GetItem(index);

            bool isEmpty = item == null;

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(_cellSize.x));

            EditorGUILayout.LabelField(isEmpty ? "Empty" : item.title, EditorStyles.boldLabel);

            if (!isEmpty && GUILayout.Button(new GUIContent("X", "Remove"), _boldButton)) Remove(index);

            EditorGUILayout.EndHorizontal();

            if (isEmpty) return;

            GUILayout.Box(item.icon, GUILayout.Width(_iconSize), GUILayout.Height(_iconSize));

            EditorGUILayout.LabelField(item.description,
                //style for alignment and word wrap long text
                new GUIStyle(EditorStyles.label) {wordWrap = true, alignment = TextAnchor.UpperLeft},
                GUILayout.ExpandHeight(true));

            //if consumable
            if (_selected == 1)
            {
                //display count and limit
                Pouch pouch = _inventory.Consumables[index];

                if (!pouch.IsEmpty) pouch.Resize(EditorGUILayout.IntSlider(pouch.Count, 0, pouch.Limit));
            }

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.ObjectField(item, typeof(Item.Item), false);

            EditorGUI.EndDisabledGroup();
        }
    }
}