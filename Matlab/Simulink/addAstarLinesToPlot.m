function addAstarLinesToPlot(actionSet,markerPos,fig)
len=size(actionSet,1);
fig;
hold on
for ii=2:len
    quiver(markerPos(actionSet(ii-1,1),1),markerPos(actionSet(ii-1,1),2),markerPos(actionSet(ii,1),1)-markerPos(actionSet(ii-1,1),1),markerPos(actionSet(ii,1),2)-markerPos(actionSet(ii-1,1),2),"off",'lineWidth',7,'Color','k');
    %quiver(markerPos(actionSet(ii-1,1),1),markerPos(actionSet(ii-1,1),2),markerPos(actionSet(ii,1),1)-markerPos(actionSet(ii-1,1),1),markerPos(actionSet(ii,1),2)-markerPos(actionSet(ii-1,1),2),"off",'lineWidth',4,'Color',[1-ii/(len), 1,0]);
    col=[1 1 1];
    switch actionSet(ii-1,2)
        case 1
        col=[1 0.2 0.2];
        case 3
        col=[0.2 0.2 1];
    end
    quiver(markerPos(actionSet(ii-1,1),1),markerPos(actionSet(ii-1,1),2),markerPos(actionSet(ii,1),1)-markerPos(actionSet(ii-1,1),1),markerPos(actionSet(ii,1),2)-markerPos(actionSet(ii-1,1),2),"off",'lineWidth',4,'Color',col);
end
hold off