clc
pause(0.5);
%init parameters
incomingDataCount=19;
incomingDataSize=incomingDataCount*4;
%init comms
port1 = 25008;
port2 = 25106;
try
udp1=udpport;
catch
    udp1.delete();
    udp1=udpport;
end
try
udp2=udpport("LocalPort",25106);
catch
udp2.delete();
udp2=udpport("LocalPort",25106);
end
%init vars
%dataReceived=zeros(1,19);
dataSent=zeros(1,3);
%reset sim
write(udp1,[10,0,0],"single","127.0.0.1",port1);
pause(0.5);
%bind comms to callbacks
configureCallback(udp2,"terminator",@onUDPreceive);
for t=1:100000
    pause(0.01);
end
%close UDP objects
fclose(udp1);
fclose(udp2);
udp1.delete();
udp2.delete();