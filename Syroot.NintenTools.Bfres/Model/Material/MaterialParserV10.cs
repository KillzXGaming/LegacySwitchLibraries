using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    public class MaterialParserV10
    {
        public static void Load(ResFileLoader loader, Material mat)
        {
            //V10 changes quite alot....

            //First change is a new struct with shader assign + tables for shader assign data
            var info = loader.Load<ShaderInfo>();
            long TextureArrayOffset = loader.ReadOffset();
            long TextureNameArray = loader.ReadOffset();
            long SamplerArrayOffset = loader.ReadOffset();
            long SamplerInfoArray = loader.ReadOffset();
            mat.SamplerDict = loader.LoadDict();
            //Next is table data
            long renderInfoDataTable = loader.ReadOffset(); //raw data
            long renderInfoCounterTable = loader.ReadOffset(); //data counts
            long renderInfoDataOffsets = loader.ReadOffset(); //offsets as shorts
            long SourceParamOffset = loader.ReadOffset(); //raw param data
            long SourceParamIndices = loader.ReadOffset(); //0xFFFF per param.
            loader.ReadUInt64(); //reserved
            long UserDataOffset = loader.ReadOffset();
            mat.UserDataDict = loader.LoadDict();
            long VolatileFlagsOffset = loader.ReadOffset();
            long userPointer = loader.ReadInt64();
            long SamplerSlotArrayOffset = loader.ReadOffset();
            long TexSlotArrayOffset = loader.ReadOffset();
            ushort idx = loader.ReadUInt16();
            byte numSampler = loader.ReadByte();
            byte numTextureRef = loader.ReadByte();
            loader.ReadUInt16(); //reserved
            ushort numUserData = loader.ReadUInt16();
            ushort renderInfoDataSize = loader.ReadUInt16();
            ushort user_shading_model_option_ubo_size = loader.ReadUInt16(); //Set at runtime?
            loader.ReadUInt32(); //padding

            mat.RenderInfoSize = renderInfoDataSize;

            long pos = loader.Position;

            mat.TextureRefs = loader.LoadCustom(() => loader.LoadStrings(numTextureRef), (uint)TextureNameArray);
            mat.Samplers = loader.LoadList<Sampler>(numSampler, SamplerInfoArray);
            mat.UserDatas = loader.LoadList<UserData>(numUserData, UserDataOffset);

            mat.TextureSlotArray = loader.LoadCustom(() => loader.ReadInt64s(numTextureRef), (uint)TexSlotArrayOffset);
            mat.SamplerSlotArray = loader.LoadCustom(() => loader.ReadInt64s(numSampler), (uint)SamplerSlotArrayOffset);

            mat.ShaderAssign = info.ShaderAssign;
            ((ShaderAssignV10)mat.ShaderAssign).ParentMaterial = mat;
            mat.ShaderParamData = loader.LoadCustom(() => loader.ReadBytes(info.ShaderAssign.ShaderParamSize), (uint)SourceParamOffset);
            mat.ParamIndices = loader.LoadCustom(() => loader.ReadInt32s(info.ShaderAssign.ShaderParameters.Count), (uint)SourceParamIndices);


            ReadRenderInfo(loader, info, mat, renderInfoCounterTable, renderInfoDataOffsets, renderInfoDataTable);
            ReadShaderParams(loader, info, mat);

            LoadAttributeAssign(info, mat);
            LoadSamplerAssign(info, mat);
            LoadShaderOptions(info, mat);

            loader.Seek(pos, SeekOrigin.Begin);
        }

        static void ReadRenderInfo(ResFileLoader loader, ShaderInfo info, Material mat,
    long renderInfoCounterTable, long renderInfoDataOffsets, long renderInfoDataTable)
        {
            mat.RenderInfoDict = new ResDict();
            mat.RenderInfos = new List<RenderInfo>();

            for (int i = 0; i < info.ShaderAssign.RenderInfos.Count; i++)
            {
                RenderInfo renderInfo = new RenderInfo();

                //Info table
                loader.Seek((int)info.ShaderAssign.renderInfoListOffset + i * 16, SeekOrigin.Begin);
                renderInfo.Name = loader.LoadString(); //name offset
                renderInfo.Type = (RenderInfoType)loader.ReadByte();

                //Count table
                loader.Seek((int)renderInfoCounterTable + i * 2, SeekOrigin.Begin);
                ushort count = loader.ReadUInt16();

                //Offset table
                loader.Seek((int)renderInfoDataOffsets + i * 2, SeekOrigin.Begin);
                renderInfo.DataOffset = loader.ReadUInt16();

                //Raw data table
                loader.Seek((int)renderInfoDataTable + renderInfo.DataOffset, SeekOrigin.Begin);
                renderInfo.ReadData(loader, renderInfo.Type, count);

                mat.RenderInfos.Add(renderInfo);
                mat.RenderInfoDict.Add(renderInfo.Name);
            }
        }

        static void ReadShaderParams(ResFileLoader loader, ShaderInfo info, Material mat)
        {
            mat.ShaderParamDict = new ResDict();
            mat.ShaderParams = new List<ShaderParam>();

            for (int i = 0; i < info.ShaderAssign.ShaderParameters.Count; i++)
            {
                ShaderParam param = new ShaderParam();

                loader.Seek((int)info.ShaderAssign.shaderParamOffset + i * 24, SeekOrigin.Begin);
                var pad0 = loader.ReadUInt64(); //padding
                param.Name = loader.LoadString(); //name offset
                param.DataOffset = loader.ReadUInt16(); //padding
                param.Type = (ShaderParamType)loader.ReadUInt16(); //type
                var pad2 = loader.ReadUInt32(); //padding

                if (pad0 != 0 || pad2 != 0)
                    throw new Exception();

                mat.ShaderParams.Add(param);
                mat.ShaderParamDict.Add(param.Name);
            }
        }

        static void LoadAttributeAssign(ShaderInfo info, Material mat)
        {
            mat.ShaderAssign.AttribAssigns.Clear();

            for (int i = 0; i < info.ShaderAssign.AttribAssignDict.Count; i++)
            {
                int idx = info.AttributeAssignIndices?.Length > 0 ? info.AttributeAssignIndices[i] : i;
                var value = idx == -1 ? "<Default Value>" : info.AttribAssigns[idx];
                mat.ShaderAssign.AttribAssigns.Add(value);
            }
        }

        static void LoadSamplerAssign(ShaderInfo info, Material mat)
        {
            mat.ShaderAssign.SamplerAssigns.Clear();

            for (int i = 0; i < info.ShaderAssign.SamplerAssignDict.Count; i++)
            {
                int idx = info.SamplerAssignIndices?.Length > 0 ? info.SamplerAssignIndices[i] : i;
                var value = idx == -1 ? "Default Value>" : info.SamplerAssigns[idx];
                mat.ShaderAssign.SamplerAssigns.Add(value);
            }
        }

        static void LoadShaderOptions(ShaderInfo info, Material mat)
        {
            //Find target option
            List<string> choices = new List<string>();
            //Boolean types
            for (int i = 0; i < info.OptionToggles.Length; i++)
                choices.Add(info.OptionToggles[i] ? "True" : "False");
            //Value types
            if (info.OptionValues != null)
                choices.AddRange(info.OptionValues);

            mat.ShaderAssign.ShaderOptions.Clear();
            for (int i = 0; i < info.ShaderAssign.ShaderOptionDict.Count; i++)
            {
                //Get the choice value index
                int idx = info.OptionIndices?.Length > 0 ? info.OptionIndices[i] : i;
                //If choice is -1, it is not used, else get the choice value
                var value = idx == -1 ? "<Default Value>" : choices[idx];
                mat.ShaderAssign.ShaderOptions.Add(value);
            }
        }

        public static void Save(ResFileSaver saver, Material mat)
        {
            ShaderInfo info = new ShaderInfo();

            if (mat.ShaderAssign != null)
            {
                //New material. Convert to v10 shader assign
                if (!(mat.ShaderAssign is ShaderAssignV10))
                {
                    mat.ShaderAssign = new ShaderAssignV10()
                    {
                        ShaderArchiveName = mat.ShaderAssign.ShaderArchiveName,
                        ShadingModelName = mat.ShaderAssign.ShadingModelName,
                        ShaderOptions = mat.ShaderAssign.ShaderOptions,
                        ShaderOptionDict = mat.ShaderAssign.ShaderOptionDict,
                        SamplerAssigns = mat.ShaderAssign.SamplerAssigns,
                        AttribAssigns = mat.ShaderAssign.AttribAssigns,
                        AttribAssignDict = mat.ShaderAssign.AttribAssignDict,
                        SamplerAssignDict = mat.ShaderAssign.SamplerAssignDict,
                        Revision = mat.ShaderAssign.Revision,
                    };
                }
                info.ShaderAssign = (ShaderAssignV10)mat.ShaderAssign;
                info.ShaderAssign.ParentMaterial = mat;
                info.SamplerAssigns = new List<string>();
                info.AttribAssigns = new List<string>();
                info.OptionValues = new List<string>();

                List<sbyte> samplerIndices = new List<sbyte>();
                List<sbyte> attributeIndices = new List<sbyte>();
                List<short> optionChoiceIndices = new List<short>();

                //Values
                foreach (var sampler in mat.ShaderAssign.SamplerAssigns)
                {
                    if (sampler == "Default Value>")
                    {
                        samplerIndices.Add(-1);
                        continue;
                    }
                    info.SamplerAssigns.Add(sampler);
                    samplerIndices.Add((sbyte)info.SamplerAssigns.IndexOf(sampler));
                }


                foreach (var att in mat.ShaderAssign.AttribAssigns)
                {
                    if (att == "<Default Value>")
                    {
                        attributeIndices.Add(-1);
                        continue;
                    }
                    info.AttribAssigns.Add(att);
                    attributeIndices.Add((sbyte)info.AttribAssigns.IndexOf(att));
                }

                int choiceIdx = 0;

                List<bool> toggles = new List<bool>();
                foreach (var op in mat.ShaderAssign.ShaderOptions)
                {
                    if (op == "<Default Value>")
                    {
                        optionChoiceIndices.Add(-1);
                        continue;
                    }

                    if (op == "True") toggles.Add(true);
                    else if (op == "False") toggles.Add(false);
                    else info.OptionValues.Add(op);

                    optionChoiceIndices.Add((short)choiceIdx);
                    choiceIdx++;
                }

                info.OptionToggles = toggles.ToArray();

                if (samplerIndices.Any(x => x == -1))
                    info.SamplerAssignIndices = samplerIndices.ToArray();
                if (attributeIndices.Any(x => x == -1))
                    info.AttributeAssignIndices = attributeIndices.ToArray();

                info.OptionIndices = optionChoiceIndices.ToArray();
            }

            List<RenderInfo> renderInfoOrdered = new List<RenderInfo>();
            renderInfoOrdered.AddRange(mat.RenderInfos.Where(x => x.Type == RenderInfoType.String));
            renderInfoOrdered.AddRange(mat.RenderInfos.Where(x => x.Type == RenderInfoType.Single));
            renderInfoOrdered.AddRange(mat.RenderInfos.Where(x => x.Type == RenderInfoType.Int32));
            mat.RenderInfos = renderInfoOrdered.ToList();

            //Calculate total buffer sizes and offsets
            int renderInfoDataSize = 0;
            foreach (var renderInfo in mat.RenderInfos)
            {
                renderInfo.DataOffset = renderInfoDataSize;
                switch (renderInfo.Type)
                {
                    case RenderInfoType.String:
                        renderInfoDataSize += 8 * renderInfo.GetValueStrings().Length;
                        break;
                    case RenderInfoType.Single:
                        renderInfoDataSize += 4 * renderInfo.GetValueSingles().Length;
                        break;
                    default:
                        renderInfoDataSize += 4 * renderInfo.GetValueInt32s().Length;
                        break;
                }
            }
            //Adds alignment
            var alignment = 128;
            renderInfoDataSize += (-renderInfoDataSize % alignment + alignment) % alignment;

            renderInfoDataSize = mat.RenderInfoSize;

            saver.SaveRelocateEntryToSection(saver.Position, 12, 1, 0, ResFileSaver.Section1, "FMAT");

            saver.SaveString(mat.Name);
            saver.Save(info);
            saver.SaveCustom(new long[mat.TextureRefs.Count], () => saver.Write(new long[mat.TextureRefs.Count]));
            saver.SaveCustom(mat.TextureRefs, () => saver.SaveStrings(mat.TextureRefs, true));
            saver.SaveCustom(new long[mat.Samplers.Count], () => saver.Write(new long[mat.Samplers.Count * 15
                ]));
            saver.SaveList(mat.Samplers);
            saver.SaveDict(mat.SamplerDict);
            //Render info data
            saver.SaveCustom(mat.RenderInfos, () =>
            {
                long pos = saver.Position;

                saver.Write(new byte[mat.RenderInfoSize]);
                saver.Seek(pos, SeekOrigin.Begin);

                var infoStrings = mat.RenderInfos.Where(x => x.Type == RenderInfoType.String).ToList();
                if (infoStrings.Count > 0)
                {
                    int numStrings = infoStrings.Sum(x => x.GetValueStrings().Length);

                    saver.SaveRelocateEntryToSection(saver.Position, (uint)numStrings, 1, 0, ResFileSaver.Section1, "Render Info Strings V10");
                }
                long startpos = saver.Position;
                foreach (var renderInfo in mat.RenderInfos)
                {
                    renderInfo.DataOffset = saver.Position - startpos;

                    switch (renderInfo.Type)
                    {
                        case RenderInfoType.String: renderInfo.SaveStrings(saver); break;
                        case RenderInfoType.Single: renderInfo.SaveFloats(saver); break;
                        default: renderInfo.SaveInts(saver); break;
                    }
                }
                saver.Seek(pos + mat.RenderInfoSize, SeekOrigin.Begin);
            });
            //Render info count
            saver.SaveCustom(new byte[mat.RenderInfoSize], () =>
            {
                foreach (var renderInfo in mat.RenderInfos)
                {
                    switch (renderInfo.Type)
                    {
                        case RenderInfoType.String: saver.Write((ushort)renderInfo.GetValueStrings()?.Length); break;
                        case RenderInfoType.Single: saver.Write((ushort)renderInfo.GetValueSingles()?.Length); break;
                        default: saver.Write((ushort)renderInfo.GetValueInt32s()?.Length); break;
                    }
                }
            });
            //Render info offsets
            saver.SaveCustom(new uint[mat.RenderInfos.Count], () =>
            {
                foreach (var renderInfo in mat.RenderInfos)
                    saver.Write((ushort)renderInfo.DataOffset);
            });
            //Shader params
            saver.SaveCustom(mat.ShaderParamData, () =>
            {
                saver.Write(mat.ShaderParamData);
                saver.Align(128);
            });
            saver.SaveCustom(mat.ParamIndices, () => saver.Write(mat.ParamIndices));
            saver.Write(0UL); //0

            saver.SaveRelocateEntryToSection(saver.Position, 3, 1, 0, ResFileSaver.Section1, "FMAT User Data");


            mat.PosUserDataMaterialOffset = saver.SaveOffset();
            mat.PosUserDataDictMaterialOffset = saver.SaveOffset();

            saver.SaveCustom(new byte[32], () => saver.Write(new byte[32])); //Volatile Flags?
            saver.Write(0UL); //userPointer?

            saver.SaveRelocateEntryToSection(saver.Position, 2, 1, 0, ResFileSaver.Section1, "Material texture slots");

            saver.SaveCustom(mat.SamplerSlotArray, () => saver.Write(mat.SamplerSlotArray));
            saver.SaveCustom(mat.TextureSlotArray, () => saver.Write(mat.TextureSlotArray));
            saver.Write((ushort)saver.CurrentIndex);
            saver.Write((byte)mat.TextureRefs.Count);
            saver.Write((byte)mat.Samplers.Count);
            saver.Write((ushort)0); //numShaderParamVolatile?
            saver.Write((ushort)mat.UserDatas.Count);
            saver.Write((ushort)renderInfoDataSize);

            saver.Write((ushort)0);
            saver.Write((ushort)0);
            saver.Write((ushort)0);
        }

        class ShaderInfo : IResData
        {
            public ShaderAssignV10 ShaderAssign;

            public IList<string> AttribAssigns;
            public IList<string> SamplerAssigns;

            public bool[] OptionToggles;
            public IList<string> OptionValues;

            public short[] OptionIndices;
            public sbyte[] AttributeAssignIndices;
            public sbyte[] SamplerAssignIndices;

            private long _optionBitFlags;

            void IResData.Load(ResFileLoader loader)
            {
                ShaderAssign = loader.Load<ShaderAssignV10>();
                long attribAssignOffset = loader.ReadOffset();
                long attribAssignIndicesOffset = loader.ReadOffset();
                long samplerAssignOffset = loader.ReadOffset();
                long samplerAssignIndicesOffset = loader.ReadOffset();
                long optionChoiceToggleOffset = loader.ReadOffset();
                long optionChoiceStringsOffset = loader.ReadOffset();
                long optionChoiceIndicesOffset = loader.ReadOffset();
                loader.ReadUInt32(); //padding
                byte numAttributeAssign = loader.ReadByte();
                byte numSamplerAssign = loader.ReadByte();
                ushort shaderOptionBooleanCount = loader.ReadUInt16();
                ushort shaderOptionChoiceCount = loader.ReadUInt16();
                loader.ReadUInt16(); //padding
                loader.ReadUInt32(); //padding

                AttribAssigns = loader.LoadCustom(() => loader.LoadStrings(numAttributeAssign), (uint)attribAssignOffset);
                SamplerAssigns = loader.LoadCustom(() => loader.LoadStrings(numSamplerAssign), (uint)samplerAssignOffset);
                _optionBitFlags = loader.LoadCustom(() => loader.ReadInt64(), (uint)optionChoiceToggleOffset);

                OptionIndices = ReadShortIndices(loader, optionChoiceIndicesOffset, shaderOptionChoiceCount, ShaderAssign.ShaderOptionDict.Count);
                AttributeAssignIndices = ReadByteIndices(loader, attribAssignIndicesOffset, numAttributeAssign, ShaderAssign.AttribAssignDict.Count);
                SamplerAssignIndices = ReadByteIndices(loader, samplerAssignIndicesOffset, numSamplerAssign, ShaderAssign.SamplerAssignDict.Count);

                var numChoiceValues = shaderOptionChoiceCount - shaderOptionBooleanCount;
                OptionValues = loader.LoadCustom(() => loader.LoadStrings((int)numChoiceValues), (uint)optionChoiceStringsOffset);

                SetupOptionBooleans(shaderOptionBooleanCount);
            }

            void IResData.Save(ResFileSaver saver)
            {
                CreateOptionFlag();

                saver.SaveRelocateEntryToSection(saver.Position, 8, 1, 0, ResFileSaver.Section1, "ShaderInfo");

                saver.Save(ShaderAssign);
                saver.SaveCustom(AttribAssigns, () => saver.SaveStrings(AttribAssigns, true));
                saver.SaveCustom(AttributeAssignIndices, () => WriteIndices(saver, AttributeAssignIndices));
                saver.SaveCustom(SamplerAssigns, () => saver.SaveStrings(SamplerAssigns, true));
                saver.SaveCustom(SamplerAssignIndices, () => WriteIndices(saver, SamplerAssignIndices));
                saver.SaveCustom(OptionToggles, () => saver.Write(_optionBitFlags));
                saver.SaveCustom(OptionValues, () => saver.SaveStrings(OptionValues, true));
                saver.SaveCustom(OptionIndices, () => WriteIndices(saver, OptionIndices));
                saver.Write(0); //padding
                saver.Write((byte)AttribAssigns?.Count);
                saver.Write((byte)SamplerAssigns?.Count);
                saver.Write((ushort)OptionToggles?.Length);
                saver.Write((ushort)(OptionToggles?.Length + OptionValues?.Count));
                saver.Write(new byte[6]); //padding
            }

            private sbyte[] ReadByteIndices(ResFileLoader loader, long offset, int usedCount, int totalCount)
            {
                if (offset == 0)
                    return null;

                using (loader.TemporarySeek((int)offset, SeekOrigin.Begin))
                {
                    var usedIndices = loader.ReadSBytes(usedCount);
                    return loader.ReadSBytes(totalCount);
                }
            }

            private short[] ReadShortIndices(ResFileLoader loader, long offset, int usedCount, int totalCount)
            {
                if (offset == 0)
                    return null;

                using (loader.TemporarySeek((int)offset, SeekOrigin.Begin))
                {
                    var usedIndices = loader.ReadInt16s(usedCount);
                    return loader.ReadInt16s(totalCount);
                }
            }

            private void SetupOptionBooleans(int count)
            {
                OptionToggles = new bool[count];
                for (int i = 0; i < count; i++)
                {
                    bool set = (_optionBitFlags & 0x1) != 0;
                    _optionBitFlags >>= 1;

                    OptionToggles[i] = set;
                }
            }

            private void CreateOptionFlag()
            {
                _optionBitFlags = 0;

                for (int i = 0; i < OptionToggles.Length; i++)
                {
                    if (OptionToggles[i])
                        _optionBitFlags |= (1u << i);
                }
            }

            private void WriteIndices(ResFileSaver saver, short[] indices)
            {
                for (short i = 0; i < indices.Length; i++)
                {
                    if (indices[i] != -1)
                        saver.Write(i);
                }
                saver.Write(indices);
                saver.Align(8);
            }

            private void WriteIndices(ResFileSaver saver, sbyte[] indices)
            {
                for (sbyte i = 0; i < indices.Length; i++)
                {
                    if (indices[i] != -1)
                        saver.Write(i);
                }
                saver.Write(indices);
                saver.Align(8);
            }
        }

        public class ShaderAssignV10 : ShaderAssign, IResData
        {
            public ResDict RenderInfos = new ResDict();
            public ResDict ShaderParameters = new ResDict();

            internal long shaderParamOffset;
            internal long renderInfoListOffset;

            public ushort ShaderParamSize;

            public ushort RenderInfoCount;
            public ushort ParamCount;

            public Material ParentMaterial;

            void IResData.Load(ResFileLoader loader)
            {
                ShaderArchiveName = loader.LoadString();
                ShadingModelName = loader.LoadString();

                //List of names + type. Data in material section
                renderInfoListOffset = loader.ReadOffset();
                RenderInfos = loader.LoadDict();
                //List of names + type. Data in material section
                shaderParamOffset = loader.ReadOffset();
                ShaderParameters = loader.LoadDict();
                AttribAssignDict = loader.LoadDict();
                SamplerAssignDict = loader.LoadDict();
                ShaderOptionDict = loader.LoadDict();
                RenderInfoCount = loader.ReadUInt16(); //render info count
                ParamCount = loader.ReadUInt16(); //param count
                ShaderParamSize = loader.ReadUInt16();
                loader.ReadUInt16(); //padding
                loader.ReadUInt32(); //padding

                //tool flag to keep track of exported materials that bind shader assign
                uint flag = loader.ReadUInt32();
                IsAnimationBinded = flag == 1;
            }

            void IResData.Save(ResFileSaver saver)
            {
                Console.WriteLine($"Shader assign {saver.Position} {ParentMaterial.Name}");

                RenderInfos = ParentMaterial.RenderInfoDict;
                ShaderParameters = ParentMaterial.ShaderParamDict;

                saver.SaveRelocateEntryToSection(saver.Position, 9, 1, 0, ResFileSaver.Section1, "ShaderAssignV10");

                saver.SaveString(ShaderArchiveName);
                saver.SaveString(ShadingModelName);
                saver.SaveCustom(new long[ParentMaterial.RenderInfos.Count], () =>
                {
                    saver.SaveRelocateEntryToSection(saver.Position, 1, (uint)ParentMaterial.RenderInfos.Count, 1, ResFileSaver.Section1, "Render Param Info V10");

                    foreach (var renderInfo in ParentMaterial.RenderInfos)
                    {
                        saver.SaveString(renderInfo.Name);
                        saver.Write((byte)renderInfo.Type);
                        saver.Write(new byte[7]);
                    }
                });
                saver.SaveDict(RenderInfos);
                saver.SaveCustom(new long[ParentMaterial.ShaderParams.Count], () =>
                {
                    saver.SaveRelocateEntryToSection(saver.Position, 2, (uint)ParentMaterial.ShaderParams.Count, 1, ResFileSaver.Section1, "Shader Param Info V10");

                    foreach (var param in ParentMaterial.ShaderParams)
                    {
                        saver.Write(new byte[8]);
                        saver.SaveString(param.Name);
                        saver.Write((ushort)param.DataOffset);
                        saver.Write((ushort)param.Type);
                        saver.Write(new byte[4]);
                    }
                });
                saver.SaveDict(ShaderParameters);
                saver.SaveDict(AttribAssignDict);
                saver.SaveDict(SamplerAssignDict);
                saver.SaveDict(ShaderOptionDict);
                saver.Write((ushort)ParentMaterial.RenderInfos.Count);
                saver.Write((ushort)ParentMaterial.ShaderParams.Count);
                saver.Write((ushort)ParentMaterial.ShaderParamData.Length);
                saver.Write((ushort)0);//padding
                saver.Write(0);//padding
                saver.Write(IsAnimationBinded ? 1 : 0); //tool flag to keep track of exported materials that bind shader assign
            }
        }
    }
}
