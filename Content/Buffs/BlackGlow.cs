using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using GivlsWeapons.Content.Dusts;

namespace GivlsWeapons.Content.Buffs
{
    internal class BlackGlow : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }
    }
}
