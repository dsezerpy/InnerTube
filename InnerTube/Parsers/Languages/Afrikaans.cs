using System.Globalization;
using System.Text.RegularExpressions;

namespace InnerTube.Parsers.Languages;

[ValueParser("af")]
public partial class Afrikaans : IValueParser
{
	private Regex fullDatePattern = FullDatePatternRegex();
	private Regex shortNumberRegex = ShortNumberRegex();
	private Regex viewCountRegex = ViewCountRegex();
	private string[] months = [
			"Jan","Feb","Mrt","Apr",
			"Mei","Jun","Jul","Aug",
			"Sep","Okt","Nov","Des"
		];

	public string ParseRelativeDate(string date)
	{
		string[] parts = date.ToLower().Split(" ");
		string metric = parts[1][..2];
		int amount = int.Parse(parts[0]);
		return metric switch
		{
			"se" => $"-{amount}s",
			"mi" => $"-{amount}m",
			"uu" =>   $"-{amount}h",
			"ur" =>   $"-{amount}h",
			"da" =>    $"-{amount}D",
			"we" =>   $"-{amount}W",
			"ma" =>  $"-{amount}M",
			"ja" =>   $"-{amount}Y",
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

		if (type.Contains("Première was")) return VideoUploadType.Premiered;
		if (type.Contains("begin stroom")) return VideoUploadType.Streaming;
		if (type.Contains("Regstreeks")) return VideoUploadType.Streamed;
		if (type.Contains("Première is")) return VideoUploadType.FuturePremiere;
		if (type.Contains("Geskeduleer")) return VideoUploadType.ScheduledStream;
		return VideoUploadType.Published;
	}

	public long ParseSubscriberCount(string subscriberCountText)
	{
		string digitsPart = subscriberCountText.Split(" ")[0];
		return ParseShortNumber(digitsPart);
	}

	public long ParseLikeCount(string likeCountText) =>
		ParseShortNumber(likeCountText);

	public long ParseViewCount(string viewCountText) => int.Parse(viewCountRegex.Match(viewCountText).Groups[1].Value,
		NumberStyles.AllowThousands, GetCultureInfo());

	public long ParseVideoCount(string videoCountText) =>
		!videoCountText.Contains("Geen video's nie")
			? ParseShortNumber(videoCountText) 
			: 0;

	public DateTimeOffset ParseLastUpdated(string lastUpdatedText) =>
		ParseFullDate(lastUpdatedText.Split("Laas opgedateer op ")[1]);

	private long ParseShortNumber(string part)
	{
		try
		{
			Match match = shortNumberRegex.Match(part);
			float value = float.Parse(match.Groups[1].Value,
				NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, GetCultureInfo());
			return (long)(match.Groups[2].Value switch
			{
				"k" => value * 1000,
				"m" => value * 1000000,
				"mjd" => value * 1000000000,
				_ => value
			});
		}
		catch (Exception)
		{
			return -1;
		}
	}

	private CultureInfo GetCultureInfo() => CultureInfo.GetCultureInfoByIetfLanguageTag("af");

	[GeneratedRegex("(\\d{1,2}) (\\w+)\\.? (\\d{4})")]
	private static partial Regex FullDatePatternRegex();

	[GeneratedRegex("([\\d.,]+)\\s?(\\w*)")]
	private static partial Regex ShortNumberRegex();

	[GeneratedRegex("([\\d. ]+)")]
	private static partial Regex ViewCountRegex();
}