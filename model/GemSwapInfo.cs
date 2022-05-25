namespace bot
{
    public class GemSwapInfo 
    {
        private int index1;
        private int index2;
        public int sizeMatch;
        public GemType type;

        public GemModifier gemModifier;

        public List<Gem> gems = new List<Gem>();

        public HashSet<GemType> gemType = new HashSet<GemType>();

        public int level = 0;

        public bool hasExtraGem = false;

        public bool hasExpGem = false;

        public bool hasModifeGem = false;

        public bool hasSwordGem = false;

        public GemSwapInfo(int index1, int index2, int sizeMatch, GemType type)
        {
            this.index1 = index1;
            this.index2 = index2;
            this.sizeMatch = sizeMatch;
            this.type = type;
        }

        public void UpdateLevel(int level)
        {
            this.level = level + 1;
        }
        
        public void UpdateLevel(int level, int count)
        {
            this.level = level + count;
        }

        public void AddGemModifier(GemModifier gemModifier)
        {
            this.gemModifier = gemModifier;
        }

        public Pair<int> getIndexSwapGem() {
            return new Pair<int>(index1, index2);
        }

        public void SetIndexSwapGem(int param1, int param2)
        {
            this.index1 = param1;
            this.index2 = param2;
        }
    }
}