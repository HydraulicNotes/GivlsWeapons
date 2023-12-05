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

namespace GivlsWeapons.Content.Projectiles.Weapons;
public class HFMuramasaProjectile : ModProjectile
{
    private const float SWINGRANGE = 1 * MathF.PI; // The angle a swing attack covers
    private const float FASTRANGE = 0.75f * MathF.PI; //The angle the fast swings cover
    private const float FIRSTHALFSWING = 0.6f; // How much of the swing happens before it reaches the target angle (in relation to swingRange)
    private const float FIRSTHALFSWINGLONG = 0.8f; //FIRSTHALFSWING but for the long swing
    private const float FIRSTHALFSWINGSHORT = 0.5f; //The same, but for the short backswing
    private const float SPINRANGE = 3f * MathF.PI; // The angle a spin attack covers
    private const float WINDUP = 0.15f; // How far back the player's hand goes when winding their attack (in relation to swingRange)
    private const float UNWIND = 0.4f; // When should the sword start disappearing
    private const float SPINTIME = 4.2f; // How much longer a spin is than a swing

    private enum AttackType // Which attack is being performed
    {
        // Swings are normal sword swings that can be slightly aimed
        // Swings goes through the full cycle of animations
        Swing,

        BackSwing, //This is the backward swing. It's the same as the swing, but goes in the opposite direction to reach the same point
        LongSwing, //Long swings are like swings, but they start from further back
        ShortBackSwing, //Starts from closer to the center than a backswing
        FastSwing1, //Faster and shorter version of a swing. Repeated twice
        FastBackSwing1, //Above, but as a backswing
        FastSwing2, //Repeated
        FastBackSwing2,
        Spin,// Spins are swings that go full circle. They are slower and deal more knockback
        Quickdraw //This is the quickdraw attack. This shouldn't be cycled to as it isn't part of the combo
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
    private ref float Opacity => ref Projectile.localAI[2]; // Size of sword

    // We define timing functions for each stage, taking into account melee attack speed
    // Note that you can change this to suit the need of your projectile

    //Projectile.extraUpdates has been set to 2, so there are 3 updates every tick.
    private float PrepTime => 14f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
    private float ExecTime => 9f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
    private float HideTime => 20f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

    public override string Texture => "GivlsWeapons/Content/Items/Weapons/HFMuramasa"; // Use texture of item as projectile texture
    public string Glowmask => "GivlsWeapons/Content/Projectiles/Weapons/HFMuramasaGlowmask";
    private Player Owner => Main.player[Projectile.owner];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 58; // Hitbox width of projectile
        Projectile.height = 72; // Hitbox height of projectile
        Projectile.friendly = true;
        Projectile.timeLeft = 10000; // Projectile manages this itself
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
        Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
        Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
        Projectile.DamageType = DamageClass.Melee;
        Projectile.extraUpdates = 2;
        Projectile.ArmorPenetration = 15;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
        if (CurrentAttack == AttackType.BackSwing || CurrentAttack == AttackType.ShortBackSwing || CurrentAttack == AttackType.FastBackSwing1 || CurrentAttack == AttackType.FastBackSwing2 || CurrentAttack == AttackType.Quickdraw)
        {
            Projectile.spriteDirection *= -1; //flip the spritedirection so the backswings and quickdraw go upward
        }

        float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

