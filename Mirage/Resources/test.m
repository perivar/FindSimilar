
dctfile = 'dct2.filter';
filterweights = 'filterweights2.filter';

writefilters(22050, 2048, 40, 20, dctfile, filterweights);
%writefilters(22050, 1024, 36, 20, dctfile, filterweights);

dct_f = fopen(dctfile, 'r');
dct_nr = fread(dct_f, 1, 'int32');
dct_nc = fread(dct_f, 1, 'int32');
dct2 = fread(dct_f, [dct_nr, dct_nc], 'float32');
fclose(dct_f);

filterweights_f = fopen(filterweights, 'r');
filterweights_nr = fread(filterweights_f, 1, 'int32');
filterweights_nc = fread(filterweights_f, 1, 'int32');
filterweights2 = fread(filterweights_f, [filterweights_nr, filterweights_nc], 'float32');
fclose(filterweights_f);

size(dct2)
size(filterweights2)

%figure('Position', [0, 0 , 1200, 600]); 
%plot(dct2);
%print(strcat(dctfile,'.png'),'-dpng');

%plot(filterweights2);
%print(strcat(filterweights,'.png'),'-dpng');
