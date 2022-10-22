function nodes=GetPathNodes(saveName)

%example2
if (strcmp(saveName,"example2.txt"))
nodeData=[29,24,-1,-1;24,-1,23,-1;23,-1,-1,22;22,26,-1,25;25,-1,-1,24;26,-1,8,-1;8,9,-1,7;9,-1,10,-1;7,-1,2,-1;10,12,-1,-1;2,1,-1,-1;1,-1,11,-1;11,-1,-1,12;12,-1,6,-1;6,5,-1,4;4,-1,3,-1;3,-1,-1,1;5,-1,28,-1;28,-1,13,-1;13,14,-1,20;14,-1,15,-1;15,17,-1,16;17,-1,18,-1;16,-1,19,-1;19,20,-1,-1;20,-1,27,-1;27,-1,21,-1;21,22,-1,-1;18,-1,-1,-1];
end

totalNodes=size(nodeData,1);

for ii=1:totalNodes
    currentNode=nodeData(ii,:);
    nodes(ii).ID=ii;
    nodes(ii).tag=currentNode(1);
    nodes(ii).nextL=currentNode(2);
    nodes(ii).nextM=currentNode(3);
    nodes(ii).nextR=currentNode(4);
    nodes(ii).weightL=999999;
    nodes(ii).weightM=999999;
    nodes(ii).weightR=999999;
    nodes(ii).Gcost=0;
    nodes(ii).Hcost=0;
    nodes(ii).Fcost=0;
    nodes(ii).markerID=0;
    nodes(ii).x=0;
    nodes(ii).y=0;
    nodes(ii).from=-1;
end