export type ApiHeaders = Record<string, string | string[]>;

export type ApiResponse = {
  statusCode: number;
  content?: string;
  headers?: ApiHeaders;
};
