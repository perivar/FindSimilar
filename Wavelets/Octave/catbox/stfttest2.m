#stfttest.m

sr = 44100;
n = 2048;
hopfac = 2;
hop = n/hopfac;
multiplier = 32768;

x = load('C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b_audiodata.ascii', '-ascii');
xstft = load('C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b_stftdata.ascii.txt', '-ascii');

% Calculate the STFT
% stft (a,win,overlap,nfft)
[X,win_pos] = stft(x*multiplier, n, hop, n);

% Invert to a waveform
% istft (X,hopfac,winlen,type)
y = istft(X, hopfac, n, 'perfect')';
y = y/multiplier;

%wavwrite(Y, FS, FILENAME)
wavwrite(x, sr, 'C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b.wav');
wavwrite(y, sr, 'C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b-istft.wav');


% Plot full screen
screen_size = get(0, 'ScreenSize');

% This will return a 4 element array: (left, bottom, width, height);
% The ones we are interested are the "width" and "height".
f1 = figure(1);
set(f1, 'Position', [0 0 screen_size(3) screen_size(4) ] );

subplot( 2,2,1 );
imagesc (flipud(log(xstft)));
subplot( 2,2,2 );
imagesc(flipud(real(log(X))));
subplot( 2,2,3 );
plot (x);
subplot( 2,2,4 );
plot(y);

