import { useCallback, useEffect, useState } from "react";
import {
  addEdge,
  ReactFlow,
  ReactFlowProvider,
  useEdgesState,
  useNodesState,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import "./App.css";
import Node from "./components/Node";

const nodeTypes = { custom: Node };

export default function App() {
  const [nodes, setNodes] = useState<Node[]>([]);

  useEffect(() => {
    fetch("http://localhost:5020/nodes").then((e) => {
      e.json().then((r) => {
        console.log(r.nodes);
      });
    });
  }, []);

  return (
    <div style={{ width: "100%", height: "100%" }}>
      <ReactFlowProvider>
        <Flow props={{ nodes: nodes }} />
      </ReactFlowProvider>
    </div>
  );
}
type FlowProps = { nodes: Node[] };

function Flow({ props }: { props: FlowProps }) {
  const [nodes, setNodes, onNodesChange] = useNodesState([
    {
      id: "1",
      type: "input",
      data: { label: "Input" },
      position: { x: 0, y: 60 },
    },
    {
      id: "2",
      type: "custom",
      data: { label: "Middle with outputs" },
      position: { x: 250, y: 40 },
    },
    {
      id: "3",
      type: "output",
      data: { label: "Output A" },
      position: { x: 560, y: 10 },
    },
    {
      id: "4",
      type: "output",
      data: { label: "Output B" },
      position: { x: 560, y: 90 },
    },
    {
      id: "5",
      type: "output",
      data: { label: "Output C", outputCount: 1 },
      position: { x: 560, y: 170 },
    },
    {
      id: "6",
      type: "custom",
      data: { label: "4242", outputCount: 5, inputCount: 5 },
      position: { x: 250, y: 40 },
    },
  ]);

  const [edges, setEdges, onEdgesChange] = useEdgesState([
    { id: "e1-2", source: "1", target: "2" },
    { id: "e2a-3", source: "2", sourceHandle: "out1", target: "3" },
    { id: "e2b-4", source: "2", sourceHandle: "out2", target: "4" },
    { id: "e2c-5", source: "2", sourceHandle: "out3", target: "5" },
  ]);

  const onConnect = useCallback(
    (params: any) => setEdges((eds) => addEdge(params, eds)),
    [setEdges]
  );

  return (
    <ReactFlow
      nodes={nodes}
      edges={edges}
      nodeTypes={nodeTypes}
      onNodesChange={onNodesChange}
      onEdgesChange={onEdgesChange}
      onConnect={onConnect}
      fitView
    />
  );
}
