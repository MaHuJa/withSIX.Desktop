// <copyright company="SIX Networks GmbH" file="RapFile.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using Mac.Arma.Config;

namespace Mac.Arma.Files
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Exception for signalling rap format errors. </summary>
    /// <remarks>  </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class RapFormatException : Exception
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor </summary>
        /// <remarks>   </remarks>
        /// <param name="message">  The message. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public RapFormatException(string message)
            : base(message) {}
    }

    /// <summary>
    ///     Supports reading and writing of Arma .RAP files
    /// </summary>
    /// <remarks>
    ///     RapFile is simply a mechanism for converting between a particular byte format in a file (ie, ,rap)
    ///     and a ConfigFile which is a flexible logical data structure representing a set of classes and
    ///     properties.  (You can think of a ConfigFile as Document Object Model if it helps).
    ///     This approach allows clients which operate on arma configurations to be agnostic about whether
    ///     the config was obtained from a rap file, cpp text file or other source.
    ///     See ConfigFile for further documentation
    /// </remarks>
    public class RapFile
    {
        readonly Stream _input;


        RapFile(Stream inputStream) {
            _input = inputStream;
            ReadAll();
        }


        RapFile(string fileName) {
            using (_input = File.OpenRead(fileName))
                ReadAll();
        }

        ConfigFile Root { get; set; }

        /// <summary>
        ///     Checks whether a file appears to be a valid .rap file
        /// </summary>
        /// <param name="path">The path of the file to be checked</param>
        /// <returns>false if the file cannot be opened or has the wrong signature. True otherwise</returns>
        public static bool IsRap(string path) {
            try {
                using (var fs = File.OpenRead(path))
                    return IsRap(fs);
            } catch (FileNotFoundException) {
                return false;
            }
        }

        /// <summary>
        ///     Checks whether a stream starts with a valid .rap signature
        /// </summary>
        /// <remarks>The process of checking the stream does not move the seek pointer</remarks>
        /// <param name="inputStream">The stream to be checked</param>
        /// <returns>false if the stream has the wrong signature. True otherwise</returns>
        public static bool IsRap(Stream inputStream) {
            return CheckSignature(inputStream);
        }

        /// <summary>
        ///     Returns a ConfigFile by parsing a .rap file
        /// </summary>
        /// <remarks>Will throw a RapFormatException if the file is malformed</remarks>
        /// <param name="fileName">The path to the .rap file</param>
        public static ConfigFile ReadConfig(string fileName) {
            var rap = new RapFile(fileName);
            return rap.Root;
        }

        /// <summary>
        ///     Returns a ConfigFile by parsing a stream representing a .rap file
        /// </summary>
        /// <remarks>Will throw a RapFormatException if the file is malformed</remarks>
        /// <param name="inputStream">The stream representing a .rap file</param>
        public static ConfigFile ReadConfig(Stream inputStream) {
            var rap = new RapFile(inputStream);
            return rap.Root;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Writes a ConfigFile in .rap format </summary>
        /// <remarks>
        ///     Not yet implemented - make a forum request if
        ///     you require this functionality
        /// </remarks>
        /// <exception cref="NotImplementedException">
        ///     Thrown when the requested operation is
        ///     unimplemented.
        /// </exception>
        /// <param name="config">   The configuration. </param>
        /// <param name="output">   The output. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void WriteConfig(ConfigFile config, Stream output) {
            throw new NotImplementedException();
        }

        //Private function used both when checking IsRap 
        //and at the start of parsing
        static bool CheckSignature(Stream input) {
            var valid = true;
            //save the position before starting to read
            var pos = input.Position;
            valid &= (input.ReadByte() == 0);
            valid &= (input.ReadByte() == 'r');
            valid &= (input.ReadByte() == 'a');
            valid &= (input.ReadByte() == 'P');
            //return to the previous position
            input.Seek(pos, SeekOrigin.Begin);
            return valid;
        }

        #region ParsingCode

        void ReadAll() {
            if (!CheckSignature(_input))
                throw new RapFormatException("Attempt to read a rap file with an invalid signature");
            //skip signature since CheckSignature helpfully rewinds
            _input.Seek(4, SeekOrigin.Current);
            //skip reserved bytes
            _input.Seek(6, SeekOrigin.Current);
            //skip first entryType
            _input.Seek(1, SeekOrigin.Current);
            //skip null classname
            BinaryFile.ReadString(_input);
            //skip enum offset
            var enumOffset = BinaryFile.ReadUInt32(_input);

            //now we are at the actual list of classes so start reading

            var c = ReadClassBody("", _input.Position);
            Root = new ConfigFile(c);

            //finally read the enums if present and add them to the Root
            if (enumOffset != 0) {
                var enums = ReadEnums(enumOffset);
                if (enums != null)
                    Root.Add(enums);
            }
        }

        // reads a class body 
        ConfigClass ReadClassBody(string name, long bodyOffset) {
            // Class bodies are distributed separately from their definitions
            // so before we go looking we need to take note of where we started
            var currentOffset = _input.Position;

            //read the parent name
            _input.Seek(bodyOffset, SeekOrigin.Begin);
            var parentName = BinaryFile.ReadString(_input);
            var cfgClass = new ConfigClass(name, parentName);

            //read all entries in the class body
            var entryCount = BinaryFile.ReadCompressedInteger(_input);
            for (var c = 0; c < entryCount; c++)
                cfgClass.Add(ReadConfigEntry());
            //ensure we haven't disturbed the seek position
            _input.Seek(currentOffset, SeekOrigin.Begin);
            return cfgClass;
        }

        //reads any one of a number of different types of entry that could
        //be found in a class body
        ConfigEntry ReadConfigEntry() {
            var type = _input.ReadByte();
            switch (type) {
            case 0:
                return ReadConfigClass();
            case 1:
                return ReadConfigValue();
            case 2:
                return ReadConfigArray();
            case 3:
                return ReadConfigExtern();
            case 4:
                return ReadConfigDelete();
            }
            return null;
        }

        //class headers are separated from their declarations 
        //so we need to fetch them separately
        ConfigClass ReadConfigClass() {
            var name = BinaryFile.ReadString(_input);
            long bodyOffset = BinaryFile.ReadInt32(_input);
            return ReadClassBody(name, bodyOffset);
        }

        //A configValue is just a basic type - int, float or string
        ConfigProperty ReadConfigValue() {
            ConfigProperty e;
            var subType = _input.ReadByte();
            var name = BinaryFile.ReadString(_input);
            switch (subType) {
            case 0:
                e = ReadConfigString(name);
                break;
            case 1:
                e = ReadConfigFloat(name);
                break;
            case 2:
                e = ReadConfigInt(name);
                break;
            default:
                throw new RapFormatException("Invalid property type " + subType + " found in rap file");
            }
            return e;
        }

        StringProperty ReadConfigString(string name) {
            var value = BinaryFile.ReadString(_input);
            return new StringProperty(name, value);
        }

        FloatProperty ReadConfigFloat(string name) {
            var value = BinaryFile.ReadFloat32(_input);
            return new FloatProperty(name, value);
        }

        IntProperty ReadConfigInt(string name) {
            var value = BinaryFile.ReadInt32(_input);
            return new IntProperty(name, value);
        }

        ExternReference ReadConfigExtern() {
            var className = BinaryFile.ReadString(_input);
            return new ExternReference(className);
        }

        DeleteReference ReadConfigDelete() {
            var className = BinaryFile.ReadString(_input);
            return new DeleteReference(className);
        }

        static IEnumerable<Object> ReadAnonymousArray(Stream input) {
            var entries = BinaryFile.ReadCompressedInteger(input);
            var objects = new List<Object>();
            for (var i = 0; i < entries; i++) {
                var subType = input.ReadByte();
                switch (subType) {
                case 0:
                    objects.Add(BinaryFile.ReadString(input));
                    break;
                case 1:
                    objects.Add(BinaryFile.ReadFloat32(input));
                    break;
                case 2:
                    objects.Add(BinaryFile.ReadInt32(input));
                    break;
                case 3:
                    objects.Add(ReadAnonymousArray(input));
                    break;
                    //TODO - need to do something with this !
                    //case 4: objects.Add(BinaryFile.ReadString(input)); break;
                default:
                    throw new RapFormatException("Unexpected array element type " + subType + " found in rap file");
                }
            }
            return objects;
        }

        ConfigProperty ReadConfigArray() {
            var name = BinaryFile.ReadString(_input);
            var objects = ReadAnonymousArray(_input);
            var a = new ArrayProperty(name, objects);
            return a;
        }

        //the enums list is just a list of strings with values
        ConfigEnum ReadEnums(long bodyOffset) {
            _input.Seek(bodyOffset, SeekOrigin.Begin);
            var entryCount = BinaryFile.ReadInt32(_input);
            if (entryCount == 0)
                return null;
            var ret = new ConfigEnum();
            for (var c = 0; c < entryCount; c++) {
                var name = BinaryFile.ReadString(_input);
                var val = BinaryFile.ReadInt32(_input);
                ret.Add(name, val);
            }
            return ret;
        }

        #endregion ParsingCode

        #region Binarising Code

        static void BinariseAssign(MemoryStream mem, string name, string val) {
            BinaryFile.WriteByte(mem, 1);
            BinaryFile.WriteByte(mem, 0);
            BinaryFile.WriteString(mem, name);
            BinaryFile.WriteString(mem, val);
        }

        static void BinariseAssign(MemoryStream mem, string name, float val) {
            BinaryFile.WriteByte(mem, 1);
            BinaryFile.WriteByte(mem, 1);
            BinaryFile.WriteString(mem, name);
            BinaryFile.WriteFloat32(mem, val);
        }

        static void BinariseAssign(MemoryStream mem, string name, Int32 val) {
            BinaryFile.WriteByte(mem, 1);
            BinaryFile.WriteByte(mem, 2);
            BinaryFile.WriteString(mem, name);
            BinaryFile.WriteUInt32(mem, (UInt32) val);
        }

        static void BinariseAssignVar(MemoryStream mem, string name, string var) {
            BinaryFile.WriteByte(mem, 1);
            BinaryFile.WriteByte(mem, 4);
            BinaryFile.WriteString(mem, name);
            BinaryFile.WriteString(mem, var);
        }

        static void BinariseClass(MemoryStream mem, string name, UInt32 offset) {
            BinaryFile.WriteByte(mem, 0);
            BinaryFile.WriteString(mem, name);
            BinaryFile.WriteUInt32(mem, offset);
        }

        static void BinariseClassBody(MemoryStream mem, string parent, int n) {
            BinaryFile.WriteString(mem, parent);
            BinaryFile.WriteCompressedInteger(mem, n);
        }

        static void BinariseArray(MemoryStream mem, string name, int n) {
            BinaryFile.WriteByte(mem, 2);
            BinaryFile.WriteString(mem, name);
            BinaryFile.WriteCompressedInteger(mem, n);
        }

        static void BinariseEmbedded(MemoryStream mem, string n) {
            BinaryFile.WriteByte(mem, 0);
            BinaryFile.WriteString(mem, n);
        }

        static void BinariseEmbedded(MemoryStream mem, float n) {
            BinaryFile.WriteByte(mem, 1);
            BinaryFile.WriteFloat32(mem, n);
        }

        static void BinariseEmbedded(MemoryStream mem, UInt32 n) {
            BinaryFile.WriteByte(mem, 2);
            BinaryFile.WriteUInt32(mem, n);
        }

        static void BinariseEmbeddedArray(MemoryStream mem) {
            BinaryFile.WriteByte(mem, 3);
        }


        static void BinariseExtern(MemoryStream mem, string name) {
            BinaryFile.WriteByte(mem, 3);
            BinaryFile.WriteString(mem, name);
        }

        static void BinariseDelete(MemoryStream mem, string name) {
            BinaryFile.WriteByte(mem, 4);
            BinaryFile.WriteString(mem, name);
        }

        static void BinariseHeader(MemoryStream mem) {
            BinaryFile.WriteByte(mem, 0);
            BinaryFile.WriteByte(mem, (byte) 'r');
            BinaryFile.WriteByte(mem, (byte) 'a');
            BinaryFile.WriteByte(mem, (byte) 'P');
            for (var i = 0; i < 12; i++)
                BinaryFile.WriteByte(mem, 0);
        }

        static void BinariseEnum(MemoryStream mem, string name, long val) {
            BinaryFile.WriteString(mem, name);
            BinaryFile.WriteUInt32(mem, (UInt32) val);
        }

        class Binariser : ConfigVisitor
        {
            MemoryStream mem;

            protected override void VisitConfigFile(ConfigFile node) {
                throw new NotImplementedException();
                BinariseHeader(mem);
                base.VisitConfigFile(node);
            }

            protected override void VisitConfigClass(ConfigClass node) {
                base.VisitConfigClass(node);
            }
        }

        #endregion
    }
}