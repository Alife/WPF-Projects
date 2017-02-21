using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Net;
using System.Windows.Interop;
using QianQianLrc;

namespace MusicSorter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isBatchDeleteMode = false;// 批处理模式下不显示确认对话框
        string OpenFileFilter = "Music Files|*.mp3;*.m4a;*.flac"; //仅支持MP3文件
        public MainWindow()
        {
            this.InitializeComponent();
            WCM = new Windows_Custom_Messagebox();
            // 在此点下面插入创建对象所需的代码。
            Sb = (Storyboard)FindResource("Storyboard_Minimize");
            Sb2 = (Storyboard)FindResource("Storyboard_BeginDownload");
            Sb3 = (Storyboard)FindResource("Storyboard_EndDownload");
            Sb4 = (Storyboard)FindResource("Storyboard_BeginEditLyrics");
            Sb5 = (Storyboard)FindResource("Storyboard_EndEditLyrics");
        }
        Storyboard Sb, Sb2, Sb3, Sb4, Sb5;
        Windows_Custom_Messagebox WCM;
        public ObservableCollection<Song> SongList = new ObservableCollection<Song>();
        private Thread AlbumPicturesAndLyricsThread;
        private Thread AutoDownloadThread;
        private Thread CheckUpdateThread;
        private int AlbumIndex = 0;
        private string[] Album_Results;
        private bool AutoDownloading = false;

        #region 检查更新
        private void CheckUpdate()
        {

            string s = WebHelper.Analytics("http://api.4321.la/analytics-musicsorter.php", "20110607");
            if (s == "Update")
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
                {
                    ShowMessageBox("系统检测到软件已经发布新版本，请立即更新\n单击确定退出软件并引导您到官网下载最新版本！\n\n官方网站：http://www.4321.la");
                    System.Diagnostics.Process.Start("http://www.4321.la");
                    Application.Current.Shutdown();

                });
            }
        }

        #endregion
        #region 文本框焦点特效
        private TextBox tb;
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Image_Small_Tip.Visibility = System.Windows.Visibility.Hidden;

            tb = (TextBox)sender;
            DoubleAnimation ta = new DoubleAnimation();
            ta.From = 0.5;
            ta.To = 0;
            ta.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            Storyboard.SetTargetName(ta, "Opacity");
            Storyboard.SetTargetProperty(ta, new PropertyPath(Border.OpacityProperty));
            Storyboard storyBoard = new Storyboard();
            storyBoard.Children.Add(ta);
            this.RegisterName("Opacity", this.ColorFulBorder);
            storyBoard.Begin(this, true);

        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {

            double fleft, ftop, fbottom, fright, fwidth;
            TextBox ntb = (TextBox)sender;
            if (tb == null)
            {
                fleft = ntb.Margin.Left;
                ftop = ntb.Margin.Top;
                fbottom = ntb.Margin.Bottom;
                fright = ntb.Margin.Right;
                fwidth = ntb.Width;
            }
            else
            {
                //获得失去焦点的文本框的位置
                fleft = tb.Margin.Left;
                ftop = tb.Margin.Top;
                fbottom = tb.Margin.Bottom;
                fright = tb.Margin.Right;
                fwidth = tb.Width;

            }
            //获得获得焦点的文本框的位置
            double nleft = ntb.Margin.Left;
            double ntop = ntb.Margin.Top;
            double nbottom = ntb.Margin.Bottom;
            double nright = ntb.Margin.Right;
            double nwidth = ntb.Width;
            //调整提示图片位置
            Image_Small_Tip.Margin = new Thickness(0, 0, nright, nbottom - 23);
            Image_Small_Tip.Visibility = System.Windows.Visibility.Visible;
            //开启动画效果

            //ThicknessAnimation ta = new ThicknessAnimation();
            SplineThicknessKeyFrame stk1 = new SplineThicknessKeyFrame();
            SplineThicknessKeyFrame stk2 = new SplineThicknessKeyFrame();
            ThicknessAnimationUsingKeyFrames du = new ThicknessAnimationUsingKeyFrames();


            stk1.Value = new Thickness(fleft, ftop, fright, fbottom);
            stk2.Value = new Thickness(nleft, ntop, nright, nbottom);

            stk1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0));
            stk2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400));
            stk2.KeySpline = new KeySpline(0, 0, 0, 1);

            du.KeyFrames.Add(stk1);
            du.KeyFrames.Add(stk2);

            //ta.From = new Thickness(fleft,ftop,0,0);
            //ta.To = new Thickness(nleft, ntop, 0, 0);
            //ta.Duration = new Duration(TimeSpan.FromMilliseconds(200));



            Storyboard.SetTargetName(du, "Margin");
            Storyboard.SetTargetProperty(du, new PropertyPath(Border.MarginProperty));

            Storyboard storyBoard = new Storyboard();

            storyBoard.Children.Add(du);

            this.RegisterName("Margin", this.ColorFulBorder);


            DoubleAnimation ta = new DoubleAnimation();
            ta.From = 0;
            ta.To = 0.25;
            ta.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            Storyboard.SetTargetName(ta, "Opacity");
            Storyboard.SetTargetProperty(ta, new PropertyPath(Border.OpacityProperty));
            Storyboard storyBoard2 = new Storyboard();
            storyBoard2.Children.Add(ta);
            this.RegisterName("Opacity", this.ColorFulBorder);
            storyBoard2.Begin(this, true);


            DoubleAnimation tw = new DoubleAnimation();
            tw.From = fwidth;
            tw.To = nwidth;
            tw.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            Storyboard.SetTargetName(tw, "Width");
            Storyboard.SetTargetProperty(tw, new PropertyPath(Border.WidthProperty));
            Storyboard storyBoard3 = new Storyboard();
            storyBoard3.Children.Add(tw);
            this.RegisterName("Width", this.ColorFulBorder);
            storyBoard3.Begin(this, true);
            storyBoard.Begin(this, true);


        }

        #endregion
        #region 标题栏拖拽相应
        private const int WM_NCHITTEST = 0x0084;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST)
            {
                // 获取屏幕坐标
                Point p = new Point();
                int pInt = lParam.ToInt32();
                p.X = (pInt << 16) >> 16;
                p.Y = pInt >> 16;
                if (isOnTitleBar(PointFromScreen(p)))
                {
                    // 欺骗系统鼠标在标题栏上
                    handled = true;
                    return new IntPtr(2);
                }
            }

            return IntPtr.Zero;
        }

        private bool isOnTitleBar(Point p)
        {
            // 假设标题栏在0和34之间
            if (p.Y >= 0 && p.Y < 34 && p.X < 520)
                return true;
            else
                return false;
        }
        #endregion
        #region 界面相关

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //窗口载入，获得句柄，用来给标题栏拖拽服务
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(new HwndSourceHook(WndProc));

            //设置数据源
            DataListBox.ItemsSource = SongList;

            if (!(CheckUpdateThread == null)) if (CheckUpdateThread.IsAlive) CheckUpdateThread.Abort();
            CheckUpdateThread = new Thread(CheckUpdate);
            CheckUpdateThread.Start();
        }

        private void Image_minimize_MouseEnter(object sender, MouseEventArgs e)
        {
            Image_minimize.Source = new BitmapImage(new Uri("Skin/minimize_2.png", UriKind.Relative));
        }

        private void Image_minimize_MouseLeave(object sender, MouseEventArgs e)
        {
            Image_minimize.Source = new BitmapImage(new Uri("Skin/minimize_1.png", UriKind.Relative));
        }

        private void Image_minimize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image_minimize.Source = new BitmapImage(new Uri("Skin/minimize_1.png", UriKind.Relative));
        }

        private void Image_minimize_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Sb.Begin();
        }

        private void Storyboard_Minimize_Completed(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
            Sb.Stop();
            this.Opacity = 1;
        }
        private void Image_close_MouseEnter(object sender, MouseEventArgs e)
        {
            Image_close.Source = new BitmapImage(new Uri("Skin/close_2.png", UriKind.Relative));
        }

        private void Image_close_MouseLeave(object sender, MouseEventArgs e)
        {
            Image_close.Source = new BitmapImage(new Uri("Skin/close_1.png", UriKind.Relative));
        }

        private void Image_close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image_close.Source = new BitmapImage(new Uri("Skin/close_1.png", UriKind.Relative));
        }

        private void Image_close_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion
        #region Helper
        /// <summary>
        /// Butes[] To Stream
        /// </summary>
        /// <param name="bytes">Bytes[]</param>
        /// <returns></returns>
        public Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

        #endregion
        #region MessageBox
        private void ShowMessageBox(string s)
        {
            WCM.Label_Content.Text = s;
            WCM.Button_Cancel.Visibility = Visibility.Hidden;
            WCM.Button_OK.Margin = new Thickness(0, 0, 7, 4);
            WCM.ShowDialog();
        }
        private int ShowMessageBoxOkCancel(string s)
        {
            WCM.Label_Content.Text = s;
            WCM.Button_Cancel.Visibility = Visibility.Visible;
            WCM.Button_OK.Margin = new Thickness(0, 0, 70, 4);
            WCM.ShowDialog();
            return WCM.Return_Result;
        }
        #endregion
        #region 添加歌曲
        /// <summary>
        /// String.IsEmptyOrNull不能判断第二种情况，重新写一个来拟补不足
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool IsStringNullOrEmpry(string s)
        {
            if (string.IsNullOrEmpty(s)) return true;
            if ((s.Length == 1) && (s[0] == '\0')) return true;
            return false;

        }

        private void AddSongs(string[] FileNames)
        {
            if (FileNames == null) return;
            if (FileNames.Length >= 100)
            {
                if (ShowMessageBoxOkCancel("您选择的歌曲数量超过100，这样可能会导致软件占用较多内存，确定要继续吗？\n（推荐您分次处理，每次处理的歌曲控制在100以内）") == 0)
                {
                    return;
                }
            }
            char[] CharToDelete = { '\0', ' ' };

            Image_Helper.Visibility = Visibility.Hidden;
            Label_URL.Visibility = Visibility.Hidden;

            foreach (string FileName in FileNames)
            {
                if (!OpenFileFilter.Split('|').Last().Replace("*","").ToUpper().Split(';').Contains(System.IO.Path.GetExtension(FileName).ToUpper())) continue;
                BitmapImage[] HasAlbum = null;
                string HasLyrics = null;


                string Title = "", Artist = "", Album = "";
                TagLib.File file = null; var msg = ""; try { file = TagLib.File.Create(FileName); }
                catch (Exception ex) { msg = ex.Message; continue; }
                Title = IsStringNullOrEmpry(Title) ? file.Tag.Title : Title;
                Artist = IsStringNullOrEmpry(Artist) ? file.Tag.FirstPerformer : Artist;
                Album = IsStringNullOrEmpry(Album) ? file.Tag.Album : Album;
                HasLyrics = IsStringNullOrEmpry(HasLyrics) ? file.Tag.Lyrics : HasLyrics;
                if (HasAlbum == null){HasAlbum=new BitmapImage[file.Tag.Pictures.Length];for (int i = 0; i < file.Tag.Pictures.Length; i++){ 
                    MemoryStream ms = new MemoryStream(file.Tag.Pictures[i].Data.Data);
                    HasAlbum[i] = new BitmapImage(); HasAlbum[i].BeginInit(); HasAlbum[i].StreamSource = (ms); 
                    try{HasAlbum[i].EndInit();}catch{HasAlbum[i] = null;}
                }}

                if (IsStringNullOrEmpry(Title)) Title = "";
                if (IsStringNullOrEmpry(Artist)) Artist = "";
                if (IsStringNullOrEmpry(Album)) Album = "";

                SongList.Add(new Song(FileName, Title.TrimEnd(CharToDelete), Artist.TrimEnd(CharToDelete), Album.TrimEnd(CharToDelete), HasAlbum == null ? null : new ObservableCollection<BitmapImage>(HasAlbum), HasLyrics));



                System.Windows.Forms.Application.DoEvents();

                //这个地方加多线程实在是不好加 因为有一个OpenFileDialog 而且结果是一个string[] 直接用object传到多线程里面会出问题
                //其实DoEvents的效果也挺好的 就用这个代替了！
            }
        }

    private void Button_AddSongs_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFile = new System.Windows.Forms.OpenFileDialog();
            OpenFile.Filter = OpenFileFilter; //仅支持MP3文件
            OpenFile.FilterIndex = 1;
            OpenFile.RestoreDirectory = true;
            OpenFile.Multiselect = true; //多文件选择
            if (OpenFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AddSongs(OpenFile.FileNames);
            }

        }
        #endregion
        #region 歌曲封面
        /*
         * 0e69498a1729a8292b3bdc9e457cf87b
         * 0677a8c1ebc102ed1366fa45c34308c7
         */

        struct Parameter
        {
            public int SelectedIndex;
            public bool GetAlbumPictures;
            public bool GetLyrics;
        };
        struct SongDetails
        {
            public string Title;
            public string Artist;
            public string Album;
        };


        /// <summary>
        /// 根据一个关键词获取匹配程度从高到低的三个专辑封面地址
        /// </summary>
        /// <param name="Keyword"></param>
        /// <returns></returns>
        private string[] GetMultiAlbumPictures(string KeyWord)
        {
            string DouBanAPI = "http://api.douban.com/music/subjects?start-index=1&max-results=10&apikey=0e69498a1729a8292b3bdc9e457cf87b&q="; //豆瓣API
            DouBanAPI += KeyWord;
            string code = WebHelper.GetPageHTMLSource(DouBanAPI);
            Regex regex = new Regex("rel=\"alternate\"/>(.*?)<link href=\"(?<data>.*?)\" rel=\"image\"/>", RegexOptions.IgnoreCase);

            if (regex.IsMatch(code))
            {
                MatchCollection matches = regex.Matches(code);
                string[] Results = new string[matches.Count];
                try
                {
                    //string PicURL = regex.Match(code).Groups["data"].Value/*.Replace("/spic/", "/lpic/")*/;
                    //if ((PicURL == null) || PicURL.IndexOf("music-default") > 0) return null;
                    //return PicURL.Replace("/spic/", "/lpic/");
                    for (int i = 0; i < matches.Count; i++)
                    {
                        Results[i] = matches[i].Groups["data"].Value.Replace("/spic/", "/lpic/");
                    }
                    return Results;
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 根据关键字获取最佳匹配的专辑封面地址
        /// </summary>
        /// <param name="KeyWord"></param>
        /// <returns></returns>
        private string GetSingleAlbumPicture(string KeyWord)
        {
            string DouBanAPI = "http://api.douban.com/music/subjects?start-index=1&max-results=1&apikey=0e69498a1729a8292b3bdc9e457cf87b&q="; //豆瓣API
            DouBanAPI += KeyWord;
            string code = WebHelper.GetPageHTMLSource(DouBanAPI);
            Regex regex = new Regex("rel=\"alternate\"/>(.*?)<link href=\"(?<data>.*?)\" rel=\"image\"/>", RegexOptions.IgnoreCase);

            if (regex.IsMatch(code))
            {
                try
                {
                    string PicURL = regex.Match(code).Groups["data"].Value/*.Replace("/spic/", "/lpic/")*/;
                    if ((PicURL == null) || PicURL.IndexOf("music-default") > 0) return null;
                    return PicURL.Replace("/spic/", "/lpic/");
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 根据歌曲获得最多不超过六个的专辑封面地址（分别根据专辑名 *10 、歌手名、歌曲名+专辑名、歌曲名+歌手名获取）
        /// </summary>
        /// <param name="SelectedIndex"></param>
        /// <returns></returns>
        private string[] GetTotalAlbumPictures(SongDetails SongDetail)
        {
            string[] Results = new string[13];
            string Result = string.Empty;


            string[] TmpResult = GetMultiAlbumPictures(string.IsNullOrEmpty(SongDetail.Album) ? SongDetail.Title : SongDetail.Album);

            int i = -1;
            if (TmpResult != null)
            {
                for (i = 0; i < TmpResult.Length; i++)
                {
                    Results[i] = TmpResult[i];
                }
            }


            //Warning: 虽然这样可以提高专辑封面的匹配准确度，但是会被豆瓣API封，测试过程中暂不开放
            /*
            i++;
            Result = GetSingleAlbumPicture(SongDetail.Artist);
            Results[i] = Result;
            i++;
            Result = GetSingleAlbumPicture(SongDetail.Title + " " + SongDetail.Album);
            Results[i] = Result;
            i++;
            Result = GetSingleAlbumPicture(SongDetail.Title + " " + SongDetail.Artist);
            Results[i] = Result; 
            */

            List<string> list = new List<string>();
            for (int j = 0; j < Results.Length; j++)
            {
                if ((!string.IsNullOrEmpty(Results[j])) && (list.IndexOf(Results[j].ToLower()) == -1) && (Results[j].IndexOf("music-default") == -1))
                    list.Add(Results[j]);
            }
            return list.ToArray();
        }

        #endregion

        private void GetAlbumPicturesAndLyrics(object p)
        {

            Parameter parameter = (Parameter)p;
            int NowSelect = parameter.SelectedIndex;

            SongDetails sd;
            sd.Title = SongList[NowSelect].SongTitle;
            sd.Artist = SongList[NowSelect].SongArtist;
            sd.Album = SongList[NowSelect].SongAlbumName;

            if (parameter.GetAlbumPictures) //需要下载专辑封面，开始联网
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    Checkbox_albumpicture.Content = "正在下载专辑封面..";
                    Checkbox_albumpicture.IsEnabled = false;
                    Checkbox_albumpicture.IsChecked = false;
                });

                string[] Results = GetTotalAlbumPictures(sd);

                Album_Results = Results;
                AlbumIndex = 0;
                if (Results == null||Results.Length ==0)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
                    {
                        //未找到专辑封面
                        Image_album.Source = new BitmapImage(new Uri("Skin/AlbumNoPicture.png", UriKind.RelativeOrAbsolute));
                        Checkbox_albumpicture.Content = "未找到专辑封面";
                        Checkbox_albumpicture.IsEnabled = false;
                        Checkbox_albumpicture.IsChecked = false;
                    });
                }
                else
                {
                    SongList[NowSelect].AlbumPictureUrl = Results;
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
                    {
                        //BitmapImage TmpBitmap = new BitmapImage(new Uri("Skin/AlbumNoPicture.png", UriKind.RelativeOrAbsolute)); //全局变量 用于判断专辑图片下载进度
                        //TmpBitmap = new BitmapImage(new Uri(Results[AlbumIndex], UriKind.RelativeOrAbsolute));
                        Image_album.Source = new BitmapImage(new Uri(Results[AlbumIndex], UriKind.RelativeOrAbsolute));
                        Checkbox_albumpicture.Content = "保存专辑封面";
                        Checkbox_albumpicture.IsEnabled = true;
                        Checkbox_albumpicture.IsChecked = true;
                        if (Results.Length > 1)
                        {
                            //显示左右按钮
                            Image_Album_Previous.Visibility = Visibility.Visible;
                            Image_Album_Next.Visibility = Visibility.Visible;
                        }
                    });
                }

            }
            if (parameter.GetLyrics) //需要下载歌词，开始联网
            {

                this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
                {
                    Checkbox_lyrics.Content = "正在下载歌曲歌词..";
                    Checkbox_lyrics.IsEnabled = false;
                    Checkbox_lyrics.IsChecked = false;
                });

                Regex r = new Regex(@"(\(|（).*?(\)|）)");
                Regex rr = new Regex(@"\[.*?\]", RegexOptions.IgnoreCase);


                string Lyrics = QianQianLrc.SearchHelper.DownloadLrc(r.Replace(sd.Artist, ""), r.Replace(sd.Title, ""));

                this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
                {
                    if (string.IsNullOrEmpty(Lyrics))
                    {
                        Checkbox_lyrics.Content = "未找到歌曲歌词";
                        Checkbox_lyrics.IsEnabled = false;
                        Checkbox_lyrics.IsChecked = false;
                    }
                    else
                    {

                        Lyrics = rr.Replace(Lyrics, "").Trim();
                        SongList[NowSelect].SongLyricsUnsaved = Lyrics.Replace("\n\n","\n");
                        Checkbox_lyrics.Content = "保存歌曲歌词";
                        Checkbox_lyrics.IsEnabled = true;
                        Checkbox_lyrics.IsChecked = true;
                    }
                });

            }
            AutoDownloading = false;
        }

        private void DataListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int NowSelect = DataListBox.SelectedIndex;
            if (NowSelect < 0) return;
            TextBox_Title.Text = SongList[NowSelect].SongTitle;
            TextBox_Artist.Text = SongList[NowSelect].SongArtist;
            TextBox_Album.Text = SongList[NowSelect].SongAlbumName;
            Image_album.Source = SongList[NowSelect].SongAlbumFirst;
            bool NeedToGetAlbumPictures = true, NeedToGetLyrics = true;
            if (SongList[NowSelect].HasSongAlbumBool)
            {
                Checkbox_albumpicture.Content = "歌曲已含有封面";
                Checkbox_albumpicture.IsEnabled = false;
                Checkbox_albumpicture.IsChecked = true;
                NeedToGetAlbumPictures = false;
            }
            else
            {
                Checkbox_albumpicture.Content = "准备中……";
            }
            if (SongList[NowSelect].HasSongLyricsBool)
            {
                Checkbox_lyrics.Content = "歌曲已含有歌词";
                Checkbox_lyrics.IsChecked = true;
                Checkbox_lyrics.IsEnabled = false;
                NeedToGetLyrics = false;
            }
            else
            {
                Checkbox_lyrics.Content = "准备中……";
            }
            Image_Album_Previous.Visibility = Visibility.Collapsed;
            Image_Album_Next.Visibility = Visibility.Collapsed;
            if (SongList[NowSelect].SongAlbum.Count > 2){//显示左右按钮
                Image_Album_Previous.Visibility = Visibility.Visible;
                Image_Album_Next.Visibility = Visibility.Visible;
            }

            if ((NeedToGetAlbumPictures || NeedToGetLyrics) && !isBatchDeleteMode)
            {
                //需要更新专辑封面或者歌词
                Parameter parameter;
                parameter.SelectedIndex = NowSelect;
                parameter.GetAlbumPictures = NeedToGetAlbumPictures;
                parameter.GetLyrics = NeedToGetLyrics;
                if (!(AlbumPicturesAndLyricsThread == null)) if (AlbumPicturesAndLyricsThread.IsAlive) AlbumPicturesAndLyricsThread.Abort();
                AlbumPicturesAndLyricsThread = new Thread(GetAlbumPicturesAndLyrics);
                AlbumPicturesAndLyricsThread.Start(parameter);
            }
            else
            {
                AutoDownloading = false;
            }


        }

        #region 按钮事件，不包括添加按钮
        private void SaveSong_Click()
        {
            isBatchDeleteMode = !true;
            //保存信息 
            //悲催的代码让我的歌曲化为灰烬……
            try
            {
                int NowSelect = DataListBox.SelectedIndex;
                if (NowSelect < 0) return;
                string FileName = SongList[NowSelect].SongPath;
                bool NeedToSave = false;

                var tag = new TagLibHelper(FileName);

                if (SongList[NowSelect].SongTitle != TextBox_Title.Text.Trim() || 
                    SongList[NowSelect].SongArtist != TextBox_Artist.Text.Trim() || 
                    SongList[NowSelect].SongAlbumName != TextBox_Album.Text.Trim()) { 
                    tag.file.Tag.Title = TextBox_Title.Text.Trim();
                    if (tag.file.Tag.Performers.Length == 0) tag.file.Tag.Performers = new string[]{TextBox_Artist.Text.Trim()};
                    else tag.file.Tag.Performers[0] = TextBox_Artist.Text.Trim();
                    tag.file.Tag.Album = TextBox_Album.Text.Trim();
                    SongList[NowSelect].SongTitle = TextBox_Title.Text.Trim();
                    SongList[NowSelect].SongArtist = TextBox_Artist.Text.Trim();
                    SongList[NowSelect].SongAlbumName = TextBox_Album.Text.Trim();
                    tag.file.Save();
                }

                if (Checkbox_lyrics.IsChecked == true && (string)Checkbox_lyrics.Content == "保存歌曲歌词" && !string.IsNullOrEmpty(SongList[NowSelect].SongLyricsUnsaved))
                {
                    NeedToSave = true;
                    if (NeedToSave) tag.SaveLyrics(SongList[NowSelect].SongLyricsUnsaved);
                    SongList[NowSelect].SongLyrics = SongList[NowSelect].SongLyricsUnsaved;
                    Checkbox_lyrics.Content = "歌曲歌词已保存";
                }
                if (Checkbox_albumpicture.IsChecked == true && (string)Checkbox_albumpicture.Content == "保存专辑封面")
                {
                    NeedToSave = true;
                    var albums = new ObservableCollection<BitmapImage>();
                    var pictures = new List<TagLib.IPicture>();
                    for (int i = 0; i < SongList[NowSelect].AlbumPictureUrl.Length; i++){
                        BitmapImage Bitmap = new BitmapImage(new Uri(SongList[NowSelect].AlbumPictureUrl[i]));
                        while (Bitmap.IsDownloading){Thread.Sleep(50);System.Windows.Forms.Application.DoEvents();}
                        albums.Add(Bitmap);

                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(Bitmap));
                        MemoryStream stream = new MemoryStream();
                        encoder.Save(stream);
               
                        pictures.Add(new TagLib.Picture() {
                            Type = TagLib.PictureType.FrontCover,
                            Description = "Cover",
                            MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                            Data = stream.ToArray()
                        });
                    }
                    SongList[NowSelect].SongAlbum = albums;
                    if (NeedToSave) tag.SavePictures(pictures.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存出错，请将下列信息反馈到官方博客，以帮助我们改进软件。谢谢！\n（官方博客：http://www.4321.la，下列信息Ctrl+C可复制）" + ex.Message);
                if (!(AutoDownloadThread == null)) if (AutoDownloadThread.IsAlive) AutoDownloadThread.Abort();
                System.Diagnostics.Debugger.Break();
            }
        }
        private void Button_SaveSong_Click(object sender, RoutedEventArgs e)
        {
            SaveSong_Click();
        }
        private void Button_Clean_Click(object sender, RoutedEventArgs e)
        {
            if (SongList.Count == 0) return;
            if (ShowMessageBoxOkCancel("\n 确定要清空歌曲列表吗？") == 1)
            {
                SongList.Clear();
                Image_Helper.Visibility = Visibility.Visible;
                Label_URL.Visibility = Visibility.Visible;
                Image_album.Source = new BitmapImage(new Uri("Skin/AlbumNoPicture.png", UriKind.RelativeOrAbsolute));
                TextBox_Title.Text = "";
                TextBox_Artist.Text = "";
                TextBox_Album.Text = "";
                Checkbox_albumpicture.IsChecked = false;
                Checkbox_albumpicture.Content = "选中歌曲以继续……";
                Checkbox_lyrics.IsChecked = false;
                Checkbox_lyrics.Content = "选中歌曲以继续……";
            }
        }
        /// <summary>
        /// 批量下载 - 操控线程
        /// </summary>
        private void AutoDownload()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
            {
                DataListBox.IsEnabled = false;
                Button_SaveSong.IsEnabled = false;
                Image_Album_Previous.IsEnabled = false;
                Image_Album_Next.IsEnabled = false;
            });
            for (int i = 0; i < SongList.Count; i++)
            {

                AutoDownloading = true;

                //设置进度
                this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
                {
                    DataListBox.SelectedIndex = i;
                    DataListBox.ScrollIntoView(DataListBox.SelectedItem);
                    double d = ((double)(i + 1) / (double)SongList.Count * 100);

                    Label_ProcessBar.Content = Math.Truncate(d).ToString() + "%";
                    ProcessBar_Download.Value = d;
                });

                while (AutoDownloading) Thread.Sleep(100);

                this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
                {
                    Button_SaveSong_Click(null, null);
                });

            }
            this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
            {
                DataListBox.IsEnabled = true;
                Button_SaveSong.IsEnabled = true;
                Image_Album_Previous.IsEnabled = true;
                Image_Album_Next.IsEnabled = true;
                Sb3.Begin();
            });

        }

        /// <summary>
        /// 批量下载 - Begin
        /// </summary>
        private void Button_DownloadAll_Click(object sender, RoutedEventArgs e)
        {
            if (SongList.Count == 0) return;
            if (ShowMessageBoxOkCancel("开始批量下载歌曲歌词、歌曲专辑封面吗？\n\n软件属于测试阶段，批量下载结果往往不尽如人意，强烈推荐您在下载前备份您的歌曲或采用手动下载，同时关闭迅雷等占用网络资源的软件以加速下载过程！") == 1)
            {

                DataListBox.SelectedIndex = -1;
                Label_ProcessBar.Content = "0%";
                ProcessBar_Download.Value = 0;
                Sb2.Begin();
                if (!(AutoDownloadThread == null)) if (AutoDownloadThread.IsAlive) AutoDownloadThread.Abort();
                //AutoDownloadThread = new Thread(AutoDownload);
                AutoDownloadThread = new Thread(() => BatchMethod(SaveSong_Click));
                AutoDownloadThread.Start();
            }
        }

        private void Button_DeleteAllAlbum_Click(object sender, RoutedEventArgs e)
        { 
            isBatchDeleteMode = true;
            if (SongList.Count == 0) return;
            if (ShowMessageBoxOkCancel("确定批量清除歌曲专辑封面吗？") == 1)
            {
                DataListBox.SelectedIndex = -1;
                Label_ProcessBar.Content = "0%";
                ProcessBar_Download.Value = 0;
                Sb2.Begin();
                if (!(AutoDownloadThread == null)) if (AutoDownloadThread.IsAlive) AutoDownloadThread.Abort();
                AutoDownloadThread = new Thread(() => BatchMethod(DeleteAlbum_Click));
                AutoDownloadThread.Start();
            }
        }

        private void Button_DeleteAllLyrics_Click(object sender, RoutedEventArgs e)
        {
            isBatchDeleteMode = true;
            if (SongList.Count == 0) return;
            if (ShowMessageBoxOkCancel("确定批量清除歌曲内嵌歌词吗？") == 1)
            {
                DataListBox.SelectedIndex = -1;
                Label_ProcessBar.Content = "0%";
                ProcessBar_Download.Value = 0;
                Sb2.Begin();
                if (!(AutoDownloadThread == null)) if (AutoDownloadThread.IsAlive) AutoDownloadThread.Abort();
                AutoDownloadThread = new Thread(() => BatchMethod(DeleteLyrics_Click));
                AutoDownloadThread.Start();
            }
        }
        private void BatchMethod(Action method)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
            {
                DataListBox.IsEnabled = false;
                Button_SaveSong.IsEnabled = false;
                Image_Album_Previous.IsEnabled = false;
                Image_Album_Next.IsEnabled = false;
            });
            for (int i = 0; i < SongList.Count; i++)
            {

                AutoDownloading = true;

                //设置进度
                this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
                {
                    DataListBox.SelectedIndex = i;
                    DataListBox.ScrollIntoView(DataListBox.SelectedItem);
                    double d = ((double)(i + 1) / (double)SongList.Count * 100);

                    Label_ProcessBar.Content = Math.Truncate(d).ToString() + "%";
                    ProcessBar_Download.Value = d;
                });
                // 30s 超时
                var timeOut=30;
                while (AutoDownloading) { if (timeOut < 0) { break; } timeOut = timeOut - 1; Thread.Sleep(100); }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Send, method);

            }
            this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
            {
                DataListBox.IsEnabled = true;
                Button_SaveSong.IsEnabled = true;
                Image_Album_Previous.IsEnabled = true;
                Image_Album_Next.IsEnabled = true;
                Sb3.Begin();
            });

        }
        #endregion


        #region 文本框事件
        private void TextBox_Title_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Image_Small_Tip.Visibility = System.Windows.Visibility.Hidden;
            int NowSelect = DataListBox.SelectedIndex;
            if (NowSelect < 0 || NowSelect > SongList.Count - 1) return;
            if (TextBox_Title.Text != SongList[NowSelect].SongTitle)
            {
                SongList[NowSelect].SongTitle = TextBox_Title.Text;
                Parameter parameter;
                parameter.SelectedIndex = NowSelect;
                parameter.GetAlbumPictures = false;
                parameter.GetLyrics = true; //重新下载歌曲歌词
                if (!(AlbumPicturesAndLyricsThread == null)) if (AlbumPicturesAndLyricsThread.IsAlive) AlbumPicturesAndLyricsThread.Abort();
                AlbumPicturesAndLyricsThread = new Thread(GetAlbumPicturesAndLyrics);
                AlbumPicturesAndLyricsThread.Start(parameter);
            }
        }

        private void TextBox_Artist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Image_Small_Tip.Visibility = System.Windows.Visibility.Hidden;
            int NowSelect = DataListBox.SelectedIndex;
            if (NowSelect < 0 || NowSelect > SongList.Count - 1) return;
            if (TextBox_Artist.Text != SongList[NowSelect].SongArtist)
            {
                SongList[NowSelect].SongArtist = TextBox_Artist.Text;
                Parameter parameter;
                parameter.SelectedIndex = NowSelect;
                parameter.GetAlbumPictures = false;
                parameter.GetLyrics = true; //重新下载歌曲歌词
                if (!(AlbumPicturesAndLyricsThread == null)) if (AlbumPicturesAndLyricsThread.IsAlive) AlbumPicturesAndLyricsThread.Abort();
                AlbumPicturesAndLyricsThread = new Thread(GetAlbumPicturesAndLyrics);
                AlbumPicturesAndLyricsThread.Start(parameter);
            }
        }

        private void TextBox_Album_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Image_Small_Tip.Visibility = System.Windows.Visibility.Hidden;
            int NowSelect = DataListBox.SelectedIndex;
            if (NowSelect < 0 || NowSelect > SongList.Count - 1) return;
            //if (TextBox_Album.Text != SongList[NowSelect].SongAlbumName)
            //{
            SongList[NowSelect].SongAlbumName = TextBox_Album.Text;
            Parameter parameter;
            parameter.SelectedIndex = NowSelect;
            parameter.GetAlbumPictures = true; //重新下载歌曲专辑封面
            parameter.GetLyrics = false;
            if (!(AlbumPicturesAndLyricsThread == null)) if (AlbumPicturesAndLyricsThread.IsAlive) AlbumPicturesAndLyricsThread.Abort();
            AlbumPicturesAndLyricsThread = new Thread(GetAlbumPicturesAndLyrics);
            AlbumPicturesAndLyricsThread.Start(parameter);
            //}
        }
        #endregion
        #region 专辑图片切换事件
        private void Image_Album_Next_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int NowSelect = DataListBox.SelectedIndex;
            if (NowSelect < 0) return;
            var Results = SongList[NowSelect].SongAlbum.Count;
            if (Results < 2) return;
            if (AlbumIndex >= Results - 1) AlbumIndex=0; //循环播放
            else AlbumIndex++;
            Image_album.Source = SongList[NowSelect].SongAlbum[AlbumIndex];
            Image_Album_Next.ToolTip = "当前第 " + (AlbumIndex + 1) + " 项，共 " + Results + " 项";
            Image_Album_Previous.ToolTip = "当前第 " + (AlbumIndex + 1) + " 项，共 " + Results + " 项";
        }

        private void Image_Album_Previous_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int NowSelect = DataListBox.SelectedIndex;
            if (NowSelect < 0) return;
            var Results = SongList[NowSelect].SongAlbum.Count;
            //if (Album_Results == null) return;
            if (Results < 2) return;
            if (AlbumIndex <= 0) AlbumIndex = Results-1; //循环播放
            else AlbumIndex--;
            Image_album.Source = SongList[NowSelect].SongAlbum[AlbumIndex];
            Image_Album_Previous.ToolTip = "当前第 " + (AlbumIndex + 1) + " 项，共 " + Results + " 项";
            Image_Album_Previous.ToolTip = "当前第 " + (AlbumIndex + 1) + " 项，共 " + Results + " 项";
        }
        #endregion


        private void DeleteAlbum_Click()
        {
            int SelectedIndex = DataListBox.SelectedIndex;
            if (!File.Exists(SongList[SelectedIndex].SongPath)) SongList.RemoveAt(SelectedIndex);
            if (SelectedIndex < 0 || SelectedIndex > SongList.Count - 1) return;
            if (isBatchDeleteMode||ShowMessageBoxOkCancel("\n确定要删除当前歌曲封面吗？") == 1)
            {
                if (SongList[SelectedIndex].HasSongAlbumBool){
                    var tag = new TagLibHelper(SongList[SelectedIndex].SongPath);
                    tag.SavePictures(null);
                    SongList[SelectedIndex].SongAlbum = null;
                }
                DataListBox.SelectedIndex = -1;
                DataListBox.SelectedIndex = SelectedIndex;
            }
        }
        private void Menu_DeleteAlbum_Click(object sender, RoutedEventArgs e)
        {
            isBatchDeleteMode = !true;
            DeleteAlbum_Click();
        }

        private void DeleteLyrics_Click()
        {
            int SelectedIndex = DataListBox.SelectedIndex;
            if (SelectedIndex < 0 || SelectedIndex > SongList.Count - 1) return;
            if (isBatchDeleteMode||ShowMessageBoxOkCancel("\n确定要删除当前歌曲歌词吗？") == 1)
            {
                if (SongList[SelectedIndex].HasSongLyricsBool){
                    var tag = new TagLibHelper(SongList[SelectedIndex].SongPath);
                    tag.SaveLyrics(null);
                    SongList[SelectedIndex].SongLyrics = null;
                }

                DataListBox.SelectedIndex = -1;
                DataListBox.SelectedIndex = SelectedIndex;
            }
        }
        private void Menu_DeleteLyrics_Click(object sender, RoutedEventArgs e)
        {
            isBatchDeleteMode = !true;
            DeleteLyrics_Click();
        }

        private void Menu_EditLyrics_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = DataListBox.SelectedIndex;
            if (SelectedIndex < 0 || SelectedIndex > SongList.Count - 1) return;
            DataListBox.IsEnabled = false;
            Textbox_EditLyrics.Text = SongList[SelectedIndex].SongLyrics;
            Sb4.Begin();
        }

        private void Button_Lyrics_OK_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = DataListBox.SelectedIndex;
            if (SelectedIndex < 0 || SelectedIndex > SongList.Count - 1) return;

            var tag = new TagLibHelper(SongList[SelectedIndex].SongPath);
            tag.SaveLyrics(Textbox_EditLyrics.Text);
            SongList[SelectedIndex].SongLyrics = Textbox_EditLyrics.Text;
            DataListBox.SelectedIndex = -1;
            DataListBox.SelectedIndex = SelectedIndex;

            Sb5.Begin();
            DataListBox.IsEnabled = true;
        }

        private void Button_Lyrics_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DataListBox.IsEnabled = true;
            Sb5.Begin();
        }

        private void Label_URL_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.4321.la");
        }

        #region 拖拽

        private void Image_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        #endregion

        private void DataListBox_Drop(object sender, DragEventArgs e)
        {
            Array array = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
            AddSongs((string[])array);
        }


    }
    #region 歌曲列表泛型类
    public class Song : INotifyPropertyChanged
    {
        public string SongPath { get; set; }  //歌曲路径
        public string SongName  //歌曲名 通过分析歌曲路径的方法返回 仅用于列表的显示
        {
            get
            {
                return System.IO.Path.GetFileName(SongPath);
            }
            set { }
        }
        public string SongTitle { get; set; }
        public string SongArtist { get; set; }
        public string SongAlbumName { get; set; }
        private ObservableCollection<BitmapImage> songalbum;
        public ObservableCollection<BitmapImage> SongAlbum
        {
            get
            {
                if (songalbum == null)
                {
                    return new ObservableCollection<BitmapImage> { };
                }
                else
                {
                    return songalbum;
                }
            }

            set
            {
                songalbum = value;
                NotifyPropertyChange("SongAlbum");
                NotifyPropertyChange("HasSongAlbum");
            }
        }
        public BitmapImage SongAlbumFirst { get { return SongAlbum.Count > 0 ? SongAlbum[0] : new BitmapImage(new Uri("Skin/AlbumNoPicture.png", UriKind.RelativeOrAbsolute)); } }
        private string songlyrics;
        public string SongLyrics
        {
            get
            {
                return songlyrics;
            }
            set
            {
                songlyrics = value;
                NotifyPropertyChange("SongLyrics");
                NotifyPropertyChange("HasSongLyrics");
            }
        }
        public string SongLyricsUnsaved { get; set; }
        public string HasSongAlbum
        {
            get
            {
                return songalbum == null||songalbum.Count==0 ? null : "Skin/icon_album.png";
            }
            set { }
        }
        public string AlbumTip { get { return "专辑图片 OK!\n共 " + songalbum.Count + " 张专辑图片。"; } }
        public string LyricsTip { get { return "歌曲歌词 OK!\n\n" + songlyrics; } }

        public bool HasSongAlbumBool
        {
            get
            {
                return songalbum != null&&songalbum.Count>0;
            }
            set { }
        }
        public string HasSongLyrics
        {
            get
            {
                return string.IsNullOrEmpty(SongLyrics) ? "" : "Skin/icon_lyrics.png";
            }
            set
            { }
        }
        public bool HasSongLyricsBool
        {
            get
            {
                return !string.IsNullOrEmpty(SongLyrics);
            }
            set { }
        }
        public string[] AlbumPictureUrl { get; set; }
        public int AlbumDownloadedNum { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Song(string SongPath, string SongTitle, string SongArtist, string SongAlbumName, ObservableCollection<BitmapImage> SongAlbum, string SongLyrics)
        {
            this.SongPath = SongPath;
            this.SongTitle = SongTitle;
            this.SongArtist = SongArtist;
            this.SongAlbum = SongAlbum;
            this.SongLyrics = SongLyrics;
            this.SongAlbumName = SongAlbumName;
        }
    }
    #endregion
}