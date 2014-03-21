﻿using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.Graphics;

namespace QuickStart.GeometricPrimitives
{
    public enum GeometricPrimitiveType
    {
        Invalid,
        Box,
        Sphere,
        Cylinder,
        Cone,
        Torus,
        Teapot,        
    }

    /// <summary>
    /// Base class for simple geometric primitive models. This provides a vertex
    /// buffer, an index buffer, plus methods for drawing the model. Classes for
    /// specific types of primitive (CubePrimitive, SpherePrimitive, etc.) are
    /// derived from this common base, and use the AddVertex and AddIndex methods
    /// to specify their geometry.
    /// </summary>
    public abstract class GeometricPrimitive : IDisposable
    {
        // During the process of constructing a primitive model, vertex
        // and index data is stored on the CPU in these managed lists.
        private List<VertexPositionNormal> vertices = new List<VertexPositionNormal>();
        public List<VertexPositionNormal> Vertices
        {
            get { return vertices; }
        }

        private List<ushort> indices = new List<ushort>();
        public List<ushort> Indices
        {
            get { return indices; }
        }

        private IndexBuffer indexBuffer;
        public IndexBuffer IndexBuffer
        {
            get { return indexBuffer; }
        }

        /// <summary>
        /// Adds a new vertex to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddVertex(Vector3 position, Vector3 normal)
        {
            vertices.Add(new VertexPositionNormal(position, normal));
        }


        /// <summary>
        /// Adds a new index to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            indices.Add((ushort)index);
        }


        /// <summary>
        /// Queries the index of the current vertex. This starts at
        /// zero, and increments every time AddVertex is called.
        /// </summary>
        protected int CurrentVertex
        {
            get { return vertices.Count; }
        }


        /// <summary>
        /// Once all the geometry has been specified by calling AddVertex and AddIndex,
        /// this method copies the vertex and index data into GPU format buffers, ready
        /// for efficient rendering.
        protected void InitializePrimitive(GraphicsDevice graphicsDevice)
        {
            // Create an index buffer, and copy our index data into it.
            indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort),
                                          indices.Count, BufferUsage.None);

            indexBuffer.SetData(indices.ToArray());       
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~GeometricPrimitive()
        {
            Dispose(false);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (indexBuffer != null)
                    indexBuffer.Dispose();
            }
        }
    }
}
