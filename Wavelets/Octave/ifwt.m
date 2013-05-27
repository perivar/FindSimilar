% Authors: Glen Alan MacLachlan, Shihao Guo
% Copyright 2010, Distributed under the GPLv3.
%
  function answer = ifwt( xi )
%
  signal_length = length(xi);
%
  if ( floor( log2( signal_length ) ) != log2(signal_length) )
    printf("Bad Signal Length\n");
    break;
  endif;
% 
  temp = xi;
%
  scaling = [ 1/sqrt(2),1/sqrt(2)];
  wavelet = [ 1/sqrt(2),-1/sqrt(2)];
% 
  filter_length = length(wavelet);
%
  for j = log2(signal_length)-1:-1:0
    approx = zeros(1,signal_length/2);
    detail = zeros(1,signal_length/2);
%
    for i = 1:signal_length/2^(j+1)
      L = signal_length/2^(j+1);
      xi(2*(i-1)+1) = temp(i);
      xi(2*i) = temp(i+L);
    end; 
%
  temp = xi;
%
  for i = 1:signal_length/2^(j+1)
    approx(i)= (temp(2*(i-1)+1)+temp(2*(i-1)+2))/sqrt(2);
    detail(i)= (temp(2*(i-1)+1)-temp(2*(i-1)+2))/sqrt(2);
  end;
%
  for i = 1:(signal_length/2^(j+1))
    xi(2*(i-1)+1) = approx(i);
    xi(2*i)       = detail(i); 
  end;
%
  temp = xi;
%
  end;
  answer = xi; 
 endfunction