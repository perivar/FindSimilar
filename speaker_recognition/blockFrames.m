function M3 = blockFrames(s, fs, m, n)
% blockFrames: Puts the signal into frames
%
% Inputs: s  contains the signal to analize
%         fs is the sampling rate of the signal
%         m  is the distance between the beginnings of two frames
%         n  is the number of samples per frame
%
% Output: M3 is a matrix containing all the frames
%
%
%%%%%%%%%%%%%%%%%%
% Mini-Project: An automatic speaker recognition system
%
% Responsible: Vladan Velisavljevic
% Authors:     Christian Cornaz
%              Urs Hunkeler
    l = length(s);
    nbFrame = floor((l - n) / m) + 1;
    
    for i = 1:n
        for j = 1:nbFrame
            M(i, j) = s(((j - 1) * m) + i);
        end
    end
    
    h = hamming(n);
    M2 = diag(h) * M;
    
    for i = 1:nbFrame
        M3(:, i) = fft(M2(:, i));
    end
    