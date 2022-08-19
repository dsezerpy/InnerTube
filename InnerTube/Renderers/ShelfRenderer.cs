﻿using System.Text;
using Newtonsoft.Json.Linq;

namespace InnerTube.Renderers;

public class ShelfRenderer : IRenderer
{
	public string Type { get; }
	
	public string Title { get; }
	public int CollapsedItemCount { get; }
	public IEnumerable<IRenderer> Items { get; }

	
	public ShelfRenderer(JToken renderer)
	{
		Type = renderer.Path.Split(".").Last();
		Title = renderer.GetFromJsonPath<string>("title.simpleText")!;
		CollapsedItemCount = renderer.GetFromJsonPath<int>("content.verticalListRenderer.collapsedItemCount")!;
		Items = Utils.ParseRenderers(renderer.GetFromJsonPath<JArray>("content.verticalListRenderer.items")!);
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder()
			.AppendLine($"[{Type}] {Title}")
			.AppendLine($"- CollapsedItemCount: {CollapsedItemCount}");
		
		foreach (IRenderer renderer in Items)
		{
			sb.AppendLine(string.Join('\n',
				renderer.ToString()?.Split('\n').Select(x => $"\t{x}") ?? Array.Empty<string>()));
		}

		return sb.ToString();
	}
}