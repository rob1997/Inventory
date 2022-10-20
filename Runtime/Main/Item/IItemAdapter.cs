using System;
using System.Collections;
using System.Collections.Generic;
using Core.Character;
using UnityEngine;

namespace Inventory.Main.Item
{
    public interface IItemAdapter
    {
        IItem Item { get; }
        
        GameObject Obj { get; }
        
        Character Character { get; }

        bool Initialized { get; }
        
        //just initializes/assigns the values
        void StartWith(IItem item, Character character);
        
        /// <summary>
        /// Initialize adapter
        /// </summary>
        /// <param name="item">item for the adapter</param>
        /// <param name="character">character holding the item, if null means item is dropped</param>
        void Initialize(IItem item, Character character);

        void Focus();

        void Pick(bool picked, string message);
    }
}
