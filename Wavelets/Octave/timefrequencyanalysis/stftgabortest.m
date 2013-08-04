% The code computes the STFT (Gabor transform) with step size = 1
% This is most useful when modifications of the signal is required in
% the frequency domain

% The Gabor transform is a STFT with a Gaussian window (w_t in the code)

% written by Nicholas Kinar

% Reference:
% [1] J. B. Allen and L. R. Rabiner, 
% “A unified approach to short-time Fourier analysis and synthesis,” 
% Proceedings of the IEEE, vol. 65, no. 11, pp. 1558 – 1564, Nov. 1977.

% generate the signal
mm = 8192;                  % signal points
t = linspace(0,1,mm);       % time axis

dt = t(2) - t(1);           % timestep t
wSize = 101;                % window size


% generate time-domain test function
% See pg. 156
% J. S. Walker, A Primer on Wavelets and Their Scientific Applications, 
% 2nd ed., Updated and fully rev. Boca Raton: Chapman & Hall/CRC, 2008.
% http://www.uwec.edu/walkerjs/primer/Ch5extract.pdf
term1 = exp(-400 .* (t - 0.2).^2);
term2 = sin(1024 .* pi .* t);
term3 = exp(-400.*(t- 0.5).^2);
term4 = cos(2048 .* pi .* t);
term5 = exp(-400 .* (t-0.7).^2);
term6 = sin(512.*pi.*t) - cos(3072.*pi.*t);
u = term1.*term2  + term3.*term4 + term5.*term6; % time domain signal
u = u';

figure;
plot(u)

Nmid = (wSize - 1) / 2 + 1;    % midway point in the window
hN = Nmid - 1;                 % number on each side of center point       


% stores the output of the Gabor transform in the frequency domain
% each column is the FFT output
Umat = zeros(wSize, mm);     


% generate the Gaussian window 
% [1] Y. Wang, Seismic inverse Q filtering. Blackwell Pub., 2008.
% pg. 123.
T = dt * hN;                    % half-width
sp = linspace(dt, T, hN); 
targ = [-sp(end:-1:1) 0 sp];    % this is t - tau
term1 = -((2 .* targ) ./ T).^2;
term2 = exp(term1);
term3 = 2 / (T * sqrt(pi));
w_t = term3 .* term2;
wt_sum = sum ( w_t ); % sum of the wavelet


% sliding window code
% NOTE that the beginning and end of the sequence
% are padded with zeros 
for Ntau = 1:mm

    % case #1: pad the beginning with zeros
    if( Ntau <= Nmid )
        diff = Nmid - Ntau;
        u_sub = [zeros(diff,1); u(1:hN+Ntau)];
    end

    % case #2: simply extract the window in the middle
    if (Ntau < mm-hN+1 && Ntau > Nmid)
        u_sub = u(Ntau-hN:Ntau+hN);
    end

    % case #3: less than the end
    if(Ntau >= mm-hN+1)
        diff = mm - Ntau;
        adiff = hN - diff;
        u_sub = [ u(Ntau-hN:Ntau+diff);  zeros(adiff,1)]; 
    end   

    % windowed trace segment
    % multiplication in time domain with
    % Gaussian window  function
    u_tau_omega = u_sub .* w_t';

    % segment in Fourier domain
    % NOTE that this must be padded to prevent
    % circular convolution if some sort of multiplication
    % occurs in the frequency domain
    U = fft( u_tau_omega );

    % make an assignment to each trace
    % in the output matrix
    Umat(:,Ntau) = U;

end

% By here, Umat contains the STFT (Gabor transform)

%figure;
%st = Umat(1:wSize/2+1,:);
%imagesc(flipud(real(log(st))));


% Notice how the Fourier transform is symmetrical 
% (we only need the first N/2+1
% points, but I've plotted the full transform here
figure;
imagesc( (abs(Umat)).^2 )


% now let's try to get back the original signal from the transformed
% signal

% use IFFT on matrix along the cols
us = zeros(wSize,mm);
for i = 1:mm 
    us(:,i) = ifft(Umat(:,i));
end

figure;
%imagesc( us );
imagesc( real(us) );

% create a vector that is the same size as the original signal,
% but allows for the zero padding at the beginning and the end of the time
% domain sequence
Nuu = hN + mm + hN;
uu = zeros(1, Nuu);

% add each one of the windows to each other, progressively shifting the
% sequence forward 
cc = 1; 
for i = 1:mm
   uu(cc:cc+wSize-1) = us(:,i) + uu(cc:cc+wSize-1)';
   cc = cc + 1;
end

% trim the beginning and end of uu 
% NOTE that this could probably be done in a more efficient manner
% but it is easiest to do here

% Divide by the sum of the window 
% see Equation 4.4 of paper by Allen and Rabiner (1977)
% We don't need to divide by L, the FFT transform size since 
% Matlab has already taken care of it 
uu2 = uu(hN+1:end-hN) ./ (wt_sum); 

figure;
plot(uu2)

% Compare the differences bewteen the original and the reconstructed
% signals.  There will be some small difference due to round-off error
% since floating point numbers are not exact
dd = u - uu2';

figure;
plot(dd);