        if (CurrentAttack == AttackType.Spin || CurrentAttack == AttackType.Quickdraw)
        {
            InitialAngle = -MathF.PI / 2 - MathF.PI * 1 / 3 * Projectile.spriteDirection; // For the spin and QD, starting angle is designated based on direction
            if (CurrentAttack == AttackType.Quickdraw)
            {
                InitialAngle += MathHelper.ToRadians(180f); //start quickdraws from the bottom to be consistent with the hold sprite\
                Projectile.damage *= 5; //This is the only non-positioning line in OnSpawn. Looks weird, but this is the best place for it
            }
        }
        else
        {
            if ((Projectile.spriteDirection == 1 && (CurrentAttack == AttackType.Swing || CurrentAttack == AttackType.LongSwing || CurrentAttack == AttackType.FastSwing1 || CurrentAttack == AttackType.FastSwing2))
            || (Projectile.spriteDirection == -1 && (CurrentAttack == AttackType.BackSwing || CurrentAttack == AttackType.ShortBackSwing || CurrentAttack == AttackType.FastBackSwing1 || CurrentAttack == AttackType.FastBackSwing2)))
            //Really lengthy way of checking if spritedirection is 1/-1 and the attack is a swing/backswing. If there's a cleaner way, I'll be mad I didn't find it sooner.
            {
                // limit the rangle of possible directions so it does not look too ridiculous.
                targetAngle = MathHelper.Clamp(targetAngle, -MathF.PI * 1 / 3, MathF.PI * 1 / 6);
            }
            else
            {
                if (targetAngle < 0)
                {
                    targetAngle += 2 * MathF.PI; // This makes the range continuous for easier operations
                }

                targetAngle = MathHelper.Clamp(targetAngle, MathF.PI * 5 / 6, MathF.PI * 4 / 3);
            }
            if (CurrentAttack == AttackType.LongSwing)
            {
                InitialAngle = targetAngle - FIRSTHALFSWINGLONG * SWINGRANGE * Projectile.spriteDirection; //calculate the angle for the appropriate swing type
            }
            else if (CurrentAttack == AttackType.Swing || CurrentAttack == AttackType.BackSwing)
            {
                InitialAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection;
            }
            else //this includes the short backswing and all the fast swings
            {
                InitialAngle = targetAngle - FIRSTHALFSWINGSHORT * SWINGRANGE * Projectile.spriteDirection;
            }
        }
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        // Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
        writer.Write((sbyte)Projectile.spriteDirection);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Projectile.spriteDirection = reader.ReadSByte();
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
        Vector2 origin;
        float rotationOffset;
        SpriteEffects swordEffects;
        SpriteEffects slashSpriteEffects;
        //Set the rotation, and adjust size for whether or not it's a quickdraw
        float rotation = MathHelper.ToRadians(-55 * Projectile.spriteDirection);
        float size = Projectile.scale * (CurrentAttack == AttackType.Quickdraw ? 1.25f : 1f);

        if (Projectile.spriteDirection > 0)
        {
            origin = new Vector2(0, Projectile.height);
            rotationOffset = MathHelper.ToRadians(45f);
            swordEffects = SpriteEffects.None;
            slashSpriteEffects = SpriteEffects.None;
        }
        else
        {
            origin = new Vector2(Projectile.width, Projectile.height);
            rotationOffset = MathHelper.ToRadians(135f);
            swordEffects = SpriteEffects.FlipHorizontally;
            slashSpriteEffects = SpriteEffects.FlipVertically;
        }

        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Texture2D glowmask = ModContent.Request<Texture2D>(Glowmask).Value;
        Main.instance.LoadProjectile(ProjectileID.TerraBlade2); //Loads the sprite in case it hasn't been already
        Texture2D slashSprite = TextureAssets.Projectile[ProjectileID.TerraBlade2].Value;
        Texture2D starSparkle = TextureAssets.Extra[98].Value;

        //Draw the slash visuals. Holy shit I hate decompiled code

        //Set up the positioning 
        Vector2 hiltPos = Projectile.Center - Main.screenPosition;
        Rectangle slashSpriteRect = slashSprite.Frame(1, 4);
        Vector2 rectCenter = slashSpriteRect.Size() / 2f;
        float num5 = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0); //still don't understand exactly what this does
        num5 = 0.5f + num5 * 0.5f;
        num5 = Utils.Remap(num5, 0.2f, 1f, 0f, 1f);

        //Define the colors
        Color darkest = new(6, 6, 80);
        Color dark = new(1, 25, 91);
        Color lightest = new(30, 60, 252);
        Color light = new(27, 53, 222);
        Color color4 = Color.White * Projectile.Opacity * 0.5f;
        color4.A = (byte)((float)(int)color4.A * (1f - num5));
        Color color5 = color4 * num5 * 0.5f;
        color5.G = (byte)((float)(int)color5.G * num5);
        color5.B = (byte)((float)(int)color5.R * (0.25f + num5 * 0.75f));

        float slashOpacity;
        slashOpacity = Math.Clamp(Progress, 0f, 1f);

