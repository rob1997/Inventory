using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Item
{
    public class Item : ScriptableObject
    {
        public string title;
    
        public string description;
    
        public Texture2D icon;
    
        public float weight;
    
        public GameObject prefab;
    }
}
