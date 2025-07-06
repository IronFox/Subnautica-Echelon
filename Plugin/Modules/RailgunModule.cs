
using Subnautica_Echelon;
using System.Collections.Generic;
using VehicleFramework.Assets;
using VehicleFramework.UpgradeTypes;

public class RailgunModule : EchelonModuleFamily<RailgunModule>
{
    public static CraftingNode RailgunGroupNode => new CraftingNode
    {
        displayName = Language.main.Get("group_Railgun"),
        icon = railgunSprite,
        name = $"echelonrailgunupgrades"
    };
    public override QuickSlotType QuickSlotType => QuickSlotType.Toggleable;



    internal static Atlas.Sprite railgunSprite;
    public static int LevelOf(EchelonModule module)
    {
        switch (module)
        {
            case EchelonModule.RailgunMk1:
                return 0;
            case EchelonModule.RailgunMk2:
                return 1;
            case EchelonModule.RailgunMk3:
                return 2;
        }
        return -1;
    }

    internal static void RegisterAll()
    {
        railgunSprite = Subnautica_Echelon.MainPatcher.LoadSprite("images/Railgun.png");
        new RailgunModule(EchelonModule.RailgunMk1).Register();
        new RailgunModule(EchelonModule.RailgunMk2).Register();
        //new RailgunModule(EchelonModule.RailgunMk3).Register();   //not for noew
    }


    public static EchelonModule GetFrom(Echelon echelon)
    {
        return echelon
            .HighestModuleType(
                EchelonModule.RailgunMk1,
                EchelonModule.RailgunMk2,
                EchelonModule.RailgunMk3);
    }


    public RailgunModule(EchelonModule module) : base(module, RailgunGroupNode)
    { }


    //public override CraftTree.Type FabricatorType => Mk == 0 ? CraftTree.Type.Fabricator : CraftTree.Type.Workbench;


    public override List<Ingredient> Recipe
    {
        get
        {
            switch (Module)
            {
                case EchelonModule.RailgunMk1:
                    return new List<Ingredient>
                    {
                        new Ingredient(TechType.TitaniumIngot, 1),
                        new Ingredient(TechType.Aerogel, 2),
                        new Ingredient(TechType.AdvancedWiringKit, 2),
                        new Ingredient(TechType.Magnetite, 3),
                    };
                case EchelonModule.RailgunMk2:
                    return new List<Ingredient>
                    {
                        new Ingredient(GetTechTypeOf(EchelonModule.RailgunMk1), 1),
                        new Ingredient(TechType.PlasteelIngot, 1),
                        new Ingredient(TechType.Kyanite, 1),
                        new Ingredient(TechType.PrecursorIonCrystal, 2),
                        new Ingredient(TechType.Polyaniline, 1),
                    };
                default:
                    return new List<Ingredient>();
            }
        }
    }


}