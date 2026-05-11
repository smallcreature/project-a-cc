using System;

namespace ProjectArcane.WorldGeneration
{
    [Serializable]
    public sealed class TerrainStageToggles
    {
        public bool Continentalness = true;
        public bool Erosion = true;
        public bool PeaksAndValleys = true;
        public bool CliffsAndOverhangs = true;
        public bool Caves = true;
    }
}
