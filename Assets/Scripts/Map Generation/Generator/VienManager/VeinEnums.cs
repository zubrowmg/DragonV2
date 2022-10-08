using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VeinEnums
{
    public enum VeinType { Simple, U, Zone, None_Set };
    public enum VeinDirection { Left, Right, None_Set };

    public enum VeinDistanceTraveled { None, One_Sixths, Two_Sixths, Three_Sixths, Four_Sixths, Five_Sixths };
    
    public enum DebugVeinTileType { None, Vein, VeinMain };

    public enum SlopeChange { None, Inc, Dec };

    public enum UVeinType { Shift, Widen, Shift_Widen };
    public enum UVeinStage { Part1, Part2, Part3, Part4 };


}
