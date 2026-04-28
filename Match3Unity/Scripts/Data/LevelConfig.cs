using System;
using System.Collections.Generic;
using Match3.Data;

namespace Match3.Level
{
    [Serializable]
    public class LevelGoal
    {
        public GoalType goalType;
        public int target;
        public TileType tileType;
    }

    [Serializable]
    public class LevelConfig
    {
        public int levelId;
        public int width = 8;
        public int height = 8;
        public int moveLimit = 25;
        public int seed;
        public List<LevelGoal> goals = new();
    }

    [Serializable]
    public class LevelCollection
    {
        public List<LevelConfig> levels = new();
    }
}
