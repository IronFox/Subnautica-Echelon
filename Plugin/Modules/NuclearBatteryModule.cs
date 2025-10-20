using System.Collections.Generic;
using UnityEngine;
using VehicleFramework.UpgradeTypes;

namespace Subnautica_Echelon
{
    public class NuclearBatteryModule : EchelonModuleFamily<NuclearBatteryModule>
    {
        public static Sprite? batterySprite;


        public static CraftingNode BatteryGroupNode => new CraftingNode
        {
            displayName = Language.main.Get($"group_NuclearBattery"),
            icon = batterySprite!,
            name = $"echelonnuclearbatteryupgrades"
        };

        public static int LevelOf(EchelonModule module)
        {
            switch (module)
            {
                case EchelonModule.NuclearBatteryMk1:
                    return 0;
                case EchelonModule.NuclearBatteryMk2:
                    return 1;
                case EchelonModule.NuclearBatteryMk3:
                    return 2;
            }
            return -1;
        }

        public override string ClassId => $"EchelonNuclearBatteryModule{LevelOf(Module)}";  //must stay for backwards compatibility


        internal static void RegisterAll()
        {
            batterySprite = MainPatcher.LoadSprite("images/nuclearBattery.png");
            new NuclearBatteryModule(EchelonModule.NuclearBatteryMk1).Register();
            new NuclearBatteryModule(EchelonModule.NuclearBatteryMk2).Register();
            //new NuclearBatteryModule(EchelonModule.NuclearBatteryMk3).Register();
        }


        public static EchelonModule GetFrom(Echelon echelon)
        {
            return echelon
                .HighestModuleType(
                    EchelonModule.NuclearBatteryMk1,
                    EchelonModule.NuclearBatteryMk2,
                    EchelonModule.NuclearBatteryMk3);
        }

        public NuclearBatteryModule(EchelonModule module) : base(module, BatteryGroupNode)
        { }




        public override List<Ingredient> Recipe
        {
            get
            {
                switch (Module)
                {
                    case EchelonModule.NuclearBatteryMk1:
                        return new List<Ingredient>
                    {
                        new Ingredient(TechType.Lead, 4),
                        new Ingredient(TechType.UraniniteCrystal, 3),
                        new Ingredient(TechType.Copper, 2),
                        new Ingredient(TechType.ComputerChip, 1),
                    };
                    case EchelonModule.NuclearBatteryMk2:
                        return new List<Ingredient>
                        {
                        new Ingredient(GetTechTypeOf(EchelonModule.NuclearBatteryMk1), 1),
                        new Ingredient(TechType.UraniniteCrystal, 9),
                        new Ingredient(TechType.AdvancedWiringKit, 2),
                        new Ingredient(TechType.Magnetite, 2),
                        new Ingredient(TechType.Nickel, 2),
                    };
                    default:
                        return new List<Ingredient>();
                }
            }
        }
    }


}
