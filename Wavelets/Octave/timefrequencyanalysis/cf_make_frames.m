function [F_x,xpad] = cf_make_frames(x,AN_WINDOW,OVERLAP)

% decompose some signal x into frames
%
% Usage: [F_x,xpad] = cf_make_frames(x,AN_WINDOW,OVERLAP)
%
% Input:
%   - x: signal of size T,
%   - AN_WINDOW: window of size W,
%   - OVERLAP: number of samples overlap
%
% Output:
%   - F_x is a W x n_frames matrix containing the frames (of length W) in columns,
%   - xpad is the zero-padded signal.

% Author: Cedric Fevotte
% cedric.fevotte@mist-technologies.com

x=x(:).'; % Produces a row signal
AN_WINDOW=AN_WINDOW(:).'; % Produces a row signal

T=length(x);
W=length(AN_WINDOW);

if T < W
    disp('You might want to choose a window smaller than the signals ?')
end

n_frames = ceil((T+OVERLAP)/(W-OVERLAP)); % Number of frames

Tpad = OVERLAP + n_frames*(W-OVERLAP); % Length of zero-padded signal
xpad = [zeros(1,OVERLAP), x, zeros(1,Tpad-T-OVERLAP)];

frames_index = 1 + [0:(n_frames-1)]*(W-OVERLAP); % Index of beginnings of frames

F_x=zeros(W,n_frames);

for n=1:n_frames
  F_x(:,n)=(xpad(frames_index(n)+(0:(W-1))).*AN_WINDOW).';
end