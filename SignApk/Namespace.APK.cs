//https://github.com/ibukisaar/SignApk (2021-11-08 - no license required)
using Sys = global::System;
using SysClG = global::System.Collections.Generic;
using SysTxt = global::System.Text;
using SysCry = global::System.Security.Cryptography;
using LibZIP = global::Libs.ZIP;
using LibAPK = global::Libs.SignApk;
namespace Libs.SignApk
{
    internal struct StaticDataSource : LibZIP.IStaticDataSource
    {
        private Sys.IO.Stream stream;
        Sys.IO.Stream LibZIP.IStaticDataSource.GetSource() { return this.stream; }
        public StaticDataSource(Sys.IO.Stream stream) { this.stream = stream; }
        public StaticDataSource(byte[] data) { this.stream = new Sys.IO.MemoryStream(data); }
    }

    public class Manifest
    {
        private const int DefaultCapacity = 65537;
        private static readonly SysTxt.RegularExpressions.Regex keyRegex = new SysTxt.RegularExpressions.Regex(@"^Name:\s*(.*)$", SysTxt.RegularExpressions.RegexOptions.Compiled);
        private static readonly SysTxt.RegularExpressions.Regex valueRegex = new SysTxt.RegularExpressions.Regex(@"^SHA1-Digest:\s*(.*)$", SysTxt.RegularExpressions.RegexOptions.Compiled);
        private string h = "Manifest-Version: 1.0\r\nCreated-By: 1.0 (Saar Tool)";
        private SysClG.Dictionary<string, string> m;
        public string Header { get { return this.h; } set { this.h = value; } }
        public SysClG.Dictionary<string, string> Map { get { return this.m; } }

        public void WriteTo(Sys.IO.Stream output)
        {
            Sys.IO.StreamWriter writer = new Sys.IO.StreamWriter(output);
            writer.WriteLine(Header);
            foreach (SysClG.KeyValuePair<string, string> kv in this.m)
            {
                writer.Write("Name: ");
                writer.WriteLine(kv.Key);
                writer.Write("SHA1-Digest: ");
                writer.WriteLine(kv.Value);
                writer.WriteLine();
            }
            writer.Flush();
        }

        public Manifest(SysClG.IDictionary<string, string> map) { this.m = new SysClG.Dictionary<string, string>(map); }
        public Manifest() { this.m = new SysClG.Dictionary<string, string>(LibAPK.Manifest.DefaultCapacity); }

