using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;
using Syroot.NintenTools.NSW.Bfres.GFX;
using System.ComponentModel;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents a <see cref="Texture"/> sampler in a <see cref="UserData"/> section, storing configuration on how to
    /// draw and interpolate textures.
    /// </summary>
    [DebuggerDisplay(nameof(Sampler) + " {" + nameof(Name) + "}")]
    public class Sampler : IResData
    {

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const ushort _flagsShrinkMask = 0b00000000_00110000;
        private const ushort _flagsExpandMask = 0b00000000_00001100;
        private const ushort _flagsMipmapMask = 0b00000000_00000011;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Sampler"/> class.
        /// </summary>
        public Sampler()
        {
            Name = "";
            WrapModeU = TexClamp.Repeat;
            WrapModeV = TexClamp.Repeat;
            WrapModeW = TexClamp.Clamp;
            CompareFunc = CompareFunction.Never;
            BorderColorType = TexBorderType.White;
            MaxAnisotropic = MaxAnisotropic.Ratio_1_1;
            LODBias = 0;
            MinLOD = 0;
            MaxLOD = 13;
            _filterFlags = 42;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Sampler}"/>
        /// instances.
        /// </summary>
        [Category("Sampler")]
        [DisplayName("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the UV wrap mode in the U direction
        /// </summary>
        [Description("The texture repetition mode on the X axis.")]
        [Category("Wrap")]
        [DisplayName("Wrap X")]
        public TexClamp WrapModeU { get; set; }

        /// <summary>
        /// Gets or sets the UV wrap mode in the V direction
        /// </summary>
        [Description("The texture repetition mode on the Y axis.")]
        [Category("Wrap")]
        [DisplayName("Wrap Y")]
        public TexClamp WrapModeV { get; set; }

        /// <summary>
        /// Gets or sets the UV wrap mode in the W direction
        /// </summary>
        [Description("The texture repetition mode on the Z axis.")]
        [Category("Wrap")]
        [DisplayName("Wrap Z")]
        public TexClamp WrapModeW { get; set; }

        /// <summary>
        /// Gets or sets the compare function
        /// </summary>
        [Description("The depth comparison function.")]
        [Category("Depth")]
        [DisplayName("Function")]
        public CompareFunction CompareFunc { get; set; }

        /// <summary>
        /// Gets or sets the border color
        /// </summary>
        [Description("Color to draw at places not reached by a texture if the clamp mode does not repeat it.")]
        [Category("Filter")]
        [DisplayName("Border Type")]
        public TexBorderType BorderColorType { get; set; }

        /// <summary>
        /// Gets or sets the max anisotropic filtering value
        /// </summary>
        [Description("The maximum anisotropic filtering level to use.")]
        [Category("Filter")]
        [DisplayName("Anisotropic Ratio")]
        public MaxAnisotropic MaxAnisotropic { get; set; }

        private ushort _filterFlags;

        [Description("The texture filtering on the X and Y axes when the texture is drawn smaller than the actual texture's resolution.")]
        [Category("Filter")]
        [DisplayName("Shrink XY")]
        public ShrinkFilterModes ShrinkXY
        {
            get { return (ShrinkFilterModes)(_filterFlags & _flagsShrinkMask); }
            set { _filterFlags = (ushort)(_filterFlags & ~_flagsShrinkMask | (ushort)value); }
        }

        [Description("The texture filtering on the X and Y axes when the texture is drawn larger than the actual texture's resolution.")]
        [Category("Filter")]
        [DisplayName("Exapnd XY")]
        public ExpandFilterModes ExpandXY
        {
            get { return (ExpandFilterModes)(_filterFlags & _flagsExpandMask); }
            set { _filterFlags = (ushort)(_filterFlags & ~_flagsExpandMask | (ushort)value); }

        }

        [Description("The texture filtering for mipmaps.")]
        [Category("Filter")]
        [DisplayName("MipMap")]
        public MipFilterModes Mipmap
        {
            get { return (MipFilterModes)(_filterFlags & _flagsMipmapMask); }
            set { _filterFlags = (ushort)(_filterFlags & ~_flagsMipmapMask | (ushort)value); }
        }

        public enum MipFilterModes : ushort
        {
            None = 0,
            Points = 1,
            Linear = 2,
        }

        public enum ExpandFilterModes : ushort
        {
            Points = 1 << 2,
            Linear = 2 << 2,
        }

        public enum ShrinkFilterModes : ushort
        {
            Points = 1 << 4,
            Linear = 2 << 4,
        }

        [Description("The minimum LoD level.")]
        [Category("LOD")]
        [DisplayName("Min")]    
        public float MinLOD { get; set; }

        [Description("The maximum LoD level.")]
        [Category("LOD")]
        [DisplayName("Max")]
        public float MaxLOD { get; set; }

        [Description("The LoD bias.")]
        [Category("LOD")]
        [DisplayName("Max")]
        public float LODBias { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            WrapModeU = loader.ReadEnum<TexClamp>(false);
            WrapModeV = loader.ReadEnum<TexClamp>(false);
            WrapModeW = loader.ReadEnum<TexClamp>(false);
            CompareFunc = loader.ReadEnum<CompareFunction>(false);
            BorderColorType = loader.ReadEnum<TexBorderType>(false);
            MaxAnisotropic = loader.ReadEnum<MaxAnisotropic>(false);
            _filterFlags = loader.ReadUInt16();
            MinLOD = loader.ReadSingle();
            MaxLOD = loader.ReadSingle();
            LODBias = loader.ReadSingle();
            loader.Seek(12);
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.Write(WrapModeU, false);
            saver.Write(WrapModeV, false);
            saver.Write(WrapModeW, false);
            saver.Write((byte)CompareFunc);
            saver.Write((byte)BorderColorType);
            saver.Write(MaxAnisotropic, false);
            saver.Write(_filterFlags);
            saver.Write((float)MinLOD);
            saver.Write((float)MaxLOD);
            saver.Write((float)LODBias);
            saver.Seek(12);
        }


    }
}