function actionSet=Astar(startTag,endTag,nodes)
%start and end nodes
%startTag=29;
%endTag=8;
%init
clear openNodes;
clear closedNodes;
endNode=nodes(endTag,:);
startNode=nodes(startTag,:);
startNode.Hcost=GetNodeDistance(startNode,endNode);
startNode.Fcost=startNode.Gcost+startNode.Hcost;
openNodes(1)=startNode;
closedNodes=[];
%iterate until pathfound
while(size(openNodes,2)~=0)
    %get openNode with lowest Fcost
    lowestFcost=999999999999999;
    lowestFIdx=1;
    for i=1:size(openNodes,2)
        if (openNodes(i).Fcost<lowestFcost)
            lowestFcost=openNodes(i).Fcost;
            lowestFIdx=i;
        end
    end
    currentNode=openNodes(lowestFIdx);
    %if not end tag
    if (currentNode.tag~=endTag)
        %handle possible left next
        if (currentNode.nextL~=-1)
            nextNode=nodes(currentNode.nextL);
            nextNode.Gcost=currentNode.Gcost+currentNode.weightL;
            nextNode.Hcost=GetNodeDistance(nextNode,endNode);
            nextNode.Fcost=nextNode.Gcost+nextNode.Hcost;
            nextNode.from=currentNode.tag;
            openNodes=[openNodes,nextNode];
        end
        %handle possible middle next
        if (currentNode.nextM~=-1)
            nextNode=nodes(currentNode.nextM);
            nextNode.Gcost=currentNode.Gcost+currentNode.weightM;
            nextNode.Hcost=GetNodeDistance(nextNode,endNode);
            nextNode.Fcost=nextNode.Gcost+nextNode.Hcost;
            nextNode.from=currentNode.tag;
            openNodes=[openNodes,nextNode];
        end
        %handle possible right next
        if (currentNode.nextR~=-1)
            nextNode=nodes(currentNode.nextR);
            nextNode.Gcost=currentNode.Gcost+currentNode.weightR;
            nextNode.Hcost=GetNodeDistance(nextNode,endNode);
            nextNode.Fcost=nextNode.Gcost+nextNode.Hcost;
            nextNode.from=currentNode.tag;
            openNodes=[openNodes,nextNode];
        end
    end

    openNodes=openNodes([1:lowestFIdx-1 lowestFIdx+1:end]);
    closedNodes=[closedNodes, currentNode];
    if (currentNode.tag==endTag)
        break;
    end
end
%backtrace the path
clear pathBackwards
pathBackwards(1)=[currentNode];
searchableClosed=squeeze(cell2mat(struct2cell(closedNodes)))';
fromIdx=find(searchableClosed(:,2)==currentNode.from);
fromNode=closedNodes(fromIdx(1));
pathBackwards=[pathBackwards, fromNode];
while (fromNode.from ~=-1)

    fromIdx=find(searchableClosed(:,2)==fromNode.from);
    fromNode=closedNodes(fromIdx(1));
    pathBackwards=[pathBackwards, fromNode];
end
%pack info into instructions
actionSet=[];
totalActions=size(pathBackwards,2);
for i=1:totalActions
    currentNode=pathBackwards(totalActions-i+1);
    instruction=-1;
    if (i<totalActions)
        nextNode=pathBackwards(totalActions-i);
        switch nextNode.tag
            case currentNode.nextL
                instruction=1;
            case currentNode.nextM
                instruction=2;
            case currentNode.nextR
                instruction=3;
            otherwise
                instruction=-1;
        end
    end
    actionSet=[actionSet;currentNode.tag,instruction];
end
end