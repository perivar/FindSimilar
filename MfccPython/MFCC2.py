from numpy import *
from numpy.linalg import *
from scipy.fftpack import dct
from scipy.io import wavfile

def melFilterBank(blockSize):
    numBands = int(numCoefficients)
    maxMel = int(freqToMel(maxHz))
    minMel = int(freqToMel(minHz))

    # Create a matrix for triangular filters, one row per filter
    filterMatrix = zeros((numBands, blockSize))

    melRange = array(xrange(numBands + 2))

    melCenterFilters = melRange * (maxMel - minMel) / (numBands + 1) + minMel

    # each array index represent the center of each triangular filter
    aux = log(1 + 1000.0 / 700.0) / 1000.0
    aux = (exp(melCenterFilters * aux) - 1) / 22050
    aux = 0.5 + 700 * blockSize * aux
    aux = floor(aux)  # Arredonda pra baixo
    centerIndex = array(aux, int)  # Get int values

    for i in xrange(numBands):
        start, centre, end = centerIndex[i:i + 3]
        k1 = float32(centre - start)
        k2 = float32(end - centre)
        up = (array(xrange(start, centre)) - start) / k1
        down = (end - array(xrange(centre, end))) / k2

        filterMatrix[i][start:centre] = up
        filterMatrix[i][centre:end] = down

    return filterMatrix.transpose()

def freqToMel(freq):
    return 1127.01048 * math.log(1 + freq / 700.0)

def melToFreq(mel):
    return 700 * (math.exp(freq / 1127.01048 - 1))


sampleRate, signal = wavfile.read("C:/Users/perivar.nerseth/Documents/My Projects/aquila-read-only/examples/test.wav")
numCoefficients = 40 # choose the sice of mfcc array
minHz = 0
maxHz = 22.000

complexSpectrum = fft.fft(signal)
powerSpectrum = abs(complexSpectrum) ** 2
filteredSpectrum = dot(powerSpectrum, melFilterBank(40))
logSpectrum = fft.log(filteredSpectrum)
dctSpectrum = dct(logSpectrum, type=2)  # MFCC :)

