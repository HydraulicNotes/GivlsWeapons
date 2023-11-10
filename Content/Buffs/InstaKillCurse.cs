using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using GivlsWeapons.Content.Dusts;
using Terraria.DataStructures;
using Terraria.Chat.Commands;
using Terraria.Localization;
using Terraria.ID;
using System;

namespace GivlsWeapons.Content.Buffs
{
    internal class InstaKillCurse : ModBuff
    { // We don't do anything here because what we need can only be done by the ModPlayer
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            float dustAngle = Main.rand.NextFloat() * MathF.Tau;
            Vector2 dustPos = player.Center + dustAngle.ToRotationVector2() * player.width;
            Dust particle = Dust.NewDustPerfect(dustPos, DustID.Wraith, (-dustAngle.ToRotationVector2() * 2f) + player.velocity);
            particle.noGravity = true;
        }
    }

    internal class InstaKillCursePlayer : ModPlayer
    {

        public override void OnHurt(Player.HurtInfo info) //If the player is hurt, check for the buff and kill them if they have it
        {
            if (Player.whoAmI == Main.myPlayer)
            {
                if (Player.HasBuff(ModContent.BuffType<InstaKillCurse>()))
                {
                    Player.KillMe(info.DamageSource, info.Damage, info.HitDirection, info.PvP);
                }
            }
        }
    }
}