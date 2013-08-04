#stfttest.m

sr = 44100;
n = 2048;
hopfac = 2;
hop = n/hopfac;
multiplier = 32768;

x = load('C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b_audiodata.ascii', '-ascii');
xstft = load('C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b_stftdata.ascii.txt', '-ascii');

% Calculate the STFT
% STFT(input signal, fftlength, window length, time resolution)
X = stft(x*multiplier, n, n, n);

% STFT = cf_stft(x,AN_WINDOW,OVERLAP)
S = cf_stft(x*multiplier, hanning(n), hop);

% Invert to a waveform
% ISTFT(input signal, fftlength,  window length, time resolution, original length)
y = istft(real(X), n, n, n, length(x));
y = y/multiplier;

% xpad = cf_istft(STFT,SY_WINDOW,OVERLAP)
s = cf_istft(real(S), hanning(n), hop);
s = s/multiplier;

% Plot full screen
screen_size = get(0, 'ScreenSize');

% This will return a 4 element array: (left, bottom, width, height);
% The ones we are interested are the "width" and "height".
f1 = figure(1);
set(f1, 'Position', [0 0 screen_size(3) screen_size(4) ] );

subplot( 3,2,1 );
imagesc (flipud(log(xstft)));
subplot( 3,2,2 );
plot(x);
subplot( 3,2,3 );
imagesc(flipud(real(log(X))));
subplot( 3,2,4 );
plot(y);
subplot( 3,2,5 );
imagesc(flipud(real(log(S))));
subplot( 3,2,6 );
plot(s);


