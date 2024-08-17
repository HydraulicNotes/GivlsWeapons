using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using GivlsWeapons.Helpers;
using System;
using Terraria.Audio;
using System.Collections.Generic;
using System.Linq;
using GivlsWeapons.Core.Systems;

namespace GivlsWeapons.Content.Items.Weapons
{
    public class TeslaDaggers : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 58;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;

            Item.DamageType = DamageClass.Magic;
            Item.damage = 25;
            Item.mana = 20;
            Item.knockBack = 5f;
            Item.shootSpeed = 15f;
            Item.shoot = ModContent.ProjectileType<TeslaDagger>();
            Item.useTime = 60;
            Item.useAnimation = 60;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Pink;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.NimbusRod)
                .AddIngredient(ItemID.MagicDagger)
                .AddIngredient(ItemID.HallowedBar, 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[Item.shoot] >= 2)
            {
                foreach (var p in Main.projectile.Take(Main.maxProjectiles).Where(x => x.active && x.owner == player.whoAmI))
                {
                    p.Kill();
                }
            }
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai2: -1f);
            return false;
        }
    }

    public class TeslaDagger : ModProjectile
    {
        private ref float DaggerType => ref Projectile.ai[0]; //Tracks whether this is the first or second dagger. * tracks whether this dagger is connected or not, and what dagger it's connected to
        private ref float AirTimer => ref Projectile.ai[1]; //Tracks time in the air, and whether the dagger is stuck in an enemy or tile
        private ref float StuckInNPC => ref Projectile.ai[2]; //Tracks which NPC this is stuck in
        private ref float InitialProjRot => ref Projectile.localAI[0]; //The target's last rotation
        private ref float InitialNPCRot => ref Projectile.localAI[1]; //The relative position of the dagger when it hits
        private Vector2 InitialOffset;
        private ref float OtherDagger => ref Projectile.localAI[2];
        public override void SetDefaults()
        {
            Projectile.Size = Vector2.One * 36;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 6000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false; //Projectile uses custom collision
            //Projectile.extraUpdates = 1;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }
        public override void OnSpawn(IEntitySource source)
        {
            DaggerType = -1f;
            OtherDagger = -1f;
            foreach (var p in Main.projectile.Take(Main.maxProjectiles).Where(x => x.active && x.owner == Projectile.owner && x.type == ModContent.ProjectileType<TeslaDagger>() && x.ai[0] == -1f && x.whoAmI != Projectile.whoAmI && x.DistanceSQ(Projectile.position) < 2560000))
            {
                DaggerType = p.identity;
                OtherDagger = p.whoAmI;
                p.ai[0] = Projectile.identity;
                Projectile.netUpdate = true;
                p.netUpdate = true;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, Vector2.Zero, ModContent.ProjectileType<TeslaArc>(), Projectile.damage, 0f, Projectile.owner, Projectile.identity, p.identity);
                break;
            }
        }
        public override void AI()
        {
            //Electric Arc
            if (OtherDagger >= 0 && !Main.projectile[(int)OtherDagger].active)
                OtherDagger = -1;

            //Movement
            if (AirTimer == -2f)
            {
                NPC target = Main.npc[(int)StuckInNPC];

                if (target.active)
                {
                    Projectile.velocity = Vector2.Zero;
                    if (Projectile.numUpdates == -1)//Only run on the first update, otherwise it will move more than the NPC does
                    {
                        /* Projectile.position += target.position - target.oldPosition;
                        Projectile.Center = Helper.RotateAroundPoint(Projectile.Center, target.Center, target.rotation - TargetOldRotation);
                        Projectile.rotation += target.rotation - TargetOldRotation;
                        TargetOldRotation = target.rotation; */

                        /* Projectile.Center = target.Center + RelativePosition.RotatedBy(InitialRotation - target.rotation);
                        Projectile.rotation = InitialRotation + target.rotation; */
                        
                        Projectile.Center = target.Center + InitialOffset.RotatedBy(target.rotation - InitialNPCRot);
                        Projectile.rotation = target.rotation + (InitialProjRot - InitialNPCRot);
                    }
                }
                else AirTimer = 35f;
            }
            else
            {
                Vector2 collisionOffset = (Projectile.rotation + MathHelper.ToRadians(135f)).ToRotationVector2() * Projectile.height / 2;
                if (!Collision.IsWorldPointSolid(Projectile.Center, true))
                { //Runs if there is no collision
                    if (AirTimer == -1f) //if in ground state, switch to air state
                    {
                        AirTimer = 35f;
                    }
                    else
                    {
                        AirTimer += 1f;
                        if (AirTimer <= 75f)
                            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
                        else
                        {
                            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * Math.Sign(Projectile.velocity.X);
                        }
                        if (AirTimer >= 35f) // Gravity sets in after flying for 35 ticks, or falling from a block or enemy
                        {
                            if (Projectile.velocity.Y <= 15f)
                                Projectile.velocity.Y += 0.6f;
                        }
                    }
                }
                else
                { //Runs if there is a collision. OnTileCollide is not used, as default collision would push the projectile out of slopes
                    Projectile.velocity = Vector2.Zero;
                    if (AirTimer > -1f)
                    {
                        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                        AirTimer = -1f; // -1 indicates that the projectile is in a tile.
                    }
                }
            }

            //Dust spawning
            if (Main.rand.NextBool(3))
            {
                Vector2 dustOffset = (Projectile.rotation + MathHelper.ToRadians(135f)).ToRotationVector2() * Projectile.height / 2;
                if (DaggerType == -1f && Main.rand.NextBool(2))
                    Dust.NewDust(Projectile.Center + dustOffset, 0, 0, DustID.Electric, 0, 0, Scale: 0.6f);
                else if (DaggerType != -1f)
                    Dust.NewDust(Projectile.Center + dustOffset, 0, 0, DustID.Electric, 0, 0, Scale: Main.rand.NextFloat(0.6f, 1.1f));
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Electric, 0, 0);
            }
        }
        public override bool? CanDamage()
        {
            return AirTimer >= 0f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            AirTimer = -2f; // -2 indicates that the projectile is in an NPC
            StuckInNPC = target.whoAmI;
            InitialOffset = Projectile.Center - target.Center;
            InitialNPCRot = target.rotation;
            InitialProjRot = Projectile.rotation;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) //Extra-precise collision is used since normal collision would cause it to appear to float when stuck in enemies
        {
            Vector2 offset = (Projectile.rotation + MathHelper.ToRadians(135f)).ToRotationVector2() * Projectile.height / 2;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center - offset);
        }
        /* public override bool PreDraw(ref Color lightColor) // Use this to see the collision line
        {
            //Vector2 offset = (Projectile.rotation + MathHelper.ToRadians(135f)).ToRotationVector2() * Projectile.height / 2;
            NPC target = Main.npc[(int)StuckInNPC];
            DebugUtils.AABBLineVisualizer(target.Center, target.Center + (InitialRotation + target.rotation + MathHelper.ToRadians(135f)).ToRotationVector2() * InitialDistance, 10);
            return true;
        } */
    }




    public class TeslaArc : ModProjectile
    {
        public override string Texture => "GivlsWeapons/Assets/Textures/LightningBColor";
        private ref float FirstDaggerID => ref Projectile.ai[0];
        private ref float SecondDaggerID => ref Projectile.ai[1];
        private ref float Distance => ref Projectile.ai[2];
        private Projectile FirstDagger;
        private Projectile SecondDagger;
        private Vector2 _start;
        private Vector2 _end;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 960;
        }
        public override void SetDefaults()
        {
            Projectile.Size = Vector2.One * 48;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 6000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            Projectile.usesIDStaticNPCImmunity = true; //Could change this to local depending on what makes it the most fun to use. Keep in mind stats should be balanced around the function, not the other way around.
            Projectile.idStaticNPCHitCooldown = 10;
        }
        public override void AI()
        {
            if (Projectile.timeLeft == 6000)
            {
                Projectile.netUpdate = true;
            }
            if (FirstDagger == null || SecondDagger == null) // Finds the whoAmIs of the daggers so their values can be used
            {
                Projectile target;
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    target = Main.projectile[i];
                    if (FirstDagger is null && target.identity == (int)FirstDaggerID)
                    {
                        FirstDagger = Main.projectile[i];
                        target.timeLeft = 6000;
                        continue;
                    }
                    else if (SecondDagger is null && target.identity == (int)SecondDaggerID)
                    {
                        target.timeLeft = 6000;
                        SecondDagger = Main.projectile[i];
                    }
                }
            }

            if (FirstDagger.active && SecondDagger.active) //Both daggers must be alive
            {
                Distance = FirstDagger.Distance(SecondDagger.position);
                _start = FirstDagger.Center + (FirstDagger.rotation + MathHelper.ToRadians(135f)).ToRotationVector2() * FirstDagger.height / 2;
                _end = SecondDagger.Center + (SecondDagger.rotation + MathHelper.ToRadians(135f)).ToRotationVector2() * SecondDagger.height / 2;
                Projectile.Center = (_start + _end) / 2;
            }
            else Projectile.Kill();

            SoundEngine.PlaySound(new SoundStyle("GivlsWeapons/Assets/Sounds/TeslaArc") with
            {
                Volume = 0.24f,
                Pitch = 1f,
                PitchVariance = 1f,
                MaxInstances = 1,
                SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
            }, Projectile.Center);
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool? CanDamage()
        {
            return Distance > 1600f ? false : null; //Don't do damage if out of range
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Rectangle enemyHitboxInflated = targetHitbox;
            enemyHitboxInflated.Inflate(30, 30);
            return Collision.CheckAABBvLineCollision(enemyHitboxInflated.TopLeft(), enemyHitboxInflated.Size(), _start, _end)
            || FirstDagger.Hitbox.Intersects(targetHitbox) || SecondDagger.Hitbox.Intersects(targetHitbox);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Distance > 1600f) return false; //Don't draw anything if the daggers are out of range

            Texture2D texture = ModContent.Request<Texture2D>("GivlsWeapons/Assets/Textures/LightningAColor").Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("GivlsWeapons/Assets/Textures/LightningA").Value;
            Color col = Color.SkyBlue;
            col.A = 0;
            col = col * (0.8f + (MathF.Sin(Projectile.timeLeft * 0.24f) * 0.2f)); //Oscillates opacity. The first number is the baseline opacity, the second is the frequency, and the third is the intensity of oscillation
            if (Distance > 960f)
            { //Scales with distance from 60 to 100 tiles.
                col = col * (1 - ((Distance - 960) / 640));
            }
            if (Projectile.timeLeft <= 60)
            {
                col = col * Easings.OutCubic((float)Projectile.timeLeft / 60f);
            }
            DrawLightning(texture, texture2, col, _start, _end, 0.032f, 60, 0.8f);
            return false;
        }

        private static void DrawLightning(Texture2D texture, Texture2D texture2, Color col, Vector2 source, Vector2 dest, float scale, float sway = 80f,
            float jaggednessNumerator = 1f)
        {
            List<Vector2> points = LightningSystem.CreateBolt(source, dest, sway, jaggednessNumerator);

            for (int i = 1; i < points.Count; i++)
            {
                Vector2 start = points[i - 1];
                Vector2 end = points[i];
                float numPoints = (end - start).Length() * 0.4f;

                for (int j = 0; j < numPoints; j++)
                {
                    float lerp = j / numPoints;
                    float rotation = Main.rand.NextFloat(MathF.Tau);
                    Vector2 drawPos = Vector2.Lerp(start, end, lerp);
                    Lighting.AddLight(drawPos, Color.SkyBlue.ToVector3());
                    Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, null, col, rotation, texture.Size() / 2, scale,
                        SpriteEffects.None);
                    Main.EntitySpriteDraw(texture2, drawPos - Main.screenPosition, null, new Color(255, 255, 255, 0), rotation,
                        texture2.Size() / 2, scale * 0.3f, SpriteEffects.None); // white center
                }
            }
        }
    }
}