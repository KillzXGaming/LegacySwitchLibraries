using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Syroot.NintenTools.NSW.Bfres.Core;
using System.ComponentModel;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FMAA section in a <see cref="ResFile"/> subfile, storing material animation data.
    /// </summary>
    [DebuggerDisplay(nameof(MaterialAnim) + " {" + nameof(Name) + "}")]
    public class MaterialAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAnim"/> class.
        /// </summary>
        public MaterialAnim()
        {
            Name = "";
            Path = "";
            BindModel = new Model();
            BindIndices = new ushort[0];
            TextureNames = new List<string>();
            TextureBindArray = new List<long>();

            Flags = 0;
            FrameCount = 1;
            BakedSize = 0;
            ShaderParamAnimCount = 0;
            TexturePatternAnimCount = 0;
            VisabiltyAnimCount = 0;
            TextureCount = 0;

            MaterialAnimDataList = new List<MaterialAnimData>();
            UserData = new List<UserData>();
            UserDataDict = new ResDict();
        }

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private const string _signature = "FMAA";
        
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
                loader.ImportMaterialAnims();
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
                loader.ImportMaterialAnims();
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
                saver.ExportMaterialAnimation();
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
                saver.ExportMaterialAnimation();
            }
        }


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{MaterialAnim}"/>
        /// instances.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        [DisplayName("Frame Count")]
        public int FrameCount { get; set; }

        [Browsable(true)]
        [Category("Animation")]
        public bool Loop
        {
            get
            {
                return Flags.HasFlag(MaterialAnimFlags.Looping);
            }
            set
            {
                if (value == true)
                    Flags |= MaterialAnimFlags.Looping;
                else
                    Flags &= ~MaterialAnimFlags.Looping;
            }
        }

        [Browsable(true)]
        [Category("Animation")]
        public bool Baked
        {
            get
            {
                return Flags.HasFlag(MaterialAnimFlags.BakedCurve);
            }
            set
            {
                if (value == true)
                    Flags |= MaterialAnimFlags.BakedCurve;
                else
                    Flags &= ~MaterialAnimFlags.BakedCurve;
            }
        }

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="Curves"/>.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Model"/> instance affected by this animation.
        /// </summary>
        [Browsable(false)]
        public Model BindModel { get; set; }

        /// <summary>
        /// Gets the indices of the <see cref="Material"/> instances in the <see cref="Model.Materials"/> dictionary to
        /// bind for each animation. <see cref="UInt16.MaxValue"/> specifies no binding.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        public ushort[] BindIndices { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        [Browsable(false)]
        public ResDict UserDataDict { get; set; }

        [Browsable(false)]
        public IList<UserData> UserData { get; set; }

        [Browsable(false)]
        public IList<MaterialAnimData> MaterialAnimDataList { get; set; }

        [Browsable(false)]
        public IList<string> TextureNames { get; set; }

        [Browsable(false)]
        public IList<long> TextureBindArray { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SkeletalAnimFlags"/> mode used to control looping and baked settings.
        /// </summary>
        [Browsable(false)]
        public MaterialAnimFlags Flags { get; set; }

        [Browsable(true)]
        [ReadOnly(true)]
        public ushort ShaderParamAnimCount { get; set; }

        [Browsable(true)]
        [ReadOnly(true)]
        public ushort TexturePatternAnimCount { get; set; }

        [Browsable(true)]
        [ReadOnly(true)]
        public ushort VisabiltyAnimCount { get; set; }

        [Browsable(true)]
        [ReadOnly(true)]
        public ushort TextureCount { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                Flags = loader.ReadEnum<MaterialAnimFlags>(true);
                loader.ReadUInt16();
            }
            else
                loader.LoadHeaderBlock();

            Name = loader.LoadString();
            Path = loader.LoadString();
            BindModel = loader.Load<Model>(true);
            long BindIndicesOffset = loader.ReadOffset();
            long PerMaterialAnimationArrayOffset = loader.ReadOffset();
            long unk = loader.ReadOffset(); //Empty section. Maybe set at runtime
            long TextureNameArrayOffset = loader.ReadOffset();
            long UserDataOffset = loader.ReadOffset();
            UserDataDict = loader.LoadDict();
            long TextureBindArrayOffset = loader.ReadOffset();

            if (loader.ResFile.VersionMajor2 < 9)
                Flags = loader.ReadEnum<MaterialAnimFlags>(true);

            ushort numUserData = 0;
            ushort PerMatAnimCount = 0;
            ushort CurveCount = 0;

            if (loader.ResFile.VersionMajor2 >= 9)
            {
                FrameCount = loader.ReadInt32();
                BakedSize = loader.ReadUInt32();
                numUserData = loader.ReadUInt16();
                PerMatAnimCount = loader.ReadUInt16();
                CurveCount = loader.ReadUInt16();
            }
            else
            {
                numUserData = loader.ReadUInt16();
                PerMatAnimCount = loader.ReadUInt16();
                CurveCount = loader.ReadUInt16();
                FrameCount = loader.ReadInt32();
                BakedSize = loader.ReadUInt32();
            }

            ShaderParamAnimCount = loader.ReadUInt16();
            TexturePatternAnimCount = loader.ReadUInt16();
            VisabiltyAnimCount = loader.ReadUInt16();
            TextureCount = loader.ReadUInt16();

            if (loader.ResFile.VersionMajor2 >= 9)
                 loader.ReadUInt16(); //padding

            BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(PerMatAnimCount), BindIndicesOffset);
            MaterialAnimDataList = loader.LoadList<MaterialAnimData>(PerMatAnimCount, PerMaterialAnimationArrayOffset);
            TextureNames = loader.LoadCustom(() => loader.LoadStrings(TextureCount), TextureNameArrayOffset);
            TextureBindArray = loader.LoadCustom(() => loader.ReadInt64s(TextureCount), TextureBindArrayOffset);
            UserData = loader.LoadList<UserData>(numUserData, UserDataOffset);
        }

        internal long PosBindModelOffset;
        internal long PosBindIndicesOffset;
        internal long PosMatAnimDataOffset;
        internal long PosTextureNamesUnkOffset;
        internal long PosTextureNamesOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;
        internal long PosTextureBindArrayOffset;

        internal void UpdateIndices()
        {
            int ShaderParamCurveIndex = 0;
            int TexturePatternCurveIndex = 0;
            int BeginVisalConstantIndex = 0;
            int VisalCurveIndex = 0;
            int VisualConstantIndex = 0;

            foreach (MaterialAnimData material in MaterialAnimDataList)
            {

                foreach (TexturePatternAnimInfo info in material.TexturePatternAnimInfos)
                {
            
                }


            }
        }

        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(Flags, true);
                saver.Seek(2);
            }
            else
                saver.Seek(12);

            saver.SaveRelocateEntryToSection(saver.Position, 2, 1, 0, ResFileSaver.Section1, "Material Animation"); //      <------------ Entry Set
            saver.SaveString(Name);
            saver.SaveString(Path);
            saver.Write(0L); //Bind Model
            saver.SaveRelocateEntryToSection(saver.Position, 7, 1, 0, ResFileSaver.Section1, "Material Animation"); //      <------------ Entry Set
            PosBindIndicesOffset = saver.SaveOffset();
            PosMatAnimDataOffset = saver.SaveOffset();
            PosTextureNamesUnkOffset = saver.SaveOffset();
            PosTextureNamesOffset = saver.SaveOffset();
            PosUserDataOffset = saver.SaveOffset();
            PosUserDataDictOffset = saver.SaveOffset();
            PosTextureBindArrayOffset = saver.SaveOffset();
            if (saver.ResFile.VersionMajor2 < 9)
                saver.Write(Flags, true);

            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(FrameCount);
                saver.Write(BakedSize);
                saver.Write((ushort)UserData.Count);
                saver.Write((ushort)MaterialAnimDataList.Count);
                saver.Write((ushort)MaterialAnimDataList.Sum((x) => x.Curves.Count));
            }
            else
            {
                saver.Write((ushort)UserData.Count);
                saver.Write((ushort)MaterialAnimDataList.Count);
                saver.Write((ushort)MaterialAnimDataList.Sum((x) => x.Curves.Count));
                saver.Write(FrameCount);
                saver.Write(BakedSize);
            }

            int constantCount = MaterialAnimDataList.Sum((x) => x.Constants != null ? x.Constants.Count : 0);
            int paramsCount = 0;
            int texpatCount = MaterialAnimDataList.Sum((x) => x.TexturePatternAnimInfos.Count);

            for (int i = 0; i < MaterialAnimDataList.Count; i++)
            {
               foreach (var paramInfo in MaterialAnimDataList[i].ParamAnimInfos)
                    paramsCount += paramInfo.ConstantCount + paramInfo.FloatCurveCount + paramInfo.IntCurveCount;
            }

            saver.Write((ushort)paramsCount);

            if (texpatCount != 0)
                saver.Write((ushort)(texpatCount + constantCount));
            else
                saver.Write((ushort)0);

            saver.Write(VisabiltyAnimCount);

            if (TextureNames != null)
                saver.Write((ushort)TextureNames.Count);
            else
                saver.Write((ushort)TextureCount);

            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write((ushort)0);
        }

        /// <summary>
        /// Represents flags specifying how animation data is stored or should be played.
        /// </summary>
        [Flags]
        public enum MaterialAnimFlags : ushort
        {
            /// <summary>
            /// The stored curve data has been baked.
            /// </summary>
            BakedCurve = 1 << 0,

            /// <summary>
            /// The animation repeats from the start after the last frame has been played.
            /// </summary>
            Looping = 1 << 2
        }
    }
}
