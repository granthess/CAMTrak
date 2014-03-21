// StaticModelContent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace QuickStart.ContentPipeline.Model
{
    public struct VertexStaticModel : IVertexType
    {
        public Vector3 Position;
        public Vector2 Texture;
        public Vector3 Normal;
        public Vector3 Binormal;
        public Vector3 Tangent;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public VertexStaticModel(Vector3 position, Vector2 texCoords, Vector3 normal,
                                 Vector3 binormal, Vector3 tangent )
        {
            Position = position;
            Texture = texCoords;
            Normal = normal;
            Binormal = binormal;
            Tangent = tangent;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement( sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement( sizeof(float) * 5, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement( sizeof(float) * 8, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
            new VertexElement( sizeof(float) * 11, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0) 
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexStaticModel.VertexDeclaration; }
        }
    }

    /// <summary>
    /// Container for static models during content creation.
    /// </summary>
    public class StaticModelContent
    {
        /// <summary>
        /// Retrieves the vertex content for the model.
        /// </summary>
        public VertexBufferContent VertexContent
        {
            get
            {
                return vertexContent;
            }
        }

        /// <summary>
        /// Retrieves the index content for the model.
        /// </summary>
        public IndexCollection IndexContent
        {
            get
            {
                return indexContent;
            }
        }

        private VertexBufferContent vertexContent;
        private IndexCollection indexContent;        

        /// <summary>
        /// Initializes a new instance of a static model.
        /// </summary>
        public StaticModelContent()
        {
            vertexContent = new VertexBufferContent();
            VertexDeclarationContent vertContent = new VertexDeclarationContent();
            foreach (VertexElement element in VertexStaticModel.VertexDeclaration.GetVertexElements())
            {
                vertContent.VertexElements.Add(element);
            }
            vertexContent.VertexDeclaration = vertContent;
            indexContent = new IndexCollection();
        }

        /// <summary>
        /// Write the static model to a content pipeline data file.
        /// </summary>
        /// <param name="output">The ContentWriter for the data file.</param>
        public void Write(ContentWriter output)
        {
            output.WriteObject<VertexBufferContent>(vertexContent);
            output.WriteObject<IndexCollection>(indexContent);            
        }
    }
}
