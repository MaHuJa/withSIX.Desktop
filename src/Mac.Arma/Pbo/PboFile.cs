// <copyright company="SIX Networks GmbH" file="PboFile.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mac.Arma.Misc;

namespace Mac.Arma.Files
{
    /// <summary>
    ///     A PboFile object represents a BIS .pbo file as a collection of PboEntry objects
    /// </summary>
    /// <remarks>
    ///     The PboFile class is the starting point when working with .pbo files. A .pbo file is really
    ///     just an archive of files, rather like a .zip file, and the PboFile object
    ///     represents a logical view of the .pbo as a collection of PboEntry objects in a virtual
    ///     directory structure.  Each PboEntry represents one of the files within the .pbo.
    ///     A PboFile can be constructed from an existing .pbo file, a stream, or a mission-folder
    ///     that contains files.
    ///     Once you have created a PboFile, you can extract its contents, save it to disk as a .pbo or
    ///     manipulate the list of files it contains.
    ///     You can iterate over a PboFile or access the PboEntries it contains by indexing into it and a Match
    ///     method can be used to select sets of files within it.
    ///     These capabilities make it easy to perform common operations on .pbo files in just a few lines of code.
    /// </remarks>
    /// <example>
    ///     <code>
    /// //Extract the contents of a pbo to a folder called "missions\mymission.utes"...
    /// PboFile pbo = PboFile.Read(@"pbos\mymission.utes.pbo");
    /// pbo.ExtractTo("missions");
    /// </code>
    ///     <code>
    /// //Create a .pbo file from a mission and save it to "pbos\mymission.utes.pbo"; 
    /// PboFile pbo = PboFile.FromFolder(@"missions\mymission.utes");
    /// pbo.Write("pbos");
    /// </code>
    ///     <code>
    /// //List all the files in a pbo
    /// foreach (PboEntry f in pbo)
    ///      System.Console.WriteLine(f.Path);
    /// </code>
    ///     <code>
    /// //Read the size of the mission.sqm file
    /// System.Console.WriteLine(pbo["mission.sqm"].DataSize);
    /// </code>
    ///     <code>
    /// //Remove all the backup files from an existing pbo
    /// PboFile pbo = PboFile.FromPbo("mymission.utes.pbo");
    /// pbo.RemoveRange(pbo.Match("*.bak",MatchType.Path));
    /// pbo.Writeback();
    /// </code>
    /// </example>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class PboFile : IEnumerable<PboEntry>
    {
        readonly List<PboEntry> _entries = new List<PboEntry>();
        bool _createdFromFolder;
        string _fileName;
        string _sourceFolder;
        //allow allowable if constructing from a file

        /// <summary>
        ///     The path to the .pbo file
        /// </summary>
        public string Path {
            get { return System.IO.Path.Combine(_sourceFolder, FileName); }
        }

        /// <summary>
        ///     Returns a PboEntry that represents the file stored with the supplied relative path
        /// </summary>
        /// <remarks>The filePath is case-insensitive</remarks>
        /// <param name="filePath"></param>
        /// <returns>The requested PboEntry or NULL if it cannot be found</returns>
        /// <example>
        ///     <code>
        /// pbo["mission.sqm"]; 
        /// pbo["sounds/intro.ogg"];
        /// </code>
        /// </example>
        public PboEntry this[string filePath] {
            get {
                if (filePath.StartsWith(@"\", StringComparison.Ordinal))
                    filePath = filePath.Substring(1);
                return
                    _entries.FirstOrDefault(
                        e => String.Compare(e.Path, filePath, StringComparison.CurrentCultureIgnoreCase) == 0);
            }
        }

        /// <summary>
        ///     Returns the PboEntry at position 'index' ddd
        /// </summary>
        /// <param name="index">Index into the list of PboEntrys</param>
        /// <returns>The PboEntry at the specified index</returns>
        public PboEntry this[int index] {
            get { return _entries[index]; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///     the number of PboEntries in this PboFile
        /// </summary>
        public int Count {
            get { return _entries.Count; }
        }

        /// <summary>
        ///     The name of the .pbo file represented by this PboFile object
        /// </summary>
        /// <remarks>
        ///     The FileName property returns the filename portion (including '.pbo' suffix) of the
        ///     PboFile Path.
        ///     FileName is automatically set when creating a PboFile from a mission folder or .pbo file
        /// </remarks>
        public string FileName {
            get { return _fileName; }
            set {
                _fileName = value;
                if (!IsPbo(_fileName))
                    _fileName = _fileName + ".pbo";
            }
        }

        /// <summary>
        ///     The same as FileName except that the .pbo suffix is not included
        /// </summary>
        public string MissionName {
            get { return FileName.Substring(0, _fileName.Length - 4); }
            set { FileName = value; }
        }

        /// <summary>
        ///     Returns the collection of PboEntry objects in this PboFile
        /// </summary>
        /// <returns></returns>
        public IEnumerator<PboEntry> GetEnumerator() {
            return ((IEnumerable<PboEntry>) _entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        ///     Checks whether a file appears to be a .pbo
        /// </summary>
        /// <remarks>
        ///     Note that this is only a cursory check - there is no
        ///     guarantee that the file is well-formed
        /// </remarks>
        /// <param name="fileName">File to check</param>
        /// <returns> True if the target file looks like it is a .pbo file</returns>
        public static bool IsPbo(string fileName) {
            return fileName.EndsWith(".pbo", StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(".ifa", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>   Returns a collection of PboEntries whose names match the specified search pattern. </summary>
        /// <remarks>   See documentation on MatchType to understand matching. </remarks>
        /// <param name="pattern">   The pattern to match </param>
        /// <param name="matchType">    Type of the match. </param>
        /// <returns>   A collection of PboEntries that match the pattern </returns>
        /// <example>
        ///     <code>
        ///  //do something will all sqf files in the PboFile
        ///  foreach (PboEntry f in pbo.Match("*.sqf",MatchType.Path) {...};
        ///  </code>
        /// </example>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IEnumerable<PboEntry> Match(string pattern, MatchType matchType) {
            pattern = Matcher.MatchString(pattern, matchType);
            return this.Where(e => Regex.Match(e.Path, pattern, RegexOptions.IgnoreCase).Success);
        }

        /// <summary>
        ///     Constructs a new PboFile from a mission folder
        /// </summary>
        /// <remarks>
        ///     All files in the specified folder are added to the PboFile with the same
        ///     relative directory structure as on disk
        ///     The name of the folder is used as the MissionName and Path is set to the parent
        ///     folder of the mission.
        /// </remarks>
        /// <param name="folder">Path of mission folder</param>
        /// <returns>A new PboFile object</returns>
        public static PboFile FromFolder(string folder) {
            var pbo = new PboFile {
                _createdFromFolder = true,
                _sourceFolder = System.IO.Path.GetDirectoryName(folder),
                MissionName = System.IO.Path.GetFileName(folder)
            };
            var src = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var f in src)
                pbo.AddFileFromFolder(f, folder);
            return pbo;
        }

        /// <summary>
        ///     Constructs a new PboFile object from an existing .pbo file
        /// </summary>
        /// <remarks>See the PboLoadMode for further information on the load-modes.</remarks>
        /// <param name="fileName">The path to the .pbo file</param>
        /// <param name="mode">The mode used when loading PboEntries within the PboFile</param>
        /// <returns>A new PboFile object</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static PboFile FromPbo(string fileName, PboLoadMode mode = PboLoadMode.Immediate) {
            var fs = File.OpenRead(fileName);
            var pbo = FromStream(fs, mode);
            if (mode == PboLoadMode.Immediate)
                fs.Close();
            pbo.FileName = System.IO.Path.GetFileName(fileName);
            pbo._sourceFolder = System.IO.Path.GetDirectoryName(fileName);
            return pbo;
        }

        /// <summary>
        ///     Constructs a new PboFile from a stream of bytes representing a .pbo file
        /// </summary>
        /// <remarks>
        ///     This constructor may be useful when the the source of the pbo file is not physically available as a file.
        ///     For example, if a pbo is included in an addon, this constructor may be used to access it
        /// </remarks>
        /// <example>
        ///     <code>
        /// //Do something with all the .pbo files inside my_addon.pbo
        /// PboFile addon = PboFile.FromPbo("my_addon.pbo");
        /// foreach (PboEntry e in addon.Match("*.pbo",MatchType.Path))
        /// {
        ///     PboFile f = PboFile.FromStream(e.ToStream()) ;
        ///     //do something with the included pbo     
        /// }
        /// </code>
        /// </example>
        /// <param name="input">A stream representing a .pbo file</param>
        /// <param name="mode">The mode used when loading PboEntries within the PboFile</param>
        /// <returns>A new PboFile</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static PboFile FromStream(Stream input, PboLoadMode mode = PboLoadMode.Immediate) {
            var pbo = new PboFile();
            while (input.Position < input.Length) {
                var e = new PboEntry();
                e.ReadHeader(input);
                if (e.Path.Length == 0) {
                    if (pbo._entries.Count != 0) {
                        //this is the end of header marker
                        break;
                    }
                    //this is a header extension so slurp up strings
                    while (BinaryFile.ReadString(input).Length != 0) {}
                    //System.Console.WriteLine("skipping " + config); ;
                } else
                    pbo._entries.Add(e);
            }
            foreach (var pf in pbo)
                pf.ReadBody(input, -1, mode);
            return pbo;
        }

        /// <summary>
        ///     Extract all files within a PboFile
        /// </summary>
        /// <remarks>
        ///     All files within the PboFile are extracted to the specified destination folder.  The
        ///     relative directory structure within the pbo is copied when extracting the contents
        /// </remarks>
        /// <param name="destinationFolder">TBD</param>
        public void ExtractTo(string destinationFolder) {
            if (destinationFolder == null)
                destinationFolder = _sourceFolder;
            foreach (var f in this)
                f.ExtractTo(System.IO.Path.Combine(destinationFolder, MissionName));
        }

        /// <summary>
        ///     Extract the contents of the PboFile to its current SourceFolder
        /// </summary>
        public void Extract() {
            ExtractTo(_sourceFolder);
        }

        /// <summary>
        ///     Saves the PboFile to disk as a .pbo file
        /// </summary>
        /// <remarks>
        ///     The PboFile is written to the specified folder with the name in the FileName
        ///     property.  The Path property is NOT updated when using this method.
        /// </remarks>
        /// <param name="folder"></param>
        public void WriteAsPboIn(string folder) {
            if (folder == null)
                folder = _sourceFolder;
            var dest = System.IO.Path.Combine(folder, FileName);
            using (var s = File.Create(dest)) {
                var entries = this.ToList();
                entries.Sort(PboEntry.PathSorter);
                foreach (var e in entries)
                    e.WriteHeader(s);
                //write the end of header marker
                var end = new PboEntry();

                //now write the file contents
                end.WriteHeader(s);

                foreach (var e in entries)
                    e.WriteBody(s);
            }
        }

        /// <summary>
        ///     Saves the PboFile to disk as a .pbo file in the SourceFolder
        /// </summary>
        public void WriteAsPbo() {
            WriteAsPboIn(_sourceFolder);
        }

        /// <summary>
        ///     Creates a PboEntry from a file and adds it to the PboFile
        /// </summary>
        /// <remarks>
        ///     This method can be used to conveniently add a file from a mission folder.  The Path of the
        ///     resulting PboEntry is calculated to be the relative path between fileName and folder
        /// </remarks>
        /// <param name="fileName">The absolute path of the input file</param>
        /// <param name="folder">The root of the mission</param>
        /// <returns>The PboEntry that represents the added file</returns>
        public PboEntry AddFileFromFolder(string fileName, string folder) {
            var e = PboEntry.FromFileRelativeToFolder(fileName, folder);
            _entries.Add(e);
            return e;
        }

        /// <summary>
        ///     Adds an existing PboEntry to the PboFile
        /// </summary>
        /// <remarks>
        ///     If a PboEntry with the same path as the new entry is already present in the PboFile,
        ///     it will be removed before the new one is added
        /// </remarks>
        /// <param name="entry">The PboEntry to be added</param>
        public void Add(PboEntry entry) {
            var prev = this[entry.Path];
            if (prev != null)
                Remove(prev);
            _entries.Add(entry);
        }

        /// <summary>
        ///     Adds a number of PboEntries to a PboFile
        /// </summary>
        /// <param name="entries">A collection of entries to be added</param>
        public void AddRange(IEnumerable<PboEntry> entries) {
            foreach (var e in entries)
                Add(e);
        }

        /// <summary>
        ///     Removes the specified PboEntry from the PboFile
        /// </summary>
        /// <param name="entry">The PboEntry to remove</param>
        public void Remove(PboEntry entry) {
            _entries.Remove(entry);
        }

        /// <summary>
        ///     Removes a number of PboEntries from a PboFile
        /// </summary>
        /// <param name="entries">A collection of entries to be removed</param>
        public void RemoveRange(IEnumerable<PboEntry> entries) {
            foreach (var e in entries)
                Remove(e);
        }


        /// <summary>
        ///     Writes the contents of a PboFile back to their source
        /// </summary>
        /// <remarks>
        ///     WriteBack allows the contents of a PboFile to be saved back to its source.  If the PboFile was constructed from
        ///     a .pbo file, the file is updated with the new contents.  If the PboFile was constructed from a folder, the updated
        ///     contents overwrite the orgininal files on the disk.  Calling WriteBack on a PboFile constructed from a stream
        ///     will result in an exception.
        /// </remarks>
        /// <example>
        ///     <code>
        /// //Add a new script file to an existing mission
        /// PboFile pbo = Pbofile.FromFolder("mission.utes");
        /// pbo.AddFile("myScriptLibraries",@"lib\newScript.sqf");
        /// pbo.WriteBack();
        /// </code>
        /// </example>
        public void WriteBack() {
            if (_createdFromFolder)
                Extract();
            else
                WriteAsPbo();
        }
    }
}