function x = istft(d, ftsize, w, h)
% X = istft(D, F, W, H)                   Inverse short-time Fourier transform.
%	Performs overlap-add resynthesis from the short-time Fourier transform 
%	data in D.  Each column of D is taken as the result of an F-point 
%	fft; each successive frame was offset by H points (default
%	W/2, or F/2 if W==0). Data is hann-windowed at W pts, or 
%       W = 0 gives a rectangular window (default); 
%       W as a vector uses that as window.
%       This version scales the output so the loop gain is 1.0 for
%       either hann-win an-syn with 25% overlap, or hann-win on
%       analysis and rect-win (W=0) on synthesis with 50% overlap.
% dpwe 1994may24.  Uses built-in 'ifft' etc.
% $Header: /home/empire6/dpwe/public_html/resources/matlab/pvoc/RCS/istft.m,v 1.5 2010/08/12 20:39:42 dpwe Exp $

if nargin < 2; ftsize = 2*(size(d,1)-1); end
if nargin < 3; w = 0; end
if nargin < 4; h = 0; end  % will become winlen/2 later

s = size(d);
if s(1) ~= (ftsize/2)+1
  error('number of rows should be fftsize/2+1')
end
cols = s(2);
 
if length(w) == 1
  if w == 0
    % special case: rectangular window
    win = ones(1,ftsize);
  else
    if rem(w, 2) == 0   % force window to be odd-len
      w = w + 1;
    end
    halflen = (w-1)/2;
    halff = ftsize/2;
    halfwin = 0.5 * ( 1 + cos( pi * (0:halflen)/halflen));
    win = zeros(1, ftsize);
    acthalflen = min(halff, halflen);
    win((halff+1):(halff+acthalflen)) = halfwin(1:acthalflen);
    win((halff+1):-1:(halff-acthalflen+2)) = halfwin(1:acthalflen);
    % 2009-01-06: Make stft-istft loop be identity for 25% hop
    win = 2/3*win;
  end
else
  win = w;
end

w = length(win);
% now can set default hop
if h == 0 
  h = floor(w/2);
end

xlen = ftsize + (cols-1)*h;			% calculate length of inverse signal
x = zeros(1,xlen);

% example settings:
% ftsize 	= 2048 		also w = length(win)
% h 		= 1024
% s=size(d)	= 1025, 35 
% cols 		= 35
% h*(cols-1)= 34816
% xlen		= 36864

for b = 0:h:(h*(cols-1))			% for index = start_value : increment_value : end_value
  ft = d(:,1+b/h)';					% take each column (from 1 to cols), e.g. ft is 1025 long
  ft = [ft, conj(ft([((ftsize/2)):-1:2]))];		% ft([9:-1:2]) is a loop (start with index 9, decrement with one and stop at index 2)
												% so the above line extends the array with a inverse duplicate array
												% e.g. [1, 2, 3, 4, 5, 6] => [1, 2, 3, 4, 5, 6, 5, 4, 3, 2, 1]
												% if ft used to be 1025 now it's 2048 long
  px = real(ifft(ft));
  x((b+1):(b+ftsize)) = x((b+1):(b+ftsize))+px.*win;  
end;