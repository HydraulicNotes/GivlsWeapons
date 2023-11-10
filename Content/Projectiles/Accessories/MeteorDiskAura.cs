﻿using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using GivlsWeapons.Content.Dusts;
using GivlsWeapons.Content.Items.Accessories;
using Terraria.ID;

namespace GivlsWeapons.Content.Projectiles.Accessories
{
    internal class MeteorDiskAura : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;

            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 10;

            Projectile.DamageType = DamageClass.Magic;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 20;

            Projectile.alpha = 200;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            Projectile.Center = new Vector2(owner.Center.X, owner.Bottom.Y + 65);

            if (owner.GetModPlayer<MeteorDiskEquipped>().Equipped)
            {
                Projectile.timeLeft = 30;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            int manaToAdd = 1 + (int)(damageDone * 0.2f);
            owner.ManaEffect(manaToAdd);
            owner.statMana += manaToAdd;

            target.AddBuff(BuffID.OnFire, 60);
        }
    }
}