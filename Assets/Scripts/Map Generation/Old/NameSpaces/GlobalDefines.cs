using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;
using Classes;
using Randomization;

class GlobalDefines
{
    // Randomization
    public static RandomClass globalRandomClass = new RandomClass();

    // Room Naming
    public static string roomNamePrefix = "Room";
    public static string bossRoomNamePrefix = "BossRoom";
    public static string sendOffRoomName = "Send Off Room";
    public static string startRoomName = "Start Room";

    // Vein and None Grid Unit id
    public static int defaultId = -99;
    public static int veinIntendedId = -1;  // For veins
    public static int startZoneId = -2;    // For start and sendoff rooms

    // Themes and Ability handler
    public static zoneThemeAndAbilityScript themeAndAbilityManager = new zoneThemeAndAbilityScript();

    // Zone Ordering
    public static int defaultOrder = int.MaxValue - 1; // Since a lot of functions search for min order, need vein orders to be large
}
