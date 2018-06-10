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
        Sc2Game.Participant.CreateComputer(Race.Terran, Difficulty.Hard)
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
            Action action = new Action();

            Observation observation = gameState.NewObservation.Observation;
            //MapState map = observation.RawData.MapState;

            RepeatedField<Unit> allUnits = observation.RawData.Units;
            var myUnits = allUnits.Where(Unit => Unit.Owner == 1);

            var myMinerals = observation.PlayerCommon.Minerals;
            var myFoodUsed = observation.PlayerCommon.FoodUsed;
            var myFoodCap = observation.PlayerCommon.FoodCap;
            var myFoodFree = myFoodCap - myFoodUsed;

            ulong? unitTag = null;
            
            // TODO check if we want to train scv's
            if (true)
            {
                // check if we can afford scv's
                if (myMinerals >= 50 && myFoodFree >= 1)
                {

                    foreach (var unit in myUnits)
                    {
                        // Unit is a command center
                        if (unit.UnitType == (uint)UNIT_TYPEID.TERRAN_COMMANDCENTER)
                        {
                            // Unit is not training a scv at the moment
                            var ScvInTraining = unit.Orders.Where(Order => Order.AbilityId == (uint)ABILITY_ID.TRAIN_SCV);
                            if (ScvInTraining.Count() == 0)
                            {
                                unitTag = unit.Tag;
                            }

                        }
                    }

                    if (unitTag != null)
                    {
                        action.ActionRaw = new ActionRaw();
                        action.ActionRaw.ClearAction();
                        action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
                        action.ActionRaw.UnitCommand.AbilityId = (int)ABILITY_ID.TRAIN_SCV;

                        action.ActionRaw.UnitCommand.UnitTags.Add(unitTag.Value);
                    }
                }
            }

            // Build supply depot
            if (myFoodFree < 5)
            {
                foreach (var unit in myUnits)
                {
                    if (unit.UnitType == (uint)UNIT_TYPEID.TERRAN_SCV)
                    {
                        unitTag = unit.Tag;
                        break;
                    }
                }

                var r = new Random();

                action.ActionRaw = new ActionRaw();
                action.ActionRaw.ClearAction();
                action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
                action.ActionRaw.UnitCommand.AbilityId = (int)ABILITY_ID.BUILD_SUPPLYDEPOT;
                action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();

                action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = r.Next(1, 1000);
                action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = r.Next(1, 1000);

                action.ActionRaw.UnitCommand.UnitTags.Add(unitTag.Value);

            }

            yield return action;
        }

    }
}
