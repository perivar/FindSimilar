function w = hanning(n,type)
% w = hanning(n,type)
% hanning window of size n
% type can be 'symmetric' (default) or 'periodic'

if nargin < 2,
    type = 'symmetric';
end


switch type,
    case 'periodic'
        % Includes the first zero sample
        w = [0; sym_hanning(n-1)];
    case 'symmetric'
        % Does not include the first and last zero sample
        w = sym_hanning(n);
end


function w = sym_hanning(n)

if ~rem(n,2)
    % Even length window
    m = n/2;
    w = .5*(1 - cos(2*pi*(1:m)'/(n+1)));
    w = [w; w(end:-1:1)];
else
    % Odd length window
    m = (n+1)/2;
    w = .5*(1 - cos(2*pi*(1:m)'/(n+1)));
    w = [w; w(end-1:-1:1)];
end
