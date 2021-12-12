using System.Collections.Generic;

namespace SheetData
{
    public partial class Stage
    {
        public List<ItemInfo> Rewards = new List<ItemInfo>(); 
        public static Dictionary<string,List<Stage>> SortedByChapter =new Dictionary<string, List<Stage>>();
        public override void Initialize()
        {
            base.Initialize();
            
            if(SortedByChapter.ContainsKey(Chapter)==false)
                SortedByChapter.Add(Chapter,new List<Stage>());
            
            SortedByChapter[Chapter].Add(this);
            
            SortedByChapter[Chapter].Sort((a, b) =>
            {
                if (a.Index < b.Index) return -1;
                if (a.Index > b.Index) return 1;
                return 0;
            });

            if (Reward1.IsNullOrEmpty() == false)
                Rewards.Add(new ItemInfo(Reward1, Amount1));
            if (Reward2.IsNullOrEmpty() == false)
                Rewards.Add(new ItemInfo(Reward2, Amount3));
            if (Reward3.IsNullOrEmpty() == false)
                Rewards.Add(new ItemInfo(Reward2, Amount3));
        }

        public static List<Stage> GetStage(Chapter chapter)
        {
            if (SortedByChapter.ContainsKey(chapter.key))
                return SortedByChapter[chapter.key];
            return null;
        }
    }
}