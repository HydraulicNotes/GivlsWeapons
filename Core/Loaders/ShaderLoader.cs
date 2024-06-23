using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace GivlsWeapons.Core.Loaders
{
	class ShaderLoader : IOrderedLoadable
	{
		public float Priority => 0.9f;

		public void Load()
		{
			if (Main.dedServ)
				return;

			MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
			var file = (TmodFile)info.Invoke(GivlsWeapons.Instance, null);

			System.Collections.Generic.IEnumerable<FileEntry> shaders = file.Where(n => n.Name.StartsWith("Assets/Effects/") && (n.Name.EndsWith(".xnb") || n.Name.EndsWith(".fxc")));

			foreach (FileEntry entry in shaders)
			{
				string name = entry.Name.Replace(".xnb", "").Replace(".fxc", "").Replace("Assets/Effects/", "");
				string path = entry.Name.Replace(".xnb", "").Replace(".fxc", "");
				LoadShader(name, path);
			}
		}

		public void Unload()
		{

		}

		public static void LoadShader(string name, string path)
		{
			var screenRef = new Ref<Effect>(GivlsWeapons.Instance.Assets.Request<Effect>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
			Filters.Scene[name] = new Filter(new ScreenShaderData(screenRef, name + "Pass"), EffectPriority.High);
			Filters.Scene[name].Load();
		}
	}
}