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

    public enum BuildingID
    {
        CommandCenter = 18,
    }

    public enum BuildingTrainingID
    {
        SCV = 524,
    }

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

            var x = new Observation();
            Console.WriteLine(x.PlayerCommon.Minerals);

        }

        public static IEnumerable<Action> MasterAgent_MainLoop(GameState gameState)
        {
            Action answer = new Action();

            Observation newObservation = gameState.NewObservation.Observation;
            MapState map = newObservation.RawData.MapState;

            RepeatedField<Unit> allUnits = newObservation.RawData.Units;

            ulong? unitTag = null;

            int i = 0;

            while (unitTag == null && i < allUnits.Count)
            {
                if (allUnits[i].UnitType == (int)BuildingID.CommandCenter)
                {
                    unitTag = allUnits[i].Tag;
                }
                i++;
            }

            answer.ActionRaw = new ActionRaw();
            answer.ActionRaw.ClearAction();
            answer.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            answer.ActionRaw.UnitCommand.AbilityId = (int)BuildingTrainingID.SCV;

            answer.ActionRaw.UnitCommand.UnitTags.Add(unitTag.Value);

            yield return answer;
        }

    }
}
