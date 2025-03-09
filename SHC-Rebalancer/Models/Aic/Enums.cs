namespace SHC_Rebalancer;

public enum AI
{
    All, /// special case for all AI
    Rat,
    Snake,
    Pig,
    Wolf,
    Saladin,
    Caliph,
    Sultan,
    Richard,
    Frederick,
    Phillip,
    Wazir,
    Emir,
    Nizar,
    Sheriff,
    Marshal,
    Abbot,
}

public enum BlacksmithSetting
{
    Maces = 21,
    Swords = 22,
    Both = -999,
}

public enum FletcherSetting
{
    Bows = 17,
    Crossbows = 18,
    Both = -999,
}

public enum HarassingUnit
{
    None,
    Catapult = 190,
    FireBallista = 358,
}

public enum LordType
{
    Europ,
    Arab,
}

public enum PoleturnerSetting
{
    Spears = 19,
    Pikes = 20,
    Both = -999,
}

public enum TargetChoice
{
    Gold = 0,
    Balanced = 1,
    Closest = 2,
    Player = 4
}

public enum WallDecoration
{
    Oil = 8,
    Flames = 9,
    Flag1 = 10,
    Flag2 = 11,
    Flag3 = 12,
    Flag4 = 13,
    Brazier = 14,
    Skull = 15,
    Steam = 21,
    Disease = 22,
    Explosions = 26,
    Crows = 28,
}
