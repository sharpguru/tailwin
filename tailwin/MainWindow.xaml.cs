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

        private string _filename; // the file to tail
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
        private bool Streaming = false; // this gets set to true when tailing a file. Set it to false to stop tailing 
        private bool Stopped = true;    // indicates state of file tailing
        private int lastnlines = 500;   // each file will skip output until the end is reached,
                                        // then the lastnlines will be emitted
                                        // lastnlines = 0 to show entire file contents

        public MainWindow()
        {
            InitializeComponent();

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                filename = args[1]; // get filename from command line 

                if (args.Length > 2) // get lastnlines from command line
                {
                    int.TryParse(args[2], out lastnlines); 
                }

                worker.RunWorkerAsync(); // if command line options were passed in then start tailing
            }

            
        }

        /// <summary>
        /// Start background process to tail file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Streaming = true;
            Stopped = false;
            streamFileToOutput(filename, lastnlines);
        }

        /// <summary>
        /// Method fired when tailing ends 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                if (Stopped) // should be stopped by the time this runs
                {
                    filename = openFileDialog.FileName;
                    worker.RunWorkerAsync();
                }
            }
        }

        /// <summary>
        /// Main method to start tailing a file
        /// </summary>
        /// <param name="filename">file to tail</param>
        /// <param name="nlines">skip file and display the last n lines</param>
        private void streamFileToOutput(string filename = "", int nlines = 0)
        {
            try
            {
                Clear();
                ShowOutput();

                using (AutoResetEvent wh = new AutoResetEvent(false)) // wh is wait handle to notify main thread an event has occurred, e.g. the file has been updated
                {
                    var fsw = new FileSystemWatcher(".");
                    fsw.Filter = filename;
                    fsw.EnableRaisingEvents = true;
                    fsw.Changed += (s, e) => wh.Set();      // when file changes notify main thread

                    var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using (var sr = new StreamReader(fs))
                    {
                        var s = "";
                        var q = new FixedSizedQueue<string>(); 
                        if (nlines > 0) q.Limit = nlines;

                        while (Streaming) // main tail loop
                        {
                            s = sr.ReadLine(); // read next line in file or respond to change event
                            if (s != null)
                            {
                                if (nlines > 0) // still reading through initial file
                                {
                                    // queue up!
                                    q.Enqueue(s);
                                }
                                else
                                {
                                    // write out text to output
                                    AppendText(s + Environment.NewLine);
                                }
                            }
                            else // done reading through entire file
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Clear the output
        /// </summary>
        private void Clear()
        {
            // dispatcher will update ui thread when it's safe w/o blocking
            this.Dispatcher.Invoke(() =>
            {
                txtOutput.Clear();
            });
        }

        /// <summary>
        /// Add text to output
        /// </summary>
        /// <param name="s"></param>
        private void AppendText(string s)
        {
            // dispatcher will update ui thread when it's safe w/o blocking
            this.Dispatcher.Invoke(() =>
            {
                txtOutput.AppendText(s);
            });
        }

        /// <summary>
        /// Show output and hide drag-drop controls
        /// </summary>
        private void ShowOutput()
        {
            // dispatcher will update ui thread when it's safe w/o blocking
            this.Dispatcher.Invoke(() =>
            {
                DragDropBorder.Visibility = System.Windows.Visibility.Hidden;
                scrollOutput.Visibility = System.Windows.Visibility.Visible;
            });
        }

        private void mainmenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); // let's get out of here!
        }

        /// <summary>
        /// File drop handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void File_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can drop more than one file
                // tailwin uses the first and ignores any others
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                filename = files[0];
                worker.RunWorkerAsync();
            }
        }
    }
}
