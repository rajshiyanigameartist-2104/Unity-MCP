namespace Match3.Data
{
    public enum TileType
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange,
        Any
    }

    public enum SpecialTileType
    {
        None,
        RocketHorizontal,
        RocketVertical,
        Bomb,
        ColorBomb
    }

    public enum GoalType
    {
        Score,
        ClearColor
    }
}
