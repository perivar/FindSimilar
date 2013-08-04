function [xr,win_pos] =  istft (X,hopfac,winlen,type)
% xr =  istft (X,hopfac,winlen)
% this function calculate the inverse STFT for a STFT matrix
% X - STFT matrix (bins 1:nfft/2+1)
% hopfac - hop factor. This is an integer specifying the number of analysis hops
% occurring within a signal frame that is one window in length. In other words,
% winlen/hopfac is the hop size in saamples and winlen*(1-1/hopfac) is the overlap
% in samples.
% winlen - window length in samples
% type - 'perfect' or 'smooth'
% (c) Shlomo Dubnov sdubnov@ucsd.edu

X = [X; conj(X(end-1:-1:2,:))];

if nargin < 2,
    hopfac = 2;
end
if nargin < 3,
    winlen = size(X,1);
end
if nargin < 4,
    type = 'perfect';
end

hop = winlen/hopfac;
bmat = ifft(X);
%STFT = real(ifft(X));

[M N] = size(bmat);
nfft = M;

xr = zeros (1,N*hop + nfft);
win_pos = [1: hop: length(xr) - nfft];
if type == 'perfect',
    win = ones(winlen,1);
elseif type == 'smooth';
    win = hanning(winlen,'periodic'); %second smoothing window
else
    error('no such istft type')
end


for i=1:length(win_pos)
    xr(win_pos(i):win_pos(i)+nfft-1) = xr(win_pos(i):win_pos(i)+nfft-1) + bmat(:,i)'.*win';
end

%xr = xr / nfft;
xr = real(xr)/hopfac*2;

