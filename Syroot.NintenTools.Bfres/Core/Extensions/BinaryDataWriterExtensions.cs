﻿using System;
using System.Collections.Generic;
using Syroot.BinaryData;
using Syroot.Maths;
using Syroot.NintenTools.NSW.Bfres.GFX;

namespace Syroot.NintenTools.NSW.Bfres.Core
{
    /// <summary>
    /// Represents extension methods for the <see cref="BinaryDataWriter"/> class.
    /// </summary>
    public static class BinaryDataWriterExtensions
    {
        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        // ignores the higher 16 bits
        public static float toFloat(int hbits)
        {
            int mant = hbits & 0x03ff;            // 10 bits mantissa
            int exp = hbits & 0x7c00;            // 5 bits exponent
            if (exp == 0x7c00)                   // NaN/Inf
                exp = 0x3fc00;                    // -> NaN/Inf
            else if (exp != 0)                   // normalized value
            {
                exp += 0x1c000;                   // exp - 15 + 127
                if (mant == 0 && exp > 0x1c400)  // smooth transition
                    return BitConverter.ToSingle(BitConverter.GetBytes((hbits & 0x8000) << 16
                                                    | exp << 13 | 0x3ff), 0);
            }
            else if (mant != 0)                  // && exp==0 -> subnormal
            {
                exp = 0x1c400;                    // make it normal
                do
                {
                    mant <<= 1;                   // mantissa * 2
                    exp -= 0x400;                 // decrease exp by 1
                } while ((mant & 0x400) == 0); // while not normal
                mant &= 0x3ff;                    // discard subnormal bit
            }                                     // else +/-0 -> +/-0
            return BitConverter.ToSingle(BitConverter.GetBytes(          // combine all parts
                (hbits & 0x8000) << 16          // sign  << ( 31 - 15 )
                | (exp | mant) << 13), 0);         // value << ( 23 - 10 )
        }
        // returns all higher 16 bits as 0 for all results
        public static int fromFloat(float fval)
        {
            int fbits = BitConverter.ToInt32(BitConverter.GetBytes(fval), 0);
            int sign = fbits >> 16 & 0x8000;          // sign only
            int val = (fbits & 0x7fffffff) + 0x1000; // rounded value

            if (val >= 0x47800000)               // might be or become NaN/Inf
            {                                     // avoid Inf due to rounding
                if ((fbits & 0x7fffffff) >= 0x47800000)
                {                                 // is or must become NaN/Inf
                    if (val < 0x7f800000)        // was value but too large
                        return sign | 0x7c00;     // make it +/-Inf
                    return sign | 0x7c00 |        // remains +/-Inf or NaN
                        (fbits & 0x007fffff) >> 13; // keep NaN (and Inf) bits
                }
                return sign | 0x7bff;             // unrounded not quite Inf
            }
            if (val >= 0x38800000)               // remains normalized value
                return sign | val - 0x38000000 >> 13; // exp - 127 + 15
            if (val < 0x33000000)                // too small for subnormal
                return sign;                      // becomes +/-0
            val = (fbits & 0x7fffffff) >> 23;  // tmp exp for subnormal calc
            return sign | ((fbits & 0x7fffff | 0x800000) // add subnormal bit
                 + (0x800000 >> val - 102)     // round depending on cut off
              >> 126 - val);   // div by 2^(1-(exp-127+15)) and >> 13 | exp=0
        }

        public static int FloatToInt2(float value)
        {
            if (value < -1 || value > 1)
            {
                throw new ArgumentException($"{value} cannot be converted to Int2 (exceeds range -1 to 1).",
                    nameof(value));
            }
            return (int)(((uint)value << 30) >> 30) & 0x3;
        }

        public static int FloatToInt10(float value)
        {
            if (value < -512 || value > 511)
            {
                throw new ArgumentException($"{value} cannot be converted to Int10 (exceeds range -512 to 511).",
                    nameof(value));
            }
            return (int)(((uint)value << 22) >> 22) & 0x3FF;
        }

        /// <summary>
        /// Writes a <see cref="AnimConstant"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="AnimConstant"/> instance.</param>
        public static void Write(this BinaryDataWriter self, AnimConstant value)
        {
            self.Write(value.AnimDataOffset);
            self.Write(value.Value);
        }

