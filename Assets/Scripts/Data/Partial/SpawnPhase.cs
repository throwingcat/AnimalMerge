using System.Collections.Generic;
using Violet;

namespace SheetData
{
    public partial class SpawnPhase
    {
        public static SpawnPhase DefaultPhase;

        public List<double> Phase = new List<double>();

        public override void Initialize()
        {
            base.Initialize();

            if (Index == 1)
                DefaultPhase = this;

            if (0 < Unit1)
                Phase.Add(Unit1);
            if (0 < Unit2)
                Phase.Add(Unit2);
            if (0 < Unit3)
                Phase.Add(Unit3);
            if (0 < Unit4)
                Phase.Add(Unit4);
            if (0 < Unit5)
                Phase.Add(Unit5);
            if (0 < Unit6)
                Phase.Add(Unit6);
            if (0 < Unit7)
                Phase.Add(Unit7);
            if (0 < Unit8)
                Phase.Add(Unit8);
            if (0 < Unit9)
                Phase.Add(Unit9);
        }

        public SpawnPhase GetNextPhase()
        {
            return TableManager.Instance.GetData<SpawnPhase>(GrowKey);
        }
    }
}