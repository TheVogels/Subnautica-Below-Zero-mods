//#define Extra
//#define Unused
#pragma warning disable CS0612
namespace Seamoth.Patches;
using HarmonyLib;
using System;
//using System.Reflection;
//using UnityEngine;
//using UWE;

[HarmonyPatch(typeof(SeaMoth), nameof(SeaMoth.Update))]
public static class Seamoth_Patch
{

    [HarmonyPostfix]
    public static void Postfix(SeaMoth __instance)
    {
        try
        {
            if (!__instance)
                return;
            SeaMoth Seamoth = __instance;
            if (!Seamoth.dockable)
            {
                Dockable dock = Seamoth.gameObject.AddComponent<Dockable>();
                dock.vehicle = Seamoth;
                dock.timeUndocked = Seamoth.timeUndocked;
                dock.docked = Seamoth.docked;
                dock.rb = Seamoth.useRigidbody;
                Seamoth.dockable = dock;
                //Seamoth.dockable = Seamoth.GetComponent<Dockable>();
                //if((VehicleDockingBay.))
                //Seamoth.dockable.SetDocked();
            }
            //Fix camera
            if (Seamoth.GetPilotingMode())
            {
                Seamoth.SetPlayerInside(false);
                //Seamoth.mainAnimator.SetFloat("view_yaw", Seamoth.steeringWheelYaw * 70f);
                //Seamoth.mainAnimator.SetFloat("view_pitch", Seamoth.steeringWheelPitch * 45f);
            }
            if(Seamoth.handLabel == "Placeholder")
            Seamoth.handLabel = "Enter Seamoth";
            //Seamoth.useRigidbody.
            if(Seamoth.welcomeNotification.text == "Placeholder")
            Seamoth.welcomeNotification.text = "Seamoth: Welcome aboard captain.";
            if(Seamoth.noPowerWelcomeNotification.text == "Placeholder")
            Seamoth.noPowerWelcomeNotification.text = "Seamoth: Warning: Emergency power only. Oxygen production offline.";
        }
        //Just don't crash.
        catch (Exception)
        {
        }
    }
}
[HarmonyPatch(typeof(SeaMoth))]
[HarmonyPatch("IInteriorSpace.CanBreathe")]
public static class Seamoth_Oxygen_Patch
{
    [HarmonyPrefix]
    public static bool Prefix( ref bool __result, SeaMoth __instance)
    {
        __result = __instance.HasEnoughEnergy(__instance.oxygenEnergyCost);
        return false;
    }
}

[HarmonyPatch(typeof(SeamothStorageInput), nameof(SeamothStorageInput.OnHandHover))]
public static class SeamothStorage_Patch
{

    [HarmonyPrefix]
    public static bool Prefix(SeamothStorageInput __instance)
    {
        try
        {
            HandReticle main = HandReticle.main;
            main.SetText(HandReticle.TextType.Hand, "Open storage", true, GameInput.Button.LeftHand);
            main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false, GameInput.Button.None);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        //Just don't crash.
        catch (Exception)
        {
            return true;
        }
        return false;
    }
}
[HarmonyPatch(typeof(GenericHandTarget), nameof(GenericHandTarget.OnHandHover))]
public static class SeamothTorpedo_Patch
{

    [HarmonyPrefix]
    public static bool Prefix(GenericHandTarget __instance)
    {
        try
        {
            if (!__instance.name.ToLower().Contains("torpedosilo"))
                return true;
            if (__instance.enabled && __instance.onHandHover != null)
            {
                HandReticle main = HandReticle.main;
                main.SetText(HandReticle.TextType.Hand, "Access torpedo bay", true, GameInput.Button.LeftHand);
                main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false, GameInput.Button.None);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            }
        }
        //Just don't crash.
        catch (Exception)
        {
            return true;
        }
        return false;
    }
}

[HarmonyPatch(typeof(CraftTree), nameof(CraftTree.ConstructorScheme))]
public static class Seamoth_Constructor_Patch
{

