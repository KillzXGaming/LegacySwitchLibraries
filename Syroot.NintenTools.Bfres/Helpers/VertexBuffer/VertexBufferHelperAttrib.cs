using System;
using System.Diagnostics;
using Syroot.Maths;
using Syroot.NintenTools.NSW.Bfres.GFX;

namespace Syroot.NintenTools.NSW.Bfres.Helpers
{
    /// <summary>
    /// Represents an attribute and the data it stores in a <see cref="VertexBufferHelper"/> instance.
    /// </summary>
    [DebuggerDisplay(nameof(VertexBufferHelperAttrib) + " {" + nameof(Name) + "} {" + nameof(Format) + "}")]
    public class VertexBufferHelperAttrib
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The name of the attribute, typically used to determine the use of the data.
        /// </summary>
        public string Name;

        /// <summary>
        /// The <see cref="AttribFormat"/> into which data will be converted upon creating a
        /// <see cref="VertexBuffer"/>.
        /// </summary>
        public AttribFormat Format;

        /// <summary>
        /// The data stored for this attribute. Has to be of the same length as every other
        /// <see cref="VertexBufferHelperAttrib"/>. Depending on <see cref="Format"/>, not every component of the
        /// <see cref="Vector4F"/> elements is used.
        /// </summary>
        public Vector4F[] Data;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        internal int FormatSize
        {
            get
            {
                switch (Format)
                {
                    case AttribFormat.Format_8_UNorm:
                    case AttribFormat.Format_8_UInt:
                    case AttribFormat.Format_8_SNorm:
                    case AttribFormat.Format_8_SInt:
                    case AttribFormat.Format_8_UIntToSingle:
                    case AttribFormat.Format_8_SIntToSingle:
                    case AttribFormat.Format_4_4_UNorm:
                        return 1;
                    case AttribFormat.Format_16_UNorm:
                    case AttribFormat.Format_16_UInt:
                    case AttribFormat.Format_16_SNorm:
                    case AttribFormat.Format_16_SInt:
                    case AttribFormat.Format_16_Single:
                    case AttribFormat.Format_16_UIntToSingle:
                    case AttribFormat.Format_16_SIntToSingle:
                    case AttribFormat.Format_8_8_UNorm:
                    case AttribFormat.Format_8_8_UInt:
                    case AttribFormat.Format_8_8_SNorm:
                    case AttribFormat.Format_8_8_SInt:
                    case AttribFormat.Format_8_8_UIntToSingle:
                    case AttribFormat.Format_8_8_SIntToSingle:
                        return 2;
                    case AttribFormat.Format_32_UInt:
                    case AttribFormat.Format_32_SInt:
                    case AttribFormat.Format_32_Single:
                    case AttribFormat.Format_16_16_UNorm:
                    case AttribFormat.Format_16_16_UInt:
                    case AttribFormat.Format_16_16_SNorm:
                    case AttribFormat.Format_16_16_SInt:
                    case AttribFormat.Format_16_16_Single:
                    case AttribFormat.Format_16_16_UIntToSingle:
                    case AttribFormat.Format_16_16_SIntToSingle:
                    case AttribFormat.Format_10_11_11_Single:
                    case AttribFormat.Format_8_8_8_8_UNorm:
                    case AttribFormat.Format_8_8_8_8_UInt:
                    case AttribFormat.Format_8_8_8_8_SNorm:
                    case AttribFormat.Format_8_8_8_8_SInt:
                    case AttribFormat.Format_8_8_8_8_UIntToSingle:
                    case AttribFormat.Format_8_8_8_8_SIntToSingle:
                    case AttribFormat.Format_10_10_10_2_UNorm:
                    case AttribFormat.Format_10_10_10_2_UInt:
                    case AttribFormat.Format_10_10_10_2_SNorm:
                    case AttribFormat.Format_10_10_10_2_SInt:
                        return 4;
                    case AttribFormat.Format_32_32_UInt:
                    case AttribFormat.Format_32_32_SInt:
                    case AttribFormat.Format_32_32_Single:
                    case AttribFormat.Format_16_16_16_16_UNorm:
                    case AttribFormat.Format_16_16_16_16_UInt:
                    case AttribFormat.Format_16_16_16_16_SNorm:
                    case AttribFormat.Format_16_16_16_16_SInt:
                    case AttribFormat.Format_16_16_16_16_Single:
                    case AttribFormat.Format_16_16_16_16_UIntToSingle:
                    case AttribFormat.Format_16_16_16_16_SIntToSingle:
                        return 8;
                    case AttribFormat.Format_32_32_32_UInt:
                    case AttribFormat.Format_32_32_32_SInt:
                    case AttribFormat.Format_32_32_32_Single:
                        return 12;
                    case AttribFormat.Format_32_32_32_32_UInt:
                    case AttribFormat.Format_32_32_32_32_SInt:
                    case AttribFormat.Format_32_32_32_32_Single:
                        return 16;
                    default: throw new ArgumentException($"Invalid {nameof(AttribFormat)} {Format}.",
                        nameof(Format));
                }
            }
        }
    }
}
