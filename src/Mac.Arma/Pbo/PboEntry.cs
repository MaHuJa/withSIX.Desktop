// <copyright company="SIX Networks GmbH" file="PboEntry.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;

namespace Mac.Arma.Files
{
    /// <summary>
    ///     Defines whether PboEntries will be extracted from an input stream immediately
    /// </summary>
    /// <remarks>
    ///     By default, PboEntries are loaded from an input stream at the time that
    ///     it is constructed.  This is usually the right thing to do since it means the input
    ///     can be closed immediately following the construction.
    ///     However, if a PboFile contains a large number of entries, loading the entire set
    ///     can be time-consuming and is wasteful if the client does not care about the contents
    ///     of all PboEntries (eg if modifying a single entry).  Deferred mode can therefore
    ///     be used to ensure that the PboEntry is only loaded at the time that it is needed.
    ///     This can result in significant speed improvements but means that the client is responsible
    ///     for ensuring that the input stream remains available for the entire lifetime of the PboFile
    /// </remarks>
    public enum PboLoadMode
    {
        /// <summary>
        ///     Load the PboEntry immediately
        /// </summary>
        Immediate,

        /// <summary>
        ///     Load the PboEntry only when (or if) needed
        /// </summary>
        Deferred
    }


    /// <summary>
    ///     Represents a single packed file with a PboFile
    /// </summary>
    /// <remarks>
    ///     Refer to the documentation for PboFile for instructions on how to work with .pbo files.
    ///     It is not usually necessary to work directly with PboEntry objects but there are some cases
    ///     where this is useful.  Each PboEntry represents a single file within a PboFile and as such
    ///     PboEntry's are used when you want to selectively access, modify or extract individual files within
    ///     a pbo.
    /// </remarks>
    public class PboEntry
    {
        //a link used to obtain the pbo contents if lzyread is being used
        byte[] _fileContents;
        Stream _input;
        long _offset;

        internal PboEntry() {
            Path = string.Empty;
        }

        /// <summary>
        ///     Constructs a new PboEntry from a stream.
        /// </summary>
        /// <param name="path">The path of the file within the pbo</param>
        /// <param name="data">The input stream</param>
        public PboEntry(string path, Stream data)
            : this() {
            Init(path, data);
        }

        /// <summary>
        ///     Constructs a new PboEntry from a file.
        /// </summary>
        /// <param name="path">The path of the file within the pbo</param>
        /// <param name="fileName">The path of the file that will be used as input</param>
        /// <example>
        ///     <code>
        /// //Create a PboEntry from c:\music.ogg and ensure that it is placed
        /// //within the mission at a relative path of sounds\x.ogg
        /// PboEntry e = new PboEntry(@"sounds\x.ogg",@"C:\music.ogg");
        /// </code>
        /// </example>
        public PboEntry(string path, string fileName)
            : this() {
            using (Stream s = File.OpenRead(fileName))
                Init(path, s);
        }

        /// <summary>The relative path of the file from the root of the pbo 'virtual' filesystem</summary>
        public string Path { get; set; }

        //Information found in the pbo header
        UInt32 PackingMethod { get; set; }
        UInt32 OriginalSize { get; set; }
        UInt32 Reserved { get; set; }
        UInt32 Timestamp { get; set; }

        /// <summary>
        ///     The size (in bytes) of the file
        /// </summary>
        public int DataSize { get; private set; }

        byte[] FileContents {
            get {
                if (_fileContents == null)
                    ReadBody(_input, _offset, PboLoadMode.Immediate);
                return _fileContents;
            }
            set { _fileContents = value; }
        }

        /*
         * Internal function used to read the Header information from a stream
         */

        internal void ReadHeader(Stream sr) {
            Path = BinaryFile.ReadString(sr);
            PackingMethod = BinaryFile.ReadUInt32(sr);
            OriginalSize = BinaryFile.ReadUInt32(sr);
            Reserved = BinaryFile.ReadUInt32(sr);
            Timestamp = BinaryFile.ReadUInt32(sr);
            DataSize = (int) BinaryFile.ReadUInt32(sr);
        }

        void Init(string path, Stream data) {
            Path = path;
            DataSize = (int) data.Length;
            ReadBody(data, -1, PboLoadMode.Immediate);
        }

        /*
         * Internal function used to read the actual file contents of a PboEntry from a stream
         */

        internal void ReadBody(Stream sr, long offset, PboLoadMode mode) {
            if (offset < 0)
                offset = sr.Position;
            if (mode == PboLoadMode.Immediate) {
                FileContents = new byte[DataSize];
                sr.Seek(offset, SeekOrigin.Begin);
                sr.Read(FileContents, 0, DataSize);
            } else {
                //save the position and skip past
                _input = sr;
                _offset = offset;
                sr.Seek(DataSize, SeekOrigin.Current);
            }
        }

