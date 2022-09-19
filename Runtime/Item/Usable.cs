using System.Collections;
using System.Collections.Generic;
using Core.Character;
using UnityEngine;

namespace Inventory.Item
{
    public abstract class Usable : Item
    {
        public abstract void Use(Character character);
    }
}
