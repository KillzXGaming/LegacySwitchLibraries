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
    /// Represents an FSKA subfile in a <see cref="ResFile"/>, storing armature animations of <see cref="Bone"/>
    /// instances in a <see cref="Skeleton"/>.
    /// </summary>
    [DebuggerDisplay(nameof(SkeletalAnim) + " {" + nameof(Name) + "}")]
    public class SkeletalAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkeletalAnim"/> class.
        /// </summary>
        public SkeletalAnim()
        {
            Name = "";
            Path = "";
            FlagsAnimSettings = 0;
            FlagsScale = SkeletalAnimFlagsScale.Maya;
            FlagsRotate = SkeletalAnimFlagsRotate.EulerXYZ;
            FrameCount = 1;
            BakedSize = 0;
            BoneAnims = new List<BoneAnim>();
            BindSkeleton = new Skeleton();
            BindIndices = new ushort[0];
            UserDatas = new List<UserData>();
            UserDataDict = new ResDict();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSKA";

        private const uint _flagsMaskScale = 0b00000000_00000000_00000011_00000000;
        private const uint _flagsMaskRotate = 0b00000000_00000000_01110000_00000000;
        private const uint _flagsMaskAnimSettings = 0b00000000_00000000_00000000_00001111;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private uint _flags;

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
                loader.ImportSkeletonAnimations();
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
                loader.ImportSkeletonAnimations();
            }
        }

        /// <summary>
        /// Saves the contents in the given <paramref name="stream"/> and optionally leaves it open
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the contents into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        public void Export(Stream stream, ResFile ResFile, bool leaveOpen = false)
        {
            using (ResFileSaver saver = new ResFileSaver(this, ResFile, stream, leaveOpen))
            {
                saver.ExportSkeletonAnimation();
            }
        }

        /// <summary>
        /// Saves the contents in the file with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to save the contents into.</param>
        public void Export(string fileName, ResFile ResFile)
        {
            using (ResFileSaver saver = new ResFileSaver(this, ResFile, fileName))
            {
                saver.ExportSkeletonAnimation();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{SkeletalAnim}"/> instances.
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

        /// <summary>
        /// Gets or sets the <see cref="SkeletalAnimFlags"/> mode used to control looping and baked settings.
        /// </summary>
        [Browsable(false)]
        public SkeletalAnimFlags FlagsAnimSettings
        {
            get { return (SkeletalAnimFlags)(_flags & _flagsMaskAnimSettings); }
            set { _flags = _flags & ~_flagsMaskAnimSettings | (uint)value; }
        }

        [Browsable(true)]
        [Category("Animation")]
        public bool Loop
        {
            get
            {
                return FlagsAnimSettings.HasFlag(SkeletalAnimFlags.Looping);
            }
            set
            {
                if (value == true)
                    FlagsAnimSettings |= SkeletalAnimFlags.Looping;
                else
                    FlagsAnimSettings &= ~SkeletalAnimFlags.Looping;
            }
        }

        [Browsable(true)]
        [Category("Animation")]
        public bool Baked
        {
            get
            {
                return FlagsAnimSettings.HasFlag(SkeletalAnimFlags.BakedCurve);
            }
            set
            {
                if (value == true)
                    FlagsAnimSettings |= SkeletalAnimFlags.BakedCurve;
                else
                    FlagsAnimSettings &= ~SkeletalAnimFlags.BakedCurve;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SkeletalAnimFlagsScale"/> mode used to store scaling values.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        [DisplayName("Scaling")]
        public SkeletalAnimFlagsScale FlagsScale
        {
            get { return (SkeletalAnimFlagsScale)(_flags & _flagsMaskScale); }
            set { _flags = _flags & ~_flagsMaskScale | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SkeletalAnimFlagsRotate"/> mode used to store rotation values.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        [DisplayName("Rotation")]
        public SkeletalAnimFlagsRotate FlagsRotate
        {
            get { return (SkeletalAnimFlagsRotate)(_flags & _flagsMaskRotate); }
            set { _flags = _flags & ~_flagsMaskRotate | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="AnimCurve"/> instances of all
        /// <see cref="BoneAnims"/>.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Animation")]
        [DisplayName("Baked Size")]
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BoneAnim"/> instances creating the animation.
        /// </summary>
        [Browsable(false)]
        public IList<BoneAnim> BoneAnims { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Skeleton"/> instance affected by this animation.
        /// </summary>
        [Browsable(false)]
        public Skeleton BindSkeleton { get; set; }

        /// <summary>
        /// Gets or sets the indices of the <see cref="Bone"/> instances in the <see cref="Skeleton.Bones"/> dictionary
        /// to bind for each animation. <see cref="UInt16.MaxValue"/> specifies no binding.
        /// </summary>
        [Browsable(false)]
        public ushort[] BindIndices { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        [Browsable(false)]
        public ResDict UserDataDict { get; set; }

        [Browsable(false)]
        public IList<UserData> UserDatas { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        public class CopyFilter
        {
            public bool CopySettings = true; //Frame count, rotate/scale modes
            public bool CopyBoneAnims = true;
            public bool CopyUserData = true;
            public bool CopyBaseData = true;
        }

        public void Copy(SkeletalAnim target, CopyFilter filter)
        {
            if (filter.CopyBoneAnims)
                BakedSize = target.BakedSize;

            if (filter.CopySettings)
            {
                FlagsAnimSettings = target.FlagsAnimSettings;
                FlagsRotate = target.FlagsRotate;
                FlagsScale = target.FlagsScale;
                FrameCount = target.FrameCount;
            }

            if (filter.CopyUserData)
            {
                UserDatas.Clear();
                UserDataDict.Clear();

                foreach (var usd in target.UserDatas)
                    UserDatas.Add(usd.Copy());
                foreach (var usd in target.UserDatas)
                    UserDataDict.Add(usd.Name);
            }

            if (filter.CopyBoneAnims)
            {
                BindSkeleton = target.BindSkeleton;
                BindIndices = target.BindIndices;

                BoneAnims.Clear();
                for (int i = 0; i < target.BoneAnims?.Count; i++)
                {
                    List<AnimCurve> curves = new List<AnimCurve>();
                    for (int c = 0; c < target.BoneAnims[i].Curves?.Count; c++)
                        curves.Add(target.BoneAnims[i].Curves[c].Copy());

                    BoneAnims.Add(new BoneAnim()
                    {
                        Name = target.BoneAnims[i].Name,
                        Curves = curves,
                        FlagsBase = target.BoneAnims[i].FlagsBase,
                        FlagsCurve = target.BoneAnims[i].FlagsCurve,
                        FlagsTransform = target.BoneAnims[i].FlagsTransform,
                        BeginBaseTranslate = target.BoneAnims[i].BeginBaseTranslate,
                        BeginCurve = target.BoneAnims[i].BeginCurve,
                        BeginRotate = target.BoneAnims[i].BeginRotate,
                        BeginTranslate = target.BoneAnims[i].BeginTranslate,
                        UseRotation = target.BoneAnims[i].UseRotation,
                        UseScale = target.BoneAnims[i].UseScale,
                        UseTranslation = target.BoneAnims[i].UseTranslation,

                        BaseData = new BoneAnimData()
                        {
                            Translate = target.BoneAnims[i].BaseData.Translate,
                            Flags = target.BoneAnims[i].BaseData.Flags,
                            Padding = target.BoneAnims[i].BaseData.Padding,
                            Rotate = target.BoneAnims[i].BaseData.Rotate,
                            Scale = target.BoneAnims[i].BaseData.Scale,
                        }
                    });
                }
            }
        }

        public void ExportYaml()
        {

        }

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.ResFile.VersionMajor2 >= 9)
                _flags = loader.ReadUInt32();
            else
                loader.LoadHeaderBlock();

            Name = loader.LoadString();
            Path = loader.LoadString();
            BindSkeleton                  = loader.Load<Skeleton>();
            long BindIndexArray           = loader.ReadOffset();
            long BoneAnimArrayOffset      = loader.ReadOffset();
            long UserDataOffset           = loader.ReadOffset();
            UserDataDict                  = loader.LoadDict();
            if (loader.ResFile.VersionMajor2 < 9)
                _flags = loader.ReadUInt32();

            FrameCount                    = loader.ReadInt32();
            int numCurve                  = loader.ReadInt32();
            BakedSize                     = loader.ReadUInt32();
            ushort numBoneAnim            = loader.ReadUInt16();
            ushort numUserData            = loader.ReadUInt16();

            if (loader.ResFile.VersionMajor2 < 9)
                loader.ReadUInt32(); //Padding

            BoneAnims     = loader.LoadList<BoneAnim>(numBoneAnim, BoneAnimArrayOffset);
            BindIndices   = loader.LoadCustom(() => loader.ReadUInt16s(numBoneAnim), BindIndexArray);
            UserDatas     = loader.LoadList<UserData>(numUserData, UserDataOffset);
        }

        internal long BindSkeletonOffset = 0;
        internal long BindIndicesOffset = 0;
        internal long BoneAnimsOffset = 0;
        internal long UserDataOffset = 0;
        internal long UserDataDictOffset = 0;

        void IResData.Save(ResFileSaver saver)
        {
            if (BindIndices == null)
                BindIndices = new ushort[0];

            saver.WriteSignature(_signature);
            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write(_flags);
            else
                saver.SaveHeaderBlock();

            saver.SaveString(Name);
            saver.SaveString(Path);
            saver.Write(0L);
            BindIndicesOffset = saver.SaveOffset();
            BoneAnimsOffset = saver.SaveOffset();
            UserDataOffset = saver.SaveOffset();
            UserDataDictOffset = saver.SaveOffset();
            if (saver.ResFile.VersionMajor2 < 9)
                saver.Write(_flags);
            saver.Write(FrameCount);
            saver.Write(BoneAnims.Sum((x) => x.Curves.Count));
            saver.Write(BakedSize);
            saver.Write((ushort)BoneAnims.Count);
            saver.Write((ushort)UserDataDict.Count);

            if (saver.ResFile.VersionMajor2 < 9)
                saver.Write(0); //padding
        }
    }
    
    /// <summary>
    /// Represents flags specifying how animation data is stored or should be played.
    /// </summary>
    [Flags]
    public enum SkeletalAnimFlags : uint
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

    /// <summary>
    /// Represents the data format in which scaling values are stored.
    /// </summary>
    public enum SkeletalAnimFlagsScale : uint
    {
        /// <summary>
        /// No scaling.
        /// </summary>
        None,

        /// <summary>
        /// Default scaling.
        /// </summary>
        Standard = 1 << 8,

        /// <summary>
        /// Autodesk Maya scaling.
        /// </summary>
        Maya = 2 << 8,

        /// <summary>
        /// Autodesk Softimage scaling.
        /// </summary>
        Softimage = 3 << 8
    }

    /// <summary>
    /// Represents the data format in which rotation values are stored.
    /// </summary>
    public enum SkeletalAnimFlagsRotate : uint
    {
        /// <summary>
        /// Quaternion, 4 components.
        /// </summary>
        Quaternion,

        /// <summary>
        /// Euler XYZ, 3 components.
        /// </summary>
        EulerXYZ = 1 << 12
    }
}