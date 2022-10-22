clc
pause(0.5);
%init comms
port1=25008;
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
t=1;
dataReceived=zeros(1000,19);
dataSent=zeros(1,3);
%reset sim
write(udp1,[10,0,0],"single","127.0.0.1",port1);
pause(0.5);
%communication loop
while(t<500)
    if (udp2.NumBytesAvailable>0)
    data=read(udp2,udp2.NumBytesAvailable,"single");
    size(data)
    end
    t=t+1;
    write(udp1,[0,2,sin(t*0.1)*15],"single","127.0.0.1",port1);
end
%close UDP objects
fclose(udp1);
fclose(udp2);
udp1.delete();
udp2.delete();