/*
 * CompiledMaterialImporter.cs
 * 
 * This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
 * for license details.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;

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
    /// Content pipeline importer for Quick Start XML material files.
    /// </summary>
    [ContentImporter(".qsm", DisplayName="QuickStart Material Importer")]
    public class CompiledMaterialImporter : ContentImporter<CompiledMaterial>
    {
        private CompiledMaterial material;

        /// <summary>
        /// Parses the specified file into a CompiledMaterial instance.
        /// </summary>
        /// <param name="filename">The material file to import.</param>
        /// <param name="context">The current content importer context.</param>
        /// <returns>The created CompiledMaterial instance.</returns>
        public override CompiledMaterial Import(string filename, ContentImporterContext context)
        {
            material = new CompiledMaterial();

            LoadFromFile(filename);

            return material;
        }

        /// <summary>
        /// Parses the specified material file.
        /// </summary>
        /// <param name="name">The material file to parse.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool LoadFromFile(string name)
        {
            XmlTextReader reader = new XmlTextReader(name);
            XmlDocument document = new XmlDocument();
            document.Load(reader);
            reader.Close();


            if(!ParseRootXMLNode(document.DocumentElement))
                return false;

            return true;
        }

        /// <summary>
        /// Parses the root XML node in the material file.
        /// </summary>
        /// <param name="node">The root XML node.</param>
        /// <returns>True is successful, false otherwise.</returns>
        private bool ParseRootXMLNode(XmlNode node)
        {
            switch(node.Name)
            {
                case "material":
                {
                    XmlNode effectNode = node.Attributes.GetNamedItem("effect");
                    if(effectNode == null)
                        throw new Exception("Error in material file: Root <material> node must have effect attribute.");

                    material.effect = effectNode.InnerText;

                    foreach(XmlNode child in node.ChildNodes)
                    {
                        if(!ParseXMLBodyNode(child))
                            return false;
                    }
                    break;
                }
                default:
                {
                    throw new Exception("Error in material file: Root node is not <material>.");
                }
            }
            return true;
        }

        /// <summary>
        /// Parses a single XML node.
        /// </summary>
        /// <param name="node">The node to parse.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool ParseXMLBodyNode(XmlNode node)
        {
            switch(node.Name)
            {
                case "sampler":
                {
                    // Parse sampler

                    if(node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <sampler> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if(semanticNode == null)
                        throw new Exception("Error in material file: <sampler> node must have semantic usage.");


                    CompiledMaterial.VariableParameter var = new CompiledMaterial.VariableParameter();
                    var.semantic = semanticNode.InnerText;
                    var.variable = node.InnerText;
                    var.type = CompiledMaterial.VariableType.Sampler;
                    material.variables.Add(var);

                    break;
                }
                case "variable_bool":
                {
                    // Parse matrix variable

                    if (node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <variable_bool> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if (semanticNode == null)
                        throw new Exception("Error in material file: <variable_bool> node must have semantic usage.");

                    XmlNode matIDNode = node.Attributes.GetNamedItem("varID");
                    if (matIDNode == null)
                        throw new Exception("Error in material file: <variable_bool> node must have rendervar attribute.");

                    CompiledMaterial.VariableParameter var = new CompiledMaterial.VariableParameter();
                    var.semantic = semanticNode.InnerText;
                    var.variable = matIDNode.InnerText;
                    var.type = CompiledMaterial.VariableType.Bool;

                    material.variables.Add(var);

                    break;
                }
                case "variable_float":
                {
                    // Parse float variable

                    if(node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <variable_float> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if(semanticNode == null)
                        throw new Exception("Error in material file: <variable_float> node must have semantic usage.");

                    XmlNode floatIDNode = node.Attributes.GetNamedItem("varID");
                    if (floatIDNode == null)
                        throw new Exception("Error in material file: <variable_float> node must have rendervar attribute.");

                    CompiledMaterial.VariableParameter var = new CompiledMaterial.VariableParameter();
                    var.semantic = semanticNode.InnerText;
                    var.variable = floatIDNode.InnerText;
                    var.type = CompiledMaterial.VariableType.Float;

                    material.variables.Add(var);

                    break;
                }
                case "variable_float2":
                {
                    // Parse float variable

                    if (node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <variable_float2> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if (semanticNode == null)
                        throw new Exception("Error in material file: <variable_float2> node must have semantic usage.");

                    XmlNode floatIDNode = node.Attributes.GetNamedItem("varID");
                    if (floatIDNode == null)
                        throw new Exception("Error in material file: <variable_float2> node must have rendervar attribute.");

                    CompiledMaterial.VariableParameter var = new CompiledMaterial.VariableParameter();
                    var.semantic = semanticNode.InnerText;
                    var.variable = floatIDNode.InnerText;
                    var.type = CompiledMaterial.VariableType.Float2;

                    material.variables.Add(var);

                    break;
                }
                case "variable_float3":
                {
                    // Parse float variable

                    if (node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <variable_float3> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if (semanticNode == null)
                        throw new Exception("Error in material file: <variable_float3> node must have semantic usage.");

                    XmlNode floatIDNode = node.Attributes.GetNamedItem("varID");
                    if (floatIDNode == null)
                        throw new Exception("Error in material file: <variable_float3> node must have rendervar attribute.");

                    CompiledMaterial.VariableParameter var = new CompiledMaterial.VariableParameter();
                    var.semantic = semanticNode.InnerText;
                    var.variable = floatIDNode.InnerText;
                    var.type = CompiledMaterial.VariableType.Float3;

                    material.variables.Add(var);

                    break;
                }
                case "variable_float4":
                {
                    // Parse float variable

                    if(node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <variable_float4> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if(semanticNode == null)
                        throw new Exception("Error in material file: <variable_float4> node must have semantic usage.");

                    XmlNode floatIDNode = node.Attributes.GetNamedItem("varID");
                    if (floatIDNode == null)
                        throw new Exception("Error in material file: <variable_float4> node must have rendervar attribute.");

                    CompiledMaterial.VariableParameter var = new CompiledMaterial.VariableParameter();
                    var.semantic = semanticNode.InnerText;
                    var.variable = floatIDNode.InnerText;
                    var.type = CompiledMaterial.VariableType.Float4;

                    material.variables.Add(var);

                    break;
                }
                case "variable_matrix":
                {
                    // Parse matrix variable

                    if(node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <variable_matrix> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if(semanticNode == null)
                        throw new Exception("Error in material file: <variable_matrix> node must have semantic usage.");

                    XmlNode matIDNode = node.Attributes.GetNamedItem("varID");
                    if(matIDNode == null)
                        throw new Exception("Error in material file: <variable_matrix> node must have varID attribute.");

                    CompiledMaterial.VariableParameter var = new CompiledMaterial.VariableParameter();
                    var.semantic = semanticNode.InnerText;
                    var.variable = matIDNode.InnerText;
                    var.type = CompiledMaterial.VariableType.Matrix;

                    material.variables.Add(var);

                    break;
                }
                case "variable_texture2D":
                {
                    // Parse texture2D variable

                    if (node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <variable_texture2D> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if (semanticNode == null)
                        throw new Exception("Error in material file: <variable_texture2D> node must have semantic usage.");

                    XmlNode textureIDNode = node.Attributes.GetNamedItem("varID");
                    if (textureIDNode == null)
                        throw new Exception("Error in material file: <variable_texture2D> node must have varID attribute.");

                    CompiledMaterial.VariableParameter var = new CompiledMaterial.VariableParameter();
                    var.semantic = semanticNode.InnerText;
                    var.variable = textureIDNode.InnerText;
                    var.type = CompiledMaterial.VariableType.Texture2D;

                    material.variables.Add(var);

                    break;
                }
                case "constant_float":
                {
                    // Parse constant variable

                    if(node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <constant_float1> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if(semanticNode == null)
                        throw new Exception("Error in material file: <constant_float1> node must have semantic usage.");

                    float value;
                    if(!float.TryParse(node.InnerText, NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value))
                        throw new Exception("Error in material file: <constant_float1> node only supports floating-point constants.");

                    CompiledMaterial.ConstantParameter cons = new CompiledMaterial.ConstantParameter();

                    cons.semantic = semanticNode.InnerText;
                    cons.numValues = 1;
                    cons.values = new float[cons.numValues];
                    cons.values[0] = value;

                    material.constants.Add(cons);

                    break;
                }

                case "constant_float4":
                {
                    // Parse constant variable

                    if(node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <constant_float4> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if(semanticNode == null)
                        throw new Exception("Error in material file: <constant_float4> node must have semantic usage.");

                    Vector4 value;

                    string[] vars = node.InnerText.Split(',');
                    if(vars.Length != 4)
                        throw new Exception("Error in material file: <constant_float4> node must specify 4 floats in the form: f1, f2, f3, f4");

                    if(!float.TryParse(vars[0], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.X) || !float.TryParse(vars[1], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.Y) || !float.TryParse(vars[2], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.Z) || !float.TryParse(vars[3], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.W))
                        throw new Exception("Error in material file: <constant_float4> node only supports floating-point constants.");

                    CompiledMaterial.ConstantParameter cons = new CompiledMaterial.ConstantParameter();

                    cons.semantic = semanticNode.InnerText;
                    cons.numValues = 4;
                    cons.values = new float[cons.numValues];
                    cons.values[0] = value.X;
                    cons.values[1] = value.Y;
                    cons.values[2] = value.Z;
                    cons.values[3] = value.W;

                    material.constants.Add(cons);

                    break;
                }

                case "constant_matrix":
                {
                    // Parse constant variable

                    if(node.ChildNodes.Count > 1)
                        throw new Exception("Error in material file: <constant_matrix> node cannot have children nodes.");

                    XmlNode semanticNode = node.Attributes.GetNamedItem("semantic");
                    if(semanticNode == null)
                        throw new Exception("Error in material file: <constant_matrix> node must have semantic usage.");

                    Matrix value;

                    string[] vars = node.InnerText.Split(',');
                    if(vars.Length != 16)
                        throw new Exception("Error in material file: <constant_matrix> node must specify 4 floats in the form: f1, f2, f3, f4, ..., f16");

                    if(!float.TryParse(vars[0], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M11) || !float.TryParse(vars[1], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M12) || !float.TryParse(vars[2], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M13) || !float.TryParse(vars[3], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M14) ||
                       !float.TryParse(vars[4], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M21) || !float.TryParse(vars[5], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M22) || !float.TryParse(vars[6], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M23) || !float.TryParse(vars[7], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M24) ||
                       !float.TryParse(vars[8], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M31) || !float.TryParse(vars[9], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M32) || !float.TryParse(vars[10], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M33) || !float.TryParse(vars[11], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M34) ||
                       !float.TryParse(vars[12], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M41) || !float.TryParse(vars[13], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M42) || !float.TryParse(vars[14], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M43) || !float.TryParse(vars[15], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out value.M44))
                        throw new Exception("Error in material file: <constant_matrix> node only supports floating-point constants.");


                    CompiledMaterial.ConstantParameter cons = new CompiledMaterial.ConstantParameter();

                    cons.semantic = semanticNode.InnerText;
                    cons.numValues = 16;
                    cons.values = new float[cons.numValues];
                    cons.values[0] = value.M11;
                    cons.values[1] = value.M12;
                    cons.values[2] = value.M13;
                    cons.values[3] = value.M14;

                    cons.values[4] = value.M21;
                    cons.values[5] = value.M22;
                    cons.values[6] = value.M23;
                    cons.values[7] = value.M24;

                    cons.values[8] = value.M31;
                    cons.values[9] = value.M32;
                    cons.values[10] = value.M33;
                    cons.values[11] = value.M34;

                    cons.values[12] = value.M41;
                    cons.values[13] = value.M42;
                    cons.values[14] = value.M43;
                    cons.values[15] = value.M44;

                    material.constants.Add(cons);

                    break;
                }


                default:
                {
                    // Ignore unknown node
                    // TODO: Warning message
                    break;
                }
            }
            return true;
        }
    }
}
