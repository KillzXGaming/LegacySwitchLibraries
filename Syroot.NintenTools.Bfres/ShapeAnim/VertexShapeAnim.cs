using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;
using System;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents a vertex shape animation in a <see cref="ShapeAnim"/> subfile.
    /// </summary>
    [DebuggerDisplay(nameof(VertexShapeAnim) + " {" + nameof(Name) + "}")]
    public class VertexShapeAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VertexShapeAnim"/> class.
        /// </summary>
        public VertexShapeAnim()
        {
            Name = "";
            KeyShapeAnimInfos = new List<KeyShapeAnimInfo>();
            Curves = new List<AnimCurve>();
            BaseDataList = new float[0];
            BeginCurve = 0;
            BeginKeyShapeAnim = 0;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name of the animated <see cref="Shape"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="KeyShapeAnimInfo"/> instances.
        /// </summary>
        public IList<KeyShapeAnimInfo> KeyShapeAnimInfos { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AnimCurve"/> instances animating properties of objects stored in this section.
        /// </summary>
        public IList<AnimCurve> Curves { get; set; }

        /// <summary>
        /// Gets or sets the list of base values, excluding the base shape (which is always being initialized with 0f).
        /// </summary>
        public float[] BaseDataList { get; set; }

        /// <summary>
        /// Gets or sets the index of the first <see cref="AnimCurve"/> relative to all curves of the parent
        /// <see cref="ShapeAnim.VertexShapeAnims"/> instances.
        /// </summary>
        internal int BeginCurve { get; set; }

        /// <summary>
        /// Gets or sets the index of the first <see cref="KeyShapeAnimInfo"/> relative to all key shape anim infos of
        /// the parent <see cref="ShapeAnim.VertexShapeAnims"/> instances.
        /// </summary>
        internal int BeginKeyShapeAnim { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Name                           = loader.LoadString();
            long CurveOffset               = loader.ReadOffset(true);
            long BaseDataOffset            = loader.ReadOffset(true);
            long KeyShapeAnimInfosOffset   = loader.ReadOffset(true);
            ushort numCurve                = loader.ReadUInt16();
            ushort numKeyShapeAnim         = loader.ReadUInt16();
            BeginCurve                     = loader.ReadInt32();
            BeginKeyShapeAnim              = loader.ReadInt32();
            loader.Seek(4); //Padding

            KeyShapeAnimInfos              = loader.LoadList<KeyShapeAnimInfo>(numKeyShapeAnim, KeyShapeAnimInfosOffset);
            Curves                         = loader.LoadList<AnimCurve>(numCurve, CurveOffset);
            BaseDataList                   = loader.LoadCustom(() => loader.ReadSingles(numKeyShapeAnim - 1), BaseDataOffset); // Without base shape.
        }

        internal long PosBaseDataOffset;
        internal long PosCurvesOffset;
        internal long PosKeyShapeAnimInfosOffset;

        void IResData.Save(ResFileSaver saver)
        {
            saver.SaveString(Name);
            PosCurvesOffset = saver.SaveOffset();
            PosBaseDataOffset = saver.SaveOffset();
            PosKeyShapeAnimInfosOffset = saver.SaveOffset();
            saver.Write((ushort)Curves.Count);
            saver.Write((ushort)KeyShapeAnimInfos.Count);
            saver.Write(BeginCurve);
            saver.Write(BeginKeyShapeAnim);
            saver.Seek(4); //Padding
        }
    }
}