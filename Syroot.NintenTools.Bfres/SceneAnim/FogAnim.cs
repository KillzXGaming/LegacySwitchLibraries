using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FCAM section in a <see cref="SceneAnim"/> subfile, storing animations controlling fog settings.
    /// </summary>
    [DebuggerDisplay(nameof(FogAnim) + " {" + nameof(Name) + "}")]
    public class FogAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FogAnim"/> class.
        /// </summary>
        public FogAnim()
        {
            Name = "";
            DistanceAttnFuncName = "";
            Flags = 0;
            FrameCount = 1;
            BakedSize = 0;
            DistanceAttnFuncIndex = 0;

            BaseData = new FogAnimData();
            Curves = new List<AnimCurve>();
            UserData = new List<UserData>();
            UserDataDict = new ResDict();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FFOG";
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets flags controlling how animation data is stored or how the animation should be played.
        /// </summary>
        public FogAnimFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        public int FrameCount { get; set; }

        /// <summary>
        /// Gets or sets the index of the distance attenuation function to use.
        /// </summary>
        public sbyte DistanceAttnFuncIndex { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="Curves"/>.
        /// </summary>
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{FogAnim}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the distance attenuation function to use.
        /// </summary>
        public string DistanceAttnFuncName { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AnimCurve"/> instances animating properties of objects stored in this section.
        /// </summary>
        public IList<AnimCurve> Curves { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FogAnimData"/> instance storing initial fog parameters.
        /// </summary>
        public FogAnimData BaseData { get; set; }

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
                Flags = loader.ReadEnum<FogAnimFlags>(true);
                loader.Seek(2);
            }
            else
                loader.LoadHeaderBlock();
            Name = loader.LoadString();
            long CurveArrayOffset = loader.ReadInt64();
            BaseData = loader.LoadCustom(() => new FogAnimData(loader));
            long UserDataOffset = loader.ReadInt64();
            UserDataDict = loader.LoadDict();
            DistanceAttnFuncName = loader.LoadString();

            ushort numUserData = 0;
            byte numCurve = 0;
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                FrameCount = loader.ReadInt32();
                numCurve = loader.ReadByte();
                DistanceAttnFuncIndex = loader.ReadSByte();
                numUserData = loader.ReadUInt16();
                BakedSize = loader.ReadUInt32();
                loader.Seek(4);
            }
            else
            {
                Flags = loader.ReadEnum<FogAnimFlags>(true);
                loader.Seek(2);
                FrameCount = loader.ReadInt32();
                numCurve = loader.ReadByte();
                DistanceAttnFuncIndex = loader.ReadSByte();
                numUserData = loader.ReadUInt16();
                BakedSize = loader.ReadUInt32();
            }

            UserData = loader.LoadList<UserData>(numUserData, UserDataOffset);
            Curves = loader.LoadList<AnimCurve>(numCurve);
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

            saver.SaveRelocateEntryToSection(saver.Position, 6, 1, 0, ResFileSaver.Section1, "Fog Animation"); //      <------------ Entry Set
            saver.SaveString(Name);
            PosCurveArrayOffset = saver.SaveOffset();
            PosBaseDataOffset = saver.SaveOffset();
            PosUserDataOffset = saver.SaveOffset();
            PosUserDataDictOffset = saver.SaveOffset();
            saver.SaveString(DistanceAttnFuncName);

            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(FrameCount);
                saver.Write((byte)Curves.Count);
                saver.Write(DistanceAttnFuncIndex);
                saver.Write((ushort)UserData.Count);
                saver.Write(BakedSize);
                saver.Seek(4);
            }
            else
            {
                saver.Write(Flags, true);
                saver.Seek(2);
                saver.Write(FrameCount);
                saver.Write((byte)Curves.Count);
                saver.Write(DistanceAttnFuncIndex);
                saver.Write((ushort)UserData.Count);
                saver.Write(BakedSize);
            }  
        }
    }
    
    /// <summary>
    /// Represents flags specifying how animation data is stored or should be played.
    /// </summary>
    [Flags]
    public enum FogAnimFlags : ushort
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