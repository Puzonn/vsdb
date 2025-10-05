import { Handle, Position } from "@xyflow/react";
import { INode } from "../types";

function portY(i: number, n: number) {
  return ((i + 1) / (n + 1)) * 100;
}

function estimatePxForLabel(label: string) {
  const avgPxPerChar = 7;
  return Math.ceil(label.length * avgPxPerChar);
}

type Data = {
  node: INode;
  nodeClicked: (node: INode) => void;
};

export default function Node({ data }: {data: Data}) {
  const { node, nodeClicked } = data;
  
  const minNodeW = 220;
  const maxNodeW = 520;
  const sidePad = 24;
  const handleGap = 18;

  const longestLeft = node.inputs.reduce(
    (m, x) => Math.max(m, estimatePxForLabel(x.name)),
    0
  );
  const longestRight = node.outputs.reduce(
    (m, x) => Math.max(m, estimatePxForLabel(x.name)),
    0
  );

  const leftNeeded = Math.max(longestLeft, 0) + sidePad + handleGap;
  const rightNeeded = Math.max(longestRight, 0) + sidePad + handleGap;

  const neededW = Math.max(minNodeW, leftNeeded + rightNeeded);
  const nodeW = Math.min(neededW, maxNodeW);

  const rows = Math.max(node.inputs.length, node.outputs.length);
  const bodyH = Math.max(96, rows * 28);

  return (
    <div
      onClick={() => nodeClicked(node)}
      className="relative rounded-2xl bg-[#2a2a2a] text-white border border-[#3a3a3a] shadow-sm"
      style={{ width: nodeW }}
    >
      <div className="flex flex-col items-center justify-center px-3 py-2 border-b border-[#3a3a3a] bg-[#1f1f1f] rounded-t-2xl">
        <span className="text-sm font-semibold tracking-wide">
          {node.name ?? "Untitled"}
        </span>
        <span className="text-[11px] text-gray-400">ID: {node.id}</span>
      </div>

      <div className="relative" style={{ height: bodyH }}>
        <div className="absolute inset-y-3 left-1/2 w-px bg-white/10 pointer-events-none" />

        {node.inputs.map((inp, i) => {
          const top = `${portY(i, node.inputs.length)}%`;
          return (
            <div key={`in-${inp.name}-${i}`}>
              <Handle
                type="target"
                position={Position.Left}
                id={inp.name}
                style={{
                  top,
                  left: -6,
                  width: 12,
                  height: 12,
                  borderRadius: "50%",
                  background: "#10b981",
                }}
              />
              <div
                className="absolute -translate-y-1/2 left-3 text-[10px] text-gray-300 whitespace-nowrap overflow-hidden text-ellipsis pointer-events-none"
                style={{
                  top,
                  width: `calc(${nodeW / 2}px - ${handleGap + 9}px)`,
                }}
                title={inp.name}
              >
                {inp.name}
              </div>
            </div>
          );
        })}

        {node.outputs.map((out, i) => {
          const top = `${portY(i, node.outputs.length)}%`;
          return (
            <div key={`out-${out.name}-${i}`}>
              <Handle
                type="source"
                position={Position.Right}
                id={out.name}
                style={{
                  top,
                  right: -6,
                  width: 12,
                  height: 12,
                  borderRadius: "50%",
                  background: "#3b82f6",
                }}
              />
              <div
                className="absolute -translate-y-1/2 right-3 text-[8px] text-xs text-gray-300 text-right whitespace-nowrap overflow-hidden text-ellipsis pointer-events-none"
                style={{
                  top,
                  width: `calc(${nodeW / 2}px - ${handleGap + 9}px)`,
                }}
                title={out.name}
              >
                {out.name}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
