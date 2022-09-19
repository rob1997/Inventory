using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory.Item;
using UnityEngine;

namespace Inventory.Main
{
    [CreateAssetMenu(order = 0, fileName = nameof(Inventory), menuName = "Inventory/Inventory")]
    public class Inventory : ScriptableObject
    {
        public int slots = 25;

        public int pouches = 15;

        public float limit;

        private bool _initialized;

        private Pouch[] _consumables;

        private Usable[] _usables;

        private const string TooHeavyMessage = "Inventory Limit Exceeded, Too Heavy";

        private const string AddedMessage = "Item Added Successfully";

        public Pouch[] Consumables => _consumables;

        public Usable[] Usables => _usables;

        public float Weight =>
            Consumables.Where(c => !c.IsEmpty).Sum(c => c.Consumable.weight) +
            Usables.Where(u => u != null).Sum(u => u.weight);

        public void Initialize()
        {
            //TODO: load from disk (persistent source)

            if (_initialized) return;

            _consumables = new Pouch[pouches];

            for (int i = 0; i < _consumables.Length; i++) _consumables[i] = new Pouch();

            _usables = new Usable[slots];

            _initialized = true;
        }

        public bool AddConsumable(Consumable consumable, int count, out string message)
        {
            message = string.Empty;

            if (Weight + consumable.weight > limit)
            {
                message = TooHeavyMessage;

                return false;
            }

            int index = FindPouch(consumable);

            if (index == -1)
            {
                message = "All Inventory Pouches Filled";

                return false;
            }

            Pouch pouch = _consumables[index];

            if (pouch.Consumable == null)
            {
                pouch.Fill(consumable, count);
            }

            else
                pouch.Add(count);

            message = AddedMessage;

            return true;
        }

        public bool AddUsable(Usable usable, out string message)
        {
            message = string.Empty;

            if (Weight + usable.weight > limit)
            {
                message = TooHeavyMessage;

                return false;
            }

            int index = FindEmptySlot();

            if (index == -1)
            {
                message = "All Inventory Slots Filled";

                return false;
            }

            _usables[index] = usable;

            message = AddedMessage;

            return true;
        }

        public void RemoveConsumable(int index)
        {
            Consumables[index].Clear();
        }

        public void RemoveUsable(int index)
        {
            Usables[index] = null;
        }

        public int FindEmptySlot()
        {
            return Array.FindIndex(Usables, u => u == null);
        }

        public int FindPouch(Consumable consumable)
        {
            int index = Array.FindIndex(Consumables, c => c.Consumable == consumable);

            if (index == -1) index = FindEmptyPouch();

            return index;
        }

        public int FindEmptyPouch()
        {
            return Array.FindIndex(Consumables, c => c.IsEmpty);
        }
    }
}