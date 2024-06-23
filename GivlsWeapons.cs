global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using GivlsWeapons.Core;
global using Terraria;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using Terraria.ID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GivlsWeapons
{ //Thanks to Starlight River for their primitive trail system
	public class TemporaryFix : PreJITFilter
	{
		public override bool ShouldJIT(MemberInfo member)
		{
			return false;
		}
	}

	public partial class GivlsWeapons : Mod
	{
		private List<IOrderedLoadable> loadCache;
		public static GivlsWeapons Instance { get; set; }
		public GivlsWeapons()
		{
			Instance = this;
			PreJITFilter = new TemporaryFix();
		}
		private Vector2 lastScreenSize;
		public static void SetLoadingText(string text)
		{
			FieldInfo Interface_loadMods = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.Interface")!.GetField("loadMods", BindingFlags.NonPublic | BindingFlags.Static)!;
			MethodInfo UIProgress_set_SubProgressText = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIProgress")!.GetProperty("SubProgressText", BindingFlags.Public | BindingFlags.Instance)!.GetSetMethod()!;

			UIProgress_set_SubProgressText.Invoke(Interface_loadMods.GetValue(null), new object[] { text });
		}

		public override void Load()
		{
			loadCache = new List<IOrderedLoadable>();

			foreach (Type type in Code.GetTypes())
			{
				if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IOrderedLoadable)))
				{
					object instance = Activator.CreateInstance(type);
					loadCache.Add(instance as IOrderedLoadable);
				}

				loadCache.Sort((n, t) => n.Priority.CompareTo(t.Priority));
			}

			for (int k = 0; k < loadCache.Count; k++)
			{
				loadCache[k].Load();
				SetLoadingText("Loading " + loadCache[k].GetType().Name);
			}

			if (!Main.dedServ)
			{
				lastScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
			}
		}

		public override void Unload()
		{
			if (loadCache != null)
			{
				foreach (IOrderedLoadable loadable in loadCache)
				{
					loadable.Unload();
				}

				loadCache = null;
			}
			else
			{
				Logger.Warn("load cache was null, IOrderedLoadable's may not have been unloaded...");
			}

			if (!Main.dedServ)
			{
				Instance ??= null;
			}
		}
		public void CheckScreenSize()
		{
			if (!Main.dedServ && !Main.gameMenu)
				lastScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
		}
	}
}