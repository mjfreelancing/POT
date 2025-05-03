// Useful for setting state to be provided to an ErrorDialog, such as:
// const [dialogEror, setDialogError] = useState<ErrorDialogState | null>(null);
export type ErrorDialogState = {
  title: string;
  description: string;
};
