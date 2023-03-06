using MapsExt.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt
{
	public class MapObjectInteraction
	{
		public class StateTransition
		{
			public MapObjectData fromState;
			public MapObjectData toState;
			public MapObjectInstance target;
		}

		private static List<Tuple<MapObjectInstance, MapObjectData>> startState;
		private static MapObjectManager cachedMapObjectManager;

		public static void BeginInteraction(MapObjectManager mapObjectManager, IEnumerable<GameObject> objects)
		{
			MapObjectInteraction.BeginInteraction(mapObjectManager, objects.Select(obj => obj.GetComponent<MapObjectInstance>()));
		}

		public static bool BeginInteraction(MapObjectManager mapObjectManager, IEnumerable<MapObjectInstance> instances)
		{
			if (instances.Any(ins => ins == null))
			{
				return false;
			}

			MapObjectInteraction.cachedMapObjectManager = mapObjectManager;
			MapObjectInteraction.startState = new List<Tuple<MapObjectInstance, MapObjectData>>();

			foreach (var instance in instances)
			{
				var state = mapObjectManager.Serialize(instance);
				MapObjectInteraction.startState.Add(new Tuple<MapObjectInstance, MapObjectData>(instance, state));
			}

			return true;
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

		public GameObject[] Targets => this.stateTransitions.Select(t => t.target.gameObject).Distinct().ToArray();

		private readonly MapObjectManager mapObjectManager;
		private readonly List<StateTransition> stateTransitions;

		private MapObjectInteraction(MapObjectManager mapObjectManager, List<StateTransition> stateTransitions)
		{
			this.mapObjectManager = mapObjectManager;
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

		public StateTransition GetTransition(GameObject target)
		{
			if (target == null)
			{
				return null;
			}

			return this.stateTransitions.FirstOrDefault(t => t.target.gameObject == target);
		}

		private void SetState(MapObjectData state, MapObjectInstance target)
		{
			this.mapObjectManager.Deserialize(state, target.gameObject);
		}
	}
}
