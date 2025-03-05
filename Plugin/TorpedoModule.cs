
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

public class TorpedoModule : ModVehicleUpgrade
{
    public int Mk { get; }
    public EchelonModule Module { get; }

    //public readonly float Frequency;
    //public readonly float ExplosionRadius;
    //public readonly float Damage;

    public List<CraftingNode> craftingPath = new List<CraftingNode>()
    {
        new CraftingNode{
            displayName = $"Echelon",
            icon = Echelon.craftingSprite,
            name = $"echelonupgradetab"
            }
    };

    public CustomPrefab CustomPrefab { get; private set; }
    public TechType TechType => CustomPrefab?.Info.TechType ?? TechType.None;

    public override string ClassId => $"EchelonTorpedoModule{Mk}";

    public override string DisplayName => $"Echelon Torpedo System Mk {Mk+1}";

    public override string Description => $"Add torpedo deployment capabilities to the Echelon (Mk {Mk+1})";

    /// <summary>
    /// 3 mks (0-2)
    /// </summary>
    /// <param name="mk"></param>
    public TorpedoModule(int mk/*, Atlas.Sprite sprite*/)
    {
        Mk = mk;
        switch (mk)
        {
            case 0:
                Module = EchelonModule.TorpedoMk1;
                break;
            case 1:
                Module = EchelonModule.TorpedoMk2;
                break;
            case 2:
                Module = EchelonModule.TorpedoMk3;
                break;
            default:
                throw new ArgumentException($"Mk is out of range [0,2]",nameof(mk));
        }

        //craftingPath.Add(new CraftingNode
        //{
        //    displayName = DisplayName,
        //    name = ClassId,
        //    icon = sprite
        //});
    }
    public override void OnAdded(AddActionParams param)
    {
        var echelon = param.vehicle as Echelon;
        if (echelon == null)
        {
            Debug.LogError($"Added to incompatible vehicle {param.vehicle}");
            ErrorMessage.AddWarning("This upgrade only compatible with the Echelon!");
            return;
        }
        echelon.ChangeCounts(Module, true);
    }
    public override void OnRemoved(AddActionParams param)
    {
        var echelon = param.vehicle as Echelon;
        if (echelon == null)
        {
            return;
        }
        echelon.ChangeCounts(Module, false);
    }

    public override List<CraftingNode> CraftingPath 
    {
        get => craftingPath;
        set => craftingPath = value;
    }

    public override bool IsVehicleSpecific => true;

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