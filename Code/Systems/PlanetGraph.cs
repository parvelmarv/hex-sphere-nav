using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for sorting

namespace _Scripts.Systems.HexGridBuildSystem
{
    public class PlanetGraph : MonoBehaviour
    {
        public List<PlanetTile> tiles = new List<PlanetTile>();

        void Awake()
        {
            GenerateGraph();
        }

        private void GenerateGraph()
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;
            
            List<List<int>> tileGroups = new List<List<int>>();
            List<Vector3> groupNormals = new List<Vector3>();

            // 1. Group triangles into raw data buckets
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 n1 = normals[triangles[i]];
                Vector3 n2 = normals[triangles[i + 1]];
                Vector3 n3 = normals[triangles[i + 2]];
                Vector3 triNormal = ((n1 + n2 + n3) / 3f).normalized;

                bool foundGroup = false;
                for (int g = 0; g < groupNormals.Count; g++)
                {
                    if (Vector3.Dot(groupNormals[g], triNormal) > 0.95f) 
                    {
                        tileGroups[g].Add(i);
                        foundGroup = true;
                        break;
                    }
                }

                if (!foundGroup)
                {
                    tileGroups.Add(new List<int> { i });
                    groupNormals.Add(triNormal);
                }
            }

            // 2. Create Tile objects WITHOUT assigning final IDs yet
            List<PlanetTile> tempTiles = new List<PlanetTile>();
            foreach (var groupIndices in tileGroups)
            {
                PlanetTile tile = new PlanetTile();
                Vector3 normalSum = Vector3.zero;
                Vector3 centerSum = Vector3.zero;
                int vertexCount = 0;
                HashSet<int> uniqueVerIndices = new HashSet<int>();

                foreach (int triIndex in groupIndices)
                {
                    Vector3 n1 = normals[triangles[triIndex]];
                    Vector3 n2 = normals[triangles[triIndex + 1]];
                    Vector3 n3 = normals[triangles[triIndex + 2]];
                    normalSum += ((n1 + n2 + n3) / 3f).normalized;

                    for (int j = 0; j < 3; j++)
                    {
                        int vertIndex = triangles[triIndex + j];
                        if (uniqueVerIndices.Add(vertIndex))
                        {
                            centerSum += vertices[vertIndex];
                            vertexCount++;
                        }
                    }
                }
                
                tile.LocalNormal = (normalSum / groupIndices.Count).normalized;
                tile.LocalCenter = centerSum / vertexCount;
                tempTiles.Add(tile);
            }

            // --- THE FIX: SORTING ---
            // Sort by Y descending (Top to Bottom), then by angle around Y (spiral order)
            tiles = tempTiles
                .OrderByDescending(t => t.LocalCenter.y)
                .ThenBy(t => Mathf.Atan2(t.LocalCenter.x, t.LocalCenter.z))
                .ToList();

            // Assign final IDs based on the new sorted order
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].ID = i;
            }

            // 3. Neighbor calculation (must happen AFTER sorting so tiles[0] is the top)
            float connectionThreshold = 2.0f;
            if (tiles.Count > 1)
            {
                float closestDist = float.MaxValue;
                for (int i = 1; i < Mathf.Min(tiles.Count, 50); i++)
                {
                    float d = Vector3.Distance(tiles[0].LocalCenter, tiles[i].LocalCenter);
                    if (d < closestDist) closestDist = d;
                }
                connectionThreshold = closestDist * 1.3f;
            }

            foreach (var tile in tiles)
            {
                tile.Neighbors.Clear();
                foreach (var potentialNeighbor in tiles)
                {
                    if (tile == potentialNeighbor) continue;
                    if (Vector3.Distance(tile.LocalCenter, potentialNeighbor.LocalCenter) < connectionThreshold)
                    {
                        tile.Neighbors.Add(potentialNeighbor);
                    }
                }
            }
        }
    }
}