        public Manifest(Sys.IO.Stream input) : this()
        {
            Sys.IO.StreamReader reader = new Sys.IO.StreamReader(input);
            string line = null;
            SysTxt.RegularExpressions.Match match = null;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line)) { continue; }
                else
                {
                    match = LibAPK.Manifest.keyRegex.Match(line);
                    if (!match.Success) continue;
                    string key = match.Groups[1].Value;
                    line = reader.ReadLine();
                    match = LibAPK.Manifest.valueRegex.Match(line);
                    if (!match.Success) continue;
                    string value = match.Groups[1].Value;
                    this.m.Add(key, value);
                }
            }
        }
    }

    public static class Common
    {
        private const int DefaultBufferSize = 80 * 1024;
        public static string StandardDirectory(string dir) { return ((dir.EndsWith("\\") || dir.EndsWith("/")) ? dir.Substring(0, dir.Length - 1) : dir); }

        public static bool TryCreateDirectory(string file)
        {
            string dir = Sys.IO.Path.GetDirectoryName(file);
            if (!Sys.IO.Directory.Exists(dir))
            {
                Sys.IO.Directory.CreateDirectory(dir);
                return true;
            } else { return false; }
        }

        public static void CopyTo(Sys.IO.Stream input, Sys.IO.Stream output) //pre .Net 4 this function does not exists, so made as in: https://referencesource.microsoft.com/#mscorlib/system/io/stream.cs,98ac7cf3acb04bb1
        {
            if (output != null || !output.CanWrite || input == null || !input.CanRead) { throw new Sys.IO.IOException(); }
            else
            {
                byte[] buffer = new byte[LibAPK.Common.DefaultBufferSize];
                int read = 0;
                while ((read = input.Read(buffer, 0, buffer.Length)) != 0) { output.Write(buffer, 0, read); }
            }
        }

        public static void TryCopy(string source, string output) { if (LibAPK.Common.TryCreateDirectory(output) || !Sys.IO.File.Exists(source)) { using (Sys.IO.Stream outputF = Sys.IO.File.Create(output, 4096, Sys.IO.FileOptions.SequentialScan)) { using (Sys.IO.FileStream sourceF = new Sys.IO.FileStream(source, Sys.IO.FileMode.Open, Sys.IO.FileAccess.Read, Sys.IO.FileShare.Read, 4096, Sys.IO.FileOptions.SequentialScan)) { LibAPK.Common.CopyTo(sourceF, outputF); } } } }
        public static Sys.IO.Stream CreateFile(string file) { if (LibAPK.Common.TryCreateDirectory(file)) { return new Sys.IO.FileStream(file, Sys.IO.FileMode.OpenOrCreate, Sys.IO.FileAccess.Write, Sys.IO.FileShare.None, 4096, Sys.IO.FileOptions.SequentialScan); } else { return null; } }
    }

    public static class ApkTool
    {
        private static readonly SysTxt.RegularExpressions.Regex sfRegex = new SysTxt.RegularExpressions.Regex(@"^META-INF/(.*)\.SF$", (SysTxt.RegularExpressions.RegexOptions.Compiled | SysTxt.RegularExpressions.RegexOptions.Singleline));

        private static string ComputeSha1Base64ForSF(SysCry.HashAlgorithm sha1, SysTxt.StringBuilder buffer, string key, string value)
        {
            buffer.Capacity = 0;
            buffer.Append("Name: ").Append(key).Append("\r\n").Append("SHA1-Digest: ").Append(value).Append("\r\n").Append("\r\n");
            byte[] hashValue = sha1.ComputeHash(SysTxt.Encoding.UTF8.GetBytes(buffer.ToString()));
            return Sys.Convert.ToBase64String(hashValue);
        }

        private static LibAPK.Manifest GetManifestForAll(LibZIP.ZipFile zip, string inputDir, SysClG.IEnumerable<string> files, out byte[] manifestData)
        {
            SysClG.Dictionary<string, string> map = new SysClG.Dictionary<string, string>();
            SysClG.List<string> filesSet = new SysClG.List<string>();
            LibZIP.ZipEntry entry = null;
            foreach (string file in files) { filesSet.Add(file.Replace('\\', '/')); }
            using (LibZIP.ZipInputStream zipInput = new LibZIP.ZipInputStream(Sys.IO.File.OpenRead(zip.Name)))
            using (SysCry.HashAlgorithm hash = SysCry.HashAlgorithm.Create("SHA1"))
            while ((entry = zipInput.GetNextEntry()) != null)
            {
                byte[] hashValue;
                if (filesSet.Contains(entry.Name)) { using (Sys.IO.Stream temp = Sys.IO.File.OpenRead(inputDir + '\\' + entry.Name)) { hashValue = hash.ComputeHash(temp); } } else { hashValue = hash.ComputeHash(zipInput); }
                map.Add(entry.Name, Sys.Convert.ToBase64String(hashValue));
            }
            Sys.IO.MemoryStream manifestStream = new Sys.IO.MemoryStream(10 * 1024);
            LibAPK.Manifest manifest = new LibAPK.Manifest(map);
            manifest.WriteTo(manifestStream);
            manifestData = manifestStream.ToArray();
            return manifest;
        }

        public static string FindSFName(LibZIP.ZipFile zip)
        {
            SysTxt.RegularExpressions.Match match = null;
            foreach (LibZIP.ZipEntry entry in zip)
            {
                match = sfRegex.Match(entry.Name);
                if (match.Success) { return match.Groups[1].Value; }
            }
            return null;
        }

        public static void ClearMETAINF(LibZIP.ZipFile zip, string sfName)
        {
            zip.Delete("META-INF/" + sfName + ".SF");
            zip.Delete("META-INF/" + sfName + ".RSA");
            zip.Delete("META-INF/" + sfName + ".DSA");
        }

        public static LibAPK.Manifest GetManifest(LibZIP.ZipFile zip, string inputDir, SysClG.IEnumerable<string> files, out byte[] manifestData)
        {
            LibZIP.ZipEntry manifestEntry = zip.GetEntry("META-INF/MANIFEST.MF");
            if (manifestEntry == null) { return LibAPK.ApkTool.GetManifestForAll(zip, inputDir, files, out manifestData); }
            else
            {
                Sys.IO.Stream oldManifestStream = zip.GetInputStream(manifestEntry);
                LibAPK.Manifest oldManifest = new LibAPK.Manifest(oldManifestStream);
                using (SysCry.HashAlgorithm hash = SysCry.HashAlgorithm.Create("SHA1")) { foreach (string file in files) { using (Sys.IO.FileStream temp = Sys.IO.File.OpenRead(inputDir + '\\' + file)) { oldManifest.Map[file.Replace('\\', '/')] = Sys.Convert.ToBase64String(hash.ComputeHash(temp)); } } }
                Sys.IO.MemoryStream newManifestStream = new Sys.IO.MemoryStream((int)manifestEntry.Size);
                oldManifest.WriteTo(newManifestStream);
                manifestData = newManifestStream.ToArray();
                oldManifestStream.Close();
                return oldManifest;
            }
        }

        public static byte[] GetSFData(LibZIP.ZipFile zip, string inputDir, SysClG.IEnumerable<string> files, LibAPK.Manifest mf, byte[] mfData, string sfName)
        {
            LibZIP.ZipEntry sfEntry = zip.GetEntry("META-INF/" + sfName + ".SF");
            SysCry.HashAlgorithm sha1 = SysCry.HashAlgorithm.Create("SHA1");
            SysTxt.StringBuilder buffer = new SysTxt.StringBuilder(1024);
            LibAPK.Manifest sf = null;
            if (sfEntry == null)
            {
                SysClG.Dictionary<string, string> sfMap = new SysClG.Dictionary<string, string>(mf.Map.Count);
                foreach (SysClG.KeyValuePair<string, string> kv in mf.Map) { sfMap[kv.Key] = LibAPK.ApkTool.ComputeSha1Base64ForSF(sha1, buffer, kv.Key, kv.Value); }
                sf = new LibAPK.Manifest(sfMap);
            }
            else
            {
                Sys.IO.Stream sfInputStream = zip.GetInputStream(sfEntry);
                sf = new LibAPK.Manifest(sfInputStream);
                foreach (string file in files) { sf.Map[file] = LibAPK.ApkTool.ComputeSha1Base64ForSF(sha1, buffer, file, mf.Map[file]); }
                sfInputStream.Close();
            }
            sf.Header = "Signature-Version: 1.0\r\nSHA1-Digest-Manifest: " + Sys.Convert.ToBase64String(sha1.ComputeHash(mfData)) + "\r\nCreated-By: 1.0 (Saar Tool)";
            Sys.IO.MemoryStream sfStream = new Sys.IO.MemoryStream(mfData.Length + 500);
            sf.WriteTo(sfStream);
            sha1.Clear();
            return sfStream.ToArray();
        }

        public static byte[] GetRSAData(LibZIP.ZipFile zip, byte[] sfData, string rsaName, string keyFile)
        {
            if (!Sys.IO.File.Exists(keyFile)) { throw new SysCry.CryptographicUnexpectedOperationException(); }
            else
            {
                SysCry.X509Certificates.X509Certificate2 certificate = new SysCry.X509Certificates.X509Certificate2(keyFile);
                SysCry.Pkcs.SignedCms signedCms = new SysCry.Pkcs.SignedCms(new SysCry.Pkcs.ContentInfo(sfData), true);
                signedCms.ComputeSignature(new SysCry.Pkcs.CmsSigner(SysCry.Pkcs.SubjectIdentifierType.IssuerAndSerialNumber, certificate));
                return signedCms.Encode();
            }
        }

        private static SysClG.IEnumerable<string> GetUpdateFilesForUnzip(LibZIP.ZipFile zip, string inputDir, SysClG.IEnumerable<string> files, out int Count)
        {
            SysClG.List<string> sorted = new SysClG.List<string>();
            foreach (string file in files) { sorted.Add(file); }
            sorted.Sort();
            SysClG.List<string> result = new SysClG.List<string>(sorted.Count);
            LibZIP.ZipEntry entry = null;
            foreach (string file in sorted)
            {
                entry = zip.GetEntry(file);
                string path = inputDir + '\\' + file;
                if (!Sys.IO.File.Exists(path) || Sys.IO.File.GetLastWriteTime(path) != entry.DateTime) { result.Add(file); }
            }
            Count = result.Count;
            return result;
        }

        private static SysClG.IEnumerable<string> GetUpdateFilesForZip(LibZIP.ZipFile zip, string inputDir, SysClG.IEnumerable<string> files, out int Count)
        {
            SysClG.List<string> result = new SysClG.List<string>();
            LibZIP.ZipEntry entry = null;
            foreach (string file in files)
            {
                entry = zip.GetEntry(file);
                if (entry == null || Sys.IO.File.GetLastWriteTime(inputDir + '\\' + file) != entry.DateTime) { result.Add(file); }
            }
            Count = result.Count;
            return result;
        }

        private static void ZipCopyToZip(string sourceApkFile, string inputDir, SysClG.IEnumerable<string> files, string outputFile)
        {
            SysClG.List<string> filesSet = new SysClG.List<string>(files);
            using (LibZIP.ZipInputStream zipInput = new LibZIP.ZipInputStream(Sys.IO.File.OpenRead(sourceApkFile)))
            using (LibZIP.ZipOutputStream zipOutput = new LibZIP.ZipOutputStream(Sys.IO.File.Create(outputFile, 4096, Sys.IO.FileOptions.SequentialScan)))
            {
                zipOutput.UseZip64 = LibZIP.UseZip64.Off;
                LibZIP.ZipEntryFactory factory = new LibZIP.ZipEntryFactory();
                LibZIP.ZipEntry entry = null;
                LibZIP.ZipEntry entry2 = null;
                while ((entry = zipInput.GetNextEntry()) != null)
                {
                    entry2 = factory.MakeFileEntry(entry.Name);
                    entry2.DosTime = entry.DosTime;
                    zipOutput.PutNextEntry(entry2);
                    if (filesSet.Remove(entry.Name)) { using (Sys.IO.FileStream temp = Sys.IO.File.OpenRead(inputDir + '\\' + entry.Name)) { LibAPK.Common.CopyTo(temp, zipOutput); } } else { LibAPK.Common.CopyTo(zipInput, zipOutput); }
                }
                foreach (string file in filesSet)
                {
                    entry = factory.MakeFileEntry(file);
                    entry.DateTime = (new Sys.IO.FileInfo(inputDir + '\\' + entry.Name)).LastWriteTime;
                    zipOutput.PutNextEntry(entry);
                    using (Sys.IO.FileStream temp = Sys.IO.File.OpenRead(inputDir + '\\' + entry.Name)) { LibAPK.Common.CopyTo(temp, zipOutput); }
                }
            }
        }

        private static void Sign(string apkFile, string inputDir, SysClG.IEnumerable<string> files, string keyFile)
        {
            inputDir = LibAPK.Common.StandardDirectory(inputDir);
            using (LibZIP.ZipFile zip = new LibZIP.ZipFile(new Sys.IO.FileStream(apkFile, Sys.IO.FileMode.Open, Sys.IO.FileAccess.ReadWrite, Sys.IO.FileShare.ReadWrite)))
            {
                string sfName = LibAPK.ApkTool.FindSFName(zip) ?? "SAAR";
                byte[] sfData = null;
                byte[] rsaData = null;
                byte[] manifestData = null;
                LibAPK.Manifest manifest = LibAPK.ApkTool.GetManifest(zip, inputDir, files, out manifestData);
                sfData = LibAPK.ApkTool.GetSFData(zip, inputDir, files, manifest, manifestData, sfName);
                rsaData = LibAPK.ApkTool.GetRSAData(zip, sfData, sfName, keyFile);
                zip.BeginUpdate();
                zip.Add(new LibAPK.StaticDataSource(manifestData), "META-INF/MANIFEST.MF");
                zip.Add(new LibAPK.StaticDataSource(sfData), "META-INF/" + sfName + ".SF");
                zip.Add(new LibAPK.StaticDataSource(rsaData), "META-INF/" + sfName + ".RSA");
                zip.CommitUpdate();
            }
        }

        public static int Unzip(string apkFile, SysClG.IEnumerable<string> files, string outputDir)
        {
            outputDir = LibAPK.Common.StandardDirectory(outputDir);
            using (LibZIP.ZipFile zip = new LibZIP.ZipFile(apkFile))
            {
                int count = 0;
                files = LibAPK.ApkTool.GetUpdateFilesForUnzip(zip, outputDir, files, out count);
                LibZIP.ZipEntry entry = null;
                Sys.IO.Stream outputStream = null;
                Sys.IO.Stream inputStream = null;
                foreach (string file in files)
                {
                    entry = zip.GetEntry(file);
                    outputStream = LibAPK.Common.CreateFile(outputDir + '\\' + entry.Name);
                    inputStream = zip.GetInputStream(entry);
                    LibAPK.Common.CopyTo(inputStream, outputStream);
                    inputStream.Close();
                    outputStream.Close();
                    Sys.IO.File.SetLastWriteTime(outputDir + '\\' + entry.Name, entry.DateTime);
                }
                return count;
            }
        }

        public static int ZipAndSign(string sourceApkFile, string inputDir, SysClG.IEnumerable<string> files, string outputFile, string keyFile)
        {
            inputDir = LibAPK.Common.StandardDirectory(inputDir);
            bool copy = false;
            int count = 0;
            if (Sys.IO.File.Exists(outputFile)) { using (LibZIP.ZipFile zip = new LibZIP.ZipFile(Sys.IO.File.OpenRead(outputFile))) { files = LibAPK.ApkTool.GetUpdateFilesForZip(zip, inputDir, files, out count); } }
            using (LibZIP.ZipFile zip = new LibZIP.ZipFile(Sys.IO.File.OpenRead(sourceApkFile)))
            {
                if (count < zip.Count / 2)
                {
                    long filesLengthSum = 0L;
                    foreach (string file in files) { filesLengthSum += (new Sys.IO.FileInfo(inputDir + '\\' + file)).Length; }
                    copy = (filesLengthSum < (new Sys.IO.FileInfo(sourceApkFile)).Length);
                } else { copy = false; }
            }
            if (copy)
            {
                if (count == 0) { return 0; }
                else
                {
                    string sfName = null;
                    byte[] manifestData = null;
                    byte[] sfData = null;
                    byte[] rsaData = null;
                    LibAPK.Common.TryCopy(sourceApkFile, outputFile);
                    using (LibZIP.ZipFile zip = new LibZIP.ZipFile(Sys.IO.File.Open(outputFile, Sys.IO.FileMode.Open, Sys.IO.FileAccess.Read, Sys.IO.FileShare.Read)))
                    {
                        sfName = LibAPK.ApkTool.FindSFName(zip) ?? "SAAR";
                        LibAPK.Manifest manifest = LibAPK.ApkTool.GetManifest(zip, inputDir, files, out manifestData);
                        sfData = LibAPK.ApkTool.GetSFData(zip, inputDir, files, manifest, manifestData, sfName);
                        rsaData = LibAPK.ApkTool.GetRSAData(zip, sfData, sfName, keyFile);
                    }
                    using (LibZIP.ZipFile zip = new LibZIP.ZipFile(Sys.IO.File.Open(outputFile, Sys.IO.FileMode.Open, Sys.IO.FileAccess.ReadWrite, Sys.IO.FileShare.None)))
                    {
                        zip.BeginUpdate();
                        LibZIP.ZipEntry entry = null;
                        foreach (string file in files)
                        {
                            entry = zip.EntryFactory.MakeFileEntry(file);
                            entry.DateTime = (new Sys.IO.FileInfo(inputDir + '\\' + file)).LastWriteTime;
                            zip.Add(inputDir + '\\' + file, entry.Name);
                        }
                        zip.Add(new LibAPK.StaticDataSource(manifestData), "META-INF/MANIFEST.MF");
                        zip.Add(new LibAPK.StaticDataSource(sfData), "META-INF/" + sfName + ".SF");
                        zip.Add(new LibAPK.StaticDataSource(rsaData), "META-INF/" + sfName + ".RSA");
                        zip.CommitUpdate();
                    }
                }
            }
            else
            {
                LibAPK.ApkTool.ZipCopyToZip(sourceApkFile, inputDir, files, outputFile);
                LibAPK.ApkTool.Sign(outputFile, inputDir, files, keyFile);
            }
            return count;
        }
    }
}
#if DEBUG
internal static class Program
{
    internal static int Main(string[] args) //https://github.com/ibukisaar/SignApk/blob/master/SignApk.Sample/MainWindow.xaml.cs
    {
	//todo
        return 0;
    }
}
#endif
