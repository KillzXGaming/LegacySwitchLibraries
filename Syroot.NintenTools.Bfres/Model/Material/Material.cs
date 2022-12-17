using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FMAT subsection of a <see cref="Model"/> subfile, storing information on with which textures and
    /// how technically a surface is drawn.
    /// </summary>
    [DebuggerDisplay(nameof(Material) + " {" + nameof(Name) + "}")]
    public class Material : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class.
        /// </summary>
        public Material()
        {
            Name = "";
            RenderInfoDict = new ResDict();
            ShaderAssign = new ShaderAssign();
            SamplerDict = new ResDict();
            ShaderParamDict = new ResDict();
            UserDataDict = new ResDict();
            Flags = MaterialFlags.Visible;
            SourceParamOffset = 0;


            RenderInfos = new List<RenderInfo>();
            TextureRefs = new List<string>();
            Samplers = new List<Sampler>();
            UserDatas = new List<UserData>();
            ShaderParams = new List<ShaderParam>();
            ShaderParamData = new byte[0];

            VolatileFlags = new byte[0];

            TextureSlotArray = new long[0];
            SamplerSlotArray = new long[0];
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FMAT";

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class from the given <paramref name="stream"/> which
        /// is optionally left open.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        public void Import(Stream stream, bool leaveOpen = false)
        {
            using (ResFileLoader loader = new ResFileLoader(this, stream, leaveOpen))
            {
                loader.ImportMaterials();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public void Import(string fileName)
        {
            using (ResFileLoader loader = new ResFileLoader(this, fileName))
            {
                loader.ImportMaterials();
            }
        }

        /// <summary>
        /// Saves the contents in the given <paramref name="stream"/> and optionally leaves it open
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the contents into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        public void Export(Stream stream, ResFile resFile, bool leaveOpen = false)
        {
            using (ResFileSaver saver = new ResFileSaver(this, resFile, stream, leaveOpen))
            {
                saver.ExportMaterials();
            }
        }

        /// <summary>
        /// Saves the contents in the file with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to save the contents into.</param>
        public void Export(string fileName, ResFile resFile)
        {
            using (ResFileSaver saver = new ResFileSaver(this, resFile, fileName))
            {
                saver.ExportMaterials();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Material}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets flags specifying how a <see cref="Material"/> is rendered.
        /// </summary>
        public MaterialFlags Flags { get; set; }

        public ResDict RenderInfoDict { get; set; }

        public IList<RenderInfo> RenderInfos { get; set; }

        public ShaderAssign ShaderAssign { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="string"/> instances referencing the <see cref="Texture"/> instances
        /// required to draw the material.
        /// </summary>
        public IList<string> TextureRefs { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of <see cref="Sampler"/> instances which configure how to draw
        /// <see cref="Texture"/> instances referenced by the <see cref="TextureRefs"/> list.
        /// </summary>
        public ResDict SamplerDict { get; set; }

        public IList<Sampler> Samplers { get; set; }

        public ResDict ShaderParamDict { get; set; }

        public IList<ShaderParam> ShaderParams { get; set; }

        /// <summary>
        /// Gets or sets the raw data block which stores <see cref="ShaderParam"/> values.
        /// </summary>
        public byte[] ShaderParamData { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public ResDict UserDataDict { get; set; }

        public IList<UserData> UserDatas { get; set; }

        /// <summary>
        /// Gets or sets a set of bits determining whether <see cref="ShaderParam"/> instances are volatile.
        /// </summary>
        // TODO: Wrap into a bool array.
        public byte[] VolatileFlags { get; set; }

        public long[] TextureSlotArray { get; set; }
        public long[] SamplerSlotArray { get; set; }

        public int[] ParamIndices { get; set; }

        public long SourceParamOffset;

        // TODO: Methods to access ShaderParam variable values.

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.ResFile.VersionMajor2 >= 9)
                Flags = loader.ReadEnum<MaterialFlags>(true);
            else
                loader.LoadHeaderBlock();

            Name = loader.LoadString();

            if (loader.ResFile.VersionMajor2 >= 10)
            {
                 MaterialParserV10.Load(loader, this);
                return;
            }

            long RenderInfoArrayOffset    = loader.ReadInt64();
            RenderInfoDict                = loader.LoadDict();
            ShaderAssign                  = loader.Load<ShaderAssign>();
            long TextureArrayOffset       = loader.ReadInt64();
            long TextureNameArray         = loader.ReadInt64();
            long SamplerArrayOffset       = loader.ReadInt64();
            long SamplerInfoArray         = loader.ReadInt64();
            SamplerDict                   = loader.LoadDict();
            long ShaderParamArrayOffset   = loader.ReadInt64();
            ShaderParamDict               = loader.LoadDict();
            SourceParamOffset             = loader.ReadInt64(); 
            long UserDataOffset           = loader.ReadInt64();
            UserDataDict                  = loader.LoadDict();
            long VolatileFlagsOffset      = loader.ReadInt64();
            long userPointer              = loader.ReadInt64();
            long SamplerSlotArrayOffset   = loader.ReadInt64();
            long TexSlotArrayOffset       = loader.ReadInt64();
            if (loader.ResFile.VersionMajor2 != 9)
                Flags = loader.ReadEnum<MaterialFlags>(true);
            ushort idx                    = loader.ReadUInt16();
            ushort numRenderInfo          = loader.ReadUInt16();
            byte numTextureRef = loader.ReadByte();
            byte numSampler               = loader.ReadByte();
            ushort numShaderParam         = loader.ReadUInt16();
            ushort numShaderParamVolatile = loader.ReadUInt16();
            ushort sizParamSource         = loader.ReadUInt16();
            ushort sizParamRaw            = loader.ReadUInt16();
            ushort numUserData            = loader.ReadUInt16();

            if (loader.ResFile.VersionMajor2 != 9)
                 loader.ReadUInt32(); //Padding

            RenderInfos      = loader.LoadList<RenderInfo>(numRenderInfo, RenderInfoArrayOffset);
            TextureRefs      = loader.LoadCustom(() => loader.LoadStrings(numTextureRef), TextureNameArray);
            Samplers         = loader.LoadList<Sampler>(numSampler, SamplerInfoArray);
            UserDatas        = loader.LoadList<UserData>(numUserData, UserDataOffset);
            ShaderParams     = loader.LoadList<ShaderParam>(numShaderParam, ShaderParamArrayOffset);
            ShaderParamData  = loader.LoadCustom(() => loader.ReadBytes(sizParamSource), SourceParamOffset);
    
            VolatileFlags = loader.LoadCustom(() => loader.ReadBytes((int)Math.Ceiling(numShaderParam / 8f)), VolatileFlagsOffset);

            TextureSlotArray = loader.LoadCustom(() => loader.ReadInt64s(numTextureRef), SamplerSlotArrayOffset);
            SamplerSlotArray = loader.LoadCustom(() => loader.ReadInt64s(numSampler), TexSlotArrayOffset);
        }

        internal long PosRenderInfoOffset;
        internal long PosRenderInfoDictOffset;
        internal long PosShaderAssignOffset;
        internal long PosTextureUnk1Offset;
        internal long PosTextureRefsOffset;
        internal long PosTextureUnk2Offset;
        internal long PosSamplersOffset;
        internal long PosSamplerDictOffset;
        internal long PosShaderParamsOffset;
        internal long PosShaderParamDictOffset;
        internal long PosShaderParamDataOffset;
        internal long PosUserDataMaterialOffset;
        internal long PosUserDataDictMaterialOffset;
        internal long PosVolatileFlagsOffset;
        internal long PosSamplerSlotArrayOffset;
        internal long PosTextureSlotArrayOffset;

        void IResData.Save(ResFileSaver saver)
        {
            if (VolatileFlags == null)
                VolatileFlags = new byte[0];
            if (TextureRefs == null)
                TextureRefs = new List<string>();
            if (ShaderParamData == null)
                ShaderParamData = new byte[0];

            //Note these change amount believe by texture ref.
            long[] unkown2 = new long[TextureRefs.Count];//Set at runtime????
            long[] unkown1 = new long[TextureRefs.Count * 15]; //Set at runtime????

            saver.WriteSignature(_signature);
            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write(Flags, true);
            else
                saver.Seek(12);

            if (saver.ResFile.VersionMajor2 >= 10)
            {
                MaterialParserV10.Save(saver, this);
                return;
            }

            saver.SaveRelocateEntryToSection(saver.Position, 15, 1, 0, ResFileSaver.Section1, "FMAT"); //      <------------ Entry Set
            saver.SaveString(Name);

            PosRenderInfoOffset = saver.SaveOffset();
            PosRenderInfoDictOffset = saver.SaveOffset();
            PosShaderAssignOffset = saver.SaveOffset();
            PosTextureUnk1Offset = saver.SaveOffset();
            PosTextureRefsOffset = saver.SaveOffset();
            PosTextureUnk2Offset = saver.SaveOffset();
            PosSamplersOffset = saver.SaveOffset();
            PosSamplerDictOffset = saver.SaveOffset();
            PosShaderParamsOffset = saver.SaveOffset();
            PosShaderParamDictOffset = saver.SaveOffset();
            PosShaderParamDataOffset = saver.SaveOffset();
            PosUserDataMaterialOffset = saver.SaveOffset();
            PosUserDataDictMaterialOffset = saver.SaveOffset();
            PosVolatileFlagsOffset = saver.SaveOffset();
            saver.Write((long)0);

            //Set the slot offsets for both sampler and texture
            saver.SaveRelocateEntryToSection(saver.Position, 2, 1, 0, ResFileSaver.Section1, "Material texture slots"); //      <------------ Entry Set
            PosSamplerSlotArrayOffset = saver.SaveOffset();
            PosTextureSlotArrayOffset = saver.SaveOffset();
            if (saver.ResFile.VersionMajor2 != 9)
                saver.Write(Flags, true);
            saver.Write((ushort)saver.CurrentIndex);
            saver.Write((ushort)RenderInfoDict.Count);
            saver.Write((byte)Samplers.Count);
            saver.Write((byte)TextureRefs.Count);
            saver.Write((ushort)ShaderParams.Count);
            saver.Write((ushort)0); //VolatileFlags.Length
            saver.Write((ushort)ShaderParamData.Length);
            saver.Write((ushort)0); // SizParamRaw
            saver.Write((ushort)UserDataDict.Count);

            if (saver.ResFile.VersionMajor2 != 9)
                saver.Write(0); // padding
        }
    }

    /// <summary>
    /// Represents general flags specifying how a <see cref="Material"/> is rendered.
    /// </summary>
    public enum MaterialFlags : uint
    {
        /// <summary>
        /// The material is not rendered at all.
        /// </summary>
        None,

        /// <summary>
        /// The material is rendered.
        /// </summary>
        Visible
    }
}