    [HarmonyPrefix]
    public static bool Prefix(ref CraftNode __result)
    {
        try
        {
            __result = new CraftNode("Root", TreeAction.None, TechType.None).AddNode(new CraftNode[]
            {
                new CraftNode("Vehicles", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
                {
                    new CraftNode("SeaTruck", TreeAction.Craft, TechType.SeaTruck),
                    new CraftNode("SeaMoth", TreeAction.Craft, TechType.Seamoth),
                    new CraftNode("Exosuit", TreeAction.Craft, TechType.Exosuit)
                }),
                #if Extra
                new CraftNode("Joy", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
                {
                    new CraftNode("Ballon", TreeAction.Craft, TechType.WeatherBalloon),
                    new CraftNode("Toy Car", TreeAction.Craft, TechType.ToyCar),
                }),
                //new CraftNode("Experiments", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
                //{
                    //new CraftNode("Rocket Stage #1", TreeAction.Craft, TechType.RocketStage1),
                //}),
                #endif
                new CraftNode("Modules", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
                {
                    new CraftNode("SeaTruckAquariumModule", TreeAction.Craft, TechType.SeaTruckAquariumModule),
                    new CraftNode("SeaTruckDockingModule", TreeAction.Craft, TechType.SeaTruckDockingModule),
                    new CraftNode("SeaTruckFabricatorModule", TreeAction.Craft, TechType.SeaTruckFabricatorModule),
                    new CraftNode("SeaTruckSleeperModule", TreeAction.Craft, TechType.SeaTruckSleeperModule),
                    new CraftNode("SeaTruckStorageModule", TreeAction.Craft, TechType.SeaTruckStorageModule),
                    new CraftNode("SeaTruckTeleportationModule", TreeAction.Craft, TechType.SeaTruckTeleportationModule)
                })
            });
        }
        //Just don't crash.
        catch (Exception)
        {
            return true;
        }
        return false;
    }
}
[HarmonyPatch(typeof(CraftTree), nameof(CraftTree.SeamothUpgradesScheme))]
public static class Seamoth_Upgrade_Patch
{

