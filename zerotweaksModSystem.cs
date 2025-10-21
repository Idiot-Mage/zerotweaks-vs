using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;


public class zerotweaksModSystem : ModSystem{
	public static int version = 1;
	public class ZeroConfigData{
		public int configVers = version;
		public bool harderTempStorms = true;
		public bool loreDamageBuff = true;
	}

	public static ZeroConfigData zeroConf;
	public void loadConfig(ICoreAPI api){
		try{
			zeroConf = api.LoadModConfig<ZeroConfigData>("zerotweaks.json");
			if(zeroConf==null || zeroConf.configVers!=version){
				zeroConf = new ZeroConfigData();
			}
			api.StoreModConfig<ZeroConfigData>(zeroConf,"zerotweaks.json");
		}catch(Exception e){
			Mod.Logger.Error("Could not load ZeroTweaks config. Using default.");
			Mod.Logger.Error(e);
			zeroConf = new ZeroConfigData();
		}
	}
	
	
        ICoreClientAPI capi;
        ICoreServerAPI sapi;
        
	public override void Start(ICoreAPI api){
		api.RegisterEntityBehaviorClass("zerotweaksRust",typeof(zerotweaksRust));
		Mod.Logger.Notification("hello from zerotweaks");
	}

	public override void StartServerSide(ICoreServerAPI api){
            base.StartServerSide(api);
            sapi = api;
            api.Event.RegisterGameTickListener(onTickServer1s, 1000, 200);
            api.Event.RegisterGameTickListener(temporalDecrease, 1500, 200);
            
            loadConfig(api);
	}

	public override void StartClientSide(ICoreClientAPI api){
		capi = api;
	}
	
	private void Event_LevelFinalize(){
		
	}
	
	public void temporalDecrease(float dt){
		if(!zeroConf.harderTempStorms){return;}
		
		var stormData = sapi.ModLoader.GetModSystem<SystemTemporalStability>().StormData;
		foreach(var plr in sapi.World.AllOnlinePlayers){
			if(stormData.nowStormActive){
				if(stormData!=null){
					var strength = stormData.stormGlitchStrength;
					double amount = 0;
					if(strength<0.67f){
						amount=0.0025;
						//Mod.Logger.Notification("light");
					}
					if(strength>=0.67f && strength<0.9f){
						amount=0.0035;
						//Mod.Logger.Notification("medium");
					}
					if(strength>=0.9f){
						amount=0.005;
						//Mod.Logger.Notification("heavy");
					}
					
					double oldStability = plr.Entity.WatchedAttributes.GetDouble("temporalStability");
					plr.Entity.WatchedAttributes.SetDouble("temporalStability", oldStability-amount);
				}
			}
		}
	}
	
