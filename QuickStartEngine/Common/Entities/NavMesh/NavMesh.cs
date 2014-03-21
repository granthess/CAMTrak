using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Input;

using QuickStart;
using QuickStart.Components;
using QuickStart.Graphics;
using QuickStart.Entities.Heightfield;

namespace QuickStart.Entities.NavMesh
{
    public class NavMesh : BaseEntity
    {
        private Dictionary<int, NavMeshChunk> meshChunks;  // All of the chunks that make up the nav-mesh

        public BaseEntity TerrainEntity
        {
            get { return this.terrain; }
        }
        private BaseEntity terrain;

        public TerrainComponent TerrainComp
        {
            get { return this.terrainComp; }
        }
        private TerrainComponent terrainComp;

        private NavMeshGenerator meshGenerator;

        private bool displayBoundingBoxes = false;

        public NavMesh(QSGame game)
            : base(game)
        {
            // Register to receive messages
            this.Game.GameMessage += this.Game_GameMessage;

            meshGenerator = new NavMeshGenerator(this);            
        }

        protected override void Game_GameMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.GenerateNavMesh:
                    {
                        // If we've already generated a nav mesh, then do nothing.
                        if (meshGenerator.MeshGenerated)
                            return;

                        CreateNavMesh();
                    }
                    break;
                case MessageType.KeyDown:
                    {
                        MsgKeyPressed msgKeyPressed = message as MsgKeyPressed;
                        message.TypeCheck(msgKeyPressed);

                        if (msgKeyPressed.Key == Keys.P)
                        {
                            // Toggle display of bounding boxes
                            displayBoundingBoxes = !displayBoundingBoxes;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void CreateNavMesh()
        {
            MsgGetTerrainEntity getTerrainMsg = ObjectPool.Aquire<MsgGetTerrainEntity>();
            getTerrainMsg.UniqueTarget = this.Game.TerrainID;       
            this.Game.SendMessage(getTerrainMsg);

            if (getTerrainMsg.TerrainEntity == null)
            {
                throw new Exception("The terrain must be initialized before a NavMesh can be generated");
            }

            this.terrain = getTerrainMsg.TerrainEntity;

            this.terrainComp = this.terrain.GetComponentByType(ComponentType.TerrainComponent) as TerrainComponent;

            meshGenerator.GenerateNavMesh(this.terrain);

            if (meshGenerator.MeshGenerated)
            {
                meshChunks = meshGenerator.chunkDictionary;
                meshGenerator.nodeDictionary.Clear();
                meshGenerator.nodeDictionary = null;

                foreach (NavMeshChunk chunk in meshChunks.Values)
                {
                    chunk.UpdateAABB();
                }
            }
        }

        public override void QueryForRenderChunks(ref RenderPassDesc desc)
        {
            if (displayBoundingBoxes)
            {
                foreach (NavMeshChunk chunk in meshChunks.Values)
                {
                    chunk.BoundingBoxDrawnThisFrame = false;
                }

                foreach (NavMeshChunk chunk in meshChunks.Values)
                {
                    if (!chunk.BoundingBoxDrawnThisFrame)
                    {
                        chunk.QueryForRenderChunks(ref desc);
                    }
                }
            }
        }
    }
}
