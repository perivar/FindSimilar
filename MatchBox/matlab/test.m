
[d,sr] = wavread('samples/named/02-pullover-2.wav');

% Calculate MFCCs using mfcc.m from the Auditory Toolbox
% (gain should be 2^15 because melfcc scales by that amount, 
% but in this case mfcc uses 2x FFT len)
[ceps,freqresp,fb,fbrecon,freqrecon] = mfcc(d*(2^14), sr);

% Scale them to match (log_10 and power)
ceps = log(10)*2*ceps; 

%imagesc(ceps(2:13,:)); axis xy; colorbar
%title('Auditory Toolbox MFCC');

imagesc(fb); axis xy; colorbar
title('Filterbank');

#{
d1 = dtw_mfcc_distance('samples/named/02-pullover-2.wav',...
                  'samples/named/02-pullover-3.wav',...
                  true);


figure;

d2 = dtw_mfcc_distance('samples/named/02-pullover-2.wav',...
                  'samples/named/04-blumentopf-3.wav',...
                  true);
				  
d1
d2

#}