using NAudio.Wave;
using NLayer.NAudioSupport;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

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
        int digits;
        Label infoLabel;
        int maximum;

        ProgressBar progessBar;
        Mp3FileReader.FrameDecompressorBuilder builder = new Mp3FileReader.FrameDecompressorBuilder(wf => new Mp3FrameDecompressor(wf));
        FileStream filestream;

        public AudioSlice(List<string> mp3Files, string destinationFolder, ProgressBar progressBar, Label info, int interval)
        {
            InitProgessbar(mp3Files.Count, progressBar);
            new Thread(() =>
            {
                try
                {
                    this.interval = interval * 60;
                    paths = mp3Files;
                    destinationPath = destinationFolder;
                    var totalTime = 0;
                    infoLabel = info;
                    foreach (var mp3File in mp3Files)
                    {
                        totalTime += GetFileTime(mp3File);
                        progessBar.InvokeIfRequired(() => { progessBar.PerformStep(); });
                        var percent = (((float)progressBar.Value / (float)progressBar.Maximum) * 100).ToString("0.##");
                        infoLabel.InvokeIfRequired(() => { infoLabel.Text = $"Calculating output Count: {percent}%"; });
                    }
                    maximum = totalTime / (this.interval);
                    digits = maximum.Digits();
                    InitProgessbar(maximum, progressBar);
                    foreach (var mp3File in mp3Files)
                    {
                        Slice(mp3File);

                    }
                    GetLastPart(filestream);
                    filestream?.Close();
                    infoLabel.InvokeIfRequired(() => { infoLabel.Text = "finished"; });
                }
                catch (Exception e)
                {
                    throw e;
                }
            }).Start();
        }

        private int GetFileTime(string mp3File)
        {
            int result;
            using (var reader = new Mp3FileReader(mp3File, builder))
            {
                result = (int)reader.TotalTime.TotalSeconds;
            }
            return result;
        }

        private void InitProgessbar(int maximum, ProgressBar progessBar)
        {
            progessBar.InvokeIfRequired(() =>
            {
                this.progessBar = progessBar;
                this.progessBar.Visible = true;
                this.progessBar.Minimum = 1;
                this.progessBar.Maximum = maximum;
                this.progessBar.Value = 1;
                this.progessBar.Step = 1;
            });

        }

        void Slice(string path)
        {
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
                                    //one File Finished
                                    progessBar.InvokeIfRequired(() => { progessBar.PerformStep(); });
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
            var name = $"{(++counter).ToString($"D{digits}")}.mp3";
            infoLabel.InvokeIfRequired(() => { infoLabel.Text = name; });
            filestream = File.Open(Path.Combine(destinationPath, name), FileMode.OpenOrCreate);
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
