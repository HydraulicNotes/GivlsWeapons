using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;

namespace GivlsWeapons.Content.Buffs
{
    internal class InstaKillCurse : ModBuff
    { // Functionality is handled by the ModPlayer because it has an OnHurt hook
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
    { //For some reason the player doesn't always appear to die right away on other clients. I don't know how to fix this yet, so I'll just be leaving it in.
    //If I do find a fix, I may allow the bug to happen during halloween because it fits thematically and doesn't affect gameplay too much
        public override void OnHurt(Player.HurtInfo info) //If the player is hurt but not killed, check for the buff and kill them anyway if they have it
        {
            if (Player.HasBuff<InstaKillCurse>() && Player.whoAmI == Main.myPlayer)
            {
                Player.KillMe(info.DamageSource, info.Damage, info.HitDirection, info.PvP);
            }
        }
    }
}