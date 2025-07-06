using System;
using System.Collections.Generic;
using VehicleFramework.UpgradeTypes;

public abstract class EchelonModuleFamily<T> : EchelonBaseModule
    where T : EchelonModuleFamily<T>
{
    protected EchelonModuleFamily(EchelonModule module, CraftingNode groupNode) : base(module, groupNode)
    {
    }
    private static Dictionary<TechType, T> Family { get; } = new Dictionary<TechType, T>();
    public static IReadOnlyDictionary<TechType, T> RegisteredFamily => Family;

    public override IReadOnlyCollection<TechType> AutoDisplace => Family.Keys;

    public static bool IsAny(TechType tt) => Family.ContainsKey(tt);
    public override TechType Register()
    {
        var type = base.Register();

        Family[type] = (T)this;
        return type;
    }

    public static TechType FindRegisteredFamilyMemberTechType(Func<T, bool> predicate)
    {
        foreach (var family in RegisteredFamily)
            if (predicate(family.Value))
                return family.Key;
        return TechType.None;
    }
}