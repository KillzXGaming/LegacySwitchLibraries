﻿using System.Diagnostics;
using Syroot.Maths;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents a spatial bounding box.
    /// </summary>
    [DebuggerDisplay(nameof(Bounding) + " [{" + nameof(Center) + "},{" + nameof(Extent) + "})")]
    public struct Bounding
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The center point of the bounding box.
        /// </summary>
        public Vector3F Center;

        /// <summary>
        /// The extent from the center point to the furthest point.
        /// </summary>
        public Vector3F Extent;
    }
}
