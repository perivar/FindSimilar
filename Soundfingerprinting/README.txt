http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting
https://github.com/AddictedCS/soundfingerprinting

Query fingerprint:
QueryFingerprintManager.cs

Create fingerprint:

Repository.cs:
        /// <summary>
        ///   Create fingerprints out of down sampled samples
        /// </summary>
        /// <param name = "samples">Down sampled to 5512 samples </param>
        /// <param name = "track">Track</param>
        /// <param name = "stride">Stride</param>
        /// <param name = "hashTables">Number of hash tables</param>
        /// <param name = "hashKeys">Number of hash keys</param>
        public void CreateInsertFingerprints(
            float[] samples,
            Track track,
            IStride stride,
            int hashTables,
            int hashKeys) {
			
            /*Create fingerprints that will be used as initial fingerprints to be queried*/
            List<bool[]> fingerprints = workUnitBuilder.BuildWorkUnit()
                                                       .On(samples)
                                                       .WithCustomConfiguration(config => config.Stride = stride)
                                                       .GetFingerprintsUsingService(service)
                                                       .Result;

            storage.InsertTrack(track); /*Insert track into the storage*/
            /*Get signature's hash signature, and associate it to a specific track*/
            List<HashSignature> creationalsignatures = GetSignatures(fingerprints, track, hashTables, hashKeys);
            foreach (HashSignature hash in creationalsignatures)
            {
                storage.InsertHash(hash, HashType.Creational);
                /*Set this hashes as also the query hashes*/
                storage.InsertHash(hash, HashType.Query);
            }			
		}
			
        private List<HashSignature> GetSignatures(IEnumerable<bool[]> fingerprints, Track track, int hashTables, int hashKeys)
        {
            List<HashSignature> signatures = new List<HashSignature>();
            foreach (bool[] fingerprint in fingerprints)
            {
                int[] signature = hasher.ComputeMinHashSignature(fingerprint); /*Compute min-hash signature out of signature*/
                Dictionary<int, long> buckets = hasher.GroupMinHashToLSHBuckets(signature, hashTables, hashKeys); /*Group Min-Hash signature into LSH buckets*/
                int[] hashSignature = new int[buckets.Count];
                foreach (KeyValuePair<int, long> bucket in buckets)
                {
                    hashSignature[bucket.Key] = (int)bucket.Value;
                }

                HashSignature hash = new HashSignature(track, hashSignature); /*associate track to hash-signature*/
                signatures.Add(hash);
            }
            return signatures; /*Return the signatures*/
        }
						
