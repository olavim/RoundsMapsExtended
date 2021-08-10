using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsExtended
{
    public class TargetSyncedStore<T> where T : IEquatable<T>
    {
        private object currentTarget = null;
        private Dictionary<int, T> values = new Dictionary<int, T>();
        private Dictionary<int, bool> valueSet = new Dictionary<int, bool>();

        public int Allocate(object target)
        {
            if (target != this.currentTarget)
            {
                this.values.Clear();
                this.valueSet.Clear();
                this.currentTarget = target;
            }

            int id = this.values.Count;

            if (!this.values.ContainsKey(id))
            {
                this.values.Add(id, default);
                this.valueSet.Add(id, false);
            }

            return id;
        }

        public bool TargetEquals(object target)
        {
            return target == this.currentTarget;
        }

        public bool IsValueSet(int id)
        {
            return this.values.ContainsKey(id) && this.valueSet[id];
        }

        public IEnumerator WaitForValue(object target, int id)
        {
            while (target == this.currentTarget && !this.IsValueSet(id))
            {
                yield return null;
            }
        }

        public T Get(int id)
        {
            return this.values[id];
        }

        public void Set(int id, T value)
        {
            if (!this.values.ContainsKey(id))
            {
                this.values.Add(id, value);
                this.valueSet.Add(id, true);
            }
            else
            {
                this.values[id] = value;
                this.valueSet[id] = true;
            }
        }
    }
}
