/*
 * TerrainPatch.cs
 * 
 * This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
 * for license details.
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.Components;
using QuickStart.Graphics;

namespace QuickStart.Entities.Heightfield
{
    /// <summary>
    /// A terrain patch is really just an index buffer representing a section of terrain for a <see cref="QuadTree"/> section.
    /// A single patch can hold any level of <see cref="LOD"/>s. A lower LOD will result in drawing less triangles for a patch, but
    /// will also result in a lower quality patch.
    /// </summary>
    public class TerrainPatch : BaseEntity
    {
        /// <summary>
        /// <see cref="Terrain"/> section this patch belongs to.
        /// </summary>
        private TerrainComponent terrain;

        /// <summary>
        /// <see cref="QuadTree"/> section this patch belongs to.
        /// </summary>
        private QuadTree parentQuadTree;

        /// <summary>
        /// Index buffer for this patch. As an array it can hold an index buffer for each LOD.
        /// </summary>
        public IndexBuffer[] indexBuffers;

        /// <summary>
        /// Holds the number of triangles in this patch. As an array it can hold triangle information
        /// for multiple LODs for this patch.
        /// </summary>
        public int[] numTris;      

        /// <summary>
        /// Create a terrain patch.
        /// </summary>
        /// <param name="game">QSGame reference</param>
        /// <param name="terrain">Terrain section this patch belongs to</param>
        /// <param name="parentQuadTree">Parent QuadTree section</param>
        public TerrainPatch(QSGame game, TerrainComponent terrain, QuadTree parentQuadTree)
            : base(game)
        {
            this.terrain = terrain;
            this.parentQuadTree = parentQuadTree;

            // Create an array index of 4 for each, as 4 is our lowest detail level
            indexBuffers = new IndexBuffer[(int)LOD.NumOfLODs];

            numTris = new int[(int)LOD.NumOfLODs];

            // Setup patch with the highest LOD available
            SetupTerrainIndices(parentQuadTree.Width, terrain.Detail);
        }

        /// <summary>
        /// Setup terrain index buffer for this patch, as a chosen level of detail.
        /// </summary>
        /// <param name="width">Width of this patch</param>
        public void SetupTerrainIndices(int width)
        {            
            // No LOD was chosen in this setup call, so default to High LOD.
            if (this.Game.Graphics.Settings.Uses32BitIndices)
            {
                SetupTerrainIndices32Bit(width, LOD.High);
            }
            else
            {
                SetupTerrainIndices16Bit(width, LOD.High);
            }
        }

        /// <summary>
        /// Setup terrain index buffer for this patch, as a chosen level of detail.
        /// </summary>
        /// <param name="width">Width of this patch</param>
        /// <param name="detailLevel">Level of detail for this patch</param>
        public void SetupTerrainIndices(int width, LOD? detailLevel)
        {
            if (detailLevel == null)
            {
                detailLevel = LOD.High;
            }

            // No LOD was chosen in this setup call, so default to High LOD.
            if (this.Game.Graphics.Settings.Uses32BitIndices)
            {
                SetupTerrainIndices32Bit(width, detailLevel);
            }
            else
            {
                SetupTerrainIndices16Bit(width, detailLevel);
            }
        }

        /// <summary>
        /// Setup 16-bit terrain index buffer for this patch, as a chosen level of detail.
        /// </summary>
        /// <param name="width">Width of this patch</param>
        /// <param name="detailLevel">Level of detail for this patch</param>
        private void SetupTerrainIndices16Bit(int width, LOD? detailLevel)
        {
            if (detailLevel == null)
            {
                detailLevel = LOD.High;
            }

            int detail = (int)detailLevel;
            int widthMinusOne = (width - 1);            

            // If detail level is smaller than the quad patch, then move up to
            // the next highest detail level.
            if (detail >= widthMinusOne)
            {
                detail /= 2;
            }

            int widthMinusOneDivDetail = (width - 1) / detail;
            short[] indices = new short[widthMinusOne * widthMinusOne * 6 / (detail * detail)];

            for (int x = 0; x < widthMinusOneDivDetail; ++x)
                for (int y = 0; y < widthMinusOneDivDetail; ++y)
                {
                    int triStartIndex = (x + y * widthMinusOneDivDetail) * 6;
                    int xPlusOne = x + 1;
                    int yTimesWidth = y * width;
                    int yPlusOneTimesWidth = (y + 1) * width;

                    indices[triStartIndex] = (short)((xPlusOne + yPlusOneTimesWidth) * detail);
                    indices[triStartIndex + 1] = (short)((xPlusOne + yTimesWidth) * detail);
                    indices[triStartIndex + 2] = (short)((x + yTimesWidth) * detail);

                    indices[triStartIndex + 3] = indices[triStartIndex];
                    indices[triStartIndex + 4] = indices[triStartIndex + 2];
                    indices[triStartIndex + 5] = (short)((x + yPlusOneTimesWidth) * detail);
                }

            int widthMinusDetail = (width - detail);
            indexBuffers[detail] = new IndexBuffer(Game.GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
            indexBuffers[detail].SetData(indices);

            numTris[detail] = indices.Length / 3;
        }

        /// <summary>
        /// Setup 32-bit terrain index buffer for this patch, as a chosen level of detail.
        /// </summary>
        /// <param name="width">Width of this patch</param>
        /// <param name="detailLevel">Level of detail for this patch</param>
        private void SetupTerrainIndices32Bit(int width, LOD? detailLevel)
        {
            if (detailLevel == null)
            {
                detailLevel = LOD.High;
            }

            int detail = (int)detailLevel;
            int widthMinusOne = (width - 1);

            // If detail level is smaller than the quad patch, then move up to
            // the next highest detail level.
            if (detail >= widthMinusOne)
            {
                detail /= 2;
            }

            int widthMinusOneDivDetail = widthMinusOne / detail;
            int[] indices = new int[widthMinusOne * widthMinusOne * 6 / (detail * detail)];

            for (int x = 0; x < widthMinusOneDivDetail; ++x)
                for (int y = 0; y < widthMinusOneDivDetail; ++y)
                {
                    int triStartIndex = (x + y * widthMinusOneDivDetail) * 6;
                    int xPlusOne = x + 1;
                    int yTimesWidth = y * width;
                    int yPlusOneTimesWidth = (y + 1) * width;

                    indices[triStartIndex] = (xPlusOne + yPlusOneTimesWidth) * detail;
                    indices[triStartIndex + 1] = (xPlusOne + yTimesWidth) * detail;
                    indices[triStartIndex + 2] = (x + yTimesWidth) * detail;

                    indices[triStartIndex + 3] = indices[triStartIndex];
                    indices[triStartIndex + 4] = indices[triStartIndex + 2];
                    indices[triStartIndex + 5] = (x + yPlusOneTimesWidth) * detail;
                }

            int widthMinusDetail = (width - detail);
            indexBuffers[detail] = new IndexBuffer(Game.GraphicsDevice, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffers[detail].SetData(indices);

            numTris[detail] = indices.Length / 3;
        }
    }    
}
