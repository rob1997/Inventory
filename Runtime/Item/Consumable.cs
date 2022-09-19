using System.Collections;
using System.Collections.Generic;
using Inventory.Main;
using UnityEngine;

namespace Inventory.Item
{
    public abstract class Consumable : Item
    {
        public int limit;

        public abstract void Consume(IConsumer consumer);
    }
}
