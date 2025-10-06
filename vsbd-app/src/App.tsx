import { useEffect, useMemo, useRef, useState } from "react";
import { ReactFlowProvider } from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import "./App.css";
import { ActionNode, INode, NodeConnection } from "./types";
import Flow from "./Flow";
import * as signalR from "@microsoft/signalr";

type FlowHandle = {
  spawnNode: (
    template: INode,
    id: number,
    pos?: { x: number; y: number }
  ) => string;
};

export default function App() {
  const [palette, setPalette] = useState<INode[]>([]);
  const [edges, setEdges] = useState<any[]>([]);
  const [spawnedNodes, setSpawnedNodes] = useState<INode[]>([]);
  const [isRunning, setIsRunning] = useState<boolean>(false);
  const [selectedId, setSelectedId] = useState<number | undefined>(undefined);
  const selected = useMemo(
    () => spawnedNodes.find((n) => n.id === selectedId),
    [spawnedNodes, selectedId]
  );

  const nextIdRef = useRef(1);
  const flowRef = useRef<FlowHandle | null>(null);
  const connection = useMemo(() => {
    return new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5020/flow")
      .withAutomaticReconnect()
      .build();
  }, []);

  useEffect(() => {
    fetch("http://localhost:5020/nodes").then(async (r) => {
      const data = await r.json();
      const templates = (data.nodes as INode[]).map((t) => ({
        ...t,
        id: undefined as unknown as number,
      }));
      setPalette(templates);
    });
  }, []);

  const handleStart = async () => {
    if (connection.state == signalR.HubConnectionState.Disconnected) {
      await connection.start();
      setIsRunning(false);
    }

    if (!isRunning) {
      const connections: NodeConnection[] = (edges || []).map((e) => ({
        to: Number(e.target),
        toType: e.targetHandle,
        from: Number(e.source),
        fromType: e.sourceHandle,
      }));

      const actionNodes: ActionNode[] = spawnedNodes.map((e) => {
        return {
          id: e.id,
          name: e.name,
          properties: e.properties,
        } as ActionNode;
      });

      console.log({
        connections: connections,
        nodes: actionNodes,
      });
      const response = await connection.invoke("StartFlow", {
        connections: connections,
        nodes: actionNodes,
      });

      if (response) {
        setIsRunning(true);
      }
    } else {
      const response = await connection.invoke<boolean>("StopFlow");
      if (response) {
        setIsRunning(false);
      }
    }
  };

  const handleCompileNodes = async () => {
    const response = await fetch("http://localhost:5020/compile", {
      method: "POST",
    });
    if (response.ok) {
      console.log("ok");
    }
  };

  const handleSpawnNode = (template: INode) => {
    const id = nextIdRef.current++;
    const newNode: INode = {
      ...template,
      id,
      properties: template.properties?.map((p) => ({ ...p })) ?? [],
    };
    flowRef.current?.spawnNode(newNode, id);
    setSpawnedNodes((prev) => [...prev, newNode]);
  };

  const handleNodeClick = (node: INode) => {
    setSelectedId(node.id);
  };

  return (
    <div className="w-full h-full flex">
      <div className="h-full w-72 shrink-0 bg-zinc-900 text-zinc-100 border-r border-zinc-800 flex flex-col">
        <div className="p-4 sticky top-0 bg-zinc-900/90 backdrop-blur border-b border-zinc-800">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-semibold tracking-wide uppercase text-zinc-300">
              {selectedId == undefined ? (
                <span>Nodes</span>
              ) : (
                <span>Properties</span>
              )}
            </h2>
          </div>
        </div>
        {selectedId == undefined ? (
          <>
            <div className="flex flex-col flex-1 overflow-y-auto">
              {palette.map((node, i) => (
                <button
                  key={`${node.name}-${i}`}
                  onClick={() => handleSpawnNode(node)}
                  className="p-2 text-left cursor-pointer hover:opacity-80 text-sm border-b border-zinc-800/40"
                >
                  {node.name}
                </button>
              ))}
            </div>

            <div className="p-3 border-t border-zinc-800 bg-zinc-900/90 flex flex-col gap-3">
              <button
                onClick={handleCompileNodes}
                className="w-full text-sm font-medium px-3 py-2 rounded-xl border border-zinc-700 hover:border-zinc-600 hover:bg-zinc-800/70 transition"
              >
                Compile Nodes
              </button>
              <button
                onClick={handleStart}
                className="w-full text-sm font-medium px-3 py-2 rounded-xl border border-zinc-700 hover:border-zinc-600 hover:bg-zinc-800/70 transition"
              >
                {isRunning ? "Stop" : "Start"}
              </button>
            </div>
          </>
        ) : (
          <>
            <div className="flex flex-col flex-1 overflow-y-auto">
              {selected!.properties.map((property, i) => {
                const type = property.type ?? "text";

                // helpers for type-safe value handling
                const getValue = () => {
                  if (type === "checkbox") return Boolean(property.value);
                  return property.value ?? "";
                };

                const handleChange = (
                  e: React.ChangeEvent<HTMLInputElement>
                ) => {
                  const newValue =
                    type === "number"
                      ? e.target.value === ""
                        ? ""
                        : Number(e.target.value)
                      : type === "checkbox"
                      ? e.target.checked
                      : e.target.value;

                  setSpawnedNodes((prev: any) =>
                    prev.map((n: INode) => {
                      if (n.id !== selected!.id) return n;

                      return {
                        ...n,
                        properties: n.properties.map((p, pi) =>
                          pi === i ? { ...p, value: newValue } : p
                        ),
                      };
                    })
                  );
                };

                return (
                  <div
                    key={`${property.name}-${i}`}
                    className="p-2 text-sm border-b border-zinc-800/40 flex flex-col gap-1"
                  >
                    <label className="text-sm text-zinc-400">
                      {property.name}
                    </label>
                    <input
                      type={type}
                      value={
                        type === "checkbox" ? undefined : (getValue() as any)
                      }
                      checked={
                        type === "checkbox"
                          ? (getValue() as boolean)
                          : undefined
                      }
                      onChange={handleChange}
                      className="w-full rounded-md border border-zinc-700 px-2 py-1 text-sm text-zinc-200 focus:outline-none focus:ring-1"
                    />
                  </div>
                );
              })}
            </div>

            <div className="p-3 border-t border-zinc-800 bg-zinc-900/90 flex flex-col gap-3">
              <button
                onClick={() => setSelectedId(undefined)}
                className="w-full text-sm font-medium px-3 py-2 rounded-xl border border-zinc-700 hover:border-zinc-600 hover:bg-zinc-800/70 transition"
              >
                Save and Back
              </button>
            </div>
          </>
        )}
      </div>
      <ReactFlowProvider>
        <Flow nodeClicked={handleNodeClick} ref={flowRef} setEdges={setEdges} />
      </ReactFlowProvider>
    </div>
  );
}
