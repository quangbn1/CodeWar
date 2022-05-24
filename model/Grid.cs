
using Sfs2X.Entities.Data;

namespace bot {
    public class Grid
    {
        private List<Gem> gems = new List<Gem>();
        private ISFSArray gemsCode;
        public HashSet<GemType> gemTypes = new HashSet<GemType>();
        private HashSet<GemType> myHeroGemType;

        public Grid(ISFSArray gemsCode,ISFSArray gemModifiers,  HashSet<GemType> gemTypes)
        {
            updateGems(gemsCode, gemModifiers);
            this.myHeroGemType = gemTypes;
        }

        public void updateGems(ISFSArray gemsCode, ISFSArray gemModifiers) {
            gems.Clear();
            gemTypes.Clear();
            for (int i = 0; i < gemsCode.Size(); i++) {
                Gem gem = new Gem(i, (GemType)gemsCode.GetByte(i) , gemModifiers != null ? (GemModifier)gemModifiers.GetByte(i) : GemModifier.NONE);
                gems.Add(gem);
                gemTypes.Add(gem.type);
            }
        }
        
        public List<GemSwapInfo> GetSuggestMatch()
        {
            return suggestMatch();;
        }

        public Pair<int> recommendSwapGem(Player botPlayer, Player enemyPlayer) {
            List<GemSwapInfo> listMatchGem = suggestMatch();

            Console.WriteLine("recommendSwapGem " + listMatchGem.Count);
            if (listMatchGem.Count == 0) {
                return new Pair<int>(-1, -1);
            }

            // //Check 5
            // GemSwapInfo matchGemSizeThanFour = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4).FirstOrDefault();
            // if (matchGemSizeThanFour != null) {
            //     return matchGemSizeThanFour.getIndexSwapGem();
            // }
            
            //Combo level 3

            //Gem Level 3 + Has Exp
            GemSwapInfo matchThreeComboExp = listMatchGem.Where(gemMatch => (gemMatch.level>2) && gemMatch.hasExpGem == true).FirstOrDefault();
            if (matchThreeComboExp != null) {
                Console.WriteLine("  -----+++ Choose Gems: "+ matchThreeComboExp.level);
                return matchThreeComboExp.getIndexSwapGem();
            }

            //Gem Level 3 + Has Sword
            GemSwapInfo matchThreeComboSword = listMatchGem.Where(gemMatch => (gemMatch.level>2) && gemMatch.hasSwordGem == true).FirstOrDefault();
            if (matchThreeComboSword != null) {
                Console.WriteLine("  -----+++ Choose Gems: "+ matchThreeComboSword.level);
                return matchThreeComboSword.getIndexSwapGem();
            }

            //Gem Level 3 + Has Modifier
            GemSwapInfo matchThreeComboModifier = listMatchGem.Where(gemMatch => (gemMatch.level>2) && gemMatch.hasModifeGem == true).FirstOrDefault();
            if (matchThreeComboModifier != null) {
                Console.WriteLine("  -----+++ Choose Gems: "+ matchThreeComboModifier.level);
                return matchThreeComboModifier.getIndexSwapGem();
            }
            
            //Match combo level 3
            GemSwapInfo matchThreeCombo = CheckMoreNeedCombo(3);
            if(matchThreeCombo!=null)
            {
                Console.WriteLine("  -----+++ Choose NORMAL Gems: ");
                Console.WriteLine("  -----+++ Choose Gems: "+ matchThreeCombo.level);
                return matchThreeCombo.getIndexSwapGem();
            }


            //Test
            //Find gem modifier
            foreach (GemSwapInfo matchGem in listMatchGem)
            {
                if(matchGem.gemModifier!=GemModifier.NONE && matchGem.gemModifier!=GemModifier.POINT)
                {
                    return matchGem.getIndexSwapGem();
                }
            }

            //Combo level 2

            //Gem Level 2 + Has Exp
            GemSwapInfo matchTowComboExp = listMatchGem.Where(gemMatch => (gemMatch.level>1) && gemMatch.hasExpGem == true).FirstOrDefault();
            if (matchTowComboExp != null) {
                Console.WriteLine("  -----+++ Choose EXP Gems: ");
                Console.WriteLine("  -----+++ Choose Gems: "+ matchTowComboExp.level);
                return matchTowComboExp.getIndexSwapGem();
            }

            //Gem Level 2 + Has Sword
            GemSwapInfo matchTwoComboSword = listMatchGem.Where(gemMatch => (gemMatch.level>1) && gemMatch.hasSwordGem == true).FirstOrDefault();
            if (matchTwoComboSword != null) {
                Console.WriteLine("  -----+++ Choose SWORD Gems: ");
                Console.WriteLine("  -----+++ Choose Gems: "+ matchTwoComboSword.level);
                return matchTwoComboSword.getIndexSwapGem();
            }

            //Will kill first hero has high atk or can kill my hero
            GemSwapInfo matchSwordToKill = listMatchGem.Where(gemMatch => gemMatch.type == GemType.SWORD).FirstOrDefault();

            if(matchSwordToKill!=null)
            {
                if (enemyPlayer.firstHeroAlive().GetAtk()>=15 || 
                    enemyPlayer.firstHeroAlive().GetAtk()>=botPlayer.firstHeroAlive().GetHp() ) {
                Console.WriteLine(" ==+----- Will kill first hero has high atk or can kill my hero > ");
                    return matchSwordToKill.getIndexSwapGem();
                }
            }

            //Gem Level 2 + Has Modifier
            GemSwapInfo matchTwoComboModifier = listMatchGem.Where(gemMatch => (gemMatch.level>1) && gemMatch.hasModifeGem == true).FirstOrDefault();
            if (matchTwoComboModifier != null) {
                Console.WriteLine("  -----+++ Choose MODIFIER Gems: ");
                Console.WriteLine("  -----+++ Choose Gems: "+ matchTwoComboModifier.level);
                return matchTwoComboModifier.getIndexSwapGem();
            }

            //Match combo level 2
            GemSwapInfo matchTwoCombo = CheckMoreNeedCombo(2);
            if(matchTwoCombo!=null)
            {
                Console.WriteLine("  -----+++ NORMAL: "+ matchTwoCombo.level);
                Console.WriteLine("  -----+++ Choose Gems: "+ matchTwoCombo.level);
                return matchTwoCombo.getIndexSwapGem();
            }
            
Console.WriteLine("Gem");
foreach (GemType type in myHeroGemType)
{
    Console.WriteLine(type);
}
Console.WriteLine("Gem");
            //Just get 4 match for recommend match gems
            foreach (GemType type in myHeroGemType)
            {
                //Check 4
                List<GemSwapInfo> matchGemSizeThanThrees = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 3).ToList();

                foreach (var matchGem in matchGemSizeThanThrees)
                {
                    if (matchGem != null) {
                        return matchGem.getIndexSwapGem();
                    }
                }
            }

