using System;
using System.Collections.Generic;
using System.Xml.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public static class ArtRepository
	{
		static ArtRepository ()
		{
			UIArtwork = new Dictionary<string, UIImage>();

			UIImage input_panel = UIImage.FromBundle (@"UIArt/input_panel.png");
			UIImage result_panel = UIImage.FromBundle (@"UIArt/result_panel.png");
			UIArtwork.Add("input_background", input_panel);
			UIArtwork.Add("result_background", result_panel);

			UIImage segSelected = UIImage.FromBundle (@"UIArt/mode_sel.png").CreateResizableImage (new UIEdgeInsets (0, 9, 0, 9));
			UIImage segUnselected = UIImage.FromBundle (@"UIArt/mode_unsel.png").CreateResizableImage (new UIEdgeInsets (0,	9, 0, 9));
			UIImage segSelUnsel = UIImage.FromBundle (@"UIArt/mode_sel_unsel.png");
			UIImage segUnselSel = UIImage.FromBundle (@"UIArt/mode_unsel_sel.png");
			UIImage segUnselUnsel = UIImage.FromBundle (@"UIArt/mode_unsel_unsel.png");
			UIArtwork.Add("mode_sel", segSelected);
			UIArtwork.Add("mode_unsel", segUnselected);
			UIArtwork.Add("mode_sel_unsel", segSelUnsel);
			UIArtwork.Add("mode_unsel_sel", segUnselSel);
			UIArtwork.Add("mode_unsel_unsel", segUnselUnsel);

			UIImage mul2unsel = UIImage.FromBundle (@"UIArt/mul2_unsel.png");
			UIImage div2unsel = UIImage.FromBundle (@"UIArt/div2_unsel.png");
			UIArtwork.Add("mul2_unsel", mul2unsel);
			UIArtwork.Add("div2_unsel", div2unsel);

			UIImage detailBackground = UIImage.FromBundle (@"UIArt/detail_background.png");
			UIImage resultBackground = UIImage.FromBundle (@"UIArt/detail_result_panel.png");
			UIArtwork.Add("detail_background", detailBackground);
			UIArtwork.Add("detail_result_background", resultBackground);

			UIImage embossedUnsel = UIImage.FromBundle(@"UIArt/embossed_unsel.png").CreateResizableImage (new UIEdgeInsets (9,9,9,9));
			UIArtwork.Add("embossed_unsel", embossedUnsel);
		}

		private static Dictionary<string,UIImage> UIArtwork { get; set; }

		public static void StyleSegmentedControl(string style, UISegmentedControl control)
		{
			control.SetBackgroundImage (UIArtwork[String.Format("{0}_unsel", style)], UIControlState.Normal, UIBarMetrics.Default);
			control.SetBackgroundImage (UIArtwork[String.Format("{0}_sel", style)], UIControlState.Selected, UIBarMetrics.Default);
			control.SetDividerImage (UIArtwork[String.Format("{0}_unsel_unsel", style)], UIControlState.Normal, UIControlState.Normal,	UIBarMetrics.Default);
			control.SetDividerImage (UIArtwork[String.Format("{0}_sel_unsel", style)],
				UIControlState.Selected,
				UIControlState.Normal,
				UIBarMetrics.Default
			);
			control.SetDividerImage (
				UIArtwork[String.Format("{0}_unsel_sel", style)],
				UIControlState.Normal,
				UIControlState.Selected,
				UIBarMetrics.Default
			);
		}

		public static void StyleView(string style, UIView view)
		{
			view.BackgroundColor = UIColor.FromPatternImage (UIArtwork[String.Format("{0}_background", style)]);
		}

		public static void StyleButton(string style, UIButton button)
		{
			string unselKey = String.Format("{0}_unsel", style);
			string selKey = String.Format("{0}_sel", style);

			if (UIArtwork.ContainsKey(unselKey))
				button.SetBackgroundImage (UIArtwork[unselKey], UIControlState.Normal);

			if (UIArtwork.ContainsKey(selKey))
				button.SetBackgroundImage (UIArtwork[selKey], UIControlState.Highlighted);
		}

		public static UIImage GetImage (Image image)
		{
			if (ImageCache == null)
				ImageCache = new NSCache ();
			
			NSObject key = NSObject.FromObject (image.Name);
			UIImage img = ImageCache.ObjectForKey (key) as UIImage;
			if (img == null) {
				if (NSFileManager.DefaultManager.FileExists (image.ImagePath)) {
					img = UIImage.FromBundle (image.ImagePath);
					ImageCache.SetObjectforKey (img, key);	
				}
				else
					Console.WriteLine("Couldn't find image for {0}", image.ImagePath);
			}
			return img;
		}
		
		/// <summary>
		/// Cache of artwork
		/// </summary>
		private static NSCache ImageCache { get; set; }
	}
}

