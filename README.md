# FFmpeg.AutoGen.Toolkit

FFmpeg.AutoGen.Toolkit 是一个.NET库，用于创建和读取多媒体文件,它使用 [FFFmpeg.Autogen](https://github.com/Ruslan-B/FFmpeg.AutoGen) 的原生FFmpeg 库(https://github.com/Ruslan-B/FFmpeg.AutoGen)绑定

## 功能

- 从USB摄像头读取视频帧并编码为H.264格式。
- 解码/编码FFmpeg支持的多种格式的音频视频文件。
- 将音频数据提取为浮点数组。
- 按时间戳访问任何视频帧。
- 使用 metadata, pixel format, bitrate, CRF, FPS, GoP, dimensions 和其它codec 解码器设置从图像创建视频。
- 支持读取 multimedia chapters 和 metadata元数据。

## 示例（参见Sample示例程序）


```c#
Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
Console.WriteLine("Running in {0}-bit mode.", Environment.Is64BitProcess ? "64" : "32");

FFmpegBinariesHelper.RegisterFFmpegBinaries();
DynamicallyLoadedBindings.Initialize();

Console.WriteLine($"FFmpeg version info: {ffmpeg.av_version_info()}");

// Register
ffmpeg.avdevice_register_all();

// 打开摄像头
using VideoCapture capture = new VideoCapture(0, VideoCapture.API.DShow);
// 检查摄像头是否打开成功
if (!capture.IsOpened)
{
	Debug.WriteLine("无法打开摄像头");
	return;
}

// 输出
var outPath = @"C:\Users\Administrator\Desktop\Sinol\test\out.mp4";


#region H264编码

try
{
	var settings = new VideoEncoderSettings(width: 1920, height: 1080, framerate: 30, codec: VideoCodec.H264);
	settings.EncoderPreset = EncoderPreset.Fast;
	settings.CRF = 17;
	var frameNumber = 0;
	using (Mat mat = new Mat())
	{
		using var vfile = MediaBuilder
		.CreateContainer(outPath)
		.WithVideo(settings)
		.Create();

		while (frameNumber <= 100)
		{
			// 从摄像头中读取视频帧
			capture.Read(mat);

			// 检查视频帧是否为空
			if (mat.IsEmpty)
				break;

			var buffer = new VectorOfByte();
			using var img_trans = mat.ToImage<Rgb, byte>();
			{
				using var bitmap = img_trans.ToBitmap();
				{
					var rect = new Rectangle(Point.Empty, bitmap.Size);
					var bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
					var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bitmap.Size);

					Console.WriteLine($"AddFrame: {bitmapData.Data.Length}");

					vfile.Video.AddFrame(bitmapData);

					bitmap.UnlockBits(bitLock);
				}
			}
			frameNumber++;
		}

	}
}
catch (Exception ex)
{
	Console.WriteLine($"{ex.Message}");
}

#endregion


#region H264解码


// 打开文件
using var file = MediaFile.Open(outPath);

// 获取视频第5秒的帧
//var frame5s = file.Video.GetFrame(TimeSpan.FromSeconds(5));

// 打印有关视频流的信息
Console.WriteLine($"Bitrate: {file.Info.Bitrate / 1000.0} kb/s");

var info = file.Video.Info;

Console.WriteLine($"Duration: {info.Duration}");
Console.WriteLine($"Frames count: {info?.NumberOfFrames}");

var frameRateInfo = (info?.IsVariableFrameRate ?? false) ? "average" : "constant";

Console.WriteLine($"Frame rate: {info?.AvgFrameRate} fps ({frameRateInfo})");
Console.WriteLine($"Frame size: {info?.FrameSize}");
Console.WriteLine($"Pixel format: {info?.PixelFormat}");
Console.WriteLine($"Codec: {info?.CodecName}");
Console.WriteLine($"Is interlaced: {info?.IsInterlaced}");

int i = 0;
while (file.Video.TryGetNextFrame(out ImageData imageData))
{
	i++;

	imageData.ToBitmap()
	.Save($@"C:\Users\Administrator\Desktop\Sinol\test\frames\frame_{i++}.png");
}


#endregion

/*
	Bitrate: 6704.875 kb/s
	Duration: 00:00:03.3333333
	Frames count: 101
	Frame rate: 30.3 fps (average)
	Frame size: {Width=1920, Height=1080}
	Pixel format: yuv420p
	Codec: h264
	Is interlaced: False
	*/

Console.WriteLine($"Done!");


```


