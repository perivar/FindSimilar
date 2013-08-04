function WINDOW = cf_sinebell(W,OVERLAP)
% Creates a sine bell window of size W with OVERLAP

% Author: Cedric Fevotte
% cedric.fevotte@mist-technologies.com

if OVERLAP > fix(W/2)
  disp('OVERLAP cant be greater than half the window size !')
end

WINDOW = zeros(1,W);

WINDOW(1:OVERLAP) = sin((pi*([1:OVERLAP]-0.5))/(2*OVERLAP));
WINDOW(OVERLAP+1:W-OVERLAP) = 1;
WINDOW(W-OVERLAP+1:W) = sin((pi*(W-[W-OVERLAP+1:W]+0.5))/(2*OVERLAP));
