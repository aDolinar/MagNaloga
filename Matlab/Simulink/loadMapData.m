function [nodes, markers, markerPositions, fig]=loadMapData(saveName,savesFolder)
%load file
fileName=savesFolder+saveName;
fileID=fopen(fileName);
mapData=fscanf(fileID,"i%dx%fy%fz%fr%de\n",[5,Inf])';
mapData(:,1)=mapData(:,1)+1;
totalObjects=size(mapData,1);
disp("loaded file: "+fileName);
%object information (dimensions wh, type, plot color)
objectInfo=[2,3,1;
    0.2,2,1;
    1,1,9;
    2,1,9;
    1,1,1;
    3,3,1;
    5,5,1;
    9,9,1;
    7,1,2;
    0.5,0.5,3;
    1,0.2,4;
    3,0.2,4;
    9,0.2,4;
    4,4,5;
    1,0.2,6;
    3,0.2,6;
    9,0.2,6;
    4,4,7;
    2,2,8];
%classify objects
%arrays contain IDs from mapData list
obstacles=[];
gates=[];
lines=[];
lineFollows=[];
markers=[];
boxes=[];
for i=1:totalObjects
    currentObject=mapData(i,:);
    currentObjectType = objectInfo(currentObject(1),3);
    switch currentObjectType
        case 1
            obstacles=[obstacles;i];
        case 2
            gates = [gates;i];
        case 3
            markers=[markers;i];
        case 4
            lines=[lines;i];
        case 5
            lines=[lines;i];
        case 6
            lineFollows=[lineFollows; i];
        case 7
            lineFollows=[lineFollows; i];
        case 8
            robotStart=currentObject;
        case 9
            boxes=[boxes; i];
        otherwise
    end
end
totalMarkers=size(markers,1);
%plot map
%define plotting vars
radius = 3.5;
objectColors=['k','k',"#D95319","#D95319",'k','k','k','k','b','r','y','y','y','y','m','m','m','m','g'];
drawOrder=[4,5,2,1,6,7,8,9,3];
%plot
fig=figure;
hold on
for d=drawOrder
    for i=1:totalObjects
        currentObject=mapData(i,:);
        currentObjectInfo = objectInfo(currentObject(1),:);
        if (currentObjectInfo(3)==d)
            currentObjectRot=-currentObject(5);
            rotMatrix=[cosd(currentObjectRot) -sind(currentObjectRot); sind(currentObjectRot) cosd(currentObjectRot)];
            rotMatrixUnity=[cosd(-currentObjectRot) -sind(-currentObjectRot); sind(-currentObjectRot) cosd(-currentObjectRot)];
            dims=[currentObjectInfo(1),currentObjectInfo(2)];
            dims=dims*rotMatrix;
            dims=abs(dims);
            pos=[currentObject(2)-dims(1)/2,currentObject(4)-dims(2)/2,dims(1),dims(2)];
            if (currentObjectInfo(3)==5 || currentObjectInfo(3)==7) %curved plots
                angle=currentObjectRot+90:1:currentObjectRot+180;
                offsets=[1.5,-1.5]*rotMatrixUnity;
                center=[currentObject(2)+offsets(1),currentObject(4)+offsets(2)];
                xCurve=center(1)+radius*cosd(angle);
                yCurve=center(2)+radius*sind(angle);
                plot(xCurve,yCurve,Color=objectColors(currentObject(1)),LineWidth=2);
            else    %regular rectangles
                if (currentObjectInfo(3)==4||currentObjectInfo(3)==6)
                    edgeColor=objectColors(currentObject(1));
                else
                    edgeColor='k';
                end
                rectangle(FaceColor=objectColors(currentObject(1)),EdgeColor=edgeColor,Position=pos);
            end
        end
    end
end
%enumerate markers
for i=1:totalMarkers
    currentMarker=mapData(markers(i),:);
    text(currentMarker(2),currentMarker(4),num2str(i,"%02d"),"HorizontalAlignment","center","FontWeight","bold","BackgroundColor",'r','FontSize',9);
end
ylabel("z")
xlabel("x")
title("Zemljevid")
hold off
axis equal

%make pathfinding data
nodes=GetPathNodes(saveName);
for i=1:size(nodes,2)
    nodes(i).markerID=markers(nodes(i).tag);
    obj1=mapData(nodes(i).markerID,:);
    nodes(i).x=obj1(2);
    nodes(i).y=obj1(4);
    if (nodes(i).nextL~=-1)
    obj2=mapData(markers(nodes(i).nextL),:);
    nodes(i).weightL=abs(nodes(i).x-obj2(2))+abs(nodes(i).y-obj2(4));
    end
    if (nodes(i).nextM~=-1)
    obj2=mapData(markers(nodes(i).nextM),:);
    nodes(i).weightM=abs(nodes(i).x-obj2(2))+abs(nodes(i).y-obj2(4));
    end
    if (nodes(i).nextR~=-1)
    obj2=mapData(markers(nodes(i).nextR),:);
    nodes(i).weightR=abs(nodes(i).x-obj2(2))+abs(nodes(i).y-obj2(4));
    end
end
%get marker positions
markerPositions=zeros(totalMarkers,2);
for i=1:totalMarkers
    objCurrent=mapData(markers(i),:);
    markerPositions(i,:)=[objCurrent(2) objCurrent(4)];

end
%sort by tag
nodes=table2struct(sortrows(struct2table(nodes),"tag"));