            //Check 3 sword
            GemSwapInfo matchGemSword = listMatchGem.Where(gemMatch => gemMatch.type == GemType.SWORD).FirstOrDefault();

            if (matchGemSword != null) {
                return matchGemSword.getIndexSwapGem();
            }

            //Get gem for hero high mana
            foreach (Hero hero in botPlayer.heroes)
            {
                if(hero.isAlive() && !hero.isFullMana() && hero.NeedMana()<=3)
                {
                    foreach (GemType gt in hero.gemTypes)
                    {
                        GemSwapInfo matchGem = listMatchGem.Where(gemMatch => gemMatch.type == gt).FirstOrDefault();
                        //listMatchGem.stream().filter(gemMatch -> gemMatch.getType() == type).findFirst();

                        if (matchGem != null) {
                            return matchGem.getIndexSwapGem();
                        }
                    }
                }
            }

            foreach (GemType type in myHeroGemType) {
                
                GemSwapInfo matchGem = listMatchGem.Where(gemMatch => gemMatch.type == type).FirstOrDefault();
                        //listMatchGem.stream().filter(gemMatch -> gemMatch.getType() == type).findFirst();
                if (matchGem != null) {
                    return matchGem.getIndexSwapGem();
                }
            }
            return listMatchGem[0].getIndexSwapGem();
        }

