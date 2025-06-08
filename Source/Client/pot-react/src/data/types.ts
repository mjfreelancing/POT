export type Paged<T> = {
  results: T[];
  totalCount: number;
  currentToken: string | null;
  previousToken: string | null;
  nextToken: string | null;
};
