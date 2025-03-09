using Subnautica_Echelon;
using System;
using System.Collections.Generic;
using UnityEngine;
using VehicleFramework.UpgradeTypes;
using System.Linq;

public abstract class EchelonBaseModule : ModVehicleUpgrade
{
    public CraftingNode GroupNode { get; }
    public EchelonModule Module { get; }
    private Atlas.Sprite icon;

    public TechType TechType { get; private set; }

    public List<CraftingNode> craftingPath;

    public virtual IReadOnlyCollection<TechType> AutoDisplace { get; }
    public override string ClassId => $"Echelon{Module}";

    public static CraftingNode RootCraftingNode { get; } = new CraftingNode
    {
        displayName = $"Echelon",
        icon = Echelon.craftingSprite,
        name = $"echelonupgradetab"
    };

    public static string GetMarkFromType(EchelonModule m)
    {
        var s = m.ToString();
        return s.Substring(s.Length - 3);

    }

    public string MarkFromType => GetMarkFromType(Module);

    public EchelonBaseModule(EchelonModule module, CraftingNode groupNode)
    {
        GroupNode = groupNode;
        Module = module;
        var path = $"images/{module}.png";
        icon = Subnautica_Echelon.MainPatcher.LoadSprite(path);
        if (icon == null)
            Debug.LogError($"Error while constructing {module} {this}: File {path} not found");

        craftingPath = new List<CraftingNode>()
        {
            RootCraftingNode,
            GroupNode
        };
    }



    public virtual TechType Register()
    {
        VehicleFramework.Admin.UpgradeCompat compat = new VehicleFramework.Admin.UpgradeCompat
        {
            skipCyclops = true,
            skipModVehicle = false,
            skipSeamoth = true,
            skipExosuit = true
        };

        var type = VehicleFramework.Admin.UpgradeRegistrar.RegisterUpgrade(this, compat).forModVehicle;
        TechType = type;
        All[type] = this;
        AllReverse[Module] = type;

        Debug.Log($"Registered module {Module} {this} as tech type {type}");

        return type;
    }

    private static Dictionary<TechType, EchelonBaseModule> All { get; } = new Dictionary<TechType, EchelonBaseModule>();
    private static Dictionary<EchelonModule, TechType> AllReverse { get; } = new Dictionary<EchelonModule, TechType>();
    public static IReadOnlyDictionary<TechType, EchelonBaseModule> Registered => All;
    public static IReadOnlyDictionary<EchelonModule, TechType> TechTypeMap => AllReverse;

    public static TechType GetTechTypeOf(EchelonModule module)
    {
        if (TechTypeMap.TryGetValue(module, out var type))
            return type;
        Debug.LogError($"Unable to retrieve tech type of echelon module {module}: not registered");
        return TechType.None;
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

        var cnt = GetNumberInstalled(echelon);
        try
        {
            //while (cnt > 1)
            //{
            //    foreach (var slot in echelon.slotIDs)
            //    {
            //        if (slot == echelon.slotIDs[param.slotID])
            //            continue;
            //        var p = echelon.modules.GetItemInSlot(slot);
            //        if (p != null)
            //        {
            //            var t = p.item.GetComponent<TechTag>();
            //            if (t != null && t.type == SelfType)
            //            {
            //                Debug.Log($"Evacuating extra self type from slot {slot}");
            //                if (!echelon.modules.RemoveItem(p.item))
            //                {
            //                    Debug.Log($"Failed remove");
            //                    continue;
            //                }
            //                Inventory.main.AddPending(p.item);
            //                Debug.Log($"Inventory moved");
            //                break;
            //            }
            //        }

            //    }
            //    cnt = GetNumberInstalled(echelon);
            //}


            foreach (var slot in echelon.slotIDs)
            {
                if (slot == echelon.slotIDs[param.slotID])
                    continue;
                var p = echelon.modules.GetItemInSlot(slot);
                if (p != null)
                {
                    var t = p.item.GetComponent<TechTag>();
                    if (t != null && AutoDisplace.Contains(t.type))
                    {
                        Debug.Log($"Evacuating extra {t.type} type from slot {slot}");
                        if (!echelon.modules.RemoveItem(p.item))
                        {
                            Debug.Log($"Failed remove");
                            continue;
                        }
                        Inventory.main.AddPending(p.item);
                        Debug.Log($"Inventory moved");
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        echelon.SetModuleCount(Module, cnt);
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