        public GemSwapInfo CheckMoreNeedCombo(int level = 2)
        {
            List<GemSwapInfo> listMatchGem = suggestMatch();
            level--;

            if(listMatchGem==null) return null;
            
            //Check more need
            List<GemSwapInfo> matchCombo = listMatchGem.Where(gemMatch => (gemMatch.level>level)).ToList();
            HashSet<GemType> gemNeeds = GetMyHeroGemsType();
            GemSwapInfo moreNeedSwapGem = null;
            int maxCountNeed = 0;

            foreach (GemSwapInfo match in matchCombo)
            {
                int countNeed = 0;
                foreach (GemType type in gemNeeds)
                {
                    foreach (GemType matchGemType in match.gemType)
                    {
                        if(matchGemType == type)
                        {
                            countNeed++;

                            if(countNeed>maxCountNeed)
                            {
                                maxCountNeed = countNeed;
                                moreNeedSwapGem = match;
                            }
                        }
                    }
                }
            }

            if(moreNeedSwapGem!=null && maxCountNeed>0)
            {
                Console.WriteLine("  -----+++ Has Need: "+ maxCountNeed);
                Console.WriteLine("  -----+++ Choose Gems Level: "+ moreNeedSwapGem.level);
                return moreNeedSwapGem;
            }

            return null;
        }

        public void UpdateMyGemType(HashSet<GemType> gemType)
        {
            myHeroGemType = gemType;
        }

        public HashSet<GemType> GetMyHeroGemsType()
        {
            return myHeroGemType;
        }

        public List<Gem> getGems()
        {
            return gems;
        }

//SIMULATION 
        public Grid(List<Gem> gemsMap)
        {
            this.gems = new List<Gem>(gemsMap);
        }

        public GemSwapInfo UpdateLevel(List<Gem> gemsMap, GemSwapInfo gemSwap)
        {
            Grid simulaGrid = new Grid(gemsMap);
            return simulaGrid.DownGems(gemSwap);
        }

        public GemSwapInfo DownGems(GemSwapInfo swapGem)
        {
            GemSwapInfo result = null;

            Gem temp1 = new Gem(swapGem.getIndexSwapGem().param1, GemType.BLUE,GemModifier.NONE);
            Gem temp2 = new Gem(swapGem.getIndexSwapGem().param2, GemType.BLUE,GemModifier.NONE);

            Gem current = gems[getGemIndexAt(temp1.x, temp1.y)];
            Gem target = gems[getGemIndexAt(temp2.x, temp2.y)];
            //current.isDestroy = true;


            CustomSwap(current, target);

            // for (int y = 0; y < 8; y++)
            // {
            //     for (int x = 0; x < 8; x++)
            //     {
            //         if(gems[getGemIndexAt(x, y)].isDestroy)
            //         {
            //             Console.Write("- ");
            //         }
            //         else
            //         {
            //             Console.Write((int) gems[getGemIndexAt(x, y)].type +" ");
            //         }
            //     }
            //     Console.WriteLine(" ");
            // }
            //     Console.WriteLine(" ");
            
            //DROP
            result = Drop(gems, swapGem);

            // current.isDestroy = false;
            // target.isDestroy = false;
            CustomSwap(current, target);

            return result;
        }

