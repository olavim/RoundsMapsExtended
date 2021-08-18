using MapsExt.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt
{
    public class MapObjectInteraction
    {
        private struct StateTransition
        {
            public MapObject fromState;
            public MapObject toState;
            public MapObjectInstance target;
        }

        private static List<Tuple<MapObjectInstance, MapObject>> startState;
        private static MapObjectManager cachedMapObjectManager;

        public static void BeginInteraction(MapObjectManager mapObjectManager, IEnumerable<GameObject> objects)
        {
            MapObjectInteraction.BeginInteraction(mapObjectManager, objects.Select(obj => obj.GetComponent<MapObjectInstance>()));
        }

        public static void BeginInteraction(MapObjectManager mapObjectManager, IEnumerable<MapObjectInstance> instances)
        {
            if (instances.Any(ins => ins == null))
            {
                throw new ArgumentException("Cannot begin interaction: MapObjectInstance must not be null");
            }

            MapObjectInteraction.cachedMapObjectManager = mapObjectManager;
            MapObjectInteraction.startState = new List<Tuple<MapObjectInstance, MapObject>>();

            foreach (var instance in instances)
            {
                var state = mapObjectManager.Serialize(instance);
                MapObjectInteraction.startState.Add(new Tuple<MapObjectInstance, MapObject>(instance, state));
            }
        }

        public static MapObjectInteraction EndInteraction()
        {
            var stateTransitions = new List<StateTransition>();

            foreach (var startState in MapObjectInteraction.startState)
            {
                var mapObjectInstance = startState.Item1;
                var toState = MapObjectInteraction.cachedMapObjectManager.Serialize(mapObjectInstance);

                var st = new StateTransition
                {
                    fromState = startState.Item2,
                    toState = toState,
                    target = mapObjectInstance
                };

                stateTransitions.Add(st);
            }

            return new MapObjectInteraction(MapObjectInteraction.cachedMapObjectManager, stateTransitions);
        }

        private readonly MapObjectManager mapObjectManager;
        private readonly List<StateTransition> stateTransitions;

        private MapObjectInteraction(MapObjectManager mapObjectManager, List<StateTransition> stateTransitions)
        {
            this.stateTransitions = stateTransitions;
        }

        public List<GameObject> GetGameObjects()
        {
            return this.stateTransitions.Select(st => st.target.gameObject).ToList();
        }

        public void Undo()
        {
            foreach (var st in this.stateTransitions)
            {
                this.SetState(st.fromState, st.target);
            }
        }

        public void Redo()
        {
            foreach (var st in this.stateTransitions)
            {
                this.SetState(st.toState, st.target);
            }
        }

        private void SetState(MapObject state, MapObjectInstance target)
        {
            this.mapObjectManager.Deserialize(state, target.gameObject);
        }
    }
}
