function formatAdditionalArgs(args: any[]): string {
  return args.length > 0 ? `, ${args.join(', ')}` : '';
}

export function logCall(instance: any, method: Function, ...args: any[]): void {
  const additionalArgs = formatAdditionalArgs(args);

  console.log(
    `[${new Date().toISOString()}] ${instance.constructor.name}.${method.name}() called${additionalArgs}`,
  );
}

export function logFunction(fn: Function, ...args: any[]): void {
  const additionalArgs = formatAdditionalArgs(args);

  console.log(
    `[${new Date().toISOString()}] Entered ${fn.name}${additionalArgs}`,
  );
}
