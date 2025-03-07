using Subnautica_Echelon;
using System;
using System.Collections.Generic;
using UnityEngine;
using VehicleFramework.UpgradeTypes;

public abstract class EchelonBaseModule : ModVehicleUpgrade
{
    public CraftingNode GroupNode { get; }
    public EchelonModule Module { get; }
    private Atlas.Sprite icon;


    public List<CraftingNode> craftingPath;

    public static CraftingNode RootCraftingNode { get; } = new CraftingNode
    {
        displayName = $"Echelon",
        icon = Echelon.craftingSprite,
        name = $"echelonupgradetab"
    };

    public EchelonBaseModule(EchelonModule module, CraftingNode groupNode)
    {
        GroupNode = groupNode;
        Module = module;
        icon = Subnautica_Echelon.MainPatcher.LoadSprite($"images/{module}.png");

        craftingPath = new List<CraftingNode>()
        {
            RootCraftingNode,
            GroupNode
        };
    }




    public override List<CraftingNode> CraftingPath
    {
        get => craftingPath;
        set => craftingPath = value;
    }

    public override bool IsVehicleSpecific => true;

    public override void OnAdded(AddActionParams param)
    {
        var now = DateTime.Now;
        Debug.Log($"[{now:HH:mm:ss.fff}] EchelonBaseModule[{Module}].OnAdded(vehicle={param.vehicle},isAdded={param.isAdded},slot={param.slotID})");
        var echelon = param.vehicle as Echelon;
        if (echelon == null)
        {
            Debug.LogError($"Added to incompatible vehicle {param.vehicle}");
            ErrorMessage.AddWarning("This is an Echelon upgrade and will not work on other subs!");
            return;
        }
        echelon.SetModuleCount(Module, GetNumberInstalled(echelon));
    }
    public override void OnRemoved(AddActionParams param)
    {
        var echelon = param.vehicle as Echelon;
        if (echelon == null)
        {
            return;
        }
        echelon.SetModuleCount(Module, GetNumberInstalled(echelon));
    }

    public override Atlas.Sprite Icon => icon ?? base.Icon;

}