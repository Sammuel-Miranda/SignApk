//SharpZIPLib (https://icsharpcode.github.io/SharpZipLib/) 0.86.0.518 06/DEZ/2006 (GNU)
#if (!OUTZIP && (XMPP || (ZIP || ZIPFILE) || (PDF || PDFEXTRACT) || MYSQL))
using Sys = global::System;
using SysTxt = global::System.Text;
using SysColl = global::System.Collections;
using SysMath = global::System.Math;
using SysDate = global::System.DateTime;
using ZIPLib = global::Libs.ZIP;
namespace System
{
    [Sys.Serializable] public class Randomizer : Sys.IDisposable, Sys.ICloneable //Sammuel (I wrote this based on Miscrosoft documentation for Random Class https://referencesource.microsoft.com/#mscorlib/system/random.cs,bb77e610694e64ca
    {
        protected const int ArraySize = 56;
        protected const int MSEED = 161803398;
        protected int inext;
        protected int inextp;
        protected int[] SeedArray;

        protected virtual int Sampler()
        {
            int retVal;
            int locINext = inext;
            int locINextp = inextp;
            if (++locINext >= Sys.Randomizer.ArraySize) locINext = 1;
            if (++locINextp >= Sys.Randomizer.ArraySize) locINextp = 1;
            retVal = this.SeedArray[locINext] - this.SeedArray[locINextp];
            if (retVal == int.MaxValue) retVal--;
            if (retVal < 0) retVal += int.MaxValue;
            this.SeedArray[locINext] = retVal;
            this.inext = locINext;
            this.inextp = locINextp;
            return retVal;
        }

        protected virtual double LargeSampler()
        {
            double result = this.Sampler();
            bool negative = (this.Sampler() % 2 == 0) ? true : false;
            if (negative) { result = -result; }
            result += (int.MaxValue - 1);
            result /= 2 * (uint)int.MaxValue - 1;
            return result;
        }

        public virtual void Dispose() { this.SeedArray = null; }
        protected virtual double Sample() { if (this.SeedArray == null) { throw new Sys.NullReferenceException("seed"); } else { return (this.Sampler() * (1.0 / int.MaxValue)); } }
        public virtual void NextBytes(byte[] buffer) { if (this.SeedArray == null) { throw new Sys.NullReferenceException("seed"); } else if (buffer == null) throw new Sys.ArgumentNullException("buffer"); else { for (int i = 0; i < buffer.Length; i++) { buffer[i] = (byte)(this.Sampler() % (byte.MaxValue + 1)); } } }
        public virtual double NextDouble() { if (this.SeedArray == null) { throw new Sys.NullReferenceException("seed"); } else { return this.Sample(); } }
        public virtual int Next() { if (this.SeedArray == null) { throw new Sys.NullReferenceException("seed"); } else { return this.Sampler(); } }
        public virtual int Next(int maxValue) { if (this.SeedArray == null) { throw new Sys.NullReferenceException("seed"); } else if (maxValue < 0) { throw new Sys.ArgumentOutOfRangeException("maxValue"); } else { return (int)(this.Sample() * maxValue); } }
        public virtual object Clone() { return new Sys.Randomizer(this.SeedArray, this.inext, this.inextp); }

        public virtual int Next(int minValue, int maxValue)
        {
            if (this.SeedArray == null) { throw new Sys.NullReferenceException("seed"); } else if (minValue > maxValue) { throw new Sys.ArgumentOutOfRangeException("minValue"); }
            long range = (long)maxValue - minValue;
            if (range <= (long)int.MaxValue) { return ((int)(this.Sample() * range) + minValue); } else { return (int)((long)(this.LargeSampler() * range) + minValue); }
        }

        protected Randomizer(int[] SeedArray, int inext, int inextp)
        {
            this.SeedArray = SeedArray;
            this.inext = inext;
            this.inextp = inextp;
        }

        public Randomizer(int Seed) : this(new int[Sys.Randomizer.ArraySize], 0, 21)
        {
            int mj = Sys.Randomizer.MSEED - ((Seed == int.MinValue) ? int.MaxValue : Sys.Math.Abs(Seed));
            int Last = Sys.Randomizer.ArraySize - 1;
            this.SeedArray[Last] = mj;
            int mk = 1;
            int ii;
            for (int i = 1; i < Last; i++)
            {
                ii = (21 * i) % Last;
                this.SeedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0) mk += int.MaxValue;
                mj = this.SeedArray[ii];
            }
            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < Sys.Randomizer.ArraySize; i++)
                {
                    this.SeedArray[i] -= this.SeedArray[1 + (i + 30) % Last];
                    if (this.SeedArray[i] < 0) this.SeedArray[i] += int.MaxValue;
                }
            }
        }

        public Randomizer() : this(Sys.Environment.TickCount) { /* NOTHING */ }
    }
}
namespace Libs
{
    namespace ZIP
    {
#if ZIPFILE
        public class KeysRequiredEventArgs : Sys.EventArgs
        {
            public KeysRequiredEventArgs(string name) { fileName = name; }

            public KeysRequiredEventArgs(string name, byte[] keyValue)
            {
                fileName = name;
                key = keyValue;
            }

            public string FileName { get { return fileName; } }

            public byte[] Key
            {
                get { return key; }
                set { key = value; }
            }

            string fileName;
            byte[] key;
        }

        public enum TestStrategy
        {
            FindFirstError,
            FindAllErrors
        }

        public enum TestOperation
        {
            Initialising,
            EntryHeader,
            EntryData,
            EntryComplete,
            MiscellaneousTests,
            Complete
        }

        public class TestStatus
        {
            public TestStatus(ZipFile file) { file_ = file; }
            public TestOperation Operation { get { return operation_; } }
            public ZipFile File { get { return file_; } }
            public ZipEntry Entry { get { return entry_; } }
            public int ErrorCount { get { return errorCount_; } }
            public long BytesTested { get { return bytesTested_; } }
            public bool EntryValid { get { return entryValid_; } }

            internal void AddError()
            {
                errorCount_++;
                entryValid_ = false;
            }

            internal void SetOperation(TestOperation operation) { operation_ = operation; }

            internal void SetEntry(ZipEntry entry)
            {
                entry_ = entry;
                entryValid_ = true;
                bytesTested_ = 0;
            }

            internal void SetBytesTested(long value)
            {
                bytesTested_ = value;
            }

            ZipFile file_;
            ZipEntry entry_;
            bool entryValid_;
            int errorCount_;
            long bytesTested_;
            TestOperation operation_;
        }

        public delegate void ZipTestResultHandler(TestStatus status, string message);
        public interface IStaticDataSource { Sys.IO.Stream GetSource(); }
        public interface IDynamicDataSource { Sys.IO.Stream GetSource(ZipEntry entry, string name); }
        interface ITaggedDataFactory { ZIPLib.ITaggedData Create(short tag, byte[] data, int offset, int count); }

        public enum FileUpdateMode
        {
            Safe,
            Direct
        }

        public class StaticDiskDataSource : IStaticDataSource
        {
            public StaticDiskDataSource(string fileName) { fileName_ = fileName; }
            public Sys.IO.Stream GetSource() { return Sys.IO.File.Open(fileName_, Sys.IO.FileMode.Open, Sys.IO.FileAccess.Read, Sys.IO.FileShare.Read); }
            string fileName_;
        }

        public class DynamicDiskDataSource : IDynamicDataSource
        {
            public DynamicDiskDataSource() { /* NOTHING */ }

            public Sys.IO.Stream GetSource(ZipEntry entry, string name)
            {
                Sys.IO.Stream result = null;
                if (name != null) { result = Sys.IO.File.Open(name, Sys.IO.FileMode.Open, Sys.IO.FileAccess.Read, Sys.IO.FileShare.Read); }
                return result;
            }
        }

        public interface IArchiveStorage
        {
            ZIPLib.FileUpdateMode UpdateMode { get; }
            Sys.IO.Stream GetTemporaryOutput();
            Sys.IO.Stream ConvertTemporaryToFinal();
            Sys.IO.Stream MakeTemporaryCopy(Sys.IO.Stream stream);
            Sys.IO.Stream OpenForDirectUpdate(Sys.IO.Stream stream);
            void Dispose();
        }

        public abstract class BaseArchiveStorage : ZIPLib.IArchiveStorage
        {
            protected BaseArchiveStorage(ZIPLib.FileUpdateMode updateMode) { this.updateMode_ = updateMode; }
            public abstract Sys.IO.Stream GetTemporaryOutput();
            public abstract Sys.IO.Stream ConvertTemporaryToFinal();
            public abstract Sys.IO.Stream MakeTemporaryCopy(Sys.IO.Stream stream);
            public abstract Sys.IO.Stream OpenForDirectUpdate(Sys.IO.Stream stream);
            public abstract void Dispose();
            public ZIPLib.FileUpdateMode UpdateMode { get { return this.updateMode_; } }
            ZIPLib.FileUpdateMode updateMode_;
        }

        public class DiskArchiveStorage : BaseArchiveStorage
        {
            public DiskArchiveStorage(ZIPLib.ZipFile file, ZIPLib.FileUpdateMode updateMode) : base(updateMode)
            {
                if (file.Name == null) { throw new Sys.Exception("Cant handle non file archives"); }
                this.fileName_ = file.Name;
            }

            public DiskArchiveStorage(ZIPLib.ZipFile file) : this(file, ZIPLib.FileUpdateMode.Safe) { /* NOTHING */ }

            public override Sys.IO.Stream GetTemporaryOutput()
            {
                if (temporaryName_ != null)
                {
                    temporaryName_ = GetTempFileName(temporaryName_, true);
                    temporaryStream_ = Sys.IO.File.Open(temporaryName_, Sys.IO.FileMode.OpenOrCreate, Sys.IO.FileAccess.Write, Sys.IO.FileShare.None);
                }
                else
                {
                    temporaryName_ = Sys.IO.Path.GetTempFileName();
                    temporaryStream_ = Sys.IO.File.Open(temporaryName_, Sys.IO.FileMode.OpenOrCreate, Sys.IO.FileAccess.Write, Sys.IO.FileShare.None);
                }
                return temporaryStream_;
            }

            public override Sys.IO.Stream ConvertTemporaryToFinal()
            {
                if (temporaryStream_ == null) { throw new Sys.Exception("No temporary stream has been created"); }
                Sys.IO.Stream result = null;
                string moveTempName = GetTempFileName(fileName_, false);
                bool newFileCreated = false;
                try
                {
                    temporaryStream_.Close();
                    Sys.IO.File.Move(fileName_, moveTempName);
                    Sys.IO.File.Move(temporaryName_, fileName_);
                    newFileCreated = true;
                    Sys.IO.File.Delete(moveTempName);

                    result = Sys.IO.File.Open(fileName_, Sys.IO.FileMode.Open, Sys.IO.FileAccess.Read, Sys.IO.FileShare.Read);
                }
                catch (Sys.Exception ex)
                {
                    result = null;
                    if (!newFileCreated)
                    {
                        Sys.IO.File.Move(moveTempName, fileName_);
                        Sys.IO.File.Delete(temporaryName_);
                    }
                    throw ex;
                }
                return result;
            }

            public override Sys.IO.Stream MakeTemporaryCopy(Sys.IO.Stream stream)
            {
                stream.Close();
                temporaryName_ = GetTempFileName(fileName_, true);
                Sys.IO.File.Copy(fileName_, temporaryName_, true);
                temporaryStream_ = new Sys.IO.FileStream(temporaryName_, Sys.IO.FileMode.Open, Sys.IO.FileAccess.ReadWrite);
                return temporaryStream_;
            }

            public override Sys.IO.Stream OpenForDirectUpdate(Sys.IO.Stream stream)
            {
                Sys.IO.Stream result;
                if ((stream == null) || !stream.CanWrite)
                {
                    if (stream != null) { stream.Close(); }
                    result = new Sys.IO.FileStream(this.fileName_, Sys.IO.FileMode.Open, Sys.IO.FileAccess.ReadWrite);
                }
                else { result = stream; }
                return result;
            }

            public override void Dispose() { if (temporaryStream_ != null) { temporaryStream_.Close(); } }

            static string GetTempFileName(string original, bool makeTempFile)
            {
                string result = null;
                if (original == null) { result = Sys.IO.Path.GetTempFileName(); }
                else
                {
                    int counter = 0;
                    int suffixSeed = SysDate.Now.Second;
                    while (result == null)
                    {
                        counter += 1;
                        string newName = string.Format("{0}.{1}{2}.tmp", original, suffixSeed, counter);
                        if (!Sys.IO.File.Exists(newName))
                        {
                            if (makeTempFile)
                            {
                                try
                                {
                                    using (Sys.IO.FileStream stream = Sys.IO.File.Create(newName)) { /* NOTHING */ }
                                    result = newName;
                                } catch { suffixSeed = SysDate.Now.Second; }
                            } else { result = newName; }
                        }
                    }
                }
                return result;
            }

            Sys.IO.Stream temporaryStream_;
            string fileName_;
            string temporaryName_;
        }

        public class MemoryArchiveStorage : BaseArchiveStorage
        {
            public MemoryArchiveStorage() : base(FileUpdateMode.Direct) { /* NOTHING */ }
            public MemoryArchiveStorage(FileUpdateMode updateMode) : base(updateMode) { /* NOTHING */ }
            public Sys.IO.MemoryStream FinalStream { get { return finalStream_; } }

            public override Sys.IO.Stream GetTemporaryOutput()
            {
                temporaryStream_ = new Sys.IO.MemoryStream();
                return temporaryStream_;
            }

            public override Sys.IO.Stream ConvertTemporaryToFinal()
            {
                if (temporaryStream_ == null) { throw new Sys.ApplicationException("No temporary stream has been created"); }
                finalStream_ = new Sys.IO.MemoryStream(temporaryStream_.ToArray());
                return finalStream_;
            }

            public override Sys.IO.Stream MakeTemporaryCopy(Sys.IO.Stream stream)
            {
                temporaryStream_ = new Sys.IO.MemoryStream();
                stream.Position = 0;
                ZIPLib.Internal.StreamUtils.Copy(stream, temporaryStream_, new byte[4096]);
                return temporaryStream_;
            }

            public override Sys.IO.Stream OpenForDirectUpdate(Sys.IO.Stream stream)
            {
                Sys.IO.Stream result;
                if ((stream == null) || !stream.CanWrite)
                {
                    result = new Sys.IO.MemoryStream();
                    if (stream != null)
                    {
                        stream.Position = 0;
                        ZIPLib.Internal.StreamUtils.Copy(stream, result, new byte[4096]);
                        stream.Close();
                    }
                } else { result = stream; }
                return result;
            }

            public override void Dispose() { if (this.temporaryStream_ != null) { this.temporaryStream_.Close(); } }

            Sys.IO.MemoryStream temporaryStream_;
            Sys.IO.MemoryStream finalStream_;
        }

        public class ZipFile : SysColl.Generic.IEnumerable<ZIPLib.ZipEntry>, Sys.IDisposable
        {
            public delegate void KeysRequiredEventHandler(object sender, KeysRequiredEventArgs e);
            public KeysRequiredEventHandler KeysRequired;
            internal byte[] Key { get { return key; } set { key = value; } }
            internal bool HaveKeys { get { return key != null; } }

            internal void OnKeysRequired(string fileName)
            {
                if (KeysRequired != null)
                {
                    KeysRequiredEventArgs krea = new KeysRequiredEventArgs(fileName, key);
                    KeysRequired(this, krea);
                    key = krea.Key;
                }
            }

            public string Password
            {
                set
                {
                    if ((value == null) || (value.Length == 0)) { key = null; }
                    else
                    {
                        rawPassword_ = value;
                        key = ZIPLib.Encryption.PkzipClassic.GenerateKeys(ZIPLib.ZipConstants.ConvertToArray(value));
                    }
                }
            }

            public ZipFile(string name)
            {
                if (name == null) { throw new Sys.ArgumentNullException("name"); }
                name_ = name;
                baseStream_ = Sys.IO.File.Open(name, Sys.IO.FileMode.Open, Sys.IO.FileAccess.Read, Sys.IO.FileShare.Read);
                isStreamOwner = true;
                try { ReadEntries(); }
                catch
                {
                    DisposeInternal(true);
                    throw;
                }
            }

            public ZipFile(Sys.IO.FileStream file)
            {
                if (file == null) { throw new Sys.ArgumentNullException("file"); }
                if (!file.CanSeek) { throw new Sys.ArgumentException("Stream is not seekable", "file"); }
                baseStream_ = file;
                name_ = file.Name;
                isStreamOwner = true;
                try { ReadEntries(); }
                catch
                {
                    DisposeInternal(true);
                    throw;
                }
            }

            public ZipFile(Sys.IO.Stream stream)
            {
                if (stream == null) { throw new Sys.ArgumentNullException("stream"); }
                if (!stream.CanSeek) { throw new Sys.ArgumentException("Stream is not seekable", "stream"); }
                baseStream_ = stream;
                isStreamOwner = true;
                if (baseStream_.Length > 0)
                {
                    try { ReadEntries(); }
                    catch
                    {
                        DisposeInternal(true);
                        throw;
                    }
                }
                else
                {
                    entries_ = new ZipEntry[0];
                    isNewArchive_ = true;
                }
            }

            internal ZipFile()
            {
                entries_ = new ZipEntry[0];
                isNewArchive_ = true;
            }

            ~ZipFile() { Dispose(false); }

            public void Close()
            {
                DisposeInternal(true);
                Sys.GC.SuppressFinalize(this);
            }

            public static ZipFile Create(string fileName)
            {
                if (fileName == null) { throw new Sys.ArgumentNullException("fileName"); }
                Sys.IO.FileStream fs = Sys.IO.File.Create(fileName);
                ZipFile result = new ZipFile();
                result.name_ = fileName;
                result.baseStream_ = fs;
                result.isStreamOwner = true;
                return result;
            }

            public static ZipFile Create(Sys.IO.Stream outStream)
            {
                if (outStream == null) { throw new Sys.ArgumentNullException("outStream"); }
                if (!outStream.CanWrite) { throw new Sys.ArgumentException("Stream is not writeable", "outStream"); }
                if (!outStream.CanSeek) { throw new Sys.ArgumentException("Stream is not seekable", "outStream"); }
                ZipFile result = new ZipFile();
                result.baseStream_ = outStream;
                return result;
            }

            public bool IsStreamOwner { get { return isStreamOwner; } set { isStreamOwner = value; } }
            public bool IsEmbeddedArchive { get { return offsetOfFirstEntry > 0; } }
            public bool IsNewArchive { get { return isNewArchive_; } }
            public string ZipFileComment { get { return comment_; } }
            public string Name { get { return name_; } }
            [Sys.Obsolete("Use the Count property instead")] public int Size { get { return entries_.Length; } }
            public long Count { get { return entries_.Length; } }
            [Sys.Runtime.CompilerServices.IndexerNameAttribute("EntryByIndex")] public ZipEntry this[int index] { get { return (ZipEntry)entries_[index].Clone(); } }
            SysColl.IEnumerator SysColl.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

            public SysColl.Generic.IEnumerator<ZIPLib.ZipEntry> GetEnumerator()
            {
                if (isDisposed_) { throw new Sys.ObjectDisposedException("ZipFile"); }
                return new ZipEntryEnumerator(entries_);
            }

            public int FindEntry(string name, bool ignoreCase)
            {
                if (isDisposed_) { throw new Sys.ObjectDisposedException("ZipFile"); }
                for (int i = 0; i < entries_.Length; i++) { if (string.Compare(name, entries_[i].Name, ignoreCase, global::System.Globalization.CultureInfo.InvariantCulture) == 0) { return i; } }
                return -1;
            }

            public ZipEntry GetEntry(string name)
            {
                if (isDisposed_) { throw new Sys.ObjectDisposedException("ZipFile"); }
                int index = FindEntry(name, true);
                return (index >= 0) ? (ZipEntry)entries_[index].Clone() : null;
            }

            public Sys.IO.Stream GetInputStream(ZipEntry entry)
            {
                if (entry == null) { throw new Sys.ArgumentNullException("entry"); }
                if (isDisposed_) { throw new Sys.ObjectDisposedException("ZipFile"); }
                long index = entry.ZipFileIndex;
                if ((index < 0) || (index >= entries_.Length) || (entries_[index].Name != entry.Name))
                {
                    index = FindEntry(entry.Name, true);
                    if (index < 0) { throw new Sys.ApplicationException("Entry cannot be found"); }
                }
                return GetInputStream(index);
            }

            public Sys.IO.Stream GetInputStream(long entryIndex)
            {
                if (isDisposed_) { throw new Sys.ObjectDisposedException("ZipFile"); }
                long start = LocateEntry(entries_[entryIndex]);
                CompressionMethod method = entries_[entryIndex].CompressionMethod;
                Sys.IO.Stream result = new PartialInputStream(this, start, entries_[entryIndex].CompressedSize);
                if (entries_[entryIndex].IsCrypted == true)
                {
                    result = CreateAndInitDecryptionStream(result, entries_[entryIndex]);
                    if (result == null) { throw new Sys.ApplicationException("Unable to decrypt this entry"); }
                }
                switch (method)
                {
                    case CompressionMethod.Stored: break;
                    case CompressionMethod.Deflated: result = new ZIPLib.Compression.Streams.InflaterInputStream(result, new ZIPLib.Compression.Inflater(true)); break;
                    default: throw new Sys.ApplicationException("Unsupported compression method " + method);
                }
                return result;
            }

            public bool TestArchive(bool testData) { return TestArchive(testData, TestStrategy.FindFirstError, null); }

            public bool TestArchive(bool testData, TestStrategy strategy, ZipTestResultHandler resultHandler)
            {
                if (isDisposed_) { throw new Sys.ObjectDisposedException("ZipFile"); }
                TestStatus status = new TestStatus(this);
                if (resultHandler != null) { resultHandler(status, null); }
                HeaderTest test = testData ? (HeaderTest.Header | HeaderTest.Extract) : HeaderTest.Header;
                bool testing = true;
                try
                {
                    int entryIndex = 0;
                    while (testing && (entryIndex < Count))
                    {
                        if (resultHandler != null)
                        {
                            status.SetEntry(this[entryIndex]);
                            status.SetOperation(TestOperation.EntryHeader);
                            resultHandler(status, null);
                        }
                        try { TestLocalHeader(this[entryIndex], test); }
                        catch (Sys.Exception ex)
                        {
                            status.AddError();
                            if (resultHandler != null) { resultHandler(status, string.Format("Exception during test - '{0}'", ex.Message)); }
                            if (strategy == ZIPLib.TestStrategy.FindFirstError) { testing = false; }
                        }
                        if (testing && testData && this[entryIndex].IsFile)
                        {
                            if (resultHandler != null)
                            {
                                status.SetOperation(TestOperation.EntryData);
                                resultHandler(status, null);
                            }
                            ZIPLib.Checksums.Crc32 crc = new ZIPLib.Checksums.Crc32();
                            using (Sys.IO.Stream entryStream = this.GetInputStream(this[entryIndex]))
                            {

                                byte[] buffer = new byte[4096];
                                long totalBytes = 0;
                                int bytesRead;
                                while ((bytesRead = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    crc.Update(buffer, 0, bytesRead);
                                    if (resultHandler != null)
                                    {
                                        totalBytes += bytesRead;
                                        status.SetBytesTested(totalBytes);
                                        resultHandler(status, null);
                                    }
                                }
                            }
                            if (this[entryIndex].Crc != crc.Value)
                            {
                                status.AddError();
                                if (resultHandler != null) { resultHandler(status, "CRC mismatch"); }
                                if (strategy == TestStrategy.FindFirstError) { testing = false; }
                            }
                            if ((this[entryIndex].Flags & (int)GeneralBitFlags.Descriptor) != 0)
                            {
                                ZipHelperStream helper = new ZipHelperStream(baseStream_);
                                DescriptorData data = new DescriptorData();
                                helper.ReadDataDescriptor(this[entryIndex].LocalHeaderRequiresZip64, data);
                                if (this[entryIndex].Crc != data.Crc) { status.AddError(); }
                                if (this[entryIndex].CompressedSize != data.CompressedSize) { status.AddError(); }
                                if (this[entryIndex].Size != data.Size) { status.AddError(); }
                            }
                        }
                        if (resultHandler != null)
                        {
                            status.SetOperation(TestOperation.EntryComplete);
                            resultHandler(status, null);
                        }
                        entryIndex += 1;
                    }
                    if (resultHandler != null)
                    {
                        status.SetOperation(TestOperation.MiscellaneousTests);
                        resultHandler(status, null);
                    }
                }
                catch (Sys.Exception ex)
                {
                    status.AddError();
                    if (resultHandler != null) { resultHandler(status, string.Format("Exception during test - '{0}'", ex.Message)); }
                }
                if (resultHandler != null)
                {
                    status.SetOperation(TestOperation.Complete);
                    status.SetEntry(null);
                    resultHandler(status, null);
                }
                return (status.ErrorCount == 0);
            }

            [Sys.Flags] enum HeaderTest
            {
                Extract = 0x01,
                Header = 0x02
            }

            long TestLocalHeader(ZipEntry entry, HeaderTest tests)
            {
                lock (baseStream_)
                {
                    bool testHeader = (tests & HeaderTest.Header) != 0;
                    bool testData = (tests & HeaderTest.Extract) != 0;
                    baseStream_.Seek(offsetOfFirstEntry + entry.Offset, Sys.IO.SeekOrigin.Begin);
                    if ((int)ReadLEUint() != ZipConstants.LocalHeaderSignature) { throw new Sys.Exception(string.Format("Wrong local header signature @{0:X}", offsetOfFirstEntry + entry.Offset)); }
                    short extractVersion = (short)(ReadLEUshort() & 0x00ff);
                    short localFlags = (short)ReadLEUshort();
                    short compressionMethod = (short)ReadLEUshort();
                    short fileTime = (short)ReadLEUshort();
                    short fileDate = (short)ReadLEUshort();
                    uint crcValue = ReadLEUint();
                    long compressedSize = ReadLEUint();
                    long size = ReadLEUint();
                    int storedNameLength = ReadLEUshort();
                    int extraDataLength = ReadLEUshort();
                    byte[] nameData = new byte[storedNameLength];
                    ZIPLib.Internal.StreamUtils.ReadFully(baseStream_, nameData);
                    byte[] extraData = new byte[extraDataLength];
                    ZIPLib.Internal.StreamUtils.ReadFully(baseStream_, extraData);
                    ZIPLib.ZipExtraData localExtraData = new ZipExtraData(extraData);
                    if (localExtraData.Find(1))
                    {
                        size = localExtraData.ReadLong();
                        compressedSize = localExtraData.ReadLong();
                        if ((localFlags & (int)ZIPLib.GeneralBitFlags.Descriptor) != 0)
                        {
                            if ((size != -1) && (size != entry.Size)) { throw new Sys.Exception("Size invalid for descriptor"); }
                            if ((compressedSize != -1) && (compressedSize != entry.CompressedSize)) { throw new Sys.ApplicationException("Compressed size invalid for descriptor"); }
                        }
                    } else { if ((extractVersion >= ZipConstants.VersionZip64) && (((uint)size == uint.MaxValue) || ((uint)compressedSize == uint.MaxValue))) { throw new Sys.ApplicationException("Required Zip64 extended information missing"); } }
                    if (testData)
                    {
                        if (entry.IsFile)
                        {
                            if (!entry.IsCompressionMethodSupported()) { throw new Sys.ApplicationException("Compression method not supported"); }
                            if ((extractVersion > ZipConstants.VersionMadeBy) || ((extractVersion > 20) && (extractVersion < ZipConstants.VersionZip64))) { throw new Sys.ApplicationException(string.Format("Version required to extract this entry not supported ({0})", extractVersion)); }
                            if ((localFlags & (int)(GeneralBitFlags.Patched | GeneralBitFlags.StrongEncryption | GeneralBitFlags.EnhancedCompress | GeneralBitFlags.HeaderMasked)) != 0) { throw new Sys.ApplicationException("The library does not support the zip version required to extract this entry"); }
                        }
                    }
                    if (testHeader)
                    {
                        if ((extractVersion <= 63) && (extractVersion != 10) && (extractVersion != 11) && (extractVersion != 20) && (extractVersion != 21) && (extractVersion != 25) && (extractVersion != 27) && (extractVersion != 45) && (extractVersion != 46) && (extractVersion != 50) && (extractVersion != 51) && (extractVersion != 52) && (extractVersion != 61) && (extractVersion != 62) && (extractVersion != 63)) { throw new Sys.ApplicationException(string.Format("Version required to extract this entry is invalid ({0})", extractVersion)); }
                        if ((localFlags & (int)(GeneralBitFlags.ReservedPKware4 | GeneralBitFlags.ReservedPkware14 | GeneralBitFlags.ReservedPkware15)) != 0) { throw new Sys.ApplicationException("Reserved bit flags cannot be set."); }
                        if (((localFlags & (int)GeneralBitFlags.Encrypted) != 0) && (extractVersion < 20)) { throw new Sys.ApplicationException(string.Format("Version required to extract this entry is too low for encryption ({0})", extractVersion)); }
                        if ((localFlags & (int)GeneralBitFlags.StrongEncryption) != 0)
                        {
                            if ((localFlags & (int)GeneralBitFlags.Encrypted) == 0) { throw new Sys.ApplicationException("Strong encryption flag set but encryption flag is not set"); }
                            if (extractVersion < 50) { throw new Sys.ApplicationException(string.Format("Version required to extract this entry is too low for encryption ({0})", extractVersion)); }
                        }
                        if (((localFlags & (int)GeneralBitFlags.Patched) != 0) && (extractVersion < 27)) { throw new Sys.ApplicationException(string.Format("Patched data requires higher version than ({0})", extractVersion)); }
                        if (localFlags != entry.Flags) { throw new Sys.ApplicationException("Central header/local header flags mismatch"); }
                        if (entry.CompressionMethod != (CompressionMethod)compressionMethod) { throw new Sys.ApplicationException("Central header/local header compression method mismatch"); }
                        if (entry.Version != extractVersion) { throw new Sys.ApplicationException("Extract version mismatch"); }
                        if ((localFlags & (int)GeneralBitFlags.StrongEncryption) != 0) { if (extractVersion < 62) { throw new Sys.ApplicationException("Strong encryption flag set but version not high enough"); } }
                        if ((localFlags & (int)GeneralBitFlags.HeaderMasked) != 0) { if ((fileTime != 0) || (fileDate != 0)) { throw new Sys.ApplicationException("Header masked set but date/time values non-zero"); } }
                        if ((localFlags & (int)GeneralBitFlags.Descriptor) == 0) { if (crcValue != (uint)entry.Crc) { throw new Sys.ApplicationException("Central header/local header crc mismatch"); } }
                        if ((size == 0) && (compressedSize == 0)) { if (crcValue != 0) { throw new Sys.ApplicationException("Invalid CRC for empty entry"); } }
                        if (entry.Name.Length > storedNameLength) { throw new Sys.ApplicationException("File name length mismatch"); }
                        string localName = ZipConstants.ConvertToStringExt(localFlags, nameData);
                        if (localName != entry.Name) { throw new Sys.ApplicationException("Central header and local header file name mismatch"); }
                        if (entry.IsDirectory)
                        {
                            if (size > 0) { throw new Sys.ApplicationException("Directory cannot have size"); }
                            if (entry.IsCrypted) { if (compressedSize > ZipConstants.CryptoHeaderSize + 2) { throw new Sys.ApplicationException("Directory compressed size invalid"); } }
                            else if (compressedSize > 2) { throw new Sys.ApplicationException("Directory compressed size invalid"); }
                        }
                        if (!ZipNameTransform.IsValidName(localName, true)) { throw new Sys.ApplicationException("Name is invalid"); }
                    }
                    if (((localFlags & (int)GeneralBitFlags.Descriptor) == 0) || ((size > 0) || (compressedSize > 0)))
                    {
                        if (size != entry.Size) { throw new Sys.ApplicationException(string.Format("Size mismatch between central header({0}) and local header({1})", entry.Size, size)); }
                        if (compressedSize != entry.CompressedSize && compressedSize != 0xFFFFFFFF && compressedSize != -1) { throw new Sys.ApplicationException( string.Format("Compressed size mismatch between central header({0}) and local header({1})", entry.CompressedSize, compressedSize)); }
                    }
                    int extraLength = storedNameLength + extraDataLength;
                    return offsetOfFirstEntry + entry.Offset + ZipConstants.LocalHeaderBaseSize + extraLength;
                }
            }

            internal const int DefaultBufferSize = 4096;
            internal enum UpdateCommand { Copy, Modify, Add }
            public ZIPLib.Internal.INameTransform NameTransform { get { return this.updateEntryFactory_.NameTransform; } set { this.updateEntryFactory_.NameTransform = value; } }
            public IEntryFactory EntryFactory { get { return updateEntryFactory_; } set { if (value == null) { updateEntryFactory_ = new ZipEntryFactory(); } else { updateEntryFactory_ = value; } } }
            public bool IsUpdating { get { return updates_ != null; } }
            public UseZip64 UseZip64 { get { return useZip64_; } set { useZip64_ = value; } }

            public int BufferSize
            {
                get { return bufferSize_; }
                set
                {
                    if (value < 1024) { throw new Sys.ArgumentOutOfRangeException("value", "cannot be below 1024"); }
                    if (bufferSize_ != value)
                    {
                        bufferSize_ = value;
                        copyBuffer_ = null;
                    }
                }
            }

            public void BeginUpdate(IArchiveStorage archiveStorage, IDynamicDataSource dataSource)
            {
                if (archiveStorage == null) { throw new Sys.ArgumentNullException("archiveStorage"); }
                if (dataSource == null) { throw new Sys.ArgumentNullException("dataSource"); }
                if (isDisposed_) { throw new Sys.ObjectDisposedException("ZipFile"); }
                if (IsEmbeddedArchive) { throw new Sys.ApplicationException("Cannot update embedded/SFX archives"); }
                archiveStorage_ = archiveStorage;
                updateDataSource_ = dataSource;
                updateIndex_ = new SysColl.Hashtable();
                updates_ = new SysColl.ArrayList(entries_.Length);
                foreach (ZipEntry entry in entries_)
                {
                    int index = updates_.Add(new ZipUpdate(entry));
                    updateIndex_.Add(entry.Name, index);
                }
                updates_.Sort(new UpdateComparer());
                int idx = 0;
                foreach (ZipUpdate update in updates_)
                {
                    if (idx == updates_.Count - 1) break;
                    update.OffsetBasedSize = ((ZipUpdate)updates_[idx + 1]).Entry.Offset - update.Entry.Offset;
                    idx++;
                }
                updateCount_ = updates_.Count;
                contentsEdited_ = false;
                commentEdited_ = false;
                newComment_ = null;
            }

            public void BeginUpdate(IArchiveStorage archiveStorage) { BeginUpdate(archiveStorage, new DynamicDiskDataSource()); }
            public void BeginUpdate() { if (Name == null) { BeginUpdate(new MemoryArchiveStorage(), new DynamicDiskDataSource()); } else { BeginUpdate(new DiskArchiveStorage(this), new DynamicDiskDataSource()); } }

            public void CommitUpdate()
            {
                if (isDisposed_) { throw new Sys.ObjectDisposedException("ZipFile"); }
                CheckUpdating();
                try
                {
                    updateIndex_.Clear();
                    updateIndex_ = null;
                    if (contentsEdited_) { RunUpdates(); }
                    else if (commentEdited_) { UpdateCommentOnly(); }
                    else
                    {
                        if (entries_.Length == 0)
                        {
                            byte[] theComment = (newComment_ != null) ? newComment_.RawComment : ZipConstants.ConvertToArray(comment_);
                            using (ZipHelperStream zhs = new ZipHelperStream(baseStream_)) { zhs.WriteEndOfCentralDirectory(0, 0, 0, theComment); }
                        }
                    }
                }
                finally { PostUpdateCleanup(); }
            }

            public void AbortUpdate() { PostUpdateCleanup(); }

            public void SetComment(string comment)
            {
                if (isDisposed_) { throw new Sys.ObjectDisposedException("ZipFile"); }
                CheckUpdating();
                newComment_ = new ZipString(comment);
                if (newComment_.RawLength > 0xffff)
                {
                    newComment_ = null;
                    throw new Sys.ApplicationException("Comment length exceeds maximum - 65535");
                }
                commentEdited_ = true;
            }

            private void AddUpdate(ZipUpdate update)
            {
                contentsEdited_ = true;
                int index = FindExistingUpdate(update.Entry.Name);
                if (index >= 0)
                {
                    if (updates_[index] == null) { updateCount_ += 1; }
                    updates_[index] = update;
                }
                else
                {
                    index = updates_.Add(update);
                    updateCount_ += 1;
                    updateIndex_.Add(update.Entry.Name, index);
                }
            }

            public void Add(string fileName, CompressionMethod compressionMethod, bool useUnicodeText)
            {
                if (fileName == null) { throw new Sys.ArgumentNullException("fileName"); }
                if (isDisposed_) { throw new Sys.ObjectDisposedException("ZipFile"); }
                if (!ZipEntry.IsCompressionMethodSupported(compressionMethod)) { throw new Sys.ArgumentOutOfRangeException("compressionMethod"); }
                CheckUpdating();
                contentsEdited_ = true;
                ZipEntry entry = EntryFactory.MakeFileEntry(fileName);
                entry.IsUnicodeText = useUnicodeText;
                entry.CompressionMethod = compressionMethod;
                AddUpdate(new ZipUpdate(fileName, entry));
            }

            public void Add(string fileName, CompressionMethod compressionMethod)
            {
                if (fileName == null) { throw new Sys.ArgumentNullException("fileName"); }
                if (!ZipEntry.IsCompressionMethodSupported(compressionMethod)) { throw new Sys.ArgumentOutOfRangeException("compressionMethod"); }
                CheckUpdating();
                contentsEdited_ = true;
                ZipEntry entry = EntryFactory.MakeFileEntry(fileName);
                entry.CompressionMethod = compressionMethod;
                AddUpdate(new ZipUpdate(fileName, entry));
            }

            public void Add(string fileName)
            {
                if (fileName == null) { throw new Sys.ArgumentNullException("fileName"); }
                CheckUpdating();
                AddUpdate(new ZipUpdate(fileName, EntryFactory.MakeFileEntry(fileName)));
            }

            public void Add(string fileName, string entryName)
            {
                if (fileName == null) { throw new Sys.ArgumentNullException("fileName"); }
                if (entryName == null) { throw new Sys.ArgumentNullException("entryName"); }
                CheckUpdating();
                AddUpdate(new ZipUpdate(fileName, EntryFactory.MakeFileEntry(fileName, entryName, true)));
            }

            public void Add(IStaticDataSource dataSource, string entryName)
            {
                if (dataSource == null) { throw new Sys.ArgumentNullException("dataSource"); }
                if (entryName == null) { throw new Sys.ArgumentNullException("entryName"); }
                CheckUpdating();
                AddUpdate(new ZipUpdate(dataSource, EntryFactory.MakeFileEntry(entryName, false)));
            }

            public void Add(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod)
            {
                if (dataSource == null) { throw new Sys.ArgumentNullException("dataSource"); }
                if (entryName == null) { throw new Sys.ArgumentNullException("entryName"); }
                CheckUpdating();
                ZipEntry entry = EntryFactory.MakeFileEntry(entryName, false);
                entry.CompressionMethod = compressionMethod;
                AddUpdate(new ZipUpdate(dataSource, entry));
            }

            public void Add(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod, bool useUnicodeText)
            {
                if (dataSource == null) { throw new Sys.ArgumentNullException("dataSource"); }
                if (entryName == null) { throw new Sys.ArgumentNullException("entryName"); }
                CheckUpdating();
                ZipEntry entry = EntryFactory.MakeFileEntry(entryName, false);
                entry.IsUnicodeText = useUnicodeText;
                entry.CompressionMethod = compressionMethod;
                AddUpdate(new ZipUpdate(dataSource, entry));
            }

            public void Add(ZipEntry entry)
            {
                if (entry == null) { throw new Sys.ArgumentNullException("entry"); }
                CheckUpdating();
                if ((entry.Size != 0) || (entry.CompressedSize != 0)) { throw new Sys.ApplicationException("Entry cannot have any data"); }
                AddUpdate(new ZipUpdate(UpdateCommand.Add, entry));
            }

            public ZipEntry AddDirectory(string directoryName)
            {
                if (directoryName == null) { throw new Sys.ArgumentNullException("directoryName"); }
                CheckUpdating();
                ZipEntry dirEntry = EntryFactory.MakeDirectoryEntry(directoryName);
                AddUpdate(new ZipUpdate(UpdateCommand.Add, dirEntry));
                return dirEntry;
            }

            public bool Delete(string fileName)
            {
                if (fileName == null) { throw new Sys.ArgumentNullException("fileName"); }
                CheckUpdating();
                bool result = false;
                int index = FindExistingUpdate(fileName);
                if ((index >= 0) && (updates_[index] != null))
                {
                    result = true;
                    contentsEdited_ = true;
                    updates_[index] = null;
                    updateCount_ -= 1;
                } else { throw new Sys.ApplicationException("Cannot find entry to delete"); }
                return result;
            }

            public void Delete(ZipEntry entry)
            {
                if (entry == null) { throw new Sys.ArgumentNullException("entry"); }
                CheckUpdating();
                int index = FindExistingUpdate(entry);
                if (index >= 0)
                {
                    contentsEdited_ = true;
                    updates_[index] = null;
                    updateCount_ -= 1;
                } else { throw new Sys.ApplicationException("Cannot find entry to delete"); }
            }

            void WriteLEShort(int value)
            {
                baseStream_.WriteByte((byte)(value & 0xff));
                baseStream_.WriteByte((byte)((value >> 8) & 0xff));
            }

            void WriteLEUshort(ushort value)
            {
                baseStream_.WriteByte((byte)(value & 0xff));
                baseStream_.WriteByte((byte)(value >> 8));
            }

            void WriteLEInt(int value)
            {
                WriteLEShort(value & 0xffff);
                WriteLEShort(value >> 16);
            }

            void WriteLEUint(uint value)
            {
                WriteLEUshort((ushort)(value & 0xffff));
                WriteLEUshort((ushort)(value >> 16));
            }

            void WriteLeLong(long value)
            {
                WriteLEInt((int)(value & 0xffffffff));
                WriteLEInt((int)(value >> 32));
            }

            void WriteLEUlong(ulong value)
            {
                WriteLEUint((uint)(value & 0xffffffff));
                WriteLEUint((uint)(value >> 32));
            }

            void WriteLocalEntryHeader(ZipUpdate update)
            {
                ZipEntry entry = update.OutEntry;
                entry.Offset = baseStream_.Position;
                if (update.Command != UpdateCommand.Copy)
                {
                    if (entry.CompressionMethod == CompressionMethod.Deflated)
                    {
                        if (entry.Size == 0)
                        {
                            entry.CompressedSize = entry.Size;
                            entry.Crc = 0;
                            entry.CompressionMethod = CompressionMethod.Stored;
                        }
                    }
                    else if (entry.CompressionMethod == CompressionMethod.Stored) { entry.Flags &= ~(int)GeneralBitFlags.Descriptor; }
                    if (HaveKeys)
                    {
                        entry.IsCrypted = true;
                        if (entry.Crc < 0) { entry.Flags |= (int)GeneralBitFlags.Descriptor; }
                    } else { entry.IsCrypted = false; }
                    switch (useZip64_)
                    {
                        case UseZip64.Dynamic: if (entry.Size < 0) { entry.ForceZip64(); } break;
                        case UseZip64.On: entry.ForceZip64(); break;
                        case UseZip64.Off: break;
                    }
                }
                WriteLEInt(ZipConstants.LocalHeaderSignature);
                WriteLEShort(entry.Version);
                WriteLEShort(entry.Flags);
                WriteLEShort((byte)entry.CompressionMethod);
                WriteLEInt((int)entry.DosTime);
                if (!entry.HasCrc)
                {
                    update.CrcPatchOffset = baseStream_.Position;
                    WriteLEInt((int)0);
                } else { WriteLEInt(unchecked((int)entry.Crc)); }
                if (entry.LocalHeaderRequiresZip64)
                {
                    WriteLEInt(-1);
                    WriteLEInt(-1);
                }
                else
                {
                    if ((entry.CompressedSize < 0) || (entry.Size < 0)) { update.SizePatchOffset = baseStream_.Position; }
                    WriteLEInt((int)entry.CompressedSize);
                    WriteLEInt((int)entry.Size);
                }
                byte[] name = ZipConstants.ConvertToArray(entry.Flags, entry.Name);
                if (name.Length > 0xFFFF) { throw new Sys.ApplicationException("Entry name too long."); }
                ZipExtraData ed = new ZipExtraData(entry.ExtraData);
                if (entry.LocalHeaderRequiresZip64)
                {
                    ed.StartNewEntry();
                    ed.AddLeLong(entry.Size);
                    ed.AddLeLong(entry.CompressedSize);
                    ed.AddNewEntry(1);
                } else { ed.Delete(1); }
                entry.ExtraData = ed.GetEntryData();
                WriteLEShort(name.Length);
                WriteLEShort(entry.ExtraData.Length);
                if (name.Length > 0) { baseStream_.Write(name, 0, name.Length); }
                if (entry.LocalHeaderRequiresZip64)
                {
                    if (!ed.Find(1)) { throw new Sys.ApplicationException("Internal error cannot find extra data"); }
                    update.SizePatchOffset = baseStream_.Position + ed.CurrentReadIndex;
                }
                if (entry.ExtraData.Length > 0) { baseStream_.Write(entry.ExtraData, 0, entry.ExtraData.Length); }
            }

            int WriteCentralDirectoryHeader(ZipEntry entry)
            {
                if (entry.CompressedSize < 0) { throw new Sys.ApplicationException("Attempt to write central directory entry with unknown csize"); }
                if (entry.Size < 0) { throw new Sys.ApplicationException("Attempt to write central directory entry with unknown size"); }
                if (entry.Crc < 0) { throw new Sys.ApplicationException("Attempt to write central directory entry with unknown crc"); }
                WriteLEInt(ZipConstants.CentralHeaderSignature);
                WriteLEShort(ZipConstants.VersionMadeBy);
                WriteLEShort(entry.Version);
                WriteLEShort(entry.Flags);
                unchecked
                {
                    WriteLEShort((byte)entry.CompressionMethod);
                    WriteLEInt((int)entry.DosTime);
                    WriteLEInt((int)entry.Crc);
                }
                if ((entry.IsZip64Forced()) || (entry.CompressedSize >= 0xffffffff)) { WriteLEInt(-1); } else { WriteLEInt((int)(entry.CompressedSize & 0xffffffff)); }
                if ((entry.IsZip64Forced()) || (entry.Size >= 0xffffffff)) { WriteLEInt(-1); } else { WriteLEInt((int)entry.Size); }
                byte[] name = ZipConstants.ConvertToArray(entry.Flags, entry.Name);
                if (name.Length > 0xFFFF) { throw new Sys.ApplicationException("Entry name is too long."); }
                WriteLEShort(name.Length);
                ZipExtraData ed = new ZipExtraData(entry.ExtraData);
                if (entry.CentralHeaderRequiresZip64)
                {
                    ed.StartNewEntry();
                    if ((entry.Size >= 0xffffffff) || (useZip64_ == UseZip64.On)) { ed.AddLeLong(entry.Size); }
                    if ((entry.CompressedSize >= 0xffffffff) || (useZip64_ == UseZip64.On)) { ed.AddLeLong(entry.CompressedSize); }
                    if (entry.Offset >= 0xffffffff) { ed.AddLeLong(entry.Offset); }
                    ed.AddNewEntry(1);
                } else { ed.Delete(1); }
                byte[] centralExtraData = ed.GetEntryData();
                WriteLEShort(centralExtraData.Length);
                WriteLEShort(entry.Comment != null ? entry.Comment.Length : 0);
                WriteLEShort(0);
                WriteLEShort(0);
                if (entry.ExternalFileAttributes != -1) { WriteLEInt(entry.ExternalFileAttributes); } else { if (entry.IsDirectory) { WriteLEUint(16); } else { WriteLEUint(0); } }
                if (entry.Offset >= 0xffffffff) { WriteLEUint(0xffffffff); } else { WriteLEUint((uint)(int)entry.Offset); }
                if (name.Length > 0) { baseStream_.Write(name, 0, name.Length); }
                if (centralExtraData.Length > 0) { baseStream_.Write(centralExtraData, 0, centralExtraData.Length); }
                byte[] rawComment = (entry.Comment != null) ? SysTxt.Encoding.ASCII.GetBytes(entry.Comment) : new byte[0];
                if (rawComment.Length > 0) { baseStream_.Write(rawComment, 0, rawComment.Length); }
                return ZipConstants.CentralHeaderBaseSize + name.Length + centralExtraData.Length + rawComment.Length;
            }

            internal void PostUpdateCleanup()
            {
                updateDataSource_ = null;
                updates_ = null;
                updateIndex_ = null;
                if (archiveStorage_ != null)
                {
                    archiveStorage_.Dispose();
                    archiveStorage_ = null;
                }
            }

            internal string GetTransformedFileName(string name)
            {
                ZIPLib.Internal.INameTransform transform = NameTransform;
                return (transform != null) ? transform.TransformFile(name) : name;
            }

            internal string GetTransformedDirectoryName(string name)
            {
                ZIPLib.Internal.INameTransform transform = NameTransform;
                return (transform != null) ? transform.TransformDirectory(name) : name;
            }

            internal byte[] GetBuffer()
            {
                if (copyBuffer_ == null) { copyBuffer_ = new byte[bufferSize_]; }
                return copyBuffer_;
            }

            private void CopyDescriptorBytes(ZipUpdate update, Sys.IO.Stream dest, Sys.IO.Stream source)
            {
                int bytesToCopy = GetDescriptorSize(update);
                if (bytesToCopy > 0)
                {
                    byte[] buffer = GetBuffer();
                    while (bytesToCopy > 0)
                    {
                        int readSize = SysMath.Min(buffer.Length, bytesToCopy);
                        int bytesRead = source.Read(buffer, 0, readSize);
                        if (bytesRead > 0)
                        {
                            dest.Write(buffer, 0, bytesRead);
                            bytesToCopy -= bytesRead;
                        } else { throw new Sys.ApplicationException("Unxpected end of stream"); }
                    }
                }
            }

            private void CopyBytes(ZipUpdate update, Sys.IO.Stream destination, Sys.IO.Stream source, long bytesToCopy, bool updateCrc)
            {
                if (destination == source) { throw new Sys.InvalidOperationException("Destination and source are the same"); }
                ZIPLib.Checksums.Crc32 crc = new ZIPLib.Checksums.Crc32();
                byte[] buffer = GetBuffer();
                long targetBytes = bytesToCopy;
                long totalBytesRead = 0;
                int bytesRead;
                do
                {
                    int readSize = buffer.Length;
                    if (bytesToCopy < readSize) { readSize = (int)bytesToCopy; }
                    bytesRead = source.Read(buffer, 0, readSize);
                    if (bytesRead > 0)
                    {
                        if (updateCrc) { crc.Update(buffer, 0, bytesRead); }
                        destination.Write(buffer, 0, bytesRead);
                        bytesToCopy -= bytesRead;
                        totalBytesRead += bytesRead;
                    }
                }
                while ((bytesRead > 0) && (bytesToCopy > 0));
                if (totalBytesRead != targetBytes) { throw new Sys.ApplicationException(string.Format("Failed to copy bytes expected {0} read {1}", targetBytes, totalBytesRead)); }
                if (updateCrc) { update.OutEntry.Crc = crc.Value; }
            }

            private int GetDescriptorSize(ZipUpdate update)
            {
                int result = 0;
                if ((update.Entry.Flags & (int)GeneralBitFlags.Descriptor) != 0)
                {
                    result = ZipConstants.DataDescriptorSize - 4;
                    if (update.Entry.LocalHeaderRequiresZip64) { result = ZipConstants.Zip64DataDescriptorSize - 4; }
                }
                return result;
            }

            private void CopyDescriptorBytesDirect(ZipUpdate update, Sys.IO.Stream stream, ref long destinationPosition, long sourcePosition)
            {
                int bytesToCopy = GetDescriptorSize(update);
                while (bytesToCopy > 0)
                {
                    int readSize = (int)bytesToCopy;
                    byte[] buffer = GetBuffer();
                    stream.Position = sourcePosition;
                    int bytesRead = stream.Read(buffer, 0, readSize);
                    if (bytesRead > 0)
                    {
                        stream.Position = destinationPosition;
                        stream.Write(buffer, 0, bytesRead);
                        bytesToCopy -= bytesRead;
                        destinationPosition += bytesRead;
                        sourcePosition += bytesRead;
                    } else { throw new Sys.ApplicationException("Unxpected end of stream"); }
                }
            }

            private void CopyEntryDataDirect(ZipUpdate update, Sys.IO.Stream stream, bool updateCrc, ref long destinationPosition, ref long sourcePosition)
            {
                long bytesToCopy = update.Entry.CompressedSize;
                ZIPLib.Checksums.Crc32 crc = new ZIPLib.Checksums.Crc32();
                byte[] buffer = GetBuffer();
                long targetBytes = bytesToCopy;
                long totalBytesRead = 0;
                int bytesRead;
                do
                {
                    int readSize = buffer.Length;
                    if (bytesToCopy < readSize) { readSize = (int)bytesToCopy; }
                    stream.Position = sourcePosition;
                    bytesRead = stream.Read(buffer, 0, readSize);
                    if (bytesRead > 0)
                    {
                        if (updateCrc) { crc.Update(buffer, 0, bytesRead); }
                        stream.Position = destinationPosition;
                        stream.Write(buffer, 0, bytesRead);
                        destinationPosition += bytesRead;
                        sourcePosition += bytesRead;
                        bytesToCopy -= bytesRead;
                        totalBytesRead += bytesRead;
                    }
                }
                while ((bytesRead > 0) && (bytesToCopy > 0));
                if (totalBytesRead != targetBytes) { throw new Sys.ApplicationException(string.Format("Failed to copy bytes expected {0} read {1}", targetBytes, totalBytesRead)); }
                if (updateCrc) { update.OutEntry.Crc = crc.Value; }
            }

            internal int FindExistingUpdate(ZipEntry entry)
            {
                int result = -1;
                string convertedName = GetTransformedFileName(entry.Name);
                if (updateIndex_.ContainsKey(convertedName)) { result = (int)updateIndex_[convertedName]; }
                return result;
            }

            internal int FindExistingUpdate(string fileName)
            {
                int result = -1;
                string convertedName = GetTransformedFileName(fileName);
                if (updateIndex_.ContainsKey(convertedName)) { result = (int)updateIndex_[convertedName]; }
                return result;
            }

            internal Sys.IO.Stream GetOutputStream(ZipEntry entry)
            {
                Sys.IO.Stream result = baseStream_;
                if (entry.IsCrypted == true) { result = CreateAndInitEncryptionStream(result, entry); }
                switch (entry.CompressionMethod)
                {
                    case CompressionMethod.Stored: result = new UncompressedStream(result); break;
                    case CompressionMethod.Deflated:
                        ZIPLib.Compression.Streams.DeflaterOutputStream dos = new ZIPLib.Compression.Streams.DeflaterOutputStream(result, new ZIPLib.Compression.Deflater(9, true));
                        dos.IsStreamOwner = false;
                        result = dos;
                        break;
                    default: throw new Sys.ApplicationException("Unknown compression method " + entry.CompressionMethod);
                }
                return result;
            }

            private void AddEntry(ZipFile workFile, ZipUpdate update)
            {
                Sys.IO.Stream source = null;
                if (update.Entry.IsFile)
                {
                    source = update.GetSource();
                    if (source == null) { source = updateDataSource_.GetSource(update.Entry, update.Filename); }
                }
                if (source != null)
                {
                    using (source)
                    {
                        long sourceStreamLength = source.Length;
                        if (update.OutEntry.Size < 0) { update.OutEntry.Size = sourceStreamLength; } else { if (update.OutEntry.Size != sourceStreamLength) { throw new Sys.ApplicationException("Entry size/stream size mismatch"); } }
                        workFile.WriteLocalEntryHeader(update);
                        long dataStart = workFile.baseStream_.Position;
                        using (Sys.IO.Stream output = workFile.GetOutputStream(update.OutEntry)) { CopyBytes(update, output, source, sourceStreamLength, true); }
                        long dataEnd = workFile.baseStream_.Position;
                        update.OutEntry.CompressedSize = dataEnd - dataStart;
                        if ((update.OutEntry.Flags & (int)GeneralBitFlags.Descriptor) == (int)GeneralBitFlags.Descriptor)
                        {
                            ZipHelperStream helper = new ZipHelperStream(workFile.baseStream_);
                            helper.WriteDataDescriptor(update.OutEntry);
                        }
                    }
                }
                else
                {
                    workFile.WriteLocalEntryHeader(update);
                    update.OutEntry.CompressedSize = 0;
                }
            }

            private void ModifyEntry(ZipFile workFile, ZipUpdate update)
            {
                workFile.WriteLocalEntryHeader(update);
                long dataStart = workFile.baseStream_.Position;
                if (update.Entry.IsFile && (update.Filename != null)) { using (Sys.IO.Stream output = workFile.GetOutputStream(update.OutEntry)) { using (Sys.IO.Stream source = this.GetInputStream(update.Entry)) { CopyBytes(update, output, source, source.Length, true); } } }
                long dataEnd = workFile.baseStream_.Position;
                update.Entry.CompressedSize = dataEnd - dataStart;
            }

            private void CopyEntryDirect(ZipFile workFile, ZipUpdate update, ref long destinationPosition)
            {
                bool skipOver = false;
                if (update.Entry.Offset == destinationPosition) { skipOver = true; }
                if (!skipOver)
                {
                    baseStream_.Position = destinationPosition;
                    workFile.WriteLocalEntryHeader(update);
                    destinationPosition = baseStream_.Position;
                }
                long sourcePosition = 0;
                const int NameLengthOffset = 26;
                long entryDataOffset = update.Entry.Offset + NameLengthOffset;
                baseStream_.Seek(entryDataOffset, Sys.IO.SeekOrigin.Begin);
                uint nameLength = ReadLEUshort();
                uint extraLength = ReadLEUshort();
                sourcePosition = baseStream_.Position + nameLength + extraLength;
                if (skipOver) { if (update.OffsetBasedSize != -1) destinationPosition += update.OffsetBasedSize; else destinationPosition += (sourcePosition - entryDataOffset) + NameLengthOffset +	update.Entry.CompressedSize + GetDescriptorSize(update); }
                else
                {
                    if (update.Entry.CompressedSize > 0) { CopyEntryDataDirect(update, baseStream_, false, ref destinationPosition, ref sourcePosition); }
                    CopyDescriptorBytesDirect(update, baseStream_, ref destinationPosition, sourcePosition);
                }
            }

            private void CopyEntry(ZipFile workFile, ZipUpdate update)
            {
                workFile.WriteLocalEntryHeader(update);
                if (update.Entry.CompressedSize > 0)
                {
                    const int NameLengthOffset = 26;
                    long entryDataOffset = update.Entry.Offset + NameLengthOffset;
                    baseStream_.Seek(entryDataOffset, Sys.IO.SeekOrigin.Begin);
                    uint nameLength = ReadLEUshort();
                    uint extraLength = ReadLEUshort();
                    baseStream_.Seek(nameLength + extraLength, Sys.IO.SeekOrigin.Current);
                    CopyBytes(update, workFile.baseStream_, baseStream_, update.Entry.CompressedSize, false);
                }
                CopyDescriptorBytes(update, workFile.baseStream_, baseStream_);
            }

            private void Reopen(Sys.IO.Stream source)
            {
                if (source == null) { throw new Sys.ApplicationException("Failed to reopen archive - no source"); }
                isNewArchive_ = false;
                baseStream_ = source;
                ReadEntries();
            }

            private void Reopen() { Reopen(Sys.IO.File.Open(Name, Sys.IO.FileMode.Open, Sys.IO.FileAccess.Read, Sys.IO.FileShare.Read)); }

            private void UpdateCommentOnly()
            {
                long baseLength = baseStream_.Length;
                ZipHelperStream updateFile = null;
                if (archiveStorage_.UpdateMode == FileUpdateMode.Safe)
                {
                    Sys.IO.Stream copyStream = archiveStorage_.MakeTemporaryCopy(baseStream_);
                    updateFile = new ZipHelperStream(copyStream);
                    updateFile.IsStreamOwner = true;
                    baseStream_.Close();
                    baseStream_ = null;
                }
                else
                {
                    if (archiveStorage_.UpdateMode == FileUpdateMode.Direct)
                    {
                        baseStream_ = archiveStorage_.OpenForDirectUpdate(baseStream_);
                        updateFile = new ZipHelperStream(baseStream_);
                    }
                    else
                    {
                        baseStream_.Close();
                        baseStream_ = null;
                        updateFile = new ZipHelperStream(Name);
                    }
                }
                using (updateFile)
                {
                    long locatedCentralDirOffset = updateFile.LocateBlockWithSignature(ZipConstants.EndOfCentralDirectorySignature, baseLength, ZipConstants.EndOfCentralRecordBaseSize, 0xffff);
                    if (locatedCentralDirOffset < 0) { throw new Sys.ApplicationException("Cannot find central directory"); }
                    const int CentralHeaderCommentSizeOffset = 16;
                    updateFile.Position += CentralHeaderCommentSizeOffset;
                    byte[] rawComment = newComment_.RawComment;
                    updateFile.WriteLEShort(rawComment.Length);
                    updateFile.Write(rawComment, 0, rawComment.Length);
                    updateFile.SetLength(updateFile.Position);
                }
                if (archiveStorage_.UpdateMode == FileUpdateMode.Safe) { Reopen(archiveStorage_.ConvertTemporaryToFinal()); } else { ReadEntries(); }
            }

            private class UpdateComparer : SysColl.IComparer
            {
                public int Compare( object x, object y)
                {
                    ZipUpdate zx = x as ZipUpdate;
                    ZipUpdate zy = y as ZipUpdate;
                    int result;
                    if (zx == null) { if (zy == null) { result = 0; } else { result = -1; } }
                    else if (zy == null) { result = 1; }
                    else
                    {
                        int xCmdValue = ((zx.Command == UpdateCommand.Copy) || (zx.Command == UpdateCommand.Modify)) ? 0 : 1;
                        int yCmdValue = ((zy.Command == UpdateCommand.Copy) || (zy.Command == UpdateCommand.Modify)) ? 0 : 1;
                        result = xCmdValue - yCmdValue;
                        if (result == 0)
                        {
                            long offsetDiff = zx.Entry.Offset - zy.Entry.Offset;
                            if (offsetDiff < 0) { result = -1; }
                            else if (offsetDiff == 0) { result = 0; }
                            else { result = 1; }
                        }
                    }
                    return result;
                }
            }

            void RunUpdates()
            {
                long sizeEntries = 0;
                long endOfStream = 0;
                bool directUpdate = false;
                long destinationPosition = 0; // NOT SFX friendly

                ZipFile workFile;

                if (IsNewArchive)
                {
                    workFile = this;
                    workFile.baseStream_.Position = 0;
                    directUpdate = true;
                }
                else if (archiveStorage_.UpdateMode == FileUpdateMode.Direct)
                {
                    workFile = this;
                    workFile.baseStream_.Position = 0;
                    directUpdate = true;

                    // Sort the updates by offset within copies/modifies, then adds.
                    // This ensures that data required by copies will not be overwritten.
                    updates_.Sort(new UpdateComparer());
                }
                else
                {
                    workFile = ZipFile.Create(archiveStorage_.GetTemporaryOutput());
                    workFile.UseZip64 = UseZip64;

                    if (key != null)
                    {
                        workFile.key = (byte[])key.Clone();
                    }
                }

                try
                {
                    foreach (ZipUpdate update in updates_)
                    {
                        if (update != null)
                        {
                            switch (update.Command)
                            {
                                case UpdateCommand.Copy:
                                    if (directUpdate)
                                    {
                                        CopyEntryDirect(workFile, update, ref destinationPosition);
                                    }
                                    else
                                    {
                                        CopyEntry(workFile, update);
                                    }
                                    break;

                                case UpdateCommand.Modify:
                                    ModifyEntry(workFile, update);
                                    break;

                                case UpdateCommand.Add:
                                    if (!IsNewArchive && directUpdate)
                                    {
                                        workFile.baseStream_.Position = destinationPosition;
                                    }

                                    AddEntry(workFile, update);

                                    if (directUpdate)
                                    {
                                        destinationPosition = workFile.baseStream_.Position;
                                    }
                                    break;
                            }
                        }
                    }

                    if (!IsNewArchive && directUpdate)
                    {
                        workFile.baseStream_.Position = destinationPosition;
                    }

                    long centralDirOffset = workFile.baseStream_.Position;

                    foreach (ZipUpdate update in updates_)
                    {
                        if (update != null)
                        {
                            sizeEntries += workFile.WriteCentralDirectoryHeader(update.OutEntry);
                        }
                    }

                    byte[] theComment = (newComment_ != null) ? newComment_.RawComment : ZipConstants.ConvertToArray(comment_);
                    using (ZipHelperStream zhs = new ZipHelperStream(workFile.baseStream_))
                    {
                        zhs.WriteEndOfCentralDirectory(updateCount_, sizeEntries, centralDirOffset, theComment);
                    }

                    endOfStream = workFile.baseStream_.Position;

                    // And now patch entries...
                    foreach (ZipUpdate update in updates_)
                    {
                        if (update != null)
                        {
                            // If the size of the entry is zero leave the crc as 0 as well.
                            // The calculated crc will be all bits on...
                            if ((update.CrcPatchOffset > 0) && (update.OutEntry.CompressedSize > 0))
                            {
                                workFile.baseStream_.Position = update.CrcPatchOffset;
                                workFile.WriteLEInt((int)update.OutEntry.Crc);
                            }

                            if (update.SizePatchOffset > 0)
                            {
                                workFile.baseStream_.Position = update.SizePatchOffset;
                                if (update.OutEntry.LocalHeaderRequiresZip64)
                                {
                                    workFile.WriteLeLong(update.OutEntry.Size);
                                    workFile.WriteLeLong(update.OutEntry.CompressedSize);
                                }
                                else
                                {
                                    workFile.WriteLEInt((int)update.OutEntry.CompressedSize);
                                    workFile.WriteLEInt((int)update.OutEntry.Size);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    workFile.Close();
                    if (!directUpdate && (workFile.Name != null))
                    {
                        Sys.IO.File.Delete(workFile.Name);
                    }
                    throw;
                }

                if (directUpdate)
                {
                    workFile.baseStream_.SetLength(endOfStream);
                    workFile.baseStream_.Flush();
                    isNewArchive_ = false;
                    ReadEntries();
                }
                else
                {
                    baseStream_.Close();
                    Reopen(archiveStorage_.ConvertTemporaryToFinal());
                }
            }

            void CheckUpdating()
            {
                if (updates_ == null)
                {
                    throw new Sys.InvalidOperationException("BeginUpdate has not been called");
                }
            }

            private class ZipUpdate
            {
                public ZipUpdate(string fileName, ZipEntry entry)
                {
                    command_ = UpdateCommand.Add;
                    entry_ = entry;
                    filename_ = fileName;
                }

                [Sys.Obsolete]
                public ZipUpdate(string fileName, string entryName, ZIPLib.CompressionMethod compressionMethod)
                {
                    command_ = UpdateCommand.Add;
                    entry_ = new ZipEntry(entryName);
                    entry_.CompressionMethod = compressionMethod;
                    filename_ = fileName;
                }

                [Sys.Obsolete]
                public ZipUpdate(string fileName, string entryName)
                    : this(fileName, entryName, ZIPLib.CompressionMethod.Deflated)
                {
                    // Do nothing.
                }

                [Sys.Obsolete]
                public ZipUpdate(IStaticDataSource dataSource, string entryName, ZIPLib.CompressionMethod compressionMethod)
                {
                    command_ = UpdateCommand.Add;
                    entry_ = new ZipEntry(entryName);
                    entry_.CompressionMethod = compressionMethod;
                    dataSource_ = dataSource;
                }

                public ZipUpdate(IStaticDataSource dataSource, ZipEntry entry)
                {
                    command_ = UpdateCommand.Add;
                    entry_ = entry;
                    dataSource_ = dataSource;
                }

                public ZipUpdate(ZipEntry original, ZipEntry updated)
                {
                    throw new Sys.ApplicationException("Modify not currently supported");
                    /*
                        command_ = UpdateCommand.Modify;
                        entry_ = ( ZipEntry )original.Clone();
                        outEntry_ = ( ZipEntry )updated.Clone();
                    */
                }

                public ZipUpdate(UpdateCommand command, ZipEntry entry)
                {
                    command_ = command;
                    entry_ = (ZipEntry)entry.Clone();
                }

                /// <summary>
                /// Copy an existing entry.
                /// </summary>
                /// <param name="entry">The existing entry to copy.</param>
                public ZipUpdate(ZipEntry entry)
                    : this(UpdateCommand.Copy, entry)
                {
                    // Do nothing.
                }

                /// <summary>
                /// Get the <see cref="ZipEntry"/> for this update.
                /// </summary>
                /// <remarks>This is the source or original entry.</remarks>
                public ZipEntry Entry
                {
                    get { return entry_; }
                }

                /// <summary>
                /// Get the <see cref="ZipEntry"/> that will be written to the updated/new file.
                /// </summary>
                public ZipEntry OutEntry
                {
                    get
                    {
                        if (outEntry_ == null)
                        {
                            outEntry_ = (ZipEntry)entry_.Clone();
                        }

                        return outEntry_;
                    }
                }

                /// <summary>
                /// Get the command for this update.
                /// </summary>
                public UpdateCommand Command
                {
                    get { return command_; }
                }

                /// <summary>
                /// Get the filename if any for this update.  Null if none exists.
                /// </summary>
                public string Filename
                {
                    get { return filename_; }
                }

                /// <summary>
                /// Get/set the location of the size patch for this update.
                /// </summary>
                public long SizePatchOffset
                {
                    get { return sizePatchOffset_; }
                    set { sizePatchOffset_ = value; }
                }

                /// <summary>
                /// Get /set the location of the crc patch for this update.
                /// </summary>
                public long CrcPatchOffset
                {
                    get { return crcPatchOffset_; }
                    set { crcPatchOffset_ = value; }
                }

                /// <summary>
                /// Get/set the size calculated by offset.
                /// Specifically, the difference between this and next entry's starting offset.
                /// </summary>
                public long OffsetBasedSize
                {
                    get { return _offsetBasedSize; }
                    set { _offsetBasedSize = value; }
                }

                public Sys.IO.Stream GetSource()
                {
                    Sys.IO.Stream result = null;
                    if (dataSource_ != null)
                    {
                        result = dataSource_.GetSource();
                    }

                    return result;
                }

                ZipEntry entry_;
                ZipEntry outEntry_;
                UpdateCommand command_;
                IStaticDataSource dataSource_;
                string filename_;
                long sizePatchOffset_ = -1;
                long crcPatchOffset_ = -1;
                long _offsetBasedSize = -1;
            }

            public void Dispose()
            {
                Close();
            }

            void DisposeInternal(bool disposing)
            {
                if (!isDisposed_)
                {
                    isDisposed_ = true;
                    entries_ = new ZipEntry[0];

                    if (IsStreamOwner && (baseStream_ != null))
                    {
                        lock (baseStream_)
                        {
                            baseStream_.Close();
                        }
                    }

                    PostUpdateCleanup();
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                DisposeInternal(disposing);
            }

            ushort ReadLEUshort()
            {
                int data1 = baseStream_.ReadByte();

                if (data1 < 0)
                {
                    throw new Sys.IO.EndOfStreamException("End of stream");
                }

                int data2 = baseStream_.ReadByte();

                if (data2 < 0)
                {
                    throw new Sys.IO.EndOfStreamException("End of stream");
                }


                return unchecked((ushort)((ushort)data1 | (ushort)(data2 << 8)));
            }

            uint ReadLEUint()
            {
                return (uint)(ReadLEUshort() | (ReadLEUshort() << 16));
            }

            ulong ReadLEUlong()
            {
                return ReadLEUint() | ((ulong)ReadLEUint() << 32);
            }

            long LocateBlockWithSignature(int signature, long endLocation, int minimumBlockSize, int maximumVariableData)
            {
                using (ZipHelperStream les = new ZipHelperStream(baseStream_))
                {
                    return les.LocateBlockWithSignature(signature, endLocation, minimumBlockSize, maximumVariableData);
                }
            }

            void ReadEntries()
            {
                if (baseStream_.CanSeek == false) { throw new Sys.ApplicationException("ZipFile stream must be seekable"); }
                long locatedEndOfCentralDir = LocateBlockWithSignature(ZipConstants.EndOfCentralDirectorySignature, baseStream_.Length, ZipConstants.EndOfCentralRecordBaseSize, 0xffff);
                if (locatedEndOfCentralDir < 0) { throw new Sys.ApplicationException("Cannot find central directory"); }
                ushort thisDiskNumber = ReadLEUshort();
                ushort startCentralDirDisk = ReadLEUshort();
                ulong entriesForThisDisk = ReadLEUshort();
                ulong entriesForWholeCentralDir = ReadLEUshort();
                ulong centralDirSize = ReadLEUint();
                long offsetOfCentralDir = ReadLEUint();
                uint commentSize = ReadLEUshort();
                if (commentSize > 0)
                {
                    byte[] comment = new byte[commentSize];
                    ZIPLib.Internal.StreamUtils.ReadFully(baseStream_, comment);
                    comment_ = ZipConstants.ConvertToString(comment);
                } else { comment_ = string.Empty; }
                bool isZip64 = false;
                if ((thisDiskNumber == 0xffff) || (startCentralDirDisk == 0xffff) || (entriesForThisDisk == 0xffff) || (entriesForWholeCentralDir == 0xffff) || (centralDirSize == 0xffffffff) || (offsetOfCentralDir == 0xffffffff))
                {
                    isZip64 = true;
                    long offset = LocateBlockWithSignature(ZipConstants.Zip64CentralDirLocatorSignature, locatedEndOfCentralDir, 0, 0x1000);
                    if (offset < 0) { throw new Sys.ApplicationException("Cannot find Zip64 locator"); } 
                    ReadLEUint();
                    ulong offset64 = ReadLEUlong();
                    uint totalDisks = ReadLEUint();
                    baseStream_.Position = (long)offset64;
                    long sig64 = ReadLEUint();
                    if (sig64 != ZipConstants.Zip64CentralFileHeaderSignature) { throw new Sys.ApplicationException(string.Format("Invalid Zip64 Central directory signature at {0:X}", offset64)); }
                    ulong recordSize = ReadLEUlong();
                    int versionMadeBy = ReadLEUshort();
                    int versionToExtract = ReadLEUshort();
                    uint thisDisk = ReadLEUint();
                    uint centralDirDisk = ReadLEUint();
                    entriesForThisDisk = ReadLEUlong();
                    entriesForWholeCentralDir = ReadLEUlong();
                    centralDirSize = ReadLEUlong();
                    offsetOfCentralDir = (long)ReadLEUlong();
                }
                entries_ = new ZipEntry[entriesForThisDisk];
                if (!isZip64 && (offsetOfCentralDir < locatedEndOfCentralDir - (4 + (long)centralDirSize)))
                {
                    offsetOfFirstEntry = locatedEndOfCentralDir - (4 + (long)centralDirSize + offsetOfCentralDir);
                    if (offsetOfFirstEntry <= 0) { throw new Sys.ApplicationException("Invalid embedded zip archive"); }
                }
                baseStream_.Seek(offsetOfFirstEntry + offsetOfCentralDir, Sys.IO.SeekOrigin.Begin);
                for (ulong i = 0; i < entriesForThisDisk; i++)
                {
                    if (ReadLEUint() != ZipConstants.CentralHeaderSignature) { throw new Sys.ApplicationException("Wrong Central Directory signature"); }
                    int versionMadeBy = ReadLEUshort();
                    int versionToExtract = ReadLEUshort();
                    int bitFlags = ReadLEUshort();
                    int method = ReadLEUshort();
                    uint dostime = ReadLEUint();
                    uint crc = ReadLEUint();
                    long csize = (long)ReadLEUint();
                    long size = (long)ReadLEUint();
                    int nameLen = ReadLEUshort();
                    int extraLen = ReadLEUshort();
                    int commentLen = ReadLEUshort();
                    int diskStartNo = ReadLEUshort();  // Not currently used
                    int internalAttributes = ReadLEUshort();  // Not currently used
                    uint externalAttributes = ReadLEUint();
                    long offset = ReadLEUint();
                    byte[] buffer = new byte[SysMath.Max(nameLen, commentLen)];
                    ZIPLib.Internal.StreamUtils.ReadFully(baseStream_, buffer, 0, nameLen);
                    string name = ZipConstants.ConvertToStringExt(bitFlags, buffer, nameLen);
                    ZipEntry entry = new ZipEntry(name, versionToExtract, versionMadeBy, (CompressionMethod)method);
                    entry.Crc = crc & 0xffffffffL;
                    entry.Size = size & 0xffffffffL;
                    entry.CompressedSize = csize & 0xffffffffL;
                    entry.Flags = bitFlags;
                    entry.DosTime = (uint)dostime;
                    entry.ZipFileIndex = (long)i;
                    entry.Offset = offset;
                    entry.ExternalFileAttributes = (int)externalAttributes;
                    if ((bitFlags & 8) == 0) { entry.CryptoCheckValue = (byte)(crc >> 24); } else { entry.CryptoCheckValue = (byte)((dostime >> 8) & 0xff); }
                    if (extraLen > 0)
                    {
                        byte[] extra = new byte[extraLen];
                        ZIPLib.Internal.StreamUtils.ReadFully(baseStream_, extra);
                        entry.ExtraData = extra;
                    }
                    entry.ProcessExtraData(false);
                    if (commentLen > 0)
                    {
                        ZIPLib.Internal.StreamUtils.ReadFully(baseStream_, buffer, 0, commentLen);
                        entry.Comment = ZipConstants.ConvertToStringExt(bitFlags, buffer, commentLen);
                    }
                    entries_[i] = entry;
                }
            }

            long LocateEntry(ZIPLib.ZipEntry entry)
            {
                return TestLocalHeader(entry, HeaderTest.Extract);
            }

            Sys.IO.Stream CreateAndInitDecryptionStream(Sys.IO.Stream baseStream, ZIPLib.ZipEntry entry)
            {
                Sys.Security.Cryptography.CryptoStream result = null;
                if ((entry.Version < ZipConstants.VersionStrongEncryption) || (entry.Flags & (int)GeneralBitFlags.StrongEncryption) == 0)
                {
                    ZIPLib.Encryption.PkzipClassicManaged classicManaged = new ZIPLib.Encryption.PkzipClassicManaged();

                    OnKeysRequired(entry.Name);
                    if (HaveKeys == false)
                    {
                        throw new Sys.ApplicationException("No password available for encrypted stream");
                    }
                    result = new Sys.Security.Cryptography.CryptoStream(baseStream, classicManaged.CreateDecryptor(key, null), Sys.Security.Cryptography.CryptoStreamMode.Read);
                    CheckClassicPassword(result, entry);
                }
                else
                {
                    if (entry.Version == ZIPLib.ZipConstants.VERSION_AES)
                    {
                        OnKeysRequired(entry.Name);
                        if (HaveKeys == false) { throw new Sys.Exception("No password available for AES encrypted stream"); }
                        int saltLen = entry.AESSaltLen;
                        byte[] saltBytes = new byte[saltLen];
                        int saltIn = baseStream.Read(saltBytes, 0, saltLen);
                        if (saltIn != saltLen) throw new Sys.ApplicationException("AES Salt expected " + saltLen + " got " + saltIn);
                        byte[] pwdVerifyRead = new byte[2];
                        baseStream.Read(pwdVerifyRead, 0, 2);
                        int blockSize = entry.AESKeySize / 8;	// bits to bytes
                        ZIPLib.Encryption.ZipAESTransform decryptor = new ZIPLib.Encryption.ZipAESTransform(rawPassword_, saltBytes, blockSize, false);
                        byte[] pwdVerifyCalc = decryptor.PwdVerifier;
                        if (pwdVerifyCalc[0] != pwdVerifyRead[0] || pwdVerifyCalc[1] != pwdVerifyRead[1]) throw new Sys.Exception("Invalid password for AES");
                        result = new ZIPLib.Encryption.ZipAESStream(baseStream, decryptor, Sys.Security.Cryptography.CryptoStreamMode.Read);
                    }
                    else { throw new Sys.Exception("Decryption method not supported"); }
                }
                return result;
            }

            Sys.IO.Stream CreateAndInitEncryptionStream(Sys.IO.Stream baseStream, ZIPLib.ZipEntry entry)
            {
                Sys.Security.Cryptography.CryptoStream result = null;
                if ((entry.Version < ZipConstants.VersionStrongEncryption) || (entry.Flags & (int)GeneralBitFlags.StrongEncryption) == 0)
                {
                    ZIPLib.Encryption.PkzipClassicManaged classicManaged = new ZIPLib.Encryption.PkzipClassicManaged();
                    OnKeysRequired(entry.Name);
                    if (HaveKeys == false) { throw new Sys.Exception("No password available for encrypted stream"); }
                    // Closing a CryptoStream will close the base stream as well so wrap it in an UncompressedStream which doesnt do this.
                    result = new Sys.Security.Cryptography.CryptoStream(new UncompressedStream(baseStream), classicManaged.CreateEncryptor(this.key, null), Sys.Security.Cryptography.CryptoStreamMode.Write);
                    if ((entry.Crc < 0) || (entry.Flags & 8) != 0) { WriteEncryptionHeader(result, entry.DosTime << 16); } else { WriteEncryptionHeader(result, entry.Crc); }
                }
                return result;
            }

            internal static void CheckClassicPassword(Sys.Security.Cryptography.CryptoStream classicCryptoStream, ZIPLib.ZipEntry entry)
            {
                byte[] cryptbuffer = new byte[ZIPLib.ZipConstants.CryptoHeaderSize];
                ZIPLib.Internal.StreamUtils.ReadFully(classicCryptoStream, cryptbuffer);
                if (cryptbuffer[ZIPLib.ZipConstants.CryptoHeaderSize - 1] != entry.CryptoCheckValue) { throw new Sys.Exception("Invalid password"); }
            }

            internal static void WriteEncryptionHeader(Sys.IO.Stream stream, long crcValue)
            {
                byte[] cryptBuffer = new byte[ZIPLib.ZipConstants.CryptoHeaderSize];
                Sys.Randomizer rnd = new Sys.Randomizer();
                rnd.NextBytes(cryptBuffer);
                cryptBuffer[11] = (byte)(crcValue >> 24);
                stream.Write(cryptBuffer, 0, cryptBuffer.Length);
            }

            bool isDisposed_;
            string name_;
            string comment_;
            string rawPassword_;
            Sys.IO.Stream baseStream_;
            bool isStreamOwner;
            long offsetOfFirstEntry;
            ZipEntry[] entries_;
            byte[] key;
            bool isNewArchive_;
            UseZip64 useZip64_ = UseZip64.Dynamic;
            SysColl.ArrayList updates_;
            long updateCount_;
            SysColl.Hashtable updateIndex_;
            IArchiveStorage archiveStorage_;
            IDynamicDataSource updateDataSource_;
            bool contentsEdited_;
            int bufferSize_ = DefaultBufferSize;
            byte[] copyBuffer_;
            ZipString newComment_;
            bool commentEdited_;
            IEntryFactory updateEntryFactory_ = new ZipEntryFactory();

            private class ZipString
            {
                public ZipString(string comment)
                {
                    comment_ = comment;
                    isSourceString_ = true;
                }

                public ZipString(byte[] rawString) { rawComment_ = rawString; }
                public bool IsSourceString { get { return isSourceString_; } }

                public int RawLength
                {
                    get
                    {
                        MakeBytesAvailable();
                        return rawComment_.Length;
                    }
                }

                public byte[] RawComment
                {
                    get
                    {
                        MakeBytesAvailable();
                        return (byte[])rawComment_.Clone();
                    }
                }

                public void Reset() { if (isSourceString_) { rawComment_ = null; } else { comment_ = null; } }
                void MakeTextAvailable() { if (comment_ == null) { comment_ = ZipConstants.ConvertToString(rawComment_); } }
                void MakeBytesAvailable() { if (rawComment_ == null) { rawComment_ = ZipConstants.ConvertToArray(comment_); } }

                static public implicit operator string(ZipString zipString)
                {
                    zipString.MakeTextAvailable();
                    return zipString.comment_;
                }

                string comment_;
                byte[] rawComment_;
                bool isSourceString_;
            }

            private class ZipEntryEnumerator : SysColl.Generic.IEnumerator<ZIPLib.ZipEntry>
            {
                public ZipEntryEnumerator(ZipEntry[] entries) { array = entries; }
                public ZIPLib.ZipEntry Current { get { return array[index]; } }
                object SysColl.IEnumerator.Current { get { return this.Current; } }
                public void Reset() { index = -1; }
                public bool MoveNext() { return (++index < array.Length); }
                ZipEntry[] array;
                int index = -1;
                void Sys.IDisposable.Dispose() { array = null; }
            }

            private class UncompressedStream : Sys.IO.Stream
            {
                public UncompressedStream(Sys.IO.Stream baseStream) { baseStream_ = baseStream; }
                public override void Close() { /* NOTHING */ }
                public override bool CanRead { get { return false; } }
                public override void Flush() { baseStream_.Flush(); }
                public override bool CanWrite { get { return baseStream_.CanWrite; } }
                public override bool CanSeek { get { return false; } }

                /// <summary>
                /// Get the length in bytes of the stream.
                /// </summary>
                public override long Length
                {
                    get
                    {
                        return 0;
                    }
                }

                /// <summary>
                /// Gets or sets the position within the current stream.
                /// </summary>
                public override long Position
                {
                    get
                    {
                        return baseStream_.Position;
                    }

                    set
                    {
                    }
                }

                /// <summary>
                /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
                /// </summary>
                /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
                /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
                /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
                /// <returns>
                /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
                /// </returns>
                /// <exception cref="T:System.Sys.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
                /// <exception cref="T:System.Sys.ObjectDisposedException">Methods were called after the stream was closed. </exception>
                /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
                /// <exception cref="T:System.Sys.ArgumentNullException">buffer is null. </exception>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
                public override int Read(byte[] buffer, int offset, int count)
                {
                    return 0;
                }

                /// <summary>
                /// Sets the position within the current stream.
                /// </summary>
                /// <param name="offset">A byte offset relative to the origin parameter.</param>
                /// <param name="origin">A value of Type <see cref="T:System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position.</param>
                /// <returns>
                /// The new position within the current stream.
                /// </returns>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
                /// <exception cref="T:System.Sys.ObjectDisposedException">Methods were called after the stream was closed. </exception>
                public override long Seek(long offset, Sys.IO.SeekOrigin origin)
                {
                    return 0;
                }

                /// <summary>
                /// Sets the length of the current stream.
                /// </summary>
                /// <param name="value">The desired length of the current stream in bytes.</param>
                /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <exception cref="T:System.Sys.ObjectDisposedException">Methods were called after the stream was closed. </exception>
                public override void SetLength(long value)
                {
                }

                /// <summary>
                /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
                /// </summary>
                /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
                /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
                /// <param name="count">The number of bytes to be written to the current stream.</param>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
                /// <exception cref="T:System.Sys.ObjectDisposedException">Methods were called after the stream was closed. </exception>
                /// <exception cref="T:System.Sys.ArgumentNullException">buffer is null. </exception>
                /// <exception cref="T:System.Sys.ArgumentException">The sum of offset and count is greater than the buffer length. </exception>
                /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
                public override void Write(byte[] buffer, int offset, int count)
                {
                    baseStream_.Write(buffer, offset, count);
                }

                Sys.IO.Stream baseStream_;
            }

            private class PartialInputStream : Sys.IO.Stream
            {
                /// <summary>
                /// Initialise a new instance of the <see cref="PartialInputStream"/> class.
                /// </summary>
                /// <param name="zipFile">The <see cref="ZipFile"/> containing the underlying stream to use for IO.</param>
                /// <param name="start">The start of the part of data.</param>
                /// <param name="length">The length of the part of data.</param>
                public PartialInputStream(ZipFile zipFile, long start, long length)
                {
                    start_ = start;
                    length_ = length;

                    // Although this is the only time the zipfile is used
                    // keeping a reference here prevents premature closure of
                    // this zip file and thus the baseStream_.

                    // Code like this will cause apparently random failures depending
                    // on the size of the files and when garbage is collected.
                    //
                    // ZipFile z = new ZipFile (stream);
                    // Sys.IO.Stream reader = z.GetInputStream(0);
                    // uses reader here....
                    zipFile_ = zipFile;
                    baseStream_ = zipFile_.baseStream_;
                    readPos_ = start;
                    end_ = start + length;
                }

                /// <summary>
                /// Read a byte from this stream.
                /// </summary>
                /// <returns>Returns the byte read or -1 on end of stream.</returns>
                public override int ReadByte()
                {
                    if (readPos_ >= end_)
                    {
                        // -1 is the correct value at end of stream.
                        return -1;
                    }

                    lock (baseStream_)
                    {
                        baseStream_.Seek(readPos_++, Sys.IO.SeekOrigin.Begin);
                        return baseStream_.ReadByte();
                    }
                }

                public override void Close() { /* Do nothing at all! */ }

                /// <summary>
                /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
                /// </summary>
                /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
                /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
                /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
                /// <returns>
                /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
                /// </returns>
                /// <exception cref="T:System.Sys.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
                /// <exception cref="T:System.Sys.ObjectDisposedException">Methods were called after the stream was closed. </exception>
                /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
                /// <exception cref="T:System.Sys.ArgumentNullException">buffer is null. </exception>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
                public override int Read(byte[] buffer, int offset, int count)
                {
                    lock (baseStream_)
                    {
                        if (count > end_ - readPos_)
                        {
                            count = (int)(end_ - readPos_);
                            if (count == 0)
                            {
                                return 0;
                            }
                        }

                        baseStream_.Seek(readPos_, Sys.IO.SeekOrigin.Begin);
                        int readCount = baseStream_.Read(buffer, offset, count);
                        if (readCount > 0)
                        {
                            readPos_ += readCount;
                        }
                        return readCount;
                    }
                }

                /// <summary>
                /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
                /// </summary>
                /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
                /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
                /// <param name="count">The number of bytes to be written to the current stream.</param>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
                /// <exception cref="T:System.Sys.ObjectDisposedException">Methods were called after the stream was closed. </exception>
                /// <exception cref="T:System.Sys.ArgumentNullException">buffer is null. </exception>
                /// <exception cref="T:System.Sys.ArgumentException">The sum of offset and count is greater than the buffer length. </exception>
                /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
                public override void Write(byte[] buffer, int offset, int count)
                {
                    throw new Sys.NotSupportedException();
                }

                /// <summary>
                /// When overridden in a derived class, sets the length of the current stream.
                /// </summary>
                /// <param name="value">The desired length of the current stream in bytes.</param>
                /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <exception cref="T:System.Sys.ObjectDisposedException">Methods were called after the stream was closed. </exception>
                public override void SetLength(long value)
                {
                    throw new Sys.NotSupportedException();
                }

                /// <summary>
                /// When overridden in a derived class, sets the position within the current stream.
                /// </summary>
                /// <param name="offset">A byte offset relative to the origin parameter.</param>
                /// <param name="origin">A value of Type <see cref="T:System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position.</param>
                /// <returns>
                /// The new position within the current stream.
                /// </returns>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
                /// <exception cref="T:System.Sys.ObjectDisposedException">Methods were called after the stream was closed. </exception>
                public override long Seek(long offset, Sys.IO.SeekOrigin origin)
                {
                    long newPos = readPos_;

                    switch (origin)
                    {
                        case Sys.IO.SeekOrigin.Begin:
                            newPos = start_ + offset;
                            break;

                        case Sys.IO.SeekOrigin.Current:
                            newPos = readPos_ + offset;
                            break;

                        case Sys.IO.SeekOrigin.End:
                            newPos = end_ + offset;
                            break;
                    }

                    if (newPos < start_) { throw new Sys.ArgumentException("Negative position is invalid"); }
                    if (newPos >= end_) { throw new Sys.IO.IOException("Cannot seek past end"); }
                    readPos_ = newPos;
                    return readPos_;
                }

                public override void Flush() { /* NOTHING */ }

                public override long Position
                {
                    get { return readPos_ - start_; }
                    set
                    {
                        long newPos = start_ + value;
                        if (newPos < start_) { throw new Sys.ArgumentException("Negative position is invalid"); }
                        if (newPos >= end_) { throw new Sys.InvalidOperationException("Cannot seek past end"); }
                        readPos_ = newPos;
                    }
                }

                public override long Length { get { return length_; } }
                public override bool CanWrite { get { return false; } }
                public override bool CanSeek { get { return true; } }
                public override bool CanRead { get { return true; } }
                public override bool CanTimeout { get { return baseStream_.CanTimeout; } }
                ZipFile zipFile_;
                Sys.IO.Stream baseStream_;
                long start_;
                long length_;
                long readPos_;
                long end_;
            }
        }
#endif

        public class ZipOutputStream : ZIPLib.Compression.Streams.DeflaterOutputStream
        {
            public ZipOutputStream(Sys.IO.Stream baseOutputStream) : base(baseOutputStream, new ZIPLib.Compression.Deflater(ZIPLib.Compression.Deflater.DEFAULT_COMPRESSION, true)) { /* NOTHING */ }
            public ZipOutputStream(Sys.IO.Stream baseOutputStream, int bufferSize) : base(baseOutputStream, new ZIPLib.Compression.Deflater(ZIPLib.Compression.Deflater.DEFAULT_COMPRESSION, true), bufferSize) { /* NOTHING */ }
            public bool IsFinished { get { return entries == null; } }

            public void SetComment(string comment)
            {
                byte[] commentBytes = ZipConstants.ConvertToArray(comment);
                if (commentBytes.Length > 0xffff) { throw new Sys.ArgumentOutOfRangeException("comment"); }
                zipComment = commentBytes;
            }

            public void SetLevel(int level)
            {
                deflater_.SetLevel(level);
                defaultCompressionLevel = level;
            }

            public int GetLevel() { return deflater_.GetLevel(); }

            public UseZip64 UseZip64
            {
                get { return useZip64_; }
                set { useZip64_ = value; }
            }

            private void WriteLeShort(int value)
            {
                unchecked
                {
                    baseOutputStream_.WriteByte((byte)(value & 0xff));
                    baseOutputStream_.WriteByte((byte)((value >> 8) & 0xff));
                }
            }

            private void WriteLeInt(int value)
            {
                unchecked
                {
                    WriteLeShort(value);
                    WriteLeShort(value >> 16);
                }
            }

            private void WriteLeLong(long value)
            {
                unchecked
                {
                    WriteLeInt((int)value);
                    WriteLeInt((int)(value >> 32));
                }
            }

            public void PutNextEntry(ZipEntry entry)
            {
                if (entry == null) { throw new Sys.ArgumentNullException("entry"); }
                if (entries == null) { throw new Sys.InvalidOperationException("ZipOutputStream was finished"); }
                if (curEntry != null) { CloseEntry(); }
                if (entries.Count == int.MaxValue) { throw new Sys.ApplicationException("Too many entries for Zip file"); }
                CompressionMethod method = entry.CompressionMethod;
                int compressionLevel = defaultCompressionLevel;
                entry.Flags &= (int)GeneralBitFlags.UnicodeText;
                patchEntryHeader = false;
                bool headerInfoAvailable;
                if (entry.Size == 0)
                {
                    entry.CompressedSize = entry.Size;
                    entry.Crc = 0;
                    method = CompressionMethod.Stored;
                    headerInfoAvailable = true;
                }
                else
                {
                    headerInfoAvailable = (entry.Size >= 0) && entry.HasCrc && entry.CompressedSize >= 0;
                    if (method == CompressionMethod.Stored)
                    {
                        if (!headerInfoAvailable)
                        {
                            if (!CanPatchEntries)
                            {
                                method = CompressionMethod.Deflated;
                                compressionLevel = 0;
                            }
                        }
                        else
                        {
                            entry.CompressedSize = entry.Size;
                            headerInfoAvailable = entry.HasCrc;
                        }
                    }
                }
                if (headerInfoAvailable == false) { if (CanPatchEntries == false) { entry.Flags |= 8; } else { patchEntryHeader = true; } }
                if (Password != null)
                {
                    entry.IsCrypted = true;
                    if (entry.Crc < 0) { entry.Flags |= 8; }
                }
                entry.Offset = offset;
                entry.CompressionMethod = (CompressionMethod)method;
                curMethod = method;
                sizePatchPos = -1;
                if ((useZip64_ == UseZip64.On) || ((entry.Size < 0) && (useZip64_ == UseZip64.Dynamic))) { entry.ForceZip64(); }
                WriteLeInt(ZipConstants.LocalHeaderSignature);
                WriteLeShort(entry.Version);
                WriteLeShort(entry.Flags);
                WriteLeShort((byte)entry.CompressionMethodForHeader);
                WriteLeInt((int)entry.DosTime);
                if (headerInfoAvailable)
                {
                    WriteLeInt((int)entry.Crc);
                    if (entry.LocalHeaderRequiresZip64)
                    {
                        WriteLeInt(-1);
                        WriteLeInt(-1);
                    }
                    else
                    {
                        WriteLeInt(entry.IsCrypted ? (int)entry.CompressedSize + ZipConstants.CryptoHeaderSize : (int)entry.CompressedSize);
                        WriteLeInt((int)entry.Size);
                    }
                }
                else
                {
                    if (patchEntryHeader) { crcPatchPos = baseOutputStream_.Position; }
                    WriteLeInt(0);
                    if (patchEntryHeader) { sizePatchPos = baseOutputStream_.Position; }
                    if (entry.LocalHeaderRequiresZip64 || patchEntryHeader)
                    {
                        WriteLeInt(-1);
                        WriteLeInt(-1);
                    }
                    else
                    {
                        WriteLeInt(0);
                        WriteLeInt(0);
                    }
                }
                byte[] name = ZipConstants.ConvertToArray(entry.Flags, entry.Name);
                if (name.Length > 0xFFFF) { throw new Sys.ApplicationException("Entry name too long."); }
                ZipExtraData ed = new ZipExtraData(entry.ExtraData);
                if (entry.LocalHeaderRequiresZip64)
                {
                    ed.StartNewEntry();
                    if (headerInfoAvailable)
                    {
                        ed.AddLeLong(entry.Size);
                        ed.AddLeLong(entry.CompressedSize);
                    }
                    else
                    {
                        ed.AddLeLong(-1);
                        ed.AddLeLong(-1);
                    }
                    ed.AddNewEntry(1);
                    if (!ed.Find(1)) { throw new Sys.ApplicationException("Internal error cant find extra data"); }
                    if (patchEntryHeader) { sizePatchPos = ed.CurrentReadIndex; }
                } else { ed.Delete(1); }
                if (entry.AESKeySize > 0) { AddExtraDataAES(entry, ed); }
                byte[] extra = ed.GetEntryData();
                WriteLeShort(name.Length);
                WriteLeShort(extra.Length);
                if (name.Length > 0) { baseOutputStream_.Write(name, 0, name.Length); }
                if (entry.LocalHeaderRequiresZip64 && patchEntryHeader) { sizePatchPos += baseOutputStream_.Position; }
                if (extra.Length > 0) { baseOutputStream_.Write(extra, 0, extra.Length); }
                offset += ZipConstants.LocalHeaderBaseSize + name.Length + extra.Length;
                if (entry.AESKeySize > 0) offset += entry.AESOverheadSize;
                curEntry = entry;
                crc.Reset();
                if (method == CompressionMethod.Deflated)
                {
                    deflater_.Reset();
                    deflater_.SetLevel(compressionLevel);
                }
                size = 0;
                if (entry.IsCrypted) { if (entry.AESKeySize > 0) { WriteAESHeader(entry); } else { if (entry.Crc < 0) { WriteEncryptionHeader(entry.DosTime << 16); } else { WriteEncryptionHeader(entry.Crc); } } }
            }

            public void CloseEntry()
            {
                if (curEntry == null) { throw new Sys.InvalidOperationException("No open entry"); }
                long csize = size;
                if (curMethod == CompressionMethod.Deflated)
                {
                    if (size >= 0)
                    {
                        base.Finish();
                        csize = deflater_.TotalOut;
                    } else { deflater_.Reset(); }
                }
                if (curEntry.AESKeySize > 0) { baseOutputStream_.Write(AESAuthCode, 0, 10); }
                if (curEntry.Size < 0) { curEntry.Size = size; }
                else if (curEntry.Size != size) { throw new Sys.ApplicationException("size was " + size + ", but I expected " + curEntry.Size); }
                if (curEntry.CompressedSize < 0) { curEntry.CompressedSize = csize; }
                else if (curEntry.CompressedSize != csize) { throw new Sys.ApplicationException("compressed size was " + csize + ", but I expected " + curEntry.CompressedSize); }
                if (curEntry.Crc < 0) { curEntry.Crc = crc.Value; }
                else if (curEntry.Crc != crc.Value) { throw new Sys.ApplicationException("crc was " + crc.Value + ", but I expected " + curEntry.Crc); }
                offset += csize;
                if (curEntry.IsCrypted) { if (curEntry.AESKeySize > 0) { curEntry.CompressedSize += curEntry.AESOverheadSize; } else { curEntry.CompressedSize += ZipConstants.CryptoHeaderSize; } }
                if (patchEntryHeader)
                {
                    patchEntryHeader = false;
                    long curPos = baseOutputStream_.Position;
                    baseOutputStream_.Seek(crcPatchPos, Sys.IO.SeekOrigin.Begin);
                    WriteLeInt((int)curEntry.Crc);
                    if (curEntry.LocalHeaderRequiresZip64)
                    {
                        if (sizePatchPos == -1) { throw new Sys.ApplicationException("Entry requires zip64 but this has been turned off"); }
                        baseOutputStream_.Seek(sizePatchPos, Sys.IO.SeekOrigin.Begin);
                        WriteLeLong(curEntry.Size);
                        WriteLeLong(curEntry.CompressedSize);
                    }
                    else
                    {
                        WriteLeInt((int)curEntry.CompressedSize);
                        WriteLeInt((int)curEntry.Size);
                    }
                    baseOutputStream_.Seek(curPos, Sys.IO.SeekOrigin.Begin);
                }
                if ((curEntry.Flags & 8) != 0)
                {
                    WriteLeInt(ZipConstants.DataDescriptorSignature);
                    WriteLeInt(unchecked((int)curEntry.Crc));
                    if (curEntry.LocalHeaderRequiresZip64)
                    {
                        WriteLeLong(curEntry.CompressedSize);
                        WriteLeLong(curEntry.Size);
                        offset += ZipConstants.Zip64DataDescriptorSize;
                    }
                    else
                    {
                        WriteLeInt((int)curEntry.CompressedSize);
                        WriteLeInt((int)curEntry.Size);
                        offset += ZipConstants.DataDescriptorSize;
                    }
                }
                entries.Add(curEntry);
                curEntry = null;
            }

            internal void WriteEncryptionHeader(long crcValue)
            {
                offset += ZipConstants.CryptoHeaderSize;
                InitializePassword(Password);
                byte[] cryptBuffer = new byte[ZipConstants.CryptoHeaderSize];
                Sys.Randomizer rnd = new Sys.Randomizer();
                rnd.NextBytes(cryptBuffer);
                cryptBuffer[11] = (byte)(crcValue >> 24);
                EncryptBlock(cryptBuffer, 0, cryptBuffer.Length);
                baseOutputStream_.Write(cryptBuffer, 0, cryptBuffer.Length);
            }

            private static void AddExtraDataAES(ZipEntry entry, ZipExtraData extraData)
            {
                const int VENDOR_VERSION = 2;
                const int VENDOR_ID = 0x4541;
                extraData.StartNewEntry();
                extraData.AddLeShort(VENDOR_VERSION);
                extraData.AddLeShort(VENDOR_ID);
                extraData.AddData(entry.AESEncryptionStrength);
                extraData.AddLeShort((int)entry.CompressionMethod);
                extraData.AddNewEntry(0x9901);
            }

            private void WriteAESHeader(ZipEntry entry)
            {
                byte[] salt;
                byte[] pwdVerifier;
                InitializeAESPassword(entry, Password, out salt, out pwdVerifier);
                baseOutputStream_.Write(salt, 0, salt.Length);
                baseOutputStream_.Write(pwdVerifier, 0, pwdVerifier.Length);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (curEntry == null) { throw new Sys.InvalidOperationException("No open entry."); }
                if (buffer == null) { throw new Sys.ArgumentNullException("buffer"); }
                if (offset < 0) { throw new Sys.ArgumentOutOfRangeException("offset", "Cannot be negative"); }
                if (count < 0) { throw new Sys.ArgumentOutOfRangeException("count", "Cannot be negative"); }
                if ((buffer.Length - offset) < count) { throw new Sys.ArgumentException("Invalid offset/count combination"); }
                crc.Update(buffer, offset, count);
                size += count;
                switch (curMethod)
                {
                    case CompressionMethod.Deflated: base.Write(buffer, offset, count); break;
                    case CompressionMethod.Stored: if (Password != null) { CopyAndEncrypt(buffer, offset, count); } else { baseOutputStream_.Write(buffer, offset, count); } break;
                }
            }

            void CopyAndEncrypt(byte[] buffer, int offset, int count)
            {
                const int CopyBufferSize = 4096;
                byte[] localBuffer = new byte[CopyBufferSize];
                while (count > 0)
                {
                    int bufferCount = (count < CopyBufferSize) ? count : CopyBufferSize;
                    Sys.Array.Copy(buffer, offset, localBuffer, 0, bufferCount);
                    EncryptBlock(localBuffer, 0, bufferCount);
                    baseOutputStream_.Write(localBuffer, 0, bufferCount);
                    count -= bufferCount;
                    offset += bufferCount;
                }
            }

            public override void Finish()
            {
                if (entries == null) { return; }
                if (curEntry != null) { CloseEntry(); }
                long numEntries = entries.Count;
                long sizeEntries = 0;
                foreach (ZipEntry entry in entries)
                {
                    WriteLeInt(ZipConstants.CentralHeaderSignature);
                    WriteLeShort(ZipConstants.VersionMadeBy);
                    WriteLeShort(entry.Version);
                    WriteLeShort(entry.Flags);
                    WriteLeShort((short)entry.CompressionMethodForHeader);
                    WriteLeInt((int)entry.DosTime);
                    WriteLeInt((int)entry.Crc);
                    if (entry.IsZip64Forced() || (entry.CompressedSize >= uint.MaxValue)) { WriteLeInt(-1); } else { WriteLeInt((int)entry.CompressedSize); }
                    if (entry.IsZip64Forced() || (entry.Size >= uint.MaxValue)) { WriteLeInt(-1); } else { WriteLeInt((int)entry.Size); }
                    byte[] name = ZipConstants.ConvertToArray(entry.Flags, entry.Name);
                    if (name.Length > 0xffff) { throw new Sys.ApplicationException("Name too long."); }
                    ZipExtraData ed = new ZipExtraData(entry.ExtraData);
                    if (entry.CentralHeaderRequiresZip64)
                    {
                        ed.StartNewEntry();
                        if (entry.IsZip64Forced() || (entry.Size >= 0xffffffff)) { ed.AddLeLong(entry.Size); }
                        if (entry.IsZip64Forced() || (entry.CompressedSize >= 0xffffffff)) { ed.AddLeLong(entry.CompressedSize); }
                        if (entry.Offset >= 0xffffffff) { ed.AddLeLong(entry.Offset); }
                        ed.AddNewEntry(1);
                    } else { ed.Delete(1); }
                    if (entry.AESKeySize > 0) { AddExtraDataAES(entry, ed); }
                    byte[] extra = ed.GetEntryData();
                    byte[] entryComment = (entry.Comment != null) ? ZipConstants.ConvertToArray(entry.Flags, entry.Comment) : new byte[0];
                    if (entryComment.Length > 0xffff) { throw new Sys.ApplicationException("Comment too long."); }
                    WriteLeShort(name.Length);
                    WriteLeShort(extra.Length);
                    WriteLeShort(entryComment.Length);
                    WriteLeShort(0);
                    WriteLeShort(0);
                    if (entry.ExternalFileAttributes != -1) { WriteLeInt(entry.ExternalFileAttributes); } else { if (entry.IsDirectory) { WriteLeInt(16); } else { WriteLeInt(0); } }
                    if (entry.Offset >= uint.MaxValue) { WriteLeInt(-1); } else { WriteLeInt((int)entry.Offset); }
                    if (name.Length > 0) { baseOutputStream_.Write(name, 0, name.Length); }
                    if (extra.Length > 0) { baseOutputStream_.Write(extra, 0, extra.Length); }
                    if (entryComment.Length > 0) { baseOutputStream_.Write(entryComment, 0, entryComment.Length); }
                    sizeEntries += ZipConstants.CentralHeaderBaseSize + name.Length + extra.Length + entryComment.Length;
                }
                using (ZipHelperStream zhs = new ZipHelperStream(baseOutputStream_)) { zhs.WriteEndOfCentralDirectory(numEntries, sizeEntries, offset, zipComment); }
                entries = null;
            }

            private SysColl.ArrayList entries = new SysColl.ArrayList();
            private ZIPLib.Checksums.Crc32 crc = new ZIPLib.Checksums.Crc32();
            private ZipEntry curEntry;
            private int defaultCompressionLevel = ZIPLib.Compression.Deflater.DEFAULT_COMPRESSION;
            private CompressionMethod curMethod = CompressionMethod.Deflated;
            private long size;
            private long offset;
            private byte[] zipComment = new byte[0];
            private bool patchEntryHeader;
            private long crcPatchPos = -1;
            private long sizePatchPos = -1;
            private UseZip64 useZip64_ = UseZip64.Dynamic;
        }

        public class ZipInputStream : ZIPLib.Compression.Streams.InflaterInputStream
        {
            private delegate int ReadDataHandler(byte[] b, int offset, int length);
            private ReadDataHandler internalReader;
            private ZIPLib.Checksums.Crc32 crc = new ZIPLib.Checksums.Crc32();
            private ZipEntry entry;
            private long size;
            private int method;
            private int flags;
            private string password;
            public ZipInputStream(Sys.IO.Stream baseInputStream) : base(baseInputStream, new ZIPLib.Compression.Inflater(true)) { internalReader = new ReadDataHandler(ReadingNotAvailable); }
            public ZipInputStream(Sys.IO.Stream baseInputStream, int bufferSize) : base(baseInputStream, new ZIPLib.Compression.Inflater(true), bufferSize) { internalReader = new ReadDataHandler(ReadingNotAvailable); }
            public string Password { get { return password; } set { password = value; } }
            public bool CanDecompressEntry { get { return (entry != null) && entry.CanDecompress; } }

            public ZipEntry GetNextEntry()
            {
                if (crc == null) { throw new Sys.InvalidOperationException("Closed."); }
                if (entry != null) { CloseEntry(); }
                int header = inputBuffer.ReadLeInt();
                if (header == ZipConstants.CentralHeaderSignature || header == ZipConstants.EndOfCentralDirectorySignature || header == ZipConstants.CentralHeaderDigitalSignature || header == ZipConstants.ArchiveExtraDataSignature || header == ZipConstants.Zip64CentralFileHeaderSignature)
                {
                    Close();
                    return null;
                }
                if ((header == ZipConstants.SpanningTempSignature) || (header == ZipConstants.SpanningSignature)) { header = inputBuffer.ReadLeInt(); }
                if (header != ZipConstants.LocalHeaderSignature) { throw new Sys.ApplicationException("Wrong Local header signature: 0x" + string.Format("{0:X}", header)); }
                short versionRequiredToExtract = (short)inputBuffer.ReadLeShort();
                flags = inputBuffer.ReadLeShort();
                method = inputBuffer.ReadLeShort();
                uint dostime = (uint)inputBuffer.ReadLeInt();
                int crc2 = inputBuffer.ReadLeInt();
                csize = inputBuffer.ReadLeInt();
                size = inputBuffer.ReadLeInt();
                int nameLen = inputBuffer.ReadLeShort();
                int extraLen = inputBuffer.ReadLeShort();
                bool isCrypted = (flags & 1) == 1;
                byte[] buffer = new byte[nameLen];
                inputBuffer.ReadRawBuffer(buffer);
                string name = ZipConstants.ConvertToStringExt(flags, buffer);
                entry = new ZipEntry(name, versionRequiredToExtract);
                entry.Flags = flags;
                entry.CompressionMethod = (CompressionMethod)method;
                if ((flags & 8) == 0)
                {
                    entry.Crc = crc2 & 0xFFFFFFFFL;
                    entry.Size = size & 0xFFFFFFFFL;
                    entry.CompressedSize = csize & 0xFFFFFFFFL;
                    entry.CryptoCheckValue = (byte)((crc2 >> 24) & 0xff);
                }
                else
                {
                    if (crc2 != 0) { entry.Crc = crc2 & 0xFFFFFFFFL; }
                    if (size != 0) { entry.Size = size & 0xFFFFFFFFL; }
                    if (csize != 0) { entry.CompressedSize = csize & 0xFFFFFFFFL; }
                    entry.CryptoCheckValue = (byte)((dostime >> 8) & 0xff);
                }
                entry.DosTime = dostime;
                if (extraLen > 0)
                {
                    byte[] extra = new byte[extraLen];
                    inputBuffer.ReadRawBuffer(extra);
                    entry.ExtraData = extra;
                }
                entry.ProcessExtraData(true);
                if (entry.CompressedSize >= 0) { csize = entry.CompressedSize; }
                if (entry.Size >= 0) { size = entry.Size; }
                if (method == (int)CompressionMethod.Stored && (!isCrypted && csize != size || (isCrypted && csize - ZipConstants.CryptoHeaderSize != size))) { throw new Sys.ApplicationException("Stored, but compressed != uncompressed"); }
                if (entry.IsCompressionMethodSupported()) { internalReader = new ReadDataHandler(InitialRead); } else { internalReader = new ReadDataHandler(ReadingNotSupported); }
                return entry;
            }

            private void ReadDataDescriptor()
            {
                if (inputBuffer.ReadLeInt() != ZipConstants.DataDescriptorSignature) { throw new Sys.ApplicationException("Data descriptor signature not found"); }
                entry.Crc = inputBuffer.ReadLeInt() & 0xFFFFFFFFL;
                if (entry.LocalHeaderRequiresZip64)
                {
                    csize = inputBuffer.ReadLeLong();
                    size = inputBuffer.ReadLeLong();
                }
                else
                {
                    csize = inputBuffer.ReadLeInt();
                    size = inputBuffer.ReadLeInt();
                }
                entry.CompressedSize = csize;
                entry.Size = size;
            }

            private void CompleteCloseEntry(bool testCrc)
            {
                StopDecrypting();
                if ((flags & 8) != 0) { ReadDataDescriptor(); }
                size = 0;
                if (testCrc && ((crc.Value & 0xFFFFFFFFL) != entry.Crc) && (entry.Crc != -1)) { throw new Sys.ApplicationException("CRC mismatch"); }
                crc.Reset();
                if (method == (int)CompressionMethod.Deflated) { inf.Reset(); }
                entry = null;
            }

            public void CloseEntry()
            {
                if (crc == null) { throw new Sys.InvalidOperationException("Closed"); }
                if (entry == null) { return; }
                if (method == (int)CompressionMethod.Deflated)
                {
                    if ((flags & 8) != 0)
                    {
                        byte[] tmp = new byte[4096];
                        while (Read(tmp, 0, tmp.Length) > 0) { /* NOTHING */ }
                        return;
                    }
                    csize -= inf.TotalIn;
                    inputBuffer.Available += inf.RemainingInput;
                }
                if ((inputBuffer.Available > csize) && (csize >= 0)) { inputBuffer.Available = (int)((long)inputBuffer.Available - csize); }
                else
                {
                    csize -= inputBuffer.Available;
                    inputBuffer.Available = 0;
                    while (csize != 0)
                    {
                        long skipped = base.Skip(csize);
                        if (skipped <= 0) { throw new Sys.ApplicationException("Zip archive ends early."); }
                        csize -= skipped;
                    }
                }
                CompleteCloseEntry(false);
            }

            public override int Available { get { return entry != null ? 1 : 0; } }
            public override long Length { get { if (entry != null) { if (entry.Size >= 0) { return entry.Size; } else { throw new Sys.ApplicationException("Length not available for the current entry"); } } else { throw new Sys.InvalidOperationException("No current entry"); } } }

            public override int ReadByte()
            {
                byte[] b = new byte[1];
                if (Read(b, 0, 1) <= 0) { return -1; }
                return b[0] & 0xff;
            }

            private int ReadingNotAvailable(byte[] destination, int offset, int count) { throw new Sys.InvalidOperationException("Unable to read from this stream"); }
            private int ReadingNotSupported(byte[] destination, int offset, int count) { throw new Sys.ApplicationException("The compression method for this entry is not supported"); }

            private int InitialRead(byte[] destination, int offset, int count)
            {
                if (!CanDecompressEntry) { throw new Sys.ApplicationException("Library cannot extract this entry. Version required is (" + entry.Version.ToString() + ")"); }
                if (entry.IsCrypted)
                {
                    if (password == null) { throw new Sys.ApplicationException("No password set."); }
                    ZIPLib.Encryption.PkzipClassicManaged managed = new ZIPLib.Encryption.PkzipClassicManaged();
                    byte[] key = ZIPLib.Encryption.PkzipClassic.GenerateKeys(ZipConstants.ConvertToArray(password));
                    inputBuffer.CryptoTransform = managed.CreateDecryptor(key, null);
                    byte[] cryptbuffer = new byte[ZipConstants.CryptoHeaderSize];
                    inputBuffer.ReadClearTextBuffer(cryptbuffer, 0, ZipConstants.CryptoHeaderSize);
                    if (cryptbuffer[ZipConstants.CryptoHeaderSize - 1] != entry.CryptoCheckValue) { throw new Sys.ApplicationException("Invalid password"); }
                    if (csize >= ZipConstants.CryptoHeaderSize) { csize -= ZipConstants.CryptoHeaderSize; }
                    else if ((entry.Flags & (int)GeneralBitFlags.Descriptor) == 0) { throw new Sys.ApplicationException(string.Format("Entry compressed size {0} too small for encryption", csize)); }
                } else { inputBuffer.CryptoTransform = null; }
                if ((csize > 0) || ((flags & (int)GeneralBitFlags.Descriptor) != 0))
                {
                    if ((method == (int)CompressionMethod.Deflated) && (inputBuffer.Available > 0)) { inputBuffer.SetInflaterInput(inf); }
                    internalReader = new ReadDataHandler(BodyRead);
                    return BodyRead(destination, offset, count);
                }
                else
                {
                    internalReader = new ReadDataHandler(ReadingNotAvailable);
                    return 0;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null) { throw new Sys.ArgumentNullException("buffer"); }
                if (offset < 0) { throw new Sys.ArgumentOutOfRangeException("offset", "Cannot be negative"); }
                if (count < 0) { throw new Sys.ArgumentOutOfRangeException("count", "Cannot be negative"); }
                if ((buffer.Length - offset) < count) { throw new Sys.ArgumentException("Invalid offset/count combination"); }
                return internalReader(buffer, offset, count);
            }

            private int BodyRead(byte[] buffer, int offset, int count)
            {
                if (crc == null) { throw new Sys.InvalidOperationException("Closed"); }
                if ((entry == null) || (count <= 0)) { return 0; }
                if (offset + count > buffer.Length) { throw new Sys.ArgumentException("Offset + count exceeds buffer size"); }
                bool finished = false;
                switch (method)
                {
                    case (int)CompressionMethod.Deflated:
                        count = base.Read(buffer, offset, count);
                        if (count <= 0)
                        {
                            if (!inf.IsFinished) { throw new Sys.ApplicationException("Inflater not finished!"); }
                            inputBuffer.Available = inf.RemainingInput;
                            if ((flags & 8) == 0 && (inf.TotalIn != csize && csize != 0xFFFFFFFF && csize != -1 || inf.TotalOut != size)) { throw new Sys.ApplicationException("Size mismatch: " + csize + ";" + size + " <-> " + inf.TotalIn + ";" + inf.TotalOut); }
                            inf.Reset();
                            finished = true;
                        }
                        break;
                    case (int)CompressionMethod.Stored:
                        if ((count > csize) && (csize >= 0)) { count = (int)csize; }
                        if (count > 0)
                        {
                            count = inputBuffer.ReadClearTextBuffer(buffer, offset, count);
                            if (count > 0)
                            {
                                csize -= count;
                                size -= count;
                            }
                        }
                        if (csize == 0) { finished = true; }
                        else if (count < 0) { throw new Sys.ApplicationException("EOF in stored block"); }
                        break;
                }
                if (count > 0) { crc.Update(buffer, offset, count); }
                if (finished) { CompleteCloseEntry(true); }
                return count;
            }

            public override void Close()
            {
                internalReader = new ReadDataHandler(ReadingNotAvailable);
                crc = null;
                entry = null;
                base.Close();
            }
        }

        public class ZipNameTransform : ZIPLib.Internal.INameTransform
        {
            public ZipNameTransform() { /* NOTHING */ }
            public ZipNameTransform(string trimPrefix) { TrimPrefix = trimPrefix; }

            static ZipNameTransform()
            {
                char[] invalidPathChars;
                invalidPathChars = Sys.IO.Path.GetInvalidPathChars();
                int howMany = invalidPathChars.Length + 2;
                InvalidEntryCharsRelaxed = new char[howMany];
                Sys.Array.Copy(invalidPathChars, 0, InvalidEntryCharsRelaxed, 0, invalidPathChars.Length);
                InvalidEntryCharsRelaxed[howMany - 1] = '*';
                InvalidEntryCharsRelaxed[howMany - 2] = '?';
                howMany = invalidPathChars.Length + 4;
                InvalidEntryChars = new char[howMany];
                Sys.Array.Copy(invalidPathChars, 0, InvalidEntryChars, 0, invalidPathChars.Length);
                InvalidEntryChars[howMany - 1] = ':';
                InvalidEntryChars[howMany - 2] = '\\';
                InvalidEntryChars[howMany - 3] = '*';
                InvalidEntryChars[howMany - 4] = '?';
            }

            public string TransformDirectory(string name)
            {
                name = TransformFile(name);
                if (name.Length > 0) { if (!name.EndsWith("/")) { name += "/"; } } else { throw new Sys.ApplicationException("Cannot have an empty directory name"); }
                return name;
            }

            /// <summary>
            /// Transform a windows file name according to the Zip file naming conventions.
            /// </summary>
            /// <param name="name">The file name to transform.</param>
            /// <returns>The transformed name.</returns>
            public string TransformFile(string name)
            {
                if (name != null)
                {
                    string lowerName = name.ToLower();
                    if ((trimPrefix_ != null) && (lowerName.IndexOf(trimPrefix_) == 0)) { name = name.Substring(trimPrefix_.Length); }
                    name = name.Replace(@"\", "/");
                    name = ZIPLib.Internal.WindowsPathUtils.DropPathRoot(name);
                    while ((name.Length > 0) && (name[0] == '/')) { name = name.Remove(0, 1); }
                    while ((name.Length > 0) && (name[name.Length - 1] == '/')) { name = name.Remove(name.Length - 1, 1); }
                    int index = name.IndexOf("//");
                    while (index >= 0)
                    {
                        name = name.Remove(index, 1);
                        index = name.IndexOf("//");
                    }
                    name = MakeValidName(name, '_');
                } else { name = string.Empty; }
                return name;
            }

            public string TrimPrefix
            {
                get { return trimPrefix_; }
                set
                {
                    trimPrefix_ = value;
                    if (trimPrefix_ != null) { trimPrefix_ = trimPrefix_.ToLower(); }
                }
            }

            private static string MakeValidName(string name, char replacement)
            {
                int index = name.IndexOfAny(InvalidEntryChars);
                if (index >= 0)
                {
                    SysTxt.StringBuilder builder = new SysTxt.StringBuilder(name);
                    while (index >= 0)
                    {
                        builder[index] = replacement;
                        if (index >= name.Length) { index = -1; } else { index = name.IndexOfAny(InvalidEntryChars, index + 1); }
                    }
                    name = builder.ToString();
                }
                if (name.Length > 0xffff) { throw new Sys.IO.PathTooLongException(); }
                return name;
            }

            public static bool IsValidName(string name, bool relaxed)
            {
                bool result = (name != null);
                if (result) { if (relaxed) { result = name.IndexOfAny(InvalidEntryCharsRelaxed) < 0; } else { result = (name.IndexOfAny(InvalidEntryChars) < 0) && (name.IndexOf('/') != 0); } }
                return result;
            }

            public static bool IsValidName(string name) { return (name != null) && (name.IndexOfAny(InvalidEntryChars) < 0) && (name.IndexOf('/') != 0); }
            private string trimPrefix_;
            private static readonly char[] InvalidEntryChars;
            private static readonly char[] InvalidEntryCharsRelaxed;
        }

        public class DescriptorData
        {
            public long CompressedSize { get { return compressedSize; } set { compressedSize = value; } }
            public long Size { get { return size; } set { size = value; } }
            public long Crc { get { return crc; } set { crc = (value & 0xffffffff); } }
            private long size;
            private long compressedSize;
            private long crc;
        }

        internal class EntryPatchData
        {
            public long SizePatchOffset { get { return sizePatchOffset_; } set { sizePatchOffset_ = value; } }
            public long CrcPatchOffset { get { return crcPatchOffset_; } set { crcPatchOffset_ = value; } }
            private long sizePatchOffset_;
            private long crcPatchOffset_;
        }

        internal class ZipHelperStream : Sys.IO.Stream
        {
            public ZipHelperStream(string name)
            {
                stream_ = new Sys.IO.FileStream(name, Sys.IO.FileMode.Open, Sys.IO.FileAccess.ReadWrite);
                isOwner_ = true;
            }

            public ZipHelperStream(Sys.IO.Stream stream) { stream_ = stream; }
            public bool IsStreamOwner { get { return isOwner_; } set { isOwner_ = value; } }
            public override bool CanRead { get { return stream_.CanRead; } }
            public override bool CanSeek { get { return stream_.CanSeek; } }
            public override bool CanTimeout { get { return stream_.CanTimeout; } }
            public override long Length { get { return stream_.Length; } }
            public override long Position { get { return stream_.Position; } set { stream_.Position = value; } }
            public override bool CanWrite { get { return stream_.CanWrite; } }
            public override void Flush() { stream_.Flush(); }
            public override long Seek(long offset, Sys.IO.SeekOrigin origin) { return stream_.Seek(offset, origin); }
            public override void SetLength(long value) { stream_.SetLength(value); }
            public override int Read(byte[] buffer, int offset, int count) { return stream_.Read(buffer, offset, count); }
            public override void Write(byte[] buffer, int offset, int count) { stream_.Write(buffer, offset, count); }

            override public void Close()
            {
                Sys.IO.Stream toClose = stream_;
                stream_ = null;
                if (isOwner_ && (toClose != null))
                {
                    isOwner_ = false;
                    toClose.Close();
                }
            }

            private void WriteLocalHeader(ZipEntry entry, EntryPatchData patchData)
            {
                CompressionMethod method = entry.CompressionMethod;
                bool headerInfoAvailable = true; // How to get this?
                bool patchEntryHeader = false;
                WriteLEInt(ZipConstants.LocalHeaderSignature);
                WriteLEShort(entry.Version);
                WriteLEShort(entry.Flags);
                WriteLEShort((byte)method);
                WriteLEInt((int)entry.DosTime);
                if (headerInfoAvailable == true)
                {
                    WriteLEInt((int)entry.Crc);
                    if (entry.LocalHeaderRequiresZip64)
                    {
                        WriteLEInt(-1);
                        WriteLEInt(-1);
                    }
                    else
                    {
                        WriteLEInt(entry.IsCrypted ? (int)entry.CompressedSize + ZipConstants.CryptoHeaderSize : (int)entry.CompressedSize);
                        WriteLEInt((int)entry.Size);
                    }
                }
                else
                {
                    if (patchData != null) { patchData.CrcPatchOffset = stream_.Position; }
                    WriteLEInt(0);
                    if (patchData != null) { patchData.SizePatchOffset = stream_.Position; }
                    if (entry.LocalHeaderRequiresZip64 && patchEntryHeader)
                    {
                        WriteLEInt(-1);
                        WriteLEInt(-1);
                    }
                    else
                    {
                        WriteLEInt(0);
                        WriteLEInt(0);
                    }
                }
                byte[] name = ZipConstants.ConvertToArray(entry.Flags, entry.Name);
                if (name.Length > 0xFFFF) { throw new Sys.ApplicationException("Entry name too long."); }
                ZipExtraData ed = new ZipExtraData(entry.ExtraData);
                if (entry.LocalHeaderRequiresZip64 && (headerInfoAvailable || patchEntryHeader))
                {
                    ed.StartNewEntry();
                    if (headerInfoAvailable)
                    {
                        ed.AddLeLong(entry.Size);
                        ed.AddLeLong(entry.CompressedSize);
                    }
                    else
                    {
                        ed.AddLeLong(-1);
                        ed.AddLeLong(-1);
                    }
                    ed.AddNewEntry(1);
                    if (!ed.Find(1)) { throw new Sys.ApplicationException("Internal error cant find extra data"); }
                    if (patchData != null) { patchData.SizePatchOffset = ed.CurrentReadIndex; }
                } else { ed.Delete(1); }
                byte[] extra = ed.GetEntryData();
                WriteLEShort(name.Length);
                WriteLEShort(extra.Length);
                if (name.Length > 0) { stream_.Write(name, 0, name.Length); }
                if (entry.LocalHeaderRequiresZip64 && patchEntryHeader) { patchData.SizePatchOffset += stream_.Position; }
                if (extra.Length > 0) { stream_.Write(extra, 0, extra.Length); }
            }

            public long LocateBlockWithSignature(int signature, long endLocation, int minimumBlockSize, int maximumVariableData)
            {
                long pos = endLocation - minimumBlockSize;
                if (pos < 0) { return -1; }
                long giveUpMarker = SysMath.Max(pos - maximumVariableData, 0);
                do
                {
                    if (pos < giveUpMarker) { return -1; }
                    Seek(pos--, Sys.IO.SeekOrigin.Begin);
                } while (ReadLEInt() != signature);
                return Position;
            }

            public void WriteZip64EndOfCentralDirectory(long noOfEntries, long sizeEntries, long centralDirOffset)
            {
                long centralSignatureOffset = stream_.Position;
                WriteLEInt(ZipConstants.Zip64CentralFileHeaderSignature);
                WriteLELong(44);
                WriteLEShort(ZipConstants.VersionMadeBy);
                WriteLEShort(ZipConstants.VersionZip64);
                WriteLEInt(0);
                WriteLEInt(0);
                WriteLELong(noOfEntries);
                WriteLELong(noOfEntries);
                WriteLELong(sizeEntries);
                WriteLELong(centralDirOffset);
                WriteLEInt(ZipConstants.Zip64CentralDirLocatorSignature);
                WriteLEInt(0);
                WriteLELong(centralSignatureOffset);
                WriteLEInt(1);
            }

            public void WriteEndOfCentralDirectory(long noOfEntries, long sizeEntries, long startOfCentralDirectory, byte[] comment)
            {
                if ((noOfEntries >= 0xffff) || (startOfCentralDirectory >= 0xffffffff) || (sizeEntries >= 0xffffffff)) { WriteZip64EndOfCentralDirectory(noOfEntries, sizeEntries, startOfCentralDirectory); }
                WriteLEInt(ZipConstants.EndOfCentralDirectorySignature);
                WriteLEShort(0);
                WriteLEShort(0);
                if (noOfEntries >= 0xffff)
                {
                    WriteLEUshort(0xffff);
                    WriteLEUshort(0xffff);
                }
                else
                {
                    WriteLEShort((short)noOfEntries);
                    WriteLEShort((short)noOfEntries);
                }
                if (sizeEntries >= 0xffffffff) { WriteLEUint(0xffffffff); } else { WriteLEInt((int)sizeEntries); }
                if (startOfCentralDirectory >= 0xffffffff) { WriteLEUint(0xffffffff); } else { WriteLEInt((int)startOfCentralDirectory); }
                int commentLength = (comment != null) ? comment.Length : 0;
                if (commentLength > 0xffff) { throw new Sys.ApplicationException(string.Format("Comment length({0}) is too long can only be 64K", commentLength)); }
                WriteLEShort(commentLength);
                if (commentLength > 0) { Write(comment, 0, comment.Length); }
            }

            public int ReadLEShort()
            {
                int byteValue1 = stream_.ReadByte();
                if (byteValue1 < 0) { throw new Sys.IO.EndOfStreamException(); }
                int byteValue2 = stream_.ReadByte();
                if (byteValue2 < 0) { throw new Sys.IO.EndOfStreamException(); }
                return byteValue1 | (byteValue2 << 8);
            }

            public int ReadLEInt() { return ReadLEShort() | (ReadLEShort() << 16); }
            public long ReadLELong() { return (uint)ReadLEInt() | ((long)ReadLEInt() << 32); }

            public void WriteLEShort(int value)
            {
                stream_.WriteByte((byte)(value & 0xff));
                stream_.WriteByte((byte)((value >> 8) & 0xff));
            }

            public void WriteLEUshort(ushort value)
            {
                stream_.WriteByte((byte)(value & 0xff));
                stream_.WriteByte((byte)(value >> 8));
            }

            public void WriteLEInt(int value)
            {
                WriteLEShort(value);
                WriteLEShort(value >> 16);
            }

            public void WriteLEUint(uint value)
            {
                WriteLEUshort((ushort)(value & 0xffff));
                WriteLEUshort((ushort)(value >> 16));
            }

            public void WriteLELong(long value)
            {
                WriteLEInt((int)value);
                WriteLEInt((int)(value >> 32));
            }

            public void WriteLEUlong(ulong value)
            {
                WriteLEUint((uint)(value & 0xffffffff));
                WriteLEUint((uint)(value >> 32));
            }

            public int WriteDataDescriptor(ZipEntry entry)
            {
                if (entry == null) { throw new Sys.ArgumentNullException("entry"); }
                int result = 0;
                if ((entry.Flags & (int)GeneralBitFlags.Descriptor) != 0)
                {
                    WriteLEInt(ZipConstants.DataDescriptorSignature);
                    WriteLEInt(unchecked((int)(entry.Crc)));
                    result += 8;
                    if (entry.LocalHeaderRequiresZip64)
                    {
                        WriteLELong(entry.CompressedSize);
                        WriteLELong(entry.Size);
                        result += 16;
                    }
                    else
                    {
                        WriteLEInt((int)entry.CompressedSize);
                        WriteLEInt((int)entry.Size);
                        result += 8;
                    }
                }
                return result;
            }

            public void ReadDataDescriptor(bool zip64, DescriptorData data)
            {
                int intValue = ReadLEInt();
                if (intValue != ZipConstants.DataDescriptorSignature) { throw new Sys.ApplicationException("Data descriptor signature not found"); }
                data.Crc = ReadLEInt();
                if (zip64)
                {
                    data.CompressedSize = ReadLELong();
                    data.Size = ReadLELong();
                }
                else
                {
                    data.CompressedSize = ReadLEInt();
                    data.Size = ReadLEInt();
                }
            }

            private bool isOwner_;
            private Sys.IO.Stream stream_;
        }

        public interface ITaggedData
        {
            short TagID { get; }
            void SetData(byte[] data, int offset, int count);
            byte[] GetData();
        }

        public class RawTaggedData : ZIPLib.ITaggedData
        {
            public RawTaggedData(short tag) { _tag = tag; }
            public short TagID { get { return _tag; } set { _tag = value; } }

            public void SetData(byte[] data, int offset, int count)
            {
                if (data == null) { throw new Sys.ArgumentNullException("data"); }
                _data = new byte[count];
                Sys.Array.Copy(data, offset, _data, 0, count);
            }

            public byte[] GetData() { return _data; }
            public byte[] Data { get { return _data; } set { _data = value; } }
            private short _tag;
            private byte[] _data;
        }

        public class ExtendedUnixData : ZIPLib.ITaggedData
        {
            [Sys.Flags] public enum Flags : byte
            {
                ModificationTime = 0x01,
                AccessTime = 0x02,
                CreateTime = 0x04
            }

            public short TagID { get { return 0x5455; } }

            public void SetData(byte[] data, int index, int count)
            {
                using (Sys.IO.MemoryStream ms = new Sys.IO.MemoryStream(data, index, count, false))
                using (ZipHelperStream helperStream = new ZipHelperStream(ms))
                {
                    _flags = (Flags)helperStream.ReadByte();
                    if (((_flags & Flags.ModificationTime) != 0) && (count >= 5))
                    {
                        int iTime = helperStream.ReadLEInt();
                        _modificationTime = (new SysDate(1970, 1, 1, 0, 0, 0).ToUniversalTime() + new Sys.TimeSpan(0, 0, 0, iTime, 0)).ToLocalTime();
                    }
                    if ((_flags & Flags.AccessTime) != 0)
                    {
                        int iTime = helperStream.ReadLEInt();
                        _lastAccessTime = (new SysDate(1970, 1, 1, 0, 0, 0).ToUniversalTime() + new Sys.TimeSpan(0, 0, 0, iTime, 0)).ToLocalTime();
                    }
                    if ((_flags & Flags.CreateTime) != 0)
                    {
                        int iTime = helperStream.ReadLEInt();
                        _createTime = (new SysDate(1970, 1, 1, 0, 0, 0).ToUniversalTime() + new Sys.TimeSpan(0, 0, 0, iTime, 0)).ToLocalTime();
                    }
                }
            }

            public byte[] GetData()
            {
                using (Sys.IO.MemoryStream ms = new Sys.IO.MemoryStream())
                using (ZipHelperStream helperStream = new ZipHelperStream(ms))
                {
                    helperStream.IsStreamOwner = false;
                    helperStream.WriteByte((byte)_flags);     // Flags
                    if ((_flags & Flags.ModificationTime) != 0)
                    {
                        Sys.TimeSpan span = _modificationTime.ToUniversalTime() - new SysDate(1970, 1, 1, 0, 0, 0).ToUniversalTime();
                        int seconds = (int)span.TotalSeconds;
                        helperStream.WriteLEInt(seconds);
                    }
                    if ((_flags & Flags.AccessTime) != 0)
                    {
                        Sys.TimeSpan span = _lastAccessTime.ToUniversalTime() - new SysDate(1970, 1, 1, 0, 0, 0).ToUniversalTime();
                        int seconds = (int)span.TotalSeconds;
                        helperStream.WriteLEInt(seconds);
                    }
                    if ((_flags & Flags.CreateTime) != 0)
                    {
                        Sys.TimeSpan span = _createTime.ToUniversalTime() - new SysDate(1970, 1, 1, 0, 0, 0).ToUniversalTime();
                        int seconds = (int)span.TotalSeconds;
                        helperStream.WriteLEInt(seconds);
                    }
                    return ms.ToArray();
                }
            }

            public static bool IsValidValue(SysDate value) { return ((value >= new SysDate(1901, 12, 13, 20, 45, 52)) || (value <= new SysDate(2038, 1, 19, 03, 14, 07))); }

            public SysDate ModificationTime
            {
                get { return _modificationTime; }
                set
                {
                    if (!IsValidValue(value)) { throw new Sys.ArgumentOutOfRangeException("value"); }
                    _flags |= Flags.ModificationTime;
                    _modificationTime = value;
                }
            }

            public SysDate AccessTime
            {
                get { return _lastAccessTime; }
                set
                {
                    if (!IsValidValue(value)) { throw new Sys.ArgumentOutOfRangeException("value"); }
                    _flags |= Flags.AccessTime;
                    _lastAccessTime = value;
                }
            }

            public SysDate CreateTime
            {
                get { return _createTime; }
                set
                {
                    if (!IsValidValue(value)) { throw new Sys.ArgumentOutOfRangeException("value"); }
                    _flags |= Flags.CreateTime;
                    _createTime = value;
                }
            }

            public ZIPLib.ExtendedUnixData.Flags Include
            {
                get { return _flags; }
                set { _flags = value; }
            }

            private ZIPLib.ExtendedUnixData.Flags _flags;
            private SysDate _modificationTime = new SysDate(1970, 1, 1);
            private SysDate _lastAccessTime = new SysDate(1970, 1, 1);
            private SysDate _createTime = new SysDate(1970, 1, 1);
        }

        public class NTTaggedData : ZIPLib.ITaggedData
        {
            /// <summary>
            /// Get the ID for this tagged data value.
            /// </summary>
            public short TagID
            {
                get { return 10; }
            }

            /// <summary>
            /// Set the data from the raw values provided.
            /// </summary>
            /// <param name="data">The raw data to extract values from.</param>
            /// <param name="index">The index to start extracting values from.</param>
            /// <param name="count">The number of bytes available.</param>
            public void SetData(byte[] data, int index, int count)
            {
                using (Sys.IO.MemoryStream ms = new Sys.IO.MemoryStream(data, index, count, false))
                using (ZipHelperStream helperStream = new ZipHelperStream(ms))
                {
                    helperStream.ReadLEInt(); // Reserved
                    while (helperStream.Position < helperStream.Length)
                    {
                        int ntfsTag = helperStream.ReadLEShort();
                        int ntfsLength = helperStream.ReadLEShort();
                        if (ntfsTag == 1)
                        {
                            if (ntfsLength >= 24)
                            {
                                long lastModificationTicks = helperStream.ReadLELong();
                                _lastModificationTime = SysDate.FromFileTime(lastModificationTicks);

                                long lastAccessTicks = helperStream.ReadLELong();
                                _lastAccessTime = SysDate.FromFileTime(lastAccessTicks);

                                long createTimeTicks = helperStream.ReadLELong();
                                _createTime = SysDate.FromFileTime(createTimeTicks);
                            }
                            break;
                        }
                        else
                        {
                            // An unknown NTFS tag so simply skip it.
                            helperStream.Seek(ntfsLength, Sys.IO.SeekOrigin.Current);
                        }
                    }
                }
            }

            /// <summary>
            /// Get the binary data representing this instance.
            /// </summary>
            /// <returns>The raw binary data representing this instance.</returns>
            public byte[] GetData()
            {
                using (Sys.IO.MemoryStream ms = new Sys.IO.MemoryStream())
                using (ZipHelperStream helperStream = new ZipHelperStream(ms))
                {
                    helperStream.IsStreamOwner = false;
                    helperStream.WriteLEInt(0);       // Reserved
                    helperStream.WriteLEShort(1);     // Tag
                    helperStream.WriteLEShort(24);    // Length = 3 x 8.
                    helperStream.WriteLELong(_lastModificationTime.ToFileTime());
                    helperStream.WriteLELong(_lastAccessTime.ToFileTime());
                    helperStream.WriteLELong(_createTime.ToFileTime());
                    return ms.ToArray();
                }
            }

            /// <summary>
            /// Test a <see cref="DateTime"> valuie to see if is valid and can be represented here.</see>
            /// </summary>
            /// <param name="value">The <see cref="DateTime">value</see> to test.</param>
            /// <returns>Returns true if the value is valid and can be represented; false if not.</returns>
            /// <remarks>
            /// NTFS filetimes are 64-bit unsigned integers, stored in Intel
            /// (least significant byte first) byte order. They determine the
            /// number of 1.0E-07 seconds (1/10th microseconds!) past WinNT "epoch",
            /// which is "01-Jan-1601 00:00:00 UTC". 28 May 60056 is the upper limit
            /// </remarks>
            public static bool IsValidValue(SysDate value)
            {
                bool result = true;
                try
                {
                    value.ToFileTimeUtc();
                }
                catch
                {
                    result = false;
                }
                return result;
            }

            /// <summary>
            /// Get/set the <see cref="DateTime">last modification time</see>.
            /// </summary>
            public SysDate LastModificationTime
            {
                get { return _lastModificationTime; }
                set
                {
                    if (!IsValidValue(value))
                    {
                        throw new Sys.ArgumentOutOfRangeException("value");
                    }
                    _lastModificationTime = value;
                }
            }

            /// <summary>
            /// Get /set the <see cref="DateTime">create time</see>
            /// </summary>
            public SysDate CreateTime
            {
                get { return _createTime; }
                set
                {
                    if (!IsValidValue(value))
                    {
                        throw new Sys.ArgumentOutOfRangeException("value");
                    }
                    _createTime = value;
                }
            }

            /// <summary>
            /// Get /set the <see cref="DateTime">last access time</see>.
            /// </summary>
            public SysDate LastAccessTime
            {
                get { return _lastAccessTime; }
                set
                {
                    if (!IsValidValue(value))
                    {
                        throw new Sys.ArgumentOutOfRangeException("value");
                    }
                    _lastAccessTime = value;
                }
            }

            SysDate _lastAccessTime = SysDate.FromFileTime(0);
            SysDate _lastModificationTime = SysDate.FromFileTime(0);
            SysDate _createTime = SysDate.FromFileTime(0);
        }

        public sealed class ZipExtraData : Sys.IDisposable
        {
            /// <summary>
            /// Initialise a default instance.
            /// </summary>
            public ZipExtraData()
            {
                Clear();
            }

            /// <summary>
            /// Initialise with known extra data.
            /// </summary>
            /// <param name="data">The extra data.</param>
            public ZipExtraData(byte[] data)
            {
                if (data == null)
                {
                    _data = new byte[0];
                }
                else
                {
                    _data = data;
                }
            }

            /// <summary>
            /// Get the raw extra data value
            /// </summary>
            /// <returns>Returns the raw byte[] extra data this instance represents.</returns>
            public byte[] GetEntryData()
            {
                if (Length > ushort.MaxValue)
                {
                    throw new Sys.ApplicationException("Data exceeds maximum length");
                }

                return (byte[])_data.Clone();
            }

            /// <summary>
            /// Clear the stored data.
            /// </summary>
            public void Clear()
            {
                if ((_data == null) || (_data.Length != 0))
                {
                    _data = new byte[0];
                }
            }

            /// <summary>
            /// Gets the current extra data length.
            /// </summary>
            public int Length
            {
                get { return _data.Length; }
            }

            /// <summary>
            /// Get a read-only <see cref="Stream"/> for the associated tag.
            /// </summary>
            /// <param name="tag">The tag to locate data for.</param>
            /// <returns>Returns a <see cref="Stream"/> containing tag data or null if no tag was found.</returns>
            public Sys.IO.Stream GetStreamForTag(int tag)
            {
                Sys.IO.Stream result = null;
                if (Find(tag))
                {
                    result = new Sys.IO.MemoryStream(_data, _index, _readValueLength, false);
                }
                return result;
            }

            /// <summary>
            /// Get the <see cref="ITaggedData">tagged data</see> for a tag.
            /// </summary>
            /// <param name="tag">The tag to search for.</param>
            /// <returns>Returns a <see cref="ITaggedData">tagged value</see> or null if none found.</returns>
            private ITaggedData GetData(short tag)
            {
                ITaggedData result = null;
                if (Find(tag))
                {
                    result = Create(tag, _data, _readValueStart, _readValueLength);
                }
                return result;
            }

            static ITaggedData Create(short tag, byte[] data, int offset, int count)
            {
                ITaggedData result = null;
                switch (tag)
                {
                    case 0x000A:
                        result = new NTTaggedData();
                        break;
                    case 0x5455:
                        result = new ExtendedUnixData();
                        break;
                    default:
                        result = new RawTaggedData(tag);
                        break;
                }
                result.SetData(data, offset, count);
                return result;
            }

            /// <summary>
            /// Get the length of the last value found by <see cref="Find"/>
            /// </summary>
            /// <remarks>This is only valid if <see cref="Find"/> has previously returned true.</remarks>
            public int ValueLength
            {
                get { return _readValueLength; }
            }

            /// <summary>
            /// Get the index for the current read value.
            /// </summary>
            /// <remarks>This is only valid if <see cref="Find"/> has previously returned true.
            /// Initially the result will be the index of the first byte of actual data.  The value is updated after calls to
            /// <see cref="ReadInt"/>, <see cref="ReadShort"/> and <see cref="ReadLong"/>. </remarks>
            public int CurrentReadIndex
            {
                get { return _index; }
            }

            /// <summary>
            /// Get the number of bytes remaining to be read for the current value;
            /// </summary>
            public int UnreadCount
            {
                get
                {
                    if ((_readValueStart > _data.Length) ||
                        (_readValueStart < 4))
                    {
                        throw new Sys.ApplicationException("Find must be called before calling a Read method");
                    }

                    return _readValueStart + _readValueLength - _index;
                }
            }

            /// <summary>
            /// Find an extra data value
            /// </summary>
            /// <param name="headerID">The identifier for the value to find.</param>
            /// <returns>Returns true if the value was found; false otherwise.</returns>
            public bool Find(int headerID)
            {
                _readValueStart = _data.Length;
                _readValueLength = 0;
                _index = 0;

                int localLength = _readValueStart;
                int localTag = headerID - 1;

                // Trailing bytes that cant make up an entry (as there arent enough
                // bytes for a tag and length) are ignored!
                while ((localTag != headerID) && (_index < _data.Length - 3))
                {
                    localTag = ReadShortInternal();
                    localLength = ReadShortInternal();
                    if (localTag != headerID)
                    {
                        _index += localLength;
                    }
                }

                bool result = (localTag == headerID) && ((_index + localLength) <= _data.Length);

                if (result)
                {
                    _readValueStart = _index;
                    _readValueLength = localLength;
                }

                return result;
            }

            /// <summary>
            /// Add a new entry to extra data.
            /// </summary>
            /// <param name="taggedData">The <see cref="ITaggedData"/> value to add.</param>
            public void AddEntry(ITaggedData taggedData)
            {
                if (taggedData == null)
                {
                    throw new Sys.ArgumentNullException("taggedData");
                }
                AddEntry(taggedData.TagID, taggedData.GetData());
            }

            /// <summary>
            /// Add a new entry to extra data
            /// </summary>
            /// <param name="headerID">The ID for this entry.</param>
            /// <param name="fieldData">The data to add.</param>
            /// <remarks>If the ID already exists its contents are replaced.</remarks>
            public void AddEntry(int headerID, byte[] fieldData)
            {
                if ((headerID > ushort.MaxValue) || (headerID < 0))
                {
                    throw new Sys.ArgumentOutOfRangeException("headerID");
                }

                int addLength = (fieldData == null) ? 0 : fieldData.Length;

                if (addLength > ushort.MaxValue)
                {
                    throw new Sys.ArgumentOutOfRangeException("fieldData", "exceeds maximum length");
                }

                // Test for new length before adjusting data.
                int newLength = _data.Length + addLength + 4;

                if (Find(headerID))
                {
                    newLength -= (ValueLength + 4);
                }

                if (newLength > ushort.MaxValue)
                {
                    throw new Sys.ApplicationException("Data exceeds maximum length");
                }

                Delete(headerID);

                byte[] newData = new byte[newLength];
                _data.CopyTo(newData, 0);
                int index = _data.Length;
                _data = newData;
                SetShort(ref index, headerID);
                SetShort(ref index, addLength);
                if (fieldData != null)
                {
                    fieldData.CopyTo(newData, index);
                }
            }

            /// <summary>
            /// Start adding a new entry.
            /// </summary>
            /// <remarks>Add data using <see cref="AddData(byte[])"/>, <see cref="AddLeShort"/>, <see cref="AddLeInt"/>, or <see cref="AddLeLong"/>.
            /// The new entry is completed and actually added by calling <see cref="AddNewEntry"/></remarks>
            /// <seealso cref="AddEntry(ITaggedData)"/>
            public void StartNewEntry()
            {
                _newEntry = new Sys.IO.MemoryStream();
            }

            /// <summary>
            /// Add entry data added since <see cref="StartNewEntry"/> using the ID passed.
            /// </summary>
            /// <param name="headerID">The identifier to use for this entry.</param>
            public void AddNewEntry(int headerID)
            {
                byte[] newData = _newEntry.ToArray();
                _newEntry = null;
                AddEntry(headerID, newData);
            }

            /// <summary>
            /// Add a byte of data to the pending new entry.
            /// </summary>
            /// <param name="data">The byte to add.</param>
            /// <seealso cref="StartNewEntry"/>
            public void AddData(byte data)
            {
                _newEntry.WriteByte(data);
            }

            /// <summary>
            /// Add data to a pending new entry.
            /// </summary>
            /// <param name="data">The data to add.</param>
            /// <seealso cref="StartNewEntry"/>
            public void AddData(byte[] data)
            {
                if (data == null)
                {
                    throw new Sys.ArgumentNullException("data");
                }

                _newEntry.Write(data, 0, data.Length);
            }

            /// <summary>
            /// Add a short value in little endian order to the pending new entry.
            /// </summary>
            /// <param name="toAdd">The data to add.</param>
            /// <seealso cref="StartNewEntry"/>
            public void AddLeShort(int toAdd)
            {
                unchecked
                {
                    _newEntry.WriteByte((byte)toAdd);
                    _newEntry.WriteByte((byte)(toAdd >> 8));
                }
            }

            /// <summary>
            /// Add an integer value in little endian order to the pending new entry.
            /// </summary>
            /// <param name="toAdd">The data to add.</param>
            /// <seealso cref="StartNewEntry"/>
            public void AddLeInt(int toAdd)
            {
                unchecked
                {
                    AddLeShort((short)toAdd);
                    AddLeShort((short)(toAdd >> 16));
                }
            }

            /// <summary>
            /// Add a long value in little endian order to the pending new entry.
            /// </summary>
            /// <param name="toAdd">The data to add.</param>
            /// <seealso cref="StartNewEntry"/>
            public void AddLeLong(long toAdd)
            {
                unchecked
                {
                    AddLeInt((int)(toAdd & 0xffffffff));
                    AddLeInt((int)(toAdd >> 32));
                }
            }

            /// <summary>
            /// Delete an extra data field.
            /// </summary>
            /// <param name="headerID">The identifier of the field to delete.</param>
            /// <returns>Returns true if the field was found and deleted.</returns>
            public bool Delete(int headerID)
            {
                bool result = false;

                if (Find(headerID))
                {
                    result = true;
                    int trueStart = _readValueStart - 4;

                    byte[] newData = new byte[_data.Length - (ValueLength + 4)];
                    Sys.Array.Copy(_data, 0, newData, 0, trueStart);

                    int trueEnd = trueStart + ValueLength + 4;
                    Sys.Array.Copy(_data, trueEnd, newData, trueStart, _data.Length - trueEnd);
                    _data = newData;
                }
                return result;
            }

            /// <summary>
            /// Read a long in little endian form from the last <see cref="Find">found</see> data value
            /// </summary>
            /// <returns>Returns the long value read.</returns>
            public long ReadLong()
            {
                ReadCheck(8);
                return (ReadInt() & 0xffffffff) | (((long)ReadInt()) << 32);
            }

            /// <summary>
            /// Read an integer in little endian form from the last <see cref="Find">found</see> data value.
            /// </summary>
            /// <returns>Returns the integer read.</returns>
            public int ReadInt()
            {
                ReadCheck(4);

                int result = _data[_index] + (_data[_index + 1] << 8) +
                    (_data[_index + 2] << 16) + (_data[_index + 3] << 24);
                _index += 4;
                return result;
            }

            /// <summary>
            /// Read a short value in little endian form from the last <see cref="Find">found</see> data value.
            /// </summary>
            /// <returns>Returns the short value read.</returns>
            public int ReadShort()
            {
                ReadCheck(2);
                int result = _data[_index] + (_data[_index + 1] << 8);
                _index += 2;
                return result;
            }

            /// <summary>
            /// Read a byte from an extra data
            /// </summary>
            /// <returns>The byte value read or -1 if the end of data has been reached.</returns>
            public int ReadByte()
            {
                int result = -1;
                if ((_index < _data.Length) && (_readValueStart + _readValueLength > _index))
                {
                    result = _data[_index];
                    _index += 1;
                }
                return result;
            }

            /// <summary>
            /// Skip data during reading.
            /// </summary>
            /// <param name="amount">The number of bytes to skip.</param>
            public void Skip(int amount)
            {
                ReadCheck(amount);
                _index += amount;
            }

            void ReadCheck(int length)
            {
                if ((_readValueStart > _data.Length) ||
                    (_readValueStart < 4))
                {
                    throw new Sys.ApplicationException("Find must be called before calling a Read method");
                }

                if (_index > _readValueStart + _readValueLength - length)
                {
                    throw new Sys.ApplicationException("End of extra data");
                }

                if (_index + length < 4)
                {
                    throw new Sys.ApplicationException("Cannot read before start of tag");
                }
            }

            /// <summary>
            /// Internal form of <see cref="ReadShort"/> that reads data at any location.
            /// </summary>
            /// <returns>Returns the short value read.</returns>
            int ReadShortInternal()
            {
                if (_index > _data.Length - 2)
                {
                    throw new Sys.ApplicationException("End of extra data");
                }

                int result = _data[_index] + (_data[_index + 1] << 8);
                _index += 2;
                return result;
            }

            void SetShort(ref int index, int source)
            {
                _data[index] = (byte)source;
                _data[index + 1] = (byte)(source >> 8);
                index += 2;
            }

            /// <summary>
            /// Dispose of this instance.
            /// </summary>
            public void Dispose()
            {
                if (_newEntry != null)
                {
                    _newEntry.Close();
                }
            }

            int _index;
            int _readValueStart;
            int _readValueLength;

            Sys.IO.MemoryStream _newEntry;
            byte[] _data;
        }

        public class ZipEntryFactory : ZIPLib.IEntryFactory
        {
            /// <summary>
            /// Defines the possible values to be used for the <see cref="ZipEntry.DateTime"/>.
            /// </summary>
            public enum TimeSetting
            {
                /// <summary>
                /// Use the recorded LastWriteTime value for the file.
                /// </summary>
                LastWriteTime,
                /// <summary>
                /// Use the recorded LastWriteTimeUtc value for the file
                /// </summary>
                LastWriteTimeUtc,
                /// <summary>
                /// Use the recorded CreateTime value for the file.
                /// </summary>
                CreateTime,
                /// <summary>
                /// Use the recorded CreateTimeUtc value for the file.
                /// </summary>
                CreateTimeUtc,
                /// <summary>
                /// Use the recorded LastAccessTime value for the file.
                /// </summary>
                LastAccessTime,
                /// <summary>
                /// Use the recorded LastAccessTimeUtc value for the file.
                /// </summary>
                LastAccessTimeUtc,
                /// <summary>
                /// Use a fixed value.
                /// </summary>
                /// <remarks>The actual <see cref="DateTime"/> value used can be
                /// specified via the <see cref="ZipEntryFactory(DateTime)"/> constructor or 
                /// using the <see cref="ZipEntryFactory(TimeSetting)"/> with the setting set
                /// to <see cref="TimeSetting.Fixed"/> which will use the <see cref="DateTime"/> when this class was constructed.
                /// The <see cref="FixedDateTime"/> property can also be used to set this value.</remarks>
                Fixed,
            }

            /// <summary>
            /// Initialise a new instance of the <see cref="ZipEntryFactory"/> class.
            /// </summary>
            /// <remarks>A default <see cref="INameTransform"/>, and the LastWriteTime for files is used.</remarks>
            public ZipEntryFactory()
            {
                nameTransform_ = new ZipNameTransform();
            }

            /// <summary>
            /// Initialise a new instance of <see cref="ZipEntryFactory"/> using the specified <see cref="TimeSetting"/>
            /// </summary>
            /// <param name="timeSetting">The <see cref="TimeSetting">time setting</see> to use when creating <see cref="ZipEntry">Zip entries</see>.</param>
            public ZipEntryFactory(TimeSetting timeSetting)
            {
                timeSetting_ = timeSetting;
                nameTransform_ = new ZipNameTransform();
            }

            /// <summary>
            /// Initialise a new instance of <see cref="ZipEntryFactory"/> using the specified <see cref="DateTime"/>
            /// </summary>
            /// <param name="time">The time to set all <see cref="ZipEntry.DateTime"/> values to.</param>
            public ZipEntryFactory(SysDate time)
            {
                timeSetting_ = TimeSetting.Fixed;
                FixedDateTime = time;
                nameTransform_ = new ZipNameTransform();
            }

            /// <summary>
            /// Get / set the <see cref="INameTransform"/> to be used when creating new <see cref="ZipEntry"/> values.
            /// </summary>
            /// <remarks>
            /// Setting this property to null will cause a default <see cref="ZipNameTransform">name transform</see> to be used.
            /// </remarks>
            public ZIPLib.Internal.INameTransform NameTransform
            {
                get { return nameTransform_; }
                set
                {
                    if (value == null)
                    {
                        nameTransform_ = new ZipNameTransform();
                    }
                    else
                    {
                        nameTransform_ = value;
                    }
                }
            }

            /// <summary>
            /// Get / set the <see cref="TimeSetting"/> in use.
            /// </summary>
            public TimeSetting Setting
            {
                get { return timeSetting_; }
                set { timeSetting_ = value; }
            }

            /// <summary>
            /// Get / set the <see cref="DateTime"/> value to use when <see cref="Setting"/> is set to <see cref="TimeSetting.Fixed"/>
            /// </summary>
            public SysDate FixedDateTime
            {
                get { return fixedDateTime_; }
                set
                {
                    if (value.Year < 1970)
                    {
                        throw new Sys.ArgumentException("Value is too old to be valid", "value");
                    }
                    fixedDateTime_ = value;
                }
            }

            /// <summary>
            /// A bitmask defining the attributes to be retrieved from the actual file.
            /// </summary>
            /// <remarks>The default is to get all possible attributes from the actual file.</remarks>
            public int GetAttributes
            {
                get { return getAttributes_; }
                set { getAttributes_ = value; }
            }

            /// <summary>
            /// A bitmask defining which attributes are to be set on.
            /// </summary>
            /// <remarks>By default no attributes are set on.</remarks>
            public int SetAttributes
            {
                get { return setAttributes_; }
                set { setAttributes_ = value; }
            }

            /// <summary>
            /// Get set a value indicating wether unidoce text should be set on.
            /// </summary>
            public bool IsUnicodeText
            {
                get { return isUnicodeText_; }
                set { isUnicodeText_ = value; }
            }

            /// <summary>
            /// Make a new <see cref="ZipEntry"/> for a file.
            /// </summary>
            /// <param name="fileName">The name of the file to create a new entry for.</param>
            /// <returns>Returns a new <see cref="ZipEntry"/> based on the <paramref name="fileName"/>.</returns>
            public ZipEntry MakeFileEntry(string fileName)
            {
                return MakeFileEntry(fileName, null, true);
            }

            /// <summary>
            /// Make a new <see cref="ZipEntry"/> for a file.
            /// </summary>
            /// <param name="fileName">The name of the file to create a new entry for.</param>
            /// <param name="useFileSystem">If true entry detail is retrieved from the file system if the file exists.</param>
            /// <returns>Returns a new <see cref="ZipEntry"/> based on the <paramref name="fileName"/>.</returns>
            public ZipEntry MakeFileEntry(string fileName, bool useFileSystem)
            {
                return MakeFileEntry(fileName, null, useFileSystem);
            }

            /// <summary>
            /// Make a new <see cref="ZipEntry"/> from a name.
            /// </summary>
            /// <param name="fileName">The name of the file to create a new entry for.</param>
            /// <param name="entryName">An alternative name to be used for the new entry. Null if not applicable.</param>
            /// <param name="useFileSystem">If true entry detail is retrieved from the file system if the file exists.</param>
            /// <returns>Returns a new <see cref="ZipEntry"/> based on the <paramref name="fileName"/>.</returns>
            public ZipEntry MakeFileEntry(string fileName, string entryName, bool useFileSystem)
            {
                ZipEntry result = new ZipEntry(nameTransform_.TransformFile(entryName != null && entryName.Length > 0 ? entryName : fileName));
                result.IsUnicodeText = isUnicodeText_;

                int externalAttributes = 0;
                bool useAttributes = (setAttributes_ != 0);

                Sys.IO.FileInfo fi = null;
                if (useFileSystem)
                {
                    fi = new Sys.IO.FileInfo(fileName);
                }

                if ((fi != null) && fi.Exists)
                {
                    switch (timeSetting_)
                    {
                        case TimeSetting.CreateTime:
                            result.DateTime = fi.CreationTime;
                            break;

                        case TimeSetting.CreateTimeUtc:
                            result.DateTime = fi.CreationTimeUtc;
                            break;

                        case TimeSetting.LastAccessTime:
                            result.DateTime = fi.LastAccessTime;
                            break;

                        case TimeSetting.LastAccessTimeUtc:
                            result.DateTime = fi.LastAccessTimeUtc;
                            break;

                        case TimeSetting.LastWriteTime:
                            result.DateTime = fi.LastWriteTime;
                            break;

                        case TimeSetting.LastWriteTimeUtc:
                            result.DateTime = fi.LastWriteTimeUtc;
                            break;

                        case TimeSetting.Fixed:
                            result.DateTime = fixedDateTime_;
                            break;

                        default:
                            throw new Sys.ApplicationException("Unhandled time setting in MakeFileEntry");
                    }

                    result.Size = fi.Length;

                    useAttributes = true;
                    externalAttributes = ((int)fi.Attributes & getAttributes_);
                }
                else
                {
                    if (timeSetting_ == TimeSetting.Fixed)
                    {
                        result.DateTime = fixedDateTime_;
                    }
                }

                if (useAttributes)
                {
                    externalAttributes |= setAttributes_;
                    result.ExternalFileAttributes = externalAttributes;
                }

                return result;
            }

            /// <summary>
            /// Make a new <see cref="ZipEntry"></see> for a directory.
            /// </summary>
            /// <param name="directoryName">The raw untransformed name for the new directory</param>
            /// <returns>Returns a new <see cref="ZipEntry"></see> representing a directory.</returns>
            public ZipEntry MakeDirectoryEntry(string directoryName)
            {
                return MakeDirectoryEntry(directoryName, true);
            }

            /// <summary>
            /// Make a new <see cref="ZipEntry"></see> for a directory.
            /// </summary>
            /// <param name="directoryName">The raw untransformed name for the new directory</param>
            /// <param name="useFileSystem">If true entry detail is retrieved from the file system if the file exists.</param>
            /// <returns>Returns a new <see cref="ZipEntry"></see> representing a directory.</returns>
            public ZipEntry MakeDirectoryEntry(string directoryName, bool useFileSystem)
            {

                ZipEntry result = new ZipEntry(nameTransform_.TransformDirectory(directoryName));
                result.IsUnicodeText = isUnicodeText_;
                result.Size = 0;

                int externalAttributes = 0;

                Sys.IO.DirectoryInfo di = null;

                if (useFileSystem)
                {
                    di = new Sys.IO.DirectoryInfo(directoryName);
                }


                if ((di != null) && di.Exists)
                {
                    switch (timeSetting_)
                    {
                        case TimeSetting.CreateTime:
                            result.DateTime = di.CreationTime;
                            break;

                        case TimeSetting.CreateTimeUtc:
                            result.DateTime = di.CreationTimeUtc;
                            break;

                        case TimeSetting.LastAccessTime:
                            result.DateTime = di.LastAccessTime;
                            break;

                        case TimeSetting.LastAccessTimeUtc:
                            result.DateTime = di.LastAccessTimeUtc;
                            break;

                        case TimeSetting.LastWriteTime:
                            result.DateTime = di.LastWriteTime;
                            break;

                        case TimeSetting.LastWriteTimeUtc:
                            result.DateTime = di.LastWriteTimeUtc;
                            break;

                        case TimeSetting.Fixed:
                            result.DateTime = fixedDateTime_;
                            break;

                        default:
                            throw new Sys.ApplicationException("Unhandled time setting in MakeDirectoryEntry");
                    }

                    externalAttributes = ((int)di.Attributes & getAttributes_);
                }
                else
                {
                    if (timeSetting_ == TimeSetting.Fixed)
                    {
                        result.DateTime = fixedDateTime_;
                    }
                }

                // Always set directory attribute on.
                externalAttributes |= (setAttributes_ | 16);
                result.ExternalFileAttributes = externalAttributes;

                return result;
            }

            ZIPLib.Internal.INameTransform nameTransform_;
            SysDate fixedDateTime_ = SysDate.Now;
            TimeSetting timeSetting_;
            bool isUnicodeText_;
            int getAttributes_ = -1;
            int setAttributes_;
        }

        public enum HostSystemID
        {
            Msdos = 0,
            Amiga = 1,
            OpenVms = 2,
            Unix = 3,
            VMCms = 4,
            AtariST = 5,
            OS2 = 6,
            Macintosh = 7,
            ZSystem = 8,
            Cpm = 9,
            WindowsNT = 10,
            MVS = 11,
            Vse = 12,
            AcornRisc = 13,
            Vfat = 14,
            AlternateMvs = 15,
            BeOS = 16,
            Tandem = 17,
            OS400 = 18,
            OSX = 19,
            WinZipAES = 99
        }

        public class ZipEntry : Sys.ICloneable
        {
            [Sys.Flags] internal enum Known : byte
            {
                None = 0,
                Size = 0x01,
                CompressedSize = 0x02,
                Crc = 0x04,
                Time = 0x08,
                ExternalAttributes = 0x10,
            }

            public ZipEntry(string name) : this(name, 0, ZipConstants.VersionMadeBy, CompressionMethod.Deflated) { /* NOTHING */ }

            internal ZipEntry(string name, int versionRequiredToExtract) : this(name, versionRequiredToExtract, ZipConstants.VersionMadeBy, CompressionMethod.Deflated) { /* NOTHING */ }

            internal ZipEntry(string name, int versionRequiredToExtract, int madeByInfo, CompressionMethod method)
            {
                if (name == null) { throw new Sys.ArgumentNullException("name"); }
                if (name.Length > 0xffff) { throw new Sys.ArgumentException("Name is too long", "name"); }
                if ((versionRequiredToExtract != 0) && (versionRequiredToExtract < 10)) { throw new Sys.ArgumentOutOfRangeException("versionRequiredToExtract"); }
                this.DateTime = SysDate.Now;
                this.name = CleanName(name);
                this.versionMadeBy = (ushort)madeByInfo;
                this.versionToExtract = (ushort)versionRequiredToExtract;
                this.method = method;
            }

            [Sys.Obsolete("Use Clone instead")] public ZipEntry(ZipEntry entry)
            {
                if (entry == null)
                {
                    throw new Sys.ArgumentNullException("entry");
                }

                known = entry.known;
                name = entry.name;
                size = entry.size;
                compressedSize = entry.compressedSize;
                crc = entry.crc;
                dosTime = entry.dosTime;
                method = entry.method;
                comment = entry.comment;
                versionToExtract = entry.versionToExtract;
                versionMadeBy = entry.versionMadeBy;
                externalFileAttributes = entry.externalFileAttributes;
                flags = entry.flags;

                zipFileIndex = entry.zipFileIndex;
                offset = entry.offset;

                forceZip64_ = entry.forceZip64_;

                if (entry.extra != null)
                {
                    extra = new byte[entry.extra.Length];
                    Sys.Array.Copy(entry.extra, 0, extra, 0, entry.extra.Length);
                }
            }

            public bool HasCrc
            {
                get
                {
                    return (known & Known.Crc) != 0;
                }
            }

            public bool IsCrypted
            {
                get
                {
                    return (flags & 1) != 0;
                }
                set
                {
                    if (value)
                    {
                        flags |= 1;
                    }
                    else
                    {
                        flags &= ~1;
                    }
                }
            }

            public bool IsUnicodeText
            {
                get
                {
                    return (flags & (int)GeneralBitFlags.UnicodeText) != 0;
                }
                set
                {
                    if (value)
                    {
                        flags |= (int)GeneralBitFlags.UnicodeText;
                    }
                    else
                    {
                        flags &= ~(int)GeneralBitFlags.UnicodeText;
                    }
                }
            }

            internal byte CryptoCheckValue
            {
                get
                {
                    return cryptoCheckValue_;
                }

                set
                {
                    cryptoCheckValue_ = value;
                }
            }

            public int Flags
            {
                get
                {
                    return flags;
                }
                set
                {
                    flags = value;
                }
            }

            public long ZipFileIndex
            {
                get
                {
                    return zipFileIndex;
                }
                set
                {
                    zipFileIndex = value;
                }
            }

            public long Offset
            {
                get
                {
                    return offset;
                }
                set
                {
                    offset = value;
                }
            }

            public int ExternalFileAttributes
            {
                get
                {
                    if ((known & Known.ExternalAttributes) == 0)
                    {
                        return -1;
                    }
                    else
                    {
                        return externalFileAttributes;
                    }
                }

                set
                {
                    externalFileAttributes = value;
                    known |= Known.ExternalAttributes;
                }
            }

            public int VersionMadeBy
            {
                get
                {
                    return (versionMadeBy & 0xff);
                }
            }

            public bool IsDOSEntry
            {
                get
                {
                    return ((HostSystem == (int)HostSystemID.Msdos) ||
                        (HostSystem == (int)HostSystemID.WindowsNT));
                }
            }

            internal bool HasDosAttributes(int attributes)
            {
                bool result = false;
                if ((known & Known.ExternalAttributes) != 0)
                {
                    if (((HostSystem == (int)HostSystemID.Msdos) ||
                        (HostSystem == (int)HostSystemID.WindowsNT)) &&
                        (ExternalFileAttributes & attributes) == attributes)
                    {
                        result = true;
                    }
                }
                return result;
            }

            public int HostSystem
            {
                get
                {
                    return (versionMadeBy >> 8) & 0xff;
                }

                set
                {
                    versionMadeBy &= 0xff;
                    versionMadeBy |= (ushort)((value & 0xff) << 8);
                }
            }

            public int Version
            {
                get
                {
                    // Return recorded version if known.
                    if (versionToExtract != 0)
                    {
                        return versionToExtract & 0x00ff;				// Only lower order byte. High order is O/S file Sys.
                    }
                    else
                    {
                        int result = 10;
                        if (AESKeySize > 0)
                        {
                            result = ZipConstants.VERSION_AES;			// Ver 5.1 = AES
                        }
                        else if (CentralHeaderRequiresZip64)
                        {
                            result = ZipConstants.VersionZip64;
                        }
                        else if (CompressionMethod.Deflated == method)
                        {
                            result = 20;
                        }
                        else if (IsDirectory == true)
                        {
                            result = 20;
                        }
                        else if (IsCrypted == true)
                        {
                            result = 20;
                        }
                        else if (HasDosAttributes(0x08))
                        {
                            result = 11;
                        }
                        return result;
                    }
                }
            }

            public bool CanDecompress
            {
                get
                {
                    return (Version <= ZipConstants.VersionMadeBy) &&
                        ((Version == 10) ||
                        (Version == 11) ||
                        (Version == 20) ||
                        (Version == 45) ||
                        (Version == 51)) &&
                        IsCompressionMethodSupported();
                }
            }

            public void ForceZip64()
            {
                forceZip64_ = true;
            }

            public bool IsZip64Forced()
            {
                return forceZip64_;
            }

            public bool LocalHeaderRequiresZip64
            {
                get
                {
                    bool result = forceZip64_;
                    if (!result)
                    {
                        ulong trueCompressedSize = compressedSize;
                        if ((versionToExtract == 0) && IsCrypted) { trueCompressedSize += ZipConstants.CryptoHeaderSize; }
                        result = ((this.size >= uint.MaxValue) || (trueCompressedSize >= uint.MaxValue)) && ((versionToExtract == 0) || (versionToExtract >= ZipConstants.VersionZip64));
                    }
                    return result;
                }
            }

            public bool CentralHeaderRequiresZip64
            {
                get
                {
                    return LocalHeaderRequiresZip64 || (offset >= uint.MaxValue);
                }
            }

            public long DosTime
            {
                get
                {
                    if ((known & Known.Time) == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return dosTime;
                    }
                }

                set
                {
                    unchecked
                    {
                        dosTime = (uint)value;
                    }

                    known |= Known.Time;
                }
            }

            public SysDate DateTime
            {
                get
                {
                    uint sec = SysMath.Min(59, 2 * (dosTime & 0x1f));
                    uint min = SysMath.Min(59, (dosTime >> 5) & 0x3f);
                    uint hrs = SysMath.Min(23, (dosTime >> 11) & 0x1f);
                    uint mon = SysMath.Max(1, SysMath.Min(12, ((dosTime >> 21) & 0xf)));
                    uint year = ((dosTime >> 25) & 0x7f) + 1980;
                    int day = SysMath.Max(1, SysMath.Min(SysDate.DaysInMonth((int)year, (int)mon), (int)((dosTime >> 16) & 0x1f)));
                    return new SysDate((int)year, (int)mon, day, (int)hrs, (int)min, (int)sec);
                }
                set
                {
                    uint year = (uint)value.Year;
                    uint month = (uint)value.Month;
                    uint day = (uint)value.Day;
                    uint hour = (uint)value.Hour;
                    uint minute = (uint)value.Minute;
                    uint second = (uint)value.Second;

                    if (year < 1980)
                    {
                        year = 1980;
                        month = 1;
                        day = 1;
                        hour = 0;
                        minute = 0;
                        second = 0;
                    }
                    else if (year > 2107)
                    {
                        year = 2107;
                        month = 12;
                        day = 31;
                        hour = 23;
                        minute = 59;
                        second = 59;
                    }

                    DosTime = ((year - 1980) & 0x7f) << 25 |
                        (month << 21) |
                        (day << 16) |
                        (hour << 11) |
                        (minute << 5) |
                        (second >> 1);
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public long Size
            {
                get
                {
                    return (known & Known.Size) != 0 ? (long)size : -1L;
                }
                set
                {
                    this.size = (ulong)value;
                    this.known |= Known.Size;
                }
            }

            public long CompressedSize
            {
                get
                {
                    return (known & Known.CompressedSize) != 0 ? (long)compressedSize : -1L;
                }
                set
                {
                    this.compressedSize = (ulong)value;
                    this.known |= Known.CompressedSize;
                }
            }

            public long Crc
            {
                get
                {
                    return (known & Known.Crc) != 0 ? crc & 0xffffffffL : -1L;
                }
                set
                {
                    if (((ulong)crc & 0xffffffff00000000L) != 0)
                    {
                        throw new Sys.ArgumentOutOfRangeException("value");
                    }
                    this.crc = (uint)value;
                    this.known |= Known.Crc;
                }
            }

            public CompressionMethod CompressionMethod
            {
                get
                {
                    return method;
                }

                set
                {
                    if (!IsCompressionMethodSupported(value))
                    {
                        throw new Sys.NotSupportedException("Compression method not supported");
                    }
                    this.method = value;
                }
            }

            internal CompressionMethod CompressionMethodForHeader
            {
                get
                {
                    return (AESKeySize > 0) ? CompressionMethod.WinZipAES : method;
                }
            }

            public byte[] ExtraData
            {

                get
                {
                    return extra;
                }

                set
                {
                    if (value == null)
                    {
                        extra = null;
                    }
                    else
                    {
                        if (value.Length > 0xffff)
                        {
                            throw new Sys.ArgumentOutOfRangeException("value");
                        }

                        extra = new byte[value.Length];
                        Sys.Array.Copy(value, 0, extra, 0, value.Length);
                    }
                }
            }

            public int AESKeySize
            {
                get
                {
                    // the strength (1 or 3) is in the entry header
                    switch (_aesEncryptionStrength)
                    {
                        case 0: return 0;	// Not AES
                        case 1: return 128;
                        case 2: return 192; // Not used by WinZip
                        case 3: return 256;
                        default: throw new Sys.ApplicationException("Invalid AESEncryptionStrength " + _aesEncryptionStrength);
                    }
                }
                set
                {
                    switch (value)
                    {
                        case 0: _aesEncryptionStrength = 0; break;
                        case 128: _aesEncryptionStrength = 1; break;
                        case 256: _aesEncryptionStrength = 3; break;
                        default: throw new Sys.ApplicationException("AESKeySize must be 0, 128 or 256: " + value);
                    }
                }
            }

            internal byte AESEncryptionStrength
            {
                get
                {
                    return (byte)_aesEncryptionStrength;
                }
            }

            internal int AESSaltLen
            {
                get
                {
                    // Key size -> Salt length: 128 bits = 8 bytes, 192 bits = 12 bytes, 256 bits = 16 bytes.
                    return AESKeySize / 16;
                }
            }

            internal int AESOverheadSize
            {
                get
                {
                    // File format:
                    //   Bytes		Content
                    // Variable		Salt value
                    //     2		Password verification value
                    // Variable		Encrypted file data
                    //    10		Authentication code
                    return 12 + AESSaltLen;
                }
            }

            internal void ProcessExtraData(bool localHeader)
            {
                ZipExtraData extraData = new ZipExtraData(this.extra);

                if (extraData.Find(0x0001))
                {
                    // Version required to extract is ignored here as some archivers dont set it correctly
                    // in theory it should be version 45 or higher

                    // The recorded size will change but remember that this is zip64.
                    forceZip64_ = true;

                    if (extraData.ValueLength < 4)
                    {
                        throw new Sys.ApplicationException("Extra data extended Zip64 information length is invalid");
                    }

                    if (localHeader || (size == uint.MaxValue))
                    {
                        size = (ulong)extraData.ReadLong();
                    }

                    if (localHeader || (compressedSize == uint.MaxValue))
                    {
                        compressedSize = (ulong)extraData.ReadLong();
                    }

                    if (!localHeader && (offset == uint.MaxValue))
                    {
                        offset = extraData.ReadLong();
                    }

                    // Disk number on which file starts is ignored
                }
                else
                {
                    if (
                        ((versionToExtract & 0xff) >= ZipConstants.VersionZip64) &&
                        ((size == uint.MaxValue) || (compressedSize == uint.MaxValue))
                    )
                    {
                        throw new Sys.ApplicationException("Zip64 Extended information required but is missing.");
                    }
                }

                if (extraData.Find(10))
                {
                    // No room for any tags.
                    if (extraData.ValueLength < 4)
                    {
                        throw new Sys.ApplicationException("NTFS Extra data invalid");
                    }

                    extraData.ReadInt(); // Reserved

                    while (extraData.UnreadCount >= 4)
                    {
                        int ntfsTag = extraData.ReadShort();
                        int ntfsLength = extraData.ReadShort();
                        if (ntfsTag == 1)
                        {
                            if (ntfsLength >= 24)
                            {
                                long lastModification = extraData.ReadLong();
                                long lastAccess = extraData.ReadLong();
                                long createTime = extraData.ReadLong();
                                DateTime = SysDate.FromFileTime(lastModification);
                            }
                            break;
                        }
                        else
                        {
                            // An unknown NTFS tag so simply skip it.
                            extraData.Skip(ntfsLength);
                        }
                    }
                }
                else if (extraData.Find(0x5455))
                {
                    int length = extraData.ValueLength;
                    int flags = extraData.ReadByte();

                    // Can include other times but these are ignored.  Length of data should
                    // actually be 1 + 4 * no of bits in flags.
                    if (((flags & 1) != 0) && (length >= 5))
                    {
                        int iTime = extraData.ReadInt();
                        DateTime = (new SysDate(1970, 1, 1, 0, 0, 0).ToUniversalTime() + new Sys.TimeSpan(0, 0, 0, iTime, 0)).ToLocalTime();
                    }
                }
                if (method == CompressionMethod.WinZipAES)
                {
                    ProcessAESExtraData(extraData);
                }
            }

            private void ProcessAESExtraData(ZipExtraData extraData)
            {
                if (extraData.Find(0x9901))
                {
                    // Set version and flag for Zipfile.CreateAndInitDecryptionStream
                    versionToExtract = ZipConstants.VERSION_AES;			// Ver 5.1 = AES see "Version" getter
                    // Set StrongEncryption flag for ZipFile.CreateAndInitDecryptionStream
                    Flags = Flags | (int)GeneralBitFlags.StrongEncryption;
                    //
                    // Unpack AES extra data field see http://www.winzip.com/aes_info.htm
                    int length = extraData.ValueLength;			// Data size currently 7
                    if (length < 7)
                        throw new Sys.ApplicationException("AES Extra Data Length " + length + " invalid.");
                    int ver = extraData.ReadShort();			// Version number (1=AE-1 2=AE-2)
                    int vendorId = extraData.ReadShort();		// 2-character vendor ID 0x4541 = "AE"
                    int encrStrength = extraData.ReadByte();	// encryption strength 1 = 128 2 = 192 3 = 256
                    int actualCompress = extraData.ReadShort(); // The actual compression method used to compress the file
                    _aesVer = ver;
                    _aesEncryptionStrength = encrStrength;
                    method = (CompressionMethod)actualCompress;
                }
                else
                    throw new Sys.ApplicationException("AES Extra Data missing");
            }

            public string Comment
            {
                get
                {
                    return comment;
                }
                set
                {
                    // This test is strictly incorrect as the length is in characters
                    // while the storage limit is in bytes.
                    // While the test is partially correct in that a comment of this length or greater 
                    // is definitely invalid, shorter comments may also have an invalid length
                    // where there are multi-byte characters
                    // The full test is not possible here however as the code page to apply conversions with
                    // isnt available.
                    if ((value != null) && (value.Length > 0xffff))
                    {
                        throw new Sys.ArgumentOutOfRangeException("value", "cannot exceed 65535");
                    }

                    comment = value;
                }
            }

            public bool IsDirectory
            {
                get
                {
                    int nameLength = name.Length;
                    return ((nameLength > 0) && ((name[nameLength - 1] == '/') || (name[nameLength - 1] == '\\'))) || HasDosAttributes(16);
                }
            }

            public object Clone()
            {
                ZipEntry result = (ZipEntry)this.MemberwiseClone();
                if (extra != null)
                {
                    result.extra = new byte[extra.Length];
                    Sys.Array.Copy(extra, 0, result.extra, 0, extra.Length);
                }
                return result;
            }

            public bool IsFile { get { return !IsDirectory && !HasDosAttributes(8); } }
            public bool IsCompressionMethodSupported() { return IsCompressionMethodSupported(CompressionMethod); }
            public override string ToString() { return name; }
            public static bool IsCompressionMethodSupported(CompressionMethod method) { return (method == CompressionMethod.Deflated) || (method == CompressionMethod.Stored); }

            public static string CleanName(string name)
            {
                if (name == null) { return string.Empty; }
                if (Sys.IO.Path.IsPathRooted(name)) { name = name.Substring(Sys.IO.Path.GetPathRoot(name).Length); }
                name = name.Replace(@"\", "/");
                while ((name.Length > 0) && (name[0] == '/')) { name = name.Remove(0, 1); }
                return name;
            }

            Known known;
            int externalFileAttributes = -1;
            ushort versionMadeBy;
            string name;
            ulong size;
            ulong compressedSize;
            ushort versionToExtract;
            uint crc;
            uint dosTime;
            CompressionMethod method = CompressionMethod.Deflated;
            byte[] extra;
            string comment;
            int flags;
            long zipFileIndex = -1;
            long offset;
            bool forceZip64_;
            byte cryptoCheckValue_;
            int _aesVer;
            int _aesEncryptionStrength;
        }

        public interface IEntryFactory
        {
            /// <summary>
            /// Create a <see cref="ZipEntry"/> for a file given its name
            /// </summary>
            /// <param name="fileName">The name of the file to create an entry for.</param>
            /// <returns>Returns a <see cref="ZipEntry">file entry</see> based on the <paramref name="fileName"/> passed.</returns>
            ZIPLib.ZipEntry MakeFileEntry(string fileName);

            /// <summary>
            /// Create a <see cref="ZipEntry"/> for a file given its name
            /// </summary>
            /// <param name="fileName">The name of the file to create an entry for.</param>
            /// <param name="useFileSystem">If true get details from the file system if the file exists.</param>
            /// <returns>Returns a <see cref="ZipEntry">file entry</see> based on the <paramref name="fileName"/> passed.</returns>
            ZIPLib.ZipEntry MakeFileEntry(string fileName, bool useFileSystem);

            /// <summary>
            /// Create a <see cref="ZipEntry"/> for a file given its actual name and optional override name
            /// </summary>
            /// <param name="fileName">The name of the file to create an entry for.</param>
            /// <param name="entryName">An alternative name to be used for the new entry. Null if not applicable.</param>
            /// <param name="useFileSystem">If true get details from the file system if the file exists.</param>
            /// <returns>Returns a <see cref="ZipEntry">file entry</see> based on the <paramref name="fileName"/> passed.</returns>
            ZIPLib.ZipEntry MakeFileEntry(string fileName, string entryName, bool useFileSystem);

            /// <summary>
            /// Create a <see cref="ZipEntry"/> for a directory given its name
            /// </summary>
            /// <param name="directoryName">The name of the directory to create an entry for.</param>
            /// <returns>Returns a <see cref="ZipEntry">directory entry</see> based on the <paramref name="directoryName"/> passed.</returns>
            ZIPLib.ZipEntry MakeDirectoryEntry(string directoryName);

            /// <summary>
            /// Create a <see cref="ZipEntry"/> for a directory given its name
            /// </summary>
            /// <param name="directoryName">The name of the directory to create an entry for.</param>
            /// <param name="useFileSystem">If true get details from the file system for this directory if it exists.</param>
            /// <returns>Returns a <see cref="ZipEntry">directory entry</see> based on the <paramref name="directoryName"/> passed.</returns>
            ZIPLib.ZipEntry MakeDirectoryEntry(string directoryName, bool useFileSystem);

            /// <summary>
            /// Get/set the <see cref="INameTransform"></see> applicable.
            /// </summary>
            ZIPLib.Internal.INameTransform NameTransform { get; set; }
        }

        public enum UseZip64
        {
            /// <summary>
            /// Zip64 will not be forced on entries during processing.
            /// </summary>
            /// <remarks>An entry can have this overridden if required <see cref="ZipEntry.ForceZip64"></see></remarks>
            Off,
            /// <summary>
            /// Zip64 should always be used.
            /// </summary>
            On,
            /// <summary>
            /// #ZipLib will determine use based on entry values when added to archive.
            /// </summary>
            Dynamic,
        }

        public enum CompressionMethod
        {
            /// <summary>
            /// A direct copy of the file contents is held in the archive
            /// </summary>
            Stored = 0,

            /// <summary>
            /// Common Zip compression method using a sliding dictionary 
            /// of up to 32KB and secondary compression from Huffman/Shannon-Fano trees
            /// </summary>
            Deflated = 8,

            /// <summary>
            /// An extension to deflate with a 64KB window. Not supported by #Zip currently
            /// </summary>
            Deflate64 = 9,

            /// <summary>
            /// BZip2 compression. Not supported by #Zip.
            /// </summary>
            BZip2 = 11,

            /// <summary>
            /// WinZip special for AES encryption, Now supported by #Zip.
            /// </summary>
            WinZipAES = 99,

        }

        public enum EncryptionAlgorithm
        {
            /// <summary>
            /// No encryption has been used.
            /// </summary>
            None = 0,
            /// <summary>
            /// Encrypted using PKZIP 2.0 or 'classic' encryption.
            /// </summary>
            PkzipClassic = 1,
            /// <summary>
            /// DES encryption has been used.
            /// </summary>
            Des = 0x6601,
            /// <summary>
            /// RCS encryption has been used for encryption.
            /// </summary>
            RC2 = 0x6602,
            /// <summary>
            /// Triple DES encryption with 168 bit keys has been used for this entry.
            /// </summary>
            TripleDes168 = 0x6603,
            /// <summary>
            /// Triple DES with 112 bit keys has been used for this entry.
            /// </summary>
            TripleDes112 = 0x6609,
            /// <summary>
            /// AES 128 has been used for encryption.
            /// </summary>
            Aes128 = 0x660e,
            /// <summary>
            /// AES 192 has been used for encryption.
            /// </summary>
            Aes192 = 0x660f,
            /// <summary>
            /// AES 256 has been used for encryption.
            /// </summary>
            Aes256 = 0x6610,
            /// <summary>
            /// RC2 corrected has been used for encryption.
            /// </summary>
            RC2Corrected = 0x6702,
            /// <summary>
            /// Blowfish has been used for encryption.
            /// </summary>
            Blowfish = 0x6720,
            /// <summary>
            /// Twofish has been used for encryption.
            /// </summary>
            Twofish = 0x6721,
            /// <summary>
            /// RC4 has been used for encryption.
            /// </summary>
            RC4 = 0x6801,
            /// <summary>
            /// An unknown algorithm has been used for encryption.
            /// </summary>
            Unknown = 0xffff
        }

        [Sys.Flags] public enum GeneralBitFlags : int
        {
            /// <summary>
            /// Bit 0 if set indicates that the file is encrypted
            /// </summary>
            Encrypted = 0x0001,
            /// <summary>
            /// Bits 1 and 2 - Two bits defining the compression method (only for Method 6 Imploding and 8,9 Deflating)
            /// </summary>
            Method = 0x0006,
            /// <summary>
            /// Bit 3 if set indicates a trailing data desciptor is appended to the entry data
            /// </summary>
            Descriptor = 0x0008,
            /// <summary>
            /// Bit 4 is reserved for use with method 8 for enhanced deflation
            /// </summary>
            ReservedPKware4 = 0x0010,
            /// <summary>
            /// Bit 5 if set indicates the file contains Pkzip compressed patched data.
            /// Requires version 2.7 or greater.
            /// </summary>
            Patched = 0x0020,
            /// <summary>
            /// Bit 6 if set indicates strong encryption has been used for this entry.
            /// </summary>
            StrongEncryption = 0x0040,
            /// <summary>
            /// Bit 7 is currently unused
            /// </summary>
            Unused7 = 0x0080,
            /// <summary>
            /// Bit 8 is currently unused
            /// </summary>
            Unused8 = 0x0100,
            /// <summary>
            /// Bit 9 is currently unused
            /// </summary>
            Unused9 = 0x0200,
            /// <summary>
            /// Bit 10 is currently unused
            /// </summary>
            Unused10 = 0x0400,
            /// <summary>
            /// Bit 11 if set indicates the filename and 
            /// comment fields for this file must be encoded using UTF-8.
            /// </summary>
            UnicodeText = 0x0800,
            /// <summary>
            /// Bit 12 is documented as being reserved by PKware for enhanced compression.
            /// </summary>
            EnhancedCompress = 0x1000,
            /// <summary>
            /// Bit 13 if set indicates that values in the local header are masked to hide
            /// their actual values, and the central directory is encrypted.
            /// </summary>
            /// <remarks>
            /// Used when encrypting the central directory contents.
            /// </remarks>
            HeaderMasked = 0x2000,
            /// <summary>
            /// Bit 14 is documented as being reserved for use by PKware
            /// </summary>
            ReservedPkware14 = 0x4000,
            /// <summary>
            /// Bit 15 is documented as being reserved for use by PKware
            /// </summary>
            ReservedPkware15 = 0x8000
        }

        public sealed class ZipConstants
        {
            /// <summary>
            /// The version made by field for entries in the central header when created by this library
            /// </summary>
            /// <remarks>
            /// This is also the Zip version for the library when comparing against the version required to extract
            /// for an entry.  See <see cref="ZipEntry.CanDecompress"/>.
            /// </remarks>
            public const int VersionMadeBy = 51; // was 45 before AES

            /// <summary>
            /// The version made by field for entries in the central header when created by this library
            /// </summary>
            /// <remarks>
            /// This is also the Zip version for the library when comparing against the version required to extract
            /// for an entry.  See <see cref="ZipInputStream.CanDecompressEntry">ZipInputStream.CanDecompressEntry</see>.
            /// </remarks>
            [Sys.Obsolete("Use VersionMadeBy instead")] public const int VERSION_MADE_BY = 51;

            /// <summary>
            /// The minimum version required to support strong encryption
            /// </summary>
            public const int VersionStrongEncryption = 50;

            /// <summary>
            /// The minimum version required to support strong encryption
            /// </summary>
            [Sys.Obsolete("Use VersionStrongEncryption instead")] public const int VERSION_STRONG_ENCRYPTION = 50;

            /// <summary>
            /// Version indicating AES encryption
            /// </summary>
            public const int VERSION_AES = 51;

            /// <summary>
            /// The version required for Zip64 extensions (4.5 or higher)
            /// </summary>
            public const int VersionZip64 = 45;

            /// <summary>
            /// Size of local entry header (excluding variable length fields at end)
            /// </summary>
            public const int LocalHeaderBaseSize = 30;

            /// <summary>
            /// Size of local entry header (excluding variable length fields at end)
            /// </summary>
            [Sys.Obsolete("Use LocalHeaderBaseSize instead")] public const int LOCHDR = 30;

            /// <summary>
            /// Size of Zip64 data descriptor
            /// </summary>
            public const int Zip64DataDescriptorSize = 24;

            /// <summary>
            /// Size of data descriptor
            /// </summary>
            public const int DataDescriptorSize = 16;

            /// <summary>
            /// Size of data descriptor
            /// </summary>
            [Sys.Obsolete("Use DataDescriptorSize instead")] public const int EXTHDR = 16;

            /// <summary>
            /// Size of central header entry (excluding variable fields)
            /// </summary>
            public const int CentralHeaderBaseSize = 46;

            /// <summary>
            /// Size of central header entry
            /// </summary>
            [Sys.Obsolete("Use CentralHeaderBaseSize instead")] public const int CENHDR = 46;

            /// <summary>
            /// Size of end of central record (excluding variable fields)
            /// </summary>
            public const int EndOfCentralRecordBaseSize = 22;

            /// <summary>
            /// Size of end of central record (excluding variable fields)
            /// </summary>
            [Sys.Obsolete("Use EndOfCentralRecordBaseSize instead")] public const int ENDHDR = 22;

            /// <summary>
            /// Size of 'classic' cryptographic header stored before any entry data
            /// </summary>
            public const int CryptoHeaderSize = 12;

            /// <summary>
            /// Size of cryptographic header stored before entry data
            /// </summary>
            [Sys.Obsolete("Use CryptoHeaderSize instead")] public const int CRYPTO_HEADER_SIZE = 12;

            /// <summary>
            /// Signature for local entry header
            /// </summary>
            public const int LocalHeaderSignature = 'P' | ('K' << 8) | (3 << 16) | (4 << 24);

            /// <summary>
            /// Signature for local entry header
            /// </summary>
            [Sys.Obsolete("Use LocalHeaderSignature instead")] public const int LOCSIG = 'P' | ('K' << 8) | (3 << 16) | (4 << 24);

            /// <summary>
            /// Signature for spanning entry
            /// </summary>
            public const int SpanningSignature = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);

            /// <summary>
            /// Signature for spanning entry
            /// </summary>
            [Sys.Obsolete("Use SpanningSignature instead")]
            public const int SPANNINGSIG = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);

            public const int SpanningTempSignature = 'P' | ('K' << 8) | ('0' << 16) | ('0' << 24);
            [Sys.Obsolete("Use SpanningTempSignature instead")] public const int SPANTEMPSIG = 'P' | ('K' << 8) | ('0' << 16) | ('0' << 24);
            public const int DataDescriptorSignature = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
            [Sys.Obsolete("Use DataDescriptorSignature instead")] public const int EXTSIG = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
            [Sys.Obsolete("Use CentralHeaderSignature instead")] public const int CENSIG = 'P' | ('K' << 8) | (1 << 16) | (2 << 24);
            public const int CentralHeaderSignature = 'P' | ('K' << 8) | (1 << 16) | (2 << 24);
            public const int Zip64CentralFileHeaderSignature = 'P' | ('K' << 8) | (6 << 16) | (6 << 24);
            [Sys.Obsolete("Use Zip64CentralFileHeaderSignature instead")] public const int CENSIG64 = 'P' | ('K' << 8) | (6 << 16) | (6 << 24);
            public const int Zip64CentralDirLocatorSignature = 'P' | ('K' << 8) | (6 << 16) | (7 << 24);
            public const int ArchiveExtraDataSignature = 'P' | ('K' << 8) | (6 << 16) | (7 << 24);
            public const int CentralHeaderDigitalSignature = 'P' | ('K' << 8) | (5 << 16) | (5 << 24);
            [Sys.Obsolete("Use CentralHeaderDigitalSignaure instead")] public const int CENDIGITALSIG = 'P' | ('K' << 8) | (5 << 16) | (5 << 24);
            public const int EndOfCentralDirectorySignature = 'P' | ('K' << 8) | (5 << 16) | (6 << 24);
            [Sys.Obsolete("Use EndOfCentralDirectorySignature instead")] public const int ENDSIG = 'P' | ('K' << 8) | (5 << 16) | (6 << 24);
            internal static int defaultCodePage = Sys.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage;

            /// <summary>
            /// Default encoding used for string conversion.  0 gives the default system OEM code page.
            /// Dont use unicode encodings if you want to be Zip compatible!
            /// Using the default code page isnt the full solution neccessarily
            /// there are many variable factors, codepage 850 is often a good choice for
            /// European users, however be careful about compatability.
            /// </summary>
            public static int DefaultCodePage
            {
                get
                {
                    return defaultCodePage;
                }
                set
                {
                    defaultCodePage = value;
                }
            }

            /// <summary>
            /// Convert a portion of a byte array to a string.
            /// </summary>		
            /// <param name="data">
            /// Data to convert to string
            /// </param>
            /// <param name="count">
            /// Number of bytes to convert starting from index 0
            /// </param>
            /// <returns>
            /// data[0]..data[length - 1] converted to a string
            /// </returns>
            public static string ConvertToString(byte[] data, int count)
            {
                if (data == null)
                {
                    return string.Empty;
                }

                return SysTxt.Encoding.GetEncoding(DefaultCodePage).GetString(data, 0, count);
            }

            /// <summary>
            /// Convert a byte array to string
            /// </summary>
            /// <param name="data">
            /// Byte array to convert
            /// </param>
            /// <returns>
            /// <paramref name="data">data</paramref>converted to a string
            /// </returns>
            public static string ConvertToString(byte[] data)
            {
                if (data == null)
                {
                    return string.Empty;
                }
                return ConvertToString(data, data.Length);
            }

            /// <summary>
            /// Convert a byte array to string
            /// </summary>
            /// <param name="flags">The applicable general purpose bits flags</param>
            /// <param name="data">
            /// Byte array to convert
            /// </param>
            /// <param name="count">The number of bytes to convert.</param>
            /// <returns>
            /// <paramref name="data">data</paramref>converted to a string
            /// </returns>
            public static string ConvertToStringExt(int flags, byte[] data, int count)
            {
                if (data == null)
                {
                    return string.Empty;
                }

                if ((flags & (int)GeneralBitFlags.UnicodeText) != 0)
                {
                    return SysTxt.Encoding.UTF8.GetString(data, 0, count);
                }
                else
                {
                    return ConvertToString(data, count);
                }
            }

            /// <summary>
            /// Convert a byte array to string
            /// </summary>
            /// <param name="data">
            /// Byte array to convert
            /// </param>
            /// <param name="flags">The applicable general purpose bits flags</param>
            /// <returns>
            /// <paramref name="data">data</paramref>converted to a string
            /// </returns>
            public static string ConvertToStringExt(int flags, byte[] data)
            {
                if (data == null)
                {
                    return string.Empty;
                }

                if ((flags & (int)GeneralBitFlags.UnicodeText) != 0)
                {
                    return SysTxt.Encoding.UTF8.GetString(data, 0, data.Length);
                }
                else
                {
                    return ConvertToString(data, data.Length);
                }
            }

            /// <summary>
            /// Convert a string to a byte array
            /// </summary>
            /// <param name="str">
            /// String to convert to an array
            /// </param>
            /// <returns>Converted array</returns>
            public static byte[] ConvertToArray(string str)
            {
                if (str == null)
                {
                    return new byte[0];
                }

                return SysTxt.Encoding.GetEncoding(DefaultCodePage).GetBytes(str);
            }

            /// <summary>
            /// Convert a string to a byte array
            /// </summary>
            /// <param name="flags">The applicable <see cref="GeneralBitFlags">general purpose bits flags</see></param>
            /// <param name="str">
            /// String to convert to an array
            /// </param>
            /// <returns>Converted array</returns>
            public static byte[] ConvertToArray(int flags, string str)
            {
                if (str == null)
                {
                    return new byte[0];
                }

                if ((flags & (int)GeneralBitFlags.UnicodeText) != 0)
                {
                    return SysTxt.Encoding.UTF8.GetBytes(str);
                }
                else
                {
                    return ConvertToArray(str);
                }
            }

            ZipConstants()
            {
                // Do nothing
            }
        }

        namespace Internal
        {
            public class ScanEventArgs : Sys.EventArgs
            {
                /// <summary>
                /// Initialise a new instance of <see cref="ScanEventArgs"/>
                /// </summary>
                /// <param name="name">The file or directory name.</param>
                public ScanEventArgs(string name) { name_ = name; }

                /// <summary>
                /// The file or directory name for this event.
                /// </summary>
                public string Name { get { return name_; } }

                /// <summary>
                /// Get set a value indicating if scanning should continue or not.
                /// </summary>
                public bool ContinueRunning
                {
                    get { return continueRunning_; }
                    set { continueRunning_ = value; }
                }

                string name_;
                bool continueRunning_ = true;
            }

            public class ProgressEventArgs : Sys.EventArgs
            {
                /// <summary>
                /// Initialise a new instance of <see cref="ScanEventArgs"/>
                /// </summary>
                /// <param name="name">The file or directory name if known.</param>
                /// <param name="processed">The number of bytes processed so far</param>
                /// <param name="target">The total number of bytes to process, 0 if not known</param>
                public ProgressEventArgs(string name, long processed, long target)
                {
                    name_ = name;
                    processed_ = processed;
                    target_ = target;
                }

                /// <summary>
                /// The name for this event if known.
                /// </summary>
                public string Name
                {
                    get { return name_; }
                }

                /// <summary>
                /// Get set a value indicating wether scanning should continue or not.
                /// </summary>
                public bool ContinueRunning
                {
                    get { return continueRunning_; }
                    set { continueRunning_ = value; }
                }

                /// <summary>
                /// Get a percentage representing how much of the <see cref="Target"></see> has been processed
                /// </summary>
                /// <value>0.0 to 100.0 percent; 0 if target is not known.</value>
                public float PercentComplete
                {
                    get
                    {
                        float result;
                        if (target_ <= 0)
                        {
                            result = 0;
                        }
                        else
                        {
                            result = ((float)processed_ / (float)target_) * 100.0f;
                        }
                        return result;
                    }
                }

                /// <summary>
                /// The number of bytes processed so far
                /// </summary>
                public long Processed
                {
                    get { return processed_; }
                }

                /// <summary>
                /// The number of bytes to process.
                /// </summary>
                /// <remarks>Target may be 0 or negative if the value isnt known.</remarks>
                public long Target
                {
                    get { return target_; }
                }

                string name_;
                long processed_;
                long target_;
                bool continueRunning_ = true;
            }

            public class DirectoryEventArgs : ScanEventArgs
            {
                /// <summary>
                /// Initialize an instance of <see cref="DirectoryEventArgs"></see>.
                /// </summary>
                /// <param name="name">The name for this directory.</param>
                /// <param name="hasMatchingFiles">Flag value indicating if any matching files are contained in this directory.</param>
                public DirectoryEventArgs(string name, bool hasMatchingFiles) : base(name) { hasMatchingFiles_ = hasMatchingFiles; }

                /// <summary>
                /// Get a value indicating if the directory contains any matching files or not.
                /// </summary>
                public bool HasMatchingFiles { get { return hasMatchingFiles_; } }

                bool hasMatchingFiles_;
            }

            public class ScanFailureEventArgs : Sys.EventArgs
            {
                /// <summary>
                /// Initialise a new instance of <see cref="ScanFailureEventArgs"></see>
                /// </summary>
                /// <param name="name">The name to apply.</param>
                /// <param name="e">The exception to use.</param>
                public ScanFailureEventArgs(string name, Sys.Exception e)
                {
                    name_ = name;
                    exception_ = e;
                    continueRunning_ = true;
                }

                /// <summary>
                /// The applicable name.
                /// </summary>
                public string Name
                {
                    get { return name_; }
                }

                /// <summary>
                /// The applicable exception.
                /// </summary>
                public Sys.Exception Exception
                {
                    get { return exception_; }
                }

                /// <summary>
                /// Get / set a value indicating wether scanning should continue.
                /// </summary>
                public bool ContinueRunning
                {
                    get { return continueRunning_; }
                    set { continueRunning_ = value; }
                }

                string name_;
                Sys.Exception exception_;
                bool continueRunning_;
            }

            public delegate void ProcessDirectoryHandler(object sender, DirectoryEventArgs e);

            public delegate void ProcessFileHandler(object sender, ScanEventArgs e);

            public delegate void ProgressHandler(object sender, ProgressEventArgs e);

            public delegate void CompletedFileHandler(object sender, ScanEventArgs e);

            public delegate void DirectoryFailureHandler(object sender, ScanFailureEventArgs e);

            public delegate void FileFailureHandler(object sender, ScanFailureEventArgs e);

            public class FileSystemScanner
            {
                /// <summary>
                /// Initialise a new instance of <see cref="FileSystemScanner"></see>
                /// </summary>
                /// <param name="filter">The <see cref="PathFilter">file filter</see> to apply when scanning.</param>
                public FileSystemScanner(string filter)
                {
                    fileFilter_ = new PathFilter(filter);
                }

                /// <summary>
                /// Initialise a new instance of <see cref="FileSystemScanner"></see>
                /// </summary>
                /// <param name="fileFilter">The <see cref="PathFilter">file filter</see> to apply.</param>
                /// <param name="directoryFilter">The <see cref="PathFilter"> directory filter</see> to apply.</param>
                public FileSystemScanner(string fileFilter, string directoryFilter)
                {
                    fileFilter_ = new PathFilter(fileFilter);
                    directoryFilter_ = new PathFilter(directoryFilter);
                }

                /// <summary>
                /// Initialise a new instance of <see cref="FileSystemScanner"></see>
                /// </summary>
                /// <param name="fileFilter">The file <see cref="IScanFilter">filter</see> to apply.</param>
                public FileSystemScanner(IScanFilter fileFilter)
                {
                    fileFilter_ = fileFilter;
                }

                /// <summary>
                /// Initialise a new instance of <see cref="FileSystemScanner"></see>
                /// </summary>
                /// <param name="fileFilter">The file <see cref="IScanFilter">filter</see>  to apply.</param>
                /// <param name="directoryFilter">The directory <see cref="IScanFilter">filter</see>  to apply.</param>
                public FileSystemScanner(IScanFilter fileFilter, IScanFilter directoryFilter)
                {
                    fileFilter_ = fileFilter;
                    directoryFilter_ = directoryFilter;
                }

                /// <summary>
                /// Delegate to invoke when a directory is processed.
                /// </summary>
                public ProcessDirectoryHandler ProcessDirectory;

                /// <summary>
                /// Delegate to invoke when a file is processed.
                /// </summary>
                public ProcessFileHandler ProcessFile;

                /// <summary>
                /// Delegate to invoke when processing for a file has finished.
                /// </summary>
                public CompletedFileHandler CompletedFile;

                /// <summary>
                /// Delegate to invoke when a directory failure is detected.
                /// </summary>
                public DirectoryFailureHandler DirectoryFailure;

                /// <summary>
                /// Delegate to invoke when a file failure is detected.
                /// </summary>
                public FileFailureHandler FileFailure;

                /// <summary>
                /// Raise the DirectoryFailure event.
                /// </summary>
                /// <param name="directory">The directory name.</param>
                /// <param name="e">The exception detected.</param>
                bool OnDirectoryFailure(string directory, Sys.Exception e)
                {
                    DirectoryFailureHandler handler = DirectoryFailure;
                    bool result = (handler != null);
                    if (result)
                    {
                        ScanFailureEventArgs args = new ScanFailureEventArgs(directory, e);
                        handler(this, args);
                        alive_ = args.ContinueRunning;
                    }
                    return result;
                }

                /// <summary>
                /// Raise the FileFailure event.
                /// </summary>
                /// <param name="file">The file name.</param>
                /// <param name="e">The exception detected.</param>
                bool OnFileFailure(string file, Sys.Exception e)
                {
                    FileFailureHandler handler = FileFailure;

                    bool result = (handler != null);

                    if (result)
                    {
                        ScanFailureEventArgs args = new ScanFailureEventArgs(file, e);
                        FileFailure(this, args);
                        alive_ = args.ContinueRunning;
                    }
                    return result;
                }

                /// <summary>
                /// Raise the ProcessFile event.
                /// </summary>
                /// <param name="file">The file name.</param>
                void OnProcessFile(string file)
                {
                    ProcessFileHandler handler = ProcessFile;

                    if (handler != null)
                    {
                        ScanEventArgs args = new ScanEventArgs(file);
                        handler(this, args);
                        alive_ = args.ContinueRunning;
                    }
                }

                /// <summary>
                /// Raise the complete file event
                /// </summary>
                /// <param name="file">The file name</param>
                void OnCompleteFile(string file)
                {
                    CompletedFileHandler handler = CompletedFile;

                    if (handler != null)
                    {
                        ScanEventArgs args = new ScanEventArgs(file);
                        handler(this, args);
                        alive_ = args.ContinueRunning;
                    }
                }

                /// <summary>
                /// Raise the ProcessDirectory event.
                /// </summary>
                /// <param name="directory">The directory name.</param>
                /// <param name="hasMatchingFiles">Flag indicating if the directory has matching files.</param>
                void OnProcessDirectory(string directory, bool hasMatchingFiles)
                {
                    ProcessDirectoryHandler handler = ProcessDirectory;

                    if (handler != null)
                    {
                        DirectoryEventArgs args = new DirectoryEventArgs(directory, hasMatchingFiles);
                        handler(this, args);
                        alive_ = args.ContinueRunning;
                    }
                }

                /// <summary>
                /// Scan a directory.
                /// </summary>
                /// <param name="directory">The base directory to scan.</param>
                /// <param name="recurse">True to recurse subdirectories, false to scan a single directory.</param>
                public void Scan(string directory, bool recurse)
                {
                    alive_ = true;
                    ScanDir(directory, recurse);
                }

                void ScanDir(string directory, bool recurse)
                {

                    try
                    {
                        string[] names = Sys.IO.Directory.GetFiles(directory);
                        bool hasMatch = false;
                        for (int fileIndex = 0; fileIndex < names.Length; ++fileIndex)
                        {
                            if (!fileFilter_.IsMatch(names[fileIndex]))
                            {
                                names[fileIndex] = null;
                            }
                            else
                            {
                                hasMatch = true;
                            }
                        }
                        OnProcessDirectory(directory, hasMatch);
                        if (alive_ && hasMatch)
                        {
                            foreach (string fileName in names)
                            {
                                try
                                {
                                    if (fileName != null)
                                    {
                                        OnProcessFile(fileName);
                                        if (!alive_) { break; }
                                    }
                                }
                                catch (Sys.Exception e) { if (!OnFileFailure(fileName, e)) { throw; } }
                            }
                        }
                    }
                    catch (Sys.Exception e) { if (!OnDirectoryFailure(directory, e)) { throw; } }

                    if (alive_ && recurse)
                    {
                        try
                        {
                            string[] names = Sys.IO.Directory.GetDirectories(directory);
                            foreach (string fulldir in names)
                            {
                                if ((directoryFilter_ == null) || (directoryFilter_.IsMatch(fulldir)))
                                {
                                    ScanDir(fulldir, true);
                                    if (!alive_)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Sys.Exception e) { if (!OnDirectoryFailure(directory, e)) { throw; } }
                    }
                }

                /// <summary>
                /// The file filter currently in use.
                /// </summary>
                IScanFilter fileFilter_;
                /// <summary>
                /// The directory filter currently in use.
                /// </summary>
                IScanFilter directoryFilter_;
                /// <summary>
                /// Flag indicating if scanning should continue running.
                /// </summary>
                bool alive_;
            }

            public interface INameTransform
            {
                /// <summary>
                /// Given a file name determine the transformed value.
                /// </summary>
                /// <param name="name">The name to transform.</param>
                /// <returns>The transformed file name.</returns>
                string TransformFile(string name);

                /// <summary>
                /// Given a directory name determine the transformed value.
                /// </summary>
                /// <param name="name">The name to transform.</param>
                /// <returns>The transformed directory name</returns>
                string TransformDirectory(string name);
            }

            public interface IScanFilter
            {
                /// <summary>
                /// Test a name to see if it 'matches' the filter.
                /// </summary>
                /// <param name="name">The name to test.</param>
                /// <returns>Returns true if the name matches the filter, false if it does not match.</returns>
                bool IsMatch(string name);
            }

            public class NameFilter : IScanFilter
            {
                /// <summary>
                /// Construct an instance based on the filter expression passed
                /// </summary>
                /// <param name="filter">The filter expression.</param>
                public NameFilter(string filter)
                {
                    filter_ = filter;
                    inclusions_ = new SysColl.ArrayList();
                    exclusions_ = new SysColl.ArrayList();
                    Compile();
                }

                /// <summary>
                /// Test a string to see if it is a valid regular expression.
                /// </summary>
                /// <param name="expression">The expression to test.</param>
                /// <returns>True if expression is a valid <see cref="System.Text.RegularExpressions.Regex"/> false otherwise.</returns>
                public static bool IsValidExpression(string expression)
                {
                    bool result = true;
                    try { SysTxt.RegularExpressions.Regex exp = new SysTxt.RegularExpressions.Regex(expression, SysTxt.RegularExpressions.RegexOptions.IgnoreCase | SysTxt.RegularExpressions.RegexOptions.Singleline); }
                    catch (Sys.ArgumentException) { result = false; }
                    return result;
                }

                /// <summary>
                /// Test an expression to see if it is valid as a filter.
                /// </summary>
                /// <param name="toTest">The filter expression to test.</param>
                /// <returns>True if the expression is valid, false otherwise.</returns>
                public static bool IsValidFilterExpression(string toTest)
                {
                    bool result = true;
                    try
                    {
                        if (toTest != null)
                        {
                            string[] items = SplitQuoted(toTest);
                            for (int i = 0; i < items.Length; ++i)
                            {
                                if ((items[i] != null) && (items[i].Length > 0))
                                {
                                    string toCompile;
                                    if (items[i][0] == '+') { toCompile = items[i].Substring(1, items[i].Length - 1); }
                                    else if (items[i][0] == '-') { toCompile = items[i].Substring(1, items[i].Length - 1); }
                                    else { toCompile = items[i]; }
                                    SysTxt.RegularExpressions.Regex testRegex = new SysTxt.RegularExpressions.Regex(toCompile, SysTxt.RegularExpressions.RegexOptions.IgnoreCase | SysTxt.RegularExpressions.RegexOptions.Singleline);
                                }
                            }
                        }
                    }
                    catch (Sys.ArgumentException) { result = false; }
                    return result;
                }

                /// <summary>
                /// Split a string into its component pieces
                /// </summary>
                /// <param name="original">The original string</param>
                /// <returns>Returns an array of <see cref="T:string"/> values containing the individual filter elements.</returns>
                public static string[] SplitQuoted(string original)
                {
                    char escape = '\\';
                    char[] separators = { ';' };
                    SysColl.ArrayList result = new SysColl.ArrayList();
                    if ((original != null) && (original.Length > 0))
                    {
                        int endIndex = -1;
                        SysTxt.StringBuilder b = new SysTxt.StringBuilder();
                        while (endIndex < original.Length)
                        {
                            endIndex += 1;
                            if (endIndex >= original.Length) { result.Add(b.ToString()); }
                            else if (original[endIndex] == escape)
                            {
                                endIndex += 1;
                                if (endIndex >= original.Length) { throw new Sys.ArgumentException("Missing terminating escape character", "original"); }
                                // include escape if this is not an escaped separator
                                if (Sys.Array.IndexOf(separators, original[endIndex]) < 0) { b.Append(escape); }

                                b.Append(original[endIndex]);
                            }
                            else
                            {
                                if (Sys.Array.IndexOf(separators, original[endIndex]) >= 0)
                                {
                                    result.Add(b.ToString());
                                    b.Length = 0;
                                }
                                else
                                {
                                    b.Append(original[endIndex]);
                                }
                            }
                        }
                    }

                    return (string[])result.ToArray(typeof(string));
                }

                /// <summary>
                /// Convert this filter to its string equivalent.
                /// </summary>
                /// <returns>The string equivalent for this filter.</returns>
                public override string ToString()
                {
                    return filter_;
                }

                /// <summary>
                /// Test a value to see if it is included by the filter.
                /// </summary>
                /// <param name="name">The value to test.</param>
                /// <returns>True if the value is included, false otherwise.</returns>
                public bool IsIncluded(string name)
                {
                    bool result = false;
                    if (inclusions_.Count == 0) { result = true; }
                    else
                    {
                        foreach (SysTxt.RegularExpressions.Regex r in inclusions_)
                        {
                            if (r.IsMatch(name))
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                    return result;
                }

                /// <summary>
                /// Test a value to see if it is excluded by the filter.
                /// </summary>
                /// <param name="name">The value to test.</param>
                /// <returns>True if the value is excluded, false otherwise.</returns>
                public bool IsExcluded(string name)
                {
                    bool result = false;
                    foreach (SysTxt.RegularExpressions.Regex r in exclusions_)
                    {
                        if (r.IsMatch(name))
                        {
                            result = true;
                            break;
                        }
                    }
                    return result;
                }

                /// <summary>
                /// Test a value to see if it matches the filter.
                /// </summary>
                /// <param name="name">The value to test.</param>
                /// <returns>True if the value matches, false otherwise.</returns>
                public bool IsMatch(string name)
                {
                    return (IsIncluded(name) && !IsExcluded(name));
                }

                /// <summary>
                /// Compile this filter.
                /// </summary>
                void Compile()
                {
                    // simple scheme would be to have one RE for inclusion and one for exclusion.
                    if (filter_ == null)
                    {
                        return;
                    }

                    string[] items = SplitQuoted(filter_);
                    for (int i = 0; i < items.Length; ++i)
                    {
                        if ((items[i] != null) && (items[i].Length > 0))
                        {
                            bool include = (items[i][0] != '-');
                            string toCompile;

                            if (items[i][0] == '+')
                            {
                                toCompile = items[i].Substring(1, items[i].Length - 1);
                            }
                            else if (items[i][0] == '-')
                            {
                                toCompile = items[i].Substring(1, items[i].Length - 1);
                            }
                            else
                            {
                                toCompile = items[i];
                            }

                            // NOTE: Regular expressions can fail to compile here for a number of reasons that cause an exception
                            // these are left unhandled here as the caller is responsible for ensuring all is valid.
                            // several functions IsValidFilterExpression and IsValidExpression are provided for such checking
                            if (include)
                            {
                                inclusions_.Add(new SysTxt.RegularExpressions.Regex(toCompile, SysTxt.RegularExpressions.RegexOptions.IgnoreCase | SysTxt.RegularExpressions.RegexOptions.Compiled | SysTxt.RegularExpressions.RegexOptions.Singleline));
                            }
                            else
                            {
                                exclusions_.Add(new SysTxt.RegularExpressions.Regex(toCompile, SysTxt.RegularExpressions.RegexOptions.IgnoreCase | SysTxt.RegularExpressions.RegexOptions.Compiled | SysTxt.RegularExpressions.RegexOptions.Singleline));
                            }
                        }
                    }
                }

                string filter_;
                SysColl.ArrayList inclusions_;
                SysColl.ArrayList exclusions_;
            }

            public class PathFilter : IScanFilter
            {
                /// <summary>
                /// Initialise a new instance of <see cref="PathFilter"></see>.
                /// </summary>
                /// <param name="filter">The <see cref="NameFilter">filter</see> expression to apply.</param>
                public PathFilter(string filter) { nameFilter_ = new NameFilter(filter); }

                /// <summary>
                /// Test a name to see if it matches the filter.
                /// </summary>
                /// <param name="name">The name to test.</param>
                /// <returns>True if the name matches, false otherwise.</returns>
                /// <remarks><see cref="Path.GetFullPath(string)"/> is used to get the full path before matching.</remarks>
                public virtual bool IsMatch(string name)
                {
                    bool result = false;
                    if (name != null)
                    {
                        string cooked = (name.Length > 0) ? Sys.IO.Path.GetFullPath(name) : string.Empty;
                        result = nameFilter_.IsMatch(cooked);
                    }
                    return result;
                }
                NameFilter nameFilter_;
            }

            public class ExtendedPathFilter : PathFilter
            {
                /// <summary>
                /// Initialise a new instance of ExtendedPathFilter.
                /// </summary>
                /// <param name="filter">The filter to apply.</param>
                /// <param name="minSize">The minimum file size to include.</param>
                /// <param name="maxSize">The maximum file size to include.</param>
                public ExtendedPathFilter(string filter, long minSize, long maxSize)
                    : base(filter)
                {
                    MinSize = minSize;
                    MaxSize = maxSize;
                }

                /// <summary>
                /// Initialise a new instance of ExtendedPathFilter.
                /// </summary>
                /// <param name="filter">The filter to apply.</param>
                /// <param name="minDate">The minimum <see cref="DateTime"/> to include.</param>
                /// <param name="maxDate">The maximum <see cref="DateTime"/> to include.</param>
                public ExtendedPathFilter(string filter, SysDate minDate, SysDate maxDate)
                    : base(filter)
                {
                    MinDate = minDate;
                    MaxDate = maxDate;
                }

                /// <summary>
                /// Initialise a new instance of ExtendedPathFilter.
                /// </summary>
                /// <param name="filter">The filter to apply.</param>
                /// <param name="minSize">The minimum file size to include.</param>
                /// <param name="maxSize">The maximum file size to include.</param>
                /// <param name="minDate">The minimum <see cref="DateTime"/> to include.</param>
                /// <param name="maxDate">The maximum <see cref="DateTime"/> to include.</param>
                public ExtendedPathFilter(string filter, long minSize, long maxSize, SysDate minDate, SysDate maxDate)
                    : base(filter)
                {
                    MinSize = minSize;
                    MaxSize = maxSize;
                    MinDate = minDate;
                    MaxDate = maxDate;
                }

                /// <summary>
                /// Test a filename to see if it matches the filter.
                /// </summary>
                /// <param name="name">The filename to test.</param>
                /// <returns>True if the filter matches, false otherwise.</returns>
                /// <exception cref="System.IO.FileNotFoundException">The <see paramref="fileName"/> doesnt exist</exception>
                public override bool IsMatch(string name)
                {
                    bool result = base.IsMatch(name);
                    if (result)
                    {
                        Sys.IO.FileInfo fileInfo = new Sys.IO.FileInfo(name);
                        result =
                            (MinSize <= fileInfo.Length) &&
                            (MaxSize >= fileInfo.Length) &&
                            (MinDate <= fileInfo.LastWriteTime) &&
                            (MaxDate >= fileInfo.LastWriteTime)
                            ;
                    }
                    return result;
                }

                /// <summary>
                /// Get/set the minimum size/length for a file that will match this filter.
                /// </summary>
                /// <remarks>The default value is zero.</remarks>
                /// <exception cref="ArgumentOutOfRangeException">value is less than zero; greater than <see cref="MaxSize"/></exception>
                public long MinSize
                {
                    get { return minSize_; }
                    set
                    {
                        if ((value < 0) || (maxSize_ < value)) { throw new Sys.ArgumentOutOfRangeException("value"); }
                        minSize_ = value;
                    }
                }

                /// <summary>
                /// Get/set the maximum size/length for a file that will match this filter.
                /// </summary>
                /// <remarks>The default value is <see cref="System.Int64.MaxValue"/></remarks>
                /// <exception cref="ArgumentOutOfRangeException">value is less than zero or less than <see cref="MinSize"/></exception>
                public long MaxSize
                {
                    get { return maxSize_; }
                    set
                    {
                        if ((value < 0) || (minSize_ > value)) { throw new Sys.ArgumentOutOfRangeException("value"); }
                        maxSize_ = value;
                    }
                }

                /// <summary>
                /// Get/set the minimum <see cref="DateTime"/> value that will match for this filter.
                /// </summary>
                /// <remarks>Files with a LastWrite time less than this value are excluded by the filter.</remarks>
                public SysDate MinDate
                {
                    get { return minDate_; }
                    set
                    {
                        if (value > maxDate_) { throw new Sys.ArgumentOutOfRangeException("value", "Exceeds MaxDate"); }
                        minDate_ = value;
                    }
                }

                /// <summary>
                /// Get/set the maximum <see cref="DateTime"/> value that will match for this filter.
                /// </summary>
                /// <remarks>Files with a LastWrite time greater than this value are excluded by the filter.</remarks>
                public SysDate MaxDate
                {
                    get { return maxDate_; }
                    set
                    {
                        if (minDate_ > value) { throw new Sys.ArgumentOutOfRangeException("value", "Exceeds MinDate"); }
                        maxDate_ = value;
                    }
                }

                long minSize_;
                long maxSize_ = long.MaxValue;
                SysDate minDate_ = SysDate.MinValue;
                SysDate maxDate_ = SysDate.MaxValue;
            }

            [Sys.Obsolete("Use ExtendedPathFilter instead")]
            public class NameAndSizeFilter : PathFilter
            {

                /// <summary>
                /// Initialise a new instance of NameAndSizeFilter.
                /// </summary>
                /// <param name="filter">The filter to apply.</param>
                /// <param name="minSize">The minimum file size to include.</param>
                /// <param name="maxSize">The maximum file size to include.</param>
                public NameAndSizeFilter(string filter, long minSize, long maxSize)
                    : base(filter)
                {
                    MinSize = minSize;
                    MaxSize = maxSize;
                }

                /// <summary>
                /// Test a filename to see if it matches the filter.
                /// </summary>
                /// <param name="name">The filename to test.</param>
                /// <returns>True if the filter matches, false otherwise.</returns>
                public override bool IsMatch(string name)
                {
                    bool result = base.IsMatch(name);
                    if (result)
                    {
                        Sys.IO.FileInfo fileInfo = new Sys.IO.FileInfo(name);
                        long length = fileInfo.Length;
                        result = (MinSize <= length) && (MaxSize >= length);
                    }
                    return result;
                }

                /// <summary>
                /// Get/set the minimum size for a file that will match this filter.
                /// </summary>
                public long MinSize
                {
                    get { return minSize_; }
                    set
                    {
                        if ((value < 0) || (maxSize_ < value)) { throw new Sys.ArgumentOutOfRangeException("value"); }
                        minSize_ = value;
                    }
                }

                /// <summary>
                /// Get/set the maximum size for a file that will match this filter.
                /// </summary>
                public long MaxSize
                {
                    get { return maxSize_; }
                    set
                    {
                        if ((value < 0) || (minSize_ > value)) { throw new Sys.ArgumentOutOfRangeException("value"); }
                        maxSize_ = value;
                    }
                }

                long minSize_;
                long maxSize_ = long.MaxValue;
            }

            public sealed class StreamUtils
            {
                /// <summary>
                /// Read from a <see cref="Stream"/> ensuring all the required data is read.
                /// </summary>
                /// <param name="stream">The stream to read.</param>
                /// <param name="buffer">The buffer to fill.</param>
                /// <seealso cref="ReadFully(Stream,byte[],int,int)"/>
                public static void ReadFully(Sys.IO.Stream stream, byte[] buffer)
                {
                    ReadFully(stream, buffer, 0, buffer.Length);
                }

                /// <summary>
                /// Read from a <see cref="Stream"/>" ensuring all the required data is read.
                /// </summary>
                /// <param name="stream">The stream to read data from.</param>
                /// <param name="buffer">The buffer to store data in.</param>
                /// <param name="offset">The offset at which to begin storing data.</param>
                /// <param name="count">The number of bytes of data to store.</param>
                /// <exception cref="Sys.ArgumentNullException">Required parameter is null</exception>
                /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> and or <paramref name="count"/> are invalid.</exception>
                /// <exception cref="EndOfStreamException">End of stream is encountered before all the data has been read.</exception>
                public static void ReadFully(Sys.IO.Stream stream, byte[] buffer, int offset, int count)
                {
                    if (stream == null)
                    {
                        throw new Sys.ArgumentNullException("stream");
                    }

                    if (buffer == null)
                    {
                        throw new Sys.ArgumentNullException("buffer");
                    }

                    // Offset can equal length when buffer and count are 0.
                    if ((offset < 0) || (offset > buffer.Length))
                    {
                        throw new Sys.ArgumentOutOfRangeException("offset");
                    }

                    if ((count < 0) || (offset + count > buffer.Length))
                    {
                        throw new Sys.ArgumentOutOfRangeException("count");
                    }

                    while (count > 0)
                    {
                        int readCount = stream.Read(buffer, offset, count);
                        if (readCount <= 0)
                        {
                            throw new Sys.IO.EndOfStreamException();
                        }
                        offset += readCount;
                        count -= readCount;
                    }
                }

                /// <summary>
                /// Copy the contents of one <see cref="Stream"/> to another.
                /// </summary>
                /// <param name="source">The stream to source data from.</param>
                /// <param name="destination">The stream to write data to.</param>
                /// <param name="buffer">The buffer to use during copying.</param>
                public static void Copy(Sys.IO.Stream source, Sys.IO.Stream destination, byte[] buffer)
                {
                    if (source == null) { throw new Sys.ArgumentNullException("source"); }
                    if (destination == null) { throw new Sys.ArgumentNullException("destination"); }
                    if (buffer == null) { throw new Sys.ArgumentNullException("buffer"); }
                    if (buffer.Length < 128) { buffer = new byte[4096]; }
                    bool copying = true;
                    while (copying)
                    {
                        int bytesRead = source.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            destination.Write(buffer, 0, bytesRead);
                        }
                        else
                        {
                            destination.Flush();
                            copying = false;
                        }
                    }
                }

                /// <summary>
                /// Copy the contents of one <see cref="Stream"/> to another.
                /// </summary>
                /// <param name="source">The stream to source data from.</param>
                /// <param name="destination">The stream to write data to.</param>
                /// <param name="buffer">The buffer to use during copying.</param>
                /// <param name="progressHandler">The <see cref="ProgressHandler">progress handler delegate</see> to use.</param>
                /// <param name="updateInterval">The minimum <see cref="TimeSpan"/> between progress updates.</param>
                /// <param name="sender">The source for this event.</param>
                /// <param name="name">The name to use with the event.</param>
                /// <remarks>This form is specialised for use within #Zip to support events during archive operations.</remarks>
                public static void Copy(Sys.IO.Stream source, Sys.IO.Stream destination, byte[] buffer, ProgressHandler progressHandler, Sys.TimeSpan updateInterval, object sender, string name) { Copy(source, destination, buffer, progressHandler, updateInterval, sender, name, -1); }

                /// <summary>
                /// Copy the contents of one <see cref="Stream"/> to another.
                /// </summary>
                /// <param name="source">The stream to source data from.</param>
                /// <param name="destination">The stream to write data to.</param>
                /// <param name="buffer">The buffer to use during copying.</param>
                /// <param name="progressHandler">The <see cref="ProgressHandler">progress handler delegate</see> to use.</param>
                /// <param name="updateInterval">The minimum <see cref="TimeSpan"/> between progress updates.</param>
                /// <param name="sender">The source for this event.</param>
                /// <param name="name">The name to use with the event.</param>
                /// <param name="fixedTarget">A predetermined fixed target value to use with progress updates.
                /// If the value is negative the target is calculated by looking at the stream.</param>
                /// <remarks>This form is specialised for use within #Zip to support events during archive operations.</remarks>
                public static void Copy(Sys.IO.Stream source, Sys.IO.Stream destination, byte[] buffer, ProgressHandler progressHandler, Sys.TimeSpan updateInterval, object sender, string name, long fixedTarget)
                {
                    if (source == null)
                    {
                        throw new Sys.ArgumentNullException("source");
                    }

                    if (destination == null)
                    {
                        throw new Sys.ArgumentNullException("destination");
                    }

                    if (buffer == null)
                    {
                        throw new Sys.ArgumentNullException("buffer");
                    }

                    // Ensure a reasonable size of buffer is used without being prohibitive.
                    if (buffer.Length < 128)
                    {
                        throw new Sys.ArgumentException("Buffer is too small", "buffer");
                    }

                    if (progressHandler == null)
                    {
                        throw new Sys.ArgumentNullException("progressHandler");
                    }

                    bool copying = true;

                    SysDate marker = SysDate.Now;
                    long processed = 0;
                    long target = 0;

                    if (fixedTarget >= 0)
                    {
                        target = fixedTarget;
                    }
                    else if (source.CanSeek)
                    {
                        target = source.Length - source.Position;
                    }

                    // Always fire 0% progress..
                    ProgressEventArgs args = new ProgressEventArgs(name, processed, target);
                    progressHandler(sender, args);

                    bool progressFired = true;

                    while (copying)
                    {
                        int bytesRead = source.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            processed += bytesRead;
                            progressFired = false;
                            destination.Write(buffer, 0, bytesRead);
                        }
                        else
                        {
                            destination.Flush();
                            copying = false;
                        }

                        if (SysDate.Now - marker > updateInterval)
                        {
                            progressFired = true;
                            marker = SysDate.Now;
                            args = new ProgressEventArgs(name, processed, target);
                            progressHandler(sender, args);

                            copying = args.ContinueRunning;
                        }
                    }

                    if (!progressFired)
                    {
                        args = new ProgressEventArgs(name, processed, target);
                        progressHandler(sender, args);
                    }
                }

                /// <summary>
                /// Initialise an instance of <see cref="StreamUtils"></see>
                /// </summary>
                private StreamUtils() { /* Do nothing. */ }
            }

            public abstract class WindowsPathUtils
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="WindowsPathUtils"/> class.
                /// </summary>
                internal WindowsPathUtils() { /* NOTHING */ }

                /// <summary>
                /// Remove any path root present in the path
                /// </summary>
                /// <param name="path">A <see cref="string"/> containing path information.</param>
                /// <returns>The path with the root removed if it was present; path otherwise.</returns>
                /// <remarks>Unlike the <see cref="System.IO.Path"/> class the path isnt otherwise checked for validity.</remarks>
                public static string DropPathRoot(string path)
                {
                    string result = path;
                    if ((path != null) && (path.Length > 0))
                    {
                        if ((path[0] == '\\') || (path[0] == '/'))
                        {
                            // UNC name ?
                            if ((path.Length > 1) && ((path[1] == '\\') || (path[1] == '/')))
                            {
                                int index = 2;
                                int elements = 2;
                                // Scan for two separate elements \\machine\share\restofpath
                                while ((index <= path.Length) && (((path[index] != '\\') && (path[index] != '/')) || (--elements > 0))) { index++; }
                                index++;
                                if (index < path.Length) { result = path.Substring(index); } else { result = string.Empty; }
                            }
                        }
                        else if ((path.Length > 1) && (path[1] == ':'))
                        {
                            int dropCount = 2;
                            if ((path.Length > 2) && ((path[2] == '\\') || (path[2] == '/'))) { dropCount = 3; }
                            result = result.Remove(0, dropCount);
                        }
                    }
                    return result;
                }
            }
        }

        namespace Checksums
        {
            public sealed class Adler32 : ZIPLib.Checksums.IChecksum
            {
                /// <summary>
                /// largest prime smaller than 65536
                /// </summary>
                const uint BASE = 65521;

                /// <summary>
                /// Returns the Adler32 data checksum computed so far.
                /// </summary>
                public long Value { get { return this.checksum; } }

                /// <summary>
                /// Creates a new instance of the Adler32 class.
                /// The checksum starts off with a value of 1.
                /// </summary>
                public Adler32() { this.Reset(); }

                /// <summary>
                /// Resets the Adler32 checksum to the initial value.
                /// </summary>
                public void Reset() { this.checksum = 1; }

                /// <summary>
                /// Updates the checksum with a byte value.
                /// </summary>
                /// <param name="value">
                /// The data value to add. The high byte of the int is ignored.
                /// </param>
                public void Update(int value)
                {
                    // We could make a length 1 byte array and call update again, but I would rather not have that overhead
                    uint s1 = checksum & 0xFFFF;
                    uint s2 = checksum >> 16;
                    s1 = (s1 + ((uint)value & 0xFF)) % BASE;
                    s2 = (s1 + s2) % BASE;
                    checksum = (s2 << 16) + s1;
                }

                /// <summary>
                /// Updates the checksum with an array of bytes.
                /// </summary>
                /// <param name="buffer">
                /// The source of the data to update with.
                /// </param>
                public void Update(byte[] buffer)
                {
                    if (buffer == null) { throw new Sys.ArgumentNullException("buffer"); }
                    this.Update(buffer, 0, buffer.Length);
                }

                /// <summary>
                /// Updates the checksum with the bytes taken from the array.
                /// </summary>
                /// <param name="buffer">
                /// an array of bytes
                /// </param>
                /// <param name="offset">
                /// the start of the data used for this update
                /// </param>
                /// <param name="count">
                /// the number of bytes to use for this update
                /// </param>
                public void Update(byte[] buffer, int offset, int count)
                {
                    if (buffer == null) { throw new Sys.ArgumentNullException("buffer"); }
                    if (offset < 0) { throw new Sys.ArgumentOutOfRangeException("offset", "cannot be negative"); }
                    if (count < 0) { throw new Sys.ArgumentOutOfRangeException("count", "cannot be negative"); }
                    if (offset >= buffer.Length) { throw new Sys.ArgumentOutOfRangeException("offset", "not a valid index into buffer"); }
                    if (offset + count > buffer.Length) { throw new Sys.ArgumentOutOfRangeException("count", "exceeds buffer size"); }
                    uint s1 = checksum & 0xFFFF; //(By Per Bothner)
                    uint s2 = checksum >> 16;
                    while (count > 0)
                    {
                        // We can defer the module operation: s1 maximally grows from 65521 to 65521 + 255 * 3800 s2 maximally grows by 3800 * median(s1) = 2090079800 < 2^31
                        int n = 3800;
                        if (n > count) { n = count; }
                        count -= n;
                        while (--n >= 0)
                        {
                            s1 = s1 + (uint)(buffer[offset++] & 0xff);
                            s2 = s2 + s1;
                        }
                        s1 %= BASE;
                        s2 %= BASE;
                    }
                    checksum = (s2 << 16) | s1;
                }
                uint checksum;
            }

            public sealed class Crc32 : ZIPLib.Checksums.IChecksum
            {
                const uint CrcSeed = 0xFFFFFFFF;

                static readonly uint[] CrcTable = new uint[]
        {
			0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419,
			0x706AF48F, 0xE963A535, 0x9E6495A3, 0x0EDB8832, 0x79DCB8A4,
			0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07,
			0x90BF1D91, 0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE,
			0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856,
			0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9,
			0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4,
			0xA2677172, 0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B,
			0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3,
			0x45DF5C75, 0xDCD60DCF, 0xABD13D59, 0x26D930AC, 0x51DE003A,
			0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423, 0xCFBA9599,
			0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924,
			0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D, 0x76DC4190,
			0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F,
			0x9FBFE4A5, 0xE8B8D433, 0x7807C9A2, 0x0F00F934, 0x9609A88E,
			0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01,
			0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED,
			0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950,
			0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3,
			0xFBD44C65, 0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2,
			0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A,
			0x346ED9FC, 0xAD678846, 0xDA60B8D0, 0x44042D73, 0x33031DE5,
			0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA, 0xBE0B1010,
			0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F,
			0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17,
			0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6,
			0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615,
			0x73DC1683, 0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8,
			0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1, 0xF00F9344,
			0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB,
			0x196C3671, 0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A,
			0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5,
			0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1,
			0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C,
			0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF,
			0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
			0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE,
			0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31,
			0x2CD99E8B, 0x5BDEAE1D, 0x9B64C2B0, 0xEC63F226, 0x756AA39C,
			0x026D930A, 0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713,
			0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B,
			0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242,
			0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1,
			0x18B74777, 0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C,
			0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45, 0xA00AE278,
			0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7,
			0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC, 0x40DF0B66,
			0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
			0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605,
			0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8,
			0x5D681B02, 0x2A6F2B94, 0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B,
			0x2D02EF8D
		};

                internal static uint ComputeCrc32(uint oldCrc, byte value) { return (uint)(Crc32.CrcTable[(oldCrc ^ value) & 0xFF] ^ (oldCrc >> 8)); }

                /// <summary>
                /// The crc data checksum so far.
                /// </summary>
                uint crc;

                /// <summary>
                /// Returns the CRC32 data checksum computed so far.
                /// </summary>
                public long Value { get { return (long)crc; } set { crc = (uint)value; } }

                /// <summary>
                /// Resets the CRC32 data checksum as if no update was ever called.
                /// </summary>
                public void Reset() { crc = 0; }

                /// <summary>
                /// Updates the checksum with the int bval.
                /// </summary>
                /// <param name = "value">
                /// the byte is taken as the lower 8 bits of value
                /// </param>
                public void Update(int value)
                {
                    crc ^= CrcSeed;
                    crc = CrcTable[(crc ^ value) & 0xFF] ^ (crc >> 8);
                    crc ^= CrcSeed;
                }

                /// <summary>
                /// Updates the checksum with the bytes taken from the array.
                /// </summary>
                /// <param name="buffer">
                /// buffer an array of bytes
                /// </param>
                public void Update(byte[] buffer)
                {
                    if (buffer == null) { throw new Sys.ArgumentNullException("buffer"); }
                    Update(buffer, 0, buffer.Length);
                }

                /// <summary>
                /// Adds the byte array to the data checksum.
                /// </summary>
                /// <param name = "buffer">
                /// The buffer which contains the data
                /// </param>
                /// <param name = "offset">
                /// The offset in the buffer where the data starts
                /// </param>
                /// <param name = "count">
                /// The number of data bytes to update the CRC with.
                /// </param>
                public void Update(byte[] buffer, int offset, int count)
                {
                    if (buffer == null) { throw new Sys.ArgumentNullException("buffer"); }
                    if (count < 0) { throw new Sys.ArgumentOutOfRangeException("count", "Count cannot be less than zero"); }
                    if (offset < 0 || offset + count > buffer.Length) { throw new Sys.ArgumentOutOfRangeException("offset"); }
                    crc ^= CrcSeed;
                    while (--count >= 0) { crc = CrcTable[(crc ^ buffer[offset++]) & 0xFF] ^ (crc >> 8); }
                    crc ^= CrcSeed;
                }
            }

            public interface IChecksum
            {
                /// <summary>
                /// Returns the data checksum computed so far.
                /// </summary>
                long Value { get; }

                /// <summary>
                /// Resets the data checksum as if no update was ever called.
                /// </summary>
                void Reset();

                /// <summary>
                /// Adds one byte to the data checksum.
                /// </summary>
                /// <param name = "value">
                /// the data value to add. The high byte of the int is ignored.
                /// </param>
                void Update(int value);

                /// <summary>
                /// Updates the data checksum with the bytes taken from the array.
                /// </summary>
                /// <param name="buffer">
                /// buffer an array of bytes
                /// </param>
                void Update(byte[] buffer);

                /// <summary>
                /// Adds the byte array to the data checksum.
                /// </summary>
                /// <param name = "buffer">
                /// The buffer which contains the data
                /// </param>
                /// <param name = "offset">
                /// The offset in the buffer where the data starts
                /// </param>
                /// <param name = "count">
                /// the number of data bytes to add.
                /// </param>
                void Update(byte[] buffer, int offset, int count);
            }
        }

        namespace Encryption
        {
            public abstract class PkzipClassic : Sys.Security.Cryptography.SymmetricAlgorithm
            {
                /// <summary>
                /// Generates new encryption keys based on given seed
                /// </summary>
                /// <param name="seed">The seed value to initialise keys with.</param>
                /// <returns>A new key value.</returns>
                public static byte[] GenerateKeys(byte[] seed)
                {
                    if (seed == null) { throw new Sys.ArgumentNullException("seed"); }
                    if (seed.Length == 0) { throw new Sys.ArgumentException("Length is zero", "seed"); }
                    uint[] newKeys = new uint[] { 0x12345678, 0x23456789, 0x34567890 };
                    for (int i = 0; i < seed.Length; ++i)
                    {
                        newKeys[0] = ZIPLib.Checksums.Crc32.ComputeCrc32(newKeys[0], seed[i]);
                        newKeys[1] = newKeys[1] + (byte)newKeys[0];
                        newKeys[1] = newKeys[1] * 134775813 + 1;
                        newKeys[2] = ZIPLib.Checksums.Crc32.ComputeCrc32(newKeys[2], (byte)(newKeys[1] >> 24));
                    }

                    byte[] result = new byte[12];
                    result[0] = (byte)(newKeys[0] & 0xff);
                    result[1] = (byte)((newKeys[0] >> 8) & 0xff);
                    result[2] = (byte)((newKeys[0] >> 16) & 0xff);
                    result[3] = (byte)((newKeys[0] >> 24) & 0xff);
                    result[4] = (byte)(newKeys[1] & 0xff);
                    result[5] = (byte)((newKeys[1] >> 8) & 0xff);
                    result[6] = (byte)((newKeys[1] >> 16) & 0xff);
                    result[7] = (byte)((newKeys[1] >> 24) & 0xff);
                    result[8] = (byte)(newKeys[2] & 0xff);
                    result[9] = (byte)((newKeys[2] >> 8) & 0xff);
                    result[10] = (byte)((newKeys[2] >> 16) & 0xff);
                    result[11] = (byte)((newKeys[2] >> 24) & 0xff);
                    return result;
                }
            }

            internal class PkzipClassicCryptoBase
            {
                /// <summary>
                /// Transform a single byte 
                /// </summary>
                /// <returns>
                /// The transformed value
                /// </returns>
                protected byte TransformByte()
                {
                    uint temp = ((keys[2] & 0xFFFF) | 2);
                    return (byte)((temp * (temp ^ 1)) >> 8);
                }

                /// <summary>
                /// Set the key schedule for encryption/decryption.
                /// </summary>
                /// <param name="keyData">The data use to set the keys from.</param>
                protected void SetKeys(byte[] keyData)
                {
                    if (keyData == null)
                    {
                        throw new Sys.ArgumentNullException("keyData");
                    }

                    if (keyData.Length != 12)
                    {
                        throw new Sys.InvalidOperationException("Key length is not valid");
                    }

                    keys = new uint[3];
                    keys[0] = (uint)((keyData[3] << 24) | (keyData[2] << 16) | (keyData[1] << 8) | keyData[0]);
                    keys[1] = (uint)((keyData[7] << 24) | (keyData[6] << 16) | (keyData[5] << 8) | keyData[4]);
                    keys[2] = (uint)((keyData[11] << 24) | (keyData[10] << 16) | (keyData[9] << 8) | keyData[8]);
                }

                /// <summary>
                /// Update encryption keys 
                /// </summary>		
                protected void UpdateKeys(byte ch)
                {
                    keys[0] = ZIPLib.Checksums.Crc32.ComputeCrc32(keys[0], ch);
                    keys[1] = keys[1] + (byte)keys[0];
                    keys[1] = keys[1] * 134775813 + 1;
                    keys[2] = ZIPLib.Checksums.Crc32.ComputeCrc32(keys[2], (byte)(keys[1] >> 24));
                }

                /// <summary>
                /// Reset the internal state.
                /// </summary>
                protected void Reset()
                {
                    keys[0] = 0;
                    keys[1] = 0;
                    keys[2] = 0;
                }

                uint[] keys;
            }

            internal class PkzipClassicEncryptCryptoTransform : PkzipClassicCryptoBase, Sys.Security.Cryptography.ICryptoTransform
            {
                /// <summary>
                /// Initialise a new instance of <see cref="PkzipClassicEncryptCryptoTransform"></see>
                /// </summary>
                /// <param name="keyBlock">The key block to use.</param>
                internal PkzipClassicEncryptCryptoTransform(byte[] keyBlock)
                {
                    SetKeys(keyBlock);
                }

                /// <summary>
                /// Transforms the specified region of the specified byte array.
                /// </summary>
                /// <param name="inputBuffer">The input for which to compute the transform.</param>
                /// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
                /// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
                /// <returns>The computed transform.</returns>
                public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
                {
                    byte[] result = new byte[inputCount];
                    TransformBlock(inputBuffer, inputOffset, inputCount, result, 0);
                    return result;
                }

                /// <summary>
                /// Transforms the specified region of the input byte array and copies 
                /// the resulting transform to the specified region of the output byte array.
                /// </summary>
                /// <param name="inputBuffer">The input for which to compute the transform.</param>
                /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
                /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
                /// <param name="outputBuffer">The output to which to write the transform.</param>
                /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
                /// <returns>The number of bytes written.</returns>
                public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
                {
                    for (int i = inputOffset; i < inputOffset + inputCount; ++i)
                    {
                        byte oldbyte = inputBuffer[i];
                        outputBuffer[outputOffset++] = (byte)(inputBuffer[i] ^ TransformByte());
                        UpdateKeys(oldbyte);
                    }
                    return inputCount;
                }

                /// <summary>
                /// Gets a value indicating whether the current transform can be reused.
                /// </summary>
                public bool CanReuseTransform { get { return true; } }

                /// <summary>
                /// Gets the size of the input data blocks in bytes.
                /// </summary>
                public int InputBlockSize { get { return 1; } }

                /// <summary>
                /// Gets the size of the output data blocks in bytes.
                /// </summary>
                public int OutputBlockSize { get { return 1; } }

                /// <summary>
                /// Gets a value indicating whether multiple blocks can be transformed.
                /// </summary>
                public bool CanTransformMultipleBlocks { get { return true; } }

                /// <summary>
                /// Cleanup internal state.
                /// </summary>
                public void Dispose() { Reset(); }
            }

            internal class PkzipClassicDecryptCryptoTransform : PkzipClassicCryptoBase, Sys.Security.Cryptography.ICryptoTransform
            {
                /// <summary>
                /// Initialise a new instance of <see cref="PkzipClassicDecryptCryptoTransform"></see>.
                /// </summary>
                /// <param name="keyBlock">The key block to decrypt with.</param>
                internal PkzipClassicDecryptCryptoTransform(byte[] keyBlock)
                {
                    SetKeys(keyBlock);
                }

                /// <summary>
                /// Transforms the specified region of the specified byte array.
                /// </summary>
                /// <param name="inputBuffer">The input for which to compute the transform.</param>
                /// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
                /// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
                /// <returns>The computed transform.</returns>
                public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
                {
                    byte[] result = new byte[inputCount];
                    TransformBlock(inputBuffer, inputOffset, inputCount, result, 0);
                    return result;
                }

                /// <summary>
                /// Transforms the specified region of the input byte array and copies 
                /// the resulting transform to the specified region of the output byte array.
                /// </summary>
                /// <param name="inputBuffer">The input for which to compute the transform.</param>
                /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
                /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
                /// <param name="outputBuffer">The output to which to write the transform.</param>
                /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
                /// <returns>The number of bytes written.</returns>
                public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
                {
                    for (int i = inputOffset; i < inputOffset + inputCount; ++i)
                    {
                        byte newByte = (byte)(inputBuffer[i] ^ TransformByte());
                        outputBuffer[outputOffset++] = newByte;
                        UpdateKeys(newByte);
                    }
                    return inputCount;
                }

                /// <summary>
                /// Gets a value indicating whether the current transform can be reused.
                /// </summary>
                public bool CanReuseTransform
                {
                    get
                    {
                        return true;
                    }
                }

                /// <summary>
                /// Gets the size of the input data blocks in bytes.
                /// </summary>
                public int InputBlockSize
                {
                    get
                    {
                        return 1;
                    }
                }

                /// <summary>
                /// Gets the size of the output data blocks in bytes.
                /// </summary>
                public int OutputBlockSize
                {
                    get
                    {
                        return 1;
                    }
                }

                /// <summary>
                /// Gets a value indicating whether multiple blocks can be transformed.
                /// </summary>
                public bool CanTransformMultipleBlocks
                {
                    get
                    {
                        return true;
                    }
                }

                /// <summary>
                /// Cleanup internal state.
                /// </summary>
                public void Dispose()
                {
                    Reset();
                }
            }

            public sealed class PkzipClassicManaged : PkzipClassic
            {
                /// <summary>
                /// Get / set the applicable block size in bits.
                /// </summary>
                /// <remarks>The only valid block size is 8.</remarks>
                public override int BlockSize
                {
                    get
                    {
                        return 8;
                    }

                    set
                    {
                        if (value != 8)
                        {
                            throw new Sys.Security.Cryptography.CryptographicException("Block size is invalid");
                        }
                    }
                }

                /// <summary>
                /// Get an array of legal <see cref="KeySizes">key sizes.</see>
                /// </summary>
                public override Sys.Security.Cryptography.KeySizes[] LegalKeySizes
                {
                    get
                    {
                        Sys.Security.Cryptography.KeySizes[] keySizes = new Sys.Security.Cryptography.KeySizes[1];
                        keySizes[0] = new Sys.Security.Cryptography.KeySizes(12 * 8, 12 * 8, 0);
                        return keySizes;
                    }
                }

                /// <summary>
                /// Generate an initial vector.
                /// </summary>
                public override void GenerateIV()
                {
                    // Do nothing.
                }

                /// <summary>
                /// Get an array of legal <see cref="KeySizes">block sizes</see>.
                /// </summary>
                public override Sys.Security.Cryptography.KeySizes[] LegalBlockSizes
                {
                    get
                    {
                        Sys.Security.Cryptography.KeySizes[] keySizes = new Sys.Security.Cryptography.KeySizes[1];
                        keySizes[0] = new Sys.Security.Cryptography.KeySizes(1 * 8, 1 * 8, 0);
                        return keySizes;
                    }
                }

                /// <summary>
                /// Get / set the key value applicable.
                /// </summary>
                public override byte[] Key
                {
                    get
                    {
                        if (key_ == null)
                        {
                            GenerateKey();
                        }

                        return (byte[])key_.Clone();
                    }

                    set
                    {
                        if (value == null)
                        {
                            throw new Sys.ArgumentNullException("value");
                        }

                        if (value.Length != 12)
                        {
                            throw new Sys.Security.Cryptography.CryptographicException("Key size is illegal");
                        }

                        key_ = (byte[])value.Clone();
                    }
                }

                /// <summary>
                /// Generate a new random key.
                /// </summary>
                public override void GenerateKey()
                {
                    key_ = new byte[12];
                    Sys.Randomizer rnd = new Sys.Randomizer();
                    rnd.NextBytes(key_);
                }

                /// <summary>
                /// Create an encryptor.
                /// </summary>
                /// <param name="rgbKey">The key to use for this encryptor.</param>
                /// <param name="rgbIV">Initialisation vector for the new encryptor.</param>
                /// <returns>Returns a new PkzipClassic encryptor</returns>
                public override Sys.Security.Cryptography.ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
                {
                    key_ = rgbKey;
                    return new PkzipClassicEncryptCryptoTransform(Key);
                }

                /// <summary>
                /// Create a decryptor.
                /// </summary>
                /// <param name="rgbKey">Keys to use for this new decryptor.</param>
                /// <param name="rgbIV">Initialisation vector for the new decryptor.</param>
                /// <returns>Returns a new decryptor.</returns>
                public override Sys.Security.Cryptography.ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
                {
                    key_ = rgbKey;
                    return new PkzipClassicDecryptCryptoTransform(Key);
                }

                byte[] key_;
            }

            internal class ZipAESStream : Sys.Security.Cryptography.CryptoStream
            {

                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="stream">The stream on which to perform the cryptographic transformation.</param>
                /// <param name="transform">Instance of ZipAESTransform</param>
                /// <param name="mode">Read or Write</param>
                public ZipAESStream(Sys.IO.Stream stream, ZipAESTransform transform, Sys.Security.Cryptography.CryptoStreamMode mode)
                    : base(stream, transform, mode)
                {
                    _stream = stream;
                    _transform = transform;
                    _slideBuffer = new byte[1024];
                    _blockAndAuth = CRYPTO_BLOCK_SIZE + AUTH_CODE_LENGTH;

                    // mode:
                    //  CryptoStreamMode.Read means we read from "stream" and pass decrypted to our Read() method.
                    //  Write bypasses this stream and uses the Transform directly.
                    if (mode != Sys.Security.Cryptography.CryptoStreamMode.Read)
                    {
                        throw new Sys.Exception("ZipAESStream only for read");
                    }
                }

                // The final n bytes of the AES stream contain the Auth Code.
                private const int AUTH_CODE_LENGTH = 10;

                private Sys.IO.Stream _stream;
                private ZipAESTransform _transform;
                private byte[] _slideBuffer;
                private int _slideBufStartPos;
                private int _slideBufFreePos;
                // Blocksize is always 16 here, even for AES-256 which has transform.InputBlockSize of 32.
                private const int CRYPTO_BLOCK_SIZE = 16;
                private int _blockAndAuth;

                /// <summary>
                /// Reads a sequence of bytes from the current CryptoStream into buffer,
                /// and advances the position within the stream by the number of bytes read.
                /// </summary>
                public override int Read(byte[] outBuffer, int offset, int count)
                {
                    int nBytes = 0;
                    while (nBytes < count)
                    {
                        // Calculate buffer quantities vs read-ahead size, and check for sufficient free space
                        int byteCount = _slideBufFreePos - _slideBufStartPos;

                        // Need to handle final block and Auth Code specially, but don't know total data length.
                        // Maintain a read-ahead equal to the length of (crypto block + Auth Code). 
                        // When that runs out we can detect these final sections.
                        int lengthToRead = _blockAndAuth - byteCount;
                        if (_slideBuffer.Length - _slideBufFreePos < lengthToRead)
                        {
                            // Shift the data to the beginning of the buffer
                            int iTo = 0;
                            for (int iFrom = _slideBufStartPos; iFrom < _slideBufFreePos; iFrom++, iTo++)
                            {
                                _slideBuffer[iTo] = _slideBuffer[iFrom];
                            }
                            _slideBufFreePos -= _slideBufStartPos;		// Note the -=
                            _slideBufStartPos = 0;
                        }
                        int obtained = _stream.Read(_slideBuffer, _slideBufFreePos, lengthToRead);
                        _slideBufFreePos += obtained;

                        // Recalculate how much data we now have
                        byteCount = _slideBufFreePos - _slideBufStartPos;
                        if (byteCount >= _blockAndAuth)
                        {
                            // At least a 16 byte block and an auth code remains.
                            _transform.TransformBlock(_slideBuffer, _slideBufStartPos, CRYPTO_BLOCK_SIZE, outBuffer, offset);
                            nBytes += CRYPTO_BLOCK_SIZE;
                            offset += CRYPTO_BLOCK_SIZE;
                            _slideBufStartPos += CRYPTO_BLOCK_SIZE;
                        }
                        else
                        {
                            // Last round.
                            if (byteCount > AUTH_CODE_LENGTH)
                            {
                                // At least one byte of data plus auth code
                                int finalBlock = byteCount - AUTH_CODE_LENGTH;
                                _transform.TransformBlock(_slideBuffer, _slideBufStartPos, finalBlock, outBuffer, offset);
                                nBytes += finalBlock;
                                _slideBufStartPos += finalBlock;
                            }
                            else if (byteCount < AUTH_CODE_LENGTH)
                                throw new Sys.Exception("Internal error missed auth code");	// Coding bug
                            // Final block done. Check Auth code.
                            byte[] calcAuthCode = _transform.GetAuthCode();
                            for (int i = 0; i < AUTH_CODE_LENGTH; i++)
                            {
                                if (calcAuthCode[i] != _slideBuffer[_slideBufStartPos + i])
                                {
                                    throw new Sys.Exception("AES Authentication Code does not match. This is a super-CRC check on the data in the file after compression and encryption. \r\n"
                                        + "The file may be damaged.");
                                }
                            }

                            break;	// Reached the auth code
                        }
                    }
                    return nBytes;
                }

                /// <summary>
                /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
                /// </summary>
                /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream. </param>
                /// <param name="offset">The byte offset in buffer at which to begin copying bytes to the current stream. </param>
                /// <param name="count">The number of bytes to be written to the current stream. </param>
                public override void Write(byte[] buffer, int offset, int count)
                {
                    // ZipAESStream is used for reading but not for writing. Writing uses the ZipAESTransform directly.
                    throw new Sys.NotImplementedException();
                }
            }

            internal class ZipAESTransform : Sys.Security.Cryptography.ICryptoTransform
            {
                private const int PWD_VER_LENGTH = 2;

                // WinZip use iteration count of 1000 for PBKDF2 key generation
                private const int KEY_ROUNDS = 1000;

                // For 128-bit AES (16 bytes) the encryption is implemented as expected.
                // For 256-bit AES (32 bytes) WinZip do full 256 bit AES of the nonce to create the encryption
                // block but use only the first 16 bytes of it, and discard the second half.
                private const int ENCRYPT_BLOCK = 16;

                private int _blockSize;
                private Sys.Security.Cryptography.ICryptoTransform _encryptor;
                private readonly byte[] _counterNonce;
                private byte[] _encryptBuffer;
                private int _encrPos;
                private byte[] _pwdVerifier;
                private Sys.Security.Cryptography.HMACSHA1 _hmacsha1;
                private bool _finalised;

                private bool _writeMode;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="key">Password string</param>
                /// <param name="saltBytes">Random bytes, length depends on encryption strength.
                /// 128 bits = 8 bytes, 192 bits = 12 bytes, 256 bits = 16 bytes.</param>
                /// <param name="blockSize">The encryption strength, in bytes eg 16 for 128 bits.</param>
                /// <param name="writeMode">True when creating a zip, false when reading. For the AuthCode.</param>
                ///
                public ZipAESTransform(string key, byte[] saltBytes, int blockSize, bool writeMode)
                {

                    if (blockSize != 16 && blockSize != 32)	// 24 valid for AES but not supported by Winzip
                        throw new Sys.Exception("Invalid blocksize " + blockSize + ". Must be 16 or 32.");
                    if (saltBytes.Length != blockSize / 2)
                        throw new Sys.Exception("Invalid salt len. Must be " + blockSize / 2 + " for blocksize " + blockSize);
                    // initialise the encryption buffer and buffer pos
                    _blockSize = blockSize;
                    _encryptBuffer = new byte[_blockSize];
                    _encrPos = ENCRYPT_BLOCK;

                    // Performs the equivalent of derive_key in Dr Brian Gladman's pwd2key.c
                    Sys.Security.Cryptography.Rfc2898DeriveBytes pdb = new Sys.Security.Cryptography.Rfc2898DeriveBytes(key, saltBytes, KEY_ROUNDS);
                    Sys.Security.Cryptography.RijndaelManaged rm = new Sys.Security.Cryptography.RijndaelManaged();
                    rm.Mode = Sys.Security.Cryptography.CipherMode.ECB;			// No feedback from cipher for CTR mode
                    _counterNonce = new byte[_blockSize];
                    byte[] byteKey1 = pdb.GetBytes(_blockSize);
                    byte[] byteKey2 = pdb.GetBytes(_blockSize);
                    _encryptor = rm.CreateEncryptor(byteKey1, byteKey2);
                    _pwdVerifier = pdb.GetBytes(PWD_VER_LENGTH);
                    //
                    _hmacsha1 = new Sys.Security.Cryptography.HMACSHA1(byteKey2);
                    _writeMode = writeMode;
                }

                /// <summary>
                /// Implement the ICryptoTransform method.
                /// </summary>
                public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
                {

                    // Pass the data stream to the hash algorithm for generating the Auth Code.
                    // This does not change the inputBuffer. Do this before decryption for read mode.
                    if (!_writeMode)
                    {
                        _hmacsha1.TransformBlock(inputBuffer, inputOffset, inputCount, inputBuffer, inputOffset);
                    }
                    // Encrypt with AES in CTR mode. Regards to Dr Brian Gladman for this.
                    int ix = 0;
                    while (ix < inputCount)
                    {
                        if (_encrPos == ENCRYPT_BLOCK)
                        {
                            /* increment encryption nonce   */
                            int j = 0;
                            while (++_counterNonce[j] == 0)
                            {
                                ++j;
                            }
                            /* encrypt the nonce to form next xor buffer    */
                            _encryptor.TransformBlock(_counterNonce, 0, _blockSize, _encryptBuffer, 0);
                            _encrPos = 0;
                        }
                        outputBuffer[ix + outputOffset] = (byte)(inputBuffer[ix + inputOffset] ^ _encryptBuffer[_encrPos++]);
                        //
                        ix++;
                    }
                    if (_writeMode)
                    {
                        // This does not change the buffer. 
                        _hmacsha1.TransformBlock(outputBuffer, outputOffset, inputCount, outputBuffer, outputOffset);
                    }
                    return inputCount;
                }

                /// <summary>
                /// Returns the 2 byte password verifier
                /// </summary>
                public byte[] PwdVerifier
                {
                    get
                    {
                        return _pwdVerifier;
                    }
                }

                /// <summary>
                /// Returns the 10 byte AUTH CODE to be checked or appended immediately following the AES data stream.
                /// </summary>
                public byte[] GetAuthCode()
                {
                    // We usually don't get advance notice of final block. Hash requres a TransformFinal.
                    if (!_finalised)
                    {
                        byte[] dummy = new byte[0];
                        _hmacsha1.TransformFinalBlock(dummy, 0, 0);
                        _finalised = true;
                    }
                    return _hmacsha1.Hash;
                }

                /// <summary>
                /// Not implemented.
                /// </summary>
                public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) { throw new Sys.NotImplementedException("ZipAESTransform.TransformFinalBlock"); }

                /// <summary>
                /// Gets the size of the input data blocks in bytes.
                /// </summary>
                public int InputBlockSize
                {
                    get
                    {
                        return _blockSize;
                    }
                }

                /// <summary>
                /// Gets the size of the output data blocks in bytes.
                /// </summary>
                public int OutputBlockSize
                {
                    get
                    {
                        return _blockSize;
                    }
                }

                /// <summary>
                /// Gets a value indicating whether multiple blocks can be transformed.
                /// </summary>
                public bool CanTransformMultipleBlocks
                {
                    get
                    {
                        return true;
                    }
                }

                /// <summary>
                /// Gets a value indicating whether the current transform can be reused.
                /// </summary>
                public bool CanReuseTransform
                {
                    get
                    {
                        return true;
                    }
                }

                /// <summary>
                /// Cleanup internal state.
                /// </summary>
                public void Dispose()
                {
                    this._encryptor.Dispose();
                    this._hmacsha1.Clear();
                }
            }
        }

        namespace Compression
        {
            public class PendingBuffer
            {
                byte[] buffer_;
                int start;
                int end;
                uint bits;
                int bitCount;

                public PendingBuffer() : this(4096) { /* NOTHING */ }

                public PendingBuffer(int bufferSize)
                {
                    buffer_ = new byte[bufferSize];
                }

                public void Reset()
                {
                    start = end = bitCount = 0;
                }

                public void WriteByte(int value)
                {
                    buffer_[end++] = unchecked((byte)value);
                }

                public void WriteShort(int value)
                {
                    buffer_[end++] = unchecked((byte)value);
                    buffer_[end++] = unchecked((byte)(value >> 8));
                }

                public void WriteInt(int value)
                {
                    buffer_[end++] = unchecked((byte)value);
                    buffer_[end++] = unchecked((byte)(value >> 8));
                    buffer_[end++] = unchecked((byte)(value >> 16));
                    buffer_[end++] = unchecked((byte)(value >> 24));
                }

                public void WriteBlock(byte[] block, int offset, int length)
                {
                    Sys.Array.Copy(block, offset, buffer_, end, length);
                    end += length;
                }

                public int BitCount
                {
                    get
                    {
                        return bitCount;
                    }
                }

                public void AlignToByte()
                {
                    if (bitCount > 0)
                    {
                        buffer_[end++] = unchecked((byte)bits);
                        if (bitCount > 8)
                        {
                            buffer_[end++] = unchecked((byte)(bits >> 8));
                        }
                    }
                    bits = 0;
                    bitCount = 0;
                }

                public void WriteBits(int b, int count)
                {
                    bits |= (uint)(b << bitCount);
                    bitCount += count;
                    if (bitCount >= 16)
                    {
                        buffer_[end++] = unchecked((byte)bits);
                        buffer_[end++] = unchecked((byte)(bits >> 8));
                        bits >>= 16;
                        bitCount -= 16;
                    }
                }

                public void WriteShortMSB(int s)
                {
                    buffer_[end++] = unchecked((byte)(s >> 8));
                    buffer_[end++] = unchecked((byte)s);
                }

                public bool IsFlushed
                {
                    get
                    {
                        return end == 0;
                    }
                }

                public int Flush(byte[] output, int offset, int length)
                {
                    if (bitCount >= 8)
                    {
                        buffer_[end++] = unchecked((byte)bits);
                        bits >>= 8;
                        bitCount -= 8;
                    }

                    if (length > end - start)
                    {
                        length = end - start;
                        Sys.Array.Copy(buffer_, start, output, offset, length);
                        start = 0;
                        end = 0;
                    }
                    else
                    {
                        Sys.Array.Copy(buffer_, start, output, offset, length);
                        start += length;
                    }
                    return length;
                }

                public byte[] ToByteArray()
                {
                    byte[] result = new byte[end - start];
                    Sys.Array.Copy(buffer_, start, result, 0, result.Length);
                    start = 0;
                    end = 0;
                    return result;
                }
            }

            public class Deflater
            {
                /// <summary>
                /// The best and slowest compression level.  This tries to find very
                /// long and distant string repetitions.
                /// </summary>
                public const int BEST_COMPRESSION = 9;

                /// <summary>
                /// The worst but fastest compression level.
                /// </summary>
                public const int BEST_SPEED = 1;

                /// <summary>
                /// The default compression level.
                /// </summary>
                public const int DEFAULT_COMPRESSION = -1;

                /// <summary>
                /// This level won't compress at all but output uncompressed blocks.
                /// </summary>
                public const int NO_COMPRESSION = 0;

                /// <summary>
                /// The compression method.  This is the only method supported so far.
                /// There is no need to use this constant at all.
                /// </summary>
                public const int DEFLATED = 8;

                private const int IS_SETDICT = 0x01;
                private const int IS_FLUSHING = 0x04;
                private const int IS_FINISHING = 0x08;
                private const int INIT_STATE = 0x00;
                private const int SETDICT_STATE = 0x01;
                private const int BUSY_STATE = 0x10;
                private const int FLUSHING_STATE = 0x14;
                private const int FINISHING_STATE = 0x1c;
                private const int FINISHED_STATE = 0x1e;
                private const int CLOSED_STATE = 0x7f;
                public Deflater() : this(DEFAULT_COMPRESSION, false) { /* NOTHING */ }
                public Deflater(int level) : this(level, false) { /* NOTHING */ }

                public Deflater(int level, bool noZlibHeaderOrFooter)
                {
                    if (level == DEFAULT_COMPRESSION)
                    {
                        level = 6;
                    }
                    else if (level < NO_COMPRESSION || level > BEST_COMPRESSION)
                    {
                        throw new Sys.ArgumentOutOfRangeException("level");
                    }

                    pending = new DeflaterPending();
                    engine = new DeflaterEngine(pending);
                    this.noZlibHeaderOrFooter = noZlibHeaderOrFooter;
                    SetStrategy(DeflateStrategy.Default);
                    SetLevel(level);
                    Reset();
                }

                /// <summary>
                /// Resets the deflater.  The deflater acts afterwards as if it was
                /// just created with the same compression level and strategy as it
                /// had before.
                /// </summary>
                public void Reset()
                {
                    state = (noZlibHeaderOrFooter ? BUSY_STATE : INIT_STATE);
                    totalOut = 0;
                    pending.Reset();
                    engine.Reset();
                }

                /// <summary>
                /// Gets the current adler checksum of the data that was processed so far.
                /// </summary>
                public int Adler
                {
                    get
                    {
                        return engine.Adler;
                    }
                }

                /// <summary>
                /// Gets the number of input bytes processed so far.
                /// </summary>
                public long TotalIn
                {
                    get
                    {
                        return engine.TotalIn;
                    }
                }

                /// <summary>
                /// Gets the number of output bytes so far.
                /// </summary>
                public long TotalOut
                {
                    get
                    {
                        return totalOut;
                    }
                }

                /// <summary>
                /// Flushes the current input block.  Further calls to deflate() will
                /// produce enough output to inflate everything in the current input
                /// block.  This is not part of Sun's JDK so I have made it package
                /// private.  It is used by DeflaterOutputStream to implement
                /// flush().
                /// </summary>
                public void Flush()
                {
                    state |= IS_FLUSHING;
                }

                /// <summary>
                /// Finishes the deflater with the current input block.  It is an error
                /// to give more input after this method was called.  This method must
                /// be called to force all bytes to be flushed.
                /// </summary>
                public void Finish()
                {
                    state |= (IS_FLUSHING | IS_FINISHING);
                }

                /// <summary>
                /// Returns true if the stream was finished and no more output bytes
                /// are available.
                /// </summary>
                public bool IsFinished
                {
                    get
                    {
                        return (state == FINISHED_STATE) && pending.IsFlushed;
                    }
                }

                /// <summary>
                /// Returns true, if the input buffer is empty.
                /// You should then call setInput(). 
                /// NOTE: This method can also return true when the stream
                /// was finished.
                /// </summary>
                public bool IsNeedingInput
                {
                    get
                    {
                        return engine.NeedsInput();
                    }
                }

                /// <summary>
                /// Sets the data which should be compressed next.  This should be only
                /// called when needsInput indicates that more input is needed.
                /// If you call setInput when needsInput() returns false, the
                /// previous input that is still pending will be thrown away.
                /// The given byte array should not be changed, before needsInput() returns
                /// true again.
                /// This call is equivalent to <code>setInput(input, 0, input.length)</code>.
                /// </summary>
                /// <param name="input">
                /// the buffer containing the input data.
                /// </param>
                /// <exception cref="System.InvalidOperationException">
                /// if the buffer was finished() or ended().
                /// </exception>
                public void SetInput(byte[] input)
                {
                    SetInput(input, 0, input.Length);
                }

                /// <summary>
                /// Sets the data which should be compressed next.  This should be
                /// only called when needsInput indicates that more input is needed.
                /// The given byte array should not be changed, before needsInput() returns
                /// true again.
                /// </summary>
                /// <param name="input">
                /// the buffer containing the input data.
                /// </param>
                /// <param name="offset">
                /// the start of the data.
                /// </param>
                /// <param name="count">
                /// the number of data bytes of input.
                /// </param>
                /// <exception cref="System.InvalidOperationException">
                /// if the buffer was Finish()ed or if previous input is still pending.
                /// </exception>
                public void SetInput(byte[] input, int offset, int count)
                {
                    if ((state & IS_FINISHING) != 0)
                    {
                        throw new Sys.InvalidOperationException("Finish() already called");
                    }
                    engine.SetInput(input, offset, count);
                }

                /// <summary>
                /// Sets the compression level.  There is no guarantee of the exact
                /// position of the change, but if you call this when needsInput is
                /// true the change of compression level will occur somewhere near
                /// before the end of the so far given input.
                /// </summary>
                /// <param name="level">
                /// the new compression level.
                /// </param>
                public void SetLevel(int level)
                {
                    if (level == DEFAULT_COMPRESSION)
                    {
                        level = 6;
                    }
                    else if (level < NO_COMPRESSION || level > BEST_COMPRESSION)
                    {
                        throw new Sys.ArgumentOutOfRangeException("level");
                    }

                    if (this.level != level)
                    {
                        this.level = level;
                        engine.SetLevel(level);
                    }
                }

                /// <summary>
                /// Get current compression level
                /// </summary>
                /// <returns>Returns the current compression level</returns>
                public int GetLevel()
                {
                    return level;
                }

                /// <summary>
                /// Sets the compression strategy. Strategy is one of
                /// DEFAULT_STRATEGY, HUFFMAN_ONLY and FILTERED.  For the exact
                /// position where the strategy is changed, the same as for
                /// SetLevel() applies.
                /// </summary>
                /// <param name="strategy">
                /// The new compression strategy.
                /// </param>
                public void SetStrategy(DeflateStrategy strategy)
                {
                    engine.Strategy = strategy;
                }

                /// <summary>
                /// Deflates the current input block with to the given array.
                /// </summary>
                /// <param name="output">
                /// The buffer where compressed data is stored
                /// </param>
                /// <returns>
                /// The number of compressed bytes added to the output, or 0 if either
                /// IsNeedingInput() or IsFinished returns true or length is zero.
                /// </returns>
                public int Deflate(byte[] output)
                {
                    return Deflate(output, 0, output.Length);
                }

                /// <summary>
                /// Deflates the current input block to the given array.
                /// </summary>
                /// <param name="output">
                /// Buffer to store the compressed data.
                /// </param>
                /// <param name="offset">
                /// Offset into the output array.
                /// </param>
                /// <param name="length">
                /// The maximum number of bytes that may be stored.
                /// </param>
                /// <returns>
                /// The number of compressed bytes added to the output, or 0 if either
                /// needsInput() or finished() returns true or length is zero.
                /// </returns>
                /// <exception cref="System.InvalidOperationException">
                /// If Finish() was previously called.
                /// </exception>
                /// <exception cref="System.ArgumentOutOfRangeException">
                /// If offset or length don't match the array length.
                /// </exception>
                public int Deflate(byte[] output, int offset, int length)
                {
                    int origLength = length;

                    if (state == CLOSED_STATE)
                    {
                        throw new Sys.InvalidOperationException("Deflater closed");
                    }

                    if (state < BUSY_STATE)
                    {
                        // output header
                        int header = (DEFLATED +
                            ((DeflaterConstants.MAX_WBITS - 8) << 4)) << 8;
                        int level_flags = (level - 1) >> 1;
                        if (level_flags < 0 || level_flags > 3)
                        {
                            level_flags = 3;
                        }
                        header |= level_flags << 6;
                        if ((state & IS_SETDICT) != 0)
                        {
                            // Dictionary was set
                            header |= DeflaterConstants.PRESET_DICT;
                        }
                        header += 31 - (header % 31);

                        pending.WriteShortMSB(header);
                        if ((state & IS_SETDICT) != 0)
                        {
                            int chksum = engine.Adler;
                            engine.ResetAdler();
                            pending.WriteShortMSB(chksum >> 16);
                            pending.WriteShortMSB(chksum & 0xffff);
                        }

                        state = BUSY_STATE | (state & (IS_FLUSHING | IS_FINISHING));
                    }

                    for (; ; )
                    {
                        int count = pending.Flush(output, offset, length);
                        offset += count;
                        totalOut += count;
                        length -= count;

                        if (length == 0 || state == FINISHED_STATE)
                        {
                            break;
                        }

                        if (!engine.Deflate((state & IS_FLUSHING) != 0, (state & IS_FINISHING) != 0))
                        {
                            if (state == BUSY_STATE)
                            {
                                // We need more input now
                                return origLength - length;
                            }
                            else if (state == FLUSHING_STATE)
                            {
                                if (level != NO_COMPRESSION)
                                {
                                    /* We have to supply some lookahead.  8 bit lookahead
                                     * is needed by the zlib inflater, and we must fill
                                     * the next byte, so that all bits are flushed.
                                     */
                                    int neededbits = 8 + ((-pending.BitCount) & 7);
                                    while (neededbits > 0)
                                    {
                                        /* write a static tree block consisting solely of
                                         * an EOF:
                                         */
                                        pending.WriteBits(2, 10);
                                        neededbits -= 10;
                                    }
                                }
                                state = BUSY_STATE;
                            }
                            else if (state == FINISHING_STATE)
                            {
                                pending.AlignToByte();

                                // Compressed data is complete.  Write footer information if required.
                                if (!noZlibHeaderOrFooter)
                                {
                                    int adler = engine.Adler;
                                    pending.WriteShortMSB(adler >> 16);
                                    pending.WriteShortMSB(adler & 0xffff);
                                }
                                state = FINISHED_STATE;
                            }
                        }
                    }
                    return origLength - length;
                }

                /// <summary>
                /// Sets the dictionary which should be used in the deflate process.
                /// This call is equivalent to <code>setDictionary(dict, 0, dict.Length)</code>.
                /// </summary>
                /// <param name="dictionary">
                /// the dictionary.
                /// </param>
                /// <exception cref="System.InvalidOperationException">
                /// if SetInput () or Deflate () were already called or another dictionary was already set.
                /// </exception>
                public void SetDictionary(byte[] dictionary)
                {
                    SetDictionary(dictionary, 0, dictionary.Length);
                }

                /// <summary>
                /// Sets the dictionary which should be used in the deflate process.
                /// The dictionary is a byte array containing strings that are
                /// likely to occur in the data which should be compressed.  The
                /// dictionary is not stored in the compressed output, only a
                /// checksum.  To decompress the output you need to supply the same
                /// dictionary again.
                /// </summary>
                /// <param name="dictionary">
                /// The dictionary data
                /// </param>
                /// <param name="index">
                /// The index where dictionary information commences.
                /// </param>
                /// <param name="count">
                /// The number of bytes in the dictionary.
                /// </param>
                /// <exception cref="System.InvalidOperationException">
                /// If SetInput () or Deflate() were already called or another dictionary was already set.
                /// </exception>
                public void SetDictionary(byte[] dictionary, int index, int count)
                {
                    if (state != INIT_STATE)
                    {
                        throw new Sys.InvalidOperationException();
                    }

                    state = SETDICT_STATE;
                    engine.SetDictionary(dictionary, index, count);
                }

                int level;
                bool noZlibHeaderOrFooter;
                int state;
                long totalOut;
                DeflaterPending pending;
                DeflaterEngine engine;
            }

            public class Inflater
            {
                static readonly int[] CPLENS = { 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258 };
                static readonly int[] CPLEXT = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0 };
                static readonly int[] CPDIST = { 1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577 };
                static readonly int[] CPDEXT = { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13 };
                const int DECODE_HEADER = 0;
                const int DECODE_DICT = 1;
                const int DECODE_BLOCKS = 2;
                const int DECODE_STORED_LEN1 = 3;
                const int DECODE_STORED_LEN2 = 4;
                const int DECODE_STORED = 5;
                const int DECODE_DYN_HEADER = 6;
                const int DECODE_HUFFMAN = 7;
                const int DECODE_HUFFMAN_LENBITS = 8;
                const int DECODE_HUFFMAN_DIST = 9;
                const int DECODE_HUFFMAN_DISTBITS = 10;
                const int DECODE_CHKSUM = 11;
                const int FINISHED = 12;
                int mode;
                int readAdler;
                int neededBits;
                int repLength;
                int repDist;
                int uncomprLen;
                bool isLastBlock;
                long totalOut;
                long totalIn;
                bool noHeader;
                ZIPLib.Compression.Streams.StreamManipulator input;
                ZIPLib.Compression.Streams.OutputWindow outputWindow;
                InflaterDynHeader dynHeader;
                InflaterHuffmanTree litlenTree, distTree;
                ZIPLib.Checksums.Adler32 adler;
                public Inflater() : this(false) { /* NOTHING */ }

                public Inflater(bool noHeader)
                {
                    this.noHeader = noHeader;
                    this.adler = new ZIPLib.Checksums.Adler32();
                    input = new ZIPLib.Compression.Streams.StreamManipulator();
                    outputWindow = new ZIPLib.Compression.Streams.OutputWindow();
                    mode = noHeader ? DECODE_BLOCKS : DECODE_HEADER;
                }

                public void Reset()
                {
                    mode = noHeader ? DECODE_BLOCKS : DECODE_HEADER;
                    totalIn = 0;
                    totalOut = 0;
                    input.Reset();
                    outputWindow.Reset();
                    dynHeader = null;
                    litlenTree = null;
                    distTree = null;
                    isLastBlock = false;
                    adler.Reset();
                }

                private bool DecodeHeader()
                {
                    int header = input.PeekBits(16);
                    if (header < 0) { return false; }
                    input.DropBits(16);
                    header = ((header << 8) | (header >> 8)) & 0xffff;
                    if (header % 31 != 0) { throw new Sys.ApplicationException("Header checksum illegal"); }
                    if ((header & 0x0f00) != (Deflater.DEFLATED << 8)) { throw new Sys.ApplicationException("Compression Method unknown"); }
                    if ((header & 0x0020) == 0) { mode = DECODE_BLOCKS; }
                    else 
                    {
                        mode = DECODE_DICT;
                        neededBits = 32;
                    }
                    return true;
                }

                private bool DecodeDict()
                {
                    while (neededBits > 0)
                    {
                        int dictByte = input.PeekBits(8);
                        if (dictByte < 0) { return false; }
                        input.DropBits(8);
                        readAdler = (readAdler << 8) | dictByte;
                        neededBits -= 8;
                    }
                    return false;
                }

                private bool DecodeHuffman()
                {
                    int free = outputWindow.GetFreeSpace();
                    while (free >= 258)
                    {
                        int symbol;
                        switch (mode)
                        {
                            case DECODE_HUFFMAN:
                                while (((symbol = litlenTree.GetSymbol(input)) & ~0xff) == 0)
                                {
                                    outputWindow.Write(symbol);
                                    if (--free < 258) { return true; }
                                }
                                if (symbol < 257)
                                {
                                    if (symbol < 0) { return false; }
                                    else
                                    {
                                        distTree = null;
                                        litlenTree = null;
                                        mode = DECODE_BLOCKS;
                                        return true;
                                    }
                                }
                                try
                                {
                                    repLength = CPLENS[symbol - 257];
                                    neededBits = CPLEXT[symbol - 257];
                                }
                                catch (Sys.Exception) { throw new Sys.ApplicationException("Illegal rep length code"); }
                                goto case DECODE_HUFFMAN_LENBITS;
                            case DECODE_HUFFMAN_LENBITS:
                                if (neededBits > 0)
                                {
                                    mode = DECODE_HUFFMAN_LENBITS;
                                    int i = input.PeekBits(neededBits);
                                    if (i < 0) { return false; }
                                    input.DropBits(neededBits);
                                    repLength += i;
                                }
                                mode = DECODE_HUFFMAN_DIST;
                                goto case DECODE_HUFFMAN_DIST;

                            case DECODE_HUFFMAN_DIST:
                                symbol = distTree.GetSymbol(input);
                                if (symbol < 0) { return false; }
                                try
                                {
                                    repDist = CPDIST[symbol];
                                    neededBits = CPDEXT[symbol];
                                }
                                catch (Sys.Exception) { throw new Sys.ApplicationException("Illegal rep dist code"); }
                                goto case DECODE_HUFFMAN_DISTBITS;
                            case DECODE_HUFFMAN_DISTBITS:
                                if (neededBits > 0)
                                {
                                    mode = DECODE_HUFFMAN_DISTBITS;
                                    int i = input.PeekBits(neededBits);
                                    if (i < 0) { return false; }
                                    input.DropBits(neededBits);
                                    repDist += i;
                                }
                                outputWindow.Repeat(repLength, repDist);
                                free -= repLength;
                                mode = DECODE_HUFFMAN;
                                break;
                            default: throw new Sys.ApplicationException("Inflater unknown mode");
                        }
                    }
                    return true;
                }

                private bool DecodeChksum()
                {
                    while (neededBits > 0)
                    {
                        int chkByte = input.PeekBits(8);
                        if (chkByte < 0)
                        {
                            return false;
                        }
                        input.DropBits(8);
                        readAdler = (readAdler << 8) | chkByte;
                        neededBits -= 8;
                    }
                    if ((int)adler.Value != readAdler) { throw new Sys.ApplicationException("Adler chksum doesn't match: " + (int)adler.Value + " vs. " + readAdler); }
                    mode = FINISHED;
                    return false;
                }

                private bool Decode()
                {
                    switch (mode)
                    {
                        case DECODE_HEADER: return DecodeHeader();
                        case DECODE_DICT: return DecodeDict();
                        case DECODE_CHKSUM: return DecodeChksum();
                        case DECODE_BLOCKS:
                            if (isLastBlock)
                            {
                                if (noHeader)
                                {
                                    mode = FINISHED;
                                    return false;
                                }
                                else
                                {
                                    input.SkipToByteBoundary();
                                    neededBits = 32;
                                    mode = DECODE_CHKSUM;
                                    return true;
                                }
                            }
                            int type = input.PeekBits(3);
                            if (type < 0) { return false; }
                            input.DropBits(3);
                            if ((type & 1) != 0) { isLastBlock = true; }
                            switch (type >> 1)
                            {
                                case DeflaterConstants.STORED_BLOCK:
                                    input.SkipToByteBoundary();
                                    mode = DECODE_STORED_LEN1;
                                    break;
                                case DeflaterConstants.STATIC_TREES:
                                    litlenTree = InflaterHuffmanTree.defLitLenTree;
                                    distTree = InflaterHuffmanTree.defDistTree;
                                    mode = DECODE_HUFFMAN;
                                    break;
                                case DeflaterConstants.DYN_TREES:
                                    dynHeader = new InflaterDynHeader();
                                    mode = DECODE_DYN_HEADER;
                                    break;
                                default: throw new Sys.ApplicationException("Unknown block Type " + type);
                            }
                            return true;
                        case DECODE_STORED_LEN1:
                            {
                                if ((uncomprLen = input.PeekBits(16)) < 0) { return false; }
                                input.DropBits(16);
                                mode = DECODE_STORED_LEN2;
                            }
                            goto case DECODE_STORED_LEN2;
                        case DECODE_STORED_LEN2:
                            {
                                int nlen = input.PeekBits(16);
                                if (nlen < 0) { return false; }
                                input.DropBits(16);
                                if (nlen != (uncomprLen ^ 0xffff)) { throw new Sys.ApplicationException("broken uncompressed block"); }
                                mode = DECODE_STORED;
                            }
                            goto case DECODE_STORED;
                        case DECODE_STORED:
                            {
                                int more = outputWindow.CopyStored(input, uncomprLen);
                                uncomprLen -= more;
                                if (uncomprLen == 0)
                                {
                                    mode = DECODE_BLOCKS;
                                    return true;
                                }
                                return !input.IsNeedingInput;
                            }
                        case DECODE_DYN_HEADER:
                            if (!dynHeader.Decode(input)) { return false; }
                            litlenTree = dynHeader.BuildLitLenTree();
                            distTree = dynHeader.BuildDistTree();
                            mode = DECODE_HUFFMAN;
                            goto case DECODE_HUFFMAN;
                        case DECODE_HUFFMAN:
                        case DECODE_HUFFMAN_LENBITS:
                        case DECODE_HUFFMAN_DIST:
                        case DECODE_HUFFMAN_DISTBITS: return DecodeHuffman();
                        case FINISHED: return false;
                        default: throw new Sys.ApplicationException("Inflater.Decode unknown mode");
                    }
                }

                public void SetDictionary(byte[] buffer) { SetDictionary(buffer, 0, buffer.Length); }

                public void SetDictionary(byte[] buffer, int index, int count)
                {
                    if (buffer == null)
                    {
                        throw new Sys.ArgumentNullException("buffer");
                    }

                    if (index < 0)
                    {
                        throw new Sys.ArgumentOutOfRangeException("index");
                    }

                    if (count < 0)
                    {
                        throw new Sys.ArgumentOutOfRangeException("count");
                    }

                    if (!IsNeedingDictionary)
                    {
                        throw new Sys.InvalidOperationException("Dictionary is not needed");
                    }

                    adler.Update(buffer, index, count);

                    if ((int)adler.Value != readAdler)
                    {
                        throw new Sys.ApplicationException("Wrong adler checksum");
                    }
                    adler.Reset();
                    outputWindow.CopyDict(buffer, index, count);
                    mode = DECODE_BLOCKS;
                }

                /// <summary>
                /// Sets the input.  This should only be called, if needsInput()
                /// returns true.
                /// </summary>
                /// <param name="buffer">
                /// the input.
                /// </param>
                public void SetInput(byte[] buffer)
                {
                    SetInput(buffer, 0, buffer.Length);
                }

                /// <summary>
                /// Sets the input.  This should only be called, if needsInput()
                /// returns true.
                /// </summary>
                /// <param name="buffer">
                /// The source of input data
                /// </param>
                /// <param name="index">
                /// The index into buffer where the input starts.
                /// </param>
                /// <param name="count">
                /// The number of bytes of input to use.
                /// </param>
                /// <exception cref="System.InvalidOperationException">
                /// No input is needed.
                /// </exception>
                /// <exception cref="System.ArgumentOutOfRangeException">
                /// The index and/or count are wrong.
                /// </exception>
                public void SetInput(byte[] buffer, int index, int count)
                {
                    input.SetInput(buffer, index, count);
                    totalIn += (long)count;
                }

                /// <summary>
                /// Inflates the compressed stream to the output buffer.  If this
                /// returns 0, you should check, whether IsNeedingDictionary(),
                /// IsNeedingInput() or IsFinished() returns true, to determine why no
                /// further output is produced.
                /// </summary>
                /// <param name="buffer">
                /// the output buffer.
                /// </param>
                /// <returns>
                /// The number of bytes written to the buffer, 0 if no further
                /// output can be produced.
                /// </returns>
                /// <exception cref="System.ArgumentOutOfRangeException">
                /// if buffer has length 0.
                /// </exception>
                /// <exception cref="System.FormatException">
                /// if deflated stream is invalid.
                /// </exception>
                public int Inflate(byte[] buffer)
                {
                    if (buffer == null)
                    {
                        throw new Sys.ArgumentNullException("buffer");
                    }

                    return Inflate(buffer, 0, buffer.Length);
                }

                /// <summary>
                /// Inflates the compressed stream to the output buffer.  If this
                /// returns 0, you should check, whether needsDictionary(),
                /// needsInput() or finished() returns true, to determine why no
                /// further output is produced.
                /// </summary>
                /// <param name="buffer">
                /// the output buffer.
                /// </param>
                /// <param name="offset">
                /// the offset in buffer where storing starts.
                /// </param>
                /// <param name="count">
                /// the maximum number of bytes to output.
                /// </param>
                /// <returns>
                /// the number of bytes written to the buffer, 0 if no further output can be produced.
                /// </returns>
                /// <exception cref="System.ArgumentOutOfRangeException">
                /// if count is less than 0.
                /// </exception>
                /// <exception cref="System.ArgumentOutOfRangeException">
                /// if the index and / or count are wrong.
                /// </exception>
                /// <exception cref="System.FormatException">
                /// if deflated stream is invalid.
                /// </exception>
                public int Inflate(byte[] buffer, int offset, int count)
                {
                    if (buffer == null)
                    {
                        throw new Sys.ArgumentNullException("buffer");
                    }

                    if (count < 0)
                    {
                        throw new Sys.ArgumentOutOfRangeException("count", "count cannot be negative");
                    }

                    if (offset < 0)
                    {
                        throw new Sys.ArgumentOutOfRangeException("offset", "offset cannot be negative");
                    }

                    if (offset + count > buffer.Length)
                    {
                        throw new Sys.ArgumentException("count exceeds buffer bounds");
                    }

                    // Special case: count may be zero
                    if (count == 0)
                    {
                        if (!IsFinished)
                        { // -jr- 08-Nov-2003 INFLATE_BUG fix..
                            Decode();
                        }
                        return 0;
                    }

                    int bytesCopied = 0;

                    do
                    {
                        if (mode != DECODE_CHKSUM)
                        {
                            /* Don't give away any output, if we are waiting for the
                            * checksum in the input stream.
                            *
                            * With this trick we have always:
                            *   IsNeedingInput() and not IsFinished()
                            *   implies more output can be produced.
                            */
                            int more = outputWindow.CopyOutput(buffer, offset, count);
                            if (more > 0)
                            {
                                adler.Update(buffer, offset, more);
                                offset += more;
                                bytesCopied += more;
                                totalOut += (long)more;
                                count -= more;
                                if (count == 0)
                                {
                                    return bytesCopied;
                                }
                            }
                        }
                    } while (Decode() || ((outputWindow.GetAvailable() > 0) && (mode != DECODE_CHKSUM)));
                    return bytesCopied;
                }

                /// <summary>
                /// Returns true, if the input buffer is empty.
                /// You should then call setInput(). 
                /// NOTE: This method also returns true when the stream is finished.
                /// </summary>
                public bool IsNeedingInput
                {
                    get
                    {
                        return input.IsNeedingInput;
                    }
                }

                /// <summary>
                /// Returns true, if a preset dictionary is needed to inflate the input.
                /// </summary>
                public bool IsNeedingDictionary
                {
                    get
                    {
                        return mode == DECODE_DICT && neededBits == 0;
                    }
                }

                /// <summary>
                /// Returns true, if the inflater has finished.  This means, that no
                /// input is needed and no output can be produced.
                /// </summary>
                public bool IsFinished
                {
                    get
                    {
                        return mode == FINISHED && outputWindow.GetAvailable() == 0;
                    }
                }

                /// <summary>
                /// Gets the adler checksum.  This is either the checksum of all
                /// uncompressed bytes returned by inflate(), or if needsDictionary()
                /// returns true (and thus no output was yet produced) this is the
                /// adler checksum of the expected dictionary.
                /// </summary>
                /// <returns>
                /// the adler checksum.
                /// </returns>
                public int Adler
                {
                    get
                    {
                        return IsNeedingDictionary ? readAdler : (int)adler.Value;
                    }
                }

                /// <summary>
                /// Gets the total number of output bytes returned by Inflate().
                /// </summary>
                /// <returns>
                /// the total number of output bytes.
                /// </returns>
                public long TotalOut
                {
                    get
                    {
                        return totalOut;
                    }
                }

                /// <summary>
                /// Gets the total number of processed compressed input bytes.
                /// </summary>
                /// <returns>
                /// The total number of bytes of processed input bytes.
                /// </returns>
                public long TotalIn
                {
                    get
                    {
                        return totalIn - (long)RemainingInput;
                    }
                }

                /// <summary>
                /// Gets the number of unprocessed input bytes.  Useful, if the end of the
                /// stream is reached and you want to further process the bytes after
                /// the deflate stream.
                /// </summary>
                /// <returns>
                /// The number of bytes of the input which have not been processed.
                /// </returns>
                public int RemainingInput
                {
                    get
                    {
                        return input.AvailableBytes;
                    }
                }
            }

            public class InflaterHuffmanTree
            {
                const int MAX_BITLEN = 15;
                short[] tree;

                /// <summary>
                /// Literal length tree
                /// </summary>
                public static InflaterHuffmanTree defLitLenTree;

                /// <summary>
                /// Distance tree
                /// </summary>
                public static InflaterHuffmanTree defDistTree;

                static InflaterHuffmanTree()
                {
                    try
                    {
                        byte[] codeLengths = new byte[288];
                        int i = 0;
                        while (i < 144)
                        {
                            codeLengths[i++] = 8;
                        }
                        while (i < 256)
                        {
                            codeLengths[i++] = 9;
                        }
                        while (i < 280)
                        {
                            codeLengths[i++] = 7;
                        }
                        while (i < 288)
                        {
                            codeLengths[i++] = 8;
                        }
                        defLitLenTree = new InflaterHuffmanTree(codeLengths);

                        codeLengths = new byte[32];
                        i = 0;
                        while (i < 32)
                        {
                            codeLengths[i++] = 5;
                        }
                        defDistTree = new InflaterHuffmanTree(codeLengths);
                    }
                    catch (Sys.Exception)
                    {
                        throw new Sys.ApplicationException("InflaterHuffmanTree: static tree length illegal");
                    }
                }

                /// <summary>
                /// Constructs a Huffman tree from the array of code lengths.
                /// </summary>
                /// <param name = "codeLengths">
                /// the array of code lengths
                /// </param>
                public InflaterHuffmanTree(byte[] codeLengths)
                {
                    BuildTree(codeLengths);
                }

                void BuildTree(byte[] codeLengths)
                {
                    int[] blCount = new int[MAX_BITLEN + 1];
                    int[] nextCode = new int[MAX_BITLEN + 1];

                    for (int i = 0; i < codeLengths.Length; i++)
                    {
                        int bits = codeLengths[i];
                        if (bits > 0)
                        {
                            blCount[bits]++;
                        }
                    }

                    int code = 0;
                    int treeSize = 512;
                    for (int bits = 1; bits <= MAX_BITLEN; bits++)
                    {
                        nextCode[bits] = code;
                        code += blCount[bits] << (16 - bits);
                        if (bits >= 10)
                        {
                            /* We need an extra table for bit lengths >= 10. */
                            int start = nextCode[bits] & 0x1ff80;
                            int end = code & 0x1ff80;
                            treeSize += (end - start) >> (16 - bits);
                        }
                    }

                    /* -jr comment this out! doesnt work for dynamic trees and pkzip 2.04g
                                if (code != 65536) 
                                {
                                    throw new SharpZipBaseException("Code lengths don't add up properly.");
                                }
                    */
                    /* Now create and fill the extra tables from longest to shortest
                    * bit len.  This way the sub trees will be aligned.
                    */
                    tree = new short[treeSize];
                    int treePtr = 512;
                    for (int bits = MAX_BITLEN; bits >= 10; bits--)
                    {
                        int end = code & 0x1ff80;
                        code -= blCount[bits] << (16 - bits);
                        int start = code & 0x1ff80;
                        for (int i = start; i < end; i += 1 << 7)
                        {
                            tree[DeflaterHuffman.BitReverse(i)] = (short)((-treePtr << 4) | bits);
                            treePtr += 1 << (bits - 9);
                        }
                    }

                    for (int i = 0; i < codeLengths.Length; i++)
                    {
                        int bits = codeLengths[i];
                        if (bits == 0)
                        {
                            continue;
                        }
                        code = nextCode[bits];
                        int revcode = DeflaterHuffman.BitReverse(code);
                        if (bits <= 9)
                        {
                            do
                            {
                                tree[revcode] = (short)((i << 4) | bits);
                                revcode += 1 << bits;
                            } while (revcode < 512);
                        }
                        else
                        {
                            int subTree = tree[revcode & 511];
                            int treeLen = 1 << (subTree & 15);
                            subTree = -(subTree >> 4);
                            do
                            {
                                tree[subTree | (revcode >> 9)] = (short)((i << 4) | bits);
                                revcode += 1 << bits;
                            } while (revcode < treeLen);
                        }
                        nextCode[bits] = code + (1 << (16 - bits));
                    }

                }

                /// <summary>
                /// Reads the next symbol from input.  The symbol is encoded using the
                /// huffman tree.
                /// </summary>
                /// <param name="input">
                /// input the input source.
                /// </param>
                /// <returns>
                /// the next symbol, or -1 if not enough input is available.
                /// </returns>
                public int GetSymbol(ZIPLib.Compression.Streams.StreamManipulator input)
                {
                    int lookahead, symbol;
                    if ((lookahead = input.PeekBits(9)) >= 0)
                    {
                        if ((symbol = tree[lookahead]) >= 0)
                        {
                            input.DropBits(symbol & 15);
                            return symbol >> 4;
                        }
                        int subtree = -(symbol >> 4);
                        int bitlen = symbol & 15;
                        if ((lookahead = input.PeekBits(bitlen)) >= 0)
                        {
                            symbol = tree[subtree | (lookahead >> 9)];
                            input.DropBits(symbol & 15);
                            return symbol >> 4;
                        }
                        else
                        {
                            int bits = input.AvailableBits;
                            lookahead = input.PeekBits(bits);
                            symbol = tree[subtree | (lookahead >> 9)];
                            if ((symbol & 15) <= bits)
                            {
                                input.DropBits(symbol & 15);
                                return symbol >> 4;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                    }
                    else
                    {
                        int bits = input.AvailableBits;
                        lookahead = input.PeekBits(bits);
                        symbol = tree[lookahead];
                        if (symbol >= 0 && (symbol & 15) <= bits)
                        {
                            input.DropBits(symbol & 15);
                            return symbol >> 4;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }
            }

            internal class InflaterDynHeader
            {
                const int LNUM = 0;
                const int DNUM = 1;
                const int BLNUM = 2;
                const int BLLENS = 3;
                const int LENS = 4;
                const int REPS = 5;
                static readonly int[] repMin = { 3, 3, 11 };
                static readonly int[] repBits = { 2, 3, 7 };
                static readonly int[] BL_ORDER = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

                public InflaterDynHeader()
                {
                }

                public bool Decode(ZIPLib.Compression.Streams.StreamManipulator input)
                {
                decode_loop:
                    for (; ; )
                    {
                        switch (mode)
                        {
                            case LNUM:
                                lnum = input.PeekBits(5);
                                if (lnum < 0)
                                {
                                    return false;
                                }
                                lnum += 257;
                                input.DropBits(5);
                                //  	    Sys.err.println("LNUM: "+lnum);
                                mode = DNUM;
                                goto case DNUM; // fall through
                            case DNUM:
                                dnum = input.PeekBits(5);
                                if (dnum < 0)
                                {
                                    return false;
                                }
                                dnum++;
                                input.DropBits(5);
                                //  	    Sys.err.println("DNUM: "+dnum);
                                num = lnum + dnum;
                                litdistLens = new byte[num];
                                mode = BLNUM;
                                goto case BLNUM; // fall through
                            case BLNUM:
                                blnum = input.PeekBits(4);
                                if (blnum < 0)
                                {
                                    return false;
                                }
                                blnum += 4;
                                input.DropBits(4);
                                blLens = new byte[19];
                                ptr = 0;
                                //  	    Sys.err.println("BLNUM: "+blnum);
                                mode = BLLENS;
                                goto case BLLENS; // fall through
                            case BLLENS:
                                while (ptr < blnum)
                                {
                                    int len = input.PeekBits(3);
                                    if (len < 0)
                                    {
                                        return false;
                                    }
                                    input.DropBits(3);
                                    //  		System.err.println("blLens["+BL_ORDER[ptr]+"]: "+len);
                                    blLens[BL_ORDER[ptr]] = (byte)len;
                                    ptr++;
                                }
                                blTree = new InflaterHuffmanTree(blLens);
                                blLens = null;
                                ptr = 0;
                                mode = LENS;
                                goto case LENS; // fall through
                            case LENS:
                                {
                                    int symbol;
                                    while (((symbol = blTree.GetSymbol(input)) & ~15) == 0)
                                    {
                                        /* Normal case: symbol in [0..15] */

                                        //  		  Sys.err.println("litdistLens["+ptr+"]: "+symbol);
                                        litdistLens[ptr++] = lastLen = (byte)symbol;

                                        if (ptr == num)
                                        {
                                            /* Finished */
                                            return true;
                                        }
                                    }

                                    /* need more input ? */
                                    if (symbol < 0)
                                    {
                                        return false;
                                    }

                                    /* otherwise repeat code */
                                    if (symbol >= 17)
                                    {
                                        /* repeat zero */
                                        //  		  Sys.err.println("repeating zero");
                                        lastLen = 0;
                                    }
                                    else
                                    {
                                        if (ptr == 0)
                                        {
                                            throw new Sys.ApplicationException();
                                        }
                                    }
                                    repSymbol = symbol - 16;
                                }
                                mode = REPS;
                                goto case REPS; // fall through
                            case REPS:
                                {
                                    int bits = repBits[repSymbol];
                                    int count = input.PeekBits(bits);
                                    if (count < 0)
                                    {
                                        return false;
                                    }
                                    input.DropBits(bits);
                                    count += repMin[repSymbol];
                                    //  	      Sys.err.println("litdistLens repeated: "+count);

                                    if (ptr + count > num)
                                    {
                                        throw new Sys.ApplicationException();
                                    }
                                    while (count-- > 0)
                                    {
                                        litdistLens[ptr++] = lastLen;
                                    }

                                    if (ptr == num)
                                    {
                                        /* Finished */
                                        return true;
                                    }
                                }
                                mode = LENS;
                                goto decode_loop;
                        }
                    }
                }

                public InflaterHuffmanTree BuildLitLenTree()
                {
                    byte[] litlenLens = new byte[lnum];
                    Sys.Array.Copy(litdistLens, 0, litlenLens, 0, lnum);
                    return new InflaterHuffmanTree(litlenLens);
                }

                public InflaterHuffmanTree BuildDistTree()
                {
                    byte[] distLens = new byte[dnum];
                    Sys.Array.Copy(litdistLens, lnum, distLens, 0, dnum);
                    return new InflaterHuffmanTree(distLens);
                }

                byte[] blLens;
                byte[] litdistLens;

                InflaterHuffmanTree blTree;

                int mode;
                int lnum, dnum, blnum, num;
                int repSymbol;
                byte lastLen;
                int ptr;
            }

            public class DeflaterPending : PendingBuffer
            {
                /// <summary>
                /// Construct instance with default buffer size
                /// </summary>
                public DeflaterPending()
                    : base(DeflaterConstants.PENDING_BUF_SIZE)
                {
                }
            }

            public class DeflaterHuffman
            {
                const int BUFSIZE = 1 << (DeflaterConstants.DEFAULT_MEM_LEVEL + 6);
                const int LITERAL_NUM = 286;
                const int DIST_NUM = 30;
                const int BITLEN_NUM = 19;
                const int REP_3_6 = 16;
                const int REP_3_10 = 17;
                const int REP_11_138 = 18;
                const int EOF_SYMBOL = 256;
                static readonly int[] BL_ORDER = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };
                static readonly byte[] bit4Reverse = { 0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15 };
                static short[] staticLCodes;
                static byte[] staticLLength;
                static short[] staticDCodes;
                static byte[] staticDLength;

                class Tree
                {
                    public short[] freqs;
                    public byte[] length;
                    public int minNumCodes;
                    public int numCodes;
                    short[] codes;
                    int[] bl_counts;
                    int maxLength;
                    DeflaterHuffman dh;

                    public Tree(DeflaterHuffman dh, int elems, int minCodes, int maxLength)
                    {
                        this.dh = dh;
                        this.minNumCodes = minCodes;
                        this.maxLength = maxLength;
                        freqs = new short[elems];
                        bl_counts = new int[maxLength];
                    }

                    public void Reset()
                    {
                        for (int i = 0; i < freqs.Length; i++)
                        {
                            freqs[i] = 0;
                        }
                        codes = null;
                        length = null;
                    }

                    public void WriteSymbol(int code)
                    {
                        //				if (DeflaterConstants.DEBUGGING) {
                        //					freqs[code]--;
                        //					//  	  Console.Write("writeSymbol("+freqs.length+","+code+"): ");
                        //				}
                        dh.pending.WriteBits(codes[code] & 0xffff, length[code]);
                    }

                    public void CheckEmpty()
                    {
                        bool empty = true;
                        for (int i = 0; i < freqs.Length; i++)
                        {
                            if (freqs[i] != 0)
                            {
                                //Console.WriteLine("freqs[" + i + "] == " + freqs[i]);
                                empty = false;
                            }
                        }

                        if (!empty)
                        {
                            throw new Sys.ApplicationException("!Empty");
                        }
                    }

                    public void SetStaticCodes(short[] staticCodes, byte[] staticLengths)
                    {
                        codes = staticCodes;
                        length = staticLengths;
                    }

                    public void BuildCodes()
                    {
                        int numSymbols = freqs.Length;
                        int[] nextCode = new int[maxLength];
                        int code = 0;

                        codes = new short[freqs.Length];

                        //				if (DeflaterConstants.DEBUGGING) {
                        //					//Console.WriteLine("buildCodes: "+freqs.Length);
                        //				}

                        for (int bits = 0; bits < maxLength; bits++)
                        {
                            nextCode[bits] = code;
                            code += bl_counts[bits] << (15 - bits);

                            //					if (DeflaterConstants.DEBUGGING) {
                            //						//Console.WriteLine("bits: " + ( bits + 1) + " count: " + bl_counts[bits]
                            //						                  +" nextCode: "+code);
                            //					}
                        }
                        for (int i = 0; i < numCodes; i++)
                        {
                            int bits = length[i];
                            if (bits > 0)
                            {

                                //						if (DeflaterConstants.DEBUGGING) {
                                //								//Console.WriteLine("codes["+i+"] = rev(" + nextCode[bits-1]+"),
                                //								                  +bits);
                                //						}

                                codes[i] = BitReverse(nextCode[bits - 1]);
                                nextCode[bits - 1] += 1 << (16 - bits);
                            }
                        }
                    }

                    public void BuildTree()
                    {
                        int numSymbols = freqs.Length;

                        /* heap is a priority queue, sorted by frequency, least frequent
                        * nodes first.  The heap is a binary tree, with the property, that
                        * the parent node is smaller than both child nodes.  This assures
                        * that the smallest node is the first parent.
                        *
                        * The binary tree is encoded in an array:  0 is root node and
                        * the nodes 2*n+1, 2*n+2 are the child nodes of node n.
                        */
                        int[] heap = new int[numSymbols];
                        int heapLen = 0;
                        int maxCode = 0;
                        for (int n = 0; n < numSymbols; n++)
                        {
                            int freq = freqs[n];
                            if (freq != 0)
                            {
                                // Insert n into heap
                                int pos = heapLen++;
                                int ppos;
                                while (pos > 0 && freqs[heap[ppos = (pos - 1) / 2]] > freq)
                                {
                                    heap[pos] = heap[ppos];
                                    pos = ppos;
                                }
                                heap[pos] = n;

                                maxCode = n;
                            }
                        }

                        /* We could encode a single literal with 0 bits but then we
                        * don't see the literals.  Therefore we force at least two
                        * literals to avoid this case.  We don't care about order in
                        * this case, both literals get a 1 bit code.
                        */
                        while (heapLen < 2)
                        {
                            int node = maxCode < 2 ? ++maxCode : 0;
                            heap[heapLen++] = node;
                        }

                        numCodes = SysMath.Max(maxCode + 1, minNumCodes);

                        int numLeafs = heapLen;
                        int[] childs = new int[4 * heapLen - 2];
                        int[] values = new int[2 * heapLen - 1];
                        int numNodes = numLeafs;
                        for (int i = 0; i < heapLen; i++)
                        {
                            int node = heap[i];
                            childs[2 * i] = node;
                            childs[2 * i + 1] = -1;
                            values[i] = freqs[node] << 8;
                            heap[i] = i;
                        }

                        /* Construct the Huffman tree by repeatedly combining the least two
                        * frequent nodes.
                        */
                        do
                        {
                            int first = heap[0];
                            int last = heap[--heapLen];

                            // Propagate the hole to the leafs of the heap
                            int ppos = 0;
                            int path = 1;

                            while (path < heapLen)
                            {
                                if (path + 1 < heapLen && values[heap[path]] > values[heap[path + 1]])
                                {
                                    path++;
                                }

                                heap[ppos] = heap[path];
                                ppos = path;
                                path = path * 2 + 1;
                            }

                            /* Now propagate the last element down along path.  Normally
                            * it shouldn't go too deep.
                            */
                            int lastVal = values[last];
                            while ((path = ppos) > 0 && values[heap[ppos = (path - 1) / 2]] > lastVal)
                            {
                                heap[path] = heap[ppos];
                            }
                            heap[path] = last;


                            int second = heap[0];

                            // Create a new node father of first and second
                            last = numNodes++;
                            childs[2 * last] = first;
                            childs[2 * last + 1] = second;
                            int mindepth = SysMath.Min(values[first] & 0xff, values[second] & 0xff);
                            values[last] = lastVal = values[first] + values[second] - mindepth + 1;

                            // Again, propagate the hole to the leafs
                            ppos = 0;
                            path = 1;

                            while (path < heapLen)
                            {
                                if (path + 1 < heapLen && values[heap[path]] > values[heap[path + 1]])
                                {
                                    path++;
                                }

                                heap[ppos] = heap[path];
                                ppos = path;
                                path = ppos * 2 + 1;
                            }

                            // Now propagate the new element down along path
                            while ((path = ppos) > 0 && values[heap[ppos = (path - 1) / 2]] > lastVal)
                            {
                                heap[path] = heap[ppos];
                            }
                            heap[path] = last;
                        } while (heapLen > 1);

                        if (heap[0] != childs.Length / 2 - 1)
                        {
                            throw new Sys.ApplicationException("Heap invariant violated");
                        }

                        BuildLength(childs);
                    }

                    public int GetEncodedLength()
                    {
                        int len = 0;
                        for (int i = 0; i < freqs.Length; i++)
                        {
                            len += freqs[i] * length[i];
                        }
                        return len;
                    }

                    public void CalcBLFreq(Tree blTree)
                    {
                        int max_count;               /* max repeat count */
                        int min_count;               /* min repeat count */
                        int count;                   /* repeat count of the current code */
                        int curlen = -1;             /* length of current code */

                        int i = 0;
                        while (i < numCodes)
                        {
                            count = 1;
                            int nextlen = length[i];
                            if (nextlen == 0)
                            {
                                max_count = 138;
                                min_count = 3;
                            }
                            else
                            {
                                max_count = 6;
                                min_count = 3;
                                if (curlen != nextlen)
                                {
                                    blTree.freqs[nextlen]++;
                                    count = 0;
                                }
                            }
                            curlen = nextlen;
                            i++;

                            while (i < numCodes && curlen == length[i])
                            {
                                i++;
                                if (++count >= max_count)
                                {
                                    break;
                                }
                            }

                            if (count < min_count)
                            {
                                blTree.freqs[curlen] += (short)count;
                            }
                            else if (curlen != 0)
                            {
                                blTree.freqs[REP_3_6]++;
                            }
                            else if (count <= 10)
                            {
                                blTree.freqs[REP_3_10]++;
                            }
                            else
                            {
                                blTree.freqs[REP_11_138]++;
                            }
                        }
                    }

                    public void WriteTree(Tree blTree)
                    {
                        int max_count;               // max repeat count
                        int min_count;               // min repeat count
                        int count;                   // repeat count of the current code
                        int curlen = -1;             // length of current code

                        int i = 0;
                        while (i < numCodes)
                        {
                            count = 1;
                            int nextlen = length[i];
                            if (nextlen == 0)
                            {
                                max_count = 138;
                                min_count = 3;
                            }
                            else
                            {
                                max_count = 6;
                                min_count = 3;
                                if (curlen != nextlen)
                                {
                                    blTree.WriteSymbol(nextlen);
                                    count = 0;
                                }
                            }
                            curlen = nextlen;
                            i++;

                            while (i < numCodes && curlen == length[i])
                            {
                                i++;
                                if (++count >= max_count)
                                {
                                    break;
                                }
                            }

                            if (count < min_count)
                            {
                                while (count-- > 0)
                                {
                                    blTree.WriteSymbol(curlen);
                                }
                            }
                            else if (curlen != 0)
                            {
                                blTree.WriteSymbol(REP_3_6);
                                dh.pending.WriteBits(count - 3, 2);
                            }
                            else if (count <= 10)
                            {
                                blTree.WriteSymbol(REP_3_10);
                                dh.pending.WriteBits(count - 3, 3);
                            }
                            else
                            {
                                blTree.WriteSymbol(REP_11_138);
                                dh.pending.WriteBits(count - 11, 7);
                            }
                        }
                    }

                    void BuildLength(int[] childs)
                    {
                        this.length = new byte[freqs.Length];
                        int numNodes = childs.Length / 2;
                        int numLeafs = (numNodes + 1) / 2;
                        int overflow = 0;

                        for (int i = 0; i < maxLength; i++)
                        {
                            bl_counts[i] = 0;
                        }

                        // First calculate optimal bit lengths
                        int[] lengths = new int[numNodes];
                        lengths[numNodes - 1] = 0;

                        for (int i = numNodes - 1; i >= 0; i--)
                        {
                            if (childs[2 * i + 1] != -1)
                            {
                                int bitLength = lengths[i] + 1;
                                if (bitLength > maxLength)
                                {
                                    bitLength = maxLength;
                                    overflow++;
                                }
                                lengths[childs[2 * i]] = lengths[childs[2 * i + 1]] = bitLength;
                            }
                            else
                            {
                                // A leaf node
                                int bitLength = lengths[i];
                                bl_counts[bitLength - 1]++;
                                this.length[childs[2 * i]] = (byte)lengths[i];
                            }
                        }

                        //				if (DeflaterConstants.DEBUGGING) {
                        //					//Console.WriteLine("Tree "+freqs.Length+" lengths:");
                        //					for (int i=0; i < numLeafs; i++) {
                        //						//Console.WriteLine("Node "+childs[2*i]+" freq: "+freqs[childs[2*i]]
                        //						                  + " len: "+length[childs[2*i]]);
                        //					}
                        //				}

                        if (overflow == 0)
                        {
                            return;
                        }

                        int incrBitLen = maxLength - 1;
                        do
                        {
                            // Find the first bit length which could increase:
                            while (bl_counts[--incrBitLen] == 0)
                                ;

                            // Move this node one down and remove a corresponding
                            // number of overflow nodes.
                            do
                            {
                                bl_counts[incrBitLen]--;
                                bl_counts[++incrBitLen]++;
                                overflow -= 1 << (maxLength - 1 - incrBitLen);
                            } while (overflow > 0 && incrBitLen < maxLength - 1);
                        } while (overflow > 0);

                        /* We may have overshot above.  Move some nodes from maxLength to
                        * maxLength-1 in that case.
                        */
                        bl_counts[maxLength - 1] += overflow;
                        bl_counts[maxLength - 2] -= overflow;

                        /* Now recompute all bit lengths, scanning in increasing
                        * frequency.  It is simpler to reconstruct all lengths instead of
                        * fixing only the wrong ones. This idea is taken from 'ar'
                        * written by Haruhiko Okumura.
                        *
                        * The nodes were inserted with decreasing frequency into the childs
                        * array.
                        */
                        int nodePtr = 2 * numLeafs;
                        for (int bits = maxLength; bits != 0; bits--)
                        {
                            int n = bl_counts[bits - 1];
                            while (n > 0)
                            {
                                int childPtr = 2 * childs[nodePtr++];
                                if (childs[childPtr + 1] == -1)
                                {
                                    // We found another leaf
                                    length[childs[childPtr]] = (byte)bits;
                                    n--;
                                }
                            }
                        }
                        //				if (DeflaterConstants.DEBUGGING) {
                        //					//Console.WriteLine("*** After overflow elimination. ***");
                        //					for (int i=0; i < numLeafs; i++) {
                        //						//Console.WriteLine("Node "+childs[2*i]+" freq: "+freqs[childs[2*i]]
                        //						                  + " len: "+length[childs[2*i]]);
                        //					}
                        //				}
                    }

                }

                public DeflaterPending pending;
                Tree literalTree;
                Tree distTree;
                Tree blTree;
                short[] d_buf;
                byte[] l_buf;
                int last_lit;
                int extra_bits;

                static DeflaterHuffman()
                {
                    // See RFC 1951 3.2.6
                    // Literal codes
                    staticLCodes = new short[LITERAL_NUM];
                    staticLLength = new byte[LITERAL_NUM];

                    int i = 0;
                    while (i < 144)
                    {
                        staticLCodes[i] = BitReverse((0x030 + i) << 8);
                        staticLLength[i++] = 8;
                    }

                    while (i < 256)
                    {
                        staticLCodes[i] = BitReverse((0x190 - 144 + i) << 7);
                        staticLLength[i++] = 9;
                    }

                    while (i < 280)
                    {
                        staticLCodes[i] = BitReverse((0x000 - 256 + i) << 9);
                        staticLLength[i++] = 7;
                    }

                    while (i < LITERAL_NUM)
                    {
                        staticLCodes[i] = BitReverse((0x0c0 - 280 + i) << 8);
                        staticLLength[i++] = 8;
                    }

                    // Distance codes
                    staticDCodes = new short[DIST_NUM];
                    staticDLength = new byte[DIST_NUM];
                    for (i = 0; i < DIST_NUM; i++)
                    {
                        staticDCodes[i] = BitReverse(i << 11);
                        staticDLength[i] = 5;
                    }
                }

                /// <summary>
                /// Construct instance with pending buffer
                /// </summary>
                /// <param name="pending">Pending buffer to use</param>
                public DeflaterHuffman(DeflaterPending pending)
                {
                    this.pending = pending;

                    literalTree = new Tree(this, LITERAL_NUM, 257, 15);
                    distTree = new Tree(this, DIST_NUM, 1, 15);
                    blTree = new Tree(this, BITLEN_NUM, 4, 7);

                    d_buf = new short[BUFSIZE];
                    l_buf = new byte[BUFSIZE];
                }

                /// <summary>
                /// Reset internal state
                /// </summary>		
                public void Reset()
                {
                    last_lit = 0;
                    extra_bits = 0;
                    literalTree.Reset();
                    distTree.Reset();
                    blTree.Reset();
                }

                /// <summary>
                /// Write all trees to pending buffer
                /// </summary>
                /// <param name="blTreeCodes">The number/rank of treecodes to send.</param>
                public void SendAllTrees(int blTreeCodes)
                {
                    blTree.BuildCodes();
                    literalTree.BuildCodes();
                    distTree.BuildCodes();
                    pending.WriteBits(literalTree.numCodes - 257, 5);
                    pending.WriteBits(distTree.numCodes - 1, 5);
                    pending.WriteBits(blTreeCodes - 4, 4);
                    for (int rank = 0; rank < blTreeCodes; rank++)
                    {
                        pending.WriteBits(blTree.length[BL_ORDER[rank]], 3);
                    }
                    literalTree.WriteTree(blTree);
                    distTree.WriteTree(blTree);
                }

                /// <summary>
                /// Compress current buffer writing data to pending buffer
                /// </summary>
                public void CompressBlock()
                {
                    for (int i = 0; i < last_lit; i++)
                    {
                        int litlen = l_buf[i] & 0xff;
                        int dist = d_buf[i];
                        if (dist-- != 0)
                        {
                            //					if (DeflaterConstants.DEBUGGING) {
                            //						Console.Write("["+(dist+1)+","+(litlen+3)+"]: ");
                            //					}

                            int lc = Lcode(litlen);
                            literalTree.WriteSymbol(lc);

                            int bits = (lc - 261) / 4;
                            if (bits > 0 && bits <= 5)
                            {
                                pending.WriteBits(litlen & ((1 << bits) - 1), bits);
                            }

                            int dc = Dcode(dist);
                            distTree.WriteSymbol(dc);

                            bits = dc / 2 - 1;
                            if (bits > 0)
                            {
                                pending.WriteBits(dist & ((1 << bits) - 1), bits);
                            }
                        }
                        else
                        {
                            //					if (DeflaterConstants.DEBUGGING) {
                            //						if (litlen > 32 && litlen < 127) {
                            //							Console.Write("("+(char)litlen+"): ");
                            //						} else {
                            //							Console.Write("{"+litlen+"}: ");
                            //						}
                            //					}
                            literalTree.WriteSymbol(litlen);
                        }
                    }
                    literalTree.WriteSymbol(EOF_SYMBOL);
                }

                /// <summary>
                /// Flush block to output with no compression
                /// </summary>
                /// <param name="stored">Data to write</param>
                /// <param name="storedOffset">Index of first byte to write</param>
                /// <param name="storedLength">Count of bytes to write</param>
                /// <param name="lastBlock">True if this is the last block</param>
                public void FlushStoredBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
                {
                    pending.WriteBits((DeflaterConstants.STORED_BLOCK << 1) + (lastBlock ? 1 : 0), 3);
                    pending.AlignToByte();
                    pending.WriteShort(storedLength);
                    pending.WriteShort(~storedLength);
                    pending.WriteBlock(stored, storedOffset, storedLength);
                    Reset();
                }

                /// <summary>
                /// Flush block to output with compression
                /// </summary>		
                /// <param name="stored">Data to flush</param>
                /// <param name="storedOffset">Index of first byte to flush</param>
                /// <param name="storedLength">Count of bytes to flush</param>
                /// <param name="lastBlock">True if this is the last block</param>
                public void FlushBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
                {
                    literalTree.freqs[EOF_SYMBOL]++;

                    // Build trees
                    literalTree.BuildTree();
                    distTree.BuildTree();

                    // Calculate bitlen frequency
                    literalTree.CalcBLFreq(blTree);
                    distTree.CalcBLFreq(blTree);

                    // Build bitlen tree
                    blTree.BuildTree();

                    int blTreeCodes = 4;
                    for (int i = 18; i > blTreeCodes; i--)
                    {
                        if (blTree.length[BL_ORDER[i]] > 0)
                        {
                            blTreeCodes = i + 1;
                        }
                    }
                    int opt_len = 14 + blTreeCodes * 3 + blTree.GetEncodedLength() +
                        literalTree.GetEncodedLength() + distTree.GetEncodedLength() +
                        extra_bits;

                    int static_len = extra_bits;
                    for (int i = 0; i < LITERAL_NUM; i++)
                    {
                        static_len += literalTree.freqs[i] * staticLLength[i];
                    }
                    for (int i = 0; i < DIST_NUM; i++)
                    {
                        static_len += distTree.freqs[i] * staticDLength[i];
                    }
                    if (opt_len >= static_len)
                    {
                        // Force static trees
                        opt_len = static_len;
                    }

                    if (storedOffset >= 0 && storedLength + 4 < opt_len >> 3)
                    {
                        // Store Block

                        //				if (DeflaterConstants.DEBUGGING) {
                        //					//Console.WriteLine("Storing, since " + storedLength + " < " + opt_len
                        //					                  + " <= " + static_len);
                        //				}
                        FlushStoredBlock(stored, storedOffset, storedLength, lastBlock);
                    }
                    else if (opt_len == static_len)
                    {
                        // Encode with static tree
                        pending.WriteBits((DeflaterConstants.STATIC_TREES << 1) + (lastBlock ? 1 : 0), 3);
                        literalTree.SetStaticCodes(staticLCodes, staticLLength);
                        distTree.SetStaticCodes(staticDCodes, staticDLength);
                        CompressBlock();
                        Reset();
                    }
                    else
                    {
                        // Encode with dynamic tree
                        pending.WriteBits((DeflaterConstants.DYN_TREES << 1) + (lastBlock ? 1 : 0), 3);
                        SendAllTrees(blTreeCodes);
                        CompressBlock();
                        Reset();
                    }
                }

                /// <summary>
                /// Get value indicating if internal buffer is full
                /// </summary>
                /// <returns>true if buffer is full</returns>
                public bool IsFull()
                {
                    return last_lit >= BUFSIZE;
                }

                /// <summary>
                /// Add literal to buffer
                /// </summary>
                /// <param name="literal">Literal value to add to buffer.</param>
                /// <returns>Value indicating internal buffer is full</returns>
                public bool TallyLit(int literal)
                {
                    //			if (DeflaterConstants.DEBUGGING) {
                    //				if (lit > 32 && lit < 127) {
                    //					//Console.WriteLine("("+(char)lit+")");
                    //				} else {
                    //					//Console.WriteLine("{"+lit+"}");
                    //				}
                    //			}
                    d_buf[last_lit] = 0;
                    l_buf[last_lit++] = (byte)literal;
                    literalTree.freqs[literal]++;
                    return IsFull();
                }

                /// <summary>
                /// Add distance code and length to literal and distance trees
                /// </summary>
                /// <param name="distance">Distance code</param>
                /// <param name="length">Length</param>
                /// <returns>Value indicating if internal buffer is full</returns>
                public bool TallyDist(int distance, int length)
                {
                    //			if (DeflaterConstants.DEBUGGING) {
                    //				//Console.WriteLine("[" + distance + "," + length + "]");
                    //			}

                    d_buf[last_lit] = (short)distance;
                    l_buf[last_lit++] = (byte)(length - 3);

                    int lc = Lcode(length - 3);
                    literalTree.freqs[lc]++;
                    if (lc >= 265 && lc < 285)
                    {
                        extra_bits += (lc - 261) / 4;
                    }

                    int dc = Dcode(distance - 1);
                    distTree.freqs[dc]++;
                    if (dc >= 4)
                    {
                        extra_bits += dc / 2 - 1;
                    }
                    return IsFull();
                }

                /// <summary>
                /// Reverse the bits of a 16 bit value.
                /// </summary>
                /// <param name="toReverse">Value to reverse bits</param>
                /// <returns>Value with bits reversed</returns>
                public static short BitReverse(int toReverse)
                {
                    return (short)(bit4Reverse[toReverse & 0xF] << 12 |
                                    bit4Reverse[(toReverse >> 4) & 0xF] << 8 |
                                    bit4Reverse[(toReverse >> 8) & 0xF] << 4 |
                                    bit4Reverse[toReverse >> 12]);
                }

                static int Lcode(int length)
                {
                    if (length == 255)
                    {
                        return 285;
                    }

                    int code = 257;
                    while (length >= 8)
                    {
                        code += 4;
                        length >>= 1;
                    }
                    return code + length;
                }

                static int Dcode(int distance)
                {
                    int code = 0;
                    while (distance >= 4)
                    {
                        code += 2;
                        distance >>= 1;
                    }
                    return code + distance;
                }
            }

            public enum DeflateStrategy
            {
                /// <summary>
                /// The default strategy
                /// </summary>
                Default = 0,
                /// <summary>
                /// This strategy will only allow longer string repetitions.  It is
                /// useful for random data with a small character set.
                /// </summary>
                Filtered = 1,
                /// <summary>
                /// This strategy will not look for string repetitions at all.  It
                /// only encodes with Huffman trees (which means, that more common
                /// characters get a smaller encoding.
                /// </summary>
                HuffmanOnly = 2
            }

            public class DeflaterEngine : DeflaterConstants
            {
                const int TooFar = 4096;

                /// <summary>
                /// Construct instance with pending buffer
                /// </summary>
                /// <param name="pending">
                /// Pending buffer to use
                /// </param>>
                public DeflaterEngine(DeflaterPending pending)
                {
                    this.pending = pending;
                    huffman = new DeflaterHuffman(pending);
                    adler = new ZIPLib.Checksums.Adler32();

                    window = new byte[2 * WSIZE];
                    head = new short[HASH_SIZE];
                    prev = new short[WSIZE];

                    // We start at index 1, to avoid an implementation deficiency, that
                    // we cannot build a repeat pattern at index 0.
                    blockStart = strstart = 1;
                }

                /// <summary>
                /// Deflate drives actual compression of data
                /// </summary>
                /// <param name="flush">True to flush input buffers</param>
                /// <param name="finish">Finish deflation with the current input.</param>
                /// <returns>Returns true if progress has been made.</returns>
                public bool Deflate(bool flush, bool finish)
                {
                    bool progress;
                    do
                    {
                        FillWindow();
                        bool canFlush = flush && (inputOff == inputEnd);
                        switch (compressionFunction)
                        {
                            case DEFLATE_STORED:
                                progress = DeflateStored(canFlush, finish);
                                break;
                            case DEFLATE_FAST:
                                progress = DeflateFast(canFlush, finish);
                                break;
                            case DEFLATE_SLOW:
                                progress = DeflateSlow(canFlush, finish);
                                break;
                            default:
                                throw new Sys.InvalidOperationException("unknown compressionFunction");
                        }
                    } while (pending.IsFlushed && progress); // repeat while we have no pending output and progress was made
                    return progress;
                }

                /// <summary>
                /// Sets input data to be deflated.  Should only be called when <code>NeedsInput()</code>
                /// returns true
                /// </summary>
                /// <param name="buffer">The buffer containing input data.</param>
                /// <param name="offset">The offset of the first byte of data.</param>
                /// <param name="count">The number of bytes of data to use as input.</param>
                public void SetInput(byte[] buffer, int offset, int count)
                {
                    if (buffer == null)
                    {
                        throw new Sys.ArgumentNullException("buffer");
                    }

                    if (offset < 0)
                    {
                        throw new Sys.ArgumentOutOfRangeException("offset");
                    }

                    if (count < 0)
                    {
                        throw new Sys.ArgumentOutOfRangeException("count");
                    }

                    if (inputOff < inputEnd)
                    {
                        throw new Sys.InvalidOperationException("Old input was not completely processed");
                    }

                    int end = offset + count;

                    /* We want to throw an ArrayIndexOutOfBoundsException early.  The
                    * check is very tricky: it also handles integer wrap around.
                    */
                    if ((offset > end) || (end > buffer.Length))
                    {
                        throw new Sys.ArgumentOutOfRangeException("count");
                    }

                    inputBuf = buffer;
                    inputOff = offset;
                    inputEnd = end;
                }

                /// <summary>
                /// Determines if more <see cref="SetInput">input</see> is needed.
                /// </summary>		
                /// <returns>Return true if input is needed via <see cref="SetInput">SetInput</see></returns>
                public bool NeedsInput()
                {
                    return (inputEnd == inputOff);
                }

                /// <summary>
                /// Set compression dictionary
                /// </summary>
                /// <param name="buffer">The buffer containing the dictionary data</param>
                /// <param name="offset">The offset in the buffer for the first byte of data</param>
                /// <param name="length">The length of the dictionary data.</param>
                public void SetDictionary(byte[] buffer, int offset, int length)
                {
                    adler.Update(buffer, offset, length);
                    if (length < MIN_MATCH)
                    {
                        return;
                    }

                    if (length > MAX_DIST)
                    {
                        offset += length - MAX_DIST;
                        length = MAX_DIST;
                    }

                    Sys.Array.Copy(buffer, offset, window, strstart, length);

                    UpdateHash();
                    --length;
                    while (--length > 0)
                    {
                        InsertString();
                        strstart++;
                    }
                    strstart += 2;
                    blockStart = strstart;
                }

                /// <summary>
                /// Reset internal state
                /// </summary>		
                public void Reset()
                {
                    huffman.Reset();
                    adler.Reset();
                    blockStart = strstart = 1;
                    lookahead = 0;
                    totalIn = 0;
                    prevAvailable = false;
                    matchLen = MIN_MATCH - 1;

                    for (int i = 0; i < HASH_SIZE; i++)
                    {
                        head[i] = 0;
                    }

                    for (int i = 0; i < WSIZE; i++)
                    {
                        prev[i] = 0;
                    }
                }

                /// <summary>
                /// Reset Adler checksum
                /// </summary>		
                public void ResetAdler()
                {
                    adler.Reset();
                }

                /// <summary>
                /// Get current value of Adler checksum
                /// </summary>		
                public int Adler
                {
                    get
                    {
                        return unchecked((int)adler.Value);
                    }
                }

                /// <summary>
                /// Total data processed
                /// </summary>		
                public long TotalIn
                {
                    get
                    {
                        return totalIn;
                    }
                }

                /// <summary>
                /// Get/set the <see cref="DeflateStrategy">deflate strategy</see>
                /// </summary>		
                public DeflateStrategy Strategy
                {
                    get
                    {
                        return strategy;
                    }
                    set
                    {
                        strategy = value;
                    }
                }

                /// <summary>
                /// Set the deflate level (0-9)
                /// </summary>
                /// <param name="level">The value to set the level to.</param>
                public void SetLevel(int level)
                {
                    if ((level < 0) || (level > 9))
                    {
                        throw new Sys.ArgumentOutOfRangeException("level");
                    }

                    goodLength = DeflaterConstants.GOOD_LENGTH[level];
                    max_lazy = DeflaterConstants.MAX_LAZY[level];
                    niceLength = DeflaterConstants.NICE_LENGTH[level];
                    max_chain = DeflaterConstants.MAX_CHAIN[level];

                    if (DeflaterConstants.COMPR_FUNC[level] != compressionFunction)
                    {
                        switch (compressionFunction)
                        {
                            case DEFLATE_STORED:
                                if (strstart > blockStart)
                                {
                                    huffman.FlushStoredBlock(window, blockStart,
                                        strstart - blockStart, false);
                                    blockStart = strstart;
                                }
                                UpdateHash();
                                break;

                            case DEFLATE_FAST:
                                if (strstart > blockStart)
                                {
                                    huffman.FlushBlock(window, blockStart, strstart - blockStart,
                                        false);
                                    blockStart = strstart;
                                }
                                break;

                            case DEFLATE_SLOW:
                                if (prevAvailable)
                                {
                                    huffman.TallyLit(window[strstart - 1] & 0xff);
                                }
                                if (strstart > blockStart)
                                {
                                    huffman.FlushBlock(window, blockStart, strstart - blockStart, false);
                                    blockStart = strstart;
                                }
                                prevAvailable = false;
                                matchLen = MIN_MATCH - 1;
                                break;
                        }
                        compressionFunction = COMPR_FUNC[level];
                    }
                }

                /// <summary>
                /// Fill the window
                /// </summary>
                public void FillWindow()
                {
                    /* If the window is almost full and there is insufficient lookahead,
                     * move the upper half to the lower one to make room in the upper half.
                     */
                    if (strstart >= WSIZE + MAX_DIST)
                    {
                        SlideWindow();
                    }

                    /* If there is not enough lookahead, but still some input left,
                     * read in the input
                     */
                    while (lookahead < DeflaterConstants.MIN_LOOKAHEAD && inputOff < inputEnd)
                    {
                        int more = 2 * WSIZE - lookahead - strstart;

                        if (more > inputEnd - inputOff)
                        {
                            more = inputEnd - inputOff;
                        }

                        Sys.Array.Copy(inputBuf, inputOff, window, strstart + lookahead, more);
                        adler.Update(inputBuf, inputOff, more);

                        inputOff += more;
                        totalIn += more;
                        lookahead += more;
                    }

                    if (lookahead >= MIN_MATCH)
                    {
                        UpdateHash();
                    }
                }

                void UpdateHash()
                {
                    /*
                                if (DEBUGGING) {
                                    Console.WriteLine("updateHash: "+strstart);
                                }
                    */
                    ins_h = (window[strstart] << HASH_SHIFT) ^ window[strstart + 1];
                }

                /// <summary>
                /// Inserts the current string in the head hash and returns the previous
                /// value for this hash.
                /// </summary>
                /// <returns>The previous hash value</returns>
                int InsertString()
                {
                    short match;
                    int hash = ((ins_h << HASH_SHIFT) ^ window[strstart + (MIN_MATCH - 1)]) & HASH_MASK;
                    prev[strstart & WMASK] = match = head[hash];
                    head[hash] = unchecked((short)strstart);
                    ins_h = hash;
                    return match & 0xffff;
                }

                void SlideWindow()
                {
                    Sys.Array.Copy(window, WSIZE, window, 0, WSIZE);
                    matchStart -= WSIZE;
                    strstart -= WSIZE;
                    blockStart -= WSIZE;

                    // Slide the hash table (could be avoided with 32 bit values
                    // at the expense of memory usage).
                    for (int i = 0; i < HASH_SIZE; ++i)
                    {
                        int m = head[i] & 0xffff;
                        head[i] = (short)(m >= WSIZE ? (m - WSIZE) : 0);
                    }

                    // Slide the prev table.
                    for (int i = 0; i < WSIZE; i++)
                    {
                        int m = prev[i] & 0xffff;
                        prev[i] = (short)(m >= WSIZE ? (m - WSIZE) : 0);
                    }
                }

                /// <summary>
                /// Find the best (longest) string in the window matching the 
                /// string starting at strstart.
                ///
                /// Preconditions:
                /// <code>
                /// strstart + MAX_MATCH &lt;= window.length.</code>
                /// </summary>
                /// <param name="curMatch"></param>
                /// <returns>True if a match greater than the minimum length is found</returns>
                bool FindLongestMatch(int curMatch)
                {
                    int chainLength = this.max_chain;
                    int niceLength = this.niceLength;
                    short[] prev = this.prev;
                    int scan = this.strstart;
                    int match;
                    int best_end = this.strstart + matchLen;
                    int best_len = SysMath.Max(matchLen, MIN_MATCH - 1);

                    int limit = SysMath.Max(strstart - MAX_DIST, 0);

                    int strend = strstart + MAX_MATCH - 1;
                    byte scan_end1 = window[best_end - 1];
                    byte scan_end = window[best_end];

                    // Do not waste too much time if we already have a good match:
                    if (best_len >= this.goodLength)
                    {
                        chainLength >>= 2;
                    }

                    /* Do not look for matches beyond the end of the input. This is necessary
                    * to make deflate deterministic.
                    */
                    if (niceLength > lookahead)
                    {
                        niceLength = lookahead;
                    }

                    do
                    {
                        if (window[curMatch + best_len] != scan_end ||
                            window[curMatch + best_len - 1] != scan_end1 ||
                            window[curMatch] != window[scan] ||
                            window[curMatch + 1] != window[scan + 1])
                        {
                            continue;
                        }

                        match = curMatch + 2;
                        scan += 2;

                        /* We check for insufficient lookahead only every 8th comparison;
                        * the 256th check will be made at strstart + 258.
                        */
                        while (
                            window[++scan] == window[++match] &&
                            window[++scan] == window[++match] &&
                            window[++scan] == window[++match] &&
                            window[++scan] == window[++match] &&
                            window[++scan] == window[++match] &&
                            window[++scan] == window[++match] &&
                            window[++scan] == window[++match] &&
                            window[++scan] == window[++match] &&
                            (scan < strend))
                        {
                            // Do nothing
                        }

                        if (scan > best_end)
                        {
                            matchStart = curMatch;
                            best_end = scan;
                            best_len = scan - strstart;

                            if (best_len >= niceLength)
                            {
                                break;
                            }

                            scan_end1 = window[best_end - 1];
                            scan_end = window[best_end];
                        }
                        scan = strstart;
                    } while ((curMatch = (prev[curMatch & WMASK] & 0xffff)) > limit && --chainLength != 0);

                    matchLen = SysMath.Min(best_len, lookahead);
                    return matchLen >= MIN_MATCH;
                }

                bool DeflateStored(bool flush, bool finish)
                {
                    if (!flush && (lookahead == 0))
                    {
                        return false;
                    }

                    strstart += lookahead;
                    lookahead = 0;

                    int storedLength = strstart - blockStart;

                    if ((storedLength >= DeflaterConstants.MAX_BLOCK_SIZE) || // Block is full
                        (blockStart < WSIZE && storedLength >= MAX_DIST) ||   // Block may move out of window
                        flush)
                    {
                        bool lastBlock = finish;
                        if (storedLength > DeflaterConstants.MAX_BLOCK_SIZE)
                        {
                            storedLength = DeflaterConstants.MAX_BLOCK_SIZE;
                            lastBlock = false;
                        }

                        huffman.FlushStoredBlock(window, blockStart, storedLength, lastBlock);
                        blockStart += storedLength;
                        return !lastBlock;
                    }
                    return true;
                }

                bool DeflateFast(bool flush, bool finish)
                {
                    if (lookahead < MIN_LOOKAHEAD && !flush)
                    {
                        return false;
                    }

                    while (lookahead >= MIN_LOOKAHEAD || flush)
                    {
                        if (lookahead == 0)
                        {
                            // We are flushing everything
                            huffman.FlushBlock(window, blockStart, strstart - blockStart, finish);
                            blockStart = strstart;
                            return false;
                        }

                        if (strstart > 2 * WSIZE - MIN_LOOKAHEAD)
                        {
                            /* slide window, as FindLongestMatch needs this.
                             * This should only happen when flushing and the window
                             * is almost full.
                             */
                            SlideWindow();
                        }

                        int hashHead;
                        if (lookahead >= MIN_MATCH &&
                            (hashHead = InsertString()) != 0 &&
                            strategy != DeflateStrategy.HuffmanOnly &&
                            strstart - hashHead <= MAX_DIST &&
                            FindLongestMatch(hashHead))
                        {
                            // longestMatch sets matchStart and matchLen			

                            bool full = huffman.TallyDist(strstart - matchStart, matchLen);

                            lookahead -= matchLen;
                            if (matchLen <= max_lazy && lookahead >= MIN_MATCH)
                            {
                                while (--matchLen > 0)
                                {
                                    ++strstart;
                                    InsertString();
                                }
                                ++strstart;
                            }
                            else
                            {
                                strstart += matchLen;
                                if (lookahead >= MIN_MATCH - 1)
                                {
                                    UpdateHash();
                                }
                            }
                            matchLen = MIN_MATCH - 1;
                            if (!full)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            // No match found
                            huffman.TallyLit(window[strstart] & 0xff);
                            ++strstart;
                            --lookahead;
                        }

                        if (huffman.IsFull())
                        {
                            bool lastBlock = finish && (lookahead == 0);
                            huffman.FlushBlock(window, blockStart, strstart - blockStart, lastBlock);
                            blockStart = strstart;
                            return !lastBlock;
                        }
                    }
                    return true;
                }

                bool DeflateSlow(bool flush, bool finish)
                {
                    if (lookahead < MIN_LOOKAHEAD && !flush)
                    {
                        return false;
                    }

                    while (lookahead >= MIN_LOOKAHEAD || flush)
                    {
                        if (lookahead == 0)
                        {
                            if (prevAvailable)
                            {
                                huffman.TallyLit(window[strstart - 1] & 0xff);
                            }
                            prevAvailable = false;

                            // We are flushing everything             
                            huffman.FlushBlock(window, blockStart, strstart - blockStart,
                                finish);
                            blockStart = strstart;
                            return false;
                        }

                        if (strstart >= 2 * WSIZE - MIN_LOOKAHEAD)
                        {
                            /* slide window, as FindLongestMatch needs this.
                             * This should only happen when flushing and the window
                             * is almost full.
                             */
                            SlideWindow();
                        }

                        int prevMatch = matchStart;
                        int prevLen = matchLen;
                        if (lookahead >= MIN_MATCH)
                        {

                            int hashHead = InsertString();

                            if (strategy != DeflateStrategy.HuffmanOnly &&
                                hashHead != 0 &&
                                strstart - hashHead <= MAX_DIST &&
                                FindLongestMatch(hashHead))
                            {

                                // longestMatch sets matchStart and matchLen

                                // Discard match if too small and too far away
                                if (matchLen <= 5 && (strategy == DeflateStrategy.Filtered || (matchLen == MIN_MATCH && strstart - matchStart > TooFar)))
                                {
                                    matchLen = MIN_MATCH - 1;
                                }
                            }
                        }

                        // previous match was better
                        if ((prevLen >= MIN_MATCH) && (matchLen <= prevLen))
                        {
                            huffman.TallyDist(strstart - 1 - prevMatch, prevLen);
                            prevLen -= 2;
                            do
                            {
                                strstart++;
                                lookahead--;
                                if (lookahead >= MIN_MATCH)
                                {
                                    InsertString();
                                }
                            } while (--prevLen > 0);

                            strstart++;
                            lookahead--;
                            prevAvailable = false;
                            matchLen = MIN_MATCH - 1;
                        }
                        else
                        {
                            if (prevAvailable)
                            {
                                huffman.TallyLit(window[strstart - 1] & 0xff);
                            }
                            prevAvailable = true;
                            strstart++;
                            lookahead--;
                        }

                        if (huffman.IsFull())
                        {
                            int len = strstart - blockStart;
                            if (prevAvailable)
                            {
                                len--;
                            }
                            bool lastBlock = (finish && (lookahead == 0) && !prevAvailable);
                            huffman.FlushBlock(window, blockStart, len, lastBlock);
                            blockStart += len;
                            return !lastBlock;
                        }
                    }
                    return true;
                }

                int ins_h;

                /// <summary>
                /// Hashtable, hashing three characters to an index for window, so
                /// that window[index]..window[index+2] have this hash code.  
                /// Note that the array should really be unsigned short, so you need
                /// to and the values with 0xffff.
                /// </summary>
                short[] head;

                /// <summary>
                /// <code>prev[index &amp; WMASK]</code> points to the previous index that has the
                /// same hash code as the string starting at index.  This way 
                /// entries with the same hash code are in a linked list.
                /// Note that the array should really be unsigned short, so you need
                /// to and the values with 0xffff.
                /// </summary>
                short[] prev;

                int matchStart;
                int matchLen;
                bool prevAvailable;
                int blockStart;

                /// <summary>
                /// Points to the current character in the window.
                /// </summary>
                int strstart;

                /// <summary>
                /// lookahead is the number of characters starting at strstart in
                /// window that are valid.
                /// So window[strstart] until window[strstart+lookahead-1] are valid
                /// characters.
                /// </summary>
                int lookahead;

                /// <summary>
                /// This array contains the part of the uncompressed stream that 
                /// is of relevance.  The current character is indexed by strstart.
                /// </summary>
                byte[] window;

                DeflateStrategy strategy;
                int max_chain, max_lazy, niceLength, goodLength;

                /// <summary>
                /// The current compression function.
                /// </summary>
                int compressionFunction;

                /// <summary>
                /// The input data for compression.
                /// </summary>
                byte[] inputBuf;

                /// <summary>
                /// The total bytes of input read.
                /// </summary>
                long totalIn;

                /// <summary>
                /// The offset into inputBuf, where input data starts.
                /// </summary>
                int inputOff;

                /// <summary>
                /// The end offset of the input data.
                /// </summary>
                int inputEnd;

                DeflaterPending pending;
                DeflaterHuffman huffman;

                /// <summary>
                /// The adler checksum
                /// </summary>
                ZIPLib.Checksums.Adler32 adler;
            }

            public class DeflaterConstants
            {
                /// <summary>
                /// Set to true to enable SysDiag.Debugging
                /// </summary>
                public const bool DebugGING = false;

                /// <summary>
                /// Written to Zip file to identify a stored block
                /// </summary>
                public const int STORED_BLOCK = 0;

                /// <summary>
                /// Identifies static tree in Zip file
                /// </summary>
                public const int STATIC_TREES = 1;

                /// <summary>
                /// Identifies dynamic tree in Zip file
                /// </summary>
                public const int DYN_TREES = 2;

                /// <summary>
                /// Header flag indicating a preset dictionary for deflation
                /// </summary>
                public const int PRESET_DICT = 0x20;

                /// <summary>
                /// Sets internal buffer sizes for Huffman encoding
                /// </summary>
                public const int DEFAULT_MEM_LEVEL = 8;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int MAX_MATCH = 258;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int MIN_MATCH = 3;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int MAX_WBITS = 15;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int WSIZE = 1 << MAX_WBITS;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int WMASK = WSIZE - 1;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int HASH_BITS = DEFAULT_MEM_LEVEL + 7;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int HASH_SIZE = 1 << HASH_BITS;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int HASH_MASK = HASH_SIZE - 1;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int HASH_SHIFT = (HASH_BITS + MIN_MATCH - 1) / MIN_MATCH;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int MIN_LOOKAHEAD = MAX_MATCH + MIN_MATCH + 1;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int MAX_DIST = WSIZE - MIN_LOOKAHEAD;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int PENDING_BUF_SIZE = 1 << (DEFAULT_MEM_LEVEL + 8);

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public static int MAX_BLOCK_SIZE = SysMath.Min(65535, PENDING_BUF_SIZE - 5);

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int DEFLATE_STORED = 0;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int DEFLATE_FAST = 1;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public const int DEFLATE_SLOW = 2;

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public static int[] GOOD_LENGTH = { 0, 4, 4, 4, 4, 8, 8, 8, 32, 32 };

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public static int[] MAX_LAZY = { 0, 4, 5, 6, 4, 16, 16, 32, 128, 258 };

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public static int[] NICE_LENGTH = { 0, 8, 16, 32, 16, 32, 128, 128, 258, 258 };

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public static int[] MAX_CHAIN = { 0, 4, 8, 32, 16, 32, 128, 256, 1024, 4096 };

                /// <summary>
                /// Internal compression engine constant
                /// </summary>		
                public static int[] COMPR_FUNC = { 0, 1, 1, 1, 1, 2, 2, 2, 2, 2 };

            }

            namespace Streams
            {
                public class DeflaterOutputStream : Sys.IO.Stream
                {
                    public DeflaterOutputStream(Sys.IO.Stream baseOutputStream) : this(baseOutputStream, new Deflater(), 512) { /* NOTHING */ }
                    public DeflaterOutputStream(Sys.IO.Stream baseOutputStream, Deflater deflater) : this(baseOutputStream, deflater, 512) { /* NOTHING */ }

                    public DeflaterOutputStream(Sys.IO.Stream baseOutputStream, Deflater deflater, int bufferSize)
                    {
                        if (baseOutputStream == null)
                        {
                            throw new Sys.ArgumentNullException("baseOutputStream");
                        }

                        if (baseOutputStream.CanWrite == false)
                        {
                            throw new Sys.ArgumentException("Must support writing", "baseOutputStream");
                        }

                        if (deflater == null)
                        {
                            throw new Sys.ArgumentNullException("deflater");
                        }

                        if (bufferSize < 512)
                        {
                            throw new Sys.ArgumentOutOfRangeException("bufferSize");
                        }

                        baseOutputStream_ = baseOutputStream;
                        buffer_ = new byte[bufferSize];
                        deflater_ = deflater;
                    }

                    public virtual void Finish()
                    {
                        deflater_.Finish();
                        while (!deflater_.IsFinished)
                        {
                            int len = deflater_.Deflate(buffer_, 0, buffer_.Length);
                            if (len <= 0)
                            {
                                break;
                            }

                            if (cryptoTransform_ != null)
                            {
                                EncryptBlock(buffer_, 0, len);
                            }

                            baseOutputStream_.Write(buffer_, 0, len);
                        }

                        if (!deflater_.IsFinished)
                        {
                            throw new Sys.ApplicationException("Can't deflate all input?");
                        }

                        baseOutputStream_.Flush();

                        if (cryptoTransform_ != null)
                        {
                            if (cryptoTransform_ is ZIPLib.Encryption.ZipAESTransform)
                            {
                                AESAuthCode = ((ZIPLib.Encryption.ZipAESTransform)cryptoTransform_).GetAuthCode();
                            }
                            cryptoTransform_.Dispose();
                            cryptoTransform_ = null;
                        }
                    }

                    public bool IsStreamOwner
                    {
                        get { return isStreamOwner_; }
                        set { isStreamOwner_ = value; }
                    }

                    public bool CanPatchEntries
                    {
                        get
                        {
                            return baseOutputStream_.CanSeek;
                        }
                    }

                    string password;
                    Sys.Security.Cryptography.ICryptoTransform cryptoTransform_;

                    protected byte[] AESAuthCode;

                    public string Password
                    {
                        get
                        {
                            return password;
                        }
                        set
                        {
                            if ((value != null) && (value.Length == 0))
                            {
                                password = null;
                            }
                            else
                            {
                                password = value;
                            }
                        }
                    }

                    protected void EncryptBlock(byte[] buffer, int offset, int length)
                    {
                        cryptoTransform_.TransformBlock(buffer, 0, length, buffer, 0);
                    }

                    protected void InitializePassword(string password)
                    {
                        ZIPLib.Encryption.PkzipClassicManaged pkManaged = new ZIPLib.Encryption.PkzipClassicManaged();
                        byte[] key = ZIPLib.Encryption.PkzipClassic.GenerateKeys(ZipConstants.ConvertToArray(password));
                        cryptoTransform_ = pkManaged.CreateEncryptor(key, null);
                    }

                    protected void InitializeAESPassword(ZipEntry entry, string rawPassword, out byte[] salt, out byte[] pwdVerifier)
                    {
                        salt = new byte[entry.AESSaltLen];
                        // Salt needs to be cryptographically random, and unique per file
                        if (_aesRnd == null)
                            _aesRnd = new Sys.Security.Cryptography.RNGCryptoServiceProvider();
                        _aesRnd.GetBytes(salt);
                        int blockSize = entry.AESKeySize / 8;	// bits to bytes

                        cryptoTransform_ = new ZIPLib.Encryption.ZipAESTransform(rawPassword, salt, blockSize, true);
                        pwdVerifier = ((ZIPLib.Encryption.ZipAESTransform)cryptoTransform_).PwdVerifier;
                    }

                    protected void Deflate()
                    {
                        while (!deflater_.IsNeedingInput)
                        {
                            int deflateCount = deflater_.Deflate(buffer_, 0, buffer_.Length);
                            if (deflateCount <= 0) { break; }
                            if (cryptoTransform_ != null) { EncryptBlock(buffer_, 0, deflateCount); }
                            baseOutputStream_.Write(buffer_, 0, deflateCount);
                        }
                        if (!deflater_.IsNeedingInput) { throw new Sys.ApplicationException("DeflaterOutputStream can't deflate all input?"); }
                    }

                    public override bool CanRead
                    {
                        get
                        {
                            return false;
                        }
                    }

                    public override bool CanSeek
                    {
                        get
                        {
                            return false;
                        }
                    }

                    public override bool CanWrite
                    {
                        get
                        {
                            return baseOutputStream_.CanWrite;
                        }
                    }

                    public override long Length
                    {
                        get
                        {
                            return baseOutputStream_.Length;
                        }
                    }

                    public override long Position
                    {
                        get
                        {
                            return baseOutputStream_.Position;
                        }
                        set
                        {
                            throw new Sys.NotSupportedException("Position property not supported");
                        }
                    }

                    public override long Seek(long offset, Sys.IO.SeekOrigin origin)
                    {
                        throw new Sys.NotSupportedException("DeflaterOutputStream Seek not supported");
                    }

                    public override void SetLength(long value)
                    {
                        throw new Sys.NotSupportedException("DeflaterOutputStream SetLength not supported");
                    }

                    public override int ReadByte() { throw new Sys.NotSupportedException("DeflaterOutputStream ReadByte not supported"); }
                    public override int Read(byte[] buffer, int offset, int count) { throw new Sys.NotSupportedException("DeflaterOutputStream Read not supported"); }

                    public override Sys.IAsyncResult BeginRead(byte[] buffer, int offset, int count, Sys.AsyncCallback callback, object state)
                    {
                        throw new Sys.NotSupportedException("DeflaterOutputStream BeginRead not currently supported");
                    }

                    public override Sys.IAsyncResult BeginWrite(byte[] buffer, int offset, int count, Sys.AsyncCallback callback, object state)
                    {
                        throw new Sys.NotSupportedException("BeginWrite is not supported");
                    }

                    public override void Flush()
                    {
                        deflater_.Flush();
                        Deflate();
                        baseOutputStream_.Flush();
                    }

                    public override void Close()
                    {
                        if (!isClosed_)
                        {
                            isClosed_ = true;

                            try
                            {
                                Finish();
                                if (cryptoTransform_ != null)
                                {
                                    GetAuthCodeIfAES();
                                    cryptoTransform_.Dispose();
                                    cryptoTransform_ = null;
                                }
                            }
                            finally
                            {
                                if (isStreamOwner_)
                                {
                                    baseOutputStream_.Close();
                                }
                            }
                        }
                    }

                    private void GetAuthCodeIfAES()
                    {
                        if (cryptoTransform_ is ZIPLib.Encryption.ZipAESTransform)
                        {
                            AESAuthCode = ((ZIPLib.Encryption.ZipAESTransform)cryptoTransform_).GetAuthCode();
                        }
                    }

                    public override void WriteByte(byte value)
                    {
                        byte[] b = new byte[1];
                        b[0] = value;
                        Write(b, 0, 1);
                    }

                    public override void Write(byte[] buffer, int offset, int count)
                    {
                        deflater_.SetInput(buffer, offset, count);
                        Deflate();
                    }

                    byte[] buffer_;
                    protected Deflater deflater_;
                    protected Sys.IO.Stream baseOutputStream_;
                    bool isClosed_;
                    bool isStreamOwner_ = true;
                    private static Sys.Security.Cryptography.RNGCryptoServiceProvider _aesRnd;
                }

                public class InflaterInputBuffer
                {
                    public InflaterInputBuffer(Sys.IO.Stream stream) : this(stream, 4096) { /* NOTHING */ }

                    public InflaterInputBuffer(Sys.IO.Stream stream, int bufferSize)
                    {
                        inputStream = stream;
                        if (bufferSize < 1024)
                        {
                            bufferSize = 1024;
                        }
                        rawData = new byte[bufferSize];
                        clearText = rawData;
                    }

                    public int RawLength
                    {
                        get
                        {
                            return rawLength;
                        }
                    }

                    public byte[] RawData
                    {
                        get
                        {
                            return rawData;
                        }
                    }

                    public int ClearTextLength
                    {
                        get
                        {
                            return clearTextLength;
                        }
                    }

                    public byte[] ClearText
                    {
                        get
                        {
                            return clearText;
                        }
                    }

                    public int Available
                    {
                        get { return available; }
                        set { available = value; }
                    }

                    public void SetInflaterInput(Inflater inflater)
                    {
                        if (available > 0)
                        {
                            inflater.SetInput(clearText, clearTextLength - available, available);
                            available = 0;
                        }
                    }

                    public void Fill()
                    {
                        rawLength = 0;
                        int toRead = rawData.Length;

                        while (toRead > 0)
                        {
                            int count = inputStream.Read(rawData, rawLength, toRead);
                            if (count <= 0)
                            {
                                break;
                            }
                            rawLength += count;
                            toRead -= count;
                        }

                        if (cryptoTransform != null)
                        {
                            clearTextLength = cryptoTransform.TransformBlock(rawData, 0, rawLength, clearText, 0);
                        }
                        else
                        {
                            clearTextLength = rawLength;
                        }

                        available = clearTextLength;
                    }

                    public int ReadRawBuffer(byte[] buffer)
                    {
                        return ReadRawBuffer(buffer, 0, buffer.Length);
                    }

                    public int ReadRawBuffer(byte[] outBuffer, int offset, int length)
                    {
                        if (length < 0)
                        {
                            throw new Sys.ArgumentOutOfRangeException("length");
                        }

                        int currentOffset = offset;
                        int currentLength = length;

                        while (currentLength > 0)
                        {
                            if (available <= 0)
                            {
                                Fill();
                                if (available <= 0)
                                {
                                    return 0;
                                }
                            }
                            int toCopy = SysMath.Min(currentLength, available);
                            Sys.Array.Copy(rawData, rawLength - (int)available, outBuffer, currentOffset, toCopy);
                            currentOffset += toCopy;
                            currentLength -= toCopy;
                            available -= toCopy;
                        }
                        return length;
                    }

                    public int ReadClearTextBuffer(byte[] outBuffer, int offset, int length)
                    {
                        if (length < 0)
                        {
                            throw new Sys.ArgumentOutOfRangeException("length");
                        }

                        int currentOffset = offset;
                        int currentLength = length;

                        while (currentLength > 0)
                        {
                            if (available <= 0)
                            {
                                Fill();
                                if (available <= 0)
                                {
                                    return 0;
                                }
                            }

                            int toCopy = SysMath.Min(currentLength, available);
                            Sys.Array.Copy(clearText, clearTextLength - (int)available, outBuffer, currentOffset, toCopy);
                            currentOffset += toCopy;
                            currentLength -= toCopy;
                            available -= toCopy;
                        }
                        return length;
                    }

                    public int ReadLeByte()
                    {
                        if (available <= 0)
                        {
                            Fill();
                            if (available <= 0)
                            {
                                throw new Sys.ApplicationException("EOF in header");
                            }
                        }
                        byte result = rawData[rawLength - available];
                        available -= 1;
                        return result;
                    }

                    public int ReadLeShort()
                    {
                        return ReadLeByte() | (ReadLeByte() << 8);
                    }

                    public int ReadLeInt()
                    {
                        return ReadLeShort() | (ReadLeShort() << 16);
                    }

                    public long ReadLeLong()
                    {
                        return (uint)ReadLeInt() | ((long)ReadLeInt() << 32);
                    }

                    public Sys.Security.Cryptography.ICryptoTransform CryptoTransform
                    {
                        set
                        {
                            cryptoTransform = value;
                            if (cryptoTransform != null)
                            {
                                if (rawData == clearText)
                                {
                                    if (internalClearText == null)
                                    {
                                        internalClearText = new byte[rawData.Length];
                                    }
                                    clearText = internalClearText;
                                }
                                clearTextLength = rawLength;
                                if (available > 0)
                                {
                                    cryptoTransform.TransformBlock(rawData, rawLength - available, available, clearText, rawLength - available);
                                }
                            }
                            else
                            {
                                clearText = rawData;
                                clearTextLength = rawLength;
                            }
                        }
                    }

                    int rawLength;
                    byte[] rawData;
                    int clearTextLength;
                    byte[] clearText;
                    byte[] internalClearText;
                    int available;
                    Sys.Security.Cryptography.ICryptoTransform cryptoTransform;
                    Sys.IO.Stream inputStream;
                }

                public class InflaterInputStream : Sys.IO.Stream
                {
                    public InflaterInputStream(Sys.IO.Stream baseInputStream) : this(baseInputStream, new Inflater(), 4096) { /* NOTHING */ }
                    public InflaterInputStream(Sys.IO.Stream baseInputStream, Inflater inf) : this(baseInputStream, inf, 4096) { /* NOTHING */ }

                    public InflaterInputStream(Sys.IO.Stream baseInputStream, Inflater inflater, int bufferSize)
                    {
                        if (baseInputStream == null)
                        {
                            throw new Sys.ArgumentNullException("baseInputStream");
                        }

                        if (inflater == null)
                        {
                            throw new Sys.ArgumentNullException("inflater");
                        }

                        if (bufferSize <= 0)
                        {
                            throw new Sys.ArgumentOutOfRangeException("bufferSize");
                        }

                        this.baseInputStream = baseInputStream;
                        this.inf = inflater;

                        inputBuffer = new InflaterInputBuffer(baseInputStream, bufferSize);
                    }

                    public bool IsStreamOwner
                    {
                        get { return isStreamOwner; }
                        set { isStreamOwner = value; }
                    }

                    public long Skip(long count)
                    {
                        if (count <= 0)
                        {
                            throw new Sys.ArgumentOutOfRangeException("count");
                        }

                        // v0.80 Skip by seeking if underlying stream supports it...
                        if (baseInputStream.CanSeek)
                        {
                            baseInputStream.Seek(count, Sys.IO.SeekOrigin.Current);
                            return count;
                        }
                        else
                        {
                            int length = 2048;
                            if (count < length)
                            {
                                length = (int)count;
                            }

                            byte[] tmp = new byte[length];
                            int readCount = 1;
                            long toSkip = count;

                            while ((toSkip > 0) && (readCount > 0))
                            {
                                if (toSkip < length)
                                {
                                    length = (int)toSkip;
                                }

                                readCount = baseInputStream.Read(tmp, 0, length);
                                toSkip -= readCount;
                            }

                            return count - toSkip;
                        }
                    }

                    protected void StopDecrypting()
                    {
                        inputBuffer.CryptoTransform = null;
                    }

                    public virtual int Available
                    {
                        get
                        {
                            return inf.IsFinished ? 0 : 1;
                        }
                    }

                    protected void Fill()
                    {
                        // Protect against redundant calls
                        if (inputBuffer.Available <= 0)
                        {
                            inputBuffer.Fill();
                            if (inputBuffer.Available <= 0)
                            {
                                throw new Sys.ApplicationException("Unexpected EOF");
                            }
                        }
                        inputBuffer.SetInflaterInput(inf);
                    }

                    public override bool CanRead
                    {
                        get
                        {
                            return baseInputStream.CanRead;
                        }
                    }

                    public override bool CanSeek
                    {
                        get
                        {
                            return false;
                        }
                    }

                    public override bool CanWrite
                    {
                        get
                        {
                            return false;
                        }
                    }

                    public override long Length
                    {
                        get
                        {
                            return inputBuffer.RawLength;
                        }
                    }

                    public override long Position
                    {
                        get
                        {
                            return baseInputStream.Position;
                        }
                        set
                        {
                            throw new Sys.NotSupportedException("InflaterInputStream Position not supported");
                        }
                    }

                    public override void Flush()
                    {
                        baseInputStream.Flush();
                    }

                    public override long Seek(long offset, Sys.IO.SeekOrigin origin)
                    {
                        throw new Sys.NotSupportedException("Seek not supported");
                    }

                    public override void SetLength(long value)
                    {
                        throw new Sys.NotSupportedException("InflaterInputStream SetLength not supported");
                    }

                    public override void Write(byte[] buffer, int offset, int count)
                    {
                        throw new Sys.NotSupportedException("InflaterInputStream Write not supported");
                    }

                    public override void WriteByte(byte value)
                    {
                        throw new Sys.NotSupportedException("InflaterInputStream WriteByte not supported");
                    }

                    public override Sys.IAsyncResult BeginWrite(byte[] buffer, int offset, int count, Sys.AsyncCallback callback, object state)
                    {
                        throw new Sys.NotSupportedException("InflaterInputStream BeginWrite not supported");
                    }

                    public override void Close()
                    {
                        if (!isClosed)
                        {
                            isClosed = true;
                            if (isStreamOwner)
                            {
                                baseInputStream.Close();
                            }
                        }
                    }

                    public override int Read(byte[] buffer, int offset, int count)
                    {
                        if (inf.IsNeedingDictionary) { throw new Sys.ApplicationException("Need a dictionary"); }
                        int remainingBytes = count;
                        while (true)
                        {
                            int bytesRead = inf.Inflate(buffer, offset, remainingBytes);
                            offset += bytesRead;
                            remainingBytes -= bytesRead;
                            if (remainingBytes == 0 || inf.IsFinished) { break; } else if (inf.IsNeedingInput) { Fill(); } else if (bytesRead == 0) { throw new Sys.ApplicationException("Dont know what to do"); }
                        }
                        return count - remainingBytes;
                    }

                    protected Inflater inf;
                    protected InflaterInputBuffer inputBuffer;
                    private Sys.IO.Stream baseInputStream;
                    protected long csize;
                    bool isClosed;
                    bool isStreamOwner = true;
                }

                public class OutputWindow
                {
                    const int WindowSize = 1 << 15;
                    const int WindowMask = WindowSize - 1;
                    byte[] window = new byte[WindowSize];
                    int windowEnd;
                    int windowFilled;

                    /// <summary>
                    /// Write a byte to this output window
                    /// </summary>
                    /// <param name="value">value to write</param>
                    /// <exception cref="InvalidOperationException">
                    /// if window is full
                    /// </exception>
                    public void Write(int value)
                    {
                        if (windowFilled++ == WindowSize)
                        {
                            throw new Sys.InvalidOperationException("Window full");
                        }
                        window[windowEnd++] = (byte)value;
                        windowEnd &= WindowMask;
                    }

                    private void SlowRepeat(int repStart, int length, int distance)
                    {
                        while (length-- > 0)
                        {
                            window[windowEnd++] = window[repStart++];
                            windowEnd &= WindowMask;
                            repStart &= WindowMask;
                        }
                    }

                    /// <summary>
                    /// Append a byte pattern already in the window itself
                    /// </summary>
                    /// <param name="length">length of pattern to copy</param>
                    /// <param name="distance">distance from end of window pattern occurs</param>
                    /// <exception cref="InvalidOperationException">
                    /// If the repeated data overflows the window
                    /// </exception>
                    public void Repeat(int length, int distance)
                    {
                        if ((windowFilled += length) > WindowSize)
                        {
                            throw new Sys.InvalidOperationException("Window full");
                        }

                        int repStart = (windowEnd - distance) & WindowMask;
                        int border = WindowSize - length;
                        if ((repStart <= border) && (windowEnd < border))
                        {
                            if (length <= distance)
                            {
                                Sys.Array.Copy(window, repStart, window, windowEnd, length);
                                windowEnd += length;
                            }
                            else
                            {
                                // We have to copy manually, since the repeat pattern overlaps.
                                while (length-- > 0)
                                {
                                    window[windowEnd++] = window[repStart++];
                                }
                            }
                        }
                        else
                        {
                            SlowRepeat(repStart, length, distance);
                        }
                    }

                    /// <summary>
                    /// Copy from input manipulator to internal window
                    /// </summary>
                    /// <param name="input">source of data</param>
                    /// <param name="length">length of data to copy</param>
                    /// <returns>the number of bytes copied</returns>
                    public int CopyStored(StreamManipulator input, int length)
                    {
                        length = SysMath.Min(SysMath.Min(length, WindowSize - windowFilled), input.AvailableBytes);
                        int copied;

                        int tailLen = WindowSize - windowEnd;
                        if (length > tailLen)
                        {
                            copied = input.CopyBytes(window, windowEnd, tailLen);
                            if (copied == tailLen)
                            {
                                copied += input.CopyBytes(window, 0, length - tailLen);
                            }
                        }
                        else
                        {
                            copied = input.CopyBytes(window, windowEnd, length);
                        }

                        windowEnd = (windowEnd + copied) & WindowMask;
                        windowFilled += copied;
                        return copied;
                    }

                    /// <summary>
                    /// Copy dictionary to window
                    /// </summary>
                    /// <param name="dictionary">source dictionary</param>
                    /// <param name="offset">offset of start in source dictionary</param>
                    /// <param name="length">length of dictionary</param>
                    /// <exception cref="InvalidOperationException">
                    /// If window isnt empty
                    /// </exception>
                    public void CopyDict(byte[] dictionary, int offset, int length)
                    {
                        if (dictionary == null)
                        {
                            throw new Sys.ArgumentNullException("dictionary");
                        }

                        if (windowFilled > 0)
                        {
                            throw new Sys.InvalidOperationException();
                        }

                        if (length > WindowSize)
                        {
                            offset += length - WindowSize;
                            length = WindowSize;
                        }
                        Sys.Array.Copy(dictionary, offset, window, 0, length);
                        windowEnd = length & WindowMask;
                    }

                    /// <summary>
                    /// Get remaining unfilled space in window
                    /// </summary>
                    /// <returns>Number of bytes left in window</returns>
                    public int GetFreeSpace()
                    {
                        return WindowSize - windowFilled;
                    }

                    /// <summary>
                    /// Get bytes available for output in window
                    /// </summary>
                    /// <returns>Number of bytes filled</returns>
                    public int GetAvailable()
                    {
                        return windowFilled;
                    }

                    /// <summary>
                    /// Copy contents of window to output
                    /// </summary>
                    /// <param name="output">buffer to copy to</param>
                    /// <param name="offset">offset to start at</param>
                    /// <param name="len">number of bytes to count</param>
                    /// <returns>The number of bytes copied</returns>
                    /// <exception cref="InvalidOperationException">
                    /// If a window underflow occurs
                    /// </exception>
                    public int CopyOutput(byte[] output, int offset, int len)
                    {
                        int copyEnd = windowEnd;
                        if (len > windowFilled)
                        {
                            len = windowFilled;
                        }
                        else
                        {
                            copyEnd = (windowEnd - windowFilled + len) & WindowMask;
                        }

                        int copied = len;
                        int tailLen = len - copyEnd;

                        if (tailLen > 0)
                        {
                            Sys.Array.Copy(window, WindowSize - tailLen, output, offset, tailLen);
                            offset += tailLen;
                            len = copyEnd;
                        }
                        Sys.Array.Copy(window, copyEnd - len, output, offset, len);
                        windowFilled -= copied;
                        if (windowFilled < 0)
                        {
                            throw new Sys.InvalidOperationException();
                        }
                        return copied;
                    }

                    /// <summary>
                    /// Reset by clearing window so <see cref="GetAvailable">GetAvailable</see> returns 0
                    /// </summary>
                    public void Reset()
                    {
                        windowFilled = windowEnd = 0;
                    }
                }

                public class StreamManipulator
                {
                    /// <summary>
                    /// Constructs a default StreamManipulator with all buffers empty
                    /// </summary>
                    public StreamManipulator()
                    {
                    }

                    /// <summary>
                    /// Get the next sequence of bits but don't increase input pointer.  bitCount must be
                    /// less or equal 16 and if this call succeeds, you must drop
                    /// at least n - 8 bits in the next call.
                    /// </summary>
                    /// <param name="bitCount">The number of bits to peek.</param>
                    /// <returns>
                    /// the value of the bits, or -1 if not enough bits available.  */
                    /// </returns>
                    public int PeekBits(int bitCount)
                    {
                        if (bitsInBuffer_ < bitCount)
                        {
                            if (windowStart_ == windowEnd_)
                            {
                                return -1; // ok
                            }
                            buffer_ |= (uint)((window_[windowStart_++] & 0xff |
                                             (window_[windowStart_++] & 0xff) << 8) << bitsInBuffer_);
                            bitsInBuffer_ += 16;
                        }
                        return (int)(buffer_ & ((1 << bitCount) - 1));
                    }

                    /// <summary>
                    /// Drops the next n bits from the input.  You should have called PeekBits
                    /// with a bigger or equal n before, to make sure that enough bits are in
                    /// the bit buffer.
                    /// </summary>
                    /// <param name="bitCount">The number of bits to drop.</param>
                    public void DropBits(int bitCount)
                    {
                        buffer_ >>= bitCount;
                        bitsInBuffer_ -= bitCount;
                    }

                    /// <summary>
                    /// Gets the next n bits and increases input pointer.  This is equivalent
                    /// to <see cref="PeekBits"/> followed by <see cref="DropBits"/>, except for correct error handling.
                    /// </summary>
                    /// <param name="bitCount">The number of bits to retrieve.</param>
                    /// <returns>
                    /// the value of the bits, or -1 if not enough bits available.
                    /// </returns>
                    public int GetBits(int bitCount)
                    {
                        int bits = PeekBits(bitCount);
                        if (bits >= 0)
                        {
                            DropBits(bitCount);
                        }
                        return bits;
                    }

                    /// <summary>
                    /// Gets the number of bits available in the bit buffer.  This must be
                    /// only called when a previous PeekBits() returned -1.
                    /// </summary>
                    /// <returns>
                    /// the number of bits available.
                    /// </returns>
                    public int AvailableBits
                    {
                        get
                        {
                            return bitsInBuffer_;
                        }
                    }

                    /// <summary>
                    /// Gets the number of bytes available.
                    /// </summary>
                    /// <returns>
                    /// The number of bytes available.
                    /// </returns>
                    public int AvailableBytes
                    {
                        get
                        {
                            return windowEnd_ - windowStart_ + (bitsInBuffer_ >> 3);
                        }
                    }

                    /// <summary>
                    /// Skips to the next byte boundary.
                    /// </summary>
                    public void SkipToByteBoundary()
                    {
                        buffer_ >>= (bitsInBuffer_ & 7);
                        bitsInBuffer_ &= ~7;
                    }

                    /// <summary>
                    /// Returns true when SetInput can be called
                    /// </summary>
                    public bool IsNeedingInput
                    {
                        get
                        {
                            return windowStart_ == windowEnd_;
                        }
                    }

                    /// <summary>
                    /// Copies bytes from input buffer to output buffer starting
                    /// at output[offset].  You have to make sure, that the buffer is
                    /// byte aligned.  If not enough bytes are available, copies fewer
                    /// bytes.
                    /// </summary>
                    /// <param name="output">
                    /// The buffer to copy bytes to.
                    /// </param>
                    /// <param name="offset">
                    /// The offset in the buffer at which copying starts
                    /// </param>
                    /// <param name="length">
                    /// The length to copy, 0 is allowed.
                    /// </param>
                    /// <returns>
                    /// The number of bytes copied, 0 if no bytes were available.
                    /// </returns>
                    /// <exception cref="ArgumentOutOfRangeException">
                    /// Length is less than zero
                    /// </exception>
                    /// <exception cref="InvalidOperationException">
                    /// Bit buffer isnt byte aligned
                    /// </exception>
                    public int CopyBytes(byte[] output, int offset, int length)
                    {
                        if (length < 0)
                        {
                            throw new Sys.ArgumentOutOfRangeException("length");
                        }

                        if ((bitsInBuffer_ & 7) != 0)
                        {
                            // bits_in_buffer may only be 0 or a multiple of 8
                            throw new Sys.InvalidOperationException("Bit buffer is not byte aligned!");
                        }

                        int count = 0;
                        while ((bitsInBuffer_ > 0) && (length > 0))
                        {
                            output[offset++] = (byte)buffer_;
                            buffer_ >>= 8;
                            bitsInBuffer_ -= 8;
                            length--;
                            count++;
                        }

                        if (length == 0)
                        {
                            return count;
                        }

                        int avail = windowEnd_ - windowStart_;
                        if (length > avail)
                        {
                            length = avail;
                        }
                        Sys.Array.Copy(window_, windowStart_, output, offset, length);
                        windowStart_ += length;

                        if (((windowStart_ - windowEnd_) & 1) != 0)
                        {
                            // We always want an even number of bytes in input, see peekBits
                            buffer_ = (uint)(window_[windowStart_++] & 0xff);
                            bitsInBuffer_ = 8;
                        }
                        return count + length;
                    }

                    /// <summary>
                    /// Resets state and empties internal buffers
                    /// </summary>
                    public void Reset()
                    {
                        buffer_ = 0;
                        windowStart_ = windowEnd_ = bitsInBuffer_ = 0;
                    }

                    /// <summary>
                    /// Add more input for consumption.
                    /// Only call when IsNeedingInput returns true
                    /// </summary>
                    /// <param name="buffer">data to be input</param>
                    /// <param name="offset">offset of first byte of input</param>
                    /// <param name="count">number of bytes of input to add.</param>
                    public void SetInput(byte[] buffer, int offset, int count)
                    {
                        if (buffer == null)
                        {
                            throw new Sys.ArgumentNullException("buffer");
                        }

                        if (offset < 0)
                        {
                            throw new Sys.ArgumentOutOfRangeException("offset", "Cannot be negative");
                        }

                        if (count < 0)
                        {
                            throw new Sys.ArgumentOutOfRangeException("count", "Cannot be negative");
                        }

                        if (windowStart_ < windowEnd_)
                        {
                            throw new Sys.InvalidOperationException("Old input was not completely processed");
                        }

                        int end = offset + count;

                        // We want to throw an ArrayIndexOutOfBoundsException early.
                        // Note the check also handles integer wrap around.
                        if ((offset > end) || (end > buffer.Length))
                        {
                            throw new Sys.ArgumentOutOfRangeException("count");
                        }

                        if ((count & 1) != 0)
                        {
                            // We always want an even number of bytes in input, see PeekBits
                            buffer_ |= (uint)((buffer[offset++] & 0xff) << bitsInBuffer_);
                            bitsInBuffer_ += 8;
                        }

                        window_ = buffer;
                        windowStart_ = offset;
                        windowEnd_ = end;
                    }

                    private byte[] window_;
                    private int windowStart_;
                    private int windowEnd_;
                    private uint buffer_;
                    private int bitsInBuffer_;
                }
            }
        }
    }
}
#endif