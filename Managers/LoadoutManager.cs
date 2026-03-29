using Rage;
using Rage.Native;
using System;

namespace EmsPlus.Managers
{
    public static class LoadoutManager
    {
        public static void EquipLoadout()
        {
            Ped player = Game.LocalPlayer.Character;

            player.Inventory.Weapons.Clear();

            string rawString = EntryPoint.LoadoutConfig.DefaultLoadout.Value;
            string[] weaponNames = rawString.Split(',');

            foreach (string weaponName in weaponNames)
            {
                string cleanName = weaponName.Trim();
                if (string.IsNullOrEmpty(cleanName)) continue;

                WeaponHash finalHash;
                bool isValid = false;

                if (Enum.TryParse(cleanName, true, out WeaponHash wHash))
                {
                    finalHash = wHash;
                    isValid = true;
                }
                else
                {
                    uint nativeHash = Game.GetHashKey(cleanName);

                    if (NativeFunction.Natives.IS_WEAPON_VALID<bool>(nativeHash))
                    {
                        finalHash = (WeaponHash)nativeHash;
                        isValid = true;
                    }
                    else
                    {
                        finalHash = 0;
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    player.Inventory.GiveNewWeapon(finalHash, 9999, false);
                }
                else
                {
                    Game.Console.Print($"[EmsPlus] Warning: Could not find valid weapon for '{cleanName}'. Check spelling or add 'WEAPON_' prefix.");
                }
            }

            // Game.Console.Print("[EmsPlus] EMS Loadout defined in Louadout.ini was equipped.");
        }
    }
}