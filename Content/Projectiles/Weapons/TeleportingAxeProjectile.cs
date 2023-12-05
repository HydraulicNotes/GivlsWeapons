using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.GameContent;
using GivlsWeapons.Content.Items.Weapons;
using System.Net;
using GivlsWeapons.Development;
using System.Text;

namespace GivlsWeapons.Content.Projectiles.Weapons
{
    public class TeleportingAxeProjectile : ModProjectile
    {
        private ref float AxeType => ref Projectile.ai[0]; //Keeps track of whether this is the blue or pink axe. 0 for blue, 1 for pink.
        private ref float Timer => ref Projectile.ai[1];
        public override void SetDefaults()
        {
            //Projectile.Size = new Vector2(30);
            Projectile.width = 68;
            Projectile.height = 60;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 30000;
            Projectile.penetrate = -1;

            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false; //We use custom collision for this projectile, so we stop it from using default collision

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;
            Projectile.rotation = 0;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.scale = 1.2f * owner.GetAdjustedItemScale(owner.HeldItem); //this must be kept the same as the swing projectile to keep the size consistent
            //create a line through the blade of the axe for custom collision
            Vector2 startPoint = Projectile.Center + (Projectile.rotation - MathHelper.ToRadians(90f)).ToRotationVector2() * Projectile.Size.Length() * Projectile.scale * 0.16f;
            Vector2 endPoint = Projectile.Center + Projectile.rotation.ToRotationVector2() * Projectile.Size.Length() * Projectile.scale * 0.32f * Projectile.spriteDirection;
            if (Collision.CanHitLine(startPoint, 0, 0, endPoint, 0, 0)) //checks the line between the points, and returns whether it collides with tiles
            { //Runs if there is no collision
                if (Timer == -1)
                {
                    Timer = 30;
                }
                else
                {
                    Timer += 1f;
                    if (Timer >= 30f) //Gravity sets in after 30 updates (1/4 second), or if it falls after sticking in a block
                    {
                        Projectile.velocity.Y += 0.1f;
                    }
                }
                Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.02f * Projectile.spriteDirection;

                if (Main.rand.NextBool(4)) //spawns a dust half the time
                {
                    Color glowColor = AxeType == 1 ? new Color(255, 34, 240) : new Color(34, 176, 255);
                    Vector2 velocity = (Projectile.rotation + MathHelper.ToRadians(45f)).ToRotationVector2() * 1.4f * Projectile.spriteDirection;
                    Dust newDust = Dust.NewDustPerfect(Vector2.Lerp(startPoint, endPoint, Main.rand.NextFloat()), DustID.FireworksRGB, velocity, newColor: glowColor);
                    newDust.noGravity = true;
                }
            }
            else
            { //Runs if there is a collision. OnTileCollide is not used, as default collision would push the projectile out of slopes
                Projectile.velocity = Vector2.Zero;
                Projectile.noEnchantmentVisuals = true;
                if (Timer > -1) //to prevent the sound from looping
                {
                    SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                    Timer = -1; // -1 indicates that the projectile is in a tile.
                }
            }
            //SetVisualOffsets();
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((sbyte)Projectile.spriteDirection);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.spriteDirection = reader.ReadSByte();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, texture.Frame(2, 2, 0, (int)AxeType), lightColor * Projectile.Opacity, Projectile.rotation, Projectile.Size * 0.5f, Projectile.scale, effects, 0);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, texture.Frame(2, 2, 1, (int)AxeType), new Color(255, 255, 255, 0) * Projectile.Opacity, Projectile.rotation, Projectile.Size * 0.5f, Projectile.scale, effects, 0);

