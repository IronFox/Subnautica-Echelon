using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Assets;
using VehicleFramework.UpgradeTypes;

namespace Subnautica_Echelon
{
    public class RepairModule : EchelonModuleFamily<RepairModule>
    {
        public static Atlas.Sprite groupSprite;


        public static CraftingNode SelfRepairGroupNode => new CraftingNode
        {
            displayName = Language.main.Get("group_RepairModule"),
            icon = groupSprite,
            name = $"echelonrepairmoduleupgrades"
        };

        public RepairModule(EchelonModule module)
            : base(module, SelfRepairGroupNode)
        {
        }

        public static void RegisterAll()
        {
            groupSprite = Subnautica_Echelon.MainPatcher.LoadSprite("images/repairModule.png");
            new RepairModule(EchelonModule.RepairModuleMk1).Register();
            new RepairModule(EchelonModule.RepairModuleMk2).Register();
            new RepairModule(EchelonModule.RepairModuleMk3).Register();
        }


        public override string Description => string.Format(base.Description, Math.Round(GetRelativeSelfRepair(Module) * 100, 1));
            


        public static float GetRelativeSelfRepair(EchelonModule module)
        {
            switch (module)
            {
                case EchelonModule.RepairModuleMk1:
                    return 0.0025f;
                case EchelonModule.RepairModuleMk2:
                    return 0.005f;
                case EchelonModule.RepairModuleMk3:
                    return 0.01f;
            }
            return 0f;

        }

        public static EchelonModule GetFrom(Echelon echelon)
        {
            return echelon
                .HighestModuleType(
                    EchelonModule.RepairModuleMk1,
                    EchelonModule.RepairModuleMk2,
                    EchelonModule.RepairModuleMk3);
        }


        public override List<Ingredient> Recipe
        {
            get
            {
                switch (Module)
                {
                    case EchelonModule.RepairModuleMk1:
                        return new List<Ingredient>
                    {
                        new Ingredient( TechType.Welder, 1 ),
                        new Ingredient(TechType.ComputerChip, 1),
                    };
                    case EchelonModule.RepairModuleMk2:
                        return new List<Ingredient>
                        {
                        new Ingredient(GetTechTypeOf(EchelonModule.RepairModuleMk1), 1),
                        new Ingredient(TechType.Welder, 1),
                        new Ingredient(TechType.AdvancedWiringKit, 2),
                        new Ingredient(TechType.Magnetite, 2),
                    };
                    case EchelonModule.RepairModuleMk3:
                        return new List<Ingredient>
                        {
                        new Ingredient(GetTechTypeOf(EchelonModule.RepairModuleMk2), 1),
                        new Ingredient(TechType.Welder, 1),
                        new Ingredient(TechType.PrecursorIonCrystal, 1),
                        new Ingredient(TechType.Polyaniline, 2),
                        new Ingredient(TechType.Nickel, 2),
                    };
                    default:
                        return new List<Ingredient>();
                }
            }
        }
    }
}
