using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using GivlsWeapons.Content.Dusts;
using GivlsWeapons.Content.Buffs;

namespace GivlsWeapons.Content.Projectiles.Weapons
{
    internal class BlackBullet : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255; //fade in
            Projectile.light = 0.5f;
            Projectile.extraUpdates = 1;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;

            AIType = ProjectileID.Bullet; // Act exactly like default Bullet
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.HasBuff<BlackGlow>())
            {
                modifiers.SourceDamage *= 3;
                modifiers.Knockback *= 3;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.FindBuffIndex(ModContent.BuffType<WhiteGlow>()) != -1)
            {
                target.DelBuff(target.FindBuffIndex(ModContent.BuffType<WhiteGlow>()));
            }
            target.AddBuff(ModContent.BuffType<BlackGlow>(), 900, false);
        }
    }
}
