function r = mfcc(s, fs)
% MFCC
%
% Inputs: s  contains the signal to analize
%         fs is the sampling rate of the signal
%
% Output: r contains the transformed signal
%
%
%%%%%%%%%%%%%%%%%%
% Mini-Project: An automatic speaker recognition system
%
% Responsible: Vladan Velisavljevic
% Authors:     Christian Cornaz
%              Urs Hunkeler

m = 100;
n = 256;
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

t = n / 2;
tmax = l / fs;

m = melfb(20, n, fs);
n2 = 1 + floor(n / 2);
z = m * abs(frame(1:n2, :)).^2;

r = dct(log(z));