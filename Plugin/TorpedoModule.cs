
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

public class TorpedoModule : EchelonBaseModule
{
    public int Mk { get; }

    public static CraftingNode TorpedoGroupNode => new CraftingNode
    {
        displayName = $"Torpedoes",
        icon = torpedoSprite,
        name = $"echelontorpedoupgrades"
    };



    internal static Atlas.Sprite torpedoSprite;

    public override string ClassId => $"EchelonTorpedoModule{Mk}";

    public override string DisplayName => $"Echelon Torpedo System Mk {Mk+1}";

    public override string Description
    {
        get
        {
            switch (Mk)
            {
                case 0:
                    return $"Adds basic torpedo deployment capabilities to the Echelon (Mk {Mk + 1}). Does not stack";
                case 1:
                    return $"Adds advanced torpedo deployment capabilities to the Echelon (Mk {Mk + 1}). Does not stack";
                case 2:
                    return $"Adds superior torpedo deployment capabilities to the Echelon (Mk {Mk + 1}). Does not stack";

            }
            return "Unsupported mark "+Mk;
        }


    }

    public static EchelonModule MarkToModule(int mk)
    {
        switch (mk)
        {
            case 0:
                return EchelonModule.TorpedoMk1;
            case 1:
                return EchelonModule.TorpedoMk2;
            case 2:
                return EchelonModule.TorpedoMk3;
            default:
                throw new ArgumentException($"Mk is out of range [0,2]", nameof(mk));
        }
    }

    /// <summary>
    /// 3 mks (0-2)
    /// </summary>
    /// <param name="mk"></param>
    public TorpedoModule(int mk): base(MarkToModule(mk), TorpedoGroupNode)
    {
        Mk = mk;
    }


    //public override CraftTree.Type FabricatorType => Mk == 0 ? CraftTree.Type.Fabricator : CraftTree.Type.Workbench;


    public override List<Ingredient> Recipe
    {
        get
        {
            switch (Mk)
            {
                case 0:
                    return new List<Ingredient>
                    {
                        new Ingredient(TechType.Titanium, 3),
                        new Ingredient(TechType.CrashPowder, 1),
                        new Ingredient(TechType.ComputerChip, 1),
                    };
                case 1:
                    return new List<Ingredient>
                    {
                        new Ingredient(Mk1Type, 1),
                        new Ingredient(TechType.Aerogel, 2),
                        new Ingredient(TechType.Sulphur, 2),
                        new Ingredient(TechType.Magnetite, 2),
                    };
                case 2:
                    return new List<Ingredient>
                    {
                        new Ingredient(Mk2Type, 1),
                        new Ingredient(TechType.Kyanite, 2),
                        new Ingredient(TechType.Lithium, 1),
                        new Ingredient(TechType.Polyaniline, 2),
                    };
                default:
                    return new List<Ingredient>();
            }
        }
    }


    public static TechType Mk1Type { get; internal set; }
    public static TechType Mk2Type { get; internal set; }
    public static TechType Mk3Type { get; internal set; }
}