using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public class ConsequenceTableSource : UITableViewSource
	{
		protected string _consequenceCellID = "ConsequenceCell";
		
		public SpendingConsequencesViewController ParentController { get; private set; }
		
		public List<ACalculator> Calculators { get; private set; }
		
		public ConsequenceResult[] CurrentResults { get; private set; }
		
		public ConsequenceTableSource (List<ACalculator> calculators, SpendingConsequencesViewController parent)
		{
			this.ParentController = parent;
			this.Calculators = calculators;
		}
		
		public void ComputeConsequences (ConsequenceRequest request)
		{	
			this.CurrentResults = (from c in this.Calculators
			        where c.WillTriggerOn (request.TriggerMode)
			        && c.LowerThreshold <= request.InitialAmount
			        && c.UpperThreshold >= request.InitialAmount
				    let result = c.Calculate (request)
					where result != null
					select result).ToArray ();
		}
		
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			//ToDo: Find a more elegant way to do this. Carry a reference around to a
			//specific ViewController subclass is ugly.
			this.ParentController.DisplayConsequenceDetails (CurrentResults [indexPath.Row]);
		}
		
		#region implemented abstract members of MonoTouch.UIKit.UITableViewSource
		public override int RowsInSection (UITableView tableview, int section)
		{
			if (CurrentResults == null)
				return 0;
			else
				return CurrentResults.Length;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			ConsequenceResult result = CurrentResults [indexPath.Row];
			
			UITableViewCell cell = tableView.DequeueReusableCell (this._consequenceCellID);
			
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, this._consequenceCellID);
			
			NSCache imgCache = ((AppDelegate)UIApplication.SharedApplication.Delegate).ImageCache;
			
			NSObject key = NSObject.FromObject (result.ImageName);
			UIImage image = imgCache.ObjectForKey (key) as UIImage;
			if (image == null) {
				image = UIImage.FromBundle (string.Format ("Artwork/{0}.png", result.ImageName));
				imgCache.SetObjectforKey (image, key);
			}
			
			cell.TextLabel.Text = String.Format (result.Calculator.ResultFormat, result.ComputedValue);
			cell.DetailTextLabel.Text = result.FormattedCaption;
			cell.ImageView.Image = image;
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			return cell;
		}
		#endregion
	}
}

