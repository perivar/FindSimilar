
d1 = dtw_mfcc_distance('samples/named/02-pullover-2.wav',...
                  'samples/named/02-pullover-3.wav',...
                  true);


figure;

d2 = dtw_mfcc_distance('samples/named/02-pullover-2.wav',...
                  'samples/named/04-blumentopf-3.wav',...
                  true);
				  
d1
d2