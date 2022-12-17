﻿using System;
using System.Collections.Generic;
using System.IO;
using Syroot.BinaryData;
using Syroot.Maths;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres.Helpers
{
    /// <summary>
    /// Represents a helper class for working with <see cref="VertexBuffer"/> instances.
    /// </summary>
    public class VertexBufferHelper
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBufferHelper"/> class.
        /// </summary>
        public VertexBufferHelper()
        {
            ByteOrder = ByteOrderHelper.SystemByteOrder;
            Attributes = new List<VertexBufferHelperAttrib>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBufferHelper"/> class with data read from the given
        /// <paramref name="vertexBuffer"/>. The data is available in the <paramref name="byteOrder"/>, which defaults
        /// to system byte order.
        /// </summary>
        /// <param name="vertexBuffer">The <see cref="VertexBuffer"/> to initially read data from.</param>
        /// <param name="byteOrder">The <see cref="ByteOrder"/> in which vertex data is available. <c>null</c> to use
        /// system byte order.</param>
        public VertexBufferHelper(VertexBuffer vertexBuffer, ByteOrder? byteOrder = null)
        {
            ByteOrder = byteOrder ?? ByteOrderHelper.SystemByteOrder;
            VertexSkinCount = vertexBuffer.VertexSkinCount;
            vertexBuffer.Attributes = vertexBuffer.Attributes ?? new List<VertexAttrib>();

            Attributes = new List<VertexBufferHelperAttrib>(vertexBuffer.Attributes.Count);
            foreach (VertexAttrib attrib in vertexBuffer.Attributes)
            {
                Attributes.Add(new VertexBufferHelperAttrib()
                {
                    Name = attrib.Name,
                    Format = attrib.Format,
                    Data = FromRawData(vertexBuffer, attrib)
                });
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="ByteOrder"/> in which vertex data will be stored when calling
        /// <see cref="ToVertexBuffer()"/>. This should be the same as the remainder of the <see cref="ResFile"/> in
        /// which it will be stored.
        /// </summary>
        public ByteOrder ByteOrder { get; set; }

        /// <summary>
        /// Gets or sets the number of bones influencing the vertices stored in the buffer. 0 influences equal
        /// rigidbodies (no skinning), 1 equal rigid skinning and 2 or more smooth skinning.
        /// </summary>
        public uint VertexSkinCount { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="VertexBufferHelperAttrib"/> instances which store the data.
        /// </summary>
        public IList<VertexBufferHelperAttrib> Attributes { get; set; }

        // ---- OPERATORS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="VertexBufferHelperAttrib"/> instance at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the <see cref="VertexBufferHelperAttrib"/> instance.</param>
        /// <returns>The <see cref="VertexBufferHelperAttrib"/> instance at the given index.</returns>
        public VertexBufferHelperAttrib this[int index]
        {
            get { return Attributes[index]; }
            set { Attributes[index] = value; }
        }

        /// <summary>
        /// Gets or sets the first <see cref="VertexBufferHelperAttrib"/> instance with the given
        /// <paramref name="attribName"/>.
        /// </summary>
        /// <param name="attribName">The name of the <see cref="VertexBufferHelperAttrib"/> instance.</param>
        /// <returns>The <see cref="VertexBufferHelperAttrib"/> instance with the given name.</returns>
        public VertexBufferHelperAttrib this[string attribName]
        {
            get
            {
                foreach (VertexBufferHelperAttrib attrib in Attributes)
                {
                    if (attrib.Name == attribName)
                    {
                        return attrib;
                    }
                }
                throw new ArgumentException($"No attribute with name {attribName} was found.", nameof(attribName));
            }
            set
            {
                int i = 0;
                foreach (VertexBufferHelperAttrib attrib in Attributes)
                {
                    if (attrib.Name == attribName)
                    {
                        Attributes[i] = value;
                        return;
                    }
                    i++;
                }
                throw new ArgumentException($"No attribute wFormat_16_16_16_16_Singleith name {attribName} was found.", nameof(attribName));
            }
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------
        
        /// <summary>
        /// Returns a <see cref="VertexBuffer"/> instance out of the stored helper data.
        /// </summary>
        /// <returns>A new <see cref="VertexBuffer"/>.</returns>
        public VertexBuffer ToVertexBuffer()
        {
            VertexBuffer vertexBuffer = new VertexBuffer();
            vertexBuffer.VertexSkinCount = VertexSkinCount;
            vertexBuffer.MemoryPool = new MemoryPool();

            // Go through each attribute and store it into its own buffer.
            int lastElementCount = Attributes[0].Data.Length;
            vertexBuffer.Attributes = new List<VertexAttrib>();
            vertexBuffer.AttributeDict = new ResDict();
            vertexBuffer.Buffers = new List<VertexBuffer.buffData>(Attributes.Count);
            vertexBuffer.StrideArray = new List<VertexBufferStride>();
            vertexBuffer.VertexBufferSizeArray = new List<VertexBufferSize>();

            foreach (VertexBufferHelperAttrib helperAttrib in Attributes)
            {
                // Check if the length of data does not match another attribute's data length.
                if (lastElementCount != helperAttrib.Data.Length)
                {
                    throw new InvalidDataException("Attribute data arrays have different sizes.");
                }

                // Add a VertexAttrib instance from the helper attribute.
                vertexBuffer.Attributes.Add(new VertexAttrib()
                {
                    Name = helperAttrib.Name,
                    Format = helperAttrib.Format,
                    BufferIndex = (byte)vertexBuffer.Buffers.Count,
                    Offset = 0
                });
                vertexBuffer.AttributeDict.Add(helperAttrib.Name);

                VertexBufferSize size = new VertexBufferSize();
                size.Size = (uint)ToRawData(helperAttrib).Length;

                VertexBufferStride stride = new VertexBufferStride();
                stride.Stride = (uint)helperAttrib.FormatSize;

                vertexBuffer.StrideArray.Add(stride);
                vertexBuffer.VertexBufferSizeArray.Add(size);

                // Create the buffer containing this attribute's data.
                vertexBuffer.Buffers.Add(new VertexBuffer.buffData()
                {
                    Stride = (ushort)helperAttrib.FormatSize,
                    Data = ToRawData(helperAttrib),
                    buffSize = ToRawData(helperAttrib).Length,
                    DataOffset = 0,
                });
            }
            
            return vertexBuffer;
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------
        
        private Vector4F[] FromRawData(VertexBuffer vertexBuffer, VertexAttrib attrib)
        {
            // Create a reader on the raw bytes of the correct endianness.
            VertexBuffer.buffData buffer = vertexBuffer.Buffers[attrib.BufferIndex];

            using (BinaryDataReader reader = new BinaryDataReader(new MemoryStream(buffer.Data)))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                // Get a conversion callback transforming the raw data into a Vector4F instance.
                Func<BinaryDataReader, Vector4F> callback = reader.GetAttribCallback(attrib.Format);

                // Read the elements.
                Vector4F[] elements = new Vector4F[vertexBuffer.VertexCount];
                for (int i = 0; i < vertexBuffer.VertexCount; i++)
                {
                    reader.Position = attrib.Offset + (i * vertexBuffer.StrideArray[attrib.BufferIndex].Stride);
                    elements[i] = callback.Invoke(reader);
                }
                return elements;
            }
        }

        private byte[] ToRawData(VertexBufferHelperAttrib helperAttrib)
        {
            // Create a write for the raw bytes of the correct endianness.
            byte[] raw = new byte[helperAttrib.Data.Length * helperAttrib.FormatSize];
            using (BinaryDataWriter writer = new BinaryDataWriter(new MemoryStream(raw, true)))
            {
                writer.ByteOrder = ByteOrder;

                // Get a conversion callback transforming the Vector4F instances into raw data.
                Action<BinaryDataWriter, Vector4F> callback = writer.GetAttribCallback(helperAttrib.Format);

                // Write the elements.
                foreach (Vector4F element in helperAttrib.Data)
                {
                    callback.Invoke(writer, element);
                }
            }
            return raw;
        }
    }
}