            //Debug code for showing custom collision line
            /* Vector2 startPoint = Projectile.Center + (Projectile.rotation - MathHelper.ToRadians(90f)).ToRotationVector2() * Projectile.Size.Length() * Projectile.scale * 0.16f;
            Vector2 endPoint = Projectile.Center + Projectile.rotation.ToRotationVector2() * Projectile.Size.Length() * Projectile.scale * 0.32f * Projectile.spriteDirection;
            startPoint = new Vector2((int)startPoint.X, (int)startPoint.Y);
            endPoint = new Vector2((int)endPoint.X, (int)endPoint.Y);
            DebugUtils.AABBLineVisualizer(startPoint, endPoint, 5); */
            return false;
        }

        public override bool? CanDamage()
        {
            return Projectile.velocity.Length() > 0f ? null : false; //if the axe is moving really slow (should only be when stuck in a tile), don't hit. Otherwise, follow default rules.
        }
    }

    public class TeleportingAxeSwingProjectile : ModProjectile
    {
        // We define some constants that determine the swing range of the sword
        // Not that we use multipliers here since that simplifies the amount of tweaks for these interactions
        // You could change the values or even replace them entirely, but they are tweaked with looks in mind
        private const float SWINGRANGE = 1.3f * MathF.PI; // The angle an attack covers
        private const float FIRSTHALFSWING = 0.55f; // How much of the swing happens before it reaches the target angle (in relation to swingRange)
        private const float WINDUP = 0.15f; // How far back the player's hand goes when winding their attack (in relation to swingRange)
        private const float UNWIND = 0.4f; // When should the sword start disappearing
        //public float RuneGlow;

        private enum AttackType // Which attack is being performed
        {
            // Swings are normal sword swings that can be slightly aimed
            // Swings goes through the full cycle of animations
            Swing,
            // Backswing is the same, except that it goes in the opposite direction
            BackSwing,
            // The pink swings are used for the pink axe's combo. They behave the same, except that the sprites are replaced with their pink versions
            SwingPink,
            BackSwingPink
        }

        private enum AttackStage // What stage of the attack is being executed, see functions found in AI for description
        {
            Prepare,
            Execute,
            Unwind
        }

        // These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
        private AttackType CurrentAttack
        {
            get => (AttackType)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private AttackStage CurrentStage
        {
            get => (AttackStage)Projectile.localAI[0];
            set
            {
                Projectile.localAI[0] = (float)value;
                Timer = 0; // reset the timer when the projectile switches states
            }
        }

        // Variables to keep track of during runtime
        private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
        private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
        private ref float Progress => ref Projectile.localAI[1]; // Position of sword relative to initial angle
        private ref float RuneGlow => ref Projectile.localAI[2]; // Whether the axe should glow. 0 for no, 1 if it hit something during the swing, and 2 if it was already active

        // We define timing functions for each stage, taking into account melee attack speed
        // Note that you can change this to suit the need of your projectile
        private float prepTime => 16f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private float execTime => 18f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private float hideTime => 16f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

        public override string Texture => "GivlsWeapons/Content/Projectiles/Weapons/TeleportingAxeProjectile"; // Use texture of item as projectile texture
        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 68; // Hitbox width of projectile
            Projectile.height = 60; // Hitbox height of projectile
            Projectile.friendly = true; // Projectile hits enemies
            Projectile.timeLeft = 10000; // Time it takes for projectile to expire
            Projectile.penetrate = -1; // Projectile pierces infinitely
            Projectile.tileCollide = false; // Projectile does not collide with tiles
            Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
            Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
            Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
            Projectile.DamageType = DamageClass.Melee; // Projectile is a melee projectile

            Projectile.extraUpdates = 1;

            Projectile.noEnchantmentVisuals = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
            if (CurrentAttack == AttackType.BackSwing || CurrentAttack == AttackType.BackSwingPink)
            {
                Projectile.spriteDirection *= -1;
            }
            float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

            if ((Projectile.spriteDirection == 1 && (CurrentAttack == AttackType.Swing || CurrentAttack == AttackType.SwingPink)) //If the spritedir is 1 and it's a swing
            || (Projectile.spriteDirection == -1 && (CurrentAttack == AttackType.BackSwing || CurrentAttack == AttackType.BackSwingPink))) // or if it's -1 and a backswing
            {
                // However, we limit the rangle of possible directions so it does not look too ridiculous
                targetAngle = MathHelper.Clamp(targetAngle, (float)-Math.PI * 1 / 3, (float)Math.PI * 1 / 6);
            }
            else
            {
                if (targetAngle < 0)
                {
                    targetAngle += 2 * (float)Math.PI; // This makes the range continuous for easier operations
                }

                targetAngle = MathHelper.Clamp(targetAngle, (float)Math.PI * 5 / 6, (float)Math.PI * 4 / 3);
            }

            InitialAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection; // Otherwise, we calculate the angle
            bool shouldGlow = (CurrentAttack == AttackType.Swing || CurrentAttack == AttackType.BackSwing) ? Owner.GetModPlayer<TeleportingAxePlayer>().blueAxeReady : Owner.GetModPlayer<TeleportingAxePlayer>().pinkAxeReady;
            RuneGlow = shouldGlow ? 1 : 0;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            // Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
            writer.Write((sbyte)Projectile.spriteDirection);
            writer.Write((sbyte)RuneGlow);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.spriteDirection = reader.ReadSByte();
            RuneGlow = reader.ReadSByte();
        }

        public override void AI()
        {
            // Extend use animation until projectile is killed
            Owner.itemAnimation = 2;
            Owner.itemTime = 2;

            // Kill the projectile if the player dies or gets crowd controlled
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            // AI depends on stage and attack
            // Note that these stages are to facilitate the scaling effect at the beginning and end
            // If this is not desireable for you, feel free to simplify
            switch (CurrentStage)
            {
                case AttackStage.Prepare:
                    PrepareStrike();
                    break;
                case AttackStage.Execute:
                    ExecuteStrike();
                    break;
                default:
                    UnwindStrike();
                    break;
            }

            SetSwordPosition();
            Timer++;

            for (float i = -MathHelper.PiOver4; i <= MathHelper.PiOver4; i += MathHelper.PiOver2) //adjust imbue dust positions
            {
                Rectangle rectangle = Utils.CenteredRectangle(Projectile.Center + (Projectile.rotation + i).ToRotationVector2() * 70f * Projectile.scale, new Vector2(60f * Projectile.scale, 60f * Projectile.scale));
                Projectile.EmitEnchantmentVisualsAt(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {

            // Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
            int axeType = (CurrentAttack == AttackType.SwingPink || CurrentAttack == AttackType.BackSwingPink) ? 1 : 0; //set to 1 to use the 2nd vertical frame, otherwise set to 0 to use the first
            Vector2 origin;
            float rotationOffset;
            SpriteEffects effects;

            if (Projectile.spriteDirection > 0)
            {
                origin = new Vector2(0, Projectile.height);
                rotationOffset = MathHelper.ToRadians(45f);
                effects = SpriteEffects.None;
            }
            else
            {
                origin = new Vector2(Projectile.width, Projectile.height);
                rotationOffset = MathHelper.ToRadians(135f);
                effects = SpriteEffects.FlipHorizontally;
            }

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float slashOffset = MathHelper.ToRadians(-55 * Projectile.spriteDirection);
            Main.instance.LoadProjectile(ProjectileID.TerraBlade2); //Loads the terra blade projectile so we can use its texture if it hasn't been loaded already
            Texture2D slashSprite = TextureAssets.Projectile[ProjectileID.TerraBlade2].Value;
            Rectangle slashSpriteRect = slashSprite.Frame(1, 4);
            Color dullColor = axeType == 1 ? new Color(69, 43, 61) : new Color(21, 32, 46);
            Color glowColor = axeType == 1 ? new Color(255, 34, 240) : new Color(34, 176, 255);

            Main.spriteBatch.Draw(slashSprite, Projectile.Center - Main.screenPosition, slashSpriteRect, dullColor * 0.2f * Projectile.Opacity, Projectile.rotation + rotationOffset + slashOffset, slashSpriteRect.Size() / 2f, Projectile.scale * 1.4f, effects, 0f);
            Main.spriteBatch.Draw(slashSprite, Projectile.Center - Main.screenPosition, slashSpriteRect, (RuneGlow > 0 ? glowColor * 0.6f : dullColor * 0.25f) * Projectile.Opacity, Projectile.rotation + rotationOffset + slashOffset, slashSpriteRect.Size() / 2f, Projectile.scale, effects, 0f);
            //34 176 255

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, texture.Frame(2, 2, 0, axeType), lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);
            if (RuneGlow > 0)
            {
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, texture.Frame(2, 2, 1, axeType), lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);
                Lighting.AddLight(Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale), glowColor.ToVector3());
                SpawnDust(glowColor);
            }
            // Since we are doing a custom draw, prevent it from normally drawing
            return false;
        }

        // Find the start and end of the sword and use a line collider to check for collision with enemies
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 1.3f);
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
        }

        // Do a similar collision check for tiles
        public override void CutTiles()
        {
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 1.3f);
            Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
        }

        // We make it so that the projectile can only do damage in its release and unwind phases
        public override bool? CanDamage()
        {
            if (CurrentStage == AttackStage.Prepare)
                return false;
            return null;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            RuneGlow = 1;
            Projectile.netUpdate = true;
        }
        //Thanks to the amazing power of Tmodloader, this hook doesn't run for PvP hits, making it useless
        /*         public override void OnHitPlayer(Player target, Player.HurtInfo info)
                {
                    Main.NewText("OnHitPlayer ran here");
                    RuneGlow = 1;
                } */
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Make knockback go away from player
            modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;
        }
        /*         public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) //This hook doesn't run on the target's client and is therefore useless
                {
                    modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;
                } */

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                if (RuneGlow == 1)
                {
                    Player player = Main.player[Projectile.owner];
                    if (CurrentAttack == AttackType.Swing || CurrentAttack == AttackType.BackSwing)
                    {
                        player.GetModPlayer<TeleportingAxePlayer>().blueAxeReady = true;
                    }
                    else
                    {
                        player.GetModPlayer<TeleportingAxePlayer>().pinkAxeReady = true;
                    }
                }
            }
        }

        // Function to easily set projectile and arm position
        public void SetSwordPosition()
        {
            Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress; // Set projectile rotation

            // Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
            Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand

            armPosition.Y += Owner.gfxOffY;
            Projectile.Center = armPosition; // Set projectile to arm position
            Projectile.scale = 1.2f * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers

            Owner.heldProj = Projectile.whoAmI; // set held projectile to this projectile
        }

        // Function facilitating the taking out of the sword
        private void PrepareStrike()
        {
            Progress = WINDUP * SWINGRANGE * (1f - Timer / prepTime); // Calculates rotation from initial angle
            Projectile.Opacity = Timer / prepTime; //fade in

            if (Timer >= prepTime)
            {
                //play sound here
                SoundEngine.PlaySound(SoundID.Item1);
                CurrentStage = AttackStage.Execute; // If attack is over prep time, we go to next stage
            }
        }

        // Function facilitating the first half of the swing
        private void ExecuteStrike()
        {
            Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execTime);

            if (Timer >= execTime)
            {
                CurrentStage = AttackStage.Unwind;
            }
        }

        // Function facilitating the latter half of the swing where the sword disappears
        private void UnwindStrike()
        {
            Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);
            Projectile.Opacity = 1f - (Timer / prepTime); //fade out
            if (Timer >= hideTime)
            {
                Projectile.Kill();
            }
        }

        private void SpawnDust(Color glowColor)
        {
            if (Main.rand.NextBool(3))
            {
                //The dust moves 90 degrees off from the projectile's rotation to create the illusion of moving with it
                Vector2 dustPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * Main.rand.NextFloat());
                Vector2 velocity = (Projectile.rotation + MathHelper.ToRadians(90f)).ToRotationVector2() * 1.4f * Projectile.spriteDirection;
                Dust newDust = Dust.NewDustPerfect(dustPos, DustID.FireworksRGB, velocity * 1.2f, newColor: glowColor);
                newDust.noGravity = true;
            }
        }
    }
}