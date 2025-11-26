using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ChestVariety
{
	public class ChestConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[ReloadRequired]
		[DefaultValue(false)]
		public bool LiteMode { get; set; }
	}
}