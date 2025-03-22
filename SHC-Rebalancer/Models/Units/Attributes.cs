namespace SHC_Rebalancer;

[AttributeUsage(AttributeTargets.Field)]
public class UnitAlwaysRunAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public class UnitDigMoatSpeedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public class UnitCostAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public class UnitMeleeDamageToBuildingsAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public class UnitMeleeDamageToTowersAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public class UnitMeleeDamageToWallsAttribute : Attribute
{
}
