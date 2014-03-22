using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Scanner {
	public partial class MainForm : Form {
		public MainForm() {
			InitializeComponent();

			string s = WIAScanner.SelectDevice();
			int i = 0;

			List<string> pages = WIAScanner.Scan(s);
			foreach (string fn in pages) {
				string outFileName = string.Format(@"E:\{0}.jpg", i++);
				Image.FromFile(fn).Save(outFileName, ImageFormat.Jpeg);
			}
		}
	}
}
