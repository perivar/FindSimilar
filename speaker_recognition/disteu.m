function d = disteu(x, y)
% DISTEU Pairwise Euclidean distances between columns of two matrices
%
% Input:
%       x, y:   Two matrices whose each column is an a vector data.
%
% Output:
%       d:      Element d(i,j) will be the Euclidean distance between two
%               column vectors X(:,i) and Y(:,j)
%
% Note:
%       The Euclidean distance D between two vectors X and Y is:
%       D = sum((x-y).^2).^0.5

[M, N] = size(x);
[M2, P] = size(y); 

if (M ~= M2)
    error('Matrix dimensions do not match.')
end

d = zeros(N, P);

if (N < P)
    copies = zeros(1,P);
    for n = 1:N
        d(n,:) = sum((x(:, n+copies) - y) .^2, 1);
    end
else
    copies = zeros(1,N);
    for p = 1:P
        d(:,p) = sum((x - y(:, p+copies)) .^2, 1)';
    end
end

d = d.^0.5;
