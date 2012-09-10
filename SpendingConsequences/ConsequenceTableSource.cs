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
		
		public AppProfile Profile { get; private set; }
		
		public List<ConsequenceResult> CurrentResults { get; private set; }
		
		public ConsequenceTableSource (AppProfile profile, SpendingConsequencesViewController parent)
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
				this.CurrentResults = (List<ConsequenceResult>)(e.Result);	
				ResultsReady (this, new EventArgs ());
			}
		}

		void HandleDoWork (object sender, DoWorkEventArgs e)
		{
			BackgroundWorker myWorker = sender as BackgroundWorker;
			ConsequenceRequest request = e.Argument as ConsequenceRequest;
				
			List<ConsequenceResult> results = (from c in Profile.AllCalculators
			        where c.WillTriggerOn (request.TriggerMode)
				&& (c.ForGender == Gender.Unspecified || c.ForGender == UserGender)
				&& (c.Country == null || c.Country == NSLocale.CurrentLocale.CountryCode)
				&& c.LowerThreshold <= request.InitialAmount
				&& c.UpperThreshold >= request.InitialAmount
				    let result = c.Calculate (request)
					where !myWorker.CancellationPending
				&& result != null
				&& result.Recommended
					select result).ToList();
				
			if (!myWorker.CancellationPending)
				e.Result = results;
		}
		
		public event EventHandler<EventArgs> ResultsReady = delegate {};
		
		public int ReplaceResult (ConsequenceResult oldResult, ConsequenceResult newResult)
		{
			if (CurrentResults == null)
				return -1;
			
			for (int i = 0; i < CurrentResults.Count; i++)
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
			tableView.DeselectRow(indexPath, true);
		}

		public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
		{
			return true;
		}

		public override bool CanMoveRow (UITableView tableView, NSIndexPath indexPath)
		{
			return true;
		}

		public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return UITableViewCellEditingStyle.Delete;
		}

		public override void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
		{
			if (CurrentResults == null)
				return;

			var item = CurrentResults[sourceIndexPath.Row];
			int deleteAt = sourceIndexPath.Row + 1;

			CurrentResults.Insert (destinationIndexPath.Row, item);
			CurrentResults.RemoveAt (deleteAt);
		}

		public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			switch (editingStyle)
			{
			case UITableViewCellEditingStyle.Delete:
			    CurrentResults.RemoveAt(indexPath.Row);
				tableView.DeleteRows (new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
			    break;
			}
		}
		
		#region implemented abstract members of MonoTouch.UIKit.UITableViewSource		
		public override int RowsInSection (UITableView tableview, int section)
		{
			if (CurrentResults == null)
				return 0;
			else
				return CurrentResults.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			ConsequenceResult result = CurrentResults [indexPath.Row];
			
			UITableViewCell cell = tableView.DequeueReusableCell (this._consequenceCellID);
			
			if (cell == null)
			{
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, this._consequenceCellID);

				cell.TextLabel.ShadowColor = UIColor.White;
				cell.TextLabel.ShadowOffset = new System.Drawing.SizeF(0,1);
				cell.TextLabel.Alpha = 0.7f;
			}
		
			if (result != null) {
				UIImage image = ArtRepository.GetImage (result.Image);
			
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

