using System;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;
using Syroot.NintenTools.NSW.Bfres.GFX;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an attribute of a <see cref="VertexBuffer"/> describing the data format, type and layout of a
    /// specific data subset in the buffer.
    /// </summary>
    [DebuggerDisplay(nameof(VertexAttrib) + " {" + nameof(Name) + "}")]
    public class VertexAttrib : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VertexAttrib"/> class.
        /// </summary>
        public VertexAttrib()
        {
            Name = "";
            BufferIndex = 0;
            Offset = 0;
            Format = AttribFormat.Format_32_32_32_32_Single;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{VertexAttrib}"/> instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the index of the buffer storing the data in the <see cref="VertexBuffer.Buffers"/> list.
        /// </summary>
        public ushort BufferIndex { get; set; }

        /// <summary>
        /// Gets or sets the offset in bytes to the attribute in each vertex.
        /// </summary>
        public ushort Offset { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AttribFormat"/> determining the type in which attribute data is available.
        /// </summary>
        public AttribFormat Format { get; set; }
        
        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Name = loader.LoadString(); 
            loader.ByteOrder = BinaryData.ByteOrder.BigEndian;
            Format = loader.ReadEnum<AttribFormat>(true);
            loader.ByteOrder = BinaryData.ByteOrder.LittleEndian;
            loader.Seek(2); //padding
            Offset = loader.ReadUInt16();
            BufferIndex = loader.ReadUInt16();
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.SaveString(Name);
            saver.ByteOrder = BinaryData.ByteOrder.BigEndian;
            saver.Write(Format, true);
            saver.ByteOrder = BinaryData.ByteOrder.LittleEndian;
            saver.Write((short)0);
            saver.Write(Offset);
            saver.Write(BufferIndex);
        }
    }
}