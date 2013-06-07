T = 1; % threshold value
v = linspace(-5,5,1024);
clf;
hold('on');
plot(v, perform_thresholding(v,T,'hard'), 'b--');
plot(v, perform_thresholding(v,T,'soft'), 'r--');
plot(v, perform_thresholding(v,[T 2*T],'semisoft'), 'g');
plot(v, perform_thresholding(v,[T 4*T],'semisoft'), 'g:');
plot(v, perform_thresholding(v',400,'strict'), 'r:');
legend('hard', 'soft', 'semisoft, \mu=2', 'semisoft, \mu=4', 'strict, 400');
hold('off');