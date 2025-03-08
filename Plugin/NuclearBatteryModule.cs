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

        public int Mk { get; }

        public static CraftingNode BatteryGroupNode => new CraftingNode
        {
            displayName = $"Nuclear Battery",
            icon = batterySprite,
            name = $"echelonnuclearbatteryupgrades"
        };


        public override string ClassId => $"EchelonNuclearBatteryModule{Mk}";
        public override string DisplayName => $"Echelon Nuclear Battery Upgrade Mk {Mk + 1}";

        public override string Description
        {
            get
            {
                switch (Mk)
                {
                    case 0:
                        return $"Doubles the energy output of the Echelon's nuclear battery (Mk {Mk + 1}). Does not stack";
                    case 1:
                        return $"Quadruples the energy output of the Echelon's nuclear battery (Mk {Mk + 1}). Does not stack";

                }
                return "Unsupported mark " + Mk;
            }
        }


        public static EchelonModule MarkToModule(int mk)
        {
            switch (mk)
            {
                case 0:
                    return EchelonModule.NuclearBatteryMk1;
                case 1:
                    return EchelonModule.NuclearBatteryMk2;
                default:
                    throw new ArgumentException($"Mk is out of range [0,1]", nameof(mk));
            }
        }


        /// <summary>
        /// 2 mks (0-1)
        /// </summary>
        /// <param name="mk"></param>
        public NuclearBatteryModule(int mk) : base(MarkToModule(mk), BatteryGroupNode)
        {
            Mk = mk;
        }




        public override List<Ingredient> Recipe
        {
            get
            {
                switch (Mk)
                {
                    case 0:
                        return new List<Ingredient>
                    {
                        new Ingredient(TechType.Lead, 4),
                        new Ingredient(TechType.UraniniteCrystal, 3),
                        new Ingredient(TechType.Copper, 2),
                        new Ingredient(TechType.ComputerChip, 1),
                    };
                    case 1:
                        return new List<Ingredient>
                        {
                        new Ingredient(FindRegisteredFamilyMemberTechType(x => x.Mk == 0), 1),
                        new Ingredient(TechType.UraniniteCrystal, 9),
                        new Ingredient(TechType.AdvancedWiringKit, 2),
                        new Ingredient(TechType.Magnetite, 2),
                        new Ingredient(TechType.Kyanite, 2),
                    };
                    default:
                        return new List<Ingredient>();
                }
            }
        }
    }


}
