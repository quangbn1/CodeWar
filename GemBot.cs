using Sfs2X.Entities;
using Sfs2X.Entities.Data;
namespace bot
{
    public class GemBot : BaseBot
    {
        internal void Load()
        {
            Console.WriteLine("Bot.Load()");
        }

        internal void Update(TimeSpan gameTime)
        {
            Console.WriteLine("Bot.Update()");
        }

        protected override void StartGame(ISFSObject gameSession, Room room)
        {
            // Assign Bot player & enemy player
            AssignPlayers(room);

            // Player & Heroes
            ISFSObject objBotPlayer = gameSession.GetSFSObject(botPlayer.displayName);
            ISFSObject objEnemyPlayer = gameSession.GetSFSObject(enemyPlayer.displayName);

            ISFSArray botPlayerHero = objBotPlayer.GetSFSArray("heroes");
            ISFSArray enemyPlayerHero = objEnemyPlayer.GetSFSArray("heroes");

            for (int i = 0; i < botPlayerHero.Size(); i++)
            {
                var hero = new Hero(botPlayerHero.GetSFSObject(i));
                botPlayer.heroes.Add(hero);
            }

            for (int i = 0; i < enemyPlayerHero.Size(); i++)
            {
                enemyPlayer.heroes.Add(new Hero(enemyPlayerHero.GetSFSObject(i)));
            }

            // Gems
            grid = new Grid(gameSession.GetSFSArray("gems"), null, botPlayer.getRecommendGemType());
            currentPlayerId = gameSession.GetInt("currentPlayerId");
            log("StartGame ");

            // SendFinishTurn(true);
            //taskScheduler.schedule(new FinishTurn(true), new Date(System.currentTimeMillis() + delaySwapGem));
            TaskSchedule(delaySwapGem, _ => SendFinishTurn(true));
        }

        protected override void SwapGem(ISFSObject paramz)
        {
            bool isValidSwap = paramz.GetBool("validSwap");
            if (!isValidSwap)
            {
                return;
            }

            HandleGems(paramz);
        }

        protected override void HandleGems(ISFSObject paramz)
        {
            ISFSObject gameSession = paramz.GetSFSObject("gameSession");
            currentPlayerId = gameSession.GetInt("currentPlayerId");
            //get last snapshot
            ISFSArray snapshotSfsArray = paramz.GetSFSArray("snapshots");
            ISFSObject lastSnapshot = snapshotSfsArray.GetSFSObject(snapshotSfsArray.Size() - 1);
            bool needRenewBoard = paramz.ContainsKey("renewBoard");
            // update information of hero
            HandleHeroes(lastSnapshot);
            if (needRenewBoard)
            {
                grid.updateGems(paramz.GetSFSArray("renewBoard"), null);
                TaskSchedule(delaySwapGem, _ => SendFinishTurn(false));
                return;
            }
            // update gem
            grid.gemTypes = botPlayer.getRecommendGemType();

            ISFSArray gemCodes = lastSnapshot.GetSFSArray("gems");
            ISFSArray gemModifiers = lastSnapshot.GetSFSArray("gemModifiers");

            if (gemModifiers != null) log("has gemModifiers");

            grid.updateGems(gemCodes, gemModifiers);

            TaskSchedule(delaySwapGem, _ => SendFinishTurn(false));
        }

        private void HandleHeroes(ISFSObject paramz)
        {
            ISFSArray heroesBotPlayer = paramz.GetSFSArray(botPlayer.displayName);
            for (int i = 0; i < botPlayer.heroes.Count; i++)
            {
                botPlayer.heroes[i].updateHero(heroesBotPlayer.GetSFSObject(i)); 
            }
            ISFSArray heroesEnemyPlayer = paramz.GetSFSArray(enemyPlayer.displayName);
            for (int i = 0; i < enemyPlayer.heroes.Count; i++)
            {
                enemyPlayer.heroes[i].updateHero(heroesEnemyPlayer.GetSFSObject(i));
            }
        }

