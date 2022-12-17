using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents a sphere boundry in a <see cref="SubMesh"/>  to determine when to show which sub mesh of a
    /// <see cref="Mesh"/>.
    /// </summary>
    public class Radius : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public float[] RadiusArray { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            RadiusArray = loader.ReadSingles(3);
        }

        void IResData.Save(ResFileSaver saver)
        {
            saver.Write(RadiusArray);
        }
    }
}