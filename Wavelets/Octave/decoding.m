%%%%% Decoding.m%%%%%%%%%%%%%%%%%
im=imread('Stego.bmp');
[cA11,cH11,cV11,cD11] = dwt2(CODED1,wname);
data=[]
data_norm=[];
n=ceil(abs(cH11(1,1)*10));
M=ceil(abs(cH11(1,2)*10));
 
 
    for(i=1:1:ceil(n/2))
        data_norm(i)=cV11(i,y);
    end
 
 
    for(i=ceil(n/2)+1:1:n)
        data_norm(i)=cD11(i,y);
    end
    data=ceil(data_norm*M)-1;
    msg='';
    for(i=1:length(data))
        msg=strcat(msg,data(i));
    end
 
    msg
%%End of Decoding.m