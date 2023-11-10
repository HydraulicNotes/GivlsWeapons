using Terraria;
using Terraria.ModLoader;
namespace GivlsWeapons.Content.Globals
{
    public class ReuseTimer : ModPlayer
    {
        public int Timer = 0; // This timer is used to measure how long the player has been continuously reusing an item for. As it is now, doesn't work with items that have a delay or a different animation and use time.
        public bool canBeReusing; // Some items will continuously have the player's itemTime > 1, but some (such as those with alternate uses) will not
        public override void PostUpdate()
        {
            if(Player.itemTime > 0)
            {
                Timer++;
                canBeReusing = true;
            }
            else
            {
                if(canBeReusing)
                {
                    canBeReusing = false;
                }
                else
                {
                    Timer = 0;
                }
            }
        }
    }
}
