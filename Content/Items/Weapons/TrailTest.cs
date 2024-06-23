using System.Collections.Generic;
using GivlsWeapons.Helpers;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Moonstone
{
	public class Moonfury : ModItem
	{
		private int cooldown = 0;

		public override string Texture => "GivlsWeapons/Assets/Textures/Moonfury";
		public override void SetDefaults()
		{
			Item.damage = 28;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 38;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 7.5f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
			Item.shootSpeed = 14f;
			Item.autoReuse = false;
			Item.shoot = ModContent.ProjectileType<MoonfuryProj>();
			Item.useTurn = true;
		}
		public override void HoldItem(Player Player)
		{
			cooldown--;
			base.HoldItem(Player);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			var direction = new Vector2(0, -1);
			direction = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
			position = Main.MouseWorld + direction * 800;

			direction *= -10;
			velocity = direction;
			damage = (int)(damage * 1.5f);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (cooldown > 0)
				return false;

			cooldown = 95;
			return true;
		}
	}

	internal class MoonfuryProj : ModProjectile, IDrawPrimitive, IDrawAdditive
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private float trailWidth = 1;
		private bool stuck = false;

		public override string Texture => "GivlsWeapons/Assets/Textures/MoonfuryProj";

		public override void SetDefaults()
		{
			Projectile.width = 36;
			Projectile.height = 50;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.extraUpdates = 6;
		}

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void AI()
		{
			if (Projectile.ai[0] == 0)
				Projectile.ai[0] = Main.MouseWorld.Y;

			if (Projectile.Bottom.Y > Projectile.ai[0])
				Projectile.tileCollide = true;
			else
				Projectile.tileCollide = false;

			if (!stuck)
			{
				ManageCaches();

				Projectile.rotation = Projectile.velocity.ToRotation() - 1.44f;
			}
			else
			{
				Projectile.friendly = false;

				if (Projectile.timeLeft <= 30)
					Projectile.alpha += 10;

				trailWidth *= 0.93f;

				if (trailWidth > 0.05f)
					trailWidth -= 0.05f;
				else
					trailWidth = 0;
			}

			ManageTrail();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (!stuck)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item96, Projectile.Center);
				stuck = true;
				Projectile.extraUpdates = 0;
				Projectile.velocity = Vector2.Zero;
				Projectile.timeLeft = 90;
			}

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 pos = Projectile.Bottom + new Vector2(0, 20) - Main.screenPosition;
			Main.spriteBatch.Draw(tex, pos, null, lightColor * (1 - Projectile.alpha / 255f), Projectile.rotation, new Vector2(tex.Width / 2, tex.Height), Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 50; i++)
				{
					cache.Add(Projectile.Bottom + new Vector2(0, 20));
				}
			}

			if (Projectile.oldPos[0] != Vector2.Zero)
				cache.Add(Projectile.oldPos[0] + new Vector2(Projectile.width / 2, Projectile.height) + new Vector2(0, 20));

			while (cache.Count > 50)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 50, new RoundedTip(12), factor => (10 + factor * 25) * trailWidth, factor => new Color(120, 20 + (int)(100 * factor.X), 255) * factor.X * trailWidth);

			trail.Positions = cache.ToArray();

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 50, new RoundedTip(6), factor => (80 + 0 + factor * 0) * trailWidth, factor => new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * 0.15f * trailWidth);

			trail2.Positions = cache.ToArray();

			if (Projectile.velocity.Length() > 1)
			{
				trail.NextPosition = Projectile.Bottom + new Vector2(0, 20) + Projectile.velocity;
				trail2.NextPosition = Projectile.Bottom + new Vector2(0, 20) + Projectile.velocity;
			}
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(8f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("GivlsWeapons/Assets/Textures/GlowTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("GivlsWeapons/Assets/Textures/DatsuzeiFlameMap2").Value);

			trail?.Render(effect);

			effect.Parameters["sampleTexture2"].SetValue(TextureAssets.MagicPixel.Value);

			trail2?.Render(effect);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture + "_Additive").Value;
			Color color = Color.White * (1 - Projectile.alpha / 255f);
			spriteBatch.Draw(tex, Projectile.Bottom + new Vector2(0, 20) - Main.screenPosition, null, color * 0.5f, Projectile.rotation, new Vector2(tex.Width / 2, tex.Height), Projectile.scale, SpriteEffects.None, 0);
		}
	}
}