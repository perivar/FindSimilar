using System;
using System.ComponentModel;
using System.Drawing;

using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace FindSimilar
{
	/// <summary>
	/// Inherits from PictureBox; adds Interpolation Mode Setting
	/// </summary>
	public class PictureBoxWithInterpolationMode : PictureBox
	{
		public InterpolationMode InterpolationMode { get; set; }

		protected override void OnPaint(PaintEventArgs paintEventArgs)
		{
			paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
			base.OnPaint(paintEventArgs);
		}
	}
}
