// <copyright company="SIX Networks GmbH" file="ConvertIcons.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NDepend.Path;
using NUnit.Framework;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground
{
    public class ConvertIcons
    {
        public void WriteClass(IAbsoluteFilePath input, IAbsoluteFilePath output) {
            File.WriteAllText(output.ToString(), Convert(input));
        }

        public string Convert(IAbsoluteFilePath input) {
            var generator = new IconClassGenerator(input);
            return generator.GetLines();
        }

        class IconClassGenerator
        {
            readonly IAbsoluteFilePath _fileName;
            StringBuilder _text;

            public IconClassGenerator(IAbsoluteFilePath fileName) {
                _fileName = fileName;
            }

            public string GetLines() {
                _text = new StringBuilder();

                WriteHeader();

                ParseCss();

                WriteFooter();

                return _text.ToString();
            }

            void WriteHeader() {
                _text.AppendLine("    public static class SixIconFont {");
            }

            void ParseCss() {
                var rxName = new Regex(@"\.(.*):before", RegexOptions.Compiled);
                var rxCode = new Regex("	content: \"(.*)\"", RegexOptions.Compiled);
                var lines = ReadFile();
                for (var index = 0; index < lines.Length; index++) {
                    var line = lines[index];
                    var match = rxName.Match(line);
                    if (!match.Success)
                        continue;
                    var name = match.Groups[1].Value.Replace("-", "_");
                    var nextLine = lines[++index];
                    var code = rxCode.Match(nextLine).Groups[1].Value.Replace(@"\e", @"\ue");
                    _text.AppendLine("public const string {0} = \"{1}\";", name, code);
                }
            }

            string[] ReadFile() {
                var lines = File.ReadLines(_fileName.ToString()).ToArray();
                return lines;
            }

            void WriteFooter() {
                _text.AppendLine("}");
            }
        }
    }

    public static class StringBuilderExtensions
    {
        public static void AppendLine(this StringBuilder builder, string format, params object[] pars) {
            builder.AppendLine(string.Format(format, pars));
        }
    }

    [TestFixture]
    public class ConvertIconsTest
    {
        [Test]
        public void GenerateIcons() {
            var input = @"D:\temp\icons\style.css".ToAbsoluteFilePath();
            var output = @"D:\temp\icons\output.css".ToAbsoluteFilePath();

            var writer = new ConvertIcons();
            Console.WriteLine(writer.Convert(input));
            //writer.WriteClass(input, output);
        }
    }
}