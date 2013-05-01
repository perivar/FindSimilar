
% http://stackoverflow.com/questions/1230906/reverse-spectrogram-a-la-aphex-twin-in-matlab
% According to Dave Gamble, this is the solution
% spectrogramWindow = image(:, i);
% spectrogramWindow = [spectrogramWindow;reverse(spectrogramWindow(skip first and last))]
% signalWindow = ifft(spectrogramWindow);
% signal = [signal; signalWindow];

% Load audio
audio = load('b19-rch plt-p0-Large Plate m_audiodata.ascii.txt','-ascii');

% Get spectrogram
spec = specgram(audio*32768, 2048, 44100, hanning(2048), 1024);

% Absolute values
absspec = abs(spec);
save ('-ascii', 'absspec.ascii.txt', 'absspec');

% Read in the image and make it symmetric.
%absspec = [absspec; flipud(absspec)];
[row, column] = size(absspec);
signal = [];

% Take the ifft of each column of pixels and piece together the results.
for i = 1 : column
	spectrogramWindow = absspec(:, i);
	signalWindow = real(ifft(spectrogramWindow));
	signal = [signal; signalWindow];
end

save ('-ascii', 'signal.ascii.txt', 'signal');
plot(signal);