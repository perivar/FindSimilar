
%[s1 fs1] = wavread('train\s1.wav');
[Signal1 Fs1] = wavread('C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!PERIVAR\samples\M1_Piano_C1.wav');

% Time in seconds, for the graphs
t = [0:length(Signal1)-1]/Fs1;

% A hamming window is chosen
winLen = 2048;
winOverlap = winLen/2;
wHamm = hamming(winLen);

% Framing and windowing the signal without for loops.
sigFramed = buffer(Signal1, winLen, winOverlap, 'nodelay');
sigWindowed = diag(sparse(wHamm)) * sigFramed; % tony's trick for windowing the signal
sigFFT = fft(sigWindowed, winLen);
sigFFT2 = sigFFT(1:ceil(size(sigFFT,1)/2),:);

% mfcc
s = Signal1;
n = winLen;
m = winOverlap;
l = length(s);

nbFrame = floor((l - n) / m) + 1;

for i = 1:n
    for j = 1:nbFrame
        M(i, j) = s(((j - 1) * m) + i);
    end
end

h = hamming(n);

M2 = diag(h) * M;

for i = 1:nbFrame
    frame(:,i) = fft(M2(:, i));
end

%frame = frame(1:ceil(size(frame,1)/2),:);

%plot(t, Signal1);
%title('Plot of signal s1.wav');
%xlabel('Time [s]');
%ylabel('Amplitude')

%pause
%close all

%c1 = mfcc(Signal1, Fs1);
%size (c1)
%plot(c1(5, :), c1(6, :));

%Question 5
%disp('> Question 5: Mel Space');
%figure('Position', [0, 0 , 1200, 600]); 
%plot(linspace(0, (fs1/2), 129), (melfb(20, 256, fs1))');
%plot(linspace(0, 22050/2, 1025), melfb(40, 2048, 22050));
%title('Mel-Spaced Filterbank');
%xlabel('Frequency [Hz]');

