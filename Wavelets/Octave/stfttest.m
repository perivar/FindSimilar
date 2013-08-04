#stfttest.m

sr = 44100;
n = 2048;
hop = n/2;
multiplier = 32768;

x = load('C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b_audiodata.ascii', '-ascii');
xstft = load('C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b_stftdata.ascii', '-ascii');
xistft = load('C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b_audiodata_inverse_stft.ascii', '-ascii');

%wavwrite(xistft, sr, 'C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b-csharp.wav');
%return;

% Calculate the basic STFT

% STFT is computed in the following procedure:
% http://www.originlab.com/www/helponline/Origin/en/UserGuide/Algorithm_(STFT).html	
% N points are taken from the input signal, where N is equal to the window size.
% A window of the chosen type is used to multiply the extracted data, point-by-point.
% Zeros will be padded on both sides of the window, if the window size is less than the size of the FFT section.
% FFT is computed on the FFT section.
% Move the window according to the user-specified overlap size, and repeat steps 1 through 4 until the end of the input signal is reached.

% function D = stft(x, f, w, h, sr)
X = stft(x*multiplier, n, hanning(n)', hop, sr);
%X = stft(x*multiplier, n, 0, n, sr);	% for a 100% recreation we need the phase info (imiginary parts) and no windowing and overlapping

%specgram(A,NFFT,Fs,WINDOW,NOVERLAP)
Z = specgram(x*multiplier, n, sr, hanning(n), hop);

% Invert to a waveform
% function x = istft(d, ftsize, w, h)
%y = istft(X, n, hanning(n)', hop, sr)';
% using abs(X) makes the spike-like artifacts
% using real(X) is OK
% using X is best!

% try to use stft of x read in, but must add another column to cover the requirements for the istft method
B = xstft;
B(hop+1,:) = zeros(1,columns(B)); 
%y = istft(B, n, 0, hop, sr)'; 	% using no window keeps the spike-like artifacts
y = istft(B, n, hanning(n)', hop, sr)'; % using hanning removes the spike-like artifacts but reduces the amplitude quite alot
y = y/multiplier * 5;

%y = istft(X, n, 0, hop, sr)'; 	% perfect recreation except first band which has no transients left after the hanning window
%y = istft(X, n, 0, n, sr)'; 	% for a 100% recreation we need the phase info (imiginary parts) and no windowing and overlapping
%y = y/multiplier;

%invspecgram(B,NFFT,Fs,WINDOW,NOVERLAP);
%z = invspecgram(Z, n, sr, n, hop);
%function x = ispecgram(d, ftsize, sr, win, nov)
% using abs(X) makes the spike-like artifacts
% using real(X) is OK
z = ispecgram(real(X), n, sr, n, hop);
z = z/multiplier;

%wavwrite(Y, FS, FILENAME)
wavwrite(x, sr, 'C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b.wav');
wavwrite(y, sr, 'C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b-istft.wav');
wavwrite(z, sr, 'C:\Users\perivar.nerseth\Documents\My Projects\FindSimilar\bin\Debug\Loreen - Euphoria - Clap 2b-ispec.wav');


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
imagesc(flipud(real(log(Z))));
subplot( 3,2,6 );
plot(z);
