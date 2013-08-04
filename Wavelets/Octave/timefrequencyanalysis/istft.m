function x = istft(sig,fftlength,winlength,tr,orlength)

% ISTFT(input signal, fftlength, window length, time resolution, original length)

%
% This routine can use hamming, rectangular or any other
% type of analysis window if this code is modified.

win = hamming(winlength)';
[ls,lss] = size(sig);

% Calculate W(0) 
w0 = sum(win);

% Estimate original length of x[n]
ol = tr*ls+lss;

% Initialize vectors
x = zeros(1,ol);
res = zeros(1,ol);

L=0;
for i = 1:ls,
 blocki = sig(i,:);
 blocki = ifft(blocki,lss);
 res(1,1+L:L+lss) = blocki;
 x = x+res;
 L = L + tr;
 res = zeros(1,ol);
end

x = x.*(tr/w0);

xl = length(x);
x(orlength+1:xl) = [];

