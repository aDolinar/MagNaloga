clc
clear all
close all
%EDIT THESE TWO VARIABLES TO ADJUST THE AMOUNT OF
%FLOATS BEING RECEIVED/SENT IN UDP
num_floats_to_receive=21;    %default 21, needs change in Unity as well
num_floats_to_send=3;       %default 3, needs change in Unity as well
%EDIT TO CHANGE SIMULINK MODEL FREQUENCY
model_frequency=200;    %in Hz
%EDIT TO CHANGE LINEFOLLOW RAY DISTANCES
rayDistance=0.075;       %in Unity units, needs change in Unity as well
%VELOCITY AND ANGULAR VELOCITY LIMITS IN SIMULINK
velLim=[-2,5];
steerLim=[-40,40];
%ignore these
receive_data_length=num_floats_to_receive*4;
send_data_length=num_floats_to_send*4;
%load map data
savesFolder="C:\UnityProjects\MagistrskaNaloga\MagNaloga\MobileRobotSimulationBuilt\MobileRobotSimulation_Data\Saves\";
saveName="example2.txt";
[nodes, markers, markerPositions,fig] = loadMapData(saveName,savesFolder);

%start up the model
launchChoice=2; %walaunchChoice=1  lineFollowing, launchChoice=2 pathFinding A*,launchChoice=3 moveToPoint, %launchChoice=other udpTemplate
startTag=29;    %configure to change pathfinding start
endTag=11;      %configure to change pathfinding end
switch launchChoice
    case 1
        system("UDPMain_LineFollow.slx");
    case 2
        pathActions=Astar(startTag,endTag,nodes);
        disp("A* path generated");
        system("UDPMain_Pathfinding.slx");
        addAstarLinesToPlot(pathActions,markerPositions,fig);
    case 3
        system("UDPMain_MoveToPoint.slx");
    case 4
        system("UDPMain_MoveToPointPrimitiveAvoidance.slx");
    case 5
        pathActions=Astar(startTag,endTag,nodes);
        disp("A* path generated");
        system("UDPMain_Pathfinding_withPrimitiveAvoidance.slx")
    case 6
        system("UDPMain_MoveToPointPrimitiveAvoidance2.slx")
    otherwise
        system("UDPtemplate.slx");
end