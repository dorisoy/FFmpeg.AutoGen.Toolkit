# FFmpeg.AutoGen.Toolkit

FFmpeg.AutoGen.Toolkit ��һ��.NET�⣬���ڴ����Ͷ�ȡ��ý���ļ�,��ʹ�� [FFFmpeg.Autogen](https://github.com/Ruslan-B/FFmpeg.AutoGen) ��ԭ��FFmpeg ��(https://github.com/Ruslan-B/FFmpeg.AutoGen)��

## ����

- ��USB����ͷ��ȡ��Ƶ֡������ΪH.264��ʽ��
- ����/����FFmpeg֧�ֵĶ��ָ�ʽ����Ƶ��Ƶ�ļ���
- ����Ƶ������ȡΪ�������顣
- ��ʱ��������κ���Ƶ֡��
- ʹ�� metadata, pixel format, bitrate, CRF, FPS, GoP, dimensions ������codec ���������ô�ͼ�񴴽���Ƶ��
- ֧�ֶ�ȡ multimedia chapters �� metadataԪ���ݡ�

## ʾ�����μ�Sampleʾ������


```c#
Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
Console.WriteLine("Running in {0}-bit mode.", Environment.Is64BitProcess ? "64" : "32");

FFmpegBinariesHelper.RegisterFFmpegBinaries();
DynamicallyLoadedBindings.Initialize();

Console.WriteLine($"FFmpeg version info: {ffmpeg.av_version_info()}");

// Register
ffmpeg.avdevice_register_all();

// ������ͷ
using VideoCapture capture = new VideoCapture(0, VideoCapture.API.DShow);
// �������ͷ�Ƿ�򿪳ɹ�
if (!capture.IsOpened)
{
	Debug.WriteLine("�޷�������ͷ");
	return;
}

// ���
var outPath = @"C:\Users\Administrator\Desktop\Sinol\test\out.mp4";


#region H264����

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
			// ������ͷ�ж�ȡ��Ƶ֡
			capture.Read(mat);

			// �����Ƶ֡�Ƿ�Ϊ��
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


#region H264����


// ���ļ�
using var file = MediaFile.Open(outPath);

// ��ȡ��Ƶ��5���֡
//var frame5s = file.Video.GetFrame(TimeSpan.FromSeconds(5));

// ��ӡ�й���Ƶ������Ϣ
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


