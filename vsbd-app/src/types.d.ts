export interface NodeInput {
  readonly name: string;
  readonly type: TypeName;
}

export interface NodeOutput {
  readonly name: string;
  readonly type: TypeName;
}

export interface ActionNode {
  readonly id: number;
  readonly name: string;
}

export interface NodeConnection {
  readonly from: number;
  readonly fromType: PortKind;
  readonly to: number;
  readonly toType: PortKind;
}

export interface NodeActionMap {
  readonly nodes: readonly ActionNode[];
  readonly connections: readonly NodeConnection[];
}

export interface INode {
  readonly name: string;
  readonly id: number;
  readonly inputs: readonly NodeInput[];
  readonly outputs: readonly NodeOutput[];
  readonly properties: readonly NodeProperty[];
}

export interface NodeProperty {
  readonly name: string;
  readonly type: string;
  readonly defaultValue: string;
}