using System;
using System.Drawing;
using System.Windows.Forms;

using System.IO; // For FileInfo Class
using System.Collections.Generic; // For List Class

using Mirage; // For Analyzer Class

using Soundfingerprinting;
using Soundfingerprinting.Fingerprinting;
using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;

using Soundfingerprinting.Hashing;
using Soundfingerprinting.DbStorage; // For DatabaseService
using Soundfingerprinting.Image; // For ImageService
using Soundfingerprinting.Fingerprinting.Configuration; // For Configuration

using System.Drawing.Drawing2D; // For InterpolationMode

using System.Linq; // For List's FirstOrDefault methods

namespace FindSimilar
{
	/// <summary>
	/// Compare two audio files
	/// </summary>
	public partial class CompareAudioForm : Form
	{
		// Soundfingerprinting
		private DatabaseService databaseService = null;
		private Repository repository = null;
		
		public CompareAudioForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
			
			// Instansiate Soundfingerprinting Repository
			FingerprintService fingerprintService = Analyzer.GetSoundfingerprintingService();
			this.databaseService = DatabaseService.Instance;

			IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms.csv", ",");
			//IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms-new.csv", ",");
			
			IFingerprintingConfiguration fingerprintingConfigCreation = new FullFrequencyFingerprintingConfiguration();
			repository = new Repository(permutations, databaseService, fingerprintService);
			ImageService imageService = new ImageService(fingerprintService.SpectrumService, fingerprintService.WaveletService);
			
			FileInfo filePathAudio1 = new FileInfo(@"C:\Users\perivar.nerseth\Music\Test Samples Database\VDUB1 Snare 004.wav");
			FileInfo filePathAudio2 = new FileInfo(@"C:\Users\perivar.nerseth\Music\Test Samples Search\VDUB1 Snare 004 - Start.wav");
			
			int fingerprintsPerRow = 2;

			double[][] logSpectrogram1 = null;
			double[][] logSpectrogram2 = null;
			List<bool[]> fingerprints1 = null;
			List<bool[]> fingerprints2 = null;
			
			WorkUnitParameterObject file1Param = Analyzer.GetWorkUnitParameterObjectFromAudioFile(filePathAudio1);
			if (file1Param != null) {
				file1Param.FingerprintingConfiguration = fingerprintingConfigCreation;
				
				// Get fingerprints
				fingerprints1 = fingerprintService.CreateFingerprintsFromAudioSamples(file1Param.AudioSamples, file1Param, out logSpectrogram1);
				
				pictureBox1.Image = imageService.GetSpectrogramImage(logSpectrogram1, logSpectrogram1.Length, logSpectrogram1[0].Length);
				pictureBoxWithInterpolationMode1.Image = imageService.GetImageForFingerprints(fingerprints1, file1Param.FingerprintingConfiguration.FingerprintLength, file1Param.FingerprintingConfiguration.LogBins, fingerprintsPerRow);
			}

			WorkUnitParameterObject file2Param = Analyzer.GetWorkUnitParameterObjectFromAudioFile(filePathAudio2);
			if (file2Param != null) {
				file2Param.FingerprintingConfiguration = fingerprintingConfigCreation;
				
				// Get fingerprints
				fingerprints2 = fingerprintService.CreateFingerprintsFromAudioSamples(file2Param.AudioSamples, file2Param, out logSpectrogram2);
				
				pictureBox2.Image = imageService.GetSpectrogramImage(logSpectrogram2, logSpectrogram2.Length, logSpectrogram2[0].Length);
				pictureBoxWithInterpolationMode2.Image = imageService.GetImageForFingerprints(fingerprints2, file2Param.FingerprintingConfiguration.FingerprintLength, file2Param.FingerprintingConfiguration.LogBins, fingerprintsPerRow);
			}
			

			MinHash minHash = repository.MinHash;
			
			// only use the first signatures
			bool[] signature1 = fingerprints1[0];
			bool[] signature2 = fingerprints2[0];

			if (signature1 != null && signature2 != null) {
				int hammingDistance = MinHash.CalculateHammingDistance(signature1, signature2);
				double jaqSimilarity = MinHash.CalculateJaqSimilarity(signature1, signature2);
				
				lblSimilarity.Text = String.Format("Hamming: {0} JAQ: {1}", hammingDistance, jaqSimilarity);
			}
		}

	}
}
