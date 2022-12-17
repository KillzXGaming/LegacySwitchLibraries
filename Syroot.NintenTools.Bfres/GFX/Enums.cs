#pragma warning disable 1591 // Document enum members only when necessary.

using System;

namespace Syroot.NintenTools.NSW.Bfres.GFX
{
    //Note. These are not up to date at all and the buffer formats aren't all up to date
    //Thanks to https://github.com/magcius/model-viewer/blob/master/src/fres_nx/nngfx_enum.ts for some of the enum data.

    public enum MaxAnisotropic : byte
    {
        Ratio_1_1 = 0x1,
        Ratio_2_1 = 0x2,
        Ratio_4_1 = 0x4,
        Ratio_8_1 = 0x8,
        Ratio_16_1 = 0x10,
    }


    /// <summary>
    /// Represents the format of a vertex attribute entry. Possible type conversions:
    /// <para/>UNorm: attrib unsigned integer is converted to/from [0.0, 1.0] in shader.
    /// <para/>UInt: attrib unsigned integer is copied to/from shader as unsigned int.
    /// <para/>SNorm: attrib signed integer is converted to/from [-1.0, 1.0] in shader.
    /// <para/>SInt: attrib signed integer is copied to/from shader as signed int.
    /// <para/>Single: attrib single is copied to/from shader as Single.
    /// <para/>UIntToSingle: attrib unsigned integer is converted Single in shader.
    /// <para/>SIntToSingle: attrib signed integer is converted Single in shader.
    /// </summary>
    public enum AttribFormat : ushort
    {
        // 8 bits (8 x 1)
        Format_8_UNorm = 0x00000102, //
        Format_8_UInt = 0x00000302, //
        Format_8_SNorm = 0x00000202, //
        Format_8_SInt = 0x00000402, //
        Format_8_UIntToSingle = 0x00000802,
        Format_8_SIntToSingle = 0x00000A02,
        // 8 bits (4 x 2)
        Format_4_4_UNorm = 0x00000001,
        // 16 bits (16 x 1)
        Format_16_UNorm = 0x0000010A,
        Format_16_UInt = 0x0000020A,
        Format_16_SNorm = 0x0000030A,
        Format_16_SInt = 0x0000040A,
        Format_16_Single = 0x0000050A,
        Format_16_UIntToSingle = 0x00000803,
        Format_16_SIntToSingle = 0x00000A03,
        // 16 bits (8 x 2)
        Format_8_8_UNorm = 0x00000109, //
        Format_8_8_UInt = 0x00000309, //
        Format_8_8_SNorm = 0x00000209, //
        Format_8_8_SInt = 0x00000409, //
        Format_8_8_UIntToSingle = 0x00000804,
        Format_8_8_SIntToSingle = 0x00000A04,
        // 32 bits (16 x 2)
        Format_16_16_UNorm = 0x00000112, //
        Format_16_16_SNorm = 0x00000212, //
        Format_16_16_UInt = 0x00000312,
        Format_16_16_SInt = 0x00000412,
        Format_16_16_Single = 0x00000512, //
        Format_16_16_UIntToSingle = 0x00000807,
        Format_16_16_SIntToSingle = 0x00000A07,
        // 32 bits (10/11 x 3)
        Format_10_11_11_Single = 0x00000809,
        // 32 bits (8 x 4)
        Format_8_8_8_8_UNorm = 0x0000010B, //
        Format_8_8_8_8_SNorm = 0x0000020B, //
        Format_8_8_8_8_UInt = 0x0000030B, //
        Format_8_8_8_8_SInt = 0x0000040B, //
        Format_8_8_8_8_UIntToSingle = 0x0000080B,
        Format_8_8_8_8_SIntToSingle = 0x00000A0B,
        // 32 bits (10 x 3 + 2)
        Format_10_10_10_2_UNorm = 0x0000000B,
        Format_10_10_10_2_UInt = 0x0000090B,
        Format_10_10_10_2_SNorm = 0x0000020E, // High 2 bits are UNorm //
        Format_10_10_10_2_SInt = 0x0000099B,
        // 64 bits (16 x 4)
        Format_16_16_16_16_UNorm = 0x00000115, //
        Format_16_16_16_16_SNorm = 0x00000215, //
        Format_16_16_16_16_UInt = 0x00000315, //
        Format_16_16_16_16_SInt = 0x00000415, //
        Format_16_16_16_16_Single = 0x00000515, //
        Format_16_16_16_16_UIntToSingle = 0x0000080E,
        Format_16_16_16_16_SIntToSingle = 0x00000A0E,
        // 32 bits (32 x 1)
        Format_32_UInt = 0x00000314,
        Format_32_SInt = 0x00000416,
        Format_32_Single = 0x00000516,
        // 64 bits (32 x 2)
        Format_32_32_UInt = 0x00000317, //
        Format_32_32_SInt = 0x00000417, //
        Format_32_32_Single = 0x00000517, //
        // 96 bits (32 x 3)
        Format_32_32_32_UInt = 0x00000318, //
        Format_32_32_32_SInt = 0x00000418, //
        Format_32_32_32_Single = 0x00000518, //
        // 128 bits (32 x 4)
        Format_32_32_32_32_UInt = 0x00000319, //
        Format_32_32_32_32_SInt = 0x00000419, //
        Format_32_32_32_32_Single = 0x00000519 //
    }

    /// <summary>
    /// Represents compare functions used for depth and stencil tests.
    /// </summary>
    public enum CompareFunction : byte
    {
        Never,
        Less,
        Equal,
        LessOrEqual,
        Greater,
        NotEqual,
        GreaterOrEqual,
        Always
    }

    /// <summary>
    /// Represents the type in which vertex indices are stored.
    /// </summary>
    public enum IndexFormat : uint
    {
        UnsignedByte = 0,
        UInt16 = 1,
        UInt32 = 2,
    }

    /// <summary>
    /// Represents the base primitive used to draw each side of the polygon when dual-sided polygon mode is enabled.
    /// </summary>
    public enum PolygonMode : uint
    {
        Point,
        Line,
        Triangle
    }

    /// <summary>
    /// Represents the type of primitives to draw.
    /// </summary>
    public enum PrimitiveType : uint
    {
        Points = 0x00,
        Lines = 0x01,
        LineStrip = 0x04,
        Triangles = 0x03,
        TriangleStrip = 0x04,
        LinesAdjacency = 0x05,
        LineStripAdjacency = 0x06,
        TrianglesAdjacency = 0x07,
        TriangleStripAdjacency = 0x08,
        Patches = 0x09,
    }

    /// <summary>
    /// Represents type of border color to use.
    /// </summary>
    public enum TexBorderType : byte
    {
        White,
        Transparent,
        Opaque,
    }

    /// <summary>
    /// Represents how to treat texture coordinates outside of the normalized coordinate texture range.
    /// </summary>
    public enum TexClamp : sbyte
    {
        Repeat,
        Mirror,
        Clamp,
        ClampToEdge,
        MirrorOnce,
        MirrorOnceClampToEdge,
    }
}

#pragma warning restore 1591