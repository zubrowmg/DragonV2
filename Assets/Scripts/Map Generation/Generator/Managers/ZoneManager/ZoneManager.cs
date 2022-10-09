using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AbilityAndThemeClasses;
using AbilityAndThemeEnums;

// ==========================================================
//              Zone Manager Accessors
// ==========================================================
public partial class ContainerAccessor
{
    public Zone_New createNewZone(GameTiming gameTiming)
    {
        // Creates a new zone and adds it to the zoneContainer
        Zone_New newZone = this.contInst.themeAndAbilityConfig.getNewZone(gameTiming);
        this.contInst.zoneContainer.addZone(ref newZone);
        return newZone;
    }
}
public class ZoneManager_New : ContainerAccessor
{
    public ZoneManager_New(ref GeneratorContainer contInst) : base(ref contInst)
    {
    }
}


