using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Util;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Tesseract;
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using PageSegMode = Tesseract.PageSegMode;

namespace ComputerVision
{
	public static class ObjectRecognizer
	{
		static readonly TesseractEngine _engineEng, _engineRus;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDC(IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint nFlags);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

		[DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
		static ObjectRecognizer()
		{
			try
			{
				_engineEng = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractOnly);
				_engineEng.SetVariable("tessedit_char_whitelist", "$¢.,0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
				_engineRus = new TesseractEngine(@"./tessdata", "rus", EngineMode.TesseractOnly);
				//_engineEng = new Tesseract();
				// _engineEng.Init(@"./tessdata", "eng", true);

                

			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.ToString());
				//Ex.Report(ex);
			}
            
		}


		public static Image TestOpenCv()
		{
			try
			{
				var bmp = (Bitmap) Image.FromFile(@"C:\temp\source.png");

				Image<Gray, byte> src = new Image<Gray, byte>(bmp);
				Image<Gray, byte> tmpl = new Image<Gray, byte>((Bitmap) Image.FromFile(@"C:\temp\template.png"));

				var res = src.MatchTemplate(tmpl, TemplateMatchingType.CcoeffNormed);
				double minVal = 0;
				double maxVal = 0;
				Point minLoc = new Point();
				Point maxLoc = new Point();

				CvInvoke.MinMaxLoc(res, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
				Point matchLoc = maxLoc;
				using (Graphics g = Graphics.FromImage(bmp))
				{
					Color customColor = Color.FromArgb(200, Color.Red);
					SolidBrush shadowBrush = new SolidBrush(customColor);
					g.FillRectangles(shadowBrush, new[] {new RectangleF(matchLoc, tmpl.Size)});
					return bmp;
				}
			}
			catch (Exception exc)
			{
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				exc.ToString();
				throw;
			}
		}

		public static bool Detect_object(this Image areaImage, Image imageObject, double accuracy = 0.9)
		{
			lock (areaImage)
			lock (imageObject)
			{
				if (areaImage.Width < imageObject.Width || areaImage.Height < imageObject.Height)
					return false;
				var src = new Image<Gray, byte>((Bitmap) areaImage);
				var tmpl = new Image<Gray, byte>((Bitmap) imageObject);
				return Detect_object(src, tmpl, accuracy);
			}
		}

		public static double ContainingAcuracy(this Image source, Image template, int matchingType = 5)
		{
			lock (source)
			lock (template)
			{
				var src = new Image<Gray, byte>((Bitmap) source);
				var tmpl = new Image<Gray, byte>((Bitmap) template);
				var processed = src.MatchTemplate(tmpl, (TemplateMatchingType) matchingType); // TemplateMatchingType.CcoeffNormed);
				double minVal = 0;
				double maxVal = 0;
				Point minLoc = new Point();
				Point maxLoc = new Point();

				CvInvoke.MinMaxLoc(processed, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

				var res = matchingType == 0 || matchingType == 1 ? minVal : maxVal;
				return res;
			}
		}


		private static bool Detect_object(Image<Gray, byte> areaImage, Image<Gray, byte> imageObject, double accuracy)
		{

			Point dftSize = new Point(areaImage.Width + (imageObject.Width * 2), areaImage.Height + (imageObject.Height * 2));
			using (Image<Gray, byte> padArray = new Image<Gray, byte>(dftSize.X, dftSize.Y))
			{
				padArray.ROI = new Rectangle(imageObject.Width, imageObject.Height, areaImage.Width, areaImage.Height);
				CvInvoke.cvCopy(areaImage, padArray, IntPtr.Zero);

				padArray.ROI = new Rectangle(0, 0, dftSize.X, dftSize.Y);

				using (Image<Gray, float> resultMatrix = padArray.MatchTemplate(imageObject, TemplateMatchingType.CcoeffNormed))
				{
					resultMatrix.ROI = new Rectangle(imageObject.Width, imageObject.Height, areaImage.Width - imageObject.Width,
						areaImage.Height - imageObject.Height);
					resultMatrix.MinMax(out _, out var max, out _, out _);
					return max.Max() >= accuracy;
				}
			}
		}

		public static Rectangle LocateTemplate(this Image source, Image template, int matchingType = 5, double accuracy = 0.9)
		{
			if (!Detect_object(source, template, accuracy))
				return new Rectangle();
			if (source == null || template == null)
				return new Rectangle();
			var src = new Image<Gray, byte>((Bitmap) source);
			var tmpl = new Image<Gray, byte>((Bitmap) template);
			var processed = src.MatchTemplate(tmpl, (TemplateMatchingType) matchingType); // TemplateMatchingType.CcoeffNormed);
			double minVal = 0;
			double maxVal = 0;
			Point minLoc = new Point();
			Point maxLoc = new Point();

			CvInvoke.MinMaxLoc(processed, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

			Point matchLoc = matchingType == 0 || matchingType == 1 ? minLoc : maxLoc;
			return new Rectangle(matchLoc, tmpl.Size);
		}

		public static Image HighlightTemplate(this Image source, Image template, int matchingType = 5, double accuracy = 0.9)
		{
			var tmp = source.Clone() as Bitmap;
			using (Graphics g = Graphics.FromImage(tmp ?? throw new InvalidOperationException()))
			{
				Color customColor = Color.FromArgb(150, Color.Red);
				SolidBrush shadowBrush = new SolidBrush(customColor);
				g.FillRectangles(shadowBrush, new[] {tmp.LocateTemplate(template, matchingType, accuracy)});
				return tmp;
			}
		}

		public static List<Rectangle> LocateAllTexts(Image src, int minHeight = 7, int maxHeight = 30, int minWidth = 40)
		{
			var img = new Image<Gray, byte>((Bitmap) src);

			var sobel = img.Convert<Gray, byte>()
				.Sobel(1, 1, 1)
				.AbsDiff(new Gray(0.0))
				.Convert<Gray, byte>()
				.ThresholdBinary(new Gray(50), new Gray(155));
			var se = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(10, 2), new Point(-1, -1));
			sobel = sobel.MorphologyEx(MorphOp.Dilate, se, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(255));
			var contours = new VectorOfVectorOfPoint();
			var m = new Mat();

			CvInvoke.FindContours(sobel, contours, m, RetrType.External, ChainApproxMethod.ChainApproxSimple);

			var list = new List<Rectangle>();

			for (var i = 0; i < contours.Size; i++)
			{
				Rectangle brect = CvInvoke.BoundingRectangle(contours[i]);
				double ar = (double) brect.Width / brect.Height;
				if (ar > 2
				    && brect.Width > minWidth
				    && brect.Height > minHeight
				    && brect.Height < maxHeight)
				{
					list.Add(brect);
				}
			}

			return list;
		}

		public static Image DrawRectangle(Image src, Rectangle r)
		{
			var img = new Image<Bgr, byte>((Bitmap) src);

			var imgout = img.CopyBlank();

			CvInvoke.Rectangle(img, r, new MCvScalar(0, 0, 255), 2);
			CvInvoke.Rectangle(imgout, r, new MCvScalar(0, 255, 255), -1);

			var res = img.Bitmap;
			return res;
		}

		public static Image HighLightText(Image src, int minHeight = 3, int maxHeight = 100, int minWidth = 10)
		{
			var list = LocateAllTexts(src, minHeight, maxHeight, minWidth);
			var img = new Image<Bgr, byte>((Bitmap) src);

			var imgout = img.CopyBlank();
			foreach (var r in list)
			{
				imgout = new Image<Bgr, byte>((Bitmap) DrawRectangle(src, r));
			}

			imgout._And(img);
			var res = img.Bitmap;
			return res;
		}


		public static List<Rectangle> LocateSpecificText(this IntPtr wndHandle, string searchText, string lang = "en", int minHeight = 7, int maxHeight = 30, int minWidth = 40)
		{
			if (wndHandle == IntPtr.Zero)
			 return new List<Rectangle>();
			var bmp  =  wndHandle.GetClientImage();
			return bmp.LocateSpecificText(new List<string>{searchText}, lang, minHeight, maxHeight, minWidth);
		}

		public static List<Rectangle> LocateSpecificText(this IntPtr wndHandle, List<string> searchText, string lang = "en", int minHeight = 7, int maxHeight = 30, int minWidth = 40)
		{
			if (wndHandle == IntPtr.Zero)
			return new List<Rectangle>();
			var bmp  =  wndHandle.GetClientImage();
			return bmp.LocateSpecificText(searchText, lang, minHeight, maxHeight, minWidth);
		}

		public static List<Rectangle> LocateSpecificText(this Image img, string searchText, string lang = "en",
			int minHeight = 7, int maxHeight = 30, int minWidth = 40)
		{
			return img.LocateSpecificText(new List<string> {searchText}, lang, minHeight, maxHeight, minWidth);
		}

		public static List<Rectangle> LocateSpecificText(this Image img, List<string> searchText, string lang = "en",
			int minHeight = 7, int maxHeight = 30, int minWidth = 40)
		{
			List<Rectangle> ret = new List<Rectangle>();
			if (!(img is Bitmap bmp) || bmp.Size.IsEmpty)
				return ret;
			var rects = ObjectRecognizer.LocateAllTexts(bmp, minHeight, maxHeight, minWidth)
				.OrderBy(rect => rect.Y)
				.ThenBy(rect => rect.X).ToList();
			var imgInput = new Image<Gray, byte>(bmp);
			var sobel = imgInput
				.Sobel(1, 0, 1)
				.AbsDiff(new Gray(0.0))
				.Convert<Gray, byte>()
				.ThresholdBinary(new Gray(80), new Gray(155));

			foreach (var r in rects)
			{
				//var t = new Rectangle(r.X, r.Y, r.Width,  (int) Math.Ceiling(r.Height * 1.4m));
				if (bmp.IsEmpty())
					continue;
				var tmp = new Image<Gray, byte>(bmp.GetPart(r));

				tmp = tmp
					.Resize(3, Inter.Cubic)
					.ThresholdAdaptive(
						new Gray(255),
						AdaptiveThresholdType.GaussianC,
						ThresholdType.Binary,
						85,
						new Gray(0.0)); //.ThresholdBinary(new Gray(128), new Gray(255))
				;
				tmp = tmp.GetAverage().Intensity < 128 ? tmp.Not() : tmp;
				var txt = RecognizeText(tmp.Bitmap, lang, 0.6f).ToUpper();
				if (txt.Length <= 0 || !searchText.Any(x => txt.Contains(x.ToUpper()))) continue;
				ret.Add(r);
			}
#if DEBUG
			int j = 0;
			var loc = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var name = Path.Combine(
				loc,
				@"NotRecognized",
				$"Searchfortext_`{searchText}`_{j}.jpg");
			while (File.Exists(name))
			{
				j++;
				name = Path.Combine(
					loc,
					@"NotRecognized",
					$"Searchfortext_`{searchText}`_{j}.jpg");
			}

			var textsHighLights = rects
				.Aggregate(img, (src, prc) => (Bitmap) ObjectRecognizer.DrawRectangle(src, prc));
			textsHighLights.Save(name, ImageFormat.Jpeg);
#endif
			return ret;
		}

		public static string RecognizeText(Bitmap source, string lang, float confidence = 0.74f)
		{
			return RecognizeText(source, new FiltersSequence(), lang, "", confidence);
		}


		public static string RecognizeText(Bitmap source, FiltersSequence seq,  string lang, string charlist, float confidence = 0.74f)
		{
			// ReSharper disable once IntroduceOptionalParameters.Global
			return RecognizeText(source, seq,  lang, charlist, confidence, PageSegMode.SingleBlock);
		}
		
		public static string RecognizeText(Bitmap source, FiltersSequence seq, string lang = "", string charlist="", float confidence=0.6f, PageSegMode psMode = PageSegMode.Auto)
		{
			try
			{
				var engine = _engineEng;
				if (lang.ToLower() == "rus" || lang.ToLower() == "ru")
				{
					engine = _engineRus;
				}

				var temp = seq.Count > 0 ? seq.Apply(source) : source;
				lock (engine)
				{
					engine.SetVariable("tessedit_char_whitelist", charlist);

					using (var page = engine.Process(temp, psMode))
					{
						page.AnalyseLayout();
						var tmp = page.GetText();

						var conf = page.GetMeanConfidence();
						page.Dispose();
						if (conf >= confidence)
						{
							return tmp;
						}
					}
				}

				return "";
			}
			catch (InvalidOperationException ioe)
			{
				//Ex.Report(ioe);
				Trace.TraceError(ioe.ToString());
				throw;
			}
			catch (Exception e)
			{
				Trace.TraceError(e.ToString());
				//Ex.Report(e);
				return "";
			}

		}

		public static bool IsEmpty(this Bitmap b)
		{
			Bitmap source = b.Clone(new Rectangle(new Point(), b.Size), PixelFormat.Format32bppArgb);

			FiltersSequence seq = new FiltersSequence
			{
				new BrightnessCorrection(-240),
				Grayscale.CommonAlgorithms.BT709,
				new OtsuThreshold(),
				new Invert()
			};
			source = seq.Apply(source);

			BlobCounter bc = new BlobCounter {FilterBlobs = false};

			try
			{
				bc.ProcessImage(source);
				Rectangle[] rects = bc.GetObjectsRectangles();
				if (rects.Length == 0)
					return true;
				else
					return false;
			}
			catch (InvalidImagePropertiesException)
			{
				return false;
			}
		}

		public static Bitmap GetPart(this Bitmap bmp, Rectangle section)
		{
			try
			{
				lock (bmp)
				{
					using (Bitmap tmp = bmp.Clone() as Bitmap)
					{
						var cr = new Crop(section);

						return cr.Apply(tmp);
					}
				}
			}
			catch
			{
				return null;
			}
		}


		public static Bitmap GetClientImage(this IntPtr hWnd, int xOffset=0, int yOffset = 0)
        {
            IntPtr sourceDC  = GetDC(hWnd);
            IntPtr hMemSrcDC = CreateCompatibleDC(sourceDC);
            IntPtr hOld = new IntPtr() ;
            RECT wndRect;
            GetClientRect(hWnd, out wndRect);
            int Widht = (wndRect.Right - wndRect.Left != 0) ? wndRect.Right - wndRect.Left : 1;
            int Height = (wndRect.Bottom - wndRect.Top != 0) ? wndRect.Bottom - wndRect.Top : 1;
            IntPtr m_HBitmap = CreateCompatibleBitmap(sourceDC, Widht, Height);

	        try
	        {
		        if (m_HBitmap != IntPtr.Zero)
		        {
			        hOld = SelectObject(hMemSrcDC, m_HBitmap);
			        BitBlt(hMemSrcDC, 0, 0, Widht, Height, sourceDC, 0, 0,
				        TernaryRasterOperations.SRCCOPY | TernaryRasterOperations.CAPTUREBLT);
			        SelectObject(hMemSrcDC, hOld);
			       
			        var bmp = System.Drawing.Image.FromHbitmap(m_HBitmap);
			        //**********************************************
			        DeleteDC(m_HBitmap);
			        DeleteObject(m_HBitmap);
		       
			        DeleteDC(hOld);
			        DeleteObject(hOld);
		        
			        DeleteDC(hMemSrcDC);
			        DeleteObject(hMemSrcDC);
		        
			        ReleaseDC(hWnd, sourceDC);
			        DeleteDC(sourceDC);
			        DeleteObject(sourceDC);
		       
			        DeleteDC(hWnd);
			        DeleteObject(hWnd);
			        //**********************************************
			        return bmp;
		        }
	        }
	        finally
	        {
		        DeleteDC(m_HBitmap);
		        DeleteObject(m_HBitmap);
		       
		        DeleteDC(hOld);
		        DeleteObject(hOld);
		        
		        DeleteDC(hMemSrcDC);
		        DeleteObject(hMemSrcDC);
		        
		        DeleteDC(sourceDC);
		        ReleaseDC(hWnd, sourceDC);
		        DeleteObject(sourceDC);
		       
		        DeleteDC(hWnd);
		        DeleteObject(hWnd);
	        }
	        return new Bitmap(Widht, Height);
        }
	}

	public struct RECT
	{
		public readonly int Left;        // x position of upper-left corner
		public readonly int Top;         // y position of upper-left corner
		public readonly int Right;       // x position of lower-right corner
		public readonly int Bottom;      // y position of lower-right corner

		public RECT(int left, int top, int right, int bottom)
		{
			this.Left = left;
			this.Top = top;
			this.Right = right;
			this.Bottom = bottom;
		}
 
		public Rectangle AsRectangle => new Rectangle(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top);

		public static RECT FromXYWH(int x, int y, int width, int height)
		{
			return new RECT(x, y, x + width, y + height);
		}
 
		public static RECT FromRectangle(Rectangle rect)
		{
			return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[Flags]
	public enum TernaryRasterOperations : uint
	{
		/// <summary>dest = source</summary>
		SRCCOPY = 0x00CC0020,
		/// <summary>dest = source OR dest</summary>
		SRCPAINT = 0x00EE0086,
		/// <summary>dest = source AND dest</summary>
		SRCAND = 0x008800C6,
		/// <summary>dest = source XOR dest</summary>
		SRCINVERT = 0x00660046,
		/// <summary>dest = source AND (NOT dest)</summary>
		SRCERASE = 0x00440328,
		/// <summary>dest = (NOT source)</summary>
		NOTSRCCOPY = 0x00330008,
		/// <summary>dest = (NOT src) AND (NOT dest)</summary>
		NOTSRCERASE = 0x001100A6,
		/// <summary>dest = (source AND pattern)</summary>
		MERGECOPY = 0x00C000CA,
		/// <summary>dest = (NOT source) OR dest</summary>
		MERGEPAINT = 0x00BB0226,
		/// <summary>dest = pattern</summary>
		PATCOPY = 0x00F00021,
		/// <summary>dest = DPSnoo</summary>
		PATPAINT = 0x00FB0A09,
		/// <summary>dest = pattern XOR dest</summary>
		PATINVERT = 0x005A0049,
		/// <summary>dest = (NOT dest)</summary>
		DSTINVERT = 0x00550009,
		/// <summary>dest = BLACK</summary>
		BLACKNESS = 0x00000042,
		/// <summary>dest = WHITE</summary>
		WHITENESS = 0x00FF0062,
		/// <summary>
		/// Capture window as seen on screen.  This includes layered windows 
		/// such as WPF windows with AllowsTransparency="true"
		/// </summary>
		CAPTUREBLT = 0x40000000
	}

}
