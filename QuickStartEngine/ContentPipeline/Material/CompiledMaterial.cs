/*
 * CompiledMaterial.cs
 * 
 * This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
 * for license details.
 */

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

namespace QuickStart.ContentPipeline.Material
{
    /// <summary>
    /// Content pipeline container for materials being compiled into content pipeline data files for efficient run-time loading.
    /// </summary>
    public class CompiledMaterial
    {
        /// <summary>
        /// Shader variable type enumeration.
        /// </summary>
        public enum VariableType
        {
            Sampler=0,
            Texture2D,
            Matrix,
            Float4,
            Float3,
            Float2,
            Float,
            Bool,
        }

        /// <summary>
        /// Shader variable definition.
        /// </summary>
        public struct VariableParameter
        {
            public string semantic;
            public string variable;
            public VariableType type;
        }

        /// <summary>
        /// Shader constant definition.
        /// </summary>
        public struct ConstantParameter
        {
            public string semantic;
            public int numValues;
            public float[] values;
        }

        public List<ConstantParameter> constants = new List<CompiledMaterial.ConstantParameter>();
        public List<VariableParameter> variables = new List<CompiledMaterial.VariableParameter>();
        public string effect;
        public ExternalReference<CompiledEffectContent> compiledEffect;

        /// <summary>
        /// Writes the material to a content pipeline data file.
        /// </summary>
        /// <param name="output"></param>
        public void Write(ContentWriter output)
        {
            // Write a reference to the associated compiled effect file
            output.WriteExternalReference<CompiledEffectContent>(compiledEffect);

            // Write shader constants
            output.Write(constants.Count);

            for(int i = 0; i < constants.Count; ++i)
            {
                output.Write(constants[i].semantic);
                output.Write(constants[i].numValues);
                for(int j = 0; j < constants[i].numValues; ++j)
                {
                    output.Write(constants[i].values[j]);
                }
            }

            // Write shader variables
            output.Write(variables.Count);

            for(int i = 0; i < variables.Count; ++i)
            {
                output.Write(variables[i].semantic);
                output.Write(string.Format("{0}:{1}", variables[i].type.ToString(), variables[i].variable));
            }
        }
    }
}
