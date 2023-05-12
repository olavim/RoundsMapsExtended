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
		private bool _suppressNotification;

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (!_suppressNotification)
			{
				base.OnCollectionChanged(e);
			}
		}

		public void AddRange(IEnumerable<T> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}

			this._suppressNotification = true;
			IList oldItems = this.ToArray();

#pragma warning disable RCS1235
			foreach (T item in list)
			{
				this.Add(item);
			}
#pragma warning restore RCS1235

			this._suppressNotification = false;

			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList) this.ToArray(), oldItems));
		}

		public void Remove(IEnumerable<T> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}

			this._suppressNotification = true;
			IList oldItems = this.ToArray();

			foreach (T item in list)
			{
				this.Remove(item);
			}

			this._suppressNotification = false;

			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList) this.ToArray(), oldItems));
		}
	}
}
