using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;
using System.Runtime.Remoting.Messaging;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FVIS subfile in a <see cref="ResFile"/>, storing visibility animations of <see cref="Bone"/> or
    /// <see cref="Material"/> instances.
    /// </summary>
    [DebuggerDisplay(nameof(VisibilityAnim) + " {" + nameof(Name) + "}")]
    public class VisibilityAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityAnim"/> class.
        /// </summary>
        public VisibilityAnim()
        {
            Name = "";
            Path = "";
            Flags = 0;
            FrameCount = 1;
            BakedSize = 0;
            Curves = new List<AnimCurve>();
            BindIndices = new ushort[0];
            Names = new List<string>();
            BaseDataList = new bool[0];
            BaseSDataList = new byte[0];
            UserData = new List<UserData>();
            UserDataDict = new ResDict();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FBVS";

        private const ushort _flagsMask = 0b00000000_00000111;
        private const ushort _flagsMaskType = 0b00000001_00000000;

        // ---- FIELDS -------------------------------------------------------------------------------------------------
        
        internal ushort _flags;


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
                loader.ImportBoneVisualAnimations();
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
                loader.ImportBoneVisualAnimations();
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
                saver.ExportBoneVisualAnimation();
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
                saver.ExportBoneVisualAnimation();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{VisibilityAnim}"/> instances.
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
        /// Gets or sets flags controlling how animation data is stored or how the animation should be played.
        /// </summary>
        [Browsable(false)]
        public VisibilityAnimFlags Flags
        {
            get { return (VisibilityAnimFlags)(_flags & _flagsMask); }
            set { _flags = (ushort)(_flags & ~_flagsMask | (ushort)value); }
        }

        [Browsable(true)]
        [Category("Animation")]
        public bool Loop
        {
            get
            {
                return Flags.HasFlag(VisibilityAnimFlags.Looping);
            }
            set
            {
                if (value == true)
                    Flags |= VisibilityAnimFlags.Looping;
                else
                    Flags &= ~VisibilityAnimFlags.Looping;
            }
        }

        [Browsable(true)]
        [Category("Animation")]
        public bool Baked
        {
            get
            {
                return Flags.HasFlag(VisibilityAnimFlags.BakedCurve);
            }
            set
            {
                if (value == true)
                    Flags |= VisibilityAnimFlags.BakedCurve;
                else
                    Flags &= ~VisibilityAnimFlags.BakedCurve;
            }
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
        /// Gets or sets the <see cref="Model"/> instance affected by this animation.
        /// </summary>
        [Browsable(false)]
        public Model BindModel { get; set; }

        /// <summary>
        /// Gets or sets the indices of entries in the <see cref="Skeleton.Bones"/> or <see cref="Model.Materials"/>
        /// dictionaries to bind to for each animation. <see cref="UInt16.MaxValue"/> specifies no binding.
        /// </summary>
        [Browsable(false)]
        public ushort[] BindIndices { get; set; }

        /// <summary>
        /// Gets or sets the names of entries in the <see cref="Skeleton.Bones"/> or <see cref="Model.Materials"/>
        /// dictionaries to bind to for each animation.
        /// </summary>
        [Browsable(false)]
        public IList<string> Names { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AnimCurve"/> instances animating properties of objects stored in this section.
        /// </summary>
        [Browsable(false)]
        public IList<AnimCurve> Curves { get; set; }

        /// <summary>
        /// Gets or sets boolean values storing the initial visibility for each <see cref="Bone"/> or
        /// <see cref="Material"/>.
        /// </summary>
        [Browsable(false)]
        public bool[] BaseDataList { get; set; }

        [Browsable(false)]
        public byte[] BaseSDataList { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        [Browsable(false)]
        public ResDict UserDataDict { get; set; }

        [Browsable(false)]
        public IList<UserData> UserData { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        private ushort Unknown;

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                _flags = loader.ReadUInt16();
                loader.ReadUInt16(); //Padding
            }
            else
                loader.LoadHeaderBlock();
            Name = loader.LoadString();
            Path = loader.LoadString();
            BindModel = loader.Load<Model>();
            long BindIndicesOffset = loader.ReadOffset();
            long CurveArrayOffset  = loader.ReadOffset();
            long BaseDataArrayOffset = loader.ReadOffset();
            long NameArrayOffset = loader.ReadOffset();
            long UserDataOffset = loader.ReadOffset();
            UserDataDict = loader.LoadDict();

            ushort numUserData = 0;
            ushort numAnim = 0;
            ushort numCurve = 0;

            if (loader.ResFile.VersionMajor2 >= 9)
            {
                FrameCount = loader.ReadInt32();
                BakedSize = loader.ReadUInt32();
                numAnim = loader.ReadUInt16();
                numCurve = loader.ReadUInt16();
                numUserData = loader.ReadUInt16();
                loader.ReadUInt16(); //padding
            }
            else
            {
                _flags = loader.ReadUInt16();
                numUserData = loader.ReadUInt16();
                FrameCount = loader.ReadInt32();
                numAnim = loader.ReadUInt16();
                numCurve = loader.ReadUInt16();
                BakedSize = loader.ReadUInt32();
            }
      
            BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(numAnim), BindIndicesOffset);
            Names = loader.LoadCustom(() => loader.LoadStrings(numAnim), NameArrayOffset); // Offset to name list.
            Curves = loader.LoadList<AnimCurve>(numCurve, CurveArrayOffset);
            UserData = loader.LoadList<UserData>(numUserData, UserDataOffset);

            BaseDataList = new bool[numAnim];
            BaseSDataList = loader.LoadCustom(() =>
            {
                List<byte> values = new List<byte>();
                int i = 0;
                while (i < numAnim)
                {
                    byte b = loader.ReadByte();
                    values.Add(b);

                    for (int j = 0; j < 8 && i < numAnim; j++)
                    {
                        BaseDataList[i] = b.GetBit(j);
                        i++;
                    }
                }
                return values.ToArray();
            }, BaseDataArrayOffset);
        }
        
        internal void WriteBaseData(ResFileSaver saver)
        {
            Console.WriteLine(Name + " " + BaseSDataList.Length);
            saver.Write(BaseSDataList);

      /*      int i = 0;
            while (i < BaseDataList.Length)
            {
                byte b = 0;
                for (int j = 0; j < 8 && i < BaseDataList.Length; j++)
                {
                    b.SetBit(j, BaseDataList[i++]);
                }
                saver.Write(b);
            }*/
        }

        internal long PosBindModelOffset;
        internal long PosBindIndicesOffset;
        internal long PosCurvesOffset;
        internal long PosBaseDataOffset;
        internal long PosNamesOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;

        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(_flags);
                saver.Write((ushort)0);
            }
            else
                saver.SaveHeaderBlock();

            saver.SaveString(Name);
            saver.SaveString(Path);
            saver.Write(0L); //Bind Model
            PosBindIndicesOffset = saver.SaveOffset();
            PosCurvesOffset = saver.SaveOffset();
            PosBaseDataOffset = saver.SaveOffset();
            PosNamesOffset = saver.SaveOffset();
            PosUserDataOffset = saver.SaveOffset();
            PosUserDataDictOffset = saver.SaveOffset();

            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(FrameCount);
                saver.Write(BakedSize);
                if (Names != null)
                    saver.Write((ushort)Names.Count);
                else
                    saver.Write((ushort)0);
                saver.Write((ushort)Curves.Count);
                saver.Write((ushort)UserDataDict.Count);
                saver.Write((ushort)0);//padding
            }
            else
            {
                saver.Write(_flags);
                saver.Write((ushort)UserDataDict.Count);
                saver.Write(FrameCount);
                if (Names != null)
                    saver.Write((ushort)Names.Count);
                else
                    saver.Write((ushort)0);
                saver.Write((ushort)Curves.Count);
                saver.Write(BakedSize);
            }
        }
    }
    
    /// <summary>
    /// Represents flags specifying how animation data is stored or should be played.
    /// </summary>
    [Flags]
    public enum VisibilityAnimFlags : ushort
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