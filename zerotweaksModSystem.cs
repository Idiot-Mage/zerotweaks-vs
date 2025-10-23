using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Util;
using Vintagestory.API.Datastructures;

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
		Mod.Logger.Notification("hello from zerotweaks");
		api.RegisterEntityBehaviorClass("zerotweaksRust",typeof(zerotweaksRust));
		api.RegisterEntityBehaviorClass("zerotweaksWraith",typeof(zerotweaksWraith));
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
	
	int spawnTimer = 0;
	private void temporalDecrease(float dt){
		if(!zeroConf.harderTempStorms){return;}
		
		var stormData = sapi.ModLoader.GetModSystem<SystemTemporalStability>().StormData;
		if(stormData.nowStormActive){
			spawnTimer++;
		}
		foreach(var plr in sapi.World.AllOnlinePlayers){
			if(stormData.nowStormActive){
				if(stormData!=null){
					var strength = stormData.stormGlitchStrength;
					double amount = 0;
					if(strength<0.67f){
						amount=0.0035;
						//Mod.Logger.Notification("light");
					}
					if(strength>=0.67f && strength<0.9f){
						amount=0.0045;
						//Mod.Logger.Notification("medium");
					}
					if(strength>=0.9f){
						amount=0.005;
						//Mod.Logger.Notification("heavy");
					}
					
					double oldStability = plr.Entity.WatchedAttributes.GetDouble("temporalStability");
					plr.Entity.WatchedAttributes.SetDouble("temporalStability", oldStability-amount);
					
					Random r = plr.Entity.World.Rand;
					int num = r.Next(0,3);
					int targetnum = 0;
					int targettime = 28;
					
					//this is kinda bad but i was struggling with rng plus its like 3am
					if(spawnTimer>=targettime && num==targetnum){
						spawnTimer=0;
						EntityProperties type = plr.Entity.World.GetEntityType(new AssetLocation("zerotweaks:wraith"));
						Entity entity = plr.Entity.World.ClassRegistry.CreateEntity(type);
						plr.Entity.World.SpawnEntity(entity);
						
						int rngx = r.Next((int)plr.Entity.ServerPos.X-25,(int)plr.Entity.ServerPos.X+25);
						int rngy = r.Next((int)plr.Entity.ServerPos.Y,(int)plr.Entity.ServerPos.Y+5);
						int rngz = r.Next((int)plr.Entity.ServerPos.Z-25,(int)plr.Entity.ServerPos.Z+25);
						Vec3d spawnPos = new Vec3d(rngx,rngy,rngz);
						
						entity.ServerPos.SetPosWithDimension(spawnPos);
						entity.Pos.SetFrom(entity.ServerPos);
						entity.PositionBeforeFalling.Set(entity.ServerPos.X,entity.ServerPos.Y,entity.ServerPos.Z);
						entity.Attributes.SetString("origin","worldgen");
					}else if(spawnTimer>=targettime && num!=targetnum){
						spawnTimer=0;
					}
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

public class zerotweaksWraith : EntityBehavior{
	public override string PropertyName() => "zerotweaksWraith";
	
	EntityPlayer target;
	int updateTime = 0;
	public zerotweaksWraith(Entity entity) : base(entity){

	}
	
	public override void OnGameTick(float dt){
		base.OnGameTick(dt);
		if(target!=null){
			updateTime++;
			if(updateTime>120){
				target=null;
				updateTime=0;
				return;
			}
			
			double dist = entity.ServerPos.DistanceTo(target.ServerPos.XYZ);
			if(dist>60){
				EntityBehaviorHealth health = entity.GetBehavior<EntityBehaviorHealth>();
				health.Health=-99;
			}
			
			double speed = Math.Clamp(dist/27,0.025,1);
			Vec3d dir = (target.ServerPos.XYZ-entity.ServerPos.XYZ).Normalize();
			
			entity.ServerPos.Yaw = (float)Math.Atan2(dir.X,dir.Z);
			entity.Pos.Yaw = (float)Math.Atan2(dir.X,dir.Z);
			
			dir*=speed;
			if(dist>0.5){
				entity.ServerPos.Add(dir.X,dir.Y,dir.Z);
				entity.Pos.SetPos(entity.ServerPos.X,entity.ServerPos.Y,entity.ServerPos.Z);
			}
		}else{
			target = entity.World.NearestPlayer(entity.ServerPos.X,entity.ServerPos.Y,entity.ServerPos.Z).Entity;
			updateTime = 0;
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

