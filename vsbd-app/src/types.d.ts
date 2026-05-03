interface ProjectRunResult {
  success: boolean;
  errors?: string | null;
  nodes?: ProjectNode[];
}

interface Project {
  id: string;
  createdAt: Date;
  flowNodes: FlowNode[];
  flowEdges: FlowConnection[];
  nodes: ProjectNode[];
}

interface ProjectNode {
  name: string;
  sourceCode: string;
  outputs: NodeOutput[];
  inputs: NodeInput[];
  properties: NodeProperty[];
}

interface FlowNode {
  id: string;
  name: string;
  outputs: NodeOutput[];
  inputs: NodeInput[];
  properties: NodeProperty[];
  connections: FlowConnection[];
  position: { x: number; y: number };
  onDeleteClicked: () => void;
  onSettingsClicked: () => void;
}

interface FlowConnection {
  id: string;
  sourceId: string;
  targetId: string;
  sourceHandleId?: string | null;
  targetHandleId?: string | null;
}

interface NodeEditView {
  id?: string;
  name: string;
  isInstance: boolean;
  sourceCode: string;
  outputs: NodeOutput[];
  inputs: NodeInput[];
  properties: NodeProperty[];
  onPropertySave?: () => void;
}

interface NodeInput {
  id: string;
  type: string;
  name: string;
}

interface NodeOutput {
  id: string;
  type: string;
  name: string;
}

interface NodeProperty {
  type: string;
  name: string;
  value: string;
}

interface ProjectCreationResponse {
  projectId: string;
  nodes?: ProjectNode[] | null;
}

interface ProjectLoadResponse {
  id: string;
  createdAt: Date;
  nodes: ProjectNode[];
  flowNodes: FlowNode[];
  flowEdges: FlowConnection[];
}