	private void onTickServer1s(float dt){
		foreach(var plr in sapi.World.AllOnlinePlayers){
			var inv = plr.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);
			if(inv==null){continue;}
			var neckSlot = inv[(int)EnumCharacterDressType.Neck];
			if(neckSlot?.Itemstack?.Collectible?.Code.Path == "clothes-neck-gear-amulet-temporal"){
				double oldStability = plr.Entity.WatchedAttributes.GetDouble("temporalStability");
				if(oldStability<1){
					plr.Entity.WatchedAttributes.SetDouble("temporalStability", oldStability+0.0005);
				}
			}
			
			if(neckSlot?.Itemstack?.Collectible?.Code.Path == "clothes-neck-merchant-amulet"){
				plr.Entity.Stats.Set("walkspeed", "zerotweaks:merchantamulet", 0.1f);
			}else{
				plr.Entity.Stats.Set("walkspeed", "zerotweaks:merchantamulet", 0f);
			}
			
			if(neckSlot?.Itemstack?.Collectible?.Code.Path == "clothes-neck-gear-amulet-rusty"){
				plr.Entity.Stats.Set("rustyGearDropRate", "zerotweaks:rustyamulet", 0.15f);
			}else{
				plr.Entity.Stats.Set("rustyGearDropRate", "zerotweaks:rustyamulet", 0f);
			}
			
			if(neckSlot?.Itemstack?.Collectible?.Code.Path == "clothes-neck-gear-amulet-gold"){
				plr.Entity.Stats.Set("miningSpeedMul", "zerotweaks:goldgear", 0.2f);
				plr.Entity.Stats.Set("oreDropRate", "zerotweaks:goldgear2", 0.1f);
			}else{
				plr.Entity.Stats.Set("miningSpeedMul", "zerotweaks:goldgear", 0f);
				plr.Entity.Stats.Set("oreDropRate", "zerotweaks:goldgear2", 0f);
			}
			
			if(neckSlot?.Itemstack?.Collectible?.Code.Path == "clothes-neck-forlorn-talisman"){
				plr.Entity.Stats.Set("armorDurabilityLoss", "zerotweaks:forlorn", -0.4f);
			}else{
				plr.Entity.Stats.Set("armorDurabilityLoss", "zerotweaks:forlorn", 0f);
			}
			
			if(neckSlot?.Itemstack?.Collectible?.Code.Path == "clothes-neck-teardrop-amulet"){
				plr.Entity.Stats.Set("animalSeekingRange", "zerotweaks:animalseek", -0.25f);
			}else{
				plr.Entity.Stats.Set("animalSeekingRange", "zerotweaks:animalseek", 0f);
			}
			
			if(neckSlot?.Itemstack?.Collectible?.Code.Path == "clothes-neck-simple-cross"){
				plr.Entity.Stats.Set("healingeffectivness", "zerotweaks:simple", 0.25f);
			}else{
				plr.Entity.Stats.Set("healingeffectivness", "zerotweaks:simple", 0f);
			}
			
			if(neckSlot?.Itemstack?.Collectible?.Code.Path == "clothes-neck-fancy-cross"){
				plr.Entity.Stats.Set("healingeffectivness", "zerotweaks:advanced", 0.5f);
			}else{
				plr.Entity.Stats.Set("healingeffectivness", "zerotweaks:advanced", 0f);
			}
			
			if(neckSlot?.Itemstack?.Collectible?.Code.Path == "clothes-neck-deep"){
				plr.Entity.Stats.Set("jumpHeightMul", "zerotweaks:deep", 1f);
			}else{
				plr.Entity.Stats.Set("jumpHeightMul", "zerotweaks:deep", 0f);
			}
			
			if(neckSlot?.Itemstack?.Collectible?.Code.Path == "clothes-neck-jade-amulet"){
				plr.Entity.Stats.Set("hungerrate", "zerotweaks:jade", -0.25f);
			}else{
				plr.Entity.Stats.Set("hungerrate", "zerotweaks:jade", 0f);
			}	
		}
	}

}

public class zerotweaksRust : EntityBehavior{
	public override string PropertyName() => "zerotweaksRust";
	
	public zerotweaksRust(Entity entity) : base(entity){
		EntityBehaviorHealth health = entity.GetBehavior<EntityBehaviorHealth>();
		if(health!=null){
			health.onDamaged+=reduceDamage;
		}
	}
	
	private float reduceDamage(float damage,DamageSource source){
		if(!zerotweaksModSystem.zeroConf.loreDamageBuff){return damage;}
	
		Entity causeEntity = source.GetCauseEntity();
		if(causeEntity == null || source.Type==EnumDamageType.Heal){
			return damage;
		}
		
		if(causeEntity is EntityPlayer plyr){
			ItemSlot slot = plyr.RightHandItemSlot;
			ItemStack stack = slot?.Itemstack;
			if(stack!=null){
				AssetLocation asset = stack.Collectible.Code;
				if(!asset.Path.Contains("falx") && !asset.Path.Contains("bow")){
					damage*=0.5f;
				}
				if(asset.Path.Contains("bow")){
					damage*=0.75f;
				}
			}
		}else{
			damage*=0.5f;
		}
		
		return damage;
	}
}

