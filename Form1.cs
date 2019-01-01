using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AudioSlicer
{
    public partial class Form1 : Form
    {
        List<string> mp3Files = new List<string>();
        string destination;
        int interval = 3;
        public Form1()
        {
            InitializeComponent();
            labelInfo.Text = "";
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //all directories dropped
                foreach (var item in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    string[] fileEntries = Directory.GetFiles(item);
                    DirectoryInfo dinfo = new DirectoryInfo(item);
                    if (textBox1.Text == "")
                    {
                        textBox1.Text = dinfo.FullName;
                    }
                    else
                    {
                        textBox1.Text += $"\n{dinfo.FullName}";
                    }

                    var infos = GetFilesByExtensions(dinfo, ".mp3");
                    var selection = infos.Select(x => x.FullName);
                    mp3Files.AddRange(selection);

                }
            }
        }

        public IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //all directories dropped
                foreach (var item in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    destination = item;
                    textBox2.Text = item;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var audio = new AudioSlice(mp3Files, destination, progressBar1, labelInfo, interval);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            InitDragEnter(e);
        }

        private static void InitDragEnter(DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All :
                                                         DragDropEffects.None;
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            InitDragEnter(e);
        }

        private void textBox2_DragEnter(object sender, DragEventArgs e)
        {
            InitDragEnter(e);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            interval = trackBar1.Value;
            label4.Text = interval.ToString();
        }


    }
}
