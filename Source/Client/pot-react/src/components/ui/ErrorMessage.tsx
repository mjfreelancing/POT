type ErrorMessageProps = {
  message: string;
};

function ErrorMessage({ message }: ErrorMessageProps) {
  return (
    <div className="flex flex-col items-center justify-center text-red-500 mb-8">
      {message}
    </div>
  );
}

export default ErrorMessage;
