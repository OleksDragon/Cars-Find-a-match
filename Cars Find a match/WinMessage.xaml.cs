using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Cars_Find_a_match
{
    /// <summary>
    /// Логика взаимодействия для WinMessage.xaml
    /// </summary>
    public partial class WinMessage : Window
    {
        public WinMessage()
        {
            InitializeComponent();
        }
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        Bitmap _bitmap;
        BitmapSource _source;       

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _source = GetSource();
            cupWinner.Source = _source;
            ImageAnimator.Animate(_bitmap, OnFrameChanged);
        }

        private BitmapSource GetSource()
        {
            try
            {
                if (_bitmap == null)
                {
                    _bitmap = new Bitmap(Application.GetResourceStream(new Uri("winner.gif", UriKind.Relative)).Stream);
                }
                IntPtr handle = IntPtr.Zero;
                handle = _bitmap.GetHbitmap();

                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                // надо удалить дескриптор точечного рисунка, который создает функция Bitmap.GetHbitmap иначе память, занятая им, не освободится
                DeleteObject(handle);

                return bitmapSource;
            }
            catch { }
            return _source;
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(FrameUpdatedCallback));
        }

        private void FrameUpdatedCallback()
        {
            ImageAnimator.UpdateFrames();
            if (_source == null)
            {
                _source.Freeze();
            }
            _source = GetSource();
            cupWinner.Source = _source;
            InvalidateVisual();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