        internal void WriteHeader(Stream sr) {
            BinaryFile.WriteString(sr, Path);
            BinaryFile.WriteUInt32(sr, PackingMethod);
            BinaryFile.WriteUInt32(sr, OriginalSize);
            BinaryFile.WriteUInt32(sr, Reserved);
            BinaryFile.WriteUInt32(sr, Timestamp);
            BinaryFile.WriteUInt32(sr, (UInt32) DataSize);
        }

        internal void WriteBody(Stream sr) {
            sr.Write(FileContents, 0, DataSize);
        }

        /// <summary>
        ///     Extracts the contents of the PboEntry to a file in the destination folder
        /// </summary>
        /// <remarks>The file is created with the relative path stored in the PboEntry</remarks>
        /// <example>
        ///     <code>
        /// //extract intro.ogg in mymission.pbo to c:\temp\sounds\intro.ogg
        /// PboFile pbo = PboFile.FromPbo("mymission.pbo");
        /// pbo[@"sounds\intro.ogg"].ExtractTo(@"C:\temp");
        /// </code>
        /// </example>
        /// <param name="destinationFolder">The root folder for the extracted file</param>
        public void ExtractTo(string destinationFolder) {
            var destinationFile = System.IO.Path.Combine(destinationFolder, Path);
            var targetDir = System.IO.Path.GetDirectoryName(destinationFile);
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);
            using (Stream sw = File.Create(destinationFile))
                WriteBody(sw);
        }

        /*!
         * from path 'source'
         * 'folder' is the root from which the relative path of the file is set
         * 
         * Examples
         * @code
         * //create a new PboEntry with relative path "sounds\intro.ogg"
         * 
         * @endcode
         */

        /// <summary>
        ///     Returns the relative path of a file
        /// </summary>
        /// <remarks>
        ///     It is often useful when dealing with pbos to calculate paths relative
        ///     to some arbitrary root.
        ///     This method allows you to easily do that.
        /// </remarks>
        /// <param name="fileName">The absolute path to a file</param>
        /// <param name="root">The root from which the relative path is calculated</param>
        /// <returns>The path of fileName relative to root</returns>
        /// <example>
        ///     <code>
        /// //relativePath will have the value "sounds\intro.ogg"
        /// string mission=@"c:\temp\testmission.utes";
        /// string file = @"c:\temp\testmission.utes\sounds\intro.ogg";
        /// string relativePath = PboEntry.RelativePath(mission,file);
        /// </code>
        /// </example>
        public static string RelativePath(string fileName, string root) {
            //add 1 to allow for trailing slash
            return fileName.Substring(root.Length + 1);
        }

        /// <summary>
        ///     Creates a new PboEntry from a file
        /// </summary>
        /// <remarks>
        ///     This is a convenience function which can be used to simply the process of adding
        ///     files with absolute paths from a particular folder which you want to treat as the
        ///     root of a mission
        /// </remarks>
        /// <param name="fileName">An absolute path to the input file</param>
        /// <param name="root">The root folder of the mission</param>
        /// <returns>A new PboEntry whose Path variable is set to the relative path of fileName vs root</returns>
        /// <example>
        ///     <code>
        /// //create a new PboEntry whose Path is set to "sound\boom.ogg"
        /// string missionFolder = @"C:\temp\mission.utes" ;
        /// string file = @"C:\temp\mission.utes\sound\boom.ogg";
        /// PboEntry e = PboEntry.FromFileRelativeToFolder(file,missionFolder);
        /// 
        /// </code>
        /// </example>
        public static PboEntry FromFileRelativeToFolder(string fileName, string root) {
            return new PboEntry(RelativePath(fileName, root), fileName);
        }

        internal static int PathSorter(PboEntry e1, PboEntry e2) {
            var s1 = e1.Path;
            var s2 = e2.Path;
            s1 = s1.ToUpperInvariant();
            s2 = s2.ToUpperInvariant();
            var c = Math.Min(s1.Length, s2.Length);
            for (var d = 0; d < c; d++) {
                var c1 = s1[d];
                var c2 = s2[d];
                if (c1 != c2) {
                    if (c1 == '_')
                        return 1;
                    if (c2 == '_')
                        return -1;
                    return c1 - c2;
                }
            }
            if (s1.Length < s2.Length)
                return -1;
            if (s1.Length > s2.Length)
                return 1;
            return 0;
        }

        /// <summary>
        ///     Returns the contents of file represented by this PboEntry in the form of a stream
        /// </summary>
        /// <returns>A stream containing the file contents</returns>
        public Stream ToStream() {
            return new MemoryStream(FileContents);
        }
    }
}