
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using Subnautica_Echelon;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VehicleFramework;
using VehicleFramework.Assets;
using VehicleFramework.UpgradeTypes;

public class TorpedoModule : EchelonModuleFamily<TorpedoModule>
{
    public static CraftingNode TorpedoGroupNode => new CraftingNode
    {
        displayName = $"Torpedoes",
        icon = torpedoSprite,
        name = $"echelontorpedoupgrades"
    };



    internal static Atlas.Sprite torpedoSprite;
    private int Mk
    {
        get
        {
            switch (Module)
            {
                case EchelonModule.TorpedoMk1:
                    return 0;
                case EchelonModule.TorpedoMk2:
                    return 1;
                case EchelonModule.TorpedoMk3:
                    return 2;
            }
            return -1;
        }
    }
    public override string ClassId => $"EchelonTorpedoModule{Mk}";  //must stay for backwards compatibility
    public override string DisplayName => $"Echelon Torpedo System {Module.ToString().Substring(7)}";

    public override string Description
    {
        get
        {
            switch (Module)
            {
                case EchelonModule.TorpedoMk1:
                    return $"Adds basic torpedo deployment capabilities to the Echelon (Mk1). Does not stack";
                case EchelonModule.TorpedoMk2:
                    return $"Adds advanced torpedo deployment capabilities to the Echelon (Mk2). Does not stack";
                case EchelonModule.TorpedoMk3:
                    return $"Adds superior torpedo deployment capabilities to the Echelon (Mk3). Does not stack";

            }
            return "Unsupported module: "+Module;
        }


    }

    internal static void RegisterAll()
    {
        torpedoSprite = Subnautica_Echelon.MainPatcher.LoadSprite("images/torpedo.png");
        new TorpedoModule(EchelonModule.TorpedoMk1).Register();
        new TorpedoModule(EchelonModule.TorpedoMk2).Register();
        new TorpedoModule(EchelonModule.TorpedoMk3).Register();
    }

    /// <summary>
    /// 3 mks (0-2)
    /// </summary>
    /// <param name="mk"></param>
    public TorpedoModule(EchelonModule module): base(module, TorpedoGroupNode)
    {}


    //public override CraftTree.Type FabricatorType => Mk == 0 ? CraftTree.Type.Fabricator : CraftTree.Type.Workbench;


    public override List<Ingredient> Recipe
    {
        get
        {
            switch (Module)
            {
                case EchelonModule.TorpedoMk1:
                    return new List<Ingredient>
                    {
                        new Ingredient(TechType.Titanium, 3),
                        new Ingredient(TechType.CrashPowder, 1),
                        new Ingredient(TechType.ComputerChip, 1),
                    };
                case EchelonModule.TorpedoMk2:
                    return new List<Ingredient>
                    {
                        new Ingredient(GetTechTypeOf(EchelonModule.TorpedoMk1), 1),
                        new Ingredient(TechType.Aerogel, 2),
                        new Ingredient(TechType.Sulphur, 2),
                        new Ingredient(TechType.Magnetite, 2),
                    };
                case EchelonModule.TorpedoMk3:
                    return new List<Ingredient>
                    {
                        new Ingredient(GetTechTypeOf(EchelonModule.TorpedoMk2), 1),
                        new Ingredient(TechType.Kyanite, 2),
                        new Ingredient(TechType.PrecursorIonCrystal, 1),
                        new Ingredient(TechType.Polyaniline, 2),
                    };
                default:
                    return new List<Ingredient>();
            }
        }
    }


}