        protected override void StartTurn(ISFSObject paramz)
        {
            currentPlayerId = paramz.GetInt("currentPlayerId");
            if (!isBotTurn())
            {
                return;
            }

            grid.mathsGem.Clear();
            
            grid.UpdateMyGemType(botPlayer.getRecommendGemType());
            
            //Swap five gems
            var fiveGem = GetFiveGems();
            if(fiveGem.param1 !=-1 && fiveGem.param2!=-1)
            {
                SendSwapGem(fiveGem);
                return;
            }

            //Swap extra gems
            var extraGem = GetExtraGems();
            if(extraGem.param1 !=-1 && extraGem.param2!=-1)
            {
                SendSwapGem(extraGem);
                return;
            }
            
            if(HeroCastSkillFirst())
            {
                return;
            }

            //Need after skill
            var preSkill = GetPreCastSkill();
            if(preSkill.param1 !=-1 && preSkill.param2!=-1)
            {
                SendSwapGem(preSkill);
                return;
            }

            if(OnHeroCastSkill())
            {
                return;
            }

            //Need after skill
            var afterSkill = AfterCastSkil();
            if(afterSkill.param1 !=-1 && afterSkill.param2!=-1)
            {
                SendSwapGem(afterSkill);
                return;
            }

            TaskSchedule(delaySwapGem, _ => SendSwapGem());
        }
        
        public bool HeroCastSkillFirst()
        {
            List<Hero> herosFullMana = botPlayer.allHeroFullMana();

            if(herosFullMana==null) return false;

            foreach (Hero hero in herosFullMana)
            {
                if(hero.id == HeroIdEnum.SEA_SPIRIT)
                {
                    Hero target = null;
                    for (int i = botPlayer.heroes.Count-1; i >=0; i--)
                    {
                        if(botPlayer.heroes[i].isAlive())
                        {
                            target = botPlayer.heroes[i];
                            break;
                        }
                    }

                    if(target!=null)
                    {
                        TaskSchedule(delaySwapGem, _ => SendCastSkill(hero, target));
                    }
                    else
                    {
                        TaskSchedule(delaySwapGem, _ => SendCastSkill(hero));
                    }

                    return true;
                }
            }

            return false;
        }

        public bool OnHeroCastSkill()
        {
            Hero heroFullMana = botPlayer.anyHeroFullMana();

            if (heroFullMana != null)
            {
                List<Hero> herosFullMana = botPlayer.allHeroFullMana();
                foreach (Hero hero in herosFullMana)
                {
                    if(hero.id == HeroIdEnum.SEA_SPIRIT)
                    {
                        Hero target = null;
                        for (int i = botPlayer.heroes.Count-1; i >=0; i--)
                        {
                            if(botPlayer.heroes[i].isAlive())
                            {
                                target = botPlayer.heroes[i];
                                break;
                            }
                        }

                        if(target!=null)
                        {
                            TaskSchedule(delaySwapGem, _ => SendCastSkill(hero, target));
                        }
                        else
                        {
                            TaskSchedule(delaySwapGem, _ => SendCastSkill(hero));
                        }

                        return true;
                    }

                    if(hero.id == HeroIdEnum.CERBERUS)
                    {
                        TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana));
                        return true;
                    }
                    
                    if(hero.id == HeroIdEnum.FIRE_SPIRIT)
                    {
                        List<Gem> gems = grid.getGems();

                        int fireGemCount = 0;
                        foreach (var gem in gems)
                        {
                            if(gem.type == GemType.RED)
                            {
                                fireGemCount++;
                            }
                        }

                        //Kill high attack hero and has skill
                        foreach (Hero eHero in  enemyPlayer.heroes)
                        {
                            if(eHero.isAlive() && eHero.GetAtk()>=12 && eHero.isFullMana())
                            {
                                if(eHero.id == HeroIdEnum.MERMAID)
                                {
                                    TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                    return true;
                                }
                                else if(eHero.id == HeroIdEnum.AIR_SPIRIT)
                                {
                                    TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                    return true;
                                }
                                else if(eHero.id == HeroIdEnum.CERBERUS)
                                {
                                    TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                    return true;
                                }
                                else if(eHero.id == HeroIdEnum.THUNDER_GOD)
                                {
                                    TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                    return true;
                                }
                                else if(eHero.id == HeroIdEnum.SEA_GOD)
                                {
                                    TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                    return true;
                                }
                            }
                        }

                        //Kill high attack hero
                        foreach (Hero eHero in  enemyPlayer.heroes)
                        {
                            if(eHero.isAlive() && eHero.GetAtk()>=12)
                            {
                                if(eHero.id == HeroIdEnum.AIR_SPIRIT)
                                {
                                    TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                    return true;
                                }
                                else if(eHero.id == HeroIdEnum.CERBERUS)
                                {
                                    TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                    return true;
                                }
                                else if(eHero.id == HeroIdEnum.MERMAID)
                                {
                                    TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                    return true;
                                }
                                else if(eHero.id == HeroIdEnum.THUNDER_GOD)
                                {
                                    TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                    return true;
                                }
                                else if(eHero.id == HeroIdEnum.MONK)
                                {
                                    TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                    return true;
                                }
                            }
                        }

                        foreach (Hero eHero in  enemyPlayer.heroes)
                        {
                            if(eHero.id == HeroIdEnum.SEA_GOD && eHero.isAlive())
                            {
                                TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                return true;
                            }

                            if(eHero.id == HeroIdEnum.DISPATER && eHero.isAlive())
                            {
                                TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                return true;
                            }

                            if(eHero.id == HeroIdEnum.MONK && eHero.isAlive())
                            {
                                TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                return true;
                            }
                        }
                        

                        //Dung skill neu giet sap duoc muc tieu
                        for (int i = enemyPlayer.heroes.Count-1; i >=0 ; i--)
                        {
                            if(enemyPlayer.heroes[i].GetAtk() + fireGemCount >= enemyPlayer.heroes[i].GetHp() && enemyPlayer.heroes[i].isAlive())
                            {
                                TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, enemyPlayer.heroes[i]));
                                return true;
                            }
                        }

