interface ProjectRunResult {
  success: boolean;
  errors?: string | null;
  nodes?: ProjectNode[] | null;
}

interface ProjectNode {
  name: string;
  sourceCode: string;
  outputs: NodeOutput[];
  inputs: NodeInput[];
  nodeProperties: NodeProperty[];
}

interface NodeInput {
  type: string;
  name: string;
}

interface NodeOutput {
  type: string;
  name: string;
}

interface NodeProperty {
  type: string;
  name: string;
  defaultValue?: string;
}

interface ProjectCreationResponse {
  projectId: string;
  nodes?: ProjectNode[] | null;
}
