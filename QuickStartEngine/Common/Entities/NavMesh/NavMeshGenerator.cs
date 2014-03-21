using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using QuickStart;
using QuickStart.Components;
using QuickStart.Entities.Heightfield;

namespace QuickStart.Entities.NavMesh
{
    class NavMeshGenerator
    {
        private NavMesh parent;
        public BaseEntity terrain;
        public TerrainComponent terrainComp;

        public bool MeshGenerated
        {
            get { return meshGenerated; }
        }
        private bool meshGenerated = false;
        
        public Dictionary<int, NavMeshNode> nodeDictionary;
        public Dictionary<int, NavMeshChunk> chunkDictionary;

        public NavMeshGenerator(NavMesh parent)
        {
            this.parent = parent;

            nodeDictionary = new Dictionary<int, NavMeshNode>();
            chunkDictionary = new Dictionary<int, NavMeshChunk>();
        }

        public void GenerateNavMesh(BaseEntity terrain)
        {
            this.terrain = terrain;
            this.terrainComp = this.terrain.GetComponentByType(ComponentType.TerrainComponent) as TerrainComponent;

            // Process each pixel of the heightmap as a navmeshnode.
                // Create a navmeshnode, even if the slope is too great.
            int width = this.terrainComp.Size;

            int widthMinusOne = width - 1;
            
            // Find four vertexs that comprise a node
            for (int x = 0; x < widthMinusOne; ++x)
                for (int z = 0; z < widthMinusOne; ++z)
                {
                    NavMeshNode newNode = new NavMeshNode( (x + (z * width)), width );

                    // Grab the normal of the quad that makes up this NavMeshNode, because the terrain engine organizes triangles into
                    //   quads, we don't need to check both triangles in the quad, as they'll have the same normal. So, any corner of the
                    //   quad will have the same normal, we'll just take the topleft corner's normal.
                    newNode.Normal = this.terrainComp.normals[newNode.ZCoordTopLeft, newNode.XCoordTopLeft];

                    nodeDictionary.Add( newNode.VertexTopLeft, newNode );
                }

            // Postprocess step, optimize navmesh by combining navmeshnodes into navmeshchunks.
            for (int x = 0; x < widthMinusOne; ++x)
                for (int z = 0; z < widthMinusOne; ++z)
                {
                    int topLeftVertex = x + (z * width);                   

                    // Check if there is a node at this [x, z] location, if there isn't then it has already
                    // been processed into a chunk
                    if ( nodeDictionary.ContainsKey(topLeftVertex) )
                    {
                        NavMeshNode node;
                        nodeDictionary.TryGetValue(topLeftVertex, out node);

                        if (node != null)
                        {
                            NavMeshChunk newChunk = new NavMeshChunk(this.parent);

                            ProcessNewChunk(newChunk, node);                        
                        }
                        else
                        {
                            throw new Exception("Failed to find a value in 'nodeDictionary'");
                        }
                    }

                
                }
            
            // Once all chunks are created, we need to go through each chunk, and create a list of connection points between chunks so we know how
            // to navigate from chunk to chunk.

            meshGenerated = true;
        }

        private void ProcessNewChunk(NavMeshChunk chunk, NavMeshNode startingNode)
        {
            bool columnsAvail = true;
            bool rowsAvail = true;

            chunk.AddNode(startingNode, true);

            chunkDictionary.Add(startingNode.VertexTopLeft, chunk);
            nodeDictionary.Remove(startingNode.VertexTopLeft);

            if (startingNode.TooSteep)
                return;

            //// TEMP!!!!!!!!!!!!!!!!!!!!
            //if (startingNode.VertexTopLeft == 0)
            //{
            //    int i = 5;
            //    i++;
            //}

            while (columnsAvail || rowsAvail)
            {
                if (columnsAvail)
                {
                    columnsAvail = AddNewColumnToChunk(chunk);
                }

                if (rowsAvail)
                {
                    rowsAvail = AddNewRowToChunk(chunk);
                }
            }

            // Once we've made it here, that means the current chunk cannot expand any more
        }

        /// <summary>
        /// Attempts to add a new column to a NavMeshChunk
        /// </summary>
        /// <param name="chunk">Chunk to add a new column to</param>
        /// <returns>Whether or not the column was added</returns>
        private bool AddNewColumnToChunk(NavMeshChunk chunk)
        {
            // Is this chunk's max column already at the end of the map
            if (chunk.GetMaxColumn() == this.terrainComp.Size - 1)
                return false;

            int column = chunk.GetMaxColumn() + 1;
            int startingRow = chunk.GetMinRow();
            int endRow = chunk.GetMaxRow();

            List<NavMeshNode> nodesToAdd = new List<NavMeshNode>();

            for (int z = startingRow; z <= endRow; ++z)
            {
                int vertexTopLeft = column + (z * this.terrainComp.Size);

                NavMeshNode node;
                nodeDictionary.TryGetValue(vertexTopLeft, out node);

                // Node doesn't exist (was already chunked), so we fail at getting a new column
                if (node == null)
                    return false;

                // If node is too steep then it cannot be part of this chunk
                if (node.TooSteep)
                    return false;

                nodesToAdd.Add(node);
            }

            // If we made it this far, lets add the column into the chunk
            foreach (NavMeshNode node in nodesToAdd)
            {
                chunk.AddNode(node, true);
                chunkDictionary.Add(node.VertexTopLeft, chunk);
                nodeDictionary.Remove(node.VertexTopLeft);
            }            
            
            return true;
        }

        /// <summary>
        /// Attempts to add a new row to a NavMeshChunk
        /// </summary>
        /// <param name="chunk">Chunk to add a new row to</param>
        /// <returns>Whether or not the row was added</returns>
        private bool AddNewRowToChunk(NavMeshChunk chunk)
        {
            // Is this chunk's max column already at the end of the map
            if (chunk.GetMaxRow() == this.terrainComp.Size - 1)
                return false;

            int row = chunk.GetMaxRow() + 1;
            int startingColumn = chunk.GetMinColumn();
            int endColumn = chunk.GetMaxColumn();

            List<NavMeshNode> nodesToAdd = new List<NavMeshNode>();

            for (int x = startingColumn; x <= endColumn; ++x)
            {
                int vertexTopLeft = x + (row * this.terrainComp.Size);

                NavMeshNode node;
                nodeDictionary.TryGetValue(vertexTopLeft, out node);

                // Node doesn't exist (was already chunked), so we fail at getting a new column
                if (node == null)
                    return false;

                // If node is too steep then it cannot be part of this chunk
                if (node.TooSteep)
                    return false;

                nodesToAdd.Add(node);
            }

            // If we made it this far, lets add the column into the chunk
            foreach (NavMeshNode node in nodesToAdd)
            {
                chunk.AddNode(node, true);
                chunkDictionary.Add(node.VertexTopLeft, chunk);
                nodeDictionary.Remove(node.VertexTopLeft);
            }

            return true;
        }
    }
}
