using System;
using System.Drawing;
using System.Collections.Generic;
using System.Xml.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SpendingConsequences
{
	public partial class TemplatePickerSheet : UIViewController
	{
		public TemplatePickerSheet (AppProfile profile) : base ("TemplatePickerSheet", null)
		{
			this.Profile = profile;
		}

		private AppProfile Profile { get; set; }
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			ArtRepository.StyleButton("embossed", this.useButton);
			ArtRepository.StyleButton("embossed", this.cancelButton);
			
			if (Model == null)
				Model = new TemplatePickerModel(Profile);

			this.templatePicker.Model = Model;

			this.cancelButton.TouchUpInside += delegate(object sender, EventArgs e) {
				Cancelled(this, e);
			};

			this.useButton.TouchUpInside += delegate(object sender, EventArgs e) {
				TemplateChosen(this, new TemplateChosenEventArgs(Model.SelectedTemplate));
			};
		}

		private TemplatePickerModel Model { get; set; }
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}

		public event EventHandler<EventArgs> Cancelled = delegate{};

		public event EventHandler<TemplateChosenEventArgs> TemplateChosen = delegate{};
	}

	public class TemplateChosenEventArgs : EventArgs 
	{
		public XElement Template { get; private set; }

		public TemplateChosenEventArgs(XElement template)
		{
			this.Template = template;
		}
	}
}

