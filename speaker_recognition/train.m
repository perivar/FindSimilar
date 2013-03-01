function code = train(traindir, n)
% Speaker Recognition: Training Stage
%
% Input:
%       traindir : string name of directory contains all train sound files
%       n        : number of train files in traindir
%
% Output:
%       code     : trained VQ codebooks, code{i} for i-th speaker
%
% Note:
%       Sound files in traindir is supposed to be: 
%                       s1.wav, s2.wav, ..., sn.wav
% Example:
%       >> code = train('C:\data\train\', 8);

k = 16;                         % number of centroids required

for i = 1:n                     % train a VQ codebook for each speaker
    file = sprintf('%ss%d.wav', traindir, i);           
    disp(file);
   
    [s, fs] = wavread(file);
    
    v = mfcc(s, fs);            % Compute MFCC's
   
    code{i} = vqlbg(v, k);      % Train VQ codebook
end
