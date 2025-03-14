﻿namespace SHC_Rebalancer;
public class AicModel
{
    public LordType? LordType { get; set; }
    public double? LordStrength { get; set; }
    public PersonalityModel? Personality { get; set; }

    public class PersonalityModel
    {
        public WallDecoration? WallDecoration { get; set; }
        public int? Unknown001 { get; set; }
        public int? Unknown002 { get; set; }
        public int? Unknown003 { get; set; }
        public int? Unknown004 { get; set; }
        public int? Unknown005 { get; set; }
        public int? CriticalPopularity { get; set; }
        public int? LowestPopularity { get; set; }
        public int? HighestPopularity { get; set; }
        public int? TaxesMin { get; set; }
        public int? TaxesMax { get; set; }
        public int? Unknown011 { get; set; }
        public Building? Farm1 { get; set; }
        public Building? Farm2 { get; set; }
        public Building? Farm3 { get; set; }
        public Building? Farm4 { get; set; }
        public Building? Farm5 { get; set; }
        public Building? Farm6 { get; set; }
        public Building? Farm7 { get; set; }
        public Building? Farm8 { get; set; }
        public int? PopulationPerFarm { get; set; }
        public int? PopulationPerWoodcutter { get; set; }
        public int? PopulationPerQuarry { get; set; }
        public int? PopulationPerIronmine { get; set; }
        public int? PopulationPerPitchrig { get; set; }
        public int? MaxQuarries { get; set; }
        public int? MaxIronmines { get; set; }
        public int? MaxWoodcutters { get; set; }
        public int? MaxPitchrigs { get; set; }
        public int? MaxFarms { get; set; }
        public int? BuildInterval { get; set; }
        public int? ResourceRebuildDelay { get; set; }
        public int? MaxFood { get; set; }
        public int? MinimumApples { get; set; }
        public int? MinimumCheese { get; set; }
        public int? MinimumBread { get; set; }
        public int? MinimumWheat { get; set; }
        public int? MinimumHop { get; set; }
        public int? TradeAmountFood { get; set; }
        public int? TradeAmountEquipment { get; set; }
        public int? AIRequestDelay { get; set; }
        public int? MinimumGoodsRequiredAfterTrade { get; set; }
        public int? DoubleRationsFoodThreshold { get; set; }
        public int? MaxWood { get; set; }
        public int? MaxStone { get; set; }
        public int? MaxResourceOther { get; set; }
        public int? MaxEquipment { get; set; }
        public int? MaxBeer { get; set; }
        public int? MaxResourceVariance { get; set; }
        public int? RecruitGoldThreshold { get; set; }
        public BlacksmithSetting? BlacksmithSetting { get; set; }
        public FletcherSetting? FletcherSetting { get; set; }
        public PoleturnerSetting? PoleturnerSetting { get; set; }
        public Resource? SellResource01 { get; set; }
        public Resource? SellResource02 { get; set; }
        public Resource? SellResource03 { get; set; }
        public Resource? SellResource04 { get; set; }
        public Resource? SellResource05 { get; set; }
        public Resource? SellResource06 { get; set; }
        public Resource? SellResource07 { get; set; }
        public Resource? SellResource08 { get; set; }
        public Resource? SellResource09 { get; set; }
        public Resource? SellResource10 { get; set; }
        public Resource? SellResource11 { get; set; }
        public Resource? SellResource12 { get; set; }
        public Resource? SellResource13 { get; set; }
        public Resource? SellResource14 { get; set; }
        public Resource? SellResource15 { get; set; }
        public int? DefWallPatrolRallyTime { get; set; }
        public int? DefWallPatrolGroups { get; set; }
        public int? DefSiegeEngineGoldThreshold { get; set; }
        public int? DefSiegeEngineBuildDelay { get; set; }
        public int? Unknown072 { get; set; }
        public int? Unknown073 { get; set; }
        public int? RecruitProbDefDefault { get; set; }
        public int? RecruitProbDefWeak { get; set; }
        public int? RecruitProbDefStrong { get; set; }
        public int? RecruitProbRaidDefault { get; set; }
        public int? RecruitProbRaidWeak { get; set; }
        public int? RecruitProbRaidStrong { get; set; }
        public int? RecruitProbAttackDefault { get; set; }
        public int? RecruitProbAttackWeak { get; set; }
        public int? RecruitProbAttackStrong { get; set; }
        public int? SortieUnitRangedMin { get; set; }
        public Unit? SortieUnitRanged { get; set; }
        public int? SortieUnitMeleeMin { get; set; }
        public Unit? SortieUnitMelee { get; set; }
        public int? DefDiggingUnitMax { get; set; }
        public Unit? DefDiggingUnit { get; set; }
        public int? RecruitInterval { get; set; }
        public int? RecruitIntervalWeak { get; set; }
        public int? RecruitIntervalStrong { get; set; }
        public int? DefTotal { get; set; }
        public int? OuterPatrolGroupsCount { get; set; }
        public bool? OuterPatrolGroupsMove { get; set; }
        public int? OuterPatrolRallyDelay { get; set; }
        public int? DefWalls { get; set; }
        public Unit? DefUnit1 { get; set; }
        public Unit? DefUnit2 { get; set; }
        public Unit? DefUnit3 { get; set; }
        public Unit? DefUnit4 { get; set; }
        public Unit? DefUnit5 { get; set; }
        public Unit? DefUnit6 { get; set; }
        public Unit? DefUnit7 { get; set; }
        public Unit? DefUnit8 { get; set; }
        public int? RaidUnitsBase { get; set; }
        public int? RaidUnitsRandom { get; set; }
        public Unit? RaidUnit1 { get; set; }
        public Unit? RaidUnit2 { get; set; }
        public Unit? RaidUnit3 { get; set; }
        public Unit? RaidUnit4 { get; set; }
        public Unit? RaidUnit5 { get; set; }
        public Unit? RaidUnit6 { get; set; }
        public Unit? RaidUnit7 { get; set; }
        public Unit? RaidUnit8 { get; set; }
        public HarassingUnit? HarassingSiegeEngine1 { get; set; }
        public HarassingUnit? HarassingSiegeEngine2 { get; set; }
        public HarassingUnit? HarassingSiegeEngine3 { get; set; }
        public HarassingUnit? HarassingSiegeEngine4 { get; set; }
        public HarassingUnit? HarassingSiegeEngine5 { get; set; }
        public HarassingUnit? HarassingSiegeEngine6 { get; set; }
        public HarassingUnit? HarassingSiegeEngine7 { get; set; }
        public HarassingUnit? HarassingSiegeEngine8 { get; set; }
        public int? HarassingSiegeEnginesMax { get; set; }
        public int? RaidRetargetDelay { get; set; }
        public int? AttForceBase { get; set; }
        public int? AttForceRandom { get; set; }
        public int? AttForceSupportAllyThreshold { get; set; }
        public int? AttForceRallyPercentage { get; set; }
        public int? Unknown129 { get; set; }
        public int? AttAssaultDelay { get; set; }
        public int? AttUnitPatrolRecommandDelay { get; set; }
        public int? Unknown132 { get; set; }
        public Unit? SiegeEngine1 { get; set; }
        public Unit? SiegeEngine2 { get; set; }
        public Unit? SiegeEngine3 { get; set; }
        public Unit? SiegeEngine4 { get; set; }
        public Unit? SiegeEngine5 { get; set; }
        public Unit? SiegeEngine6 { get; set; }
        public Unit? SiegeEngine7 { get; set; }
        public Unit? SiegeEngine8 { get; set; }
        public int? CowThrowInterval { get; set; }
        public int? Unknown142 { get; set; }
        public int? AttMaxEngineers { get; set; }
        public Unit? AttDiggingUnit { get; set; }
        public int? AttDiggingUnitMax { get; set; }
        public Unit? AttUnitVanguard { get; set; }
        public int? AttUnitVanguardMax { get; set; }
        public int? AttMaxAssassins { get; set; }
        public int? AttMaxLaddermen { get; set; }
        public int? AttMaxTunnelers { get; set; }
        public Unit? AttUnitPatrol { get; set; }
        public int? AttUnitPatrolMax { get; set; }
        public int? AttUnitPatrolGroupsCount { get; set; }
        public Unit? AttUnitBackup { get; set; }
        public int? AttUnitBackupMax { get; set; }
        public int? AttUnitBackupGroupsCount { get; set; }
        public Unit? AttUnitEngage { get; set; }
        public int? AttUnitEngageMax { get; set; }
        public Unit? AttUnitSiegeDef { get; set; }
        public int? AttUnitSiegeDefMax { get; set; }
        public int? AttUnitSiegeDefGroupsCount { get; set; }
        public Unit? AttUnitMain1 { get; set; }
        public Unit? AttUnitMain2 { get; set; }
        public Unit? AttUnitMain3 { get; set; }
        public Unit? AttUnitMain4 { get; set; }
        public int? AttMaxDefault { get; set; }
        public int? AttMainGroupsCount { get; set; }
        public TargetChoice? TargetChoice { get; set; }
    }
}
