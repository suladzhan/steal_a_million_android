using System;
using System.Collections.Generic;

namespace StealAMillion.Core
{
    public static class LevelGenerator
    {
        public static LevelData Generate(int level, int seed, int world, int chunk = 0, bool bonus = false)
        {
            var random = new Random(unchecked(seed + chunk * 11939));
            var segments = new List<string> { "Cash", "Cash" };
            string[] library = level < 8 ? new[] { "Cash", "Choice", "Tax", "Police", "Power" }
                : level < 15 ? new[] { "Cash", "Choice", "Tax", "Police", "Power", "Split", "Moving", "Investment", "Keys" }
                : new[] { "Cash", "Choice", "Tax", "Police", "Power", "Split", "Moving", "Investment", "Keys", "Thief", "Market", "Chain" };
            int count = 12 + Math.Min(6, level / 20);
            for (int i = 2; i < count; i++)
            {
                string next = bonus ? (i % 4 == 0 ? "Keys" : "Bonus") : library[random.Next(library.Length)];
                if (segments[i-1] == "Moving" || segments[i-1] == "Tax" || segments[i-1] == "Police") next = "Cash";
                if (!bonus && i == count-3 && level >= 5 && random.NextDouble()<.25) next="Jackpot";
                segments.Add(next);
            }
            segments.Add("Finish");
            return new LevelData { number=level,world=world,seed=seed,speed=7+RunnerProgress.Difficulty(level)*1.6f,segments=segments.ToArray(),name="UI_LEVEL" };
        }
        public static List<string> Validate(LevelData level)
        {
            var errors=new List<string>();
            if(level==null||level.segments==null||level.segments.Length<3){errors.Add("Missing track");return errors;}
            if(level.segments[0]!="Cash"&&level.segments[0]!="Bonus")errors.Add("Entry must allow recovery");
            if(level.segments[level.segments.Length-1]!="Finish")errors.Add("Missing finish");
            foreach(string segment in level.segments){SegmentKind parsed;if(!Enum.TryParse(segment,out parsed))errors.Add("Unknown segment: "+segment);}
            if(level.speed<6||level.speed>9)errors.Add("Unsafe forward speed");
            return errors;
        }
    }
}
