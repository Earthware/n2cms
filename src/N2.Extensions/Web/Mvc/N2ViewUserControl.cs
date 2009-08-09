using System;
using System.Collections;
using System.Web;
using System.Web.Mvc;
using N2.Web.UI;

namespace N2.Web.Mvc
{
	/// <summary>
	/// A ViewUserControl implementation that allows N2 Display helpers to be used.
	/// </summary>
	/// <typeparam name="TItem"></typeparam>
	public class N2ViewUserControl<TItem> : ViewUserControl<TItem>, IItemContainer<TItem>
		where TItem : ContentItem
	{
		#region IItemContainer<TItem> Members

		public TItem CurrentItem
		{
			get { return Model; }
		}

		ContentItem IItemContainer.CurrentItem
		{
			get { return CurrentItem; }
		}

		#endregion
	}
}