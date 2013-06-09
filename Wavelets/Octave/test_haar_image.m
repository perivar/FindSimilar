% Init
clear all
close all
clc

% Set output number format
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

% Calculate min and max
image_min = min(image(:));
image_max = max(image(:));
printf('Image original (''image''), min: %f and max: %f\n', image_min, image_max);

% Normalize the pixel values to the range 0..1 It does this by dividing all pixel values by the max value.
image_normalized = image/max(image(:));

% Write to delimited file
dlmwrite('image_normalized.csv', image_normalized, ';'); % works with Norwegian Excel
printf('Wrote ''image_normalized.csv''\n');

% Calculate min and max
min_normalized = min(image_normalized(:));
max_normalized = max(image_normalized(:));
printf('Image normalized (''image_normalized''), min: %f and max: %f\n', min_normalized, max_normalized);

% 2D Haar Wavelet Transform
haar_image_normalized = haar_2d(image_normalized);

% Write to delimited file
dlmwrite('haar_image_normalized.csv', haar_image_normalized, ';'); % works with Norwegian Excel
printf('Wrote ''haar_image_normalized.csv''\n');

% 2D Inverse Haar Wavelet Transform
inverse_haar_image_normalized = haar_2d_inverse(haar_image_normalized);

% Calculate min and max
min_inverse_normalized = min(inverse_haar_image_normalized(:));
max_inverse_normalized = max(inverse_haar_image_normalized(:));
printf('Inverse Haar Image (''inverse_haar_image_normalized''), min: %f and max: %f\n', min_inverse_normalized, max_inverse_normalized);

% Display
% subplot (nrows,ncols,plot_number)
clf;
subplot(2,2,1);
imagesc(image_normalized);
title('original normalized image');
colormap (gray);
axis image off;
set(gcf,'position', get(0,'screensize'));

subplot(2,2,2);
imagesc(uint8(haar_image_normalized*5000));
title('haar transformed image');
colormap (gray);
axis image off;
set(gcf,'position', get(0,'screensize'));

subplot(2,2,3);
imagesc(inverse_haar_image_normalized);
title('inverse haar transformed image');
colormap (gray);
axis image off;
set(gcf,'position', get(0,'screensize'));

% Test if original is equal to inverse
tf = abs((image_normalized-inverse_haar_image_normalized)./inverse_haar_image_normalized)<0.001;
similar = all(tf(:));
if (similar)
	printf('Success! ''image_normalized'' is equal to ''inverse_haar_image_normalized''\n');
end
