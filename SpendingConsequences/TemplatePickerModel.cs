using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

using MonoTouch;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

using ETFLib.Composition;

namespace SpendingConsequences
{
	public class TemplatePickerModel : UIPickerViewModel
	{
		public TemplatePickerModel (AppProfile profile)
		{
			this.Profile = profile;
		}

		private AppProfile Profile { get; set; }

		public override int GetComponentCount (UIPickerView picker)
		{
			return 1;
		}

		public override int GetRowsInComponent (UIPickerView picker, int component)
		{
			return Profile.AllConsequenceTemplates.Count ();
		}

		public override string GetTitle (UIPickerView picker, int row, int component)
		{
			var template = Profile.AllConsequenceTemplates.Skip(row).FirstOrDefault();
			if (template != null)
				return template.Name;
			else
				return "Error";
		}

		public override void Selected (UIPickerView picker, int row, int component)
		{
			SelectedTemplate = Profile.AllConsequenceTemplates.Skip(row).FirstOrDefault();
		}

		public Template SelectedTemplate { get; private set; }
	}
}

