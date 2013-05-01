
% Load audio
audio = load('b19-rch plt-p0-Large Plate m_audiodata.ascii.txt','-ascii');

% Get spectrogram
spec = specgram(audio*32768, 2048, 44100, hanning(2048), 1024);

% Absolute values
aspec = abs(spec);
save ('-ascii', 'aspec.ascii.txt', 'aspec');

% Read in the image and make it symmetric.
%aspec = [aspec; flipud(aspec)];
[row, column] = size(aspec);
signal = [];

% Take the ifft of each column of pixels and piece together the results.
for i = 1 : column
	spectrogramWindow = aspec(:, i);
	signalWindow = real(ifft(spectrogramWindow));
	signal = [signal; signalWindow];
end

save ('-ascii', 'signal.ascii.txt', 'signal');
plot(signal);