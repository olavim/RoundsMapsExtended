using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExtended
{
    // An InteractionTimeline forms a one-dimensional timeline of MapObjectInteractions that can be traversed by "undoing" and "redoing".
    public class InteractionTimeline
    {
        private readonly List<MapObjectInteraction> interactionStack = new List<MapObjectInteraction>();
        private readonly Dictionary<int, GameObject[]> interactionIndexGameObjects = new Dictionary<int, GameObject[]>();

        private int interactionIndex = -1;
        private GameObject[] interactionGameObjects;

        public void BeginInteraction(GameObject obj, bool isCreateInteraction = false)
        {
            this.BeginInteraction(new GameObject[] { obj }, isCreateInteraction);
        }

        public void BeginInteraction(IEnumerable<GameObject> objects, bool isCreateInteraction = false)
        {
            MapObjectInteraction.BeginInteraction(objects);
            this.interactionGameObjects = isCreateInteraction ? objects.ToArray() : null;
        }

        public void EndInteraction()
        {
            if (this.interactionIndex < this.interactionStack.Count - 1)
            {
                int removeFrom = this.interactionIndex + 1;
                int removeCount = this.interactionStack.Count - removeFrom;
                this.interactionStack.RemoveRange(removeFrom, removeCount);

                /* Deleting map objects really only disables them so that they can be easily retrieved during undo/redo. However, undoing and
                 * then interacting with map objects will branch into a logically new timeline, and the old branch is lost forever. If map objects
                 * were created in this old/deleted branch, they can never be retrieved, and so we can safely destroy those game objects.
                 */
                for (int i = removeFrom; i < removeFrom + removeCount; i++)
                {
                    if (this.interactionIndexGameObjects.TryGetValue(i, out GameObject[] list))
                    {
                        foreach (var obj in list)
                        {
                            GameObject.Destroy(obj);
                        }

                        this.interactionIndexGameObjects.Remove(i);
                    }
                }
            }

            var interaction = MapObjectInteraction.EndInteraction();
            this.interactionStack.Add(interaction);
            this.interactionIndex++;

            if (this.interactionGameObjects != null)
            {
                this.interactionIndexGameObjects.Add(this.interactionIndex, this.interactionGameObjects);
                this.interactionGameObjects = null;
            }
        }

        public bool CanUndo()
        {
            return this.interactionIndex >= 0;
        }

        public bool CanRedo()
        {
            return this.interactionIndex < this.interactionStack.Count - 1;
        }

        public bool Undo()
        {
            if (!this.CanUndo())
            {
                return false;
            }

            var interaction = this.interactionStack[this.interactionIndex];
            interaction.Undo();
            this.interactionIndex--;
            return true;
        }

        public bool Redo()
        {
            if (!this.CanRedo())
            {
                return false;
            }

            this.interactionIndex++;
            var interaction = this.interactionStack[this.interactionIndex];
            interaction.Redo();
            return true;
        }
    }
}
