using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;

namespace GivlsWeapons.Content.Projectiles.Accessories
{
    internal class AltLeafCrystal : ModProjectile
    {
        const int COOLDOWN = 40; //Same as regular leaf crystal's cooldown for passive targeting
        private ref float ShotTimer => ref Projectile.ai[0];
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 42;

            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180; //timeLeft and DURATION determine the amount of shots that can be fired over its lifetime.
            //Currently, this is 4 shots plus 20 ticks. Since it  doesn't always have a target, it should have some extra time
        }

        public override void AI()
        {
            Projectile.velocity *= 0.9f; //deceleration
            if (ShotTimer == 0)
            {
                NPC target = null;
                float finalDistance = 700f; //Maximum distance of 700 pixels or 43.75 tiles, same as the real leaf crystal's passive targeting
                for (int i = 0; i < Main.npc.Length; i++)
                {
                    NPC potentialTarget = Main.npc[i];
                    float targDistance = potentialTarget.Center.Distance(Projectile.Center);
                    if (potentialTarget.CanBeChasedBy(this) && targDistance <= finalDistance && Collision.CanHitLine(Projectile.Center, 0, 0, potentialTarget.Center, 0, 0))
                    {
                        finalDistance = targDistance;
                        target = potentialTarget;
                    }
                    if (target != null)
                    {
                        Player owner = Main.player[Projectile.owner];
                        ShotTimer = COOLDOWN;
                        Vector2 velocityVector = target.Center - Projectile.Center;
                        //Unsure what the purpose of this step is, but it seemed to be important when I decompiled the game. It's the same for both types of targeting.
                        Utils.ChaseResults chaseResults = Utils.GetChaseResults(Projectile.Center, 2160f, target.Center, target.velocity);//chase target with speed of 2160
                        if (chaseResults.InterceptionHappens && chaseResults.InterceptionTime <= 180f) //if intercepted quickly enough
                        {
                            velocityVector = chaseResults.ChaserVelocity / 180f; //set velocity to chaser velocity divided by 180, which is 12
                        }
                        Projectile.NewProjectile(Projectile.InheritSource(Projectile), new Vector2(Projectile.Center.X - 4f, Projectile.Center.Y), velocityVector, ProjectileID.CrystalLeafShot, (int)owner.GetDamage(DamageClass.Melee).ApplyTo(80f), owner.GetKnockback(DamageClass.Melee).ApplyTo(8f), Projectile.owner);
                        break;
                    }
                }
            }
            else
            {
                ShotTimer--;
            }
            //Make the projectile bob slightly up and down. Not done in velocity because of the deceleration effect. There's probably a way to make it work through velocity, but it's not worth my time. Bobbing is meant to be visual anyway, so it may be better not to.
            Projectile.position += new Vector2(0, MathF.Sin(Projectile.timeLeft * 0.03f) * 0.15f);

            if (Projectile.timeLeft > COOLDOWN) //Fade out slowly, and then become fully opaque after shooting
            {
                Projectile.alpha = COOLDOWN - (int)ShotTimer;
            }
            else //Fade out near the end of the lifetime. Checks if alpha is already higher
            {
                int newAlpha = 255 - (int)(255 * (Projectile.timeLeft * 0.02f));
                if(newAlpha > Projectile.alpha) Projectile.alpha = newAlpha;
            }
        }
    }
}