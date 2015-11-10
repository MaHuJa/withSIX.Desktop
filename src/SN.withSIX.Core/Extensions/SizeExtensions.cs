// <copyright company="SIX Networks GmbH" file="SizeExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Core.Extensions
{
    public static class SizeExtensions
    {
        public static readonly ContentLengthRange DefaultContentLengthRange =
            new ContentLengthRange(1.ToBytes(FileSizeHelper.Units.kB), 5.ToBytes(FileSizeHelper.Units.MB));

        public static long ToBytes(this long size, FileSizeHelper.Units inputUnit) {
            switch (inputUnit) {
            case FileSizeHelper.Units.B:
                return size;
            case FileSizeHelper.Units.kB:
                return size*1024;
            case FileSizeHelper.Units.MB:
                return size*1024*1024;
            case FileSizeHelper.Units.GB:
                return size*1024*1024*1024;
            case FileSizeHelper.Units.TB:
                return size*1024*1024*1024*1024;
            case FileSizeHelper.Units.PB:
                throw new NotSupportedException("PetaBytes are not supported",
                    new OverflowException(
                        "Only crazy people said computers would be around after the year 2000 now look where we are, How do you even have a file this big?"));
            case FileSizeHelper.Units.EB:
                throw new NotSupportedException("ExaBytes are not supported",
                    new OverflowException("How to kill a Mockingbird... I mean server."));
            case FileSizeHelper.Units.ZB:
                throw new NotSupportedException("ZetaBytes are not supported",
                    new OverflowException(
                        "For every 1 ZetaByte you upload you get a free PetaByte! Unfortunately, we're out of both."));
            case FileSizeHelper.Units.YB:
                throw new NotSupportedException("YotaBytes are not supported",
                    new OverflowException(
                        "We choose to upload YotaBytes in this decade and do the other things, not because they are easy, but because they are hard."));
            default:
                throw new ArgumentOutOfRangeException("inputUnit");
            }
        }

        public static long ToBytes(this int size, FileSizeHelper.Units inputUnit) {
            return ToBytes((long) size, inputUnit);
        }

        public static void VerifySize(this ContentLengthRange range, long size) {
            if (size < range.Minimum) {
                throw new UnsupportedFileSizeException("The file you try to upload is too small. (Minimum Size is " +
                                                       range.Minimum.FormatSize() + ")");
            }
            if (size > range.Maximum) {
                throw new UnsupportedFileSizeException("The file you try to upload is too big. (Maximum Size is " +
                                                       range.Maximum.FormatSize() + ")");
            }
        }

        public static string FormatSize(this double size, Tools.FileTools.Units unit = Tools.FileTools.Units.B) {
            return Tools.FileUtil.GetFileSize(size, unit);
        }

        public static string FormatSpeed(this double speed, Tools.FileTools.Units unit = Tools.FileTools.Units.B) {
            return Tools.FileUtil.GetFileSize(speed, postFix: "/s");
        }

        public static string FormatSize(this long size, Tools.FileTools.Units unit = Tools.FileTools.Units.B) {
            return Tools.FileUtil.GetFileSize(size, unit);
        }

        public static string FormatSize(this int size, Tools.FileTools.Units unit = Tools.FileTools.Units.B) {
            return Tools.FileUtil.GetFileSize(size, unit);
        }

        public static string FormatSpeed(this int speed, Tools.FileTools.Units unit = Tools.FileTools.Units.B) {
            return Tools.FileUtil.GetFileSize(speed, postFix: "/s");
        }

        public struct ContentLengthRange
        {
            public ContentLengthRange(long minimum, long maximum) {
                Minimum = minimum;
                Maximum = maximum;
            }

            public long Minimum { get; }
            public long Maximum { get; }
        }

        public class UnsupportedFileSizeException : Exception
        {
            public UnsupportedFileSizeException(string message) : base(message) {}
        }
    }

    public static class FileSizeHelper
    {
        public enum Units
        {
            B = 0,
            kB,
            MB,
            GB,
            TB,
            PB,
            EB,
            ZB,
            YB
        }

        static readonly string defaultSizeReturn = String.Empty;

        public static string GetFileSize(this double size, Units unit = Units.B, string postFix = null) {
            if (size < 0)
                return defaultSizeReturn;

            while (size >= 1024) {
                size /= 1024;
                ++unit;
            }

            var s = String.Format("{0:0.##} {1}", size, unit);
            return postFix == null ? s : s + postFix;
        }

        public static string GetFileSize(this int size, Units unit = Units.B, string postFix = null) {
            return ((double) size).GetFileSize(unit, postFix);
        }

        public static string GetFileSize(this long size, Units unit = Units.B, string postFix = null) {
            return ((double) size).GetFileSize(unit, postFix);
        }

        public static class FileSize
        {
            const int Unit = 1024;
            public const int KB = Unit;
            public const int MB = KB*Unit;
            public const long GB = MB*Unit;
            public const long TB = GB*Unit;
        }
    }
}