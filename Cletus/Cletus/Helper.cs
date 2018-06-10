using Google.Protobuf.Collections;
using SC2APIProtocol;
using Starcraft2;
using System;
using System.Collections.Generic;
using System.Linq;

using Action = SC2APIProtocol.Action;

namespace Cletus
{
    abstract class Helper
    {
        public static Observation observation;

        public static List<Unit> getIdleWorkers()
        {
            if(observation.PlayerCommon.IdleWorkerCount == 0)
            {
                return new List<Unit>();
            }

            List<Unit> workers = getAllWorkers();
            foreach (Unit worker in workers)
            {
                if (worker.Orders.Count() != 0)
                {
                    workers.Remove(worker);
                }
            }
            return workers;
        }

        public static Unit getNearestMineralfield(Unit unit)
        {
            var unitX = unit.Pos.X;
            var unitY = unit.Pos.Y;

            float distance = float.MaxValue;
            Unit nearestMineral = null;

            foreach (var mineral in observation.RawData.Units.Where(Unit => Unit.UnitType == (uint)UNIT_TYPEID.NEUTRAL_MINERALFIELD))
            {
                var distanceX = (unitX - mineral.Pos.X);
                if (distanceX < 0) { distanceX *= -1; }
                var distanceY = (unitY - mineral.Pos.Y);
                if (distanceY < 0) { distanceY *= -1; }

                var checkDistance = distanceX + distanceY;

                //new shortest distance
                if (checkDistance < distance )
                {
                    nearestMineral = mineral;
                    distance = checkDistance;
                }
            }

            return nearestMineral;
        }


        public static List<Unit> getAllWorkers()
        {
            return getMyUnits().Where(Unit => Unit.UnitType == (uint)UNIT_TYPEID.TERRAN_SCV).ToList();
        }

        public static List<Unit> getCollectingWorkers()
        {
            var workers = getAllWorkers();
            foreach (Unit worker in workers)
            {
                if (!(worker.Orders.Any(Order => Order.AbilityId == (uint)ABILITY_ID.HARVEST_GATHER)))
                {
                    workers.Remove(worker);
                }
            }
            return workers;
        }

        public static List<Unit> getAllCommandCenters()
        {
            return getMyUnits().Where(Unit => Unit.UnitType == (uint)UNIT_TYPEID.TERRAN_COMMANDCENTER).ToList();
        }

        public static Unit getNearestUnitOfUnitType(Unit unit, UNIT_TYPEID unitType)
        {
            var unitX = unit.Pos.X;
            var unitY = unit.Pos.Y;

            float distance = float.MaxValue;
            Unit nearestUnit = null;

            foreach (var mineral in observation.RawData.Units.Where(Unit => Unit.UnitType == (uint)unitType))
            {
                var distanceX = (unitX - mineral.Pos.X);
                if (distanceX < 0) { distanceX *= -1; }
                var distanceY = (unitY - mineral.Pos.Y);
                if (distanceY < 0) { distanceY *= -1; }

                var checkDistance = distanceX + distanceY;

                //new shortest distance
                if (checkDistance < distance)
                {
                    nearestUnit = mineral;
                    distance = checkDistance;
                }
            }

            return nearestUnit;
        }

        public static List<Unit> getAllUnits()
        {
            return observation.RawData.Units.ToList();
        }

        public static List<Unit> getMyUnits()
        {
            return getAllUnits().Where(Unit => Unit.Owner == 1).ToList();
        }
    }
}
