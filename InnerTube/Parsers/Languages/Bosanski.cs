using System.Globalization;
using System.Text.RegularExpressions;

namespace InnerTube.Parsers.Languages;

[ValueParser("bs")]
public partial class Bosanski : IValueParser
{
	private Regex fullDatePattern = FullDatePatternRegex();
	private Regex shortNumberRegex = ShortNumberRegex();
	private Regex viewCountRegex = ViewCountRegex();
	private string[] months = [
			"jan","feb","mar","apr",
			"maj","jun","jul","aug",
			"sep","okt","nov","dec"
		];

	public string ParseRelativeDate(string date)
	{
		string[] parts = date.ToLower().Split(" ");
		string metric = parts[1].Remove(parts[1].Length - 1,1);
		int amount = int.Parse(parts[0]);
		// TODO: FIX!!!
		return metric switch
		{
			"sekund" => $"-{amount}s",
			"minut" => $"-{amount}m",
			"sat" =>   $"-{amount}h",
			"dana" =>    $"-{amount}D",
			"sedm" =>   $"-{amount}W",
			"mjesec" =>  $"-{amount}M",
			"godin" =>   $"-{amount}Y",
			_ => $"!Unknown metric;{metric};{amount};{date}"
		};
	}

	public DateTimeOffset ParseFullDate(string date)
	{
		Match match = fullDatePattern.Match(date);
		
		return DateTimeOffset.ParseExact($"{match.Groups[3].Value}/{match.Groups[2].Value}/{match.Groups[1].Value}",
			"yyyy/MMM/d", GetCultureInfo());
	}

	public VideoUploadType ParseVideoUploadType(string type)
	{

		if (type.Contains("prikazan")) return VideoUploadType.Premiered;
		if (type.Contains("Prijenos je")) return VideoUploadType.Streaming;
		if (type.Contains("Datum")) return VideoUploadType.Streamed;
		if (type.Contains("prikazivanje")) return VideoUploadType.FuturePremiere;
		if (type.Contains("Zakazano")) return VideoUploadType.ScheduledStream;
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
		!videoCountText.Contains("videozapisa")
			? ParseShortNumber(videoCountText) 
			: 0;

	public DateTimeOffset ParseLastUpdated(string lastUpdatedText) =>
		ParseFullDate(lastUpdatedText.Split("Zadnje")[1]);

	private long ParseShortNumber(string part)
	{
		try
		{
			Match match = shortNumberRegex.Match(part);
			float value = float.Parse(match.Groups[1].Value,
				NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, GetCultureInfo());
			return (long)(match.Groups[2].Value switch
			{
				"hilj." => value * 1000,
				"mil." => value * 1000000,
				"mlrd." => value * 1000000000,
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