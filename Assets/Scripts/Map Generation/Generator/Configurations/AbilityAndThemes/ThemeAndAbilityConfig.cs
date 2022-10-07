using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AbilityAndThemeEnums;
using AbilityAndThemeClasses;

public class ThemeAndAbilityConfig
{
    public Themes earlyGameZoneThemes;
    public Themes midGameZoneThemes;
    public Themes lateGameZoneThemes;

    public Abilities earlyGameAbilities;
    public Abilities midGameAbilities;
    public Abilities lateGameAbilities;

    public int currentZoneId;

    public List<LinkedZonesAndAbilities> linkedZoneThemesAndAbilities;

    public ThemeAndAbilityConfig()
    {
        this.earlyGameZoneThemes = new Themes(GameTiming.Early);
        this.midGameZoneThemes = new Themes(GameTiming.Mid);
        this.lateGameZoneThemes = new Themes(GameTiming.Late);

        this.earlyGameAbilities = new Abilities(GameTiming.Early);
        this.midGameAbilities = new Abilities(GameTiming.Mid);
        this.lateGameAbilities = new Abilities(GameTiming.Late);

        this.currentZoneId = 0;

        this.linkedZoneThemesAndAbilities = new List<LinkedZonesAndAbilities>
        {
            new LinkedZonesAndAbilities(ZoneAbilities.AirMask, ZoneThemes.Lake)
        };
    }

    public class LinkedZonesAndAbilities
    {
        public ZoneAbilities ability;
        public ZoneThemes zoneTheme;

        public LinkedZonesAndAbilities(ZoneAbilities ability, ZoneThemes theme)
        {
            this.ability = ability;
            this.zoneTheme = theme;
        }
    }


    public Zone_New getNewZone(GameTiming timing)
    {
        ZoneThemes randomTheme = ZoneThemes.None;
        ZoneAbilities randomAbility = ZoneAbilities.None;

        switch (timing)
        {
            case GameTiming.Early:
                getStartingZone(ref randomTheme, ref randomAbility);
                break;

            case GameTiming.Mid:
                getMidGameZone(ref randomTheme, ref randomAbility);
                break;

                //case GameTiming.Late:
                //    choosenZoneProps = getLateGameZone();
                //    break;

                //case GameTiming.Post:
                //    choosenZoneProps = getPostGameZone();
                //    break;
        }

        Zone_New newZone = new Zone_New(currentZoneId, randomAbility, randomTheme);

        currentZoneId++;

        return newZone;
    }

    //public ZoneThemes getZoneTheme(int zoneId)
    //{
    //    // Gets the zone theme from the id
    //    ZoneThemes theme = ZoneThemes.None;

    //    for (int i = 0; i < choosenUnitZoneList.Count; i++)
    //    {
    //        if (choosenUnitZoneList[i].zoneAreaId == zoneId)
    //        {
    //            theme = choosenUnitZoneList[i].zoneTheme;
    //        }
    //    }

    //    return theme;
    //}

    //public ZoneAbilities getZoneAbility(int zoneId)
    //{
    //    // Gets the zone ability from the id
    //    ZoneAbilities ability = ZoneAbilities.None;

    //    for (int i = 0; i < choosenUnitZoneList.Count; i++)
    //    {
    //        if (choosenUnitZoneList[i].zoneAreaId == zoneId)
    //        {
    //            ability = choosenUnitZoneList[i].zoneAbility;
    //        }
    //    }

    //    return ability;
    //}

    public void getStartingZone(ref ZoneThemes randomTheme, ref ZoneAbilities randomAbility)
    {
        // Will randomly choose a zone theme and ability
        //      Once 3 are choosen the remaining fourth ability/theme will be added to mid game

        // There should only be 3 starting zones and once the third one is choosen then the fourth should flow over to mid game
        if (earlyGameZoneThemes.list.Count == 1)
            Debug.LogError("getStartingZone() - Too many starting zones created");

        // Randomly choose the theme/ability
        int randomThemeIndex = Random.Range(0, earlyGameZoneThemes.list.Count);
        int randomAbilityIndex = Random.Range(0, earlyGameAbilities.list.Count);

        randomTheme = (ZoneThemes)earlyGameZoneThemes.list[randomThemeIndex];
        randomAbility = (ZoneAbilities)earlyGameAbilities.list[randomAbilityIndex];

        // Remove from the list
        earlyGameZoneThemes.list.RemoveAt(randomThemeIndex);
        earlyGameAbilities.list.RemoveAt(randomAbilityIndex);


        // Once we have one left over, add it to the mid game list
        if (earlyGameZoneThemes.list.Count == 1)
        {
            ZoneThemes leftOverTheme = earlyGameZoneThemes.list[0];
            ZoneAbilities leftOverAbility = earlyGameAbilities.list[0];

            // Add to mid game list
            midGameZoneThemes.list.Add(leftOverTheme);
            midGameAbilities.list.Add(leftOverAbility);

            // Remove from early game list
            earlyGameZoneThemes.list.Remove(leftOverTheme);
            earlyGameAbilities.list.Remove(leftOverAbility);
        }
    }


    public void getMidGameZone(ref ZoneThemes randomTheme, ref ZoneAbilities randomAbility)
    {
        // Will randomly choose a zone theme and ability
        //    Remember that some abilities need to be paired with certain zones, EX air mask and lake theme

        // Randomly choose the theme/ability
        int randomThemeIndex = Random.Range(0, midGameZoneThemes.list.Count);
        int randomAbilityIndex = 0;

        randomTheme = (ZoneThemes)midGameZoneThemes.list[randomThemeIndex];

        // Check if the random theme has a linked ability
        if (isLinkedTheme(randomTheme))
        {
            randomAbility = getLinkedAbility(randomTheme);
        }
        // Else choose a random ability that isn't linked (for random fairness)
        else
        {
            randomAbility = ZoneAbilities.None;

            while (randomAbility == ZoneAbilities.None || isLinkedAbility(ZoneAbilities.None))
            {
                randomAbilityIndex = Random.Range(0, midGameAbilities.list.Count);
                randomAbility = (ZoneAbilities)midGameAbilities.list[randomAbilityIndex];
            }
        }

        // Remove the choosen themes and abilities from the list
        midGameZoneThemes.list.Remove(randomTheme);
        midGameAbilities.list.Remove(randomAbility);
    }

    bool isLinkedTheme(ZoneThemes theme)
    {
        for (int i = 0; i < linkedZoneThemesAndAbilities.Count; i++)
        {
            if (linkedZoneThemesAndAbilities[i].zoneTheme == theme)
            {
                return true;
            }
        }

        return false;
    }

    ZoneAbilities getLinkedAbility(ZoneThemes theme)
    {
        ZoneAbilities linkedAbility = ZoneAbilities.None;

        for (int i = 0; i < linkedZoneThemesAndAbilities.Count; i++)
        {
            if (linkedZoneThemesAndAbilities[i].zoneTheme == theme)
            {
                linkedAbility = linkedZoneThemesAndAbilities[i].ability;
            }
        }

        return linkedAbility;
    }

    bool isLinkedAbility(ZoneAbilities ability)
    {
        for (int i = 0; i < linkedZoneThemesAndAbilities.Count; i++)
        {
            if (linkedZoneThemesAndAbilities[i].ability == ability)
            {
                return true;
            }
        }

        return false;
    }
}
