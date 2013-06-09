%%%%%%%%%%%%%%Encoding.m
 
clear all
close all
clc
im=imread('cameraman.tif');
wname='haar';
 
msg='Grasshopper Network';
data=[];
for(i=1:length(msg))
    d=msg(i)+0;
    data=[data d];
end
 
 
  imshow(im);
  [cA1,cH1,cV1,cD1] = dwt2(im,wname);
  dec1 = [cA1     cH1;     
           cV1     cD1      
        ];
 
    figure
    imshow(uint8(dec1));
 
 
 
    M=max(data);
    data_norm=data/M;
    n=length(data);
    [x y]=size(cH1);
    cH1(1,1)=-1*n/10;
    cH1(1,2)=-1*M/10;
 
 
    for(i=1:1:ceil(n/2))
        cV1(i,y)=data_norm(i);
    end
 
 
    for(i=ceil(n/2)+1:1:n)
        cD1(i,y)=data_norm(i);
    end
 
     CODED1=idwt2(cA1,cH1,cV1,cD1,wname);
 
     figure
     imshow(uint8(CODED1))
     [x y]=size(cA1);
      imshow(uint8(CODED1))
 
     ms=abs(CODED1-double(im));
     ms=ms.*ms;
     ms=mean(mean(ms))
     ps= (255*255)/ms;
     ps=10*log10(ps)
    imwrite(uint8(CODED1),'Stego.bmp','bmp'); 
     return
 
%%% END of encoding.m%%%%%%%%%%%%%%%%