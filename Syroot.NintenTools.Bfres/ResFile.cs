using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using Syroot.BinaryData;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents a NintendoWare for Cafe (NW4F) graphics data archive file.
    /// </summary>
    [DebuggerDisplay(nameof(ResFile) + " {" + nameof(Name) + "}")]
    public class ResFile : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FRES";

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class.
        /// </summary>
        public ResFile()
        {
            Name = "";
            Alignment = 0xC;
            TargetAddressSize = 0;
            Flag = 0;

            VersionMajor  = 0;
            VersionMajor2 = 5;
            VersionMinor  = 0;
            VersionMinor2 = 3;

            //Initialize memory, string table, and buffer data
            MemoryPool = new MemoryPool();
            BufferInfo = new BufferInfo();
            StringTable = new StringTable();

            //Initialize lists
            Models = new List<Model>();
            SkeletalAnims = new List<SkeletalAnim>();
            MaterialAnims = new List<MaterialAnim>();
            BoneVisibilityAnims = new List<VisibilityAnim>();
            ShapeAnims = new List<ShapeAnim>();
            SceneAnims = new List<SceneAnim>();
            ExternalFiles = new List<ExternalFile>();

            //Initialize Dictionaries
            ModelDict = new ResDict();
            SkeletalAnimDict = new ResDict();
            MaterialAnimDict = new ResDict();
            BoneVisibilityAnimDict = new ResDict();
            ShapeAnimDict = new ResDict();
            SceneAnimDict = new ResDict();
            ExternalFileDict = new ResDict();

            BlockOffset = 208;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class from the given <paramref name="stream"/> which
        /// is optionally left open.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        public ResFile(Stream stream, bool leaveOpen = false)
        {
            using (ResFileLoader loader = new ResFileLoader(this, stream, leaveOpen))
            {
                loader.Execute();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public ResFile(string fileName)
        {
            using (ResFileLoader loader = new ResFileLoader(this, fileName))
            {
                loader.Execute();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a name describing the contents.
        /// </summary>
        [Browsable(true)]
        [Category("Binary Info")]
        [DisplayName("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alignment to use for raw data blocks in the file.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Binary Info")]
        [DisplayName("Alignment")]
        public int DataAlignment
        {
            get
            {
                return (1 << (int)Alignment);
            }
        }

        public uint DataAlignmentOverride = 0;

        /// <summary>
        /// Gets or sets the major revision of the BFRES structure formats.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Version")]
        [DisplayName("Version Full")]
        public string VersionFull
        {
            get
            {
                return $"{VersionMajor},{VersionMajor2},{VersionMinor},{VersionMinor2}";
            }
        }

        internal uint Version { get; set; }

        /// <summary>
        /// Gets or sets the major revision of the BFRES structure formats.
        /// </summary>
        [Browsable(true)]
        [Category("Version")]
        [DisplayName("Version Major")]
        public uint VersionMajor { get; set; }
        /// <summary>
        /// Gets or sets the second major revision of the BFRES structure formats.
        /// </summary>
        [Browsable(true)]
        [Category("Version")]
        [DisplayName("Version Major 2")]
        public uint VersionMajor2 { get; set; }
        /// <summary>
        /// Gets or sets the minor revision of the BFRES structure formats.
        /// </summary>
        [Browsable(true)]
        [Category("Version")]
        [DisplayName("Version Minor")]
        public uint VersionMinor { get; set; }
        /// <summary>
        /// Gets or sets the second minor revision of the BFRES structure formats.
        /// </summary>
        [Browsable(true)]
        [Category("Version")]
        [DisplayName("Version Minor 2")]
        public uint VersionMinor2 { get; set; }

        /// <summary>
        /// Gets the byte order in which data is stored. Must be the endianness of the target platform.
        /// </summary>
        [Browsable(false)]
        public ByteOrder ByteOrder { get; private set; } = ByteOrder.LittleEndian;


        /// <summary>
        /// Gets or sets the alignment value.
        /// </summary>
        [Browsable(false)]
        public uint Alignment { get; set; }

        /// <summary>
        /// Gets or sets the target adress size to use for raw data blocks in the file.
        /// </summary>
        [Browsable(false)]
        public uint TargetAddressSize { get; set; }

        /// <summary>
        /// Gets or sets the flag. Unknown purpose.
        /// </summary>
        [Browsable(false)]
        public uint Flag { get; set; }

        /// <summary>
        /// Gets or sets the BlockOffset. 
        /// </summary>
        [Browsable(false)]
        public uint BlockOffset { get; set; }


        /// <summary>
        /// Gets or sets the stored <see cref="Model"/> (FMDL) names.
        /// </summary>
        [Browsable(false)]
        public ResDict ModelDict { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="Model"/> (FMDL) instances.
        /// </summary>
        [Browsable(false)]
        public IList<Model> Models { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="SkeletalAnim"/> (FSKA) instances.
        /// </summary>
        [Browsable(false)]
        public ResDict SkeletalAnimDict { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="SkeletalAnim"/> (FSKA)  instances for many types of skeletal animations.
        /// </summary>
        [Browsable(false)]
        public IList<SkeletalAnim> SkeletalAnims { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="MaterialAnim"/> (FMAA) names.
        /// </summary>
        [Browsable(false)]
        public ResDict MaterialAnimDict { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="MaterialAnim"/> (FMAA) instances for many types of material animations.
        /// </summary>
        [Browsable(false)]
        public IList<MaterialAnim> MaterialAnims { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="VisibilityAnim"/> (FVIS) names
        /// </summary>
        [Browsable(false)]
        public ResDict BoneVisibilityAnimDict { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="VisibilityAnim"/> (FVIS) instances for bone visibility animations.
        /// </summary>
        [Browsable(false)]
        public IList<VisibilityAnim> BoneVisibilityAnims { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="ShapeAnim"/> (FSHA) names.
        /// </summary>
        [Browsable(false)]
        public ResDict ShapeAnimDict { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="ShapeAnim"/> (FSHA) instances.
        /// </summary>
        [Browsable(false)]
        public IList<ShapeAnim> ShapeAnims { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="SceneAnim"/> (FSCN) names.
        /// </summary>
        [Browsable(false)]
        public ResDict SceneAnimDict { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="SceneAnim"/> (FSCN) instances.
        /// </summary>
        [Browsable(false)]
        public IList<SceneAnim> SceneAnims { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="MemoryPool"/> instances. 
        /// </summary>
        [Browsable(false)]
        public MemoryPool MemoryPool { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="BufferInfo"/> instances.
        /// </summary>
        [Browsable(false)]
        public BufferInfo BufferInfo { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="ExternalFile"/> names
        /// </summary>
        [Browsable(false)]
        public ResDict ExternalFileDict { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="ExternalFile"/> instances. The key of the dictionary typically represents
        /// the name of the file they were originally created from.
        /// </summary>
        [Browsable(false)]
        public IList<ExternalFile> ExternalFiles { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Saves the contents in the given <paramref name="stream"/> and optionally leaves it open
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the contents into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        public void Save(Stream stream, bool leaveOpen = false)
        {
            using (ResFileSaver saver = new ResFileSaver(this, stream, leaveOpen))
            {
                saver.Execute();
            }
        }

        /// <summary>
        /// Saves the contents in the file with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to save the contents into.</param>
        public void Save(string fileName)
        {
            using (ResFileSaver saver = new ResFileSaver(this, fileName))
            {
                saver.Execute();
            }
        }

        internal uint SaveVersion()
        {
            return VersionMajor << 24 | VersionMajor2 << 16  | VersionMinor << 8  | VersionMinor2;
        }

        internal void SetVersionInfo(uint Version)
        {
            VersionMajor = Version >> 24;
            VersionMajor2 = Version >> 16 & 0xFF;
            VersionMinor = Version >> 8 & 0xFF;
            VersionMinor2 = Version & 0xFF;
        }

        [Browsable(false)]
        public StringTable StringTable { get; set; }

        public static bool UseExternalGPU = false;

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            uint padding = loader.ReadUInt32();
            Version = loader.ReadUInt32();
            SetVersionInfo(Version);
            ByteOrder = loader.ReadEnum<ByteOrder>(false);
            Alignment = loader.ReadByte();
            TargetAddressSize = loader.ReadByte(); //Thanks MasterF0X for pointing out the layout of the these
            uint OffsetToFileName = loader.ReadUInt32();
            Flag = loader.ReadUInt16();
            BlockOffset = loader.ReadUInt16();
            uint RelocationTableOffset = loader.ReadUInt32();
            uint sizFile = loader.ReadUInt32();
            Name = loader.LoadString();

            // loader.Load<RelocationTableTest>(RelocationTableOffset);

            long ModelArrayOffset = loader.ReadOffset();
            ModelDict = loader.LoadDict();

            if (loader.ResFile.VersionMajor2 >= 9)
            {
                long unkOffset = loader.ReadOffset();
                long unkDictOffset = loader.ReadOffset();
                long unk2Offset = loader.ReadOffset();
                long unk2DictOffset = loader.ReadOffset();
            }

            long SkeletalAnimArrayOffset = loader.ReadOffset();
            SkeletalAnimDict = loader.LoadDict();
            long MaterialArrayAnimOffset = loader.ReadOffset();
            MaterialAnimDict = loader.LoadDict();
            long BoneVisibilityAnimArrayOffset = loader.ReadOffset();
            BoneVisibilityAnimDict = loader.LoadDict();
            long ShapeAnimArrayOffset = loader.ReadOffset();
            ShapeAnimDict = loader.LoadDict();
            long SceneAnimArrayOffset = loader.ReadOffset();
            SceneAnimDict = loader.LoadDict();
            MemoryPool = loader.Load<MemoryPool>(true);
            BufferInfo = loader.Load<BufferInfo>(true);
            long ExternalFileOffset = loader.ReadOffset();
            ExternalFileDict = loader.LoadDict();
            long padding1 = loader.ReadInt64();
            StringTable = loader.Load<StringTable>(true);
            uint StringPoolSize = loader.ReadUInt32();
            ushort numModel = loader.ReadUInt16();

            System.Console.WriteLine($"ModelArrayOffset {ModelArrayOffset}");
            System.Console.WriteLine($"numModel {numModel}");

            if (loader.ResFile.VersionMajor2 >= 9)
            {
                //Count for 2 reserved sections
                ushort unkCount = loader.ReadUInt16();
                ushort unk2Count = loader.ReadUInt16();

                if (unkCount != 0) throw new System.Exception("unk1 has section!");
                if (unk2Count != 0) throw new System.Exception("unk2 has section!");
            }

            ushort numSkeletalAnim = loader.ReadUInt16();
            ushort numMaterialAnim = loader.ReadUInt16();
            ushort numBoneVisibilityAnim = loader.ReadUInt16();
            ushort numShapeAnim = loader.ReadUInt16();
            ushort numSceneAnim = loader.ReadUInt16();
            ushort numExternalFile = loader.ReadUInt16();
            byte externalFlags = loader.ReadByte();
            byte reserve10 = loader.ReadByte();

            //If string cache for TOTK 
            if (externalFlags == 4)
            {
                using (loader.TemporarySeek(ExternalFileOffset, SeekOrigin.Begin))
                {
                    StringCache.Strings.Clear();
                    foreach (var str in StringTable.Strings)
                    {
                        long stringID = loader.ReadInt64();
                        StringCache.Strings.Add(stringID, str);
                    }
                }
                return;
            }
            //GPU section for TOTK
            if (externalFlags == 11)
            {
                UseExternalGPU = true;
                using (loader.TemporarySeek(sizFile, SeekOrigin.Begin))
                {
                    uint gpuDataOffset = loader.ReadUInt32();
                    uint gpuBufferSize = loader.ReadUInt32();

                //    loader.Seek(gpuDataOffset, SeekOrigin.Begin);
                    BufferInfo = new BufferInfo();
                    BufferInfo.BufferOffset = sizFile + 288;
                }
            }

            //Now load each subfile by list. 
            Models = loader.LoadList<Model>(numModel, ModelArrayOffset);
            SkeletalAnims = loader.LoadList<SkeletalAnim>(numSkeletalAnim, SkeletalAnimArrayOffset);
            MaterialAnims = loader.LoadList<MaterialAnim>(numMaterialAnim, MaterialArrayAnimOffset);
            BoneVisibilityAnims = loader.LoadList<VisibilityAnim>(numBoneVisibilityAnim, BoneVisibilityAnimArrayOffset);
            ShapeAnims = loader.LoadList<ShapeAnim>(numShapeAnim, ShapeAnimArrayOffset);
            SceneAnims = loader.LoadList<SceneAnim>(numSceneAnim, SceneAnimArrayOffset);
            ExternalFiles = loader.LoadList<ExternalFile>(numExternalFile, ExternalFileOffset);
        }

        internal long ModelOffset = 0;
        internal long SkeletonAnimationOffset = 0;
        internal long MaterialAnimationOffset = 0;
        internal long ShapeAnimationOffset = 0;
        internal long BoneVisAnimationOffset = 0;
        internal long SceneAnimationOffset = 0;
        internal long ExternalFileOffset = 0;


        internal long ModelDictOffset = 0;
        internal long SkeletonAnimationDictOffset = 0;
        internal long MaterialAnimationnDictOffset = 0;
        internal long ShapeAnimationDictOffset = 0;
        internal long BoneVisAnimationDictOffset = 0;
        internal long SceneAnimationDictOffset = 0;
        internal long ExternalFileDictOffset = 0;

        internal long BufferInfoOffset = 0;
        
        void IResData.Save(ResFileSaver saver)
        {
          //  if (saver.ResFile.VersionMajor2 > 9)
              //  throw new System.Exception($"Version {saver.ResFile.VersionMajor2} does not support saving!");

            PreSave();

            saver.WriteSignature(_signature);
            saver.Write(0x20202020);
            saver.Write(SaveVersion());
            saver.Write(ByteOrder, true);
            saver.Write((byte)Alignment);
            saver.Write((byte)TargetAddressSize);
            saver.SaveFileNameString(Name);
            saver.Write((ushort)Flag);
            saver.SaveHeaderBlock(true);
            saver.SaveRelocationTablePointerPointer();
            saver.SaveFieldFileSize();

            if (saver.ResFile.VersionMajor2 == 9)
                saver.SaveRelocateEntryToSection(saver.Position, 15, 1, 0, ResFileSaver.Section1, "ResFile"); //      <------------ Entry Set
            else
                saver.SaveRelocateEntryToSection(saver.Position, 13, 1, 0, ResFileSaver.Section1, "ResFile"); //      <------------ Entry Set

            saver.SaveString(Name);
            ModelOffset = saver.SaveOffset();
            ModelDictOffset = saver.SaveOffset();

            //2 New sections
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(0L);
                saver.Write(0L);
                saver.Write(0L);
                saver.Write(0L);
            }

            SkeletonAnimationOffset = saver.SaveOffset();
            SkeletonAnimationDictOffset = saver.SaveOffset();
            MaterialAnimationOffset = saver.SaveOffset();
            MaterialAnimationnDictOffset = saver.SaveOffset();
            BoneVisAnimationOffset = saver.SaveOffset();
            BoneVisAnimationDictOffset = saver.SaveOffset();
            ShapeAnimationOffset = saver.SaveOffset();
            ShapeAnimationDictOffset = saver.SaveOffset();
            SceneAnimationOffset = saver.SaveOffset();
            SceneAnimationDictOffset = saver.SaveOffset();

            if (MemoryPool != null)
                saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSaver.Section4, "Memory pool"); //      <------------ Entry Set
            saver.SaveMemoryPoolPointer();

            if (BufferInfo != null)
                saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSaver.Section1, "Buffer info"); //      <------------ Entry Set

            BufferInfoOffset = saver.SaveOffset();
            if (ExternalFiles.Count > 0)
                saver.SaveRelocateEntryToSection(saver.Position, 2, 1, 0, ResFileSaver.Section1, "External Files"); //      <------------ Entry Set
            ExternalFileOffset = saver.SaveOffset();
            ExternalFileDictOffset = saver.SaveOffset();
            saver.Write(0L); // padding
            saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSaver.Section1, "String pool"); //      <------------ Entry Set
            saver.SaveFieldStringPool();
            saver.Write((ushort)ModelDict.Count);

            //2 New sections
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write((ushort)0);
                saver.Write((ushort)0);
            }

            saver.Write((ushort)SkeletalAnimDict.Count);
            saver.Write((ushort)MaterialAnimDict.Count);
            saver.Write((ushort)BoneVisibilityAnimDict.Count);
            saver.Write((ushort)ShapeAnimDict.Count);
            saver.Write((ushort)SceneAnimDict.Count);
            saver.Write((ushort)ExternalFileDict.Count);

            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Seek(2); // padding
            else
                saver.Seek(6); // padding
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private void SaveDataBlocks()
        {

        }

        private void PreSave()
        {
            //Update dictionaries if possible to the names set in their section
            ModelDict.Clear();
            SkeletalAnimDict.Clear();
            MaterialAnimDict.Clear();
            BoneVisibilityAnimDict.Clear();
            ShapeAnimDict.Clear();
            SceneAnimDict.Clear();

            // Update Shape instances.
            foreach (Model model in Models)
            {
                model.ShapeDict.Clear();
                model.MaterialDict.Clear();
                model.UserDataDict.Clear();
                model.Skeleton.BoneDict.Clear();

                ModelDict.Add(model.Name);
                foreach (VertexBuffer vertexBuffer in model.VertexBuffers)
                {
                    vertexBuffer.AttributeDict.Clear();
                    foreach (VertexAttrib vertexAttrib in vertexBuffer.Attributes)
                    {
                        vertexBuffer.AttributeDict.Add(vertexAttrib.Name);
                    }
                }
                foreach (Shape shape in model.Shapes)
                {
                    model.ShapeDict.Add(shape.Name);
                    shape.VertexBuffer = model.VertexBuffers[shape.VertexBufferIndex];
                }
                foreach (Material material in model.Materials)
                {
                    material.RenderInfoDict.Clear();
                    material.ShaderParamDict.Clear();
                    material.UserDataDict.Clear();

                    model.MaterialDict.Add(material.Name);

                    foreach (RenderInfo renderinfo in material.RenderInfos)
                    {
                        material.RenderInfoDict.Add(renderinfo.Name);
                    }
                    foreach (ShaderParam shaderparam in material.ShaderParams)
                    {
                        material.ShaderParamDict.Add(shaderparam.Name);
                    }
                    foreach (UserData userData in material.UserDatas)
                    {
                        material.UserDataDict.Add(userData.Name);
                    }
                }
                foreach (Bone bone in model.Skeleton.Bones)
                {
                    model.Skeleton.BoneDict.Add(bone.Name);

                    bone.UserDataDict.Clear();
                    foreach (UserData userData in bone.UserData)
                    {
                        bone.UserDataDict.Add(userData.Name);
                    }
                }
                foreach (UserData userData in model.UserData)
                {
                    model.UserDataDict.Add(userData.Name);
                }
            }

            // Update SkeletalAnim instances.
            foreach (SkeletalAnim anim in SkeletalAnims)
            {
                SkeletalAnimDict.Add(anim.Name);
                anim.UserDataDict.Clear();
                foreach (UserData userData in anim.UserDatas)
                {
                    anim.UserDataDict.Add(userData.Name);
                }

                int curveIndex = 0;
                foreach (BoneAnim subAnim in anim.BoneAnims)
                {
                    subAnim.BeginCurve = curveIndex;
                    curveIndex += subAnim.Curves.Count;
                }
            }

            // Update SkeletalAnim instances.
            foreach (MaterialAnim anim in MaterialAnims)
            {
                MaterialAnimDict.Add(anim.Name);
                anim.UserDataDict.Clear();
                foreach (UserData userData in anim.UserData)
                {
                    anim.UserDataDict.Add(userData.Name);
                }

                int curveIndex = 0;
                foreach (MaterialAnimData subAnim in anim.MaterialAnimDataList)
                {
                    //Todo these need to be updated!
                    curveIndex += subAnim.Curves.Count;
                }
            }

            foreach (VisibilityAnim anim in BoneVisibilityAnims)
            {
                BoneVisibilityAnimDict.Add(anim.Name);

                anim.UserDataDict.Clear();
                foreach (UserData userData in anim.UserData)
                {
                    anim.UserDataDict.Add(userData.Name);
                }
            }

            foreach (SceneAnim anim in SceneAnims)
            {
                anim.FogAnimDict.Clear();
                anim.LightAnimDict.Clear();
                anim.CameraAnimDict.Clear();
                SceneAnimDict.Add(anim.Name);

                anim.UserDataDict.Clear();
                foreach (UserData userData in anim.UserData)
                {
                    anim.UserDataDict.Add(userData.Name);
                }
                foreach (FogAnim subAnim in anim.FogAnims)
                {
                    anim.FogAnimDict.Add(subAnim.Name);

                    subAnim.UserDataDict.Clear();
                    foreach (UserData userData in subAnim.UserData)
                    {
                        subAnim.UserDataDict.Add(userData.Name);
                    }
                }
                foreach (LightAnim subAnim in anim.LightAnims)
                {
                    anim.LightAnimDict.Add(subAnim.Name);

                    subAnim.UserDataDict.Clear();
                    foreach (UserData userData in subAnim.UserData)
                    {
                        subAnim.UserDataDict.Add(userData.Name);
                    }
                }
                foreach (CameraAnim subAnim in anim.CameraAnims)
                {
                    anim.CameraAnimDict.Add(subAnim.Name);

                    subAnim.UserDataDict.Clear();
                    foreach (UserData userData in subAnim.UserData)
                    {
                        subAnim.UserDataDict.Add(userData.Name);
                    }
                }
            }

            // Update ShapeAnim instances.
            foreach (ShapeAnim anim in ShapeAnims)
            {
                ShapeAnimDict.Add(anim.Name);

                int curveIndex = 0;
                int infoIndex = 0;
                foreach (VertexShapeAnim subAnim in anim.VertexShapeAnims)
                {
                    subAnim.BeginCurve = curveIndex;
                    subAnim.BeginKeyShapeAnim = infoIndex;
                    curveIndex += subAnim.Curves.Count;
                    infoIndex += subAnim.KeyShapeAnimInfos.Count;
                }
            }
        }

        //Flags thanks to watertoon
        public enum ExternalFlags : byte
        {
            IsExternalModelUninitalized = 1 << 0,
            HasExternalString = 1 << 1,
            HoldsExternalStrings = 1 << 2,
            HasExternalGPU = 1 << 3,

            MeshCodecResave = 1 << 7,
        }
    }
}
