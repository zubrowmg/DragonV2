using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedDefinesAndEnums;
using AbilityAndThemeEnums;
using AbilityAndThemeClasses;

public class Zone_New
{
    int id = CommonDefines.DefualtId;
    ZoneAbilities zoneAbility = ZoneAbilities.None;
    ZoneThemes zoneTheme = ZoneThemes.None;

    // Zones are created during vein zone generation
    public Zone_New(int id, ZoneAbilities ability, ZoneThemes theme)
    {
        this.id = id;
        this.zoneAbility = ability;
        this.zoneTheme = theme;
    }


}
