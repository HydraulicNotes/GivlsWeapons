using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using GivlsWeapons.Content.Dusts;
using Terraria.ID;

namespace GivlsWeapons.Content.Projectiles.Weapons
{
    internal class Pentacle : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];
        private ref float HealingEnergy => ref Projectile.ai[1];
        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 52;

            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.aiStyle = -1;
            Projectile.penetrate = 8;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Timer++;
            Player owner = Main.player[Projectile.owner];
            if (Timer < 120)
            {
                Projectile.velocity *= 1.03f;
            }

            if (Projectile.Colliding(Projectile.Hitbox, owner.Hitbox) && HealingEnergy > 0)
            {
                int healAmount = (int)HealingEnergy * 2;
                owner.statLife += healAmount;
                owner.HealEffect(healAmount, true);
                HealingEnergy = 0;
            }

            float rotateSpeed = 0.35f * Projectile.direction;
            Projectile.rotation += rotateSpeed;

            Lighting.AddLight(Projectile.Center, 0.75f, 0.0f, 0.0f);

            if (Main.rand.NextBool(2))
            {
                int numToSpawn = Main.rand.Next(3);
                for (int i = 0; i < numToSpawn; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<BoringDust>(), Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f,
                        0, default, 1f);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.velocity *= -1f;
            Projectile.timeLeft += 10;
            if(target.type != NPCID.TargetDummy)
            {
                HealingEnergy++;
            }
            Projectile.netUpdate = true;
        }
    }
}
