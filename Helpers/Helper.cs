using System;

namespace GivlsWeapons.Helpers
{
	public static partial class Helper
	{
		/// <summary>
		/// Reduces the scale and damage of a projectile, based on the ratio of power to targPower. Kills the target if power is greater than or equal to targPower, or the damage or size become 0 or less.
		/// </summary>
		/// <param name="target">The projectile to weaken or destroy</param>
		/// <param name="power">The strength of the weakening effect. Will kill the target if it is greater than or equal to targPower</param>
		/// <param name="targPower">The target projectile's resistance to the effect. </param>
		/// <param name="reduceDamage">Whether to reduce the damage of the target, or just its scale</param>
		/// <param name="kill">Whether to kill the projectile if it gets too small or weak. Only set to false if you want to kill the projectile manually</param>
		public static bool WeakenProjectile(Projectile target, float power, float targPower, bool reduceDamage = true, bool kill = true)
		{
			target.scale *= (targPower - power) / targPower;
			if(reduceDamage)
			{
				target.damage = (int)(target.damage * ((targPower - power) / targPower));
			}

			if((power >= targPower || target.damage <= 0 || target.scale <= 0f) && kill)
			{
				target.Kill();
				return true;
			}
			else return false;
		}
		/// <summary>
		/// Disables most default projectile behavior while leaving the projectile intact
		/// </summary>
		/// <param name="proj"></param>The projectile to disable
		public static void DisableProjectile(Projectile proj)
		{
			proj.tileCollide = false;
			proj.damage = 0;
			proj.alpha = 255;
			proj.velocity = Vector2.Zero;
			proj.noEnchantments = true;
			proj.noEnchantmentVisuals = true;
		}
		public static Vector3 Vec3(this Vector2 vector)
		{
			return new Vector3(vector.X, vector.Y, 0);
		}
		public static T[] FastUnion<T>(this T[] front, T[] back)
		{
			var combined = new T[front.Length + back.Length];

			Array.Copy(front, combined, front.Length);
			Array.Copy(back, 0, combined, front.Length, back.Length);

			return combined;
		}
	}
}