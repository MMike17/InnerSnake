using System;

/// <summary>Extends the TimeSpan class</summary>
public static class TimeSpanExtention
{
	public static string ToNiceString(this TimeSpan span)
	{
		string result = span.ToString(@"mm\:ss\:ms");
		string[] frag = result.Split(':');

		if (frag[frag.Length - 1].Length > 2)
		{
			result = result.Substring(0, result.Length - (frag[frag.Length - 1].Length - 2));
		}

		return result;
	}
}