        //Draw the swings
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSpriteRect, light * Opacity * 0.35f * Projectile.Opacity * slashOpacity, Projectile.rotation + 0.3f + rotation, rectCenter, size * 1.69f, slashSpriteEffects, 0f);
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSpriteRect, dark * Opacity * 0.35f * Projectile.Opacity * slashOpacity, Projectile.rotation + 0.32f + rotation, rectCenter, size * 1.69f, slashSpriteEffects, 0f);
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSpriteRect, darkest * Opacity * 0.7f * Projectile.Opacity * slashOpacity, Projectile.rotation + rotation, rectCenter, size * 1.33f, slashSpriteEffects, 0f);
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSpriteRect, color5 * 0.25f * Projectile.Opacity * slashOpacity, Projectile.rotation + rotation, rectCenter, size * 1.33f, slashSpriteEffects, 0f);
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSpriteRect, color5 * 0.15f * Projectile.Opacity * slashOpacity, Projectile.rotation + rotation, rectCenter, size * 1.54f, slashSpriteEffects, 0f);
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSpriteRect, light * num5 * Opacity * 0.35f * Projectile.Opacity * slashOpacity, Projectile.rotation - 0.4f + rotation, rectCenter, size * 1.33f, slashSpriteEffects, 0f);
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSpriteRect, light * num5 * Opacity * 0.3f * Projectile.Opacity * slashOpacity, Projectile.rotation + rotation, rectCenter, size * 1.69f + 0.09f * Progress, slashSpriteEffects, 0f);
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSpriteRect, lightest * num5 * Opacity * 0.09f * Projectile.Opacity * slashOpacity, Projectile.rotation + 0.1f + rotation, rectCenter, size * 1.65f + 0.09f * Progress, slashSpriteEffects, 0f);
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSprite.Frame(1, 4, 0, 3), Color.Blue * 0.6f * Projectile.Opacity * slashOpacity, Projectile.rotation + rotation, rectCenter, size * 1.33f, slashSpriteEffects, 0f);
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSprite.Frame(1, 4, 0, 3), Color.Blue * 0.5f * Projectile.Opacity * slashOpacity, Projectile.rotation + rotation, rectCenter, size * 1.11f, slashSpriteEffects, 0f);
        Main.spriteBatch.Draw(slashSprite, hiltPos, slashSprite.Frame(1, 4, 0, 3), Color.Blue * 0.4f * Projectile.Opacity * slashOpacity, Projectile.rotation + rotation, rectCenter, size * 0.88f, slashSpriteEffects, 0f);

        int starCount = 30;
        //draw the blue star sparkles
        for (float i = 0f; i < starCount; i++)
        {
            float starAngle = Projectile.rotation + (i - starCount / 2) * MathHelper.ToRadians(150f) / starCount + rotation + MathHelper.ToRadians(-30) * Projectile.spriteDirection;
            if (starAngle >= InitialAngle && Projectile.spriteDirection > 0)
            {
                Vector2 drawpos = hiltPos + starAngle.ToRotationVector2() * Projectile.Size.Length() * size * 1.65f;
                Main.spriteBatch.Draw(starSparkle, drawpos, default, Main.hslToRgb(0.64f, 1f, 0.3f, 0) * Projectile.Opacity * (i / starCount), starAngle, starSparkle.Size() / 2, size * 1.2f, SpriteEffects.None, 0f);
            }
            else if (starAngle <= InitialAngle && Projectile.spriteDirection < 0)
            {
                Vector2 drawpos = hiltPos + starAngle.ToRotationVector2() * Projectile.Size.Length() * size * 1.65f;
                Main.spriteBatch.Draw(starSparkle, drawpos, default, Main.hslToRgb(0.64f, 1f, 0.3f, 0) * Projectile.Opacity * ((starCount - i) / starCount), starAngle, starSparkle.Size() / 2, size * 1.2f, SpriteEffects.None, 0f);
            }
        }

        //draw a second set of smaller, white sparkles to make it look like it's glowing
        for (float i = 0f; i < starCount; i += 1f)
        {
            float starAngle = Projectile.rotation + (i - starCount / 2) * MathHelper.ToRadians(150f) / starCount + rotation + MathHelper.ToRadians(-30) * Projectile.spriteDirection;
            if (starAngle >= InitialAngle && Projectile.spriteDirection > 0)
            {
                Vector2 drawpos = hiltPos + starAngle.ToRotationVector2() * Projectile.Size.Length() * size * 1.65f;
                Main.spriteBatch.Draw(starSparkle, drawpos, default, new Color(255, 255, 255, 0) * Projectile.Opacity * (i / starCount) * 0.7f, starAngle, starSparkle.Size() / 2, size * new Vector2(0.13f, 1), SpriteEffects.None, 0f);
            }
            else if (starAngle <= InitialAngle && Projectile.spriteDirection < 0)
            {
                Vector2 drawpos = hiltPos + starAngle.ToRotationVector2() * Projectile.Size.Length() * size * 1.65f;
                Main.spriteBatch.Draw(starSparkle, drawpos, default, new Color(255, 255, 255, 0) * Projectile.Opacity * ((starCount - i) / starCount) * 0.7f, starAngle, starSparkle.Size() / 2, size * new Vector2(0.13f, 1), SpriteEffects.None, 0f);
            }
        }

        //Draw the sword last so it goes on top
        Main.spriteBatch.Draw(texture, hiltPos, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, swordEffects, 0);
        Main.spriteBatch.Draw(glowmask, hiltPos, default, Color.White * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, swordEffects, 0);

        return false;
    }

    // Find the start and end of the sword and use a line collider to check for collision with enemies
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 start = Owner.MountedCenter;
        Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 1.75f);
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 25f * Projectile.scale, ref collisionPoint);
    }

    // Do a similar collision check for tiles
    public override void CutTiles()
    {
        Vector2 start = Owner.MountedCenter;
        Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 1.7f);
        Utils.PlotTileLine(start, end, 25 * Projectile.scale, DelegateMethods.CutTiles);
    }

    // We make it so that the projectile can only do damage in its release and unwind phases
    public override bool? CanDamage()
    {
        if (CurrentStage == AttackStage.Prepare)
            return false;
        return base.CanDamage();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        //Creates a glowing slash when it hits an enemy

        Vector2 spawnPoint = Main.rand.NextVector2FromRectangle(target.Hitbox);
        float angle = spawnPoint.AngleTo(Main.rand.NextVector2FromRectangle(target.Hitbox));
        Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.HeldItem), spawnPoint, Vector2.Zero, ModContent.ProjectileType<HFMuramasaGlowingSlash>(), Projectile.damage, 0f, Owner.whoAmI, angle, target.whoAmI);
        if (CurrentAttack == AttackType.Quickdraw) //spawn an extra if it's a quickdraw. Add 90 degrees cause it makes a cool cross
        {
            angle += MathHelper.ToRadians(90f);
            Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.HeldItem), spawnPoint, Vector2.Zero, ModContent.ProjectileType<HFMuramasaGlowingSlash>(), Projectile.damage, 0f, Owner.whoAmI, angle, target.whoAmI);
        }
        Projectile.netUpdate = true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        // Make knockback go away from player
        modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;

        // If the NPC is hit by the spin attack, increase knockback slightly. Increase it even more and ignore half the enemy armor for the quickdraw
        if (CurrentAttack == AttackType.Spin)
            modifiers.Knockback += 1;
        if (CurrentAttack == AttackType.Quickdraw)
        {
            modifiers.Knockback += 3;
            modifiers.ScalingArmorPenetration += 0.5f;
        }
    }
    //Thank you TML, for running ModifyHitPlayer on the projectile owner's client instead of the client that actually has authority over the hit
