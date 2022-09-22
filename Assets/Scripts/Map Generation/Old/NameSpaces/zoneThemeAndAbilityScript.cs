using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;
using Classes;

public class zoneThemeAndAbilityScript
{
    public Themes earlyGameZoneThemes;
    public Themes midGameZoneThemes;
    public Themes lateGameZoneThemes;

    public Abilities earlyGameAbilities;
    public Abilities midGameAbilities;
    public Abilities lateGameAbilities;

    public int currentZoneId;
    public List<ZoneUnitProperties> choosenUnitZoneList;

    public List<LinkedZonesAndAbilities> linkedZoneThemesAndAbilities;

    public zoneThemeAndAbilityScript()
    {
        this.earlyGameZoneThemes = new Themes(gameTiming.Early);
        this.midGameZoneThemes = new Themes(gameTiming.Mid);
        this.lateGameZoneThemes = new Themes(gameTiming.Late);

        this.earlyGameAbilities = new Abilities(gameTiming.Early);
        this.midGameAbilities = new Abilities(gameTiming.Mid);
        this.lateGameAbilities = new Abilities(gameTiming.Late);

        this.currentZoneId = 0;
        this.choosenUnitZoneList = new List<ZoneUnitProperties>();

        this.linkedZoneThemesAndAbilities = new List<LinkedZonesAndAbilities>
        {
            new LinkedZonesAndAbilities(abilities.AirMask, zoneThemes.Lake)
        };
    }

    public class LinkedZonesAndAbilities
    {
        public abilities ability;
        public zoneThemes zoneTheme;

        public LinkedZonesAndAbilities(abilities ability, zoneThemes theme)
        {
            this.ability = ability;
            this.zoneTheme = theme;
        }
    }


    public ZoneUnitProperties getNewZone(gameTiming timing)
    {
        ZoneUnitProperties choosenZoneProps = new ZoneUnitProperties();

        zoneThemes randomTheme = zoneThemes.None;
        abilities randomAbility = abilities.None;

        switch (timing)
        {
            case gameTiming.Early:
                getStartingZone(ref randomTheme, ref randomAbility);
                break;

            case gameTiming.Mid:
                getMidGameZone(ref randomTheme, ref randomAbility);
                break;

            //case gameTiming.Late:
            //    choosenZoneProps = getLateGameZone();
            //    break;

            //case gameTiming.Post:
            //    choosenZoneProps = getPostGameZone();
            //    break;
        }

        choosenZoneProps = new ZoneUnitProperties(randomTheme, randomAbility, currentZoneId);
        choosenUnitZoneList.Add(choosenZoneProps);

        currentZoneId++;

        return choosenZoneProps;
    }

    public zoneThemes getZoneTheme(int zoneId)
    {
        // Gets the zone theme from the id
        zoneThemes theme = zoneThemes.None;

        for (int i = 0; i < choosenUnitZoneList.Count; i++)
        {
            if (choosenUnitZoneList[i].zoneAreaId == zoneId)
            {
                theme = choosenUnitZoneList[i].zoneTheme;
            }
        }

        return theme;
    }

    public abilities getZoneAbility(int zoneId)
    {
        // Gets the zone ability from the id
        abilities ability = abilities.None;

        for (int i = 0; i < choosenUnitZoneList.Count; i++)
        {
            if (choosenUnitZoneList[i].zoneAreaId == zoneId)
            {
                ability = choosenUnitZoneList[i].zoneAbility;
            }
        }

        return ability;
    }

    public void getStartingZone(ref zoneThemes randomTheme, ref abilities randomAbility)
    {
        // Will randomly choose a zone theme and ability
        //      Once 3 are choosen the remaining fourth ability/theme will be added to mid game

        // There should only be 3 starting zones and once the third one is choosen then the fourth should flow over to mid game
        if (earlyGameZoneThemes.list.Count == 1)
            Debug.LogError("getStartingZone() - Too many starting zones created");

        // Randomly choose the theme/ability
        int randomThemeIndex = Random.Range(0, earlyGameZoneThemes.list.Count);
        int randomAbilityIndex = Random.Range(0, earlyGameAbilities.list.Count);

        randomTheme = (zoneThemes)earlyGameZoneThemes.list[randomThemeIndex];
        randomAbility = (abilities)earlyGameAbilities.list[randomAbilityIndex];

        // Remove from the list
        earlyGameZoneThemes.list.RemoveAt(randomThemeIndex);
        earlyGameAbilities.list.RemoveAt(randomAbilityIndex);


        // Once we have one left over, add it to the mid game list
        if (earlyGameZoneThemes.list.Count == 1)
        {
            zoneThemes leftOverTheme = earlyGameZoneThemes.list[0];
            abilities leftOverAbility = earlyGameAbilities.list[0];

            // Add to mid game list
            midGameZoneThemes.list.Add(leftOverTheme);
            midGameAbilities.list.Add(leftOverAbility);

            // Remove from early game list
            earlyGameZoneThemes.list.Remove(leftOverTheme);
            earlyGameAbilities.list.Remove(leftOverAbility);
        }
    }

    
    public void getMidGameZone(ref zoneThemes randomTheme, ref abilities randomAbility)
    {
        // Will randomly choose a zone theme and ability
        //    Remember that some abilities need to be paired with certain zones, EX air mask and lake theme

        // Randomly choose the theme/ability
        int randomThemeIndex = Random.Range(0, midGameZoneThemes.list.Count);
        int randomAbilityIndex = 0; 

        randomTheme = (zoneThemes)midGameZoneThemes.list[randomThemeIndex];

        // Check if the random theme has a linked ability
        if (isLinkedTheme(randomTheme))
        {
            randomAbility = getLinkedAbility(randomTheme);
        }
        // Else choose a random ability that isn't linked (for random fairness)
        else
        {
            randomAbility = abilities.None;

            while (randomAbility == abilities.None || isLinkedAbility(abilities.None))
            {
                randomAbilityIndex = Random.Range(0, midGameAbilities.list.Count);
                randomAbility = (abilities)midGameAbilities.list[randomAbilityIndex];
            }
        }

        // Remove the choosen themes and abilities from the list
        midGameZoneThemes.list.Remove(randomTheme);
        midGameAbilities.list.Remove(randomAbility);
    }

    public bool isLinkedTheme(zoneThemes theme)
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

    public abilities getLinkedAbility(zoneThemes theme)
    {
        abilities linkedAbility = abilities.None;

        for (int i = 0; i < linkedZoneThemesAndAbilities.Count; i++)
        {
            if (linkedZoneThemesAndAbilities[i].zoneTheme == theme)
            {
                linkedAbility = linkedZoneThemesAndAbilities[i].ability;
            }
        }

        return linkedAbility;
    }

    public bool isLinkedAbility(abilities ability)
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
