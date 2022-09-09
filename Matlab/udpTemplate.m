clc
%init comms
port1=25008;
port2 = 25106;
udp1=udpport;
try
udp2=udpport("LocalPort",25106);
catch
udp2.delete();
udp2=udpport("LocalPort",25106);
end
t=0;
dataReceived=zeros(1,19);
dataSent=zeros(1,3);
%communication loop
while(t<1000)
    udp2.NumBytesAvailable
    if (udp2.NumBytesAvailable>0)
    data=read(udp2,udp2.NumBytesAvailable,"single");
    end
    t=t+0.1;
    write(udp1,[0,2,sin(t)*15],"single","127.0.0.1",port1);
end
fclose(udp1);
fclose(udp2);