disp(' ');
[s1 fs1] = wavread('train\s1.wav');

%Question 5
disp('> Question 5: Mel Space');
plot(linspace(0, (fs1/2), 129), (melfb(20, 256, fs1))');
title('Mel-Spaced Filterbank');
xlabel('Frequency [Hz]');

pause
close all