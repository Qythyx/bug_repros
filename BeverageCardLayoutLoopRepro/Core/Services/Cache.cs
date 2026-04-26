using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Beerbox.Service.Contracts;

namespace Beerbox.App.Core.Services;

public sealed class Cache : IDisposable
{
	/// <summary>
	/// Bumped when the on-disk format changes incompatibly (e.g., Newtonsoft → STJ migration).
	/// Pre-existing caches written under a different filename are ignored on startup.
	/// </summary>
	private const string INDEX_FILENAME = "index.v2.json";

	private readonly string _baseDirectory;
	private readonly SHA256 _hasher = SHA256.Create();
	private readonly Dictionary<string, DateTime> _index;
	private readonly string _indexFile;
	private readonly ReaderWriterLockSlim _indexLock;

	public Cache(string cacheDirectory)
	{
		_baseDirectory = cacheDirectory;
		if (!Directory.Exists(_baseDirectory))
		{
			_ = Directory.CreateDirectory(_baseDirectory);
		}

		PurgeLegacyCache(_baseDirectory);

		_index = [];
		_indexFile = Path.Combine(_baseDirectory, INDEX_FILENAME);
		_indexLock = new(LockRecursionPolicy.SupportsRecursion);

		LoadIndex();
		WriteIndex();
		Empty(CacheState.Expired);
	}

	private static void PurgeLegacyCache(string directory)
	{
		var legacyIndex = Path.Combine(directory, "index.json");
		if (!File.Exists(legacyIndex))
		{
			return;
		}
		foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
		{
			try
			{
				File.Delete(file);
			}
			catch
			{
				// best-effort cleanup; stale files are harmless
			}
		}
	}

	/// <summary>Adds an entry to the cache, overwriting any existing entry.</summary>
	/// <typeparam name="T">The type of the data to cache.</typeparam>
	/// <param name="key">Unique identifier for the entry</param>
	/// <param name="data">Data object to store</param>
	/// <param name="expireIn">Time from UtcNow to expire entry in</param>
	/// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
	public void Add<T>(string key, T data, TimeSpan expireIn)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Key can not be null or empty.", nameof(key));
		}

		_indexLock.EnterWriteLock();

		try
		{
			File.WriteAllText(GetKeyPath(key), JsonSerializer.Serialize(data, SerializerOptions.Default));
			_index[key] = GetExpiration(expireIn);
			WriteIndex();
		}
		finally
		{
			_indexLock.ExitWriteLock();
		}
	}

	/// <summary>
	/// Removes all specified entries regardless if they are expired.
	/// Throws an exception if any deletions fail and rolls back changes.
	/// </summary>
	/// <param name="keys">keys to remove</param>
	public void Remove(params string[] keys)
	{
		_indexLock.EnterWriteLock();

		try
		{
			foreach (var key in keys)
			{
				_ = _index.Remove(key);
				File.Delete(GetKeyPath(key));
			}
			WriteIndex();
		}
		finally
		{
			_indexLock.ExitWriteLock();
		}
	}

	/// <summary>Empties entries in the cache with the given state.</summary>
	/// <param name="state">The cache state to empty.</param>
	public void Empty(CacheState state = CacheState.Expired)
	{
		_indexLock.EnterWriteLock();
		try
		{
			foreach (var key in GetKeys(state).ToArray())
			{
				File.Delete(GetKeyPath(key));
				_ = _index.Remove(key);
			}
			WriteIndex();
		}
		finally
		{
			_indexLock.ExitWriteLock();
		}
	}

	/// <summary>
	/// Checks to see if the key exists in the cache.
	/// </summary>
	/// <param name="key">Unique identifier for the entry to check</param>
	/// <returns>If the key exists</returns>
	public bool Exists(string key) => _index.ContainsKey(key);

	/// <summary>
	/// Gets all the keys that are saved in the cache
	/// </summary>
	/// <param name="state">The cache state to filter by.</param>
	/// <returns>The IEnumerable of keys</returns>
	public IEnumerable<string> GetKeys(CacheState state = CacheState.Active)
	{
		_indexLock.EnterReadLock();

		try
		{
			return (
				state switch
				{
					CacheState.Active => _index.Where(i => i.Value >= DateTime.UtcNow),
					CacheState.Expired => _index.Where(i => i.Value < DateTime.UtcNow),
					_ => _index,
				}
			).Select(i => i.Key);
		}
		finally
		{
			_indexLock.ExitReadLock();
		}
	}

	/// <summary>
	/// Gets the data entry for the specified key.
	/// </summary>
	/// <typeparam name="T">The type of the cached data.</typeparam>
	/// <param name="key">Unique identifier for the entry to get</param>
	/// <param name="result">The data object that was stored if found, else default(T)</param>
	/// <returns><c>true</c> if the key was found and not expired; otherwise <c>false</c>.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
	public bool TryGet<T>(string key, out T? result)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Key can not be null or empty.", nameof(key));
		}

		_indexLock.EnterReadLock();
		result = default;

		try
		{
			var path = GetKeyPath(key);
			if (_index.TryGetValue(key, out var expiration) && expiration > DateTime.UtcNow && File.Exists(path))
			{
				result = JsonSerializer.Deserialize<T>(File.ReadAllText(path), SerializerOptions.Default);
				return true;
			}
		}
		catch
		{
			// bad cache
		}
		finally
		{
			_indexLock.ExitReadLock();
		}
		return false;
	}

	private void WriteIndex() =>
		File.WriteAllText(_indexFile, JsonSerializer.Serialize(_index, SerializerOptions.Default));

	private void LoadIndex()
	{
		try
		{
			_index.Clear();
			foreach (
				var item in JsonSerializer.Deserialize<Dictionary<string, DateTime>>(
					File.ReadAllText(_indexFile),
					SerializerOptions.Default
				)!
			)
			{
				_index.Add(item.Key, item.Value);
			}
		}
		catch
		{
			File.Delete(_indexFile);
		}
	}

	public string GetKeyPath(string key) =>
		Path.ChangeExtension(
			Path.Combine(_baseDirectory, Convert.ToHexString(_hasher.ComputeHash(Encoding.Default.GetBytes(key)))),
			"json"
		);

	private static DateTime GetExpiration(TimeSpan timeSpan)
	{
		try
		{
			return DateTime.UtcNow.Add(timeSpan);
		}
		catch
		{
			return timeSpan.TotalSeconds < 0 ? DateTime.MinValue : DateTime.MaxValue;
		}
	}

	public void Dispose()
	{
		_indexLock.Dispose();
		_hasher.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <summary>Current state of the item in the cache.</summary>
	[Flags]
	public enum CacheState
	{
		/// <summary>Represents all cache states.</summary>
		All = 0,

		/// <summary>Expired cache item.</summary>
		Expired = 1,

		/// <summary>Active non-expired cache item.</summary>
		Active = 1 << 1,
	}
}
