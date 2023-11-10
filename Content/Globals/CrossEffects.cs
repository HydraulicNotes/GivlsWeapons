using Terraria;
using Terraria.ModLoader;
using GivlsWeapons.Content.Items.Weapons;

namespace GivlsWeapons.Content.Globals
{
    public class CrossEffects : ModPlayer
    {
        public override void SetControls()
        {
            if (Player.HeldItem.ModItem is Cross  && Player.ItemAnimationActive)
            {
                Player.controlDown = false;
                Player.controlJump = false;
                Player.controlLeft = false;
                Player.controlRight = false;
                Player.controlUp = false;
                Player.controlHook = false;
                Player.controlUseTile = false;
                Player.controlThrow = false;
            }
        }
    }
}
