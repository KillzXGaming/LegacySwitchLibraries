using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FSHA subfile in a <see cref="ResFile"/>, storing shape animations of a <see cref="Model"/>
    /// instance.
    /// </summary>
    [DebuggerDisplay(nameof(ShapeAnim) + " {" + nameof(Name) + "}")]
    public class ShapeAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeAnim"/> class.
        /// </summary>
        public ShapeAnim()
        {
            Name = "";
            Path = "";
            Flags = 0;
            BindModel = new Model();
            BindIndices = new ushort[0];
            VertexShapeAnims = new List<VertexShapeAnim>();
            FrameCount = 1;
            BakedSize = 0;
            BindIndices = new ushort[0];
            UserData = new List<UserData>();
            UserDataDict = new ResDict();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSHA";

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
                loader.ImportShapeAnimations();
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
                loader.ImportShapeAnimations();
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
                saver.ExportShapeAnimation();
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
                saver.ExportShapeAnimation();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{ShapeAnim}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets flags controlling how animation data is stored or how the animation should be played.
        /// </summary>
        public ShapeAnimFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        public int FrameCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="AnimCurve"/> instances of all
        /// <see cref="VertexShapeAnims"/>.
        /// </summary>
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Model"/> instance affected by this animation.
        /// </summary>
        public Model BindModel { get; set; }

        /// <summary>
        /// Gets or sets the indices of the <see cref="Shape"/> instances in the <see cref="Model.Shapes"/> dictionary
        /// to bind for each animation. <see cref="UInt16.MaxValue"/> specifies no binding.
        /// </summary>
        public ushort[] BindIndices { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="VertexShapeAnim"/> instances creating the animation.
        /// </summary>
        public IList<VertexShapeAnim> VertexShapeAnims { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public ResDict UserDataDict { get; set; }

        public IList<UserData> UserData { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.ResFile.VersionMajor2 == 9)
            {
                Flags = loader.ReadEnum<ShapeAnimFlags>(false);
                loader.ReadUInt16();
            }
            else
                loader.LoadHeaderBlock();

            Name = loader.LoadString();
            Path = loader.LoadString();
            BindModel = loader.Load<Model>();
            long BindIndicesOffset = loader.ReadOffset();
            long VertexShapeAnimsArrayOffset = loader.ReadOffset();
            long UserDataOffset = loader.ReadOffset();
            UserDataDict = loader.LoadDict();

            ushort numUserData = 0;
            ushort numVertexShapeAnim = 0;
            ushort CurveCount = 0;

            if (loader.ResFile.VersionMajor2 == 9)
            {
                BakedSize = loader.ReadUInt32();
                FrameCount = loader.ReadInt32();
                numUserData = loader.ReadUInt16();
                numVertexShapeAnim = loader.ReadUInt16();
                ushort numKeyShapeAnim = loader.ReadUInt16();
                CurveCount = loader.ReadUInt16();
            }
            else
            {
                Flags = loader.ReadEnum<ShapeAnimFlags>(false);
                numUserData = loader.ReadUInt16();
                numVertexShapeAnim = loader.ReadUInt16();
                ushort numKeyShapeAnim = loader.ReadUInt16();
                FrameCount = loader.ReadInt32();
                BakedSize = loader.ReadUInt32();
                ushort numCurve = loader.ReadUInt16();
            }

            BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(numVertexShapeAnim), BindIndicesOffset);
            VertexShapeAnims = loader.LoadList<VertexShapeAnim>(numVertexShapeAnim, VertexShapeAnimsArrayOffset);
            UserData = loader.LoadList<UserData>(numUserData, UserDataOffset);
        }

        internal long PosBindModelOffset;
        internal long PosBindIndicesOffset;
        internal long PosVertexShapeAnimsOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;

        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            if (saver.ResFile.VersionMajor2 == 9)
            {
                saver.Write(Flags, false);
                saver.Write((ushort)0);
            }
            else
                saver.SaveHeaderBlock();

            saver.SaveString(Name);
            saver.SaveString(Path);
            PosBindModelOffset = saver.SaveOffset();
            PosBindIndicesOffset = saver.SaveOffset();
            PosVertexShapeAnimsOffset = saver.SaveOffset();
            PosUserDataOffset = saver.SaveOffset();
            PosUserDataDictOffset = saver.SaveOffset();

            if (saver.ResFile.VersionMajor2 == 9)
            {
                saver.Write(BakedSize);
                saver.Write(FrameCount);
                saver.Write((ushort)UserData.Count);
                saver.Write((ushort)VertexShapeAnims.Count);
                saver.Write((ushort)VertexShapeAnims.Sum((x) => x.KeyShapeAnimInfos.Count));
                saver.Write((ushort)VertexShapeAnims.Sum((x) => x.Curves.Count));
            }
            else
            {
                saver.Write(Flags, false);
                saver.Write((ushort)UserData.Count);
                saver.Write((ushort)VertexShapeAnims.Count);
                saver.Write((ushort)VertexShapeAnims.Sum((x) => x.KeyShapeAnimInfos.Count));
                saver.Write(FrameCount);
                saver.Write(BakedSize);
                saver.Write((ushort)VertexShapeAnims.Sum((x) => x.Curves.Count));
            }
        }
    }

    /// <summary>
    /// Represents flags specifying how animation data is stored or should be played.
    /// </summary>
    [Flags]
    public enum ShapeAnimFlags : ushort
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