        /// <summary>
        /// Writes <see cref="AnimConstant"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="AnimConstant"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<AnimConstant> values)
        {
            foreach (AnimConstant value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Bounding"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Bounding"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Bounding value)
        {
            self.Write(value.Center);
            self.Write(value.Extent);
        }

        /// <summary>
        /// Writes <see cref="Bounding"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Bounding"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Bounding> values)
        {
            foreach (Bounding value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Decimal10x5"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Decimal10x5"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Decimal10x5 value)
        {
            self.Write(value.Raw);
        }

        /// <summary>
        /// Writes <see cref="Decimal10x5"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Decimal10x5"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Decimal10x5> values)
        {
            foreach (Decimal10x5 value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Half"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Half"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Half value)
        {
            self.Write(value.Raw);
        }

        /// <summary>
        /// Writes <see cref="Half"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Half"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Half> values)
        {
            foreach (Half value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Matrix3x4"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Matrix3x4"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Matrix3x4 value)
        {
            self.Write(value.M11); self.Write(value.M12); self.Write(value.M13); self.Write(value.M14);
            self.Write(value.M21); self.Write(value.M22); self.Write(value.M23); self.Write(value.M24);
            self.Write(value.M31); self.Write(value.M32); self.Write(value.M33); self.Write(value.M34);
        }

        /// <summary>
        /// Writes <see cref="Matrix3x4"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Matrix3x4"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Matrix3x4> values)
        {
            foreach (Matrix3x4 value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector2Bool"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector2Bool"/> instance.</param>
        /// <param name="format">The <see cref="BinaryBooleanFormat"/> in which values are stored.</param>
        public static void Write(this BinaryDataWriter self, Vector2Bool value,
            BinaryBooleanFormat format = BinaryBooleanFormat.NonZeroByte)
        {
            self.Write(value.X, format);
            self.Write(value.Y, format);
        }

        /// <summary>
        /// Writes <see cref="Vector2Bool"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector2Bool"/> instances.</param>
        /// <param name="format">The <see cref="BinaryBooleanFormat"/> in which values are stored.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector2Bool> values,
            BinaryBooleanFormat format = BinaryBooleanFormat.NonZeroByte)
        {
            foreach (Vector2Bool value in values)
            {
                self.Write(value, format);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector2F"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector2F"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Vector2F value)
        {
            self.Write(value.X);
            self.Write(value.Y);
        }

        /// <summary>
        /// Writes <see cref="Vector2F"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector2F"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector2F> values)
        {
            foreach (Vector2F value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector2U"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector2U"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Vector2U value)
        {
            self.Write(value.X);
            self.Write(value.Y);
        }

        /// <summary>
        /// Writes <see cref="Vector2U"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector2U"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector2U> values)
        {
            foreach (Vector2U value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector3"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector3"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Vector3 value)
        {
            self.Write(value.X);
            self.Write(value.Y);
            self.Write(value.Z);
        }

        /// <summary>
        /// Writes <see cref="Vector3"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector3"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector3> values)
        {
            foreach (Vector3 value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector3Bool"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector3Bool"/> instance.</param>
        /// <param name="format">The <see cref="BinaryBooleanFormat"/> in which values are stored.</param>
        public static void Write(this BinaryDataWriter self, Vector3Bool value,
            BinaryBooleanFormat format = BinaryBooleanFormat.NonZeroByte)
        {
            self.Write(value.X, format);
            self.Write(value.Y, format);
            self.Write(value.Z, format);
        }

        /// <summary>
        /// Writes <see cref="Vector3Bool"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector3Bool"/> instances.</param>
        /// <param name="format">The <see cref="BinaryBooleanFormat"/> in which values are stored.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector3Bool> values,
            BinaryBooleanFormat format = BinaryBooleanFormat.NonZeroByte)
        {
            foreach (Vector3Bool value in values)
            {
                self.Write(value, format);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector3F"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector3F"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Vector3F value)
        {
            self.Write(value.X);
            self.Write(value.Y);
            self.Write(value.Z);
        }

        /// <summary>
        /// Writes <see cref="Vector3F"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector3F"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector3F> values)
        {
            foreach (Vector3F value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector3U"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector3U"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Vector3U value)
        {
            self.Write(value.X);
            self.Write(value.Y);
            self.Write(value.Z);
        }

        /// <summary>
        /// Writes <see cref="Vector3U"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector3U"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector3U> values)
        {
            foreach (Vector3U value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector4"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector4"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Vector4 value)
        {
            self.Write(value.X);
            self.Write(value.Y);
            self.Write(value.Z);
            self.Write(value.W);
        }

        /// <summary>
        /// Writes <see cref="Vector4"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector4"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector4> values)
        {
            foreach (Vector4 value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector4Bool"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector4Bool"/> instance.</param>
        /// <param name="format">The <see cref="BinaryBooleanFormat"/> in which values are stored.</param>
        public static void Write(this BinaryDataWriter self, Vector4Bool value,
            BinaryBooleanFormat format = BinaryBooleanFormat.NonZeroByte)
        {
            self.Write(value.X, format);
            self.Write(value.Y, format);
            self.Write(value.Z, format);
            self.Write(value.W, format);
        }

        /// <summary>
        /// Writes <see cref="Vector4Bool"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector4Bool"/> instances.</param>
        /// <param name="format">The <see cref="BinaryBooleanFormat"/> in which values are stored.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector4Bool> values,
            BinaryBooleanFormat format = BinaryBooleanFormat.NonZeroByte)
        {
            foreach (Vector4Bool value in values)
            {
                self.Write(value, format);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector4F"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector4F"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Vector4F value)
        {
            self.Write(value.X);
            self.Write(value.Y);
            self.Write(value.Z);
            self.Write(value.W);
        }

        /// <summary>
        /// Writes <see cref="Vector4F"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector4F"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector4F> values)
        {
            foreach (Vector4F value in values)
            {
                self.Write(value);
            }
        }


        /// <summary>
        /// Writes a <see cref="Vector4U"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector4U"/> instance.</param>
        public static void Write(this BinaryDataWriter self, Vector4U value)
        {
            self.Write(value.X);
            self.Write(value.Y);
            self.Write(value.Z);
            self.Write(value.W);
        }

        /// <summary>
        /// Writes <see cref="Vector4U"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector4U"/> instances.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector4U> values)
        {
            foreach (Vector4U value in values)
            {
                self.Write(value);
            }
        }

        /// <summary>
        /// Returns the conversion delegate for converting data available in the given <paramref name="attribFormat"/>
        /// from a <see cref="Vector4F"/> instance. Useful to prevent repetitive lookup for multiple values.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="attribFormat">The <see cref="ShaderParamType"/> of the data.</param>
        /// <returns>A conversion delegate for the data.</returns>
        public static Action<BinaryDataWriter, Vector4F> GetShaderParamTypeCallback(this BinaryDataWriter self,
            ShaderParamType paramType)
        {
            switch (paramType)
            {
                case ShaderParamType.Bool: return Write_8_UNorm;
                case ShaderParamType.Bool2: return Write_8_UNorm;
                case ShaderParamType.Bool3: return Write_8_UNorm;
                case ShaderParamType.Bool4: return Write_8_UNorm;
                case ShaderParamType.Float: return Write_8_UNorm;
                case ShaderParamType.Float2: return Write_8_UNorm;
                case ShaderParamType.Float3: return Write_8_UNorm;
                case ShaderParamType.Float4: return Write_8_UNorm;
                case ShaderParamType.Float2x2: return Write_8_UNorm;
                case ShaderParamType.Float2x3: return Write_8_UNorm;
                case ShaderParamType.Float2x4: return Write_8_UNorm;
                case ShaderParamType.Float4x2: return Write_8_UNorm;
                case ShaderParamType.Float4x3: return Write_8_UNorm;
                case ShaderParamType.Float4x4: return Write_8_UNorm;
                case ShaderParamType.Int: return Write_8_UNorm;
                case ShaderParamType.Int2: return Write_8_UNorm;
                case ShaderParamType.Int3: return Write_8_UNorm;
                case ShaderParamType.Int4: return Write_8_UNorm;
                case ShaderParamType.Reserved2: return Write_8_UNorm;
                case ShaderParamType.Reserved3: return Write_8_UNorm;
                case ShaderParamType.Reserved4: return Write_8_UNorm;
                case ShaderParamType.Srt2D: return Write_8_UNorm;
                case ShaderParamType.Srt3D: return Write_8_UNorm;
                case ShaderParamType.TexSrt: return Write_8_UNorm;
                case ShaderParamType.TexSrtEx: return Write_8_UNorm;
                case ShaderParamType.UInt: return Write_8_UNorm;
                case ShaderParamType.UInt2: return Write_8_UNorm;
                case ShaderParamType.UInt3: return Write_8_UNorm;
                case ShaderParamType.UInt4: return Write_8_UNorm;
                // Invalid
                default:
                    throw new ArgumentException($"Invalid {nameof(ShaderParamType)} {paramType}.",
               nameof(paramType));
            }
        }

            /// <summary>
            /// Returns the conversion delegate for converting data available in the given <paramref name="attribFormat"/>
            /// from a <see cref="Vector4F"/> instance. Useful to prevent repetitive lookup for multiple values.
            /// </summary>
            /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
            /// <param name="attribFormat">The <see cref="AttribFormat"/> of the data.</param>
            /// <returns>A conversion delegate for the data.</returns>
            public static Action<BinaryDataWriter, Vector4F> GetAttribCallback(this BinaryDataWriter self,
            AttribFormat attribFormat)
        {
            switch (attribFormat)
            {
                // 8-bit (8 x 1)
                case AttribFormat.Format_8_UNorm: return Write_8_UNorm;
                case AttribFormat.Format_8_UInt: return Write_8_UInt;
                case AttribFormat.Format_8_SNorm: return Write_8_SNorm;
                case AttribFormat.Format_8_SInt: return Write_8_SInt;
                case AttribFormat.Format_8_UIntToSingle: return Write_8_UIntToSingle;
                case AttribFormat.Format_8_SIntToSingle: return Write_8_SIntToSingle;
                // 8-bit (4 x 2)
                case AttribFormat.Format_4_4_UNorm: return Write_4_4_UNorm;
                // 16-bit (16 x 1)
                case AttribFormat.Format_16_UNorm: return Write_16_UNorm;
                case AttribFormat.Format_16_UInt: return Write_16_UInt;
                case AttribFormat.Format_16_SNorm: return Write_16_SNorm;
                case AttribFormat.Format_16_SInt: return Write_16_SInt;
                case AttribFormat.Format_16_Single: return Write_16_Single;
                case AttribFormat.Format_16_UIntToSingle: return Write_16_UIntToSingle;
                case AttribFormat.Format_16_SIntToSingle: return Write_16_SIntToSingle;
                // 16-bit (8 x 2)
                case AttribFormat.Format_8_8_UNorm: return Write_8_8_UNorm;
                case AttribFormat.Format_8_8_UInt: return Write_8_8_UInt;
                case AttribFormat.Format_8_8_SNorm: return Write_8_8_SNorm;
                case AttribFormat.Format_8_8_SInt: return Write_8_8_SInt;
                case AttribFormat.Format_8_8_UIntToSingle: return Write_8_8_UIntToSingle;
                case AttribFormat.Format_8_8_SIntToSingle: return Write_8_8_SIntToSingle;
                // 32-bit (32 x 1)
                case AttribFormat.Format_32_UInt: return Write_32_UInt;
                case AttribFormat.Format_32_SInt: return Write_32_SInt;
                case AttribFormat.Format_32_Single: return Write_32_Single;
                // 32-bit (16 x 2)
                case AttribFormat.Format_16_16_UNorm: return Write_16_16_UNorm;
                case AttribFormat.Format_16_16_UInt: return Write_16_16_UInt;
                case AttribFormat.Format_16_16_SNorm: return Write_16_16_SNorm;
                case AttribFormat.Format_16_16_SInt: return Write_16_16_SInt;
                case AttribFormat.Format_16_16_Single: return Write_16_16_Single;
                case AttribFormat.Format_16_16_UIntToSingle: return Write_16_16_UIntToSingle;
                case AttribFormat.Format_16_16_SIntToSingle: return Write_16_16_SIntToSingle;
                // 32-bit (10/11 x 3)
                case AttribFormat.Format_10_11_11_Single: return Write_10_11_11_Single;
                // 32-bit (8 x 4)
                case AttribFormat.Format_8_8_8_8_UNorm: return Write_8_8_8_8_UNorm;
                case AttribFormat.Format_8_8_8_8_UInt: return Write_8_8_8_8_UInt;
                case AttribFormat.Format_8_8_8_8_SNorm: return Write_8_8_8_8_SNorm;
                case AttribFormat.Format_8_8_8_8_SInt: return Write_8_8_8_8_SInt;
                case AttribFormat.Format_8_8_8_8_UIntToSingle: return Write_8_8_8_8_UIntToSingle;
                case AttribFormat.Format_8_8_8_8_SIntToSingle: return Write_8_8_8_8_SIntToSingle;
                // 32-bit (10 x 3 + 2)
                case AttribFormat.Format_10_10_10_2_UNorm: return Write_10_10_10_2_UNorm;
                case AttribFormat.Format_10_10_10_2_UInt: return Write_10_10_10_2_UInt;
                case AttribFormat.Format_10_10_10_2_SNorm: return Write_10_10_10_2_SNorm;
                case AttribFormat.Format_10_10_10_2_SInt: return Write_10_10_10_2_SInt;
                // 64-bit (32 x 2)
                case AttribFormat.Format_32_32_UInt: return Write_32_32_UInt;
                case AttribFormat.Format_32_32_SInt: return Write_32_32_SInt;
                case AttribFormat.Format_32_32_Single: return Write_32_32_Single;
                // 64-bit (16 x 4)
                case AttribFormat.Format_16_16_16_16_UNorm: return Write_16_16_16_16_UNorm;
                case AttribFormat.Format_16_16_16_16_UInt: return Write_16_16_16_16_UInt;
                case AttribFormat.Format_16_16_16_16_SNorm: return Write_16_16_16_16_SNorm;
                case AttribFormat.Format_16_16_16_16_SInt: return Write_16_16_16_16_SInt;
                case AttribFormat.Format_16_16_16_16_Single: return Write_16_16_16_16_Single;
                case AttribFormat.Format_16_16_16_16_UIntToSingle: return Write_16_16_16_16_UIntToSingle;
                case AttribFormat.Format_16_16_16_16_SIntToSingle: return Write_16_16_16_16_SIntToSingle;
                // 96-bit (32 x 3)
                case AttribFormat.Format_32_32_32_UInt: return Write_32_32_32_UInt;
                case AttribFormat.Format_32_32_32_SInt: return Write_32_32_32_SInt;
                case AttribFormat.Format_32_32_32_Single: return Write_32_32_32_Single;
                // 128-bit (32 x 4)
                case AttribFormat.Format_32_32_32_32_UInt: return Write_32_32_32_32_UInt;
                case AttribFormat.Format_32_32_32_32_SInt: return Write_32_32_32_32_SInt;
                case AttribFormat.Format_32_32_32_32_Single: return Write_32_32_32_32_Single;
                // Invalid
                default: throw new ArgumentException($"Invalid {nameof(AttribFormat)} {attribFormat}.",
                    nameof(attribFormat));
            }
        }

        /// <summary>
        /// Writes a <see cref="Vector4U"/> instance into the current stream with the given
        /// <paramref name="attribFormat"/>.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector4F"/> instance.</param>
        /// <param name="attribFormat">The <see cref="AttribFormat"/> of the data.</param>
        public static void Write(this BinaryDataWriter self, Vector4F value, AttribFormat attribFormat)
        {
            self.GetAttribCallback(attribFormat).Invoke(self, value);
        }

        /// <summary>
        /// Writes <see cref="Vector4U"/> instances into the current stream with the given
        /// <paramref name="attribFormat"/>.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector4U"/> instances.</param>
        /// <param name="attribFormat">The <see cref="AttribFormat"/> of the data.</param>
        public static void Write(this BinaryDataWriter self, IEnumerable<Vector4F> values,
            AttribFormat attribFormat)
        {
            Action<BinaryDataWriter, Vector4F> callback = self.GetAttribCallback(attribFormat);
            foreach (Vector4F value in values)
            {
                callback.Invoke(self, value);
            }
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        // ---- 8-bit (8 x 1) ----

        private static void Write_8_UNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((byte)(Algebra.Clamp(value.X, 0, 1) * 255));
        }

        private static void Write_8_UInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((byte)value.X);
        }

        private static void Write_8_SNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((sbyte)(Algebra.Clamp(value.X, -1, 1) * 127));
        }

        private static void Write_8_SInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((sbyte)value.X);
        }

        private static void Write_8_UIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((byte)value.X);
        }

        private static void Write_8_SIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((sbyte)value.X);
        }

        // ---- 8-bit (4 x 2) ----

        private static void Write_4_4_UNorm(this BinaryDataWriter self, Vector4F value)
        {
            byte x = (byte)(Algebra.Clamp(value.X, 0, 1) * 127);
            byte y = (byte)(Algebra.Clamp(value.Y, 0, 1) * 127);
            self.Write((byte)(x | y << 4));
        }

        // ---- 16-bit (16 x 1) ----

        private static void Write_16_UNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((ushort)(Algebra.Clamp(value.X, 0, 1) * 65535));
        }

        private static void Write_16_UInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((ushort)value.X);
        }

        private static void Write_16_SNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((short)(Algebra.Clamp(value.X, -1, 1) * 32767));
        }

        private static void Write_16_SInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((short)value.X);
        }

        private static void Write_16_Single(this BinaryDataWriter self, Vector4F value)
        {
            Write(self, (Half)value.X);
        }

        private static void Write_16_UIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((ushort)value.X);
        }

        private static void Write_16_SIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((short)value.X);
        }

        // ---- 16-bit (8 x 2) ----

        private static void Write_8_8_UNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((byte)(Algebra.Clamp(value.X, 0, 1) * 255));
            self.Write((byte)(Algebra.Clamp(value.Y, 0, 1) * 255));
        }

        private static void Write_8_8_UInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((byte)value.X);
            self.Write((byte)value.Y);
        }

        private static void Write_8_8_SNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((sbyte)(Algebra.Clamp(value.X, -1, 1) * 127));
            self.Write((sbyte)(Algebra.Clamp(value.Y, -1, 1) * 127));
        }

