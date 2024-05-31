using System.Globalization;
using System.Text.RegularExpressions;

namespace InnerTube.Parsers.Languages;

[ValueParser("ms")]
public partial class Malasian : IValueParser
{
	private Regex fullDatePattern = FullDatePatternRegex();
	private Regex shortNumberRegex = ShortNumberRegex();
	private Regex viewCountRegex = ViewCountRegex();
	private string[] months = [
			"Jan","Feb","Mac","Apr",
			"Mei","Jun","Jul","Ogo",
			"Sep","Okt","Nov","Dis"
		];

	public string ParseRelativeDate(string date)
	{
		string[] parts = date.ToLower().Split(" ");
		string metric = parts[1];
		int amount = int.Parse(parts[0]);
		return metric switch
		{
			"saat" => $"-{amount}s",
			"minit" => $"-{amount}m",
			"jam" =>   $"-{amount}h",
			"hari" =>    $"-{amount}D",
			"minggu" =>   $"-{amount}W",
			"bulan" =>  $"-{amount}M",
			"tahun" =>   $"-{amount}Y",
			_ => $"!Unknown metric;{metric};{amount};{date}"
		};
	}

	public DateTimeOffset ParseFullDate(string date)
	{
		Match match = fullDatePattern.Match(date);

		return DateTimeOffset.ParseExact($"{match.Groups[3].Value}/{Array.IndexOf(months, match.Groups[2].Value.TrimEnd('.')) + 1}/{match.Groups[1].Value}",
			"yyyy/M/d", GetCultureInfo());
	}

	public VideoUploadType ParseVideoUploadType(string type)
	{
		// premiered and future premier is the same, returning premiered.
		if (type.StartsWith("Tayangan")) return VideoUploadType.Premiered;
		if (type.StartsWith("Mula")) return VideoUploadType.Streaming;
		if (type.StartsWith("Distrim")) return VideoUploadType.Streamed;
		if (type.StartsWith("Dijadualkan")) return VideoUploadType.ScheduledStream;
		return VideoUploadType.Published;
	}

	public long ParseSubscriberCount(string subscriberCountText)
	{
		string digitsPart = subscriberCountText.Split(" ")[0];
		return ParseShortNumber(digitsPart);
	}

	public long ParseLikeCount(string likeCountText) =>
		ParseShortNumber(likeCountText);

	public long ParseViewCount(string viewCountText) => int.Parse(viewCountRegex.Match(viewCountText).Groups[1].Value.Replace(".",""),
		NumberStyles.AllowThousands, GetCultureInfo());

	public long ParseVideoCount(string videoCountText) =>
		!videoCountText.Contains("video")
			? ParseShortNumber(videoCountText) 
			: 0;

	public DateTimeOffset ParseLastUpdated(string lastUpdatedText) =>
		ParseFullDate(lastUpdatedText.Split("Terakhir")[1]);

	private long ParseShortNumber(string part)
	{
		try
		{
			Match match = shortNumberRegex.Match(part);
			float value = float.Parse(match.Groups[1].Value,
				NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, GetCultureInfo());
			return (long)(match.Groups[2].Value switch
			{
				"K" => value * 1000,
				"J" => value * 1000000,
				"B" => value * 1000000000,
				_ => value
			});
		}
		catch (Exception)
		{
			return -1;
		}
	}

	private CultureInfo GetCultureInfo() => CultureInfo.GetCultureInfoByIetfLanguageTag("af");

	[GeneratedRegex("(\\d{1,2}) (\\w+)? (\\d{4})")]
	private static partial Regex FullDatePatternRegex();

	[GeneratedRegex("([\\d.,]+)\\s?(\\w*)")]
	private static partial Regex ShortNumberRegex();

	[GeneratedRegex("([\\d.]+)")]
	private static partial Regex ViewCountRegex();
}