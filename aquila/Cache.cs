using System;
using System.Collections.Generic;

/**
 * @file Cache.cpp
 *
 * A generic cache class - implementation
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.6.0
 * @since 2.6.0
 */
namespace Aquila
{
	/**
	 * A generic calculation cache.
	 *
	 * The cache class holds results of possibly expensive calculations, for
	 * example intermediate values in FFT transform. The values (which can
	 * be scalars, C-style arrays, vectors etc.) are identified by a cache
	 * key of type Cache::KeyType.
	 *
	 * Cache object constructor call needs to be supplied with a generator
	 * function. It can be a standalone function or a static class method
	 * which takes a const reference to a cache key type and returns a value
	 * of the cache value type. For example, if the cache stores integer
	 * values identified by a string, the generator signature should match
	 * int generate(const string& key).
	 */
	public class Cache <K, V>
	{
		public const int AQUILA_CACHE_STATS = 0;
		internal static int s_cacheHits = 0;
		internal static int s_cacheLookups = 0;
		
		/**
		 * Generator function pointer type.
		 */
		public delegate V GeneratorFunction(K NamelessParameter);

		/**
		 * A pointer to generator function.
		 */
		private GeneratorFunction generator;

		/**
		 * Internal representation of the cache as a key-value map.
		 */
		private Dictionary<K, V> m_map = new Dictionary<K, V>();

		/**
		 * Creates the cache and sets the pointer to a generator function.
		 *
		 * @param fn a pointer to a generator function or static class method
		 */
		public Cache(GeneratorFunction fn)
		{
			generator = new GeneratorFunction(fn);
		}

		/**
		 * Clears the cache.
		 *
		 * If AQUILA_CACHE_STATS is defined, displays a small cache lookup stats
		 * to stdout.
		 */
		public void Dispose()
		{
			#if AQUILA_CACHE_STATS
			Console.Write("[Aquila::Cache] hits/lookups: ");
			Console.Write(GlobalMembersCache.s_cacheHits);
			Console.Write("/");
			Console.Write(GlobalMembersCache.s_cacheLookups);
			Console.Write(", hit rate: ");
			Console.Write(100 * GlobalMembersCache.s_cacheHits / (double)(GlobalMembersCache.s_cacheLookups));
			Console.Write("%\n");
			#endif
		}

		/**
		 * Returns a value associated with the given key.
		 *
		 * If the value is in cache, it is returned immediately. In other case,
		 * the generator function is called with the key as the argument and
		 * its return value is stored in cache and returned to the caller.
		 *
		 * @param key cache key
		 * @return cache value
		 */
		public V get(K key)
		{
			#if AQUILA_CACHE_STATS
			GlobalMembersCache.s_cacheLookups++;
			#endif
			if (m_map.ContainsKey(key))
			{
				#if AQUILA_CACHE_STATS
				GlobalMembersCache.s_cacheHits++;
				#endif
				return m_map[key];
			}

			V result = generator(key);
			m_map.Add(key, result);
			return result;
		}
	}
}
