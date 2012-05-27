using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using System.ComponentModel;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public class ConsequenceTableSource : UITableViewSource
	{
		protected string _consequenceCellID = "ConsequenceCell";
		
		public SpendingConsequencesViewController ParentController { get; private set; }
		
		public Profile Profile { get; private set; }
		
		public ConsequenceResult[] CurrentResults { get; private set; }
		
		public ConsequenceTableSource (Profile profile, SpendingConsequencesViewController parent)
		{
			this.ParentController = parent;
			this.Profile = profile;
			
			if (NSUserDefaults.StandardUserDefaults ["Gender"] != null)
				Enum.TryParse (NSUserDefaults.StandardUserDefaults ["Gender"].ToString (), true, out UserGender);
		}
		
		private Gender UserGender = Gender.Unspecified;
		
		private BackgroundWorker computeWorker;
		
		public void ComputeConsequences (ConsequenceRequest request)
		{	
			if (computeWorker != null && computeWorker.IsBusy)
				computeWorker.CancelAsync ();
			
			computeWorker = new BackgroundWorker ();
			computeWorker.DoWork += HandleDoWork;
			computeWorker.RunWorkerCompleted += HandleRunWorkerCompleted;
			computeWorker.WorkerSupportsCancellation = true;
			computeWorker.RunWorkerAsync (request);
		}

		void HandleRunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Result != null) {
				this.CurrentResults = (ConsequenceResult[])(e.Result);	
				ResultsReady (this, new EventArgs ());
			}
		}

		void HandleDoWork (object sender, DoWorkEventArgs e)
		{
			BackgroundWorker myWorker = sender as BackgroundWorker;
			ConsequenceRequest request = e.Argument as ConsequenceRequest;
				
			ConsequenceResult[] results = (from c in Profile.Calculators
			        where c.WillTriggerOn (request.TriggerMode)
				&& (c.ForGender == Gender.Unspecified || c.ForGender == UserGender)
				&& (c.Country == null || c.Country == NSLocale.CurrentLocale.CountryCode)
				&& c.LowerThreshold <= request.InitialAmount
				&& c.UpperThreshold >= request.InitialAmount
				    let result = c.Calculate (request)
					where !myWorker.CancellationPending
				&& result != null
				&& result.Recommended
					select result).ToArray ();
				
			if (!myWorker.CancellationPending)
				e.Result = results;
		}
		
		public event EventHandler<EventArgs> ResultsReady = delegate {};
		
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
			
			if (result != null) {
				UIImage image = Profile.GetImage (result.ImageName);
			
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

