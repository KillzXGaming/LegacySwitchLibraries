using System.Collections.Generic;
using Syroot.Maths;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents a material animation in a <see cref="MaterialAnim"/> subfile, storing material animation data.
    /// </summary>
    public class TexturePatternAnimInfo : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TexturePatternAnimInfo"/> class.
        /// </summary>
        public TexturePatternAnimInfo()
        {
            Name = "";
            CurveIndex = ushort.MaxValue;
            BeginConstant = ushort.MaxValue;
            SubBindIndex = -1;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the index of the curve in the <see cref="TexturePatternAnimInfo"/>.
        /// </summary>
        public ushort CurveIndex;

        /// <summary>
        /// Gets or sets the index of the first <see cref="AnimConstant"/> instance in the parent
        /// <see cref="ShaderParamMatAnim"/>.
        /// </summary>
        public ushort BeginConstant { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="KeyShape"/> in the <see cref="Shape"/>.
        /// </summary>
        public sbyte SubBindIndex;

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{MaterialAnim}"/>
        /// instances.
        /// </summary>
        public string Name
        {
            get; set;
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Name = loader.LoadString();
            CurveIndex              = loader.ReadUInt16();
            BeginConstant           = loader.ReadUInt16(); 
            SubBindIndex            = loader.ReadSByte();
            loader.Seek(3); //padding
        }

        void IResData.Save(ResFileSaver saver)
        {
            saver.SaveString(Name);
            saver.Write(CurveIndex);
            saver.Write(BeginConstant);
            saver.Write(SubBindIndex);
            saver.Seek(3); //padding

        }
    }
}
