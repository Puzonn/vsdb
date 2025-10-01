import { Handle, Position } from "@xyflow/react";


type Props = { data: { label?: string; outputCount?: number; inputCount?: number } };

export default function Node({ data }: Props) {
  const outputs = Math.max(0, data.outputCount ?? 0);
  const inputs  = Math.max(0, data.inputCount ?? 0);

  const pct = (i: number, n: number) => (n <= 1 ? 50 : ((i + 1) / (n + 1)) * 100);

  return (
    <div
      style={{
        padding: 10,
        border: "1px solid #333",
        borderRadius: 8,
        background: "#343434",
        color: "#fff",
        minWidth: 160,
        position: "relative",
      }}
    >
      <div style={{ fontWeight: 600, marginBottom: 6 }}>
        {data.label ?? "Custom Node"}
      </div>

      {Array.from({ length: inputs }, (_, i) => (
        <Handle
          key={`in-${i}`}
          type="target"
          position={Position.Left}
          id={`in-${i}`}
          style={{ top: `${pct(i, inputs)}%` }}
        >
        </Handle>
      ))}

      {Array.from({ length: outputs }, (_, i) => (
        <Handle
          key={`out-${i}`}
          type="source"
          position={Position.Right}
          id={`out-${i}`}
          style={{ top: `${pct(i, outputs)}%` }}
        />
      ))}
    </div>
  );
}
