using System.IO;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents a file attachment to a <see cref="ResFile"/> which can be of arbitrary data.
    /// </summary>
    public class ExternalFile : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalFile"/> class.
        /// </summary>
        public ExternalFile()
        {
            Data = new byte[0];
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the raw data stored by the external file.
        /// </summary>
        public byte[] Data { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Opens and returns a <see cref="MemoryStream"/> on the raw <see cref="Data"/> byte array, which optionally
        /// can be written to.
        /// </summary>
        /// <param name="writable"><c>true</c> to allow write access to the raw data.</param>
        /// <returns>The opened <see cref="MemoryStream"/> instance.</returns>
        public MemoryStream GetStream(bool writable = false)
        {
            return new MemoryStream(Data, writable);
        }

        public long ofsData;
        public long sizData;
        public long ExternalOffPos { get; set; }


        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            ExternalOffPos = loader.Position;

            ofsData = loader.ReadOffset(true);
            sizData = loader.ReadInt64();
            Data = loader.LoadCustom(() => loader.ReadBytes((int)sizData), ofsData);
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSaver.Section5, "External files"); //      <------------ Entry Set
            if (Data.Length <= 3)
                saver.SaveBlock(Data, (int)512, () => saver.Write(Data));
            else
                saver.SaveBlock(Data, (int)saver.ResFile.DataAlignment, () => saver.Write(Data));
            saver.Write((long)Data.Length);
        }
    }
}