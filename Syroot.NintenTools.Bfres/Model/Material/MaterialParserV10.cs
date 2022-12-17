using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    internal class MaterialParserV10
    {
        public static void Load(ResFileLoader loader, Material mat)
        {
            //V10 changes quite alot....

            //First change is a new struct with shader assign + tables for shader assign data
            var info = loader.Load<ShaderInfo>();
            long TextureArrayOffset = loader.ReadInt64();
            long TextureNameArray = loader.ReadInt64();
            long SamplerArrayOffset = loader.ReadInt64();
            long SamplerInfoArray = loader.ReadInt64();
            mat.SamplerDict = loader.LoadDict();
            loader.ReadInt64();
            loader.ReadInt64();
            loader.ReadInt64();
            loader.ReadInt64();
            loader.ReadInt64();
            loader.ReadUInt64(); //0
            long UserDataOffset = loader.ReadInt64();
            mat.UserDataDict = loader.LoadDict();
            loader.ReadInt64();
            loader.ReadInt64();
            loader.ReadInt64();
            loader.ReadInt64();
            ushort idx = loader.ReadUInt16();
            byte numTextureRef = loader.ReadByte();
            byte numSampler = loader.ReadByte();
            ushort numShaderParamVolatile = loader.ReadUInt16(); //idk
            ushort numUserData = loader.ReadUInt16();
            loader.ReadUInt16();
            loader.ReadUInt16(); //0
            loader.ReadUInt16(); //0
            loader.ReadUInt16(); //0

            long pos = loader.Position;

            mat.TextureRefs = loader.LoadCustom(() => loader.LoadStrings(numTextureRef), (uint)TextureNameArray);
            mat.Samplers = loader.LoadList<Sampler>(numSampler, SamplerInfoArray);
            mat.UserDatas = loader.LoadList<UserData>(numUserData, UserDataOffset);

            mat.ShaderAssign = new ShaderAssign()
            {
                ShaderArchiveName = info.ShaderAssign.ShaderArchiveName,
                ShadingModelName = info.ShaderAssign.ShadingModelName,
            };
        }

        public static void Save(ResFileSaver saver, Material mat)
        {
        }

        class ShaderInfo : IResData
        {
            public ShaderAssignV10 ShaderAssign;
            void IResData.Load(ResFileLoader loader)
            {
                ShaderAssign = loader.Load<ShaderAssignV10>();
            }

            void IResData.Save(ResFileSaver saver)
            {
            }
        }

        class ShaderAssignV10 : IResData
        {
            public string ShaderArchiveName;
            public string ShadingModelName;

            public Material ParentMaterial;

            void IResData.Load(ResFileLoader loader)
            {
                ShaderArchiveName = loader.LoadString();
                ShadingModelName = loader.LoadString();
            }

            void IResData.Save(ResFileSaver saver)
            {
            }
        }
    }
}
