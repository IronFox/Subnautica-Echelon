using System.Collections.Generic;
using UnityEngine;
using VehicleFramework.UpgradeTypes;

namespace Subnautica_Echelon
{
    public class DriveModule : EchelonModuleFamily<DriveModule>
    {
        public static Sprite? driveSprite;

        public static CraftingNode BatteryGroupNode => new CraftingNode
        {
            displayName = Language.main.Get("group_Drive"),
            icon = driveSprite!,
            name = $"echelondriveupgrades"
        };


        public DriveModule(EchelonModule module) : base(module, BatteryGroupNode)
        {
        }

        public static void RegisterAll()
        {
            driveSprite = Subnautica_Echelon.MainPatcher.LoadSprite("images/drive.png");
            new DriveModule(EchelonModule.DriveMk1).Register();
            new DriveModule(EchelonModule.DriveMk2).Register();
            new DriveModule(EchelonModule.DriveMk3).Register();
        }



        public static float GetSpeedBoost(EchelonModule module)
        {
            switch (module)
            {
                case EchelonModule.DriveMk1:
                    return 0.9f;
                case EchelonModule.DriveMk2:
                    return 1.7f;
                case EchelonModule.DriveMk3:
                    return 2.5f;
            }
            return 0.3f;

        }

        public static EchelonModule GetFrom(Echelon echelon)
        {
            return echelon
                .HighestModuleType(
                    EchelonModule.DriveMk1,
                    EchelonModule.DriveMk2,
                    EchelonModule.DriveMk3);
        }


        public override string Description
                => string.Format(base.Description, Mathf.RoundToInt(GetSpeedBoost(Module) * 100));



        public override List<Ingredient> Recipe
        {
            get
            {
                switch (Module)
                {
                    case EchelonModule.DriveMk1:
                        return new List<Ingredient>
                    {
                        new Ingredient(TechType.WiringKit, 2),
                        new Ingredient(TechType.Copper, 2),
                        new Ingredient(TechType.ComputerChip, 1),
                        new Ingredient(TechType.Silicone, 1),
                    };
                    case EchelonModule.DriveMk2:
                        return new List<Ingredient>
                        {
                        new Ingredient(GetTechTypeOf(EchelonModule.DriveMk1), 1),
                        new Ingredient(TechType.AdvancedWiringKit, 2),
                        new Ingredient(TechType.Magnetite, 2),
                        new Ingredient(TechType.Aerogel, 2),
                    };
                    case EchelonModule.DriveMk3:
                        return new List<Ingredient>
                        {
                        new Ingredient(GetTechTypeOf(EchelonModule.DriveMk2), 1),
                        new Ingredient(TechType.Nickel, 2),
                        new Ingredient(TechType.PrecursorIonCrystal, 1),
                        new Ingredient(TechType.Kyanite, 1),
                    };
                    default:
                        return new List<Ingredient>();
                }
            }
        }
    }
}