        public GemSwapInfo Drop(List<Gem> gemsMap, GemSwapInfo swapGem)
        {
            List<Gem> result =  new List<Gem>(gemsMap);
            
            if(swapGem.gems.Count>0)
            {
                foreach (Gem gem in (swapGem.gems))
                {
                    swapGem.gemType.Add(gem.type);
                }
            }

            //Safe Lock
            if(swapGem.level>7)
            {
                return swapGem;
            }
            
            //Set destroy type
            foreach (Gem gem in swapGem.gems)
            {
                gem.isDestroy = true;
            }

            //Find count by column
            List<int> countX =  new List<int>();

            for (int x = 0; x < 8; x++)
            {
                int count = 0;
                for (int y = 0; y < 8; y++)
                {
                    if(result[getGemIndexAt(x, y)].isDestroy)
                    {
                        count++;
                    }
                }

                countX.Add(count);
            }

            List<Gem> rowGem = new List<Gem>();
            //Find min of column
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Gem gem = result[getGemIndexAt(x,y)];

                    if(gem.isDestroy)
                    {
                        rowGem.Add(gem);
                        break;
                    }
                }
            }

            Console.WriteLine("  -----------  ");

            foreach (var item in countX)
            {
                Console.Write(item+" ");
            }
            Console.WriteLine("");
            Console.WriteLine("  -----------  ");

            //Drop gem
            foreach (Gem row in rowGem)
            {
                List<Gem> newColumn = DownGem(row);

                if(newColumn!=null)
                {
                    for (int y = 0; y < newColumn.Count; y++)
                    {
                        result[getGemIndexAt(row.x, y)] = newColumn[y];
                    }
                }

                // for (int y = 0; y < 8; y++)
                // {
                //     for (int x = 0; x < 8; x++)
                //     {
                //         if(result[getGemIndexAt(x, y)].isDestroy)
                //         {
                //             Console.Write("- ");
                //         }
                //         else
                //         {
                //             Console.Write((int) result[getGemIndexAt(x, y)].type +" ");
                //         }
                //     }
                //     Console.WriteLine(" ");
                // }
                // Console.WriteLine(" ");
            }

            foreach (Gem gem in swapGem.gems)
            {
                gem.isDestroy = false;
                gem.isDisable = true;
            }

            // Console.WriteLine("  -----------  ");

            // foreach (var item in countX)
            // {
            //     Console.Write(item+" ");
            // }
            // Console.WriteLine("  -----------  ");

    
            //Check any match
            int countCombo = 0;
            int matchSize = 1;

            List<Gem> resultMatch = new List<Gem>();
            List<Gem> preMatch = new List<Gem>();

            //Check column
            for (int x = 0; x < 8; x++)
            {
                bool isFirst = true;

                if(preMatch.Count<3)
                {
                    preMatch.Clear();
                    matchSize = 1;
                }

                for (int y = 0; y < 7; y++)
                {
                    Gem currentGem = result[getGemIndexAt(x,y)];
                    Gem nextGem = result[getGemIndexAt(x,y+1)];

                    if(preMatch.Count<=0)
                    {
                        if(currentGem.isDisable)
                        {
                            preMatch.Clear();
                            
                            matchSize = 1;
                            isFirst = true;
                            continue;
                        }
                        preMatch.Add(currentGem);
                    }

                    if(currentGem.type == nextGem.type && nextGem.isDisable == false && currentGem.isDisable == false)
                    {
                        matchSize++;

                        preMatch.Add(nextGem);

                        if(!isFirst)
                        {
                            continue;
                        }

                        if(matchSize>=3)
                        {
                            countCombo++;
                            isFirst = false;
                        }
                    }
                    else
                    {
                        if(preMatch.Count>2)
                        {
                            resultMatch.AddRange(preMatch);
                        }

                        preMatch.Clear();
                        
                        matchSize = 1;
                        isFirst = true;
                    }
                }
            }
            preMatch.Clear();
            
            //Check row
            for (int y = 0; y < 8; y++)
            {
                if(preMatch.Count<3)
                {
                    preMatch.Clear();
                    matchSize = 1;
                }
                bool isFirst = true;
                for (int x = 0; x < 7; x++)
                {
                    Gem currentGem = result[getGemIndexAt(x,y)];
                    Gem nextGem = result[getGemIndexAt(x+1,y)];

                    if(preMatch.Count<=0)
                    {
                        if(currentGem.isDisable)
                        {
                            preMatch.Clear();
                            
                            matchSize = 1;
                            isFirst = true;
                            continue;
                        }
                        preMatch.Add(currentGem);
                    }

                    if(currentGem.type == nextGem.type && nextGem.isDisable == false && currentGem.isDisable == false)
                    {
                        matchSize++;

                        preMatch.Add(nextGem);

                        if(!isFirst)
                        {
                            continue;
                        }

                        if(matchSize>=3)
                        {
                            countCombo++;
                            isFirst = false;
                        }
                    }
                    else
                    {
                        if(preMatch.Count>2)
                        {
                            resultMatch.AddRange(preMatch);
                        }

                        preMatch.Clear();
                        
                        matchSize = 1;
                        isFirst = true;
                    }
                }
            }

            preMatch.Clear();
                
            // for (int y = 0; y < 8; y++)
            // {
            //     for (int x = 0; x < 8; x++)
            //     {
            //         if(result[getGemIndexAt(x, y)].isDestroy)
            //         {
            //             Console.Write("- ");
            //         }
            //         else if(result[getGemIndexAt(x, y)].isDisable)
            //         {
            //             Console.Write("= ");
            //         }
            //         else
            //         {
            //             Console.Write((int) result[getGemIndexAt(x, y)].type +" ");
            //         }
            //     }
            //     Console.WriteLine(" ");
            // }
            // Console.WriteLine(" ==================== ");
            
            //Check gems
            foreach (Gem gem in swapGem.gems)
            {
                if(gem.modifier!=GemModifier.NONE && gem.modifier != GemModifier.POINT)
                {
                    if(gem.modifier == GemModifier.EXTRA_TURN)
                    {
                        swapGem.hasExtraGem = true;
                    }

                    if(gem.modifier == GemModifier.EXPLODE_HORIZONTAL || 
                       gem.modifier == GemModifier.EXPLODE_SQUARE || 
                       gem.modifier == GemModifier.EXPLODE_VERTICAL)
                    {
                        swapGem.hasExpGem = true;
                    }

                    swapGem.hasModifeGem = true;
                }

                if(gem.type == GemType.SWORD)
                {
                    swapGem.hasSwordGem = true;
                }
            }

            //Drop again
            if(countCombo>0)
            {
                Console.WriteLine("Combo --- "+ countCombo);
                swapGem.UpdateLevel(swapGem.level, countCombo);
                
                GemSwapInfo newSwapGem = new GemSwapInfo(swapGem.getIndexSwapGem().param1, swapGem.getIndexSwapGem().param2, swapGem.sizeMatch, swapGem.type);
                newSwapGem.gems.Clear();
                newSwapGem.gems.AddRange(resultMatch);
                newSwapGem.level = swapGem.level;
                //Get infor
                newSwapGem.hasExtraGem = swapGem.hasExtraGem;
                newSwapGem.hasExpGem = swapGem.hasExpGem;
                newSwapGem.hasModifeGem = swapGem.hasModifeGem;
                newSwapGem.hasSwordGem = swapGem.hasSwordGem;

                //Check hor-ver gem
                //PRe destroy
                foreach (Gem gem in newSwapGem.gems)
                {
                    gem.isDisable = true;
                }

                foreach (Gem gem in newSwapGem.gems.ToList())
                {
                    if(gem.modifier == GemModifier.EXPLODE_HORIZONTAL)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            if(result[getGemIndexAt(x, gem.y)].isDisable == false)
                            {
                                newSwapGem.gems.Add(result[getGemIndexAt(x, gem.y)]);
                            }
                        }
                    }

                    if(gem.modifier == GemModifier.EXPLODE_VERTICAL)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            if(result[getGemIndexAt(gem.x, y)].isDisable == false)
                            {
                                newSwapGem.gems.Add(result[getGemIndexAt(gem.x, y)]);
                            }
                        }
                    }

                    if(gem.modifier == GemModifier.EXPLODE_SQUARE)
                    {
                        int startX = gem.x>0?gem.x-1:gem.x;
                        int endX = gem.x<7?gem.x+1:gem.x;
                        int startY = gem.y>0?gem.y-1:gem.x;
                        int endY = gem.y<7?gem.x+1:gem.x;

                        for (int x = startX; x <= endX; x++)
                        {
                            for (int y = startY; y <= endY; y++)
                            {
                                if(result[getGemIndexAt(x, y)].isDisable == false)
                                {
                                    newSwapGem.gems.Add(result[getGemIndexAt(x, y)]);
                                }
                            }
                        }
                    }
                }

                //Undo
                foreach (Gem gem in newSwapGem.gems)
                {
                    gem.isDisable = false;
                }

                GemSwapInfo newSwapGem2 = Drop(result, newSwapGem);

                //Get infor
                swapGem.level = newSwapGem2.level;
                swapGem.hasExtraGem = newSwapGem2.hasExtraGem;
                swapGem.hasExpGem = newSwapGem2.hasExpGem;
                swapGem.hasModifeGem = newSwapGem2.hasModifeGem;
                swapGem.hasSwordGem = newSwapGem2.hasSwordGem;

                foreach (GemType type in newSwapGem2.gemType)
                {
                    swapGem.gemType.Add(type);
                }
            }

            //Check warning


            foreach (Gem disableGem in swapGem.gems)
            {
                disableGem.isDisable = false;
            }
            

            result.Clear();

            Console.WriteLine(" ++++Gem Type++++ ");
            foreach (GemType type in swapGem.gemType)
            {
                Console.Write(type+" ");
            }
            Console.WriteLine(" ++++++++++++++++ ");
