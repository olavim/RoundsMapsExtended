using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MapsExt.Editor
{
	public class RangeObservableCollection<T> : ObservableCollection<T>
	{
		private bool suppressNotification = false;

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (!suppressNotification)
			{
				base.OnCollectionChanged(e);
			}
		}

		public void AddRange(IEnumerable<T> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}

			this.suppressNotification = true;
			IList oldItems = this.ToArray();

			foreach (T item in list)
			{
				this.Add(item);
			}

			this.suppressNotification = false;

			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList) this.ToArray(), oldItems));
		}

		public void Remove(IEnumerable<T> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}

			this.suppressNotification = true;
			IList oldItems = this.ToArray();

			foreach (T item in list)
			{
				this.Remove(item);
			}

			this.suppressNotification = false;

			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList) this.ToArray(), oldItems));
		}
	}
}
