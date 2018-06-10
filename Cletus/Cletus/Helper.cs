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

            List<Unit> workers = getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_SCV);
            List<Unit> idleWorkers = new List<Unit>();
            foreach (Unit worker in workers)
            {
                if (worker.Orders.Count() == 0)
                {
                    idleWorkers.Add(worker);
                }
            }
            return idleWorkers;
        }

        public static List<Unit> getCollectingWorkers()
        {
            var workers = getAllUnitsOfUnitType(UNIT_TYPEID.TERRAN_SCV);
            var collectingWorkers = new List<Unit>();


            foreach (Unit worker in workers)
            {
                if (worker.Orders.Any(Order => Order.AbilityId == (uint)ABILITY_ID.HARVEST_GATHER_SCV))
                {
                    collectingWorkers.Add(worker);
                }
                else if (worker.Orders.Any(Order => Order.AbilityId == (uint)ABILITY_ID.HARVEST_RETURN_SCV))
                {
                    collectingWorkers.Add(worker);
                }
            }
            return collectingWorkers;
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

        public static Action getAction(Unit unit, ABILITY_ID ability, Unit targetUnit)
        {
            Action action = new Action();

            action.ActionRaw = new ActionRaw();
            action.ActionRaw.ClearAction();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = (int)ability;
            action.ActionRaw.UnitCommand.TargetUnitTag = targetUnit.Tag;

            action.ActionRaw.UnitCommand.UnitTags.Add(unit.Tag);

            return action;
        }

        public static Action getAction(Unit unit, ABILITY_ID ability)
        {
            Action action = new Action();

            action.ActionRaw = new ActionRaw();
            action.ActionRaw.ClearAction();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = (int)ability;

            action.ActionRaw.UnitCommand.UnitTags.Add(unit.Tag);

            return action;
        }

        public static Action getAction(Unit unit, ABILITY_ID ability, Point2D position)
        {
            Action action = new Action();

            action.ActionRaw = new ActionRaw();
            action.ActionRaw.ClearAction();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = (int)ability;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = position;

            action.ActionRaw.UnitCommand.UnitTags.Add(unit.Tag);

            return action;
        }

        public static bool isOrderQueued(ABILITY_ID ability)
        {
            foreach (var unit in getMyUnits())
            {
                if (unit.Orders.Any(Order => Order.AbilityId == (uint)ability))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<Unit> getAllUnitsOfUnitType(UNIT_TYPEID unitType)
        {
            return getMyUnits().Where(Unit => Unit.UnitType == (uint)unitType).ToList();
        }

        public static bool isOrderQueued(ABILITY_ID ability, UNIT_TYPEID unitType)
        {
            foreach (var unit in getMyUnits().Where(Unit => Unit.UnitType == (uint)unitType))
            {
                if (unit.Orders.Any(Order => Order.AbilityId == (uint)ability))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isOrderQueued(ABILITY_ID ability, Unit unit)
        {
            if (unit.Orders.Any(Order => Order.AbilityId == (uint)ability))
            {
                return true;
            }
            return false;
        }
    }
}