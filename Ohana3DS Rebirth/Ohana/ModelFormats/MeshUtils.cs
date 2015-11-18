﻿using System.Collections.Generic;

namespace Ohana3DS_Rebirth.Ohana.ModelFormats
{
    class MeshUtils
    {
        /// <summary>
        ///     Calculates the minimun and maximum vector values for a Model.
        /// </summary>
        /// <param name="mdl">The target model</param>
        /// <param name="vertex">The current mesh vertex</param>
        public static void calculateBounds(RenderBase.OModel mdl, RenderBase.OVertex vertex)
        {
            if (vertex.position.x < mdl.minVector.x) mdl.minVector.x = vertex.position.x;
            else if (vertex.position.x > mdl.maxVector.x) mdl.maxVector.x = vertex.position.x;
            else if (vertex.position.y < mdl.minVector.y) mdl.minVector.y = vertex.position.y;
            else if (vertex.position.y > mdl.maxVector.y) mdl.maxVector.y = vertex.position.y;
            else if (vertex.position.z < mdl.minVector.z) mdl.minVector.z = vertex.position.z;
            else if (vertex.position.z > mdl.maxVector.z) mdl.maxVector.z = vertex.position.z;
        }

        /// <summary>
        ///     Clamps a Float value between 0 and 255 and return as Byte.
        /// </summary>
        /// <param name="value">The float value</param>
        /// <returns></returns>
        public static byte saturate(float value)
        {
            if (value > 0xff) return 0xff;
            if (value < 0) return 0;
            return (byte)value;
        }

        /// <summary>
        ///     Gets a generic Material, that can be used when the model don't have any, to make the shape visible on the viewport.
        /// </summary>
        /// <returns></returns>
        public static RenderBase.OMaterial getGenericMaterial()
        {
            RenderBase.OMaterial material = new RenderBase.OMaterial();

            material.fragmentShader.alphaTest.isTestEnabled = true;
            material.fragmentShader.alphaTest.testFunction = RenderBase.OTestFunction.greaterOrEqual;
            material.fragmentShader.alphaTest.testReference = 0x7f;

            material.textureMapper[0].wrapU = RenderBase.OTextureWrap.repeat;
            material.textureMapper[0].wrapV = RenderBase.OTextureWrap.repeat;

            material.textureMapper[0].minFilter = RenderBase.OTextureMinFilter.linearMipmapLinear;
            material.textureMapper[0].magFilter = RenderBase.OTextureMagFilter.linear;

            for (int i = 0; i < 6; i++)
            {
                material.fragmentShader.textureCombiner[i].rgbSource[0] = RenderBase.OCombineSource.texture0;
                material.fragmentShader.textureCombiner[i].rgbSource[1] = RenderBase.OCombineSource.primaryColor;
                material.fragmentShader.textureCombiner[i].combineRgb = RenderBase.OCombineOperator.modulate;
                material.fragmentShader.textureCombiner[i].alphaSource[0] = RenderBase.OCombineSource.texture0;
                material.fragmentShader.textureCombiner[i].rgbScale = 1;
                material.fragmentShader.textureCombiner[i].alphaScale = 1;
            }
            material.fragmentOperation.depth.isTestEnabled = true;
            material.fragmentOperation.depth.testFunction = RenderBase.OTestFunction.lessOrEqual;
            material.fragmentOperation.depth.isMaskEnabled = true;

            return material;
        }

        const uint optimizerLookBack = 32;

        public class optimizedMesh
        {
            public List<RenderBase.OVertex> vertices = new List<RenderBase.OVertex>();
            public List<uint> indices = new List<uint>();

            public bool hasNormal;
            public bool hasTangent;
            public bool hasColor;
            public bool hasNode;
            public bool hasWeight;
            public int texUVCount;
        }

        /// <summary>
        ///     Creates a Index Buffer for a Mesh, trying to repeat as less Vertices as possible.
        /// </summary>
        /// <param name="mesh">The Mesh that should be optimized</param>
        /// <returns></returns>
        public static optimizedMesh optimizeMesh(RenderBase.OModelObject mesh)
        {
            optimizedMesh output = new optimizedMesh();

            output.hasNormal = mesh.hasNormal;
            output.hasTangent = mesh.hasTangent;
            output.hasColor = mesh.hasColor;
            output.hasNode = mesh.hasNode;
            output.hasWeight = mesh.hasWeight;
            output.texUVCount = mesh.texUVCount;

            for (int i = 0; i < mesh.obj.Count; i++)
            {
                bool found = false;
                for (int j = 1; j <= optimizerLookBack; j++)
                {
                    int p = output.vertices.Count - j;
                    if (p < 0 || p >= output.vertices.Count) break;
                    if (output.vertices[p].Equals(mesh.obj[i]))
                    {
                        output.indices.Add((uint)p);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    output.vertices.Add(mesh.obj[i]);
                    output.indices.Add((uint)(output.vertices.Count - 1));
                }
            }

            return output;
        }
    }
}