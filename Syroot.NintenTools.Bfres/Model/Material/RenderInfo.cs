﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;
using System.IO;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents a render info in a FMAT section storing uniform parameters required to render the
    /// <see cref="UserData"/>.
    /// </summary>
    [DebuggerDisplay(nameof(RenderInfo) + " {" + nameof(Name) + "}")]
    public class RenderInfo : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderInfo"/> class.
        /// </summary>
        public RenderInfo()
        {
            Name = "";
            SetValue(new int[0]);
        }

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        public object _value;
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the <see cref="RenderInfoType"/> determining the data type of the stored value.
        /// </summary>
        public RenderInfoType Type { get; internal set; }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{RenderInfo}"/> instances.
        /// </summary>
        public string Name { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the stored value as an <see cref="Int32"/> array. Only valid if <see cref="Type"/> is
        /// <see cref="RenderInfoType.Int32"/>.
        /// </summary>
        /// <returns>The stored value as an <see cref="Int32"/> array.</returns>
        public int[] GetValueInt32s()
        {
            if (_value == null)
                return new int[0];

            return (int[])_value;
        }

        /// <summary>
        /// Gets the stored value as a <see cref="Single"/> array. Only valid if <see cref="Type"/> is
        /// <see cref="RenderInfoType.Single"/>.
        /// </summary>
        /// <returns>The stored value as a <see cref="Single"/> array.</returns>
        public float[] GetValueSingles()
        {
            if (_value == null)
                return new float[0];

            return (float[])_value;
        }

        /// <summary>
        /// Gets the stored value as a <see cref="String"/> array. Only valid if <see cref="Type"/> is
        /// <see cref="RenderInfoType.String"/>.
        /// </summary>
        /// <returns>The stored value as a <see cref="String"/> array.</returns>
        public string[] GetValueStrings()
        {
            if (_value == null || Type != RenderInfoType.String)
                return new string[0];

            return (string[])_value;
        }

        /// <summary>
        /// Sets the stored value as an <see cref="Int32"/> array and sets <see cref="Type"/> to
        /// <see cref="RenderInfoType.Int32"/>.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> array to set as the value.</param>
        public void SetValue(int[] value)
        {
            Type = RenderInfoType.Int32;
            _value = value;
        }

        /// <summary>
        /// Sets the stored value as a <see cref="Single"/> array and sets <see cref="Type"/> to
        /// <see cref="RenderInfoType.Single"/>.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> array to set as the value.</param>
        public void SetValue(float[] value)
        {
            Type = RenderInfoType.Single;
            _value = value;
        }

        /// <summary>
        /// Sets the stored value as a <see cref="String"/> array and sets <see cref="Type"/> to
        /// <see cref="RenderInfoType.String"/>.
        /// </summary>
        /// <param name="value">The <see cref="String"/> array to set as the value.</param>
        public void SetValue(string[] value)
        {
            Type = RenderInfoType.String;
            _value = value;
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Name = loader.LoadString();
            long DataOffset = loader.ReadOffset();
            ushort count    = loader.ReadUInt16();
            Type            = loader.ReadEnum<RenderInfoType>(true);
            loader.Seek(5);

            switch (Type)
            {
                case RenderInfoType.Int32:
                    _value = loader.LoadCustom(() => loader.ReadInt32s(count), DataOffset);
                    break;
                case RenderInfoType.Single:
                    _value = loader.LoadCustom(() => loader.ReadSingles(count), DataOffset);
                    break;
                case RenderInfoType.String:
                    if (DataOffset == 0) //Some games have empty data offset and no strings
                    {
                        string[] nullStrings = new string[0];
                        _value = nullStrings;
                    }
                    else
                        _value = loader.LoadCustom(() => loader.LoadStrings(count), DataOffset);
                    break;
            }
        }

        internal void ReadData(ResFileLoader loader, RenderInfoType type, int count)
        {
            Type = type;

            switch (Type)
            {
                case RenderInfoType.Int32:
                    _value = loader.ReadInt32s(count);
                    break;
                case RenderInfoType.Single:
                    _value = loader.ReadSingles(count);
                    break;
                case RenderInfoType.String:
                    _value = loader.LoadStrings(count);
                    break;
            }
        }

        internal void SaveStrings(ResFileSaver saver)
        {
            saver.SaveStrings((string[])_value);
        }
        internal void SaveInts(ResFileSaver saver)
        {
            saver.Write((int[])_value);
        }
        internal void SaveFloats(ResFileSaver saver)
        {
            saver.Write((float[])_value);
        }
        internal long DataOffset; 

        void IResData.Save(ResFileSaver saver)
        {
            saver.SaveString(Name);
            DataOffset = saver.SaveOffset();
            saver.Write(_value != null ? (ushort)((Array)_value).Length : (ushort)0); // Unsafe cast, but _value should always be Array.
            saver.Write(Type, true);
            saver.Seek(5);
        }

        public override string ToString()
        {
            return $"{Name} {Type} {string.Join(",", _value)}";
        }
    }

    /// <summary>
    /// Represents the data type of elements of the <see cref="RenderInfo"/> value array.
    /// </summary>
    public enum RenderInfoType : byte
    {
        /// <summary>
        /// The elements are <see cref="System.Int32"/> instances.
        /// </summary>
        Int32,

        /// <summary>
        /// The elements are <see cref="System.Single"/> instances.
        /// </summary>
        Single,

        /// <summary>
        /// The elements are <see cref="String"/> instances.
        /// </summary>
        String
    }
}