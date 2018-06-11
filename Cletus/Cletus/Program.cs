using Google.Protobuf.Collections;
using SC2APIProtocol;
using Starcraft2;
using System;
using System.Collections.Generic;
using System.Linq;

using Action = SC2APIProtocol.Action;


namespace Cletus
{
    class Program
    {
        static void Main(string[] args)
        {
            var userSettings = Sc2SettingsFile.settingsFromUserDir();

            var instanceSettings = Instance.StartSettings.OfUserSettings(userSettings);

            Func<Instance.Sc2Instance> createInstance =
                () => Runner.run(Instance.start(instanceSettings));

            var participants = new Sc2Game.Participant[] {
        Sc2Game.Participant.CreateParticipant(
            createInstance(),
            Race.Terran,
            MasterAgent_MainLoop),
        Sc2Game.Participant.CreateComputer(Race.Terran, Difficulty.VeryEasy)
    };

            var gameSettings =
                Sc2Game.GameSettings.OfUserSettings(userSettings)
                .WithMap(@"Ladder2017Season1\AbyssalReefLE.SC2Map")
                .WithRealtime(true);

            try
            {
                // Runs the game to the end with the given bots / map and configuration
                Runner.run(Sc2Game.runGame(gameSettings, participants));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Press a key to exit...");
                Console.ReadKey();
            }
            

        }

        public static IEnumerable<Action> MasterAgent_MainLoop(GameState gameState)
        {
            Observation observation = gameState.NewObservation.Observation;
            //MapState map = observation.RawData.MapState;

            Helper.observation = gameState.NewObservation.Observation;

            var allUnits = Helper.getAllUnits();
            var myUnits = Helper.getMyUnits();

            var myMinerals = observation.PlayerCommon.Minerals;
            var myFoodUsed = observation.PlayerCommon.FoodUsed;
            var myFoodCap = observation.PlayerCommon.FoodCap;
            var myFoodFree = myFoodCap - myFoodUsed;

            int SUPPLY_DEPOT_COST = 100;
            int SCV_COST = 50;
            int BARRACKS_COST = 150;
            int REFINERY_COST = 75;

            //send idle workers to minerals
            if (observation.PlayerCommon.IdleWorkerCount > 0)
            {
                foreach (var idleWorker  in Helper.getIdleWorkers())
                {
                    Unit nearestMineral = Helper.getNearestUnitOfUnitType(idleWorker, UNIT_TYPEID.NEUTRAL_MINERALFIELD);
                    yield return Helper.getAction(idleWorker, ABILITY_ID.HARVEST_GATHER, nearestMineral);

                }
            }


            // TODO check if we want to train scv's
            if (true)
            {
                // check if we can afford scv's
                if (myMinerals >= SCV_COST && myFoodFree >= 1)
                {

                    foreach (var commandCenter in Helper.getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_COMMANDCENTER))
                    {
                        if (!Helper.isOrderQueued(ABILITY_ID.TRAIN_SCV, commandCenter))
                        {
                            yield return Helper.getAction(commandCenter, ABILITY_ID.TRAIN_SCV);
                            break;
                        }
                    }
                }
            }

            // Build supply depot
            if (myFoodFree < 5 && myMinerals > SUPPLY_DEPOT_COST && !Helper.isOrderQueued(ABILITY_ID.BUILD_SUPPLYDEPOT, UNIT_TYPEID.TERRAN_SCV))
            {

                Unit commandCenter = Helper.getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_COMMANDCENTER).First();
                if(Helper.getCollectingWorkers().Count > 0)
                {
                    Unit worker = Helper.getCollectingWorkers().First();

                    var r = new Random();

                    var BASE_SIZE = 15;
                    var ccX = commandCenter.Pos.X + (r.Next(-1 * BASE_SIZE, BASE_SIZE));
                    var ccY = commandCenter.Pos.Y + (r.Next(-1 * BASE_SIZE, BASE_SIZE));

                    Point2D position = new Point2D();
                    position.X = ccX;
                    position.Y = ccY;

                    yield return Helper.getAction(worker, ABILITY_ID.BUILD_SUPPLYDEPOT, position);
                }
            }

            // Barracks
            if ( myMinerals > BARRACKS_COST && !Helper.isOrderQueued(ABILITY_ID.BUILD_BARRACKS, UNIT_TYPEID.TERRAN_SCV))
            {
                // check if we want to build barracks
                int barracksCount = Helper.getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_BARRACKS).Count;
                if (barracksCount < 1)
                {
                    if (Helper.getCollectingWorkers().Count > 0)
                    {
                        Unit worker = Helper.getCollectingWorkers().First();
                        Unit commandCenter = Helper.getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_COMMANDCENTER).First();
                        var r = new Random();

                        var BASE_SIZE = 15;
                        var ccX = commandCenter.Pos.X + (r.Next(-1 * BASE_SIZE, BASE_SIZE));
                        var ccY = commandCenter.Pos.Y + (r.Next(-1 * BASE_SIZE, BASE_SIZE));

                        Point2D position = new Point2D();
                        position.X = ccX;
                        position.Y = ccY;

                        yield return Helper.getAction(worker, ABILITY_ID.BUILD_BARRACKS, position);
                    }
                }
            }

            // Vespine Gas
            if (Helper.getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_BARRACKS).Count > 0)
            {
                if (myMinerals > REFINERY_COST && !Helper.isOrderQueued(ABILITY_ID.BUILD_REFINERY, UNIT_TYPEID.TERRAN_SCV))
                {

                    if (Helper.getCollectingWorkers().Count > 0)
                    {
                        Unit worker = Helper.getCollectingWorkers().First();
                        Unit commandCenter = Helper.getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_COMMANDCENTER).First();

                        Unit gas = Helper.getNearestUnitOfUnitType(commandCenter, UNIT_TYPEID.NEUTRAL_VESPENEGEYSER);


                        yield return Helper.getAction(worker, ABILITY_ID.BUILD_REFINERY, gas);
                    }
                    
                }

            }

            // Build Marines
            // TODO check if we want to train marines
            if(Helper.getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_BARRACKS).Count > 0 && myMinerals > 200)
            {
                // check if we can afford marines
                if (myMinerals >= 50 && myFoodFree >= 1)
                {

                    foreach (var barracks in Helper.getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_BARRACKS))
                    {
                        if (!Helper.isOrderQueued(ABILITY_ID.TRAIN_MARINE, barracks))
                        {
                            yield return Helper.getAction(barracks, ABILITY_ID.TRAIN_MARINE);
                            break;
                        }
                    }
                }

            }

            // TODO check if we want to attack
            if (true)
            {
                foreach (var marine in Helper.getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_MARINE).Where(Unit => Unit.Orders.Count == 0))
                {
                    Point2D attackLocation = new Point2D();
                    attackLocation.X = gameState.GameInfo.StartRaw.StartLocations.First().X;
                    attackLocation.Y = gameState.GameInfo.StartRaw.StartLocations.First().Y;

                    yield return Helper.getAction(marine,ABILITY_ID.ATTACK, attackLocation);
                }
            }

            yield return new Action();
        }

    }
}
