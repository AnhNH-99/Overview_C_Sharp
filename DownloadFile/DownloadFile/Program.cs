using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace DownloadFile
{
    class Program
    {
        static void Main(string[] args)
        {
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
            WebRequest webRequest = HttpWebRequest.Create(url);
            WebResponse webResponse = webRequest.GetResponse();
            //Lấy Content-Type 
            string contenType = webResponse.Headers.Get("Content-Type");
            Console.WriteLine(contenType);
            if (!String.IsNullOrEmpty(contenType))
            {
                //Lấy Content-Length
                string contenLengthstr = webResponse.Headers.Get("Content-Length");
                if (!String.IsNullOrEmpty(contenType))
                {
                    long contenLength = long.Parse(contenLengthstr);
                    Console.WriteLine(contenLength);
                    //Chia nhỏ file thành từng phần
                    List<Range> listRange = new List<Range>();
                    for (int i = 0; i < munber; i++)
                    {
                        long start = i * (contenLength / munber);
                        long end = (i + 1) * (contenLength / munber) - 1;
                        Range range = new Range(start, end);
                        Console.WriteLine(start + "-" + end);
                        listRange.Add(range);
                    }
                    /*
                     * Chạy vòng for để tải file 
                     * Em đang bị lỗi đoạn này
                     * Nếu thay nội content trong method DownloadMultiPartFile là chạy vòng for hay
                     * một số câu lệnh khác để test thì hàm chạy đa luồng ok
                     * Nhưng cứ để content tải file là đa luồng chỉ chạy tải đc 2 file và chương trình bị đơ.
                     */
                    DownloadMultiPartFile(listRange[0], 0);
                    for (int i = 1; i < listRange.Count; i++)
                    {
                        var item = i;
                        Thread t = new Thread(() => {
                            DownloadMultiPartFile(listRange[item], item);
                        });
                        t.Start();
                    }
                }
            }
        }

        //Method để tải file
        public void DownloadMultiPartFile(Range range, int index)
        {
            Console.WriteLine("=====Start - " + index);
            HttpWebRequest httpWebRequest = HttpWebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.Method = "GET";
            httpWebRequest.AddRange(range.start, range.end);
            using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
            {
                Stream stream = httpWebResponse.GetResponseStream();
                string file = "test" + index;
                using (var fileStream = new FileStream(@"../../" + file + ".pdf", FileMode.Create, FileAccess.ReadWrite))
                {
                    stream.CopyTo(fileStream);
                }
            }
            if(index==1)
                Thread.Sleep(50000);
            Console.WriteLine("=====End - " + index);
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
