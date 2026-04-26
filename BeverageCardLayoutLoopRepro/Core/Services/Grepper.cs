using System.IO.Compression;

namespace Beerbox.App.Core.Services;

public static class Grepper
{
	/// <summary>Finds the line in the given file that starts with the given text.</summary>
	/// <param name="filename">
	/// The path to the file to read.
	/// If the file's extension is ".gz", then it will automatically be processed as a compressed file.
	/// </param>
	/// <param name="startsWith">The text to search for.</param>
	/// <returns>The matching line, or <c>null</c> if no lines match.</returns>
	public static string? FindLine(string filename, string startsWith)
	{
		if (File.Exists(filename))
		{
			using var fileStream = File.OpenRead(filename);
			using var inputStream = Path.GetExtension(filename) switch
			{
				".gz" => (Stream)new GZipStream(fileStream, CompressionMode.Decompress),
				_ => fileStream,
			};
			using var reader = new StreamReader(inputStream);
			string? line;
			while ((line = reader.ReadLine()) != null)
			{
				if (line.StartsWith(startsWith, StringComparison.Ordinal))
				{
					return line;
				}
			}
		}
		return null;
	}
}
