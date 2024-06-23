using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using GivlsWeapons.Effects;
using System;
using GivlsWeapons.Core.Systems.PixelationSystem;
using GivlsWeapons.Helpers;
using System.Linq;

namespace GivlsWeapons.Content.Items.Weapons
{
    public class BreakerBullet : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 7));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;

            Item.value = Item.sellPrice(copper: 10);
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.ammo = AmmoID.Bullet;
            Item.rare = ItemRarityID.Cyan;

            Item.damage = 15;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 3f;
            Item.shootSpeed = 16f;
            Item.shoot = ModContent.ProjectileType<BreakerBulletProjectile>();
        }
        public override void AddRecipes()
        {
            CreateRecipe(200)
                .AddIngredient(ItemID.EmptyBullet, 200)
                .AddIngredient(ItemID.LivingDemonFireBlock, 3)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    public class BreakerBulletProjectile : ModProjectile
    {
        private ref float Power => ref Projectile.ai[2];
        private ref float Timer => ref Projectile.localAI[0];
        public override void SetDefaults()
        {
            /* Projectile.width = 12;
            Projectile.height = 12;
            Projectile.alpha = 255; //needed for fade-in to work

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 600;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            */
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Bullet;
            Projectile.CloneDefaults(ProjectileID.VenomBullet);
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Main.player[Projectile.owner].GetModPlayer<BreakerBulletManager>().BreakerBulletsSpawned++;
        }
        public override void AI()
        {
            Player myOwner = Main.player[Projectile.owner];
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                Projectile target = Main.projectile[i];
                //if target has damage and is active; is hostile and not friendly, or friendly and not owned by the same player; isn't too big; and collides with this one, with some extra space
                if (target.damage > 0 && target.active && ((target.hostile && !target.friendly) || (target.friendly && target.owner != Projectile.owner)) && target.Size.LengthSquared() < 100000 && target.Colliding(target.Hitbox, Projectile.Hitbox.Modified(-8, -8, 16, 16)))
                {
                    if (Main.netMode != NetmodeID.SinglePlayer && target.owner != 255) //if in multiplayer and projectile is owned by another player
                    {
                        Player targetOwner = Main.player[target.owner];
                        if (!targetOwner.InOpposingTeam(myOwner) || target.minion || target.sentry) //don't destroy minions or sentries, or projectiles owned by friendly players
                        {
                            continue;
                        }
                    }
                    float projSize = Power * Projectile.scale; //Arbitrary value to determine effectiveness of degeneration
                    float targSize = target.width * target.height * (ProjectileID.Sets.DrawScreenCheckFluff[target.type] / 480f); //Hitbox size and scale determine effectiveness, along with the screen check distance to account for projectiles whose effects extend beyond their hitbox.

                    if (projSize <= 0f) //Kill checks are done early here to prevent division by 0 or negative scales
                    {
                        Projectile.Kill();
                        break;
                    }
                    if (targSize <= 0f)
                    {
                        target.Kill();
                        continue;
                    }

                    if (Projectile.penetrate > 0) //Decrements penetrate when degenerating projectiles, like how meteor bullets decrement penetrate when bouncing
                    {
                        Projectile.penetrate--;
                    }
                    else //The projectile will shrink if it has already hit a target or degenerated a projectile
                    {
                        //Projectile.scale *= (projSize - targSize) / projSize;
                        Helper.WeakenProjectile(Projectile, targSize, projSize, reduceDamage: false);
                    }
                    //target.scale *= (targSize - projSize) / targSize;
                    //target.damage = (int)(target.damage * ((targSize - projSize) / targSize));
                    Helper.WeakenProjectile(target, projSize, targSize);

                    //projSize = Projectile.width * Projectile.height * Projectile.scale;
                    //targSize = target.width * target.height * target.scale;

                    for (int j = 0; j < 12; j++)
                    {
                        Dust.NewDustPerfect(Projectile.Center + (MathF.Tau / 12 * (float)j).ToRotationVector2(), DustID.ShadowbeamStaff);
                    }
                }
            }
            Timer++;
        }
        public override void OnKill(int timeLeft)
        {
            for (int j = 0; j < 12; j++)
            {
                Dust.NewDustPerfect(Projectile.Center + (MathF.Tau / 12 * j).ToRotationVector2(), DustID.ShadowbeamStaff);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int j = 0; j < 12; j++)
            {
                Dust.NewDustPerfect(Projectile.Center + (MathF.Tau / 12 * (float)j).ToRotationVector2(), DustID.ShadowbeamStaff);
            }
        }
        private static VertexStrip vertexStrip = new VertexStrip();
        private float transitToDark;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
            {
                transitToDark = Utils.GetLerpValue(0f, 6f, Timer, clamped: true);
                MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
                miscShaderData.UseSaturation(-2f);
                miscShaderData.UseOpacity(MathHelper.Lerp(4f, 8f, transitToDark));
                miscShaderData.Apply();
                vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f, includeBacksides: true);
                vertexStrip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            });
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3());
            return false;
        }
        private Color StripColors(float progressOnStrip)
        {
            float lerpValue = Utils.GetLerpValue(0f - 0.1f * transitToDark, 0.7f - 0.2f * transitToDark, progressOnStrip, clamped: true);
            Color result = Color.Lerp(Color.Lerp(Color.Purple, Color.Black, transitToDark * 0.5f), Color.Purple, lerpValue) * (1f - Utils.GetLerpValue(0.98f, 0f, progressOnStrip));
            return result;
        }

        private float StripWidth(float progressOnStrip)
        {
            float lerpValue = Utils.GetLerpValue(0f, 0.06f + transitToDark * 0.01f, progressOnStrip, clamped: true);
            lerpValue = 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(12f + transitToDark * 16f, 8f, Utils.GetLerpValue(0f, 1f, progressOnStrip, clamped: true)) * lerpValue;
        }
    }
    /// <summary>
    /// Manages the power multiplier of all breaker bullets spawned by this player, to account for item use speed and guns spawning more than one projectile per tick
    /// </summary>
    public class BreakerBulletManager : ModPlayer
    {
        public int BreakerBulletsSpawned = 0;
/*         public override void PreUpdate()
        {
            BreakerBulletsSpawned = 0;
        } */
        public override void PostUpdate()
        {
            foreach (var p in Main.projectile.Take(Main.maxProjectiles).Where(x => x.active && x.owner == Player.whoAmI
                    && x.type == ModContent.ProjectileType<BreakerBulletProjectile>() && x.ai[2] == 0f))
            {
                p.ai[2] = ContentSamples.ItemsByType[Player.HeldItem.type].useAnimation * 50f / BreakerBulletsSpawned;
                if(Player.HeldItem.type == ItemID.VortexBeater) p.ai[2] /= 4;
            }
            BreakerBulletsSpawned = 0;
        }
    }
}