//RETURN
            return swapGem;
        }

        public List<Gem> DownGem(Gem gem)
        {
            List<Gem> result = new List<Gem>();
            List<Gem> ableGems = new List<Gem>();
            List<Gem> disableGems = new List<Gem>();

            for (int i = 0; i < 8; i++)
            {
                Gem g = gems[getGemIndexAt(gem.x, i)];

                if(g.isDisable || g.isDestroy)
                {
                    disableGems.Add(g);
                }
                else
                {
                    ableGems.Add(g);
                }
            }

            //Add able gems first
            foreach (Gem g in ableGems)
            {
                result.Add(g);
            }

            //Add destroy gems next
            foreach (Gem g in disableGems)
            {
                result.Add(gem);
            }
            return result;
        }
        
        private void CustomSwap(Gem a, Gem b) {
            int tempIndex = a.index;
            int tempX = a.x;
            int tempY = a.y;

            // update reference
            gems[a.index] = b;
            gems[b.index] = a;

            // update data of element
            a.index = b.index;
            a.x = b.x;
            a.y = b.y;

            b.index = tempIndex;
            b.x = tempX;
            b.y = tempY;
        }
//END
    public List<GemSwapInfo> mathsGem = new List<GemSwapInfo>();

        private List<GemSwapInfo> suggestMatch() {

            if(mathsGem.Count>0)
            {
                return mathsGem;
            }

            var listMatchGem = new List<GemSwapInfo>();

            var tempGems = new List<Gem>(gems);
            foreach (Gem currentGem in tempGems) {
                Gem swapGem = null;
                // If x > 0 => swap left & check
                if (currentGem.x > 0) {
                    swapGem = gems[getGemIndexAt(currentGem.x - 1, currentGem.y)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
                // If x < 7 => swap right & check
                if (currentGem.x < 7) {
                    swapGem = gems[getGemIndexAt(currentGem.x + 1, currentGem.y)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
                // If y < 7 => swap up & check
                if (currentGem.y < 7) {
                    swapGem = gems[getGemIndexAt(currentGem.x, currentGem.y + 1)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
                // If y > 0 => swap down & check
                if (currentGem.y > 0) {
                    swapGem = gems[getGemIndexAt(currentGem.x, currentGem.y - 1)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
            }

            mathsGem.AddRange(listMatchGem);

            return listMatchGem;
        }
        private void checkMatchSwapGem(List<GemSwapInfo> listMatchGem, Gem currentGem, Gem swapGem) {
            swap(currentGem, swapGem);
            HashSet<Gem> matchGems = matchesAt(currentGem.x, currentGem.y);

            GemModifier currGemModifier = GemModifier.NONE;

            foreach (Gem gem in matchGems)
            {
                if(gem.modifier!=GemModifier.NONE)
                {
                    currGemModifier = gem.modifier;
                    break;
                }
            }

            swap(currentGem, swapGem);
            if (matchGems.Count > 0) {
                GemSwapInfo gemSwapInfo = new GemSwapInfo(currentGem.index, swapGem.index, matchGems.Count, currentGem.type);
                gemSwapInfo.AddGemModifier(currGemModifier);

                foreach (Gem gem in matchGems)
                {
                    gemSwapInfo.gems.Add(gem);
                }
                // if(!isFirst)
                // {
                //     gemSwapInfo = UpdateLevel(gems, gemSwapInfo);
                    
                // Console.WriteLine("  ----- Level Gems: "+ gemSwapInfo.level);
                //     isFirst = true;
                // }
                gemSwapInfo.level++;
                gemSwapInfo = UpdateLevel(gems, gemSwapInfo);
                Console.WriteLine("  ----- Level Gems: "+ gemSwapInfo.level);

                listMatchGem.Add(gemSwapInfo);
            }
        }
        bool isFirst = false;

        private int getGemIndexAt(int x, int y) {
            return x + y * 8;
        }

        private void swap(Gem a, Gem b) {
            int tempIndex = a.index;
            int tempX = a.x;
            int tempY = a.y;

            // update reference
            gems[a.index] = b;
            gems[b.index] = a;

            // update data of element
            a.index = b.index;
            a.x = b.x;
            a.y = b.y;

            b.index = tempIndex;
            b.x = tempX;
            b.y = tempY;
        }

        // private HashSet<Gem> CustomMatchesAt(int x, int y) {
        //     HashSet<Gem> res = new HashSet<Gem>();
        //     Gem center = gemAt(x, y);
        //     if (center == null  || center.isDisable || center.isDestroy) {
        //         return res;
        //     }

        //     // check horizontally
        //     List<Gem> hor = new List<Gem>();
        //     hor.Add(center);
        //     int xLeft = x - 1, xRight = x + 1;
        //     while (xLeft >= 0) {
        //         Gem gemLeft = gemAt(xLeft, y);
        //         if (gemLeft != null) {
        //             if (!gemLeft.sameType(center) || gemLeft.isDisable || gemLeft.isDestroy) {
        //                 break;
        //             }
        //             hor.Add(gemLeft);
        //         }
        //         xLeft--;
        //     }
        //     while (xRight < 8) {
        //         Gem gemRight = gemAt(xRight, y);
        //         if (gemRight != null) {
        //             if (!gemRight.sameType(center) || gemRight.isDisable || gemRight.isDestroy) {
        //                 break;
        //             }
        //             hor.Add(gemRight);
        //         }
        //         xRight++;
        //     }
        //     if (hor.Count >= 3) res.UnionWith(hor);

        //     // check vertically
        //     List<Gem> ver = new List<Gem>();
        //     ver.Add(center);
        //     int yBelow = y - 1, yAbove = y + 1;
        //     while (yBelow >= 0) {
        //         Gem gemBelow = gemAt(x, yBelow);
        //         if (gemBelow != null) {
        //             if (!gemBelow.sameType(center) || gemBelow.isDisable || gemBelow.isDestroy) {
        //                 break;
        //             }
        //             ver.Add(gemBelow);
        //         }
        //         yBelow--;
        //     }
        //     while (yAbove < 8) {
        //         Gem gemAbove = gemAt(x, yAbove);
        //         if (gemAbove != null) {
        //             if (!gemAbove.sameType(center) || gemAbove.isDisable || gemAbove.isDestroy) {
        //                 break;
        //             }
        //             ver.Add(gemAbove);
        //         }
        //         yAbove++;
        //     }
        //     if (ver.Count >= 3) res.UnionWith(ver);

        //     return res;
        // }

        private HashSet<Gem> matchesAt(int x, int y) {
            HashSet<Gem> res = new HashSet<Gem>();
            Gem center = gemAt(x, y);
            if (center == null) {
                return res;
            }

            // check horizontally
            List<Gem> hor = new List<Gem>();
            hor.Add(center);
            int xLeft = x - 1, xRight = x + 1;
            while (xLeft >= 0) {
                Gem gemLeft = gemAt(xLeft, y);
                if (gemLeft != null) {
                    if (!gemLeft.sameType(center)) {
                        break;
                    }
                    hor.Add(gemLeft);
                }
                xLeft--;
            }
            while (xRight < 8) {
                Gem gemRight = gemAt(xRight, y);
                if (gemRight != null) {
                    if (!gemRight.sameType(center)) {
                        break;
                    }
                    hor.Add(gemRight);
                }
                xRight++;
            }
            if (hor.Count >= 3) res.UnionWith(hor);

            // check vertically
            List<Gem> ver = new List<Gem>();
            ver.Add(center);
            int yBelow = y - 1, yAbove = y + 1;
            while (yBelow >= 0) {
                Gem gemBelow = gemAt(x, yBelow);
                if (gemBelow != null) {
                    if (!gemBelow.sameType(center)) {
                        break;
                    }
                    ver.Add(gemBelow);
                }
                yBelow--;
            }
            while (yAbove < 8) {
                Gem gemAbove = gemAt(x, yAbove);
                if (gemAbove != null) {
                    if (!gemAbove.sameType(center)) {
                        break;
                    }
                    ver.Add(gemAbove);
                }
                yAbove++;
            }
            if (ver.Count >= 3) res.UnionWith(ver);

            return res;
        }

        // Find Gem at Position (x, y)
        private Gem gemAt(int x, int y) {
            foreach (Gem g in gems) {
                if (g != null && g.x == x && g.y == y) {
                    return g;
                }
            }
            return null;
        }
    }
}