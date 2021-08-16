using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt
{
    public class MapObjectInteraction
    {
        struct InteractionState
        {
            public GameObject mapObject;
            public Vector3 position;
            public Vector3 scale;
            public Quaternion rotation;
            public bool active;
        }

        private static List<InteractionState> startState;

        public static void BeginInteraction(IEnumerable<GameObject> objects)
        {
            MapObjectInteraction.startState = new List<InteractionState>();

            foreach (var obj in objects)
            {
                var state = new InteractionState()
                {
                    mapObject = obj,
                    position = obj.transform.position,
                    scale = obj.transform.localScale,
                    rotation = obj.transform.rotation,
                    active = obj.gameObject.activeSelf
                };

                MapObjectInteraction.startState.Add(state);
            }
        }

        public static MapObjectInteraction EndInteraction()
        {
            var statePairs = new List<Tuple<InteractionState, InteractionState>>();

            foreach (var startState in MapObjectInteraction.startState)
            {
                var endState = new InteractionState()
                {
                    mapObject = startState.mapObject,
                    position = startState.mapObject.transform.position,
                    scale = startState.mapObject.transform.localScale,
                    rotation = startState.mapObject.transform.rotation,
                    active = startState.mapObject.gameObject.activeSelf
                };

                statePairs.Add(new Tuple<InteractionState, InteractionState>(startState, endState));
            }

            return new MapObjectInteraction(statePairs);
        }

        private readonly List<Tuple<InteractionState, InteractionState>> statePairs;

        private MapObjectInteraction(List<Tuple<InteractionState, InteractionState>> statePairs)
        {
            this.statePairs = statePairs;
        }

        public List<GameObject> GetGameObjects()
        {
            return this.statePairs.Select(p => p.Item1.mapObject).ToList();
        }

        public void Undo()
        {
            foreach (var statePair in this.statePairs)
            {
                var from = statePair.Item2;
                var to = statePair.Item1;

                if (!this.IsCurrentState(from))
                {
                    throw new InvalidOperationException("Cannot Undo: map object state does not match interaction state");
                }

                this.SetState(to);
            }
        }

        public void Redo()
        {
            foreach (var statePair in this.statePairs)
            {
                var from = statePair.Item1;
                var to = statePair.Item2;

                if (!this.IsCurrentState(from))
                {
                    throw new InvalidOperationException("Cannot Undo: map object state does not match interaction state");
                }

                this.SetState(to);
            }
        }

        private void SetState(InteractionState state)
        {
            var mapObject = state.mapObject;
            mapObject.transform.position = state.position;
            mapObject.transform.localScale = state.scale;
            mapObject.transform.rotation = state.rotation;
            mapObject.SetActive(state.active);
        }

        private bool IsCurrentState(InteractionState state)
        {
            var obj = state.mapObject;
            return state.position == obj.transform.position &&
                state.scale == obj.transform.localScale &&
                state.rotation == obj.transform.rotation &&
                state.active == obj.gameObject.activeSelf;
        }
    }
}
