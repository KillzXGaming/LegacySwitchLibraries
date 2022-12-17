using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FCAM section in a <see cref="SceneAnim"/> subfile, storing animations controlling camera settings.
    /// </summary>
    [DebuggerDisplay(nameof(CameraAnim) + " {" + nameof(Name) + "}")]
    public class CameraAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraAnim"/> class.
        /// </summary>
        public CameraAnim()
        {
            Name = "";
            Flags = CameraAnimFlags.EulerZXY;
            FrameCount = 1;
            BakedSize = 0;

            BaseData = new CameraAnimData();
            Curves = new List<AnimCurve>();
            UserData = new List<UserData>();
            UserDataDict = new ResDict();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FCAM";
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets flags controlling how animation data is stored or how the animation should be played.
        /// </summary>
        public CameraAnimFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        public int FrameCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="Curves"/>.
        /// </summary>
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{CameraAnim}"/> instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AnimCurve"/> instances animating properties of objects stored in this section.
        /// </summary>
        public IList<AnimCurve> Curves { get; set; }

        /// <summary>
        /// Gets the <see cref="CameraAnimData"/> instance storing initial camera parameters.
        /// </summary>
        public CameraAnimData BaseData { get; set; }

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
                Flags = loader.ReadEnum<CameraAnimFlags>(true);
                loader.Seek(2);
            }
            else
                loader.LoadHeaderBlock();
            Name = loader.LoadString();
            long CurveArrayOffset = loader.ReadInt64();
            BaseData = loader.LoadCustom(() => new CameraAnimData(loader));
            long UserDataOffset = loader.ReadInt64();
            UserDataDict = loader.LoadDict();

            ushort numUserData = 0;
            byte numCurve = 0;
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                FrameCount = loader.ReadInt32();
                BakedSize = loader.ReadUInt32();
                numUserData = loader.ReadUInt16();
                numCurve = loader.ReadByte();
                loader.Seek(1);
                loader.Seek(4);
            }
            else
            {
                Flags = loader.ReadEnum<CameraAnimFlags>(true);
                loader.Seek(2);
                FrameCount = loader.ReadInt32();
                numCurve = loader.ReadByte();
                loader.Seek(1);
                numUserData = loader.ReadUInt16();
                BakedSize = loader.ReadUInt32();
            }

            Curves = loader.LoadList<AnimCurve>(numCurve, CurveArrayOffset);
            UserData = loader.LoadList<UserData>(numUserData, UserDataOffset);
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

            saver.SaveRelocateEntryToSection(saver.Position, 5, 1, 0, ResFileSaver.Section1, "Camera Animation"); //      <------------ Entry Set
            saver.SaveString(Name);
            PosCurveArrayOffset = saver.SaveOffset();
            PosBaseDataOffset = saver.SaveOffset();
            PosUserDataOffset = saver.SaveOffset();
            PosUserDataDictOffset = saver.SaveOffset();
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(FrameCount);
                saver.Write(BakedSize);
                saver.Write((ushort)UserData.Count);
                saver.Write((byte)Curves.Count);
                saver.Seek(1);
                saver.Seek(4);
            }
            else
            {
                saver.Write(Flags, true);
                saver.Seek(2);
                saver.Write(FrameCount);
                saver.Write((byte)Curves.Count);
                saver.Seek(1);
                saver.Write((ushort)UserData.Count);
                saver.Write(BakedSize);
            }
        }
    }
    
    /// <summary>
    /// Represents flags specifying how animation data is stored or should be played.
    /// </summary>
    [Flags]
    public enum CameraAnimFlags : ushort
    {
        /// <summary>
        /// The stored curve data has been baked.
        /// </summary>
        BakedCurve = 1 << 0,

        /// <summary>
        /// The animation repeats from the start after the last frame has been played.
        /// </summary>
        Looping = 1 << 2,
        
        /// <summary>
        /// The rotation mode stores ZXY angles rather than look-at points in combination with a twist.
        /// </summary>
        EulerZXY = 1 << 8,

        /// <summary>
        /// The projection mode is perspective rather than ortographic.
        /// </summary>
        Perspective = 1 << 10
    }
}