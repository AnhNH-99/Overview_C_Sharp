using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Overview_C_Sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
            Download download = new Download("https://www.pdf-archive.com/2014/08/16/c-in-depth-3rd-edition-jon-skeet/c-in-depth-3rd-edition-jon-skeet.pdf", 3);
            download.DownloadFile();
            Console.ReadKey();
        }

    }
    class Download
    {
        public string url { get; set; }
        public int number { get; set; }
        public Download(string url, int number)
        {
            this.url = url;
            this.number = number;
        }
        public void DownloadFile()
        {
            int munber = this.number;
            string url = this.url;
            string urlstr = url;
            int indexName = url.LastIndexOf("/");
            int indexTpe = url.LastIndexOf(".");
            string typeFile = urlstr.Substring(indexTpe+1);
            urlstr = urlstr.Remove(indexTpe);
            string fileName = urlstr.Substring(indexName+1);
            WebRequest webRequest = HttpWebRequest.Create(url);
            WebResponse webResponse = webRequest.GetResponse();
            //Lấy Content-Type 
            string contenType = webResponse.Headers.Get("Content-Type");
            Console.WriteLine("==========Start==========");
            if (!String.IsNullOrEmpty(contenType))
            {
                //Lấy Content-Length
                string contenLengthstr = webResponse.Headers.Get("Content-Length");
                if (!String.IsNullOrEmpty(contenType))
                {
                    long contenLength = long.Parse(contenLengthstr);
                    long size = contenLength / number;
                    //Chia nhỏ file thành từng phần
                    List<Range> listRange = new List<Range>();
                    for (int i = 0; i < munber; i++)
                    {
                        long start = i * (contenLength / munber);
                        long end = (i + 1) * (contenLength / munber) - 1;
                        Range range = new Range(start, end);
                        listRange.Add(range);
                    }
                    using (FileStream destinationStream = new FileStream($"../../{fileName}.{typeFile}", FileMode.Append))
                    {
                        ConcurrentDictionary<int, String> tempFilesDictionary = new ConcurrentDictionary<int, String>();

                        Task[] tasks = new Task[number];
                        for (int i = 0; i < listRange.Count; i++)
                        {
                            var item = i;
                            tasks[item] = Task.Run(()=> { DownloadMultiPartFile(tempFilesDictionary, listRange[item], item, Convert.ToInt32(size), fileName, typeFile); });
                    }
                        Task.WaitAll(tasks);
                        foreach (var tempFile in tempFilesDictionary.OrderBy(b => b.Key))
                        {
                            byte[] tempFileBytes = File.ReadAllBytes(tempFile.Value);
                            destinationStream.Write(tempFileBytes, 0, tempFileBytes.Length);
                            File.Delete(tempFile.Value);
                        }
                      //  Console.WriteLine("==========Download File Success==========");
                    }
                }
            }
        }

        //Method để tải file
        public void DownloadMultiPartFile(ConcurrentDictionary<int, String> tempFilesDictionary, Range range, int index, int size, string fileName, string typeFile)
        {
            int col = Console.CursorLeft;
            int row = Console.CursorTop;
            HttpWebRequest httpWebRequest = HttpWebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.Method = "GET";
            httpWebRequest.AddRange(range.start, range.end);
            using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
            {
                Stream stream = httpWebResponse.GetResponseStream();
                string file = "c-in-depth-3rd-edition-jon-skeet" + index;
                using (var fileStream = new FileStream($"../../{fileName}{index}.{typeFile}", FileMode.OpenOrCreate))
                {
                    int iByteSize = 0;
                    byte[] byteBuffer = new byte[size];
                    int runByte = 0;
                    while ((iByteSize = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                    {
                        fileStream.Write(byteBuffer, 0, iByteSize);
                        runByte += iByteSize;
                        double dIndex = (double)(runByte);
                        double dTotal = (double)byteBuffer.Length;
                        // Print
                        Console.SetCursorPosition(col, row+index);
                        var temp = Math.Round(((dIndex / dTotal) * 100), 2);
                        Console.WriteLine($"File{index+1, 3}: {temp, 5}%");
                        fileStream.Flush();
                    }
                    fileStream.Close();
                    tempFilesDictionary.TryAdd((int)index, $"../../{fileName}{index}.{typeFile}");
                }
            }
        }
    }
    class Range
    {
        public long start { get; set; }
        public long end { get; set; }
        public Range(long start, long end)
        {
            this.start = start;
            this.end = end;
        }
        public Range()
        {

        }
    }
}