/*     public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        // Make knockback go away from player
        modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;

        // If the player is hit by the spin attack, increase knockback slightly. Increase it even more and ignore half the enemy player's armor for the quickdraw
        if (CurrentAttack == AttackType.Spin)
            modifiers.Knockback += 1;
        if (CurrentAttack == AttackType.Quickdraw)
        {
            modifiers.Knockback += 3;
            modifiers.ScalingArmorPenetration += 0.5f;
        }
    } */
    public override void OnKill(int timeLeft)
    { //We aren't spawning retrievable ammo, so I removed the myPlayer check.
        if (CurrentAttack == AttackType.BackSwing || CurrentAttack == AttackType.ShortBackSwing || CurrentAttack == AttackType.FastBackSwing1 || CurrentAttack == AttackType.FastBackSwing2 || CurrentAttack == AttackType.Quickdraw)
        {
            Owner.direction = Projectile.spriteDirection * -1;
        }
        else
        {
            Owner.direction = Projectile.spriteDirection; //reset the player's direction at the end of each strike
        }
    }
    // Function to easily set projectile and arm position
    public void SetSwordPosition()
    {
        Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress; // Set projectile rotation

        // Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
        Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathF.PI / 2); // get position of hand

        armPosition.Y += Owner.gfxOffY;
        Projectile.Center = armPosition; // Set projectile to arm position
        Projectile.scale = 1.5f * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers
        Projectile.Opacity = Opacity;

        Owner.heldProj = Projectile.whoAmI; // set held projectile to this projectile
    }

    // Function facilitating the taking out of the sword
    private void PrepareStrike()
    {
        float prepDuration = PrepTime;
        if (CurrentAttack >= AttackType.FastSwing1 && CurrentAttack <= AttackType.FastBackSwing2) //if this is any fast swing
        {
            prepDuration *= FASTRANGE / MathF.PI; //adjust length of swing to correspond with range
        }

        Progress = WINDUP * SWINGRANGE * (1f - Timer / prepDuration); // Calculates rotation from initial angle
        Opacity = MathHelper.SmoothStep(0, 1, Timer / prepDuration); // Slowly increase the opacity to fade in

        if (Timer >= prepDuration)
        {
            SoundEngine.PlaySound(SoundID.Item1); // Play sword sound here since playing it on spawn is too early
            CurrentStage = AttackStage.Execute; // If attack is over prep time, we go to next stage
        }
    }

    // Function facilitating the first half of the swing
    private void ExecuteStrike()
    {
        float execDuration = ExecTime;
        if (CurrentAttack >= AttackType.FastSwing1 && CurrentAttack <= AttackType.FastBackSwing2) //if this is any fast swing
        {
            execDuration *= FASTRANGE / MathF.PI; //adjust length of swing to correspond with range
        }
        if (CurrentAttack != AttackType.Spin && CurrentAttack != AttackType.Quickdraw)
        {
            if (CurrentAttack >= AttackType.FastSwing1 && CurrentAttack <= AttackType.FastBackSwing2)
            {
                Progress = MathHelper.SmoothStep(0, FASTRANGE, (1f - UNWIND) * Timer / execDuration);
            }
            else
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execDuration);
            }

            if (Timer >= execDuration)
            {
                CurrentStage = AttackStage.Unwind;
            }
        }
        else
        {
            Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) * Timer / (ExecTime * SPINTIME));

            if (Timer == (int)(ExecTime * SPINTIME * 3 / 4))
            {
                SoundEngine.PlaySound(SoundID.Item1); // Play sword sound again
                Projectile.ResetLocalNPCHitImmunity(); // Reset the local npc hit immunity for second half of spin
            }

            if (Timer >= ExecTime * SPINTIME)
            {
                CurrentStage = AttackStage.Unwind;
            }

            Vector2 end = Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
            if (end.X < Owner.MountedCenter.X && Progress >= 1)
            {
                Owner.direction = -1;
            }
            else if (end.X > Owner.MountedCenter.X && Progress >= 1)
            {
                Owner.direction = 1;
            }
        }
        SpawnDust();
    }

    // Function facilitating the latter half of the swing where the sword disappears
    private void UnwindStrike()
    {
        float hideDuration = HideTime;
        if (CurrentAttack >= AttackType.FastSwing1 && CurrentAttack <= AttackType.FastBackSwing2) //if this is any fast swing
        {
            hideDuration *= FASTRANGE / MathF.PI; //adjust length of swing to correspond with range
        }
        if (CurrentAttack != AttackType.Spin && CurrentAttack != AttackType.Quickdraw)
        {
            if (CurrentAttack >= AttackType.FastSwing1 && CurrentAttack <= AttackType.FastBackSwing2)
            {
                Progress = MathHelper.SmoothStep(0, FASTRANGE, (1f - UNWIND) + UNWIND * Timer / hideDuration); //some of my code is pretty hard to read, should probably fix that sometime. SoonTM
            }
            else
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideDuration);
            }
            Opacity = 1f - MathHelper.SmoothStep(0, 1, Timer / hideDuration); // Make sword slowly decrease in size as we end the swing to make a smooth hiding animation

            if (Timer >= hideDuration)
            {
                Projectile.Kill();
            }
        }
        else
        {
            Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) + UNWIND / 2 * Timer / (hideDuration * SPINTIME / 2));
            Opacity = 1f - MathHelper.SmoothStep(0, 1, Timer / (hideDuration * SPINTIME / 2));

            if (Timer >= hideDuration * SPINTIME / 2)
            {
                Projectile.Kill();
            }
        }
    }

    private void SpawnDust()
    {
        //The dust moves 90 degrees off from the projectile's rotation to create the illusion of moving with it
        int dustCount = Main.rand.Next(1, 3);
        for (int i = 0; i < dustCount; i++)
        {
            Vector2 dustPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * Main.rand.NextFloat());
            Vector2 velocity = (Projectile.rotation + MathHelper.ToRadians(90f)).ToRotationVector2() * 1.4f * Projectile.spriteDirection;
            Dust newDust = Dust.NewDustPerfect(dustPos, DustID.DungeonWater, velocity);
            newDust.noGravity = true;
        }
    }


}

