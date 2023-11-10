using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using GivlsWeapons.Graphics;
using Terraria.Graphics.Shaders;

namespace GivlsWeapons.Content.Projectiles.Weapons
{
    internal class RisingFlameArrowProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.Size = Vector2.One * 10;

            Projectile.extraUpdates = 1;
            Projectile.arrow = true;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 1200;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                Dust.NewDustPerfect(Main.rand.NextVector2FromRectangle(Projectile.Hitbox), DustID.RedTorch, Projectile.velocity * 0.5f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, new Vector2(0, -10), ModContent.ProjectileType<RisingFlameProjectile>(), (int)(Projectile.damage * 0.6f), 0, Projectile.owner);
        }
    }

    internal class RisingFlameProjectile : ModProjectile
    {

        private ref float Timer => ref Projectile.localAI[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 36;

            Projectile.extraUpdates = 1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash with { Volume = 1.5f }, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustPos = Main.rand.NextVector2FromRectangle(Projectile.Hitbox);
                Dust particle = Dust.NewDustPerfect(dustPos, DustID.RedTorch, dustPos.AngleFrom(Projectile.Center).ToRotationVector2() * 2f, Scale: 1.7f);
                particle.noGravity = true;
            }
            Projectile.rotation = 0;
        }

        public override void AI()
        {
            if (Timer <= 6)
            {
                Projectile.Opacity = Timer * 0.16f;
                Timer++;
            }

            if (Main.rand.NextBool(2, 5))
            {
                Dust.NewDustPerfect(Main.rand.NextVector2FromRectangle(Projectile.Hitbox), DustID.RedTorch, Projectile.velocity * 0.5f, Scale: Main.rand.NextBool(7) ? 1.3f : 1);
            }
            Lighting.AddLight(Projectile.position, Color.LightPink.ToVector3());
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);
        }
        private static VertexStrip vertexStrip = new VertexStrip();
        private float transitToDark;
        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            transitToDark = Utils.GetLerpValue(0f, 6f, Timer, clamped: true);
            MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
            miscShaderData.UseSaturation(-2f);
            miscShaderData.UseOpacity(MathHelper.Lerp(4f, 8f, transitToDark));
            miscShaderData.Apply();
            vertexStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f, includeBacksides: true);
            vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            return true;
        }
        private Color StripColors(float progressOnStrip)
        {
            float lerpValue = Utils.GetLerpValue(0f - 0.1f * transitToDark, 0.7f - 0.2f * transitToDark, progressOnStrip, clamped: true);
            Color result = Color.Lerp(Color.Lerp(Color.White, Color.Orange, transitToDark * 0.5f), Color.Red, lerpValue) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
            result.A /= 8;
            return result;
        }
        private float StripWidth(float progressOnStrip)
        {
            float lerpValue = Utils.GetLerpValue(0f, 0.06f + transitToDark * 0.01f, progressOnStrip, clamped: true);
            lerpValue = 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(12f + transitToDark * 6f, 8f, Utils.GetLerpValue(0f, 1f, progressOnStrip, clamped: true)) * lerpValue;
        }
    }
}