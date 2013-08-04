function STFT = cf_stft(x,AN_WINDOW,OVERLAP)

% STFT of a real signal x
%
% Usage: STFT = cf_stft(x,AN_WINDOW,OVERLAP)
%
% Input:
%   - x: signal of size T,
%   - AN_WINDOW: analysis window of size W,   
%   - OVERLAP: number of samples overlap
%
% Output:
%   - STFT is a (W/2+1 if W even) or (W/2+1/2 if W odd) x n_frames matrix.
%
% If AN_WINDOW is an integer, a sine bell window of size AN_WINDOW
% is used by default.
%
% Author: Cedric Fevotte
% fevotte@tsi.enst.fr

% Default window
if length(AN_WINDOW) == 1;
  AN_WINDOW = cf_sinebell(AN_WINDOW,OVERLAP);
end

W=length(AN_WINDOW);
T=length(x);

x=x(:).'; % Produces a row signal
AN_WINDOW=AN_WINDOW(:).';

F_x = cf_make_frames(x,AN_WINDOW,OVERLAP);

STFT = fft(F_x);

% Keep half of the spectrum
if rem(W,2)==0
  STFT = STFT(1:W/2+1,:);
else
  STFT = STFT(1:W/2+1/2,:);
end