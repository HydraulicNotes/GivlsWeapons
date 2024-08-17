using Terraria;
using Terraria.ModLoader;
namespace GivlsWeapons.Common.Players
{
    public class ReuseTimer : ModPlayer
    {
        public int ReuseTime = 0; // This timer is used to measure how long the player has been continuously reusing an item for. As it is now, doesn't work with items that have a delay or a different animation and use time.
        public bool canBeReusing; // Some items will continuously have the player's itemTime > 1, but some (such as those with alternate uses) will not
        public int holdTime = 0; // How long the player has been holding the button to use an item
        public override void PostUpdate()
        {
            if (Player.itemTime > 0)
            {
                ReuseTime++;
                canBeReusing = true;
            }
            else
            {
                if (canBeReusing)
                {
                    canBeReusing = false;
                }
                else
                {
                    ReuseTime = 0;
                }
            }

            if (Player.controlUseItem || Player.controlUseTile)
            {
                holdTime++;
            }
            else holdTime = 0;
        }
    }
}
