using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ZoneConfigEnums;

namespace AbilityAndThemeClasses
{
    public class Themes
    {
        public List<ZoneThemes> list;

        public Themes(GameTiming timing)
        {
            switch (timing)
            {
                case GameTiming.Early:
                    this.list = new List<ZoneThemes>() {
                        ZoneThemes.Rock,
                        ZoneThemes.Fire,
                        ZoneThemes.Forest,
                        ZoneThemes.Wind
                    };

                    break;

                case GameTiming.Mid:
                    this.list = new List<ZoneThemes>() {
                        ZoneThemes.Lake
                    };

                    break;

                case GameTiming.Late:

                    break;

                case GameTiming.Post:

                    break;
            }
        }
    }

    public class Abilities
    {
        public List<ZoneAbilities> list;

        public Abilities(GameTiming timing)
        {

            switch (timing)
            {
                case GameTiming.Early:
                    this.list = new List<ZoneAbilities>() {
                        ZoneAbilities.DoubleJump,
                        ZoneAbilities.Dash,
                        ZoneAbilities.WallJump,
                        ZoneAbilities.MagicBullet
                    };

                    break;

                case GameTiming.Mid:
                    this.list = new List<ZoneAbilities>(){
                        ZoneAbilities.AirMask
                    };
                    break;

                case GameTiming.Late:

                    break;

                case GameTiming.Post:

                    break;
            }
        }
    }

}
