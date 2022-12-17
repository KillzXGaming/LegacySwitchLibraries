using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents a key shape animation info in a <see cref="VertexShapeAnim"/> instance.
    /// </summary>
    [DebuggerDisplay(nameof(ParamAnimInfo) + " {" + nameof(Name) + "}")]
    public class KeyShapeAnimInfo : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyShapeAnimInfo"/> class.
        /// </summary>
        public KeyShapeAnimInfo()
        {
            Name = "";
            CurveIndex = 0;
            SubBindIndex = 0;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the index of the curve in the <see cref="VertexShapeAnim"/>.
        /// </summary>
        public sbyte CurveIndex;

        /// <summary>
        /// Gets or sets the index of the <see cref="KeyShape"/> in the <see cref="Shape"/>.
        /// </summary>
        public sbyte SubBindIndex;

        /// <summary>
        /// Gets or sets the name of the <see cref="KeyShape"/> in the <see cref="Shape"/>.
        /// </summary>
        public string Name;

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Name         = loader.LoadString();
            CurveIndex   = loader.ReadSByte();
            SubBindIndex = loader.ReadSByte();
            loader.Seek(6);
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.SaveString(Name);
            saver.Write(CurveIndex);
            saver.Write(SubBindIndex);
            saver.Seek(6);
        }
    }
}