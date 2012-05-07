using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreText;
using MonoTouch.CoreGraphics;

namespace SpendingConsequences
{
	public class CoreTextView : UIView
	{
		public CoreTextView () : base()
		{
			Initialize();
		}
		
		public CoreTextView (RectangleF frame) : base(frame)
		{
			Initialize();
		}
		
		private void Initialize ()
		{
			Layer.GeometryFlipped = true;
			this.BackgroundColor = UIColor.Clear;
		}
		
		public NSAttributedString Text { 
			get {
				return _text;
			}
			set {
				_text = value;
				Framesetter = new CTFramesetter (this.Text);
				SetNeedsDisplay ();
			}
		}
		
		public SizeF SuggestedFrameSize (SizeF constraints)
		{
			if (this.Text == null)
				return this.Frame.Size;
			
			CTFrameAttributes attribs = new CTFrameAttributes ();
			attribs.Progression = CTFrameProgression.TopToBottom;
			NSRange fitRange;
			return Framesetter.SuggestFrameSize (new NSRange (0, Text.Length), attribs, constraints, out fitRange);
		}
		
		private CTFramesetter Framesetter { get; set; }
		
		private NSAttributedString _text = null;
		
		public override void Draw (System.Drawing.RectangleF rect)
		{
			base.Draw (rect);
			
			CGContext context = UIGraphics.GetCurrentContext ();
			// Apply a transform that flips the Y axis, since CT uses a different coords system
//			context.TextMatrix = CGAffineTransform.MakeIdentity ();
//			context.TranslateCTM (0, this.Bounds.Size.Height);
//			context.ScaleCTM (1.0f, -1.0f);
			
			CGPath path = new CGPath ();
			path.AddRect (this.Bounds);
			
			try {
				CTFrame frame = Framesetter.GetFrame (new NSRange (0, this._text.Length), path, null);
			
				frame.Draw (context);				
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
			}

		}
	}
}

