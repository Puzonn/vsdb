export function projectNodeToEditView(node: ProjectNode): NodeEditView {
  return {
    id: undefined,
    name: node.name,
    isInstance: false,
    sourceCode: node.sourceCode,
    outputs: node.outputs,
    inputs: node.inputs,
    properties: node.properties,
    onPropertySave: undefined,
  };
}

export function flowNodeToEditView(node: FlowNode): NodeEditView {
  return {
    id: undefined,
    name: node.name,
    isInstance: true,
    sourceCode: "",
    outputs: node.outputs,
    inputs: node.inputs,
    properties: node.properties,
    onPropertySave: undefined,
  };
}
