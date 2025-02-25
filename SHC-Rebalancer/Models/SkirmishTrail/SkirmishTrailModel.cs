namespace SHC_Rebalancer;
public class SkirmishTrailModel
{
    public string? MapNameAddress { get; set; }

    public string? MapName { get; set; }

    public SkirmishDifficulty? Difficulty { get; set; }

    public SkirmishMode? Type { get; set; }

    public int? NumberOfPlayers => (AIs?.Length > 1 ? AIs.Skip(1).Count(x => (int)x > 0) : 0) + 1;

    public AI[] AIs
    {
        get => _ais;
        set => _ais = (value?.Length == 8) ? value : ResizeArray(value, 8);
    }
    private AI[] _ais = new AI[8];

    public int[] Locations
    {
        get => _locations;
        set => _locations = (value?.Length == 8) ? value : ResizeArray(value, 8);
    }
    private int[] _locations = new int[8];

    public SkirmishTeam[] Teams
    {
        get => _teams;
        set => _teams = (value?.Length == 8) ? value : ResizeArray(value, 8);
    }
    private SkirmishTeam[] _teams = new SkirmishTeam[8];

    public int[] AIVs
    {
        get => _aivs;
        set => _aivs = (value?.Length == 8) ? value : ResizeArray(value, 8);
    }
    private int[] _aivs = new int[8];

    private static T[] ResizeArray<T>(T[]? input, int size)
    {
        T[] newArray = new T[size];
        if (input != null)
            Array.Copy(input, newArray, Math.Min(input.Length, size));
        return newArray;
    }
}
