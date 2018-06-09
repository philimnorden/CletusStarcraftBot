using Google.Protobuf.Collections;
using SC2APIProtocol;
using Starcraft2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Runs the game to the end with the given bots / map and configuration
            Runner.run(Sc2Game.runGame(gameSettings, participants));

        }

        public static IEnumerable<Action> MasterAgent_MainLoop(GameState gameState)
        {
            Action action = new Action();

            Observation observation = gameState.NewObservation.Observation;
            //MapState map = observation.RawData.MapState;

            RepeatedField<Unit> allUnits = observation.RawData.Units;
            var myUnits = allUnits.Where(Unit => Unit.Owner == 1);

            ulong? unitTag = null;

            int i = 0;

            while (unitTag == null && i < allUnits.Count)
            {
                if (allUnits[i].UnitType == (uint)UNIT_TYPEID.TERRAN_COMMANDCENTER)
                {
                    unitTag = allUnits[i].Tag;
                }
                i++;
            }

            action.ActionRaw = new ActionRaw();
            action.ActionRaw.ClearAction();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = (int)ABILITY_ID.TRAIN_SCV;

            action.ActionRaw.UnitCommand.UnitTags.Add(unitTag.Value);
            yield return action;
        }

    }
}
