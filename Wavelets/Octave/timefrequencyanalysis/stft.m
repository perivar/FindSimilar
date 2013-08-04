function x = stft(sig,fftlength,winlength,tr)

% STFT(input signal, fftlength, window length, time resolution)
%
% This routine can use hamming, rectangular or any other
% type of analysis window if this code is modified.

sig = sig';
win = hamming(winlength)';
ls = length(sig);
lw = length(win);
lm = lw;

i = 1;		% block counter and index
 while lw < ls,	% throws away the last unfilled block
  blocki = sig(1,lw-lm+1:lw).*win;
  x(i,1:fftlength)=fft(blocki,fftlength);
  lw = lw + tr;
  i = i + 1;
 end

return


a = abs(x);
a = a';
a = a/max(max(a));
a = 1-a;
a = a*128;
colormap(gray(128))
image(a);
