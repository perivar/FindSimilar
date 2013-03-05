
[s1 fs1] = wavread('train\s1.wav');

%Question 5
disp('> Question 5: Mel Space');
figure('Position', [0, 0 , 1200, 600]); 
%plot(linspace(0, (fs1/2), 129), (melfb(20, 256, fs1))');
plot(linspace(0, 22050/2, 1025), melfb(40, 2048, 22050));
%plot(linspace(0, (fs1/2), 1025), (melfb(40, 2048, fs1))');
title('Mel-Spaced Filterbank');
xlabel('Frequency [Hz]');

