using NAudio.Wave;
using NLayer.NAudioSupport;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AudioSlicer
{
    class AudioSlice
    {
        List<string> paths;
        string destinationPath;
        int interval = 180; // 3 minutes
        List<byte> overflowBytes;
        int overflowTime;
        int counter;

        ProgressBar progessBar;
        Mp3FileReader.FrameDecompressorBuilder builder = new Mp3FileReader.FrameDecompressorBuilder(wf => new Mp3FrameDecompressor(wf));
        FileStream filestream;

        public AudioSlice(List<string> mp3Files, string destinationFolder, ProgressBar progessBar)
        {
            InitProgessbar(mp3Files, progessBar);
            paths = mp3Files;
            destinationPath = destinationFolder;
            foreach (var mp3File in mp3Files)
            {
                Slice(mp3File);
                progessBar.PerformStep();
            }
            filestream?.Close();
        }

        private void InitProgessbar(List<string> mp3Files, ProgressBar progessBar)
        {
            this.progessBar = progessBar;
            progessBar.Visible = true;
            progessBar.Minimum = 1;
            progessBar.Maximum = mp3Files.Count;
            progessBar.Value = 1;
            progessBar.Step = 1;
        }

        void Slice(string path)
        {
            var mp3Dir = Path.GetDirectoryName(path);
            var splitDir = Path.Combine(mp3Dir, Path.GetFileNameWithoutExtension(path));
            try
            {

                using (var reader = new Mp3FileReader(path, builder))
                {
                    Mp3Frame frame;

                    // rest secondes from the last file
                    var restOffset = GetLastPart(filestream);
                    if (restOffset == 0)
                    {
                        CreateWriter();
                    }
                    var timeOfLastCut = 0;
                    var totalTime = reader.TotalTime.TotalSeconds + restOffset;
                    while ((frame = reader.ReadNextFrame()) != null)
                    {
                        //the rest of the file
                        if (overflowTime > 0)
                        {
                            overflowBytes.AddRange(frame.RawData);
                        }
                        else
                        {
                            // not enough time left for a cut
                            if ((totalTime - interval) < timeOfLastCut)
                            {
                                overflowTime = (int)(totalTime - timeOfLastCut);
                                overflowBytes = new List<byte>();
                                overflowBytes.AddRange(frame.RawData);
                            }
                            else
                            {
                                if (((int)reader.CurrentTime.TotalSeconds + restOffset - timeOfLastCut) >= interval)
                                {
                                    // time for a new file
                                    filestream?.Dispose();
                                    CreateWriter();
                                    timeOfLastCut = (int)reader.CurrentTime.TotalSeconds + restOffset;
                                }

                                filestream.Write(frame.RawData, 0, frame.RawData.Length);
                            }

                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void CreateWriter()
        {
            filestream?.Close();
            //todo calculate the numbers of files to determine a better format
            filestream = File.Open(Path.Combine(destinationPath, $"{(++counter).ToString("D4")}.mp3"), FileMode.OpenOrCreate);
        }

        private int GetLastPart(FileStream writer)
        {
            var result = overflowTime;
            if (overflowTime > 0)
            {
                var data = overflowBytes.ToArray();
                writer.Write(data, 0, data.Length);
                overflowTime = 0;
            }
            return result;
        }

    }
}
