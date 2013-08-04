function [data]=invspecgram(B,NFFT,Fs,WINDOW,NOVERLAP);
% INVSPECGRAM Performs the Inverse Short Time FFT (Inverse SPECGRAM).
%                                                                                                                         
% Usage: [data] = INVSPECGRAM(B,NFFT,Fs,WINDOW,NOVERLAP);
% 
% B is the STFT data; NFFT is the number of points for which the FFT was
% calculated per slice.
% Fs is the sampling frequency of the original data. WINDOW is the window
% length in samples; NOVERLAP is the number of samples which each FFT slide
% overlap with each other.
%
% See also SPECGRAM, FFT and IFFT.

stepsize = WINDOW - NOVERLAP;
[a,b]=size(B);
transB=zeros(size(b,a));
ispecgram = zeros((((stepsize*b)+a)),1);
B = ifft(B,NFFT);
transB = B';
counter3 = 1;
for counter2= 1:1:b    
    ispecgram(counter3:(((floor(a/2))-1)+counter3),1) = (transB(counter2,1:(floor(a/2)))') + ispecgram(counter3:(((floor(a/2))-1)+counter3),1);
    counter3 = counter3 + stepsize;
end
NX =(b*(WINDOW-NOVERLAP))+NOVERLAP;
data =  real(ispecgram) .* 2;
data = data(1:NX);