        private static void Write_8_8_SInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((sbyte)value.X);
            self.Write((sbyte)value.Y);
        }

        private static void Write_8_8_UIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((byte)value.X);
            self.Write((byte)value.Y);
        }

        private static void Write_8_8_SIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((sbyte)value.X);
            self.Write((sbyte)value.Y);
        }

        // ---- 32-bit (32 x 1) ----

        private static void Write_32_UInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((uint)value.X);
        }

        private static void Write_32_SInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((int)value.X);
        }

        private static void Write_32_Single(this BinaryDataWriter self, Vector4F value)
        {
            self.Write(value.X);
        }

        // ---- 32-bit (16 x 2) ----

        private static void Write_16_16_UNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((ushort)(Algebra.Clamp(value.X, 0, 1) * 65535));
            self.Write((ushort)(Algebra.Clamp(value.Y, 0, 1) * 65535));
        }

        private static void Write_16_16_UInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((ushort)value.X);
            self.Write((ushort)value.Y);
        }

        private static void Write_16_16_SNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((short)(Algebra.Clamp(value.X, -1, 1) * 32767));
            self.Write((short)(Algebra.Clamp(value.Y, -1, 1) * 32767));
        }

        private static void Write_16_16_SInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((short)value.X);
            self.Write((short)value.Y);
        }

        private static void Write_16_16_Single(this BinaryDataWriter self, Vector4F value)
        {
            Write(self, (Half)value.X);
            Write(self, (Half)value.Y);
        }

        private static void Write_16_16_UIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((ushort)value.X);
            self.Write((ushort)value.Y);
        }

        private static void Write_16_16_SIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((short)value.X);
            self.Write((short)value.Y);
        }

        // ---- 32-bit (10/11 x 3) ----

        private static void Write_10_11_11_Single(this BinaryDataWriter self, Vector4F value)
        {
            throw new NotImplementedException("10-bit and 11-bit Single values have not yet been implemented.");
        }

        // ---- 32-bit (8 x 4) ----

        private static void Write_8_8_8_8_UNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((byte)(Algebra.Clamp(value.X, 0, 1) * 255));
            self.Write((byte)(Algebra.Clamp(value.Y, 0, 1) * 255));
            self.Write((byte)(Algebra.Clamp(value.Z, 0, 1) * 255));
            self.Write((byte)(Algebra.Clamp(value.W, 0, 1) * 255));
        }

        private static void Write_8_8_8_8_UInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((byte)value.X);
            self.Write((byte)value.Y);
            self.Write((byte)value.Z);
            self.Write((byte)value.W);
        }

        private static void Write_8_8_8_8_SNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((sbyte)(Algebra.Clamp(value.X, -1, 1) * 127));
            self.Write((sbyte)(Algebra.Clamp(value.Y, -1, 1) * 127));
            self.Write((sbyte)(Algebra.Clamp(value.Z, -1, 1) * 127));
            self.Write((sbyte)(Algebra.Clamp(value.W, -1, 1) * 127));
        }

        private static void Write_8_8_8_8_SInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((sbyte)value.X);
            self.Write((sbyte)value.Y);
            self.Write((sbyte)value.Z);
            self.Write((sbyte)value.W);
        }

        private static void Write_8_8_8_8_UIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((byte)value.X);
            self.Write((byte)value.Y);
            self.Write((byte)value.Z);
            self.Write((byte)value.W);
        }

        private static void Write_8_8_8_8_SIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((sbyte)value.X);
            self.Write((sbyte)value.Y);
            self.Write((sbyte)value.Z);
            self.Write((sbyte)value.W);
        }

        // ---- 32-bit (10 x 3 + 2) ----

        private static void Write_10_10_10_2_UNorm(this BinaryDataWriter self, Vector4F value)
        {
            uint x = SingleToUInt10(Algebra.Clamp(value.X, 0, 1) * 1023);
            uint y = SingleToUInt10(Algebra.Clamp(value.Y, 0, 1) * 1023);
            uint z = SingleToUInt10(Algebra.Clamp(value.Z, 0, 1) * 1023);
            uint w = SingleToUInt2(Algebra.Clamp(value.W, 0, 1) * 3);
            self.Write(x | (y << 10) | (z << 20) | (w << 30));
        }

        private static void Write_10_10_10_2_UInt(this BinaryDataWriter self, Vector4F value)
        {
            uint x = SingleToUInt10(value.X);
            uint y = SingleToUInt10(value.Y);
            uint z = SingleToUInt10(value.Z);
            uint w = SingleToUInt2(value.W);
            self.Write(x | (y << 10) | (z << 20) | (w << 30));
        }

        private static void Write_10_10_10_2_SNorm(this BinaryDataWriter self, Vector4F value)
        {
            int x = SingleToInt10(Algebra.Clamp(value.X, -1, 1) * 511);
            int y = SingleToInt10(Algebra.Clamp(value.Y, -1, 1) * 511);
            int z = SingleToInt10(Algebra.Clamp(value.Z, -1, 1) * 511);
            int w = SingleToInt2(Algebra.Clamp(value.W, 0, 1));
            self.Write(x | (y << 10) | (z << 20) | (w << 30));
        }

        private static void Write_10_10_10_2_SInt(this BinaryDataWriter self, Vector4F value)
        {
            int x = SingleToInt10(value.X);
            int y = SingleToInt10(value.Y);
            int z = SingleToInt10(value.Z);
            int w = SingleToInt2(value.W);
            self.Write(x | (y << 10) | (z << 20) | (w << 30));
        }

        // ---- 64-bit (32 x 2) ----

        private static void Write_32_32_UInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((uint)value.X);
            self.Write((uint)value.Y);
        }

        private static void Write_32_32_SInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((int)value.X);
            self.Write((int)value.Y);
        }

        private static void Write_32_32_Single(this BinaryDataWriter self, Vector4F value)
        {
            self.Write(value.X);
            self.Write(value.Y);
        }

        // ---- 64-bit (16 x 4) ----

        private static void Write_16_16_16_16_UNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((ushort)(Algebra.Clamp(value.X, 0, 1) * 65535));
            self.Write((ushort)(Algebra.Clamp(value.Y, 0, 1) * 65535));
            self.Write((ushort)(Algebra.Clamp(value.Z, 0, 1) * 65535));
            self.Write((ushort)(Algebra.Clamp(value.W, 0, 1) * 65535));
        }

        private static void Write_16_16_16_16_UInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((ushort)value.X);
            self.Write((ushort)value.Y);
            self.Write((ushort)value.Z);
            self.Write((ushort)value.W);
        }

        private static void Write_16_16_16_16_SNorm(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((short)(Algebra.Clamp(value.X, -1, 1) * 32767));
            self.Write((short)(Algebra.Clamp(value.Y, -1, 1) * 32767));
            self.Write((short)(Algebra.Clamp(value.Z, -1, 1) * 32767));
            self.Write((short)(Algebra.Clamp(value.W, -1, 1) * 32767));
        }

        private static void Write_16_16_16_16_SInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((short)value.X);
            self.Write((short)value.Y);
            self.Write((short)value.Z);
            self.Write((short)value.W);
        }

        private static void Write_16_16_16_16_Single(this BinaryDataWriter self, Vector4F value)
        {
            Write(self, (Half)value.X);
            Write(self, (Half)value.Y);
            Write(self, (Half)value.Z);
            Write(self, (Half)value.W);
        }

        private static void Write_16_16_16_16_UIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((ushort)value.X);
            self.Write((ushort)value.Y);
            self.Write((ushort)value.Z);
            self.Write((ushort)value.W);
        }

        private static void Write_16_16_16_16_SIntToSingle(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((short)value.X);
            self.Write((short)value.Y);
            self.Write((short)value.Z);
            self.Write((short)value.W);
        }

        // --- 96-bit (32 x 3) ----

        private static void Write_32_32_32_UInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((uint)value.X);
            self.Write((uint)value.Y);
            self.Write((uint)value.Z);
        }

        private static void Write_32_32_32_SInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((int)value.X);
            self.Write((int)value.Y);
            self.Write((int)value.Z);
        }

        private static void Write_32_32_32_Single(this BinaryDataWriter self, Vector4F value)
        {
            self.Write(value.X);
            self.Write(value.Y);
            self.Write(value.Z);
        }

        // ---- 128-bit (32 x 4) ----

        private static void Write_32_32_32_32_UInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((uint)value.X);
            self.Write((uint)value.Y);
            self.Write((uint)value.Z);
            self.Write((uint)value.W);
        }

        private static void Write_32_32_32_32_SInt(this BinaryDataWriter self, Vector4F value)
        {
            self.Write((int)value.X);
            self.Write((int)value.Y);
            self.Write((int)value.Z);
            self.Write((int)value.W);
        }

        private static void Write_32_32_32_32_Single(this BinaryDataWriter self, Vector4F value)
        {
            self.Write(value.X);
            self.Write(value.Y);
            self.Write(value.Z);
            self.Write(value.W);
        }

        // --- Helper Methods ----

        private static int SingleToInt2(float value)
        {
            if (value < -1 || value > 1)
            {
                throw new ArgumentException($"{value} cannot be converted to Int2 (exceeds range -1 to 1).",
                    nameof(value));
            }
            return (int)(((uint)value << 30) >> 30) & 0b00000000_00000000_00000000_00000011;
        }

        private static int SingleToInt10(float value)
        {
            if (value < -512 || value > 511)
            {
                throw new ArgumentException($"{value} cannot be converted to Int10 (exceeds range -512 to 511).",
                    nameof(value));
            }
            return (int)(((uint)value << 22) >> 22) & 0b00000000_00000000_00000011_11111111;
        }

        private static uint SingleToUInt2(float value)
        {
            if (value < 0 || value > 3)
            {
                throw new ArgumentException($"{value} cannot be converted to UInt2 (exceeds range 0 to 3).",
                    nameof(value));
            }
            return (uint)value;
        }

        private static uint SingleToUInt10(float value)
        {
            if (value < 0 || value > 1023)
            {
                throw new ArgumentException($"{value} cannot be converted to UInt10 (exceeds range 0 to 1023).",
                    nameof(value));
            }
            return (uint)value;
        }
    }
}
