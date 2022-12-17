using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FLIT section in a <see cref="SceneAnim"/> subfile, storing animations controlling light settings.
    /// </summary>
    [DebuggerDisplay(nameof(LightAnim) + " {" + nameof(Name) + "}")]
    public class LightAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightAnim"/> class.
        /// </summary>
        public LightAnim()
        {
            Name = "";
            DistanceAttnFuncName = "";
            AngleAttnFuncName = "";
            Flags = 0;
            LightTypeIndex = 0;
            AnimatedFields = 0;
            FrameCount = 1;
            BakedSize = 0;
            DistanceAttnFuncIndex = 0;
            AngleAttnFuncIndex = 0;

            BaseData = new LightAnimData();
            Curves = new List<AnimCurve>();
            UserData = new List<UserData>();
            UserDataDict = new ResDict();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FLIT";

        private const ushort _flagsMask = 0b00000001_00000101;
        private const ushort _flagsMaskFields = 0b11111110_00000000;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private ushort _flags;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets flags controlling how the animation should be played.
        /// </summary>
        public LightAnimFlags Flags
        {
            get { return (LightAnimFlags)(_flags & _flagsMask); }
            set { _flags = (ushort)(_flags & ~_flagsMask | (ushort)value); }
        }

        /// <summary>
        /// Gets or sets flags controlling how animation data is stored or how the animation should be played.
        /// </summary>
        public LightAnimField AnimatedFields
        {
            get { return (LightAnimField)(_flags & _flagsMaskFields); }
            set { _flags = (ushort)(_flags & ~_flagsMaskFields | (ushort)value); }
        }

        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        public int FrameCount { get; set; }

        /// <summary>
        /// Gets or sets the index of the light type.
        /// </summary>
        public sbyte LightTypeIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the distance attenuation function to use.
        /// </summary>
        public sbyte DistanceAttnFuncIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the angle attenuation function to use.
        /// </summary>
        public sbyte AngleAttnFuncIndex { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="Curves"/>.
        /// </summary>
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{LightAnim}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the light type.
        /// </summary>
        public string LightTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the distance attenuation function to use.
        /// </summary>
        public string DistanceAttnFuncName { get; set; }

        /// <summary>
        /// Gets or sets the name of the angle attenuation function to use.
        /// </summary>
        public string AngleAttnFuncName { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AnimCurve"/> instances animating properties of objects stored in this section.
        /// </summary>
        public IList<AnimCurve> Curves { get; set; }
        
        /// <summary>
        /// Gets the <see cref="LightAnimData"/> instance storing initial light parameters.
        /// </summary>
        public LightAnimData BaseData { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public IList<UserData> UserData { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public ResDict UserDataDict { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                Flags = loader.ReadEnum<LightAnimFlags>(true);
                loader.Seek(2);
            }
            else
                loader.LoadHeaderBlock();
            Name = loader.LoadString();
            long CurveArrayOffset = loader.ReadInt64();
            BaseData = loader.LoadCustom(() => new LightAnimData(loader, AnimatedFields));
            long UserDataOffset = loader.ReadInt64();
            UserDataDict = loader.LoadDict();
            LightTypeName = loader.LoadString();
            DistanceAttnFuncName = loader.LoadString();
            AngleAttnFuncName = loader.LoadString();

            ushort numUserData = 0;
            byte numCurve = 0;
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                FrameCount = loader.ReadInt32();
                numCurve = loader.ReadByte();
                LightTypeIndex = loader.ReadSByte();
                DistanceAttnFuncIndex = loader.ReadSByte();
                AngleAttnFuncIndex = loader.ReadSByte();
                BakedSize = loader.ReadUInt32();
                numUserData = loader.ReadUInt16();
                loader.Seek(2);
            }
            else
            {
                Flags = loader.ReadEnum<LightAnimFlags>(true);
                numUserData = loader.ReadUInt16();
                FrameCount = loader.ReadInt32();
                numCurve = loader.ReadByte();
                LightTypeIndex = loader.ReadSByte();
                DistanceAttnFuncIndex = loader.ReadSByte();
                AngleAttnFuncIndex = loader.ReadSByte();
                BakedSize = loader.ReadUInt32();
            }

            UserData = loader.LoadList<UserData>(numUserData, UserDataOffset);
            Curves = loader.LoadList<AnimCurve>(numCurve, CurveArrayOffset);
        }

        internal long PosCurveArrayOffset;
        internal long PosBaseDataOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;

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

            saver.SaveRelocateEntryToSection(saver.Position, 8, 1, 0, ResFileSaver.Section1, "Light Animation"); //      <------------ Entry Set
            saver.SaveString(Name);
            PosCurveArrayOffset = saver.SaveOffset();
            PosBaseDataOffset = saver.SaveOffset();
            PosUserDataOffset = saver.SaveOffset();
            PosUserDataDictOffset = saver.SaveOffset();
            saver.SaveString(LightTypeName);
            saver.SaveString(DistanceAttnFuncName);
            saver.SaveString(AngleAttnFuncName);
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(FrameCount);
                saver.Write((byte)Curves.Count);
                saver.Write(LightTypeIndex);
                saver.Write(DistanceAttnFuncIndex);
                saver.Write(AngleAttnFuncIndex);
                saver.Write(BakedSize);
                saver.Write((ushort)UserData.Count);
                saver.Seek(2);
            }
            else
            {
                saver.Write(Flags, true);
                saver.Write((ushort)UserData.Count);
                saver.Write(FrameCount);
                saver.Write((byte)Curves.Count);
                saver.Write(LightTypeIndex);
                saver.Write(DistanceAttnFuncIndex);
                saver.Write(AngleAttnFuncIndex);
                saver.Write(BakedSize);
            }
        }
    }
    
    /// <summary>
    /// Represents flags specifying how animation data is stored.
    /// </summary>
    [Flags]
    public enum LightAnimFlags : ushort
    {
        /// <summary>
        /// The stored curve data has been baked.
        /// </summary>
        BakedCurve = 1 << 0,

        /// <summary>
        /// The animation repeats from the start after the last frame has been played.
        /// </summary>
        Looping = 1 << 2,

        EnableCurve = 1 << 8,

        BaseEnable= 1 << 9,

        BasePos = 1 << 10,

        BaseDir = 1 << 11,

        BaseDistAttn = 1 << 12,

        BaseAngleAttn = 1 << 13,

        BaseColor0 = 1 << 14,

        BaseColor1 = 1 << 15,
    }

    /// <summary>
    /// Represents flags specifying which fields are animated.
    /// </summary>
    [Flags]
    public enum LightAnimField : ushort
    {
        /// <summary>
        /// Enabled state is animated.
        /// </summary>
        Enable = 1 << 9,

        /// <summary>
        /// Position is animated.
        /// </summary>
        Position = 1 << 10,

        /// <summary>
        /// Rotation is animated.
        /// </summary>
        Rotation = 1 << 11,

        /// <summary>
        /// Distance attenuation is animated.
        /// </summary>
        DistanceAttn = 1 << 12,

        /// <summary>
        /// Angle attenuation is animated in degrees.
        /// </summary>
        AngleAttn = 1 << 13,

        /// <summary>
        /// Color 0 is animated.
        /// </summary>
        Color0 = 1 << 14,

        /// <summary>
        /// Color 1 is animated.
        /// </summary>
        Color1 = 1 << 15
    }
}