
namespace bot {
    public class Player
    {
        public int playerId;
        public string displayName;
        public List<Hero> heroes;
        public HashSet<GemType> heroGemType;

        public Player(int playerId, string name)
        {
            this.playerId = playerId;
            this.displayName = name;

            heroes = new List<Hero>();
            heroGemType = new HashSet<GemType>();
        }

        public Hero anyHeroFullMana() {
            foreach(var hero in heroes){
                if (hero.isAlive() && hero.isFullMana()) return hero;
            }

            return null;
        }

        public List<Hero> allHeroFullMana()
        {
            List<Hero> result = new List<Hero>();

            foreach(Hero hero in heroes)
            {
                if(hero.isAlive() && hero.isFullMana()) result.Add(hero);
            }

            return result.Count>0 ? result : null;
        }

        public Hero firstHeroAlive() {
            foreach(var hero in heroes){
                if (hero.isAlive()) return hero;
            }

            return null;
        }

        public bool monkUseSkill = false;

        public bool isFirstHeroUseSkill = false;

        //public bool isFirstRevert = false;

        public bool isFirstRevert = false;

        public HashSet<GemType> getRecommendGemType() {

            heroGemType.Clear();

            // if(!heroes[0].isAlive() || heroes[0].isFullMana())
            // {
            //     isFirstRevert = true;
            // }

            if(isFirstRevert)
            {
                //Revert
                for (int i = heroes.Count-1; i >=0; i--)
                {
                    if (!heroes[i].isAlive()) continue;
                    
                    foreach(var gt in heroes[i].gemTypes){
                        heroGemType.Add((GemType)gt);
                    }
                }
            }
            else
            {
                foreach(var hero in heroes){
                    if (!hero.isAlive()) continue;
                    
                    foreach(var gt in hero.gemTypes){
                        heroGemType.Add((GemType)gt);
                    }
                }
            }

            return heroGemType;
        }

        public HashSet<GemType> getRecommendGemType(bool hasWarning = false) {

            heroGemType.Clear();

            foreach (Hero hero in heroes)
            {
                if(hero.id == HeroIdEnum.FIRE_SPIRIT && !hero.isAlive())
                {
                    hasWarning = false;
                }
            }

            if(hasWarning)
            {
                foreach (Hero hero in heroes)
                {
                    if(hero.id == HeroIdEnum.FIRE_SPIRIT)
                    {
                        foreach(var gt in hero.gemTypes)
                        {
                            heroGemType.Add((GemType)gt);
                        }
                    }
                }

                foreach (Hero hero in heroes)
                {
                    if(hero.id != HeroIdEnum.FIRE_SPIRIT)
                    {
                        foreach(var gt in hero.gemTypes)
                        {
                            heroGemType.Add((GemType)gt);
                        }
                    }
                }

                return heroGemType;
            }

            // if(!heroes[0].isAlive() || heroes[0].isFullMana())
            // {
            //     isFirstRevert = true;
            // }

            if(isFirstRevert)
            {
                //Revert
                for (int i = heroes.Count-1; i >=0; i--)
                {
                    if (!heroes[i].isAlive()) continue;
                    
                    foreach(var gt in heroes[i].gemTypes){
                        heroGemType.Add((GemType)gt);
                    }
                }
            }
            else
            {
                foreach(var hero in heroes){
                    if (!hero.isAlive()) continue;
                    
                    foreach(var gt in hero.gemTypes){
                        heroGemType.Add((GemType)gt);
                    }
                }
            }

            return heroGemType;
        }
    }
}