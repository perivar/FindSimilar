% ===GNU/Octave Haar Fast Wavelet Transform Code=== 
% The following is a fast implementation for the Haar FWT written for GNU/Octave.
%
function retval = fwt( xi ) 
% Author: Glen Alan MacLachlan, bindatype@gmail.com 
% Copyright 2010, 2012; Distributed under the GPLv3. 
%
%
% Create Basis Vectors
 scaling = [ 1/sqrt(2),1/sqrt(2)];
 wavelet = [ 1/sqrt(2),-1/sqrt(2)];
%
% 
 signal.signal = xi;
 signal.signal_length = length(xi);
 signal.support = length(wavelet);
 signal.scaling = scaling;
 signal.wavelet = wavelet;
%
%
% Loop over all octaves
 for j = 0:log2(signal.signal_length)-1  
   signal.approx = zeros(1,signal.signal_length/2);
   signal.detail = zeros(1,signal.signal_length/2);
%
   signal = dot(signal,j);
   signal = reconstruct(signal,j);
   retval = signal.signal;
%
 end;
endfunction
%
%%%%%%%%%% END MAIN BODY %%%%%%%%%%%%%%
%
%
%
%
%%%%%%%%%% Generate Approx and Detail Coefficients
function retval = dot(signal,j)
% 
% This is the cycle part
    for i = 1:2:signal.signal_length/2^j
% This is the dot part
      for k = 1:signal.support
        Ind_xi = i+k-1;
        Ind_ad = (i-1)/2+1;
        signal.approx( Ind_ad ) += signal.signal( Ind_xi )*signal.scaling(k); 
        signal.detail( Ind_ad ) += signal.signal( Ind_xi )*signal.wavelet(k);   
      end;
    end;
 retval = signal;
 return
endfunction 
%
% 
%%%%%%%%%%% Repack new signal
function retval = reconstruct(signal,j)
%
    for i = 1:signal.signal_length/2^(j+1)
      Ind_ap = i;
      Ind_de = signal.signal_length/2^(j+1)+i;
      signal.signal(Ind_ap) = signal.approx(i);
      signal.signal(Ind_de) = signal.detail(i);
    end; 
 retval = signal;
return
endfunction