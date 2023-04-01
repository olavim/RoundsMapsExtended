using System;
using System.Collections.Generic;

namespace MapsExt.Editor.Tests
{
	public static class ListIterator
	{
		public static ListIterator<T> From<T>(IList<T> list)
		{
			return new ListIterator<T>(list);
		}
	}

	public class ListIterator<T>
	{
		private readonly IList<T> list;
		private int index;

		public T Current => this.list[this.index];
		public int Count => this.list.Count;

		public ListIterator(IList<T> list)
		{
			if (list == null || list.Count == 0)
			{
				throw new ArgumentException(nameof(list));
			}

			this.list = list;
		}

		public ListIterator<T> MoveNext()
		{
			return this.Move(this.index + 1);
		}

		public ListIterator<T> MovePrevious()
		{
			return this.Move(this.index - 1);
		}

		public ListIterator<T> MoveFirst()
		{
			return this.Move(0);
		}

		public ListIterator<T> MoveLast()
		{
			return this.Move(this.Count - 1);
		}

		public ListIterator<T> Move(int newIndex)
		{
			if (newIndex < 0 || newIndex >= this.list.Count)
			{
				throw new ArgumentException("Invalid index");
			}

			this.index = newIndex;
			return this;
		}
	}
}
