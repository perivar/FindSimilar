using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// http://blogs.msdn.com/b/spt/archive/2008/06/11/locality-sensitive-hashing-lsh-and-min-hash.aspx
namespace SetSimilarity
{
	class LSH<T>
	{
		int m_numHashFunctions;
		int m_numBands;
		Dictionary<int, HashSet<int>> m_lshBuckets = new Dictionary<int, HashSet<int>>();
		HashSet<T>[] m_sets;
		int[,] m_minHashMatrix;
		const int ROWSINBAND = 5;

		//first index is Set, second index contains hashValue (so index is hash function)
		public LSH(int[,] minHashMatrix, HashSet<T>[] sets)
		{
			m_numHashFunctions = minHashMatrix.Length/sets.Length;
			m_numBands=  m_numHashFunctions / ROWSINBAND;
			m_sets = sets;
			m_minHashMatrix = minHashMatrix;

			for (int s = 0; s < sets.Length; s++)
			{
				for (int b = 0; b < m_numBands; b++)
				{
					//combine all 5 MH values and then hash get its hashcode
					//need not be sum
					int sum = 0;

					for (int i = 0; i < ROWSINBAND; i++)
					{
						sum += minHashMatrix[s, b*ROWSINBAND+i];
					}

					if(m_lshBuckets.ContainsKey(sum))
					{
						m_lshBuckets[sum].Add(s);
					}
					else
					{
						var set = new HashSet<int>();
						set.Add(s);
						m_lshBuckets.Add(sum, set);
					}
				}
			}
		}

		public int FindClosest(int setIndex, MinHash minHasher)
		{
			//First find potential "close" candidates
			HashSet<int> potentialSetIndexes = new HashSet<int>();

			for (int b = 0; b < m_numBands; b++)
			{
				//combine all 5 MH values and then hash get its hashcode
				int sum = 0;

				for (int i = 0; i < ROWSINBAND; i++)
				{
					sum += m_minHashMatrix[setIndex, b * ROWSINBAND + i];
				}

				foreach (var i in m_lshBuckets[sum])
				{
					potentialSetIndexes.Add(i);
				}
			}

			//From the candidates compute similarity using min-hash and find the index of the closet set
			int minIndex = -1;
			double similarityOfMinIndex = 0.0;
			foreach (int candidateIndex in potentialSetIndexes.Where(i => i != setIndex))
			{
				// TODO: FIX this
				//double similarity = minHasher.ComputeSimilarity(m_minHashMatrix, setIndex, candidateIndex);
				//double similarity = minHasher.Similarity(m_minHashMatrix, setIndex, candidateIndex);
				double similarity = 0.0;
				if (similarity > similarityOfMinIndex)
				{
					similarityOfMinIndex = similarity;
					minIndex = candidateIndex;
				}
			}

			return minIndex;
		}
	}
}