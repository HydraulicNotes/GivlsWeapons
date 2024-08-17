using GivlsWeapons.Content.Buffs;
using GivlsWeapons.Content.Dusts;
using System;
using Terraria.ID;

namespace GivlsWeapons.Common.NPCs
{
    public class PlayerBuffEffects : ModPlayer
    {
        public override void UpdateBadLifeRegen()
        {
            if (Player.HasBuff(BuffID.ShadowFlame))
            {
                if (Player.lifeRegen > 0) Player.lifeRegen = 0;
                Player.lifeRegenTime = 0;
                Player.lifeRegen -= 30;
                if (Player.HasBuff(BuffID.Oiled)) Player.lifeRegen -= 50;

                if (Main.rand.Next(5) < 4)
                {
                    Dust dust3 = Dust.NewDustDirect(new Vector2(Player.position.X - 2f, Player.position.Y - 2f), Player.width + 4, Player.height + 4, DustID.Shadowflame, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 180, default(Color), 1.95f);
                    dust3.noGravity = true;
                    dust3.velocity *= 0.75f;
                    dust3.velocity.X *= 0.75f;
                    dust3.velocity.Y -= 1f;
                    if (Main.rand.NextBool(4))
                    {
                        dust3.noGravity = false;
                        dust3.scale *= 0.5f;
                    }
                }
            }
        }
    }
}
