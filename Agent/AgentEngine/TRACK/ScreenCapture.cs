using System;
using System.Drawing;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Drawing2D;
using INMC.Communication.Inmc.Client;
using INMC.Communication.Inmc.Communication.EndPoints.Tcp;
using INMC.INMMessage;

namespace AgentEngine
{
	/// <summary>
	/// This class shall keep all the functionality for capturing 
	/// the desktop.
	/// </summary>
	public class ScreenCapture
	{
        #region Fields and Properties        
        public M4RealScreenMon scrImage { get; private set; }

        //private static ScreenCapture _screenCapture = null;
        //private Thread _threadCapture;        
        public bool captureEnable { get; set; }
        #endregion
 
        #region Constructors
        public ScreenCapture()
        {
            //_screenCapture = this;

            scrImage = new M4RealScreenMon();

            //_threadCapture = new Thread(new ThreadStart(CaptureHandler));
            captureEnable = false;
            //_threadCapture.Start();
        }

        #endregion
 
        #region public Methods
        public void CaptureScreen(bool pCapture)
        {
            if (pCapture == true)
            {
                captureEnable = true;
            }
            else
            {
                captureEnable = false;
            }
        }
        
        #endregion
 
        #region Private Methods
        private struct SIZE
        {
            public int cx;
            public int cy;
        }

        private Bitmap CaptureDesktop()
        {
            SIZE size;
            IntPtr hBitmap = IntPtr.Zero;            

            size.cx = Win32.GetSystemMetrics(Win32.SM_CXSCREEN);
            size.cy = Win32.GetSystemMetrics(Win32.SM_CYSCREEN);

            Bitmap bmpScreen = new Bitmap(size.cx, size.cy);

            IntPtr hDC = Win32.GetDC(Win32.GetDesktopWindow());
            if (hDC == IntPtr.Zero)
                return bmpScreen;

            IntPtr hMemDC = Win32.CreateCompatibleDC(hDC);
            if (hMemDC == IntPtr.Zero)
            {
                Win32.ReleaseDC(Win32.GetDesktopWindow(), hDC);
                return bmpScreen;
            }

            hBitmap = Win32.CreateCompatibleBitmap(hDC, size.cx, size.cy);
            if (hBitmap == IntPtr.Zero)
            {
                Win32.DeleteDC(hMemDC); 
                Win32.ReleaseDC(Win32.GetDesktopWindow(), hDC);
                return bmpScreen;
            }

            IntPtr hOld = (IntPtr)Win32.SelectObject(hMemDC, hBitmap);

            Win32.BitBlt(hMemDC, 0, 0, size.cx, size.cy, hDC, 0, 0, Win32.SRCCOPY);
            bmpScreen = System.Drawing.Image.FromHbitmap(hBitmap);

            Win32.SelectObject(hMemDC, hOld);
            Win32.DeleteObject(hBitmap);
            Win32.DeleteDC(hMemDC);                
            Win32.ReleaseDC(Win32.GetDesktopWindow(), hDC);

            GC.Collect();
            return bmpScreen;
        }

        private Bitmap CaptureCursor(ref int x, ref int y)
        {
            Bitmap bmp;
            IntPtr hicon;
            Win32.CURSORINFO ci = new Win32.CURSORINFO();
            Win32.ICONINFO icInfo;
            ci.cbSize = Marshal.SizeOf(ci);
            if (Win32.GetCursorInfo(out ci))
            {
                if (ci.flags == Win32.CURSOR_SHOWING)
                {
                    hicon = Win32.CopyIcon(ci.hCursor);
                    if (Win32.GetIconInfo(hicon, out icInfo))
                    {
                        x = ci.ptScreenPos.x - ((int)icInfo.xHotspot);
                        y = ci.ptScreenPos.y - ((int)icInfo.yHotspot);

                        Icon ic = Icon.FromHandle(hicon);
                        bmp = ic.ToBitmap();
                        return bmp;
                    }
                }
            }

            return null;
        }

        private Bitmap CaptureDesktopWithCursor()
        {
            int cursorX = 0;
            int cursorY = 0;
            Bitmap desktopBMP;
            Bitmap cursorBMP;
            Graphics g;
            Rectangle r;

            desktopBMP = CaptureDesktop();
            cursorBMP = CaptureCursor(ref cursorX, ref cursorY);
            if (desktopBMP != null)
            {
                if (cursorBMP != null)
                {
                    r = new Rectangle(cursorX, cursorY, cursorBMP.Width, cursorBMP.Height);
                    g = Graphics.FromImage(desktopBMP);
                    g.DrawImage(cursorBMP, r);
                    g.Flush();

                    return desktopBMP;
                }
                else
                    return desktopBMP;
            }

            return null;

        }
       
        private Image Resize(MemoryStream pStream, int width, int height)
        {
            try
            {
                Image img = Image.FromStream(pStream);

                int originalW = img.Width;
                int originalH = img.Height;
                int resizedW = width;
                int resizedH = height;

                Bitmap bmp = new Bitmap(resizedW, resizedH);
                Graphics graphic = Graphics.FromImage((Image)bmp);
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.DrawImage(img, 0, 0, resizedW, resizedH);
                graphic.Dispose();

                return (Image)bmp;
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        } 

        #endregion
 
        #region Event Methods
        public void CaptureHandler()
        {
            try
            {
                if (captureEnable == true)
                {
                    //var bm = GetDesktopImage();
                    var bm = CaptureDesktopWithCursor();

                    MemoryStream mStream = new MemoryStream();
                    bm.Save(mStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                    scrImage.scrImg = Resize(mStream, 800, 600);
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }            
        }
        #endregion        
	}
}
