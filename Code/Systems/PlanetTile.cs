using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Systems.HexGridBuildSystem
{
    [System.Serializable]
    public class PlanetTile
    {
        public int ID;
        public Vector3 LocalCenter;
        public Vector3 LocalNormal;
        
        
        [System.NonSerialized]
        public List<PlanetTile>  Neighbors = new List<PlanetTile>();
        
        public bool isOccupied;
        public GameObject Occupier;
    }
}