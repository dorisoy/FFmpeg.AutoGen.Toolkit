namespace FFmpeg.AutoGen.Toolkit
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using FFmpeg.AutoGen.Toolkit.Interop;
    using FFmpeg.AutoGen.Abstractions;

    /// <summary>
    /// Contains methods for managing FFmpeg libraries.
    /// </summary>
    public static class FFmpegLoader
    {
        private static LogLevel logLevel = LogLevel.Error;
        private static bool isPathSet;

        /// <summary>
        /// Delegate for log message callback.
        /// </summary>
        /// <param name="message">The message.</param>
        public delegate void LogCallbackDelegate(string message);

        /// <summary>
        /// Log message callback event.
        /// </summary>
        public static event LogCallbackDelegate LogCallback;

        /// <summary>
        /// Gets or sets the verbosity level of FFMpeg logs printed to standard error/output.
        /// Default value is <see cref="LogLevel.Error"/>.
        /// </summary>
        public static LogLevel LogVerbosity
        {
            get => logLevel;
            set
            {
                if (IsFFmpegLoaded)
                {
                    ffmpeg.av_log_set_level((int)value);
                }

                logLevel = value;
            }
        }

        /// <summary>
        /// Gets the FFmpeg version info string.
        /// Empty when FFmpeg libraries were not yet loaded.
        /// </summary>
        public static string FFmpegVersion { get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the loaded FFmpeg binary files are licensed under the GPL.
        /// Null when FFmpeg libraries were not yet loaded.
        /// </summary>
        public static bool? IsFFmpegGplLicensed { get; private set; }

        /// <summary>
        /// Gets the FFmpeg license text
        /// Empty when FFmpeg libraries were not yet loaded.
        /// </summary>
        public static string FFmpegLicense { get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the FFmpeg binary files were successfully loaded.
        /// </summary>
        internal static bool IsFFmpegLoaded { get; private set; }

   
        //public static void LoadFFmpeg()
        //{
        //    if (IsFFmpegLoaded)
        //    {
        //        return;
        //    }

        //    if (!isPathSet)
        //    {
        //        try
        //        {
        //            FFmpegPath = NativeMethods.GetFFmpegDirectory();
        //        }
        //        catch (DirectoryNotFoundException)
        //        {
        //            throw new DirectoryNotFoundException("Cannot found the default FFmpeg directory.\n" +
        //                "On Windows you have to set \"FFmpegLoader.FFmpegPath\" with full path to the directory containing FFmpeg 5.x shared build \".dll\" files\n" +
        //                "For more informations please see https://github.com/radek-k/FFmpeg.AutoGen.Toolkit#setup");
        //        }
        //    }

        //    try
        //    {
        //        FFmpegVersion = ffmpeg.av_version_info();
        //        FFmpegLicense = ffmpeg.avcodec_license();
        //        IsFFmpegGplLicensed = FFmpegLicense.StartsWith("GPL");
        //    }
        //    catch (DllNotFoundException ex)
        //    {
        //        HandleLibraryLoadError(ex);
        //    }
        //    catch (NotSupportedException ex)
        //    {
        //        HandleLibraryLoadError(ex);
        //    }

        //    IsFFmpegLoaded = true;
        //    LogVerbosity = logLevel;
        //}

        /// <summary>
        /// Start logging ffmpeg output.
        /// </summary>
        public static unsafe void SetupLogging()
        {
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);

            // do not convert to local function
            av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
            {
                if (level > ffmpeg.av_log_get_level())
                    return;

                var lineSize = 1024;
                var lineBuffer = stackalloc byte[lineSize];
                var printPrefix = 1;
                ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
                var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
                LogCallback?.Invoke(line);
            };

            ffmpeg.av_log_set_callback(logCallback);
        }

    }
}
