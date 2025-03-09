using Discord;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework.Assets;
using VehicleFramework.UpgradeTypes;

namespace Subnautica_Echelon
{
    public class NuclearBatteryModule : EchelonModuleFamily<NuclearBatteryModule>
    {
        public static Atlas.Sprite batterySprite;


        public static CraftingNode BatteryGroupNode => new CraftingNode
        {
            displayName = $"Nuclear Battery",
            icon = batterySprite,
            name = $"echelonnuclearbatteryupgrades"
        };

        private int Mk
        {
            get
            {
                switch (Module)
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
        }

        public override string ClassId => $"EchelonNuclearBatteryModule{Mk}";  //must stay for backwards compatibility
        public override string DisplayName => $"Echelon Nuclear Battery Upgrade {Module.ToString().Substring(14)}";

        public override string Description
        {
            get
            {
                switch (Module)
                {
                    case EchelonModule.NuclearBatteryMk1:
                        return $"Doubles the energy output of the Echelon's nuclear battery (Mk1). Does not stack";
                    case EchelonModule.NuclearBatteryMk2:
                        return $"Quadruples the energy output of the Echelon's nuclear battery (Mk2). Does not stack";

                }
                return "Unsupported battery type: " + Module;
            }
        }


        internal static void RegisterAll()
        {
            batterySprite = MainPatcher.LoadSprite("images/nuclearBattery.png");
            new NuclearBatteryModule(EchelonModule.NuclearBatteryMk1).Register();
            new NuclearBatteryModule(EchelonModule.NuclearBatteryMk2).Register();
            //new NuclearBatteryModule(EchelonModule.NuclearBatteryMk3).Register();
        }

        public NuclearBatteryModule(EchelonModule module) : base(module, BatteryGroupNode)
        {}




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