public class HFMuramasaGlowingSlash : ModProjectile
{
    private ref float Angle => ref Projectile.ai[0]; // The randomly selected angle of the slash
    private ref float IDTarget => ref Projectile.ai[1]; //The NPC this slash targets. If the NPC dies, instantly executes the slash.
    //TO DO: Implement targeting enemy players
    public override void SetDefaults()
    {
        Projectile.width = 58; //not sure what height and width should be yet since the projectile is invisible
        Projectile.height = 72;
        Projectile.friendly = true;
        Projectile.timeLeft = 60; //timeLeft used as a timer to manage the animation
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1; // This should only hit once
        Projectile.DamageType = DamageClass.Melee;
        Projectile.ArmorPenetration = 15;

        Projectile.noEnchantmentVisuals = true;
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Projectile.timeLeft <= 8)
        {
            Vector2 start = Projectile.Center + Angle.ToRotationVector2() * Projectile.Size;
            Vector2 end = Projectile.Center - Angle.ToRotationVector2() * Projectile.Size * Projectile.timeLeft / 8;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 25f * Projectile.scale, ref collisionPoint);
        }
        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D starSparkle = TextureAssets.Extra[98].Value;
        Vector2 start = Projectile.Center - Main.screenPosition;
        //draw the "cut mark" before the strike happens.
        Main.spriteBatch.Draw(starSparkle, start, default, new Color(8, 21, 200, 0), Angle, starSparkle.Size() / 2, new Vector2(0.001f + (60 - Projectile.timeLeft) / 70f, 3f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(starSparkle, start, default, new Color(255, 255, 255, 0) * 0.7f, Angle, starSparkle.Size() / 2, new Vector2(0.001f + (60 - Projectile.timeLeft) / 420f, 0.5f), SpriteEffects.None, 0f);
        if (Projectile.timeLeft <= 8) //animate the real slash
            for (int i = 0; i <= 24 - Projectile.timeLeft * 3f; i++) //draw the larger blue sparkles of the slash
            {
                Main.spriteBatch.Draw(starSparkle,
                start + (Angle + MathHelper.ToRadians(90f)).ToRotationVector2() * Projectile.Size * (i - 12f) / 5f, //rotated 90 degrees because star texture is horizontal. Also uses i here to adjust length.
                default,
                new Color(0, 30, 179, 0),
                Angle,
                starSparkle.Size() / 2,
                new Vector2(0.25f + 1.25f / 8 * Projectile.timeLeft, 1.5f),
                SpriteEffects.None, 0f
                );

                Main.spriteBatch.Draw(starSparkle, //then draw white sparkles to make it look like it's glowing. Note that while the sword's glowing edge does this in 2 loops because of layering, we do it in one here.
                start + (Angle + MathHelper.ToRadians(90f)).ToRotationVector2() * Projectile.Size * (i - 12f) / 5f,
                default,
                Color.White * 0.5f, //opacity is reduced to make it less intense
                Angle,
                starSparkle.Size() / 2,
                new Vector2(0.0417f + 0.25f / 8 * Projectile.timeLeft, 1f), //size is reduced to one sixth that of the blue sparkles
                SpriteEffects.None, 0f
                );
            }
        return false;
    }

    public override void AI()
    {
        NPC target = Main.npc[(int)IDTarget];
        if (Projectile.timeLeft >= 8)
        {
            if (Projectile.timeLeft == 8)
                SoundEngine.PlaySound(SoundID.Item131 with { Volume = 0.3f, Pitch = 1.9f, MaxInstances = 0 }, Projectile.position);

            if (target.active && target.life + target.defense > Projectile.damage + Projectile.ArmorPenetration)
            {
                Projectile.position = Projectile.position + (target.position - target.oldPosition);
                if (!target.Hitbox.Intersects(Projectile.Hitbox))
                {
                    Projectile.Center = target.Center; //failsafe for enemies with special movement (e.g. lunatic cultist, moon lord eyes)
                }
            }
            else
            {
                Projectile.timeLeft = 8;
            }
        }
    }
}