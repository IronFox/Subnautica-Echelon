
using Subnautica_Echelon;
using System.Collections.Generic;
using VehicleFramework.Assets;
using VehicleFramework.UpgradeTypes;

public class TorpedoModule : EchelonModuleFamily<TorpedoModule>
{
    public static CraftingNode TorpedoGroupNode => new CraftingNode
    {
        displayName = Language.main.Get("group_Torpedoes"),
        icon = torpedoSprite,
        name = $"echelontorpedoupgrades"
    };



    internal static Atlas.Sprite torpedoSprite;
    public static int LevelOf(EchelonModule module)
    {
        switch (module)
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
    public override string ClassId => $"EchelonTorpedoModule{LevelOf(Module)}";  //must stay for backwards compatibility

    internal static void RegisterAll()
    {
        torpedoSprite = Subnautica_Echelon.MainPatcher.LoadSprite("images/torpedo.png");
        new TorpedoModule(EchelonModule.TorpedoMk1).Register();
        new TorpedoModule(EchelonModule.TorpedoMk2).Register();
        new TorpedoModule(EchelonModule.TorpedoMk3).Register();
    }


    public static EchelonModule GetFrom(Echelon echelon)
    {
        return echelon
            .HighestModuleType(
                EchelonModule.TorpedoMk1,
                EchelonModule.TorpedoMk2,
                EchelonModule.TorpedoMk3);
    }

    /// <summary>
    /// 3 mks (0-2)
    /// </summary>
    /// <param name="mk"></param>
    public TorpedoModule(EchelonModule module) : base(module, TorpedoGroupNode)
    { }


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