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
			
			if (NSUserDefaults.StandardUserDefaults ["Gender"] != null)
				Enum.TryParse (NSUserDefaults.StandardUserDefaults ["Gender"].ToString (), true, out UserGender);
		}
		
		private Gender UserGender = Gender.Unspecified;
		
		public void ComputeConsequences (ConsequenceRequest request)
		{	
			this.CurrentResults = (from c in this.Calculators
			        where c.WillTriggerOn (request.TriggerMode)
			        && (c.ForGender == Gender.Unspecified || c.ForGender == UserGender)
			        && (c.Country == null || c.Country == NSLocale.CurrentLocale.CountryCode)
			        && c.LowerThreshold <= request.InitialAmount
			        && c.UpperThreshold >= request.InitialAmount
				    let result = c.Calculate (request)
					where result != null
					select result).ToArray ();
		}
		
		public int ReplaceResult (ConsequenceResult oldResult, ConsequenceResult newResult)
		{
			if (CurrentResults == null)
				return -1;
			
			for (int i = 0; i < CurrentResults.Length; i++)
				if (CurrentResults [i] == oldResult) {
					CurrentResults [i] = newResult;
					return i;
				}
			
			return -1;
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

			if (result != null) {
				NSObject key = NSObject.FromObject (result.ImageName);
				UIImage image = imgCache.ObjectForKey (key) as UIImage;
				if (image == null) {
					string filename = string.Format ("Artwork/{0}.png", result.ImageName);
					if (NSFileManager.DefaultManager.FileExists (filename)) {
						image = UIImage.FromBundle (filename);
						imgCache.SetObjectforKey (image, key);	
					}
				}
			
				if (image != null)
					cell.ImageView.Image = image;
				cell.TextLabel.Text = result.ComputedValue.ToString ();
				cell.DetailTextLabel.Text = result.FormattedCaption;
				cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;				
			} else {
				// Return empty cell. We'll remove this later rather than return a null from this method, which raises an assertion anyway
				cell.ImageView.Image = null;
				cell.TextLabel.Text = "";
				cell.DetailTextLabel.Text = "";
				cell.Accessory = UITableViewCellAccessory.None;
			}

			
			return cell;
		}
		#endregion
	}
}

