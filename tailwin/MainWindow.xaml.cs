using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace tailwin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();

        private string _filename;
        private string filename
        {
            get
            {
                return _filename;
            }

            set
            {
                _filename = value;
                this.Title = string.Format("tailing - {0}", value);
            }
        }
        private bool Streaming = false;
        private bool Stopped = true;

        public MainWindow()
        {
            InitializeComponent();

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                filename = args[1];
                worker.RunWorkerAsync();
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Streaming = true;
            Stopped = false;
            streamFileToOutput(filename, 500);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stopped = true;
        }

        private void mainmenuOpen_Click(object sender, RoutedEventArgs e)
        {
            // Stop streaming current file
            Streaming = false;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (Stopped)
                {
                    filename = openFileDialog.FileName;
                    worker.RunWorkerAsync();
                }
            }
        }

        private void streamFileToOutput(string filename = "", int nlines = 0)
        {
            try
            {
                Clear();
                ShowOutput();

                var wh = new AutoResetEvent(false);
                var fsw = new FileSystemWatcher(".");
                fsw.Filter = filename;
                fsw.EnableRaisingEvents = true;
                fsw.Changed += (s, e) => wh.Set();

                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var sr = new StreamReader(fs))
                {
                    var s = "";
                    var q = new FixedSizedQueue<string>();
                    if (nlines > 0) q.Limit = nlines;

                    while (Streaming)
                    {
                        s = sr.ReadLine();
                        if (s != null)
                        {
                            if (nlines > 0)
                            {
                                // queue up!
                                q.Enqueue(s);
                            }
                            else
                            {
                                AppendText(s + Environment.NewLine);
                            }
                        }
                        else
                        {
                            if (nlines > 0)
                            {
                                // dequeue last lines
                                while (q.TryDequeue(out s))
                                {
                                    AppendText(s + Environment.NewLine);
                                }
                                nlines = 0;
                            }
                            wh.WaitOne(1000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void Clear()
        {
            this.Dispatcher.Invoke(() =>
            {
                txtOutput.Clear();
            });
        }

        private void AppendText(string s)
        {
            this.Dispatcher.Invoke(() =>
            {
                txtOutput.AppendText(s);
            });
        }

        private void ShowOutput()
        {
            this.Dispatcher.Invoke(() =>
            {
                DragDropBorder.Visibility = System.Windows.Visibility.Hidden;
                scrollOutput.Visibility = System.Windows.Visibility.Visible;
            });
        }

        private void mainmenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void File_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                filename = files[0];
                worker.RunWorkerAsync();
            }
        }
    }
}