MinHash.cs:
        /// <summary>
        ///   Compute Min Hash signature of a fingerprint
        /// </summary>
        /// <param name = "fingerprint">Signature</param>
        /// <returns>MinHashes [concatenated of size PERMUTATION SIZE]</returns>
        /// <remarks>
        ///   The basic idea in the Min Hashing scheme is to randomly permute the rows and for each 
        ///   column c(i) compute its hash value h(c(i)) as the index of the first row under the permutation that has a 1 in that column.
        /// </remarks>
        public int[] ComputeMinHashSignature(bool[] fingerprint)
        {
            bool[] signature = fingerprint;
            int[] minHash = new int[permutations.Length]; /*100*/
            for (int i = 0; i < permutations.Length /*100*/; i++)
            {
                minHash[i] = 255; /*The probability of occurrence of 1 after position 255 is very insignificant*/
                int len = permutations[i].Length;
                for (int j = 0; j < len /*256*/; j++)
                {
                    if (signature[permutations[i][j]])
                    {
                        minHash[i] = j; /*Looking for first occurrence of '1'*/
                        break;
                    }
                }
            }
            return minHash; /*Array of 100 elements with bit turned ON if permutation captured successfully a TRUE bit*/
        }

		/// <summary>
        ///   Compute LSH hash buckets which will be inserted into hash tables.
        ///   Each fingerprint will have a candidate in each of the hash tables.
        /// </summary>
        /// <param name = "minHashes">Min Hashes gathered from every fingerprint [N = 100]</param>
        /// <param name = "numberOfHashTables">Number of hash tables [L = 25]</param>
        /// <param name = "numberOfMinHashesPerKey">Number of min hashes per key [N = 4]</param>
        /// <returns>Collection of Pairs with Key = Hash table index, Value = Hash bucket</returns>
        public Dictionary<int, long> GroupMinHashToLSHBuckets(int[] minHashes, int numberOfHashTables, int numberOfMinHashesPerKey)
        {
            Dictionary<int, long> result = new Dictionary<int, long>();
            const int MaxNumber = 8; /*Int64 biggest value for MinHash*/
            if (numberOfMinHashesPerKey > MaxNumber)
            {
                throw new ArgumentException("numberOfMinHashesPerKey cannot be bigger than 8");
            }

            for (int i = 0; i < numberOfHashTables /*hash functions*/; i++)
            {
                byte[] array = new byte[MaxNumber];
                for (int j = 0; j < numberOfMinHashesPerKey /*r min hash signatures*/; j++)
                {
                    array[j] = (byte)minHashes[(i * numberOfMinHashesPerKey) + j];
                }
                
                long hashbucket = BitConverter.ToInt64(array, 0); // actual value of the signature
                // hashbucket = ((A * hashbucket + B) % PrimeP) % HashBucketSize;
                result.Add(i, hashbucket);
            }

            return result;
        }
	
		public static int CalculateHammingDistance(bool[] a, bool[] b)
		public static int CalculateHammingDistance(long a, long b)
		
        /// <summary>
        ///   Calculate similarity between 2 fingerprints.
        /// </summary>
        /// <param name = "x">Signature x</param>
        /// <param name = "y">Signature y</param>
        /// <returns></returns>
        /// <remarks>
        ///   Similarity defined as  (A intersection B)/(A union B)
        ///   for types of columns a (1,1), b(1,0), c(0,1) and d(0,0), it will be equal to
        ///   Sim(x,y) = a/(a+b+c)
        ///   +1 = 10
        ///   -1 = 01
        ///   0 = 00
        /// </remarks>
        public static double CalculateJaqSimilarity(bool[] x, bool[] y)		
		
FingerprintService.cs:
        private List<bool[]> CreateFingerprintsFromAudioSamples(float[] samples, WorkUnitParameterObject param)
        {
            IFingerprintingConfiguration configuration = param.FingerprintingConfiguration;
            AudioServiceConfiguration audioServiceConfiguration = new AudioServiceConfiguration
                {
                    LogBins = configuration.LogBins,
                    LogBase = configuration.LogBase,
                    MaxFrequency = configuration.MaxFrequency,
                    MinFrequency = configuration.MinFrequency,
                    Overlap = configuration.Overlap,
                    SampleRate = configuration.SampleRate,
                    WdftSize = configuration.WdftSize,
                    NormalizeSignal = configuration.NormalizeSignal,
                    UseDynamicLogBase = configuration.UseDynamicLogBase
                };

            float[][] spectrum = audioService.CreateLogSpectrogram(
                samples, configuration.WindowFunction, audioServiceConfiguration);

            return this.CreateFingerprintsFromLogSpectrum(
                spectrum,
                configuration.Stride,
                configuration.FingerprintLength,
                configuration.Overlap,
                configuration.TopWavelets);
        }

        private List<bool[]> CreateFingerprintsFromLogSpectrum(
            float[][] logarithmizedSpectrum, IStride stride, int fingerprintLength, int overlap, int topWavelets)
        {
            List<float[][]> spectralImages = spectrumService.CutLogarithmizedSpectrum(
                logarithmizedSpectrum, stride, fingerprintLength, overlap);

            waveletService.ApplyWaveletTransformInPlace(spectralImages);
            List<bool[]> fingerprints = new List<bool[]>();

            foreach (var spectralImage in spectralImages)
            {
                bool[] image = fingerprintDescriptor.ExtractTopWavelets(spectralImage, topWavelets);
                fingerprints.Add(image);
            }

            return fingerprints;
        }
		
SpectrumService.cs:
        public List<float[][]> CutLogarithmizedSpectrum(
            float[][] logarithmizedSpectrum, IStride strideBetweenConsecutiveImages, int fingerprintImageLength, int overlap)
			
		private float[][] AllocateMemoryForFingerprintImage(int fingerprintLength, int logBins)
		
		