                        foreach (Hero eHero in  enemyPlayer.heroes)
                        {
                            if(eHero.id == HeroIdEnum.AIR_SPIRIT && eHero.isAlive())
                            {
                                TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                return true;
                            }

                            if(eHero.id == HeroIdEnum.CERBERUS && eHero.isAlive())
                            {
                                TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                return true;
                            }

                            if(eHero.id == HeroIdEnum.MERMAID && eHero.isAlive())
                            {
                                TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                return true;
                            }
                        }

                        foreach (Hero eHero in enemyPlayer.heroes)
                        {
                            //Dung skill neu giet duoc muc tieu
                            if(eHero.GetAtk()+fireGemCount>=15 && eHero.isAlive())
                            {
                                TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana, eHero));
                                return true;
                            }
                        }

                    }
                }

                TaskSchedule(delaySwapGem, _ => SendCastSkill(heroFullMana));
                return true;
            }

            return false;
        }

        public Pair<int> GetFiveGems()
        {
            //Check match 5 gems
            List<GemSwapInfo> listMatchGem = grid.GetSuggestMatch();

            if (listMatchGem.Count == 0) {
                return new Pair<int>(-1, -1);
            }

            GemSwapInfo matchGemSizeThanFour = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4).FirstOrDefault();
            if (matchGemSizeThanFour != null) {
                return matchGemSizeThanFour.getIndexSwapGem();
            }

            return new Pair<int>(-1, -1);
        }

        public Pair<int> GetExtraGems()
        {
            //Check match
            List<GemSwapInfo> listMatchGem = grid.GetSuggestMatch();

            if (listMatchGem.Count == 0) {
                return new Pair<int>(-1, -1);
            }

            //Gem Level + Has extra
            GemSwapInfo matchFourComboExtra = listMatchGem.Where(gemMatch => gemMatch.hasExtraGem == true).FirstOrDefault();
            if (matchFourComboExtra != null) {
                Console.WriteLine("  -----+++ Choose Gems: "+ matchFourComboExtra.level);
                return matchFourComboExtra.getIndexSwapGem();
            }

            //ExtraGems
            foreach (GemSwapInfo matchGem in listMatchGem)
            {
                if(matchGem.gemModifier==GemModifier.EXTRA_TURN)
                {
                    return matchGem.getIndexSwapGem();
                }
            }

            return new Pair<int>(-1, -1);
        }

        public Pair<int> GetPreCastSkill()
        {
            Console.WriteLine("PreSkill");
            //Check match
            List<GemSwapInfo> listMatchGem = grid.GetSuggestMatch();

            if (listMatchGem.Count == 0) {
                return new Pair<int>(-1, -1);
            }

            //Code

            //Check 3 sword to kill first hero
            GemSwapInfo matchGemSword = listMatchGem.Where(gemMatch => gemMatch.type == GemType.SWORD).FirstOrDefault();

            if (matchGemSword != null) {
                
                if(enemyPlayer.firstHeroAlive().GetHp()<=botPlayer.firstHeroAlive().GetAtk())
                {
                    if(!botPlayer.firstHeroAlive().isFullMana() && botPlayer.firstHeroAlive().id==HeroIdEnum.FIRE_SPIRIT)
                    {
                        return matchGemSword.getIndexSwapGem();
                    }
                }
            }

            //Gem Level 4 + Has exp gem
            GemSwapInfo matchFourComboExp= listMatchGem.Where(gemMatch => (gemMatch.level>3) && gemMatch.hasExpGem == true).FirstOrDefault();
            if (matchFourComboExp != null) {
                Console.WriteLine("  -----+++ Choose Gems: "+ matchFourComboExp.level);
                return matchFourComboExp.getIndexSwapGem();
            }

            //Get buff damage
            foreach (GemSwapInfo matchGem in listMatchGem)
            {
                if(matchGem.gemModifier==GemModifier.BUFF_ATTACK)
                {
                    return matchGem.getIndexSwapGem();
                }
            }

            //Check 4 sword
            GemSwapInfo matchFourGemSword = listMatchGem.Where(gemMatch => (gemMatch.type == GemType.SWORD) && gemMatch.sizeMatch>3 ).FirstOrDefault();

            if (matchFourGemSword != null) {
                return matchFourGemSword.getIndexSwapGem();
            }

            //Gem Level 4 + Has sword
            GemSwapInfo matchFourComboSword = listMatchGem.Where(gemMatch => (gemMatch.level>3) && gemMatch.hasSwordGem == true).FirstOrDefault();
            if (matchFourComboSword != null) {
                Console.WriteLine("  -----+++ Choose Gems Level: "+ matchFourComboSword.level);
                return matchFourComboSword.getIndexSwapGem();
            }

            //Gem Level 4 + Has modifier
            GemSwapInfo matchFourComboModifier = listMatchGem.Where(gemMatch => (gemMatch.level>3) && gemMatch.hasModifeGem == true).FirstOrDefault();
            if (matchFourComboModifier != null) {
                Console.WriteLine("  -----+++ Choose Gems Level: "+ matchFourComboModifier.level);
                return matchFourComboModifier.getIndexSwapGem();
            }

            //Gem Level 4
            GemSwapInfo matchFourCombo = listMatchGem.Where(gemMatch => (gemMatch.level>3)).FirstOrDefault();
            if (matchFourCombo != null) {
                Console.WriteLine("  -----+++ Choose NORMAL Gems ");
                Console.WriteLine("  -----+++ Choose Gems Level: "+ matchFourCombo.level);
                return matchFourCombo.getIndexSwapGem();
            }

            Console.WriteLine("PreSkill");
            return new Pair<int>(-1, -1);
        }

        public Pair<int> AfterCastSkil()
        {
            Console.WriteLine("AfterSkill");
            //Check match
            List<GemSwapInfo> listMatchGem = grid.GetSuggestMatch();

            if (listMatchGem.Count == 0) {
                return new Pair<int>(-1, -1);
            }

            //Code

            for (int i = botPlayer.heroes.Count-1; i >=0; i--)
            {
                if(!botPlayer.heroes[i].isAlive()) continue;

                if(botPlayer.heroes[i].NeedMana()<=3)
                {
                    GemSwapInfo gemSwap = null;
                    foreach (GemType type in botPlayer.heroes[i].gemTypes)
                    {
                        gemSwap = listMatchGem.Where(gemMatch => gemMatch.type == type).FirstOrDefault();
                    }

                    if(gemSwap!=null)
                    {
                        if(botPlayer.heroes[i].id == HeroIdEnum.CERBERUS)
                        {
                            int countKill = 0;

                            foreach (Hero eHero in enemyPlayer.heroes)
                            {
                                if(botPlayer.heroes[i].GetAtk()+4>=eHero.GetHp())
                                {
                                    countKill++;
                                }
                            }

                            if(countKill>1)
                            {
                                return gemSwap.getIndexSwapGem();
                            }
                        }
                    }
                }
            }


            //Last sword will end game
            GemSwapInfo matchSwordEndGame = listMatchGem.Where(gemMatch => gemMatch.type == GemType.SWORD).FirstOrDefault();
            if (matchSwordEndGame != null) {
                int countEnemyHeroAlive = 0;
                foreach (Hero eHero in enemyPlayer.heroes)
                {
                    if(eHero.isAlive()) countEnemyHeroAlive++;
                }

                if(countEnemyHeroAlive == 1 && botPlayer.firstHeroAlive().GetAtk()>=enemyPlayer.firstHeroAlive().GetHp())
                {
                    return matchSwordEndGame.getIndexSwapGem();
                }
            }


            return new Pair<int>(-1, -1);
        }

        protected bool isBotTurn()
        {
            return botPlayer.playerId == currentPlayerId;
        }
    }
}