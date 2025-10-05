import {
  forwardRef,
  useCallback,
  useEffect,
  useImperativeHandle,
  useRef,
} from "react";
import {
  addEdge,
  ReactFlow,
  useEdgesState,
  useNodesState,
  useReactFlow,
} from "@xyflow/react";
import Node from "./components/Node";
import { INode } from "./types";

const nodeTypes = { custom: Node };

type FlowProps = {
  setEdges: (edges: any[]) => void;
  nodeClicked: (node: INode) => void;
};

export type FlowHandle = {
  spawnNode: (
    template: INode,
    id: number,
    pos?: { x: number; y: number }
  ) => string;
};

const Flow = forwardRef<FlowHandle, FlowProps>(function Flow(
  { setEdges, nodeClicked },
  ref
) {
  const [nodes, setNodes, onNodesChange] = useNodesState([]);
  const [edges, _setEdges, onEdgesChange] = useEdgesState([]);

  const { screenToFlowPosition } = useReactFlow();
  const paneRef = useRef<HTMLDivElement | null>(null);

  const onConnect = useCallback(
    (params: any) => _setEdges((eds: any) => addEdge(params, eds)),
    [_setEdges]
  );

  useEffect(() => {
    setEdges(edges);
  }, [edges, setEdges]);

  const centerFlowPos = useCallback(() => {
    const pane = paneRef.current;
    if (!pane) return { x: 0, y: 0 };
    const rect = pane.getBoundingClientRect();
    const centerScreen = {
      x: rect.left + rect.width / 2,
      y: rect.top + rect.height / 2,
    };
    return screenToFlowPosition(centerScreen);
  }, [screenToFlowPosition]);

  useImperativeHandle(ref, () => ({
    spawnNode: (
      template: INode,
      id: number,
      pos?: { x: number; y: number }
    ) => {
      const position = pos ?? centerFlowPos();

      const nodeForData: INode = {
        id,
        name: template.name,
        inputs: template.inputs,
        outputs: template.outputs,
        properties: template.properties
      };

      const newNode = {
        id: String(id),
        type: "custom",
        position,
        data: {
          node: nodeForData,
          nodeClicked,
        },
      };

      setNodes((nds: any) => nds.concat(newNode));
      return String(id);
    },
  }));

  return (
    <div className="flex-1" ref={paneRef}>
      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={nodeTypes}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        fitView
      />
    </div>
  );
});

export default Flow;
