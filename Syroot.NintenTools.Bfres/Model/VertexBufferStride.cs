using System;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;
using Syroot.NintenTools.NSW.Bfres.GFX;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents stride and size in a <see cref="VertexBuffer"/>
    /// specific data subset in the buffer.
    /// </summary>
    [DebuggerDisplay(nameof(VertexBufferStride) + " {" + nameof(Stride) + "}")]
    public class VertexBufferStride : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// The size of a full vertex in bytes.
        /// </summary>
        public uint Stride { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Stride = loader.ReadUInt32();
            loader.Seek(12);
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.Write(Stride);
            saver.Seek(12);
        }
    }
}