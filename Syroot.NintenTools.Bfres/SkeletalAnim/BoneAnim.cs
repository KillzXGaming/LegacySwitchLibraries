﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;
using System.ComponentModel;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents the animation of a single <see cref="Bone"/> in a <see cref="SkeletalAnim"/> subfile.
    /// </summary>
    [DebuggerDisplay(nameof(BoneAnim) + " {" + nameof(Name) + "}")]
    public class BoneAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoneAnim"/> class.
        /// </summary>
        public BoneAnim()
        {
            Name = "";
            _flags = 0;
            FlagsBase = 0;
            BeginRotate = 0;
            BeginTranslate = 0;
            BeginBaseTranslate = 0;

            Curves = new List<AnimCurve>();
            BaseData = new BoneAnimData();
            BeginCurve = 0;
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const uint _flagsMaskBase = 0b00000000_00000000_00000000_00111000;
        private const uint _flagsMaskCurve = 0b00000000_00000000_11111111_11000000;
        private const uint _flagsMaskTransform = 0b00001111_10000000_00000000_00000000;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private uint _flags;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a set of flags indicating whether initial transformation values exist in
        /// <see cref="BaseData"/>.
        /// </summary>
        [Browsable(false)]
        public BoneAnimFlagsBase FlagsBase
        {
            get { return (BoneAnimFlagsBase)(_flags & _flagsMaskBase); }
            set { _flags = _flags & ~_flagsMaskBase | (uint)value; }
        }

        /// <summary>
        /// Gets or sets a set of flags indicating whether curves animating the corresponding transformation exist.
        /// </summary>
        [Browsable(false)]
        public BoneAnimFlagsCurve FlagsCurve
        {
            get { return (BoneAnimFlagsCurve)(_flags & _flagsMaskCurve); }
            set { _flags = _flags & ~_flagsMaskCurve | (uint)value; }
        }

        /// <summary>
        /// Gets or sets a set of flags controlling how to transform bones.
        /// </summary>
        [Browsable(false)]
        public BoneAnimFlagsTransform FlagsTransform
        {
            get { return (BoneAnimFlagsTransform)(_flags & _flagsMaskTransform); }
            set { _flags = _flags & ~_flagsMaskTransform | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the name of the animated <see cref="Bone"/>.
        /// </summary>
        [Browsable(true)]
        [Category("Bone")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a field with unknown purpose.
        /// </summary>
        [Browsable(true)]
        [Category("Bone")]
        [DisplayName("Begin Rotate")]
        public byte BeginRotate { get; set; }

        /// <summary>
        /// Gets or sets a field with unknown purpose.
        /// </summary>
        [Browsable(true)]
        [Category("Bone")]
        [DisplayName("Begin Translate")]
        public byte BeginTranslate { get; set; }

        /// <summary>
        /// Gets or sets the element offset in the <see cref="BaseData"/> to an initial translation.
        /// </summary>
        [Browsable(true)]
        [Category("Bone")]
        [DisplayName("Begin Base Translate")]
        public byte BeginBaseTranslate { get; set; }

        /// <summary>
        /// Gets the index of the first <see cref="AnimCurve"/> relative to all curves of the parent
        /// <see cref="SkeletalAnim.BoneAnims"/> instances.
        /// </summary>
        [Browsable(true)]
        [Category("Bone")]
        [DisplayName("Begin Curve")]
        internal int BeginCurve { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AnimCurve"/> instances animating properties of objects stored in this section.
        /// </summary>
        [Browsable(false)]
        public IList<AnimCurve> Curves { get; set; }

        /// <summary>
        /// Gets or sets initial transformation values. Only stores specific transformations according to
        /// <see cref="FlagsBase"/>.
        /// </summary>
        [Browsable(false)]
        public BoneAnimData BaseData { get; set; }


        [Browsable(true)]
        [Category("Base Data")]
        [DisplayName("Use Translation")]
        public bool UseTranslation
        {
            get { return FlagsBase.HasFlag(BoneAnimFlagsBase.Translate); }
            set
            {
                if (value == true)
                    FlagsBase |= BoneAnimFlagsBase.Translate;
                else
                    FlagsBase &= ~BoneAnimFlagsBase.Translate;
            }
        }

        [Browsable(true)]
        [Category("Base Data")]
        [DisplayName("Use Scale")]
        public bool UseScale
        {
            get { return FlagsBase.HasFlag(BoneAnimFlagsBase.Scale); }
            set
            {
                if (value == true)
                    FlagsBase |= BoneAnimFlagsBase.Scale;
                else
                    FlagsBase &= ~BoneAnimFlagsBase.Scale;
            }
        }

        [Browsable(true)]
        [Category("Base Data")]
        [DisplayName("Use Rotation")]
        public bool UseRotation
        {
            get { return FlagsBase.HasFlag(BoneAnimFlagsBase.Rotate); }
            set
            {
                if (value == true)
                    FlagsBase |= BoneAnimFlagsBase.Rotate;
                else
                    FlagsBase &= ~BoneAnimFlagsBase.Rotate;
            }
        }

        [Browsable(true)]
        [Category("Transform Modes")]
        [DisplayName("Identity")]
        public bool ApplyIdentity
        {
            get { return FlagsTransform.HasFlag(BoneAnimFlagsTransform.Identity); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneAnimFlagsTransform.Identity;
                else
                    FlagsTransform &= ~BoneAnimFlagsTransform.Identity;
            }
        }

        [Browsable(true)]
        [Category("Transform Modes")]
        [DisplayName("Rotate Translate Zero")]
        public bool ApplyRotateTranslateZero
        {
            get { return FlagsTransform.HasFlag(BoneAnimFlagsTransform.RotateTranslateZero); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneAnimFlagsTransform.RotateTranslateZero;
                else
                    FlagsTransform &= ~BoneAnimFlagsTransform.RotateTranslateZero;
            }
        }

        [Browsable(true)]
        [Category("Transform Modes")]
        [DisplayName("Rotate Zero")]
        public bool ApplyRotateZero
        {
            get { return FlagsTransform.HasFlag(BoneAnimFlagsTransform.RotateZero); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneAnimFlagsTransform.RotateZero;
                else
                    FlagsTransform &= ~BoneAnimFlagsTransform.RotateZero;
            }
        }

        [Browsable(true)]
        [Category("Transform Modes")]
        [DisplayName("Scale One")]
        public bool ApplyScaleOne
        {
            get { return FlagsTransform.HasFlag(BoneAnimFlagsTransform.ScaleOne); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneAnimFlagsTransform.ScaleOne;
                else
                    FlagsTransform &= ~BoneAnimFlagsTransform.ScaleOne;
            }
        }

        [Browsable(true)]
        [Category("Transform Modes")]
        [DisplayName("Scale Volume One")]
        public bool ApplyScaleVolumeOne
        {
            get { return FlagsTransform.HasFlag(BoneAnimFlagsTransform.ScaleVolumeOne); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneAnimFlagsTransform.ScaleVolumeOne;
                else
                    FlagsTransform &= ~BoneAnimFlagsTransform.ScaleVolumeOne;
            }
        }

        [Browsable(true)]
        [Category("Transform Modes")]
        [DisplayName("Scale Uniform")]
        public bool ApplyScaleUniform
        {
            get { return FlagsTransform.HasFlag(BoneAnimFlagsTransform.ScaleUniform); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneAnimFlagsTransform.ScaleUniform;
                else
                    FlagsTransform &= ~BoneAnimFlagsTransform.ScaleUniform;
            }
        }

        [Browsable(true)]
        [Category("Transform Modes")]
        [DisplayName("Segment Scale Compensate")]
        public bool ApplySegmentScaleCompensate
        {
            get { return FlagsTransform.HasFlag(BoneAnimFlagsTransform.SegmentScaleCompensate); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneAnimFlagsTransform.SegmentScaleCompensate;
                else
                    FlagsTransform &= ~BoneAnimFlagsTransform.SegmentScaleCompensate;
            }
        }

        [Browsable(true)]
        [Category("Transform Modes")]
        [DisplayName("Translate Zero")]
        public bool ApplyTranslateZero
        {
            get { return FlagsTransform.HasFlag(BoneAnimFlagsTransform.TranslateZero); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneAnimFlagsTransform.TranslateZero;
                else
                    FlagsTransform &= ~BoneAnimFlagsTransform.TranslateZero;
            }
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Name = loader.LoadString();
            long CurveOffset = loader.ReadOffset(true);
            long BaseDataOffset = loader.ReadOffset(true);
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                long unk1 = loader.ReadInt64();
                long unk2 = loader.ReadInt64();
                if (unk1 != 0 || unk2 != 0)
                    throw new Exception();
            }
            _flags = loader.ReadUInt32();
            BeginRotate = loader.ReadByte();
            BeginTranslate = loader.ReadByte();
            byte numCurve = loader.ReadByte();
            BeginBaseTranslate = loader.ReadByte();
            BeginCurve = loader.ReadInt32();
            int padding = loader.ReadInt32();

            BaseData = loader.LoadCustom(() => new BoneAnimData(loader, FlagsBase), BaseDataOffset);
            Curves = loader.LoadList<AnimCurve>(numCurve, CurveOffset);
        }

        internal long PosCurvesOffset;
        internal long PosBaseDataOffset;
        void IResData.Save(ResFileSaver saver)
        {
            saver.SaveString(Name);
            PosCurvesOffset = saver.SaveOffset();
            PosBaseDataOffset = saver.SaveOffset();
            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Seek(16);

            saver.Write(_flags);
            saver.Write(BeginRotate);
            saver.Write(BeginTranslate);
            saver.Write((byte)Curves.Count);
            saver.Write(BeginBaseTranslate);
            saver.Write(BeginCurve);
            saver.Write(0); //padding
        }
    }

    /// <summary>
    /// Represents if initial values exist for the corresponding transformation in the base animation data.
    /// </summary>
    [Flags]
    public enum BoneAnimFlagsBase : uint
    {
        /// <summary>
        /// Initial scaling values exist.
        /// </summary>
        Scale = 1 << 3,

        /// <summary>
        /// Initial rotation values exist.
        /// </summary>
        Rotate = 1 << 4,

        /// <summary>
        /// Initial translation values exist.
        /// </summary>
        Translate = 1 << 5
    }

    /// <summary>
    /// Represents if curves exist which animate the corresponding transformation component.
    /// </summary>
    [Flags]
    public enum BoneAnimFlagsCurve : uint
    {
        /// <summary>
        /// Curve animating the X component of a bone's scale.
        /// </summary>
        ScaleX = 1 << 6,

        /// <summary>
        /// Curve animating the Y component of a bone's scale.
        /// </summary>
        ScaleY = 1 << 7,

        /// <summary>
        /// Curve animating the Z component of a bone's scale.
        /// </summary>
        ScaleZ = 1 << 8,

        /// <summary>
        /// Curve animating the X component of a bone's rotation.
        /// </summary>
        RotateX = 1 << 9,

        /// <summary>
        /// Curve animating the Y component of a bone's rotation.
        /// </summary>
        RotateY = 1 << 10,

        /// <summary>
        /// Curve animating the Z component of a bone's rotation.
        /// </summary>
        RotateZ = 1 << 11,

        /// <summary>
        /// Curve animating the W component of a bone's rotation.
        /// </summary>
        RotateW = 1 << 12,

        /// <summary>
        /// Curve animating the X component of a bone's translation.
        /// </summary>
        TranslateX = 1 << 13,

        /// <summary>
        /// Curve animating the Y component of a bone's translation.
        /// </summary>
        TranslateY = 1 << 14,

        /// <summary>
        /// Curve animating the Z component of a bone's translation.
        /// </summary>
        TranslateZ = 1 << 15
    }

    /// <summary>
    /// Represents how a bone transformation has to be applied.
    /// </summary>
    [Flags]
    public enum BoneAnimFlagsTransform : uint // Same as BoneFlagsTransform
    {
        SegmentScaleCompensate = 1 << 23,
        ScaleUniform = 1 << 24,
        ScaleVolumeOne = 1 << 25,
        RotateZero = 1 << 26,
        TranslateZero = 1 << 27,
        ScaleOne = ScaleVolumeOne | ScaleUniform,
        RotateTranslateZero = RotateZero | TranslateZero,
        Identity = ScaleOne | RotateZero | TranslateZero
    }
}