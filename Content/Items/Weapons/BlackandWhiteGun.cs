using System.Collections.Generic;
using GivlsWeapons.Helpers;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.GameContent.Creative;
using System.Linq;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using System;
using GivlsWeapons.Core.Systems.PixelationSystem;
using Terraria.Audio;

namespace GivlsWeapons.Content.Items.Weapons
{
    public class BlackandWhiteGun : ModItem
    {
        private int Combo = 0;
        const int ComboLength = 6;
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 15;
            Item.rare = ItemRarityID.LightRed;
            Item.value = 100000;

            Item.useTime = 18;
            Item.useAnimation = 18;
            //Item.reuseDelay = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = false;

            Item.damage = 45;
            Item.DamageType = DamageClass.Ranged;
            Item.crit = 4;
            Item.knockBack = 3.5f;
            Item.noMelee = true;
            Item.useAmmo = AmmoID.Bullet;

            Item.shootSpeed = 12f;
            Item.shoot = ProjectileID.PurificationPowder;

            Item.UseSound = SoundID.Item41;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Combo == ComboLength - 1)
            {
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(MathHelper.ToRadians(45f)) * 2.7f, ModContent.ProjectileType<YinYang>(), damage, knockback, player.whoAmI);
            }
            Combo = (Combo + 1) % ComboLength;
            return true;
        }

        public override void AddRecipes()
        {
            //CreateRecipe()
            //.AddIngredient(ModContent.ItemType<BalanceSoul>(), 2)
            //.AddIngredient(ItemID.TheUndertaker)
            //.AddTile(TileID.MythrilAnvil)
            //.Register();
            //CreateRecipe()
            //.AddIngredient(ModContent.ItemType<BalanceSoul>(), 2)
            //.AddIngredient(ItemID.Musket)
            //.AddTile(TileID.MythrilAnvil)
            //.Register();
            CreateRecipe()
                .AddIngredient(ItemID.DarkShard)
                .AddIngredient(ItemID.LightShard)
                .AddIngredient(ItemID.Musket)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            CreateRecipe()
                .AddIngredient(ItemID.DarkShard)
                .AddIngredient(ItemID.LightShard)
                .AddIngredient(ItemID.TheUndertaker)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class YinYang : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 3000;
            Projectile.alpha = 255; //fade in
        }
        public override void AI()
        {
            Projectile.rotation += MathHelper.ToRadians(2f);
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 6;
            }
            Projectile.velocity *= 0.9f;
            if (Projectile.velocity.LengthSquared() < 10 && Main.myPlayer == Projectile.owner)
            {
                Player owner = Main.player[Projectile.owner];
                foreach (var p in Main.projectile.Take(Main.maxProjectiles).Where(x => x.active && !owner.InOpposingTeam(Main.player[x.owner]) && x.aiStyle == ProjAIStyleID.Arrow && x.Colliding(x.Hitbox, Projectile.Hitbox)))
                {
                    /* for (int i = 0; i < 3; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2CircularEdge(2, 2), ModContent.ProjectileType<YinYangBullet>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: Projectile.identity, ai1: 0f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2CircularEdge(2, 2), ModContent.ProjectileType<YinYangBullet>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: Projectile.identity, ai1: 1f);
                    } */
                    int bulletCount = 0;
                    /*for (int i = 0; i < Main.projectile.Length; i++)
                    {
                        if (bulletCount >= 6 || bulletCount >= owner.ownedProjectileCounts[Type] - 1) break;
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.type == Type && proj.whoAmI != Projectile.whoAmI && !owner.InOpposingTeam(Main.player[proj.owner]))
                        {
                            SpawnBullet(bulletCount % 2 == 0, proj.Center);
                            bulletCount++;
                        }
                    }*/
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        if (bulletCount >= 4) break;
                        NPC target = Main.npc[i];
                        if (target.DistanceSQ(Projectile.position) <= 409600 && target.CanBeChasedBy(this)) //max distance is 40 tiles ((40 * 16)^2)
                        {
                            SpawnBullet(bulletCount % 2 == 0, target.Center);
                            bulletCount++;
                        }
                    }
                    while (bulletCount < 4)
                    {
                        SpawnBullet(bulletCount % 2 == 0, Projectile.Center + Main.rand.NextVector2CircularEdge(1, 1));
                        bulletCount++;
                    }
                    Projectile.Kill();
                    break;
                }
            }
            else Timer++;
        }
        public void SpawnBullet(bool isBlack, Vector2 targPos)
        {
            Vector2 velocity;
            if (targPos == Projectile.Center)
            {
                targPos = Projectile.Center + Main.rand.NextVector2CircularEdge(1, 1);
            }
            velocity = Vector2.Normalize(targPos - Projectile.Center) * 4;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(),
            Projectile.Center,
            velocity,
            ModContent.ProjectileType<YinYangBullet>(),
            Projectile.damage,
            Projectile.knockBack,
            Projectile.owner,
            ai0: isBlack ? 0f : 1f);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.7f; //multiply to make it slow on bouncing
            }

            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.7f;
            }

            return false;
        }
        public override bool? CanDamage()
        {
            return false;
        }
    }
    public class YinYangBullet : ModProjectile, IDrawPrimitive
    {
        private bool disabled = false;
        const int LENGTH = 100;
        private List<Vector2> cache;
        private Trail trail;

        private float trailWidth = 0.1f;
        public int BulletType
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public enum BulletColors
        {
            Black,
            White
        }
        private ref float HomingTimer => ref Projectile.ai[2];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 0;
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 3000;

            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 5;
            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            AIType = ProjectileID.Bullet;
        }
        public override void AI()
        {
            if (!disabled)
            {
                if (HomingTimer <= 540) HomingTimer++;
                float maxDetectRadius = 640f; // The maximum radius at which a projectile can detect a target
                float projSpeed = 4f; // The speed at which the projectile moves towards the target

                // Trying to find NPC closest to the projectile
                NPC closestNPC = FindClosestNPC(maxDetectRadius);
                if (closestNPC != null)
                {
                    float targetAngle = Projectile.Center.AngleTo(closestNPC.Center);
                    Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(1.2f * HomingTimer / 90f)).ToRotationVector2() * projSpeed;
                    Projectile.rotation = Projectile.velocity.ToRotation();
                }

                if(Projectile.timeLeft <= 600) 
                {
                    Helper.DisableProjectile(Projectile);
                    disabled = true;
                }
            }

            ManageCaches();
            ManageTrail();
        }
        public NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;

            // Using squared values in distance checks will let us skip square root calculations, drastically improving this method's speed.
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

            // Loop through all NPCs
            foreach (var target in Main.ActiveNPCs)
            {
                // Check if NPC able to be targeted. It means that NPC is
                // 1. active (alive)
                // 2. chaseable (e.g. not a cultist archer)
                // 3. max life bigger than 5 (e.g. not a critter)
                // 4. can take damage (e.g. moonlord core after all it's parts are downed)
                // 5. hostile (!friendly)
                // 6. not immortal (e.g. not a target dummy)
                if (target.CanBeChasedBy())
                {
                    // The DistanceSquared function returns a squared distance between 2 points, skipping relatively expensive square root calculations
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);

                    // Check if it is within the radius and the projectile has line of sight
                    if (sqrDistanceToTarget < sqrMaxDetectDistance && Collision.CanHitLine(Projectile.Center, 0, 0, target.Center, 0, 0))
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }

            return closestNPC;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(!disabled) Helper.DisableProjectile(Projectile);
            disabled = true;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if(!disabled) Helper.DisableProjectile(Projectile);
            disabled = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }

            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.7f;
            }

            return false;
        }
        public override bool PreKill(int timeLeft)
        {
            if (timeLeft <= 0) return true;
            else
            {
                Helper.DisableProjectile(Projectile);
                return false;
            }
        }
        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 50; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            if (Projectile.oldPos[0] != Vector2.Zero)
                cache.Add(Projectile.oldPos[0] + new Vector2(Projectile.width / 2, Projectile.height / 2));

            while (cache.Count > 50)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail ??= new Trail(Main.instance.GraphicsDevice, 50, new NoTip(), factor => (40 + factor * 100) * trailWidth, factor => new Color(240, 240, 240) * factor.Y);

            trail.Positions = cache.ToArray();

            if (Projectile.velocity.Length() > 1)
            {
                trail.NextPosition = Projectile.Center + Projectile.velocity;
            }
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["Compile/BlackAndWhiteTrail"].GetShader().Shader;
            var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.1f);
            effect.Parameters["repeats"].SetValue(5f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("GivlsWeapons/Assets/Textures/TrailTex").Value);
            effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("GivlsWeapons/Assets/Textures/noiseTexture3").Value);

            trail?.Render(effect);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
