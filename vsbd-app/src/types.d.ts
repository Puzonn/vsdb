export interface INode {
  name: string;
  id: number;
  inputs: NodeInput[];
  outputs: NodeOutput[];
}

export interface NodeActionMap {
  nodes: ActionNode[];
  connections: NodeConnection[];
}

export interface NodeConnection {
  from: number;
  fromType: string;
  to: number;
  toType: string;
}

export interface NodeInput {
  name: string;
  type: string;
}

export interface NodeOutput {
  name: string;
  type: string;
}

export interface ActionNode {
  id: number;
  name: string;
}