    [HarmonyPrefix]
    public static bool Prefix(ref CraftNode __result)
    {
        try
        {
            __result = new CraftNode("Root", TreeAction.None, TechType.None).AddNode(new CraftNode[]
        {
            new CraftNode("CommonModules", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
            {
                new CraftNode("VehicleStorageUpgrade", TreeAction.Craft, TechType.VehicleStorageModule),
                new CraftNode("VehicleArmorPlating", TreeAction.Craft, TechType.VehicleArmorPlating),
                new CraftNode("VehiclePowerUpgradeModule", TreeAction.Craft, TechType.VehiclePowerUpgradeModule),
            }),
            new CraftNode("SeamothModules", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
            {
                new CraftNode("SeamothTorpedoModule", TreeAction.Craft, TechType.SeamothTorpedoModule),
                new CraftNode("SeamothSolarCharge", TreeAction.Craft, TechType.SeamothSolarCharge),
                new CraftNode("SeamothElectricalDefense", TreeAction.Craft, TechType.SeamothElectricalDefense),
                new CraftNode("SeamothSonarModule", TreeAction.Craft, TechType.SeamothSonarModule),
                new CraftNode("VehicleHullModule1", TreeAction.Craft, TechType.VehicleHullModule1),
                new CraftNode("WorkBench", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
                {
                    new CraftNode("VehicleHullModule2", TreeAction.Craft, TechType.VehicleHullModule2),
                    new CraftNode("VehicleHullModule3", TreeAction.Craft, TechType.VehicleHullModule3),
                }),
                #if Unused
                new CraftNode("Scrapped", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
                {
                     new CraftNode("SeamothReinforcementModule", TreeAction.Craft, TechType.SeamothReinforcementModule),
                     new CraftNode("LootSensorMetal", TreeAction.Craft, TechType.LootSensorMetal),
                     new CraftNode("LootSensorLithium", TreeAction.Craft, TechType.LootSensorLithium),
                     new CraftNode("LootSensorFragment", TreeAction.Craft, TechType.LootSensorFragment),
                 }),
                 #endif
            }),
            new CraftNode("ExosuitModules", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
            {
                new CraftNode("ExoHullModule1", TreeAction.Craft, TechType.ExoHullModule1),
                new CraftNode("ExosuitThermalReactorModule", TreeAction.Craft, TechType.ExosuitThermalReactorModule),
                new CraftNode("ExosuitJetUpgradeModule", TreeAction.Craft, TechType.ExosuitJetUpgradeModule),
                new CraftNode("ExosuitPropulsionArmModule", TreeAction.Craft, TechType.ExosuitPropulsionArmModule),
                new CraftNode("ExosuitGrapplingArmModule", TreeAction.Craft, TechType.ExosuitGrapplingArmModule),
                new CraftNode("ExosuitDrillArmModule", TreeAction.Craft, TechType.ExosuitDrillArmModule),
                new CraftNode("ExosuitTorpedoArmModule", TreeAction.Craft, TechType.ExosuitTorpedoArmModule),
                #if Unused
                new CraftNode("Scrapped", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
                {
                     new CraftNode("ExosuitClawArmModule", TreeAction.Craft, TechType.ExosuitClawArmModule),
                }),
                #endif
            }),
            new CraftNode("SeaTruckUpgrade", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
            {
                new CraftNode("SeaTruckUpgradeAfterburner", TreeAction.Craft, TechType.SeaTruckUpgradeAfterburner),
                new CraftNode("SeaTruckUpgradeEnergyEfficiency", TreeAction.Craft, TechType.SeaTruckUpgradeEnergyEfficiency),
                new CraftNode("SeaTruckUpgradeHorsePower", TreeAction.Craft, TechType.SeaTruckUpgradeHorsePower),
                new CraftNode("SeaTruckUpgradeHull1", TreeAction.Craft, TechType.SeaTruckUpgradeHull1),
                new CraftNode("SeaTruckUpgradePerimeterDefense", TreeAction.Craft, TechType.SeaTruckUpgradePerimeterDefense),
                #if Unused
                new CraftNode("Scrapped", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
                {
                     new CraftNode("SeaTruckUpgradeThruster", TreeAction.Craft, TechType.SeaTruckUpgradeThruster),
                }),
                #endif
            }),
            new CraftNode("Torpedoes", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
            {
                new CraftNode("WhirlpoolTorpedo", TreeAction.Craft, TechType.WhirlpoolTorpedo),
                new CraftNode("GasTorpedo", TreeAction.Craft, TechType.GasTorpedo)
            })
            #if Extra
            ,new CraftNode("Deployables", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
            {
                new CraftNode("Seamoth", TreeAction.Craft, TechType.Seamoth),
                new CraftNode("Exosuit", TreeAction.Craft, TechType.Exosuit),
                new CraftNode("SeaTruck", TreeAction.Craft, TechType.SeaTruck),
                new CraftNode("Modules", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
                {
                new CraftNode("SeaTruckAquariumModule", TreeAction.Craft, TechType.SeaTruckAquariumModule),
                new CraftNode("SeaTruckDockingModule", TreeAction.Craft, TechType.SeaTruckDockingModule),
                new CraftNode("SeaTruckFabricatorModule", TreeAction.Craft, TechType.SeaTruckFabricatorModule),
                new CraftNode("SeaTruckSleeperModule", TreeAction.Craft, TechType.SeaTruckSleeperModule),
                new CraftNode("SeaTruckStorageModule", TreeAction.Craft, TechType.SeaTruckStorageModule),
                #if Unused
                new CraftNode("Scrapped", TreeAction.Expand, TechType.None).AddNode(new CraftNode[]
                {
                    new CraftNode("SeaTruckPlanterModule", TreeAction.Craft, TechType.SeaTruckPlanterModule),
                }),
                #endif
                })
            })
            #endif
        });
        }
        //Just don't crash.
        catch (Exception)
        {
            return true;
        }
        return false;
    }
}

