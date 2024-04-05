#pragma warning disable CS0612
using BepInEx;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using ProtoBuf;
using System;
using UnityEngine;
using UWE;

namespace TheVogels.PortableBase
{
    [BepInPlugin(MyGuid, PluginName, VersionString)]
    [BepInDependency("com.snmodding.nautilus")]
    public class PortableBaseMain : BaseUnityPlugin
    {

        private const string MyGuid = "com.thevogels.portalblebase";
        private const string PluginName = "Portable Base";
        private const string VersionString = "0.0.7";
        //Version 0.0.5 -Implementing...
        //Version 0.0.7 -Working but with a fabricator.
        //Version 0.1.0 -Working!
        //Version 0.1.5 -Release?

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo(PluginName + " " + VersionString + " " + "loaded.");
            PortableBase.Register();
        }
    }

    [ProtoContract]
    public class PortableBase : LifepodDrop, IPowerInterface, IInteriorSpace
    {

        public static PrefabInfo Info { get; private set; }
        public static void Register()
        {
            Info = PrefabInfo
            .WithTechType
            (
                "PortableBase",
                "Deployable Base",
                "Can be deployed and pickupped again.\nOxygen and heating requires power and power can only be recharged under certain conditions."
            )
            .WithIcon(SpriteManager.Get(TechType.RocketStage3))
            .WithSizeInInventory(new Vector2int(3, (int)4.2f));

            var BasePrefab = new CustomPrefab(Info);
            //GameObject mirrorVariant1 = MyAssetBundle.LoadAsset<GameObject>("WorldEntities/Tools/LifepodDrop.prefab");
            // The model of our coal will use the same one as Nickel's.
            var BaseObj = new CloneTemplate(Info, "5c6464c0-96e8-45dc-92ba-a68adb32017a");
            BaseObj.ModifyPrefab += obj =>
            {
                try
                {
                LifepodDrop DeleteBase;
                if(obj.TryGetComponent(out DeleteBase));
                Destroy(DeleteBase);
                Destroy(obj.FindChild("LifePodDrop_Fabricator(Placeholder)"));
                }
                catch(Exception) { }
                var Base = obj.AddComponent<PortableBase>();
                try
                {
                    obj.FindChild("StorageContiner").GetComponent<StorageContainer>().Resize(2,4);
                    obj.FindChild("StorageContiner").GetComponent<StorageContainer>().container.Resize(2,4);
                } catch (Exception) { }
                obj.AddComponent<Pickupable>();
                obj.AddComponent<Battery>()._capacity=16.8f;
                FreezeRigidbodyWhenFar Freeze = obj.AddComponent<FreezeRigidbodyWhenFar>();
                Freeze.freezeDist = 30f;
                Base.dropComplete = false;
                Base.restartDrop = false;
            };
            BasePrefab.SetGameObject(BaseObj);
            var recipe = new RecipeData
            (
                new Ingredient(TechType.Battery, 1), 
                new Ingredient(TechType.AdvancedWiringKit, 2), 
                new Ingredient(TechType.TitaniumIngot, 1)
            );

            BasePrefab.SetRecipe(recipe)
            .WithFabricatorType(CraftTree.Type.Workbench)
            .WithCraftingTime(8.968999367f).WithStepsToFabricatorTab("root");

            BasePrefab.SetEquipment(EquipmentType.Hand);
            BasePrefab.Register();
        }

        private bool PickedUp = true;

        public void OnDropped(Pickupable p)
        {
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            PickedUp = false;
            dropComplete = false;
            OnDropBegin();
            if (Player.main.IsSwimming())
            {
                OnWaterCollision(transform.position);
            }
        }
        
        public void OnPickedUp(Pickupable p)
        {
            gameObject.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
            PickedUp = true;
        }

        public new bool IsDropSuspended()
        {
            return !PickedUp;
        }

        public bool IsPowered()
        {
            return GetPower() > 0;
        }

        private bool Pickupinit = false;

        public void Update()
        {
            restartDrop = false;
            //this..MasterIntensity = this.interiorMasterIntensity.Lerp(1f - powerLossValue);
            //this.interiorSky.DiffIntensity = this.interiorDiffuseIntensity.Lerp(1f - powerLossValue);
            //this.interiorSky.SpecIntensity = this.interiorSpecIntensity.Lerp(1f - powerLossValue);
            battery+= GetPowerUsage(Time.deltaTime);
            if (Upgrade_02)
            {
                BackupO2+= (battery<2.2621f ? (PlayerInside() ? -0.4f : 0f) : 0.2f)*Time.deltaTime;
                if(BackupO2 > 60f)
                BackupO2 = 60f;
                else if(BackupO2 < 0f)
                BackupO2 = 0f;
            }
            if (battery<2.2621f && !BaseO2)
            {
                ErrorMessage.AddMessage(Language.main.Get("Warning: not enough battery to power oxygen production, oxygen production is offline."));
                BaseO2 = true;
            }
            else if (battery>2.2621f)
            {
                BaseO2 = false;
            }
            if (battery<2.2621f && BackupO2 == 0 && !BaseO2Back && Upgrade_02)
            {
                ErrorMessage.AddMessage(Language.main.Get("Warning: Oxygen backup empty!"));
                BaseO2Back = true;
            }
            else if (battery>2.2621f)
            {
                BaseO2Back = false;
            }
            if (battery<5.16f && !BaseTempLow)
            {
                ErrorMessage.AddMessage(Language.main.Get("Warning: low battery, heater effiency low."));
                BaseTempLow = true;
            }
            else if (battery>5.16f)
            {
                BaseTempLow = false;
            }
            if (battery<5f && !BaseTemp)
            {
                ErrorMessage.AddMessage(Language.main.Get("Warning: not enough battery to power heaters."));
                BaseTemp = true;
            }
            else if (battery>5f)
            {
                BaseTemp = false;
            }
            if (battery<6.8f && !BaseLow)
            {
                ErrorMessage.AddMessage(Language.main.Get("Notice: battery low, heating systems will go offline and oxygen systems later."));
                BaseLow = true;
            }
            else if (battery>6.8f)
            {
                BaseLow = false;
            }
            var PlaceholderBattery = gameObject.GetComponent<Battery>();
            var pickupable = gameObject.GetComponent<Pickupable>();
            if (!Pickupinit) {
                pickupable.pickedUpEvent.AddHandler(this, new Event<Pickupable>.HandleFunction(OnPickedUp));
                pickupable.droppedEvent.AddHandler(this, new Event<Pickupable>.HandleFunction(OnDropped));
                Pickupinit = true;
            }
            PlaceholderBattery._capacity = GetMaxPower();
            PlaceholderBattery._charge = GetPower();
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            if (pickupable)
                pickupable.isPickupable = !PlayerInside();
            if (battery > GetMaxPower())
                battery = GetMaxPower();
            if (battery < 0f)
                battery = 0f;
            DisableParachute();
            base.Update();
            try
            {
                gameObject.FindChild("StorageContiner").GetComponent<StorageContainer>().Resize(2, 4);
                gameObject.FindChild("StorageContiner").GetComponent<StorageContainer>().container.Resize(2, 4);
                Destroy(gameObject.GetComponentsInChildren<Fabricator>(true)[1].gameObject);
            }
            catch (Exception) { }
        }

        private bool PlayerInside() => Player.main != null && (UnityEngine.Object)Player.main.currentInterior == this;

        public float GetPowerUsage(float DeltaTime)
        {
            //float Usage = GetPowerGain() - GetPowerDrain();
            return ( GetPowerGain() + GetPowerDrain() )*DeltaTime;
        }

        public float GetPowerGain()
        {
            WaterscapeVolume.Settings settings;
            WaterBiomeManager.main.GetSettings(transform.position, true, out settings);
            float PowerGain = 0.000001f;
            PowerGain+= ( (DayNightCycle.main.GetLocalLightScalar() * Mathf.Clamp01(settings.sunlightScale)) * ( depthCurve.Evaluate(Mathf.Clamp01((200f - Ocean.GetDepthOf(gameObject)) / 200f))) )* 0.0682f;
            return PowerGain;
        }

        private AnimationCurve depthCurve = new AnimationCurve();

        public float GetPowerDrain()
        {
            float Usage = 0f;
            if(PlayerInside())
            {
                var player = Player.main;
                //Oxygen
                if ((player.GetOxygenAvailable() != player.GetOxygenCapacity()) &&! (battery<2.2621f))
                Usage-= 1.1f;
            }
            //Temperature
            if(battery>5f)
            Usage-= 0.7f;
            return (Usage*0.065f)+0.00006f;
        }

        //[ProtoMember(4)]
        //[NonSerialized]
        //public Pickupable pickupable;

        public float GetPower()
        {
            return battery;
        }

        public float GetMaxPower()
        {
            return 16.83f;
        }
        
        float IPowerInterface.GetPower()
        {
            return battery;
        }

        float IPowerInterface.GetMaxPower()
        {
            return 16.83f;
        }

        bool IPowerInterface.ModifyPower(float amount, out float modified)
        {
            throw new NotImplementedException();
            //modified = 0f;
            //return false;
        }

        public bool HasInboundPower(IPowerInterface powerInterface)
        {
            return false;
        }

        public bool GetInboundHasSource(IPowerInterface powerInterface)
        {
            return false;
        }

        public void PollPowerRate(out float consumed, out float created)
        {
            throw new NotImplementedException();
        }

        public float GetPercentage()
        {
            return battery*5.941770647653001f;
        }

        VFXSurfaceTypes IInteriorSpace.GetDefaultSurface()
        {
            return VFXSurfaceTypes.metal;
        }

        //Base doesn't have enough power to have it's heating systems on.
        bool BaseTemp = false;
        //Base doesn't have enough power to have it's heating efficent.
        bool BaseTempLow = false;
        //Base doesn't have enough power to have it's O2 systems on.
        bool BaseO2 = false;
        bool BaseO2Back = false;
        //Base is low on power.
        bool BaseLow = false;

        public void SetBattery(float Battery)
        {
            battery = Battery;
        }
        
        public void AddSetBattery(float Add)
        {
            battery+= Add;
        }

        float IInteriorSpace.GetInsideTemperature()
        {
            return battery>5.16f ? 12.47f : (battery>5f ? -0.9888999f : 6f);
        }

        bool IInteriorSpace.CanBreathe()
        {
            return (battery>2.2621f) || (Upgrade_02 && (BackupO2>0f));
        }

        bool IInteriorSpace.CanDropItemsInside()
        {
            return false;
        }

        bool IInteriorSpace.IsStoryBase()
        {
            return false;
        }

        bool IInteriorSpace.IsValidForRespawn()
        {
            return true;
        }

        public GameObject GetGameObject() => gameObject;

        /*public void OnEquip(GameObject sender, string slot)
        {
            gameObject.transform.localScale.Set(0.04f, 0.04f, 0.04f);
        }

        public void OnUnequip(GameObject sender, string slot)
        {
            gameObject.transform.localScale.Set(1f, 1f, 1f);
        }

        public void UpdateEquipped(GameObject sender, string slot)
        {
        }*/

        [ProtoMember(3)]
        [NonSerialized]
        public float battery = 16.83f;

        [ProtoMember(5)]
        [NonSerialized]
        public float BackupO2 = 0f;

        public void SetUpgrades(bool O2 = false, bool LandSupport = false)
        {
            Upgrade_02 = O2;
            Upgrade_Land = LandSupport;
        }

        [ProtoMember(6)]
        [NonSerialized]
        public bool Upgrade_02 = false;
        [ProtoMember(4)]
        [NonSerialized]
        public bool Upgrade_Land = false;
    }
}