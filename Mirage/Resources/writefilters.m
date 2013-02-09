function writefilters(samplingrate, winsize, numfilters, nummfccs, dctfile, filterweights)
% Precompute the MFCC filterweights and DCT.
% Adopted from Malcolm Slaneys mfcc.m, August 1993
% Mirage uses filters computed by the following command in octave 3.0
%     as of August 26th, 2008
%     writefilters(22050, 1024, 36, 20, 'dct.filter', 'filterweights.filter');
%
% see http://www.ee.columbia.edu/~dpwe/muscontent/practical/mfcc.m

fft_freq = linspace(0, samplingrate/2, winsize/2 + 1);
f = 20:(samplingrate/2);
mel = log(1 + f/700) * 1127.01048;
m_idx = linspace(1, max(mel), numfilters+2);
for i = 1:numfilters+2
    [tmp f_idx(i)] = min(abs(mel - m_idx(i)));
end

freqs = f(f_idx);
lo = freqs(1:numfilters);
ce = freqs(2:numfilters+1);
up = freqs(3:numfilters+2);

% filters outside of spectrum
[tmp idx] = max(find(ce <= samplingrate/2));
numfilters = min(idx, numfilters);

mfcc_filterweights = zeros(numfilters, winsize/2 + 1);
triangleh = 2./(up-lo);

for i = 1:numfilters
    mfcc_filterweights(i,:) =...
        (fft_freq > lo(i) & fft_freq <= ce(i)).*...
            triangleh(i).*(fft_freq-lo(i))/(ce(i)-lo(i)) +...
        (fft_freq > ce(i) & fft_freq < up(i)).*...
            triangleh(i).*(up(i)-fft_freq)/(up(i)-ce(i));
end

dct = 1/sqrt(numfilters/2) * cos((0:(nummfccs-1))' *...
    (2*(0:(numfilters-1))+1) * pi/2/numfilters);
dct(1,:) = dct(1,:) * sqrt(2)/2;

dct_f = fopen(dctfile, 'w');
fwrite(dct_f, size(dct, 1), 'int32');
fwrite(dct_f, size(dct, 2), 'int32');
fwrite(dct_f, dct', 'float32');
fclose(dct_f);

filterweights_f = fopen(filterweights, 'w');
fwrite(filterweights_f, size(mfcc_filterweights, 1), 'int32');
fwrite(filterweights_f, size(mfcc_filterweights, 2), 'int32');
fwrite(filterweights_f, mfcc_filterweights', 'float32');
fclose(filterweights_f);

% debugging
%dct
%mfcc_filterweights

size(dct)

size(mfcc_filterweights)
