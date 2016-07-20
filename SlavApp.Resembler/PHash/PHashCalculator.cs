using DBreeze;
using DBreeze.Transactions;
using log4net;
using Murmur;
using SlavApp.Resembler.Storage;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SlavApp.Resembler.PHash
{
    public class PHashCalculator : IHashCalculator
    {
        [DllImport(@"Lib\pHash.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ph_dct_imagehash(string file, ref ulong hash);

        [DllImport(@"Lib\pHash.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ph_dct_imagehash2(IntPtr pxarray, int width, int height, ref ulong hash);

        const int BufferSize = 5;
        private static DataflowBlockOptions opts = new DataflowBlockOptions() { BoundedCapacity = BufferSize };
        private static ExecutionDataflowBlockOptions funOpts = new ExecutionDataflowBlockOptions() { BoundedCapacity = BufferSize };
        private static ExecutionDataflowBlockOptions actOpts = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded, BoundedCapacity = BufferSize };
        private static DataflowLinkOptions linkOpts = new DataflowLinkOptions() { PropagateCompletion = true };

        public event ProgressEventHandler OnProgress;
        public event ProgressEventHandler OnSkipProgress;

        private readonly DBreezeInstance dbInstance;
        private readonly HashRepository hashes;
        private readonly MurmurRepository murmurs;
        private readonly ILog log;
        private readonly Murmur128 murmur128 = MurmurHash.Create128(managed: false);

        public PHashCalculator(DBreezeInstance dbInstance, HashRepository hashes, MurmurRepository murmurs, ILog log)
        {
            this.dbInstance = dbInstance;
            this.hashes = hashes;
            this.murmurs = murmurs;
            this.log = log;
        }

        public void Run(IEnumerable<string> files)
        {
            this.Run(files, () => true, () => false);
        }
        
        public void Run(IEnumerable<string> files, Func<bool> continueTest, Func<bool> pauseTest)
        {            
            var fileQueue = new BufferBlock<FileAnalysis>(opts);
            var fileWithHashQueue = new TransformBlock<FileAnalysis, FileAnalysis>(x => CalculateHashes(x), funOpts);
            var fileRawQueue = new TransformBlock<FileAnalysis, FileAnalysis>(x => ReadData(x), funOpts);
            var processFile = new ActionBlock<FileAnalysis>(i => CalculatePHash(i), actOpts);
            var signalProcessedAction = new ActionBlock<FileAnalysis>(i => OnProgress(i.Name), actOpts);

            fileQueue.LinkTo(fileWithHashQueue, linkOpts);
            fileWithHashQueue.LinkTo(signalProcessedAction, linkOpts, x => x.CurrentHash == x.StoredHash);
            fileWithHashQueue.LinkTo(fileRawQueue, linkOpts);
            fileRawQueue.LinkTo(DataflowBlock.NullTarget<FileAnalysis>(), linkOpts, x => x == null);
            fileRawQueue.LinkTo(processFile, linkOpts);
            
            files.ToList().ForEach(f =>
            {
                if (continueTest())
                {
                    while (pauseTest())
                    {
                        Thread.Sleep(500);
                    }
                    
                    var file = Pathing.GetUNCPath(f);
                    var lastModified = File.GetLastWriteTime(file);

                    var result = false;
                    do
                    {
                        result = fileQueue.Post(new FileAnalysis { Name = file, LastModified = lastModified });
                        if (!result)
                        {
                            Thread.Sleep(500);
                        }
                    }
                    while (!result);
                }
            });

            fileQueue.Complete();
        }

        private FileAnalysis CalculateHashes(FileAnalysis fa)
        {
            var storedMurmur = string.Empty;
            using (var tran = this.dbInstance.GetTransaction())
            {
                storedMurmur = murmurs.GetMurMur(tran, fa.Name);                
            }

            if (fa.LastModified < DateTime.Now.AddDays(-1) && !string.IsNullOrEmpty(storedMurmur))
            {
                return new FileAnalysis
                {
                    Name = fa.Name,
                    LastModified = fa.LastModified,
                    CurrentHash = storedMurmur,
                    StoredHash = storedMurmur
                };
            }

            using (var fs = new FileStream(fa.Name, FileMode.Open, FileAccess.Read, FileShare.Read, 4096 * 1024, true))
            {
                var currentMurmur = BitConverter.ToString(murmur128.ComputeHash(fs)).Replace("-", "");
                return new FileAnalysis
                {
                    Name = fa.Name,
                    LastModified = fa.LastModified,
                    CurrentHash = currentMurmur,
                    StoredHash = storedMurmur
                };
            }
        }

        private FileAnalysis ReadData(FileAnalysis fa)
        {
            try
            {
                using (var tran = this.dbInstance.GetTransaction())
                {
                    if (fa.StoredHash != null)
                    {
                        murmurs.RemoveMurMur(tran, fa.Name);
                        hashes.RemoveHash(tran, fa.StoredHash);
                    }
                    murmurs.InsertMurMur(tran, fa.Name, fa.CurrentHash);
                    tran.Commit();
                }

                using (var fs = new FileStream(fa.Name, FileMode.Open, FileAccess.Read, FileShare.Read, 4096 * 1024, true))
                using (var ms = new MemoryStream((int)fs.Length))
                {
                    var imgW = 0;
                    var imgH = 0;

                    using (var img = Image.FromStream(fs, false, false))
                    {
                        imgW = img.Width;
                        imgH = img.Height;
                        if (img.RawFormat.Guid == System.Drawing.Imaging.ImageFormat.Bmp.Guid)
                        {
                            fs.CopyTo(ms);
                        }
                        else
                        {
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        }
                    }
                    var bytes = ms.GetBuffer();
                    return new FileAnalysis
                    {
                        Name = fa.Name,
                        LastModified = fa.LastModified,
                        CurrentHash = fa.CurrentHash,
                        StoredHash = fa.StoredHash,
                        Width = imgW,
                        Height = imgH,
                        Data = bytes
                    };
                }
            }
            catch (Exception ex)
            {
                this.log.Warn(string.Format("Data reading failed for {0}", fa.Name), ex);
                using (var tran = this.dbInstance.GetTransaction())
                {
                    murmurs.RemoveMurMur(tran, fa.Name);
                    tran.Commit();
                }
                if (OnSkipProgress != null)
                    OnSkipProgress(fa.Name);
            }

            return null;
        }

        [HandleProcessCorruptedStateExceptions]
        private unsafe void CalculatePHash(FileAnalysis fa)
        {
            try
            {
                ulong hash = 0;
                fixed (byte* p = fa.Data)
                {
                    ph_dct_imagehash2((IntPtr)p, fa.Width, fa.Height, ref hash);
                }

                using (var tran = this.dbInstance.GetTransaction())
                {
                    hashes.InsertHash(tran, fa.CurrentHash, hash);
                    tran.Commit();
                }
                if (OnProgress != null)
                    OnProgress(fa.Name);
            }
            catch (AccessViolationException avex)
            {
                this.log.Warn(string.Format("Access violation for {0}", fa.Name), avex);
                using (var tran = this.dbInstance.GetTransaction())
                {
                    murmurs.RemoveMurMur(tran, fa.Name);
                    tran.Commit();
                }
                if (OnSkipProgress != null)
                    OnSkipProgress(fa.Name);
            }
            catch (SEHException sehex)
            {
                this.log.Warn(string.Format("SEH error for {0}", fa.Name), sehex);
                using (var tran = this.dbInstance.GetTransaction())
                {
                    murmurs.RemoveMurMur(tran, fa.Name);
                    tran.Commit();
                }
                if (OnSkipProgress != null)
                    OnSkipProgress(fa.Name);
            }
        }
    }
}