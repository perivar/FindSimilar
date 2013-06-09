% Init
clear all
close all
clc

% One technique for denoising is wavelet thresholding (or "shrinkage"). 
% When we decompose data using the wavelet transform, we use filters that act as averaging filters, 
% and others that produce details. 
% Some of the resulting wavelet coefficients correspond to details in the data set (high frequency sub-bands). 
% If the details are small, they might be omitted without substantially affecting the main features of the data set. 
% The idea of thresholding is to set all high frequency sub-band coefficients that are less than a particular threshold to zero. 
% These coefficients are used in an inverse wavelet transformation to reconstruct the data set

format short g; % set console to output human readable numbers

% Read image
image = imread('C:\Users\perivar.nerseth\Pictures\lena_color.jpg');
image = double(image); 	% convert to double precision			

% if image has color, use only one of the rgb color's
rgbImage = image;

% Extract the individual red, green, and blue color planes.
redPlane = rgbImage(:, :, 1);
greenPlane = rgbImage(:, :, 2);
bluePlane = rgbImage(:, :, 3);

% Use Blue plane
image = bluePlane;

% Normalize the pixel values to the range 0..1.0. It does this by dividing all pixel values by the max value.
image_normalized = image/max(image(:));

% Add Noise
image_noisy = image_normalized + 0.1 * randn(size(image_normalized));
%image_noisy = image_normalized;

% 2D Haar Wavelet Transform
image_haar = haar_2d(image_noisy);

% Soft Thresholding sets coefficients with values less than the threshold(T) to 0, then substracts T from the non-zero coefficients.
%T = 0.3;
%y = max(abs(image_haar) - T, 0);
%y = y./(y+T) .* image_haar;

% Perform Thresholding
% type is either 'hard' or 'soft' or 'semisoft' or 'strict'
yHard = perform_thresholding(image_haar, 0.15, 'hard');
ySoft = perform_thresholding(image_haar, 0.15, 'soft');
ySemisoft = perform_thresholding(image_haar, 0.15, 'semisoft');
yStrict = perform_thresholding(image_haar, 20, 'strict');

% Inverse 2D Haar Wavelet Transform
zHard = haar_2d_inverse(yHard);
zSoft = haar_2d_inverse(ySoft);
zSemisoft = haar_2d_inverse(ySemisoft);
zStrict = haar_2d_inverse(yStrict);

% Display
% subplot (nrows,ncols,plot_number)
clf;
subplot(3,2,1);
imagesc(image_normalized);
title('original image');
colormap (gray);
axis image off;
set(gcf,'position', get(0,'screensize'));

subplot(3,2,2);
imagesc(image_noisy);
title('noisy image');
colormap ( gray(256) );
axis image off;
set(gcf,'position', get(0,'screensize'));

subplot(3,2,3);
imagesc(zHard);
title('denoised image (hard)');
colormap ( gray(256) );
axis image off;
set(gcf,'position', get(0,'screensize'));

subplot(3,2,4);
imagesc(zSoft);
title('denoised image (soft)');
colormap ( gray(256) );
axis image off;
set(gcf,'position', get(0,'screensize'));

subplot(3,2,5);
imagesc(zSemisoft);
title('denoised image (semisoft)');
colormap ( gray(256) );
axis image off;
set(gcf,'position', get(0,'screensize'));

subplot(3,2,6);
imagesc(zStrict);
title('denoised image (strict)');
colormap ( gray(256) );
axis image off;
set(gcf,'position', get(